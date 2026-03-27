namespace SensorDataParser.Parser.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string referenceTableName, string primaryKeyName)
        {
            ReferenceTableName = referenceTableName;
            PrimaryKeyName = primaryKeyName;
        }

        public string ReferenceTableName { get; set; } = null!;
        public string PrimaryKeyName { get; set; } = null!;
    }
}