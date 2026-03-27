using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Point : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int PointID { get; set; }
        public int? MachineID { get; set; }

        [ForeignKey(nameof(Machine), nameof(Machine.Id))]
        public int? SMachineID { get; set; }
        public string? Name { get; set; }
        public string? UniqueGuid { get; set; }
        public int? OrdSeq { get; set; }
        public double? RPMMultiplier { get; set; }
        public double? RollerDiameter { get; set; }

    }
}
