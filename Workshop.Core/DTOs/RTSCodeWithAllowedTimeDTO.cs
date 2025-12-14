using Workshop.Core.DTOs;

namespace Workshop.Core.DTOs
{
    public class RTSCodeWithAllowedTimeDTO
    {
        public RTSCodeDTO RTSCode { get; set; }
        public AllowedTimeDTO AllowedTime { get; set; }
        public IEnumerable <RTSCodeDTO> RTSCodes { get; set; }
        public List<AllowedTimeListItemDTO> AllowedTimes { get; set; }
        public List<AllowedTimeDTO> RTSAllowedTime { get; set; }

    }

}