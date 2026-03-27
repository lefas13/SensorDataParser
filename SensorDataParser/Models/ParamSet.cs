using SensorDataParser.Parser.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SensorDataParser.Models
{
    public class ParamSet : IEntity
    {
        [Key]
        public int Id { get; set; }
        public int? ParamSetID { get; set; }
        public int? ParamTypeID { get; set; }

        [ForeignKey(nameof(ParamType), nameof(ParamType.Id))]
        public int? SParamTypeID { get; set; }
        public int? StreamType { get; set; }
        public int? DisplayMode { get; set; }
        public int? MaxDurationTime { get; set; }
        public int? Delay { get; set; }
        public int? NumberOfSamples { get; set; }
        public int? FMax { get; set; }
        public int? TachoTrigger { get; set; }
        public int? KeyphasorSelect { get; set; }
        public int? WindowType { get; set; }
        public int? MeasurementURTID { get; set; }

        [ForeignKey(nameof(UnitRefType), nameof(UnitRefType.Id))]
        public int? SMeasurementURTID { get; set; }
        public int? AverageSamples { get; set; }
        public int? WeightingFactor { get; set; }
        public int? AverageType { get; set; }
        public int? DMin { get; set; }
        public int? DMax { get; set; }
        public int? RecordRPM { get; set; }
        public int? KeypadUETID { get; set; }

        [ForeignKey(nameof(UnitEngType), nameof(UnitEngType.Id))]
        public int? SKeypadUETID { get; set; }
        public int? NumberOfChannels { get; set; }
        public bool? DftPS { get; set; }
        public bool? OrderTracked { get; set; }
        public bool? PSLock { get; set; }
        public string? Description { get; set; }
        public string? KeypadPrompt { get; set; }
        public string? DefaultValue { get; set; }
        public double? AverageOverlapPct { get; set; }
        public double? FMin { get; set; }
        public double? DaNumberOfOrdersta { get; set; }
        public double? NumberOfRevs { get; set; }
        public double? RPMEntered { get; set; }
        public double? ForceHammerTriggerPercent { get; set; }
    }
}
