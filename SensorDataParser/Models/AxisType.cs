using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class AxisType : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int AxisTypeID { get; set; }
        public string? Name { get; set; }
    }
}
