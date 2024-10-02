using System.Security.Cryptography;
using System.Text;

namespace CMCCyberSecurity.Helpers
{
    public class HMACUtil
    {
        public static string HmacGenerator(string input, string key, string type = "SHA1")
        {
            try
            {
                byte[] bKey = System.Text.Encoding.UTF8.GetBytes(key);
                string retVal = "";
                switch (type.ToUpper().Trim())
                {
                    case "SHA256":
                        HMACSHA256 myhmacsha256 = new HMACSHA256(bKey);
                        byte[] byteArray256 = Encoding.ASCII.GetBytes(input);
                        MemoryStream stream256 = new MemoryStream(byteArray256);
                        retVal = myhmacsha256.ComputeHash(stream256).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
                        break;

                    default:
                        HMACSHA1 myhmacshaDefault = new HMACSHA1(bKey);
                        byte[] byteArrayDefault = Encoding.ASCII.GetBytes(input);
                        MemoryStream streamDefault = new MemoryStream(byteArrayDefault);
                        retVal = myhmacshaDefault.ComputeHash(streamDefault).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
                        break;
                }
                return retVal;
            }
            catch (Exception ex)
            {
                Console.WriteLine("HmacGenerator: input={0}, key={1}, type={2}, ex={3}", input, key, type, ex.Message);
                return "";
            }
        }
    }
}
