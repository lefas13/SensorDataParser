using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class ParamType : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int? ParamTypeID { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public int? OrdSeq { get; set; }
        public bool? RouteAdd { get; set; }
    }
}
