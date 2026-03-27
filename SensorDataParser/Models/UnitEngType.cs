using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class UnitEngType : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int UETID { get; set; }
        public int? URTID { get; set; }

        [ForeignKey(nameof(UnitRefType), nameof(UnitRefType.Id))]
        public int? SURTID { get; set; }
        public string? Descriprion { get; set; }
        public string? AscentName { get; set; }
        public double? MultiplierToRef { get; set; }
        public double? OffsetToRef { get; set; }
        public bool? SensorUse { get; set; }
        public bool? MeasureUse { get; set; }
        public string? PreferredMagnitude { get; set; }
    }
}
