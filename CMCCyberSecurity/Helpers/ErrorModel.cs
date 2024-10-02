using static CMCCyberSecurity.Helpers.EnumHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMCCyberSecurity.Helpers
{
    public class ErrorModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ECode ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
