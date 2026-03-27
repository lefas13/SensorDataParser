using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Schedule : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int ScheduleID { get; set; }
        public int? AxisID { get; set; }

        [ForeignKey(nameof(Axis), nameof(Axis.Id))]
        public int? SAxisID { get; set; }
        public string? UniqueGuid { get; set; }
        public int? MultiSensorID { get; set; }

        [ForeignKey(nameof(MultiSensor), nameof(MultiSensor.Id))]
        public int? SMultiSensorID { get; set; }
        public int? ParamSetID { get; set; }

        [ForeignKey(nameof(ParamSet), nameof(ParamSet.Id))]
        public int? SParamSetID { get; set; }
        public int? Status { get; set; }
        public int? OrdSeq { get; set; }
        public double? RMS { get; set; }
    }
}
