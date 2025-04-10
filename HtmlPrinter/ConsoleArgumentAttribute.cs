namespace HtmlPrinter;

/// <summary>
/// 表示该属性对应的控制台参数信息
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConsoleArgumentAttribute : Attribute
{
    /// <summary>
    /// 在控制台中对应的参数名称
    /// </summary>
    public string ArgumentName { get; }

    /// <summary>
    /// 表示该属性对应的控制台参数信息
    /// </summary>
    /// <param name="argumentName">在控制台中对应的参数名称</param>
    public ConsoleArgumentAttribute(string argumentName)
    {
        ArgumentName = argumentName;
    }

    /// <summary>
    /// 该参数对应的帮助信息
    /// </summary>
    public string? Help { get; set; }

    /// <summary>
    /// 用于验证数据的正则表达式，为空则不验证
    /// </summary>
    public string? ValidateExpress { get; set; }
}