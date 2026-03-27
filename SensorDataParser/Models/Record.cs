using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class Record : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int RecordID { get; set; }
        public int? ScheduleID { get; set; }

        [ForeignKey(nameof(Schedule), nameof(Schedule.Id))]
        public int? SScheduleID { get; set; }
        public string? Date { get; set; }
        public bool? Complex { get; set; }
        public int? FMax { get; set; }
        public int? RepeatedRecordID { get; set; }
        public int? LinkRecordID { get; set; }
        public int? MultiLinkID { get; set; }
        public double? RPM { get; set; }
        public double? Mean { get; set; }
        public double? DCLevel { get; set; }
        public int? UTCDelta { get; set; }
        public double? RMS { get; set; }
        public int? ParamTypeID { get; set; }

        [ForeignKey(nameof(ParamType), nameof(ParamType.Id))]
        public int? SParamTypeID { get; set; }
        public int? NativeUETID { get; set; }

        [ForeignKey(nameof(UnitEngType), nameof(UnitEngType.Id))]
        public int? SNativeUETID { get; set; }
        public int? DataInfo { get; set; }
        public int? DataSize { get; set; }
        public int? ChannelID { get; set; }
        public string? Data { get; set; }
        public string? ImaginaryData { get; set; }
        public string? TachData { get; set; }
        public string? Text { get; set; }
        public double? ProcessData { get; set; }
        public bool? BaseLine { get; set; }
        public bool? AscentUploaded { get; set; }
        public int? ResponseDirection { get; set; }
        public int? ReferencePointID { get; set; }

        [ForeignKey(nameof(Point), nameof(Point.Id))]
        public int? SReferencePointID { get; set; }
        public int? ReferenceDirection { get; set; }
    }
}
