using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class BiasVoltage : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int BiasVoltageID { get; set; }
        public string? Name { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
    }
}
