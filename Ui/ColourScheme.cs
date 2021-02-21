using Terminal.Gui;

namespace FitGirlDownloader.Ui
{
    public static class ColourScheme
    {
        public static ColorScheme Base { get; }
        public static ColorScheme Dialog { get; }

        static ColourScheme()
        {
            Base = new ColorScheme
            {
                Disabled  = Attribute.Make(Color.Black,     Color.Black),
                Focus     = Attribute.Make(Color.Black,     Color.Gray),
                HotFocus  = Attribute.Make(Color.BrighCyan, Color.Gray),
                HotNormal = Attribute.Make(Color.Black,     Color.BrighCyan),
                Normal    = Attribute.Make(Color.White,     Color.Cyan)
            };

            Dialog = new ColorScheme
            {
                Disabled  = Attribute.Make(Color.Black, Color.Black),
                Focus     = Attribute.Make(Color.Black, Color.DarkGray),
                HotFocus  = Attribute.Make(Color.Red,   Color.DarkGray),
                HotNormal = Attribute.Make(Color.Red,   Color.Gray),
                Normal    = Attribute.Make(Color.Black, Color.Gray)
            };
        }
    }
}