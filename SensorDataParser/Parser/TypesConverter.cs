namespace SensorDataParser.Parser
{
    internal class TypesConverter
    {
        private readonly Dictionary<string, Func<string, object>> _converters;

        internal TypesConverter()
        {
            _converters = new Dictionary<string, Func<string, object>>();
        }

        internal TypesConverter AddConvertibleType(string type, Func<string, object> converter)
        {
            _converters[type] = converter;
            return this;
        }

        internal object Convert(string type, string value)
        {
            if (!_converters.TryGetValue(type, out var converter))
            {
                throw new ArgumentException($"Преобразование невозможно, так как передан неизвестный тип: {type}");
            }
            return converter(value);
        }
    }
}