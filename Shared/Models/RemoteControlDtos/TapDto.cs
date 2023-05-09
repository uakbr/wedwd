using iControl.Shared.Enums;
using System.Runtime.Serialization;

namespace iControl.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class TapDto : BaseDto
    {

        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.Tap;

        [DataMember(Name = "PercentX")]
        public double PercentX { get; set; }

        [DataMember(Name = "PercentY")]
        public double PercentY { get; set; }
    }
}
