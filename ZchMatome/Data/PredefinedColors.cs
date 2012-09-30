using System.Collections.ObjectModel;

namespace ZchMatome.Data
{
    public class PredefinedColors
    {
        public Collection<NamedSolidColorBrush> AccentColors
        {
            get
            {
                return Constants.Media.AccentColors;
            }
        }
    }
}
