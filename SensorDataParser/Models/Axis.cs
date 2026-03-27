using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Axis : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int AxisID { get; set; }
        public int? PointID { get; set; }

        [ForeignKey(nameof(Point), nameof(Point.Id))]
        public int? SPointID { get; set; }
        public int? AxisTypeID { get; set; }

        [ForeignKey(nameof(AxisType), nameof(AxisType.Id))]
        public int? SAxisTypeID { get; set; }
        public string? UniqueGuid { get; set; }
        public int? OrdSeq { get; set; }
        public int? TriaxAxisID { get; set; }
    }
}
