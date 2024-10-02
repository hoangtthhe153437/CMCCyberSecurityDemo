using static CMCCyberSecurity.Helpers.EnumHelper;

namespace CMCCyberSecurity.Helpers
{
    public class HttpStatusException : Exception
    {
        public int StatusCode { get; private set; }
        public ECode Code { get; private set; }

        public HttpStatusException(string msg, ECode code, int statusCode = 501) : base(msg)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }
}
