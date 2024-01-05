using System.Diagnostics;
using Microsoft.Web.WebView2.Core;

namespace HtmlPrinter
{
    public partial class Form1 : Form
    {
        private readonly InputArgument _inputArgument;
        private readonly string _webView2TmpDir;

        void TraceLog(string message)
        {
            Trace.WriteLine($"[HtmlPrinter] {message}");
        }

        public Form1()
        {
            _webView2TmpDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HtmlPrinter");
            InitializeComponent();
            _inputArgument = new InputArgument();
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            Left = 0;
            Top = 0;
            Width = 1;
            Height = 1;
            Opacity = 0;
            Load += Form1_Load;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("/clean"))
            {
                if (Directory.Exists(_webView2TmpDir))
                {
                    try
                    {
                        Directory.Delete(_webView2TmpDir);
                    }
                    catch (Exception ex)
                    {
                        TraceLog($"清理{_webView2TmpDir}出错,{ex.Message}");
                    }
                }

                Environment.Exit(0);
                return;
            }

            if (!_inputArgument.Build(TraceLog))
            {
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrWhiteSpace(_inputArgument.Url))
            {
                TraceLog("Url不能为空");
                Environment.Exit(2);
                return;
            }

            try
            {
                var env = await CoreWebView2Environment.CreateAsync(null, _webView2TmpDir);
                webView21.CoreWebView2InitializationCompleted += WebView21_CoreWebView2InitializationCompleted;
                await webView21.EnsureCoreWebView2Async(env);

                TraceLog("正在下载页面...");
                webView21.Source = new Uri(_inputArgument.Url);
            }
            catch (Exception ex)
            {
                TraceLog($"下载页面出错 {ex.Message}");
                Environment.Exit(3);
            }
        }

        private void WebView21_CoreWebView2InitializationCompleted(object? sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            TraceLog("页面下载完成，准备开始打印...");
            try
            {
                var ps = webView21.CoreWebView2.Environment.CreatePrintSettings();

                if (_inputArgument.MarginTop >= 0) ps.MarginTop = _inputArgument.MarginTop;
                if (_inputArgument.MarginBottom >= 0) ps.MarginBottom = _inputArgument.MarginBottom;
                if (_inputArgument.MarginLeft >= 0) ps.MarginLeft = _inputArgument.MarginLeft;
                if (_inputArgument.MarginRight >= 0) ps.MarginRight = _inputArgument.MarginRight;
                if (_inputArgument.ShouldPrintHeaderAndFooter)
                {
                    ps.HeaderTitle = _inputArgument.Header;
                    ps.FooterUri = _inputArgument.Footer;
                }


                if (_inputArgument.Copies is > 1 and < 1000)
                {
                    ps.Copies = _inputArgument.Copies;
                }

                ps.ShouldPrintHeaderAndFooter = _inputArgument.ShouldPrintHeaderAndFooter;
                switch (_inputArgument.ColorMode.ToLower())
                {
                    case "default":
                        ps.ColorMode = CoreWebView2PrintColorMode.Default;
                        break;
                    case "color":
                        ps.ColorMode = CoreWebView2PrintColorMode.Color;
                        break;
                    case "gray":
                        ps.ColorMode = CoreWebView2PrintColorMode.Grayscale;
                        break;
                }

                switch (_inputArgument.Orientation.ToLower())
                {
                    case "portrait":
                        ps.Orientation = CoreWebView2PrintOrientation.Portrait;
                        break;
                    case "landscape":
                        ps.Orientation = CoreWebView2PrintOrientation.Landscape;
                        break;
                }

                ps.PagesPerSide = _inputArgument.PagesPerSide;

                if (!string.IsNullOrWhiteSpace(_inputArgument.PrinterName))
                {
                    ps.PrinterName = _inputArgument.PrinterName;
                }

                ps.ShouldPrintBackgrounds = _inputArgument.IncludeBackground;

                if (_inputArgument.Scale is >= 0.1 and <= 2)
                {
                    ps.ScaleFactor = _inputArgument.Scale;
                }

                if (!string.IsNullOrWhiteSpace(_inputArgument.Pdf))
                {
                    TraceLog($"正在打印PDF {_inputArgument.Pdf}");
                    var pdf = Path.GetFullPath(_inputArgument.Pdf);
                    await webView21.CoreWebView2.PrintToPdfAsync(pdf, ps);
                }
                else
                {
                    TraceLog($"正在打印...");
                    await webView21.CoreWebView2.PrintAsync(ps);
                }

                TraceLog("打印完成");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                TraceLog($"打印出错 {ex.Message}");
                Environment.Exit(3);
            }
        }
    }
}