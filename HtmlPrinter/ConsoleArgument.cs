using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable LocalizableElement

namespace HtmlPrinter
{
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
        public string Help { get; set; }

        /// <summary>
        /// 用于验证数据的正则表达式，为空则不验证
        /// </summary>
        public string ValidateExpress { get; set; }
    }

    /// <summary>
    /// 表示该对象可以从控制台参数进行初始化
    /// </summary>
    public abstract class ConsoleArgument
    {
        private readonly Dictionary<string, ConsoleArgumentMetaInfo> _argsDictionary;

        protected ConsoleArgument()
        {
            _argsDictionary = new Dictionary<string, ConsoleArgumentMetaInfo>();
            var props = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance |
                               BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.GetCustomAttribute<ConsoleArgumentAttribute>() is { } consoleArgument)
                {
                    _argsDictionary[consoleArgument.ArgumentName.ToLower()] =
                        new ConsoleArgumentMetaInfo(consoleArgument, propertyInfo);
                }
            }
        }

        protected virtual string[] Usage()
        {
            return Array.Empty<string>();
        }

        /// <summary>
        /// 读取控制台参数，并且构造该对象
        /// </summary>
        /// <returns>如果有参数不满足构造验证规则，则返回false</returns>
        public bool Build(Action<string> log)
        {
            var isValid = true;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                var key = args[i].ToLower();
                var value = GetNextItem(args, i);
                if (!_argsDictionary.ContainsKey(key)) continue;

                var propInfo = _argsDictionary[key];
                if (!string.IsNullOrWhiteSpace(propInfo.Meta.ValidateExpress))
                {
                    var regex = new Regex(propInfo.Meta.ValidateExpress);
                    if (!regex.IsMatch(value))
                    {
                        isValid = false;
                        log($"参数{propInfo.Meta.ArgumentName}的值{value}不合法！");
                        continue;
                    }
                }

                if (propInfo.Property.PropertyType == typeof(string))
                {
                    propInfo.Property.SetValue(this, value);
                }
                else if (propInfo.Property.PropertyType == typeof(bool))
                {
                    propInfo.Property.SetValue(this, value?.ToLower() == "y");
                }
                else if (propInfo.Property.PropertyType == typeof(int))
                {
                    propInfo.Property.SetValue(this, int.TryParse(value, out var iVal) ? iVal : 0);
                }
                else if (propInfo.Property.PropertyType == typeof(double))
                {
                    propInfo.Property.SetValue(this, double.TryParse(value, out var iVal) ? iVal : 0);
                }
                else
                {
                    log($"不支持的参数类型：{propInfo.Property.PropertyType.Name}");
                }
            }

            if (args.Contains("-?") || args.Contains("-help"))
            {
                OutputHelp();
            }

            return isValid;
        }

        /// <summary>
        /// 输出帮助信息
        /// </summary>
        public void OutputHelp()
        {
            var msg = new StringBuilder();
            var usage = Usage();
            if (usage.Length != 0)
            {
                for (var i = 0; i < usage.Length; i++)
                {
                    msg.AppendLine($"{usage[i]}");
                }

                msg.AppendLine();
            }

            msg.AppendLine("本程序支持如下参数：");
            foreach (var info in _argsDictionary)
            {
                msg.AppendLine(
                    // ReSharper disable once LocalizableElement
                    $"  {info.Value.Meta.ArgumentName}:{(info.Value.Property == typeof(bool) ? " y/n" : "")} {info.Value.Meta.Help ?? "<没有帮助信息>"}");
            }

            MessageBox.Show(msg.ToString(), "帮助");
            Environment.Exit(0);
        }

        string GetNextItem(string[] array, int index)
        {
            return index + 1 < array.Length ? array[index + 1] : null;
        }
    }

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
}
