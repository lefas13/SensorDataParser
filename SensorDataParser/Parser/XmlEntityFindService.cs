using System.Globalization;
using System.Xml;

namespace SensorDataParser.Parser
{
    public class XmlEntityFindService
    {
        private readonly TypesConverter _converter;

        public XmlEntityFindService()
        {
            _converter = new TypesConverter()
                .AddConvertibleType("integer", v => Convert.ToInt32(v))
                .AddConvertibleType("double", v => Convert.ToDouble(v, CultureInfo.InvariantCulture))
                .AddConvertibleType("string", v => v.ToString())
                .AddConvertibleType("bool", v => Convert.ToBoolean(v))
                .AddConvertibleType("binary", v => v.ToString());
        }

        public List<T> FindAll<T>(string input) where T : class
        {
            var targetElement = FindElement(FindRoot(input), typeof(T).Name);

            if (targetElement == null)
            {
                return new List<T>();
            }

            var metaDataElement = targetElement["MetaData"] ?? throw new InvalidOperationException("Узел MetaData не найден");
            var entities = new List<T>();

            foreach (XmlElement element in targetElement.ChildNodes)
            {
                if (element.Name == "Row")
                {
                    var entity = ToObject<T>(metaDataElement, element);

                    if (entity != null)
                    {
                        entities.Add(entity);
                    }
                }
            }

            return entities;
        }

        private static XmlElement FindRoot(string input)
        {
            var document = new XmlDocument();
            document.Load(input);

            return document.DocumentElement ?? throw new ArgumentException("Документ не имеет корневого элемента");
        }

        private XmlElement? FindElement(XmlElement root, string name)
        {
            foreach (XmlElement child in root.ChildNodes)
            {
                if (child.Name == "Table" && child.GetAttribute("name") == name)
                {
                    return child;
                }

                if (child.Name == "Global" || child.Name == "Folder")
                {
                    XmlElement? result;
                    if ((result = FindElement(child, name)) != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private static string? GetValueAsString(XmlElement rowElement, string position)
        {
            foreach (XmlElement element in rowElement.ChildNodes)
            {
                if (element.Name == $"C{position}")
                {
                    return element.InnerText;
                }
            }
            return null;
        }

        private T ToObject<T>(XmlElement metaDataElement, XmlElement rowElement) where T : class
        {
            var builder = new ObjectBuilder(_converter);

            foreach (XmlElement element in metaDataElement.ChildNodes)
            {
                var value = GetValueAsString(rowElement, element.GetAttribute("pos"));

                if (value == null)
                {
                    continue;
                }

                builder = builder.SetPropertyValue(element.GetAttribute("name"), element.GetAttribute("type"), value);
            }

            return builder.Build<T>();
        }
    }
}