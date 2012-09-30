using Coding4Fun.Phone.Controls.Data;

namespace Helpers
{
    public class AppAttributes
    {
        public static string Version
        {
            get
            {
                return PhoneHelper.GetAppAttribute("Version");
            }
        }

        public static float VersionAsFloat
        {
            get
            {
                string version = PhoneHelper.GetAppAttribute("Version");
                var versions = version.Split('.');
                return
                      int.Parse(versions[0])
                    + (float)int.Parse(versions[1]) * 0.01f
                    + (float)int.Parse(versions[2]) * 0.0001f
                    + (float)int.Parse(versions[3]) * 0.000001f
                    ;
            }
        }
    }
}
