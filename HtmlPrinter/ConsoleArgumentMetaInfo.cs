using System.Reflection;

namespace HtmlPrinter;

class ConsoleArgumentMetaInfo
{
    public ConsoleArgumentMetaInfo(ConsoleArgumentAttribute attribute, PropertyInfo property)
    {
        Meta = attribute;
        Property = property;
    }

    public ConsoleArgumentAttribute Meta { get; }

    public PropertyInfo Property { get; }
}