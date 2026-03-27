using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class UnitRefType : IEntity

    {
        [Key]
        public int Id { get; set; }
        public int URTID { get; set; }
        public int? DefaultUnitEngType { get; set; }
        public int? DefaultUSUnitEngType { get; set; }
        public int? BaseLineMode { get; set; }
        public int? LogRange { get; set; }
        public int? RefType { get; set; }
        public int? OrdSeq { get; set; }
        public int? LogType { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? OldName { get; set; }
        public string? AscentName { get; set; }
        public string? FullName { get; set; }
        public double? LogMax { get; set; }
        public bool? SensorUse { get; set; }
        public bool? UseOrders { get; set; }
    }
}
