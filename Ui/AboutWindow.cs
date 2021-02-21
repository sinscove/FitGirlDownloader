using System.Text;
using Terminal.Gui;

namespace FitGirlDownloader.Ui
{
    public static class AboutWindow
    {
        public static void Run()
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine(" ");
            message.AppendLine("fitgirl-repacks.site is THE ONLY official site of FG's repacks.");
            message.AppendLine("Every single FG repack installer has a link inside, which leads to it.");
            message.AppendLine(" ");
            message.AppendLine("This app scrapes its data from fitgirl-repacks.site. As such, all links to files originate from there.");
            message.AppendLine(" ");

            MessageBox.Query("FitGirl Repack Downloader", message.ToString(), "OK");
        }
    }
}