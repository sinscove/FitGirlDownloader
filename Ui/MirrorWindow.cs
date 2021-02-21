using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace FitGirlDownloader.Ui
{
    public class MirrorWindow
    {
        private readonly FitGirlCache _fitGirlCache;
        private readonly string       _gameName;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MirrorWindow(FitGirlCache fitGirlCache, string gameName)
        {
            _fitGirlCache = fitGirlCache;
            _gameName     = gameName;
        }

        public async Task Run(bool regenerate)
        {
            await _lock.WaitAsync();

            try
            {
                IEnumerable<Mirror> mirrors = await _fitGirlCache.TryGetMirrors(_gameName, regenerate);
                List<string> mirrorNames    = mirrors.Select(mirror => mirror.MirrorName).ToList();

                Button okButton = new Button("OK");
                okButton.Clicked += Application.RequestStop;

                Button regenerateButton = new Button("Regenerate");
                regenerateButton.Clicked += async () =>
                {
                    Application.RequestStop();
                    await Run(true);
                };

                Dialog internalDialog = new Dialog(_gameName, okButton, regenerateButton)
                {
                    Width       = Dim.Percent(65),
                    Height      = Dim.Percent(65),
                    ColorScheme = ColourScheme.Dialog
                };

                Label description = new Label(_fitGirlCache.GetDescription(_gameName) ?? "")
                {
                    X = 1,
                    Y = 0,
                    Width  = Dim.Fill(1),
                    Height = Dim.Percent(20)
                };

                Label mirrorsTitle = new Label("Mirrors:")
                {
                    X = 0,
                    Y = Pos.Bottom(description) + 2,
                    Width = Dim.Fill(1),
                };

                ListView listView = new ListView(mirrorNames)
                {
                    X = 2,
                    Y = Pos.Bottom(mirrorsTitle),
                    Width  = Dim.Fill(1),
                    Height = Dim.Fill(1)
                };

                listView.OpenSelectedItem += OnListViewItem;

                internalDialog.Add(description, mirrorsTitle, listView);

                Application.Run(internalDialog);
                internalDialog.Dispose();
            }
            catch (Exception exception)
            {
                MessageBox.ErrorQuery(exception.GetType().FullName, exception.Message, "OK");
            }
            finally
            {
                _lock.Release();
            }
        }

        private void OnListViewItem(ListViewItemEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Value.ToString()))
            {
                return;
            }
            
            IEnumerable<Mirror> mirrors = _fitGirlCache.TryGetMirrors(_gameName, false).Result;
            string url = mirrors.First(mirror => mirror.MirrorName == args.Value.ToString()).MirrorUrl;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                MessageBox.ErrorQuery("Error", $"Cannot open url \"{url}\" on this platform!", "OK");
            }
        }
    }
}