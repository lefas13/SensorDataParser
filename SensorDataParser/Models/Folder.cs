using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Folder : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int FolderID { get; set; }
        public string? Name { get; set; }
        public string? UniqueGuid { get; set; }
        public int? UserID { get; set; }
        public int? LastEntityType { get; set; }
        public int? EntityID { get; set; }
    }
}
