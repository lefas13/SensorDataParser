using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Sensor : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int SensorID { get; set; }
        public string? Name { get; set; }
        public double? Sensitivity { get; set; }
        public double? DcOffset { get; set; }
        public int? SensitivityUnitEngType { get; set; }
        public bool? DriveCurrent { get; set; }
        public int? SettlingTime { get; set; }
        public int? BiasVoltageID { get; set; }

        [ForeignKey(nameof(BiasVoltage), nameof(BiasVoltage.Id))]
        public int? SBiasVoltageID { get; set; }
        public int? InputMode { get; set; }
        public bool? AutoSettling { get; set; }
        public double? AmpereMin { get; set; }
        public double? AmpereMax { get; set; }
    }
}
