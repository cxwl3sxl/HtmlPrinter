namespace HtmlPrinter
{
    internal class InputArgument : ConsoleArgument
    {
        protected override string[] Usage()
        {
            return new[]
            {
                "返回值注释清单：",
                "  0: 程序正常完成",
                "  1: 部分参数非法，具体参见Trace日志",
                "  2: 参数-url不能为空",
                "  3: 程序出错，具体参见Trace日志",
                "支持的特殊命令：",
                "  /clean: 清理临时目录"
            };
        }

        [ConsoleArgument("-mt", Help = "顶部边距，单位英寸")]
        public double MarginTop { get; set; } = 0.4;

        [ConsoleArgument("-mb", Help = "底部边距，单位英寸")]
        public double MarginBottom { get; set; } = 0.4;

        [ConsoleArgument("-ml", Help = "左边边距，单位英寸")]
        public double MarginLeft { get; set; } = 0.4;

        [ConsoleArgument("-mr", Help = "右边边距，单位英寸")]
        public double MarginRight { get; set; } = 0.4;

        [ConsoleArgument("-hf", Help = "是否打印页眉和页脚，默认false")]
        public bool ShouldPrintHeaderAndFooter { get; set; }

        [ConsoleArgument("-cm", Help = "颜色模式，可选 default:默认, color:彩色, gray:黑白")]
        public string ColorMode { get; set; } = "default";

        [ConsoleArgument("-bg", Help = "是否打印背景，默认false")]
        public bool IncludeBackground { get; set; }

        [ConsoleArgument("-header", Help = "页眉内容，需要设置-hf为true")]
        public string Header { get; set; }

        [ConsoleArgument("-footer", Help = "页脚内容，需要设置-hf为true")]
        public string Footer { get; set; }

        [ConsoleArgument("-url", Help = "需要答应的页面")]
        public string Url { get; set; }

        [ConsoleArgument("-pdf", Help = "打印成PDF文件的路径")]
        public string Pdf { get; set; }

        [ConsoleArgument("-count", Help = "打印份数，默认1，最大999，超出范围将取默认值", ValidateExpress = @"^\d+$")]
        public int Copies { get; set; } = 1;

        [ConsoleArgument("-orientation", Help = "打印方向，portrait：纵向打印(默认)，landscape：横向打印", ValidateExpress = @"^\d+$")]
        public string Orientation { get; set; } = "portrait";

        [ConsoleArgument("-pps", Help = "每页打印的数量，1、2、4、6、9、16", ValidateExpress = @"^1$|^2$|^4$|^6$|^9$|^16$")]
        public int PagesPerSide { get; set; } = 1;

        [ConsoleArgument("-pn", Help = "打印机名称")]
        public string PrinterName { get; set; }

        [ConsoleArgument("-scale", Help = "缩放，0.1~2.0，默认1")]
        public double Scale { get; set; }
    }
}
