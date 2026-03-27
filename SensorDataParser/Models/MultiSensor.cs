using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class MultiSensor : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int MultiSensorID { get; set; }
        public int? SensorID1 { get; set; }

        [ForeignKey(nameof(Sensor), nameof(Sensor.Id))]
        public int? SSensorID1 { get; set; }
        public int? SensorID2 { get; set; }

        [ForeignKey(nameof(Sensor), nameof(Sensor.Id))]
        public int? SSensorID2 { get; set; }
        public int? SensorID3 { get; set; }

        [ForeignKey(nameof(Sensor), nameof(Sensor.Id))]
        public int? SSensorID3 { get; set; }
        public int? SensorID4 { get; set; }

        [ForeignKey(nameof(Sensor), nameof(Sensor.Id))]
        public int? SSensorID4 { get; set; }

    }
}
