namespace SensorDataParser.Parser.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
        public bool Ignore { get; set; } = true;
    }
}