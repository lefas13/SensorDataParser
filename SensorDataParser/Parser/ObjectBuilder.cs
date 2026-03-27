using System.Reflection;

namespace SensorDataParser.Parser
{
    internal class ObjectBuilder
    {
        private readonly TypesConverter _converter;
        private readonly Dictionary<string, object> _properties;

        internal ObjectBuilder(TypesConverter converter)
        {
            _converter = converter;
            _properties = new Dictionary<string, object>();
        }

        internal ObjectBuilder SetPropertyValue(string propertyName, string valueType, string value)
        {
            _properties[propertyName] = _converter.Convert(valueType, value);
            return this;
        }

        internal T Build<T>() where T : class
        {
            var entity = Activator.CreateInstance<T>();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (_properties.TryGetValue(property.Name, out var value) && property.CanWrite)
                {
                    property.SetValue(entity, value);
                }
            }
            return entity;
        }
    }
}