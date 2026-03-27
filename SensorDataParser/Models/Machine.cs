using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Machine : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int MachineID { get; set; }
        public int? FolderID { get; set; }

        [ForeignKey(nameof(Folder), nameof(Folder.Id))]
        public int? SFolderID { get; set; }
        public string? Name { get; set; }
        public string? UniqueGuid { get; set; }
        public int? OrdSeq { get; set; }
        public bool? SpeedIsLinear { get; set; }
        public bool? AskSpeed { get; set; }
        public double? Speed { get; set; }
    }
}
