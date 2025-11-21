using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Runtime.InteropServices;

namespace HtmlPrinter
{
    internal class Program
    {
        private static readonly AutoResetEvent EventLoop = new(false);

        #region main

        static void Main()
        {
            var ia = new InputArgument();
            var correct = ia.Build(Console.WriteLine);
            if (!correct)
            {
                ia.OutputHelp();
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrWhiteSpace(ia.ChromePath) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //如果没有传递chrome程序位置且是windows平台，那么自动寻找
                ia.ChromePath = ChromeEdgeHelper.FindChromeOrEdge();
            }

            Process(ia);
            EventLoop.WaitOne();
        }

        #endregion

        static async void Process(InputArgument ia)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ia.Url))
                {
                    Console.WriteLine("-url 不能为空");
                    Environment.Exit(2);
                    return;
                }

                if (!File.Exists(ia.ChromePath ?? string.Empty))
                {
                    Console.WriteLine("-chrome-path 不能为空或指定的文件不存在");
                    Environment.Exit(1);
                    return;
                }

                var pdf = await CreatePdf(ia);
                if (!File.Exists(pdf))
                {
                    Console.WriteLine("无法生成文件或生成的文件不存在");
                    Environment.Exit(3);
                    return;
                }

                if (pdf == ia.Pdf)
                {
                    Console.WriteLine($"文件已经保存到{pdf}");
                    return;
                }

                await PrintPdf(ia, pdf);

                await Task.Delay(1000);

                File.Delete(pdf);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行出错{Environment.NewLine}{ex}");
                Environment.Exit(3);
            }
            finally
            {
                EventLoop.Set();
            }
        }

        static async Task<string> CreatePdf(InputArgument ia)
        {
            var options = new LaunchOptions
            {
                Headless = true, // 无界面模式
                ExecutablePath = ia.ChromePath,
                Args =
                [
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage" // 防止共享内存不足问题
                ]
                //"/opt/apps/cn.google.chrome-pre/files/google/chrome/google-chrome" // Linux/macOS 需指定Chromium路径
                //ExecutablePath =
                //    "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe" // Linux/macOS 需指定Chromium路径
            };

            // 启动浏览器实例
            await using var browser = await Puppeteer.LaunchAsync(options);
            await using var page = await browser.NewPageAsync();

            // 加载HTML内容（直接生成含链接的HTML或其他来源）
            await page.GoToAsync(ia.Url, new NavigationOptions()
            {
                ReferrerPolicy = "no-referrer"
            }); // 示例：传入网页链接
            // 或直接设置HTML内容： await page.SetContentAsync("<a href='https://example.com'>Example Link</a>");

            var pdf = ia.Pdf;
            if (string.IsNullOrWhiteSpace(pdf))
            {
                pdf = $"{Path.GetTempFileName()}.pdf";
            }

            // 调用打印机（需用户授权，或配置静默打印参数）
            await page.PdfAsync(pdf, new PdfOptions()
            {
                DisplayHeaderFooter = ia.ShouldPrintHeaderAndFooter,
                HeaderTemplate = ia.Header ?? "",
                FooterTemplate = ia.Footer ?? "",
                PrintBackground = ia.IncludeBackground,
                MarginOptions = new MarginOptions()
                {
                    Top = ia.MarginTop,
                    Left = ia.MarginLeft,
                    Bottom = ia.MarginBottom,
                    Right = ia.MarginRight,
                },
                Landscape = ia.Orientation == "landscape",
                Scale = (decimal)ia.Scale
            }); // 可选：生成PDF再打印

            return pdf;
        }

        static async Task PrintPdf(InputArgument ia, string doc)
        {
            var count = ia.Copies;
            while (count > 0)
            {
                Console.WriteLine($"正在打印第{ia.Copies - count + 1}份...");
                // 示例：通过命令行打印（Linux/macOS）
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    await PrintLinux(ia, doc);
                }
                else
                {
                    Console.WriteLine("不支持的操作系统");
                }

                count--;
            }
        }

        static async Task PrintLinux(InputArgument ia, string pdf)
        {
            var args = string.IsNullOrWhiteSpace(ia.PrinterName)
                ? pdf
                : $"-d {ia.PrinterName} {pdf}";
            var p = System.Diagnostics.Process.Start("lp", args);
            await p.WaitForExitAsync();
        }
    }
}