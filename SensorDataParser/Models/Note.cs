using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Note : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int NoteID { get; set; }
        public string? Text { get; set; }
        public int? OrdSeq { get; set; }
        public bool? RouteAdd { get; set; }
    }
}
