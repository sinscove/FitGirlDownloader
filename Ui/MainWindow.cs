using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace FitGirlDownloader.Ui
{
    public class MainWindow : Window
    {
        private readonly ListView     _listView;
        private readonly FitGirlCache _fitGirlCache;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MainWindow(FitGirlCache fitGirlCache) : base("FitGirl Repack Downloader")
        {
            base.Height      = Dim.Fill(1);
            base.ColorScheme = ColourScheme.Base;

            _fitGirlCache = fitGirlCache;

            _listView = new ListView
            {
                X = 0,
                Y = 0,
                Width  = Dim.Fill(),
                Height = Dim.Fill()
            };

            _listView.OpenSelectedItem += OnListViewItemSelected;

            base.Add(_listView);

            SetListViewSource(false);
        }

        public async Task SetListViewSource(bool regenerate, string search = "")
        {
            await _lock.WaitAsync();

            try
            {
                IEnumerable<string> names = (await _fitGirlCache.GetGameNames(regenerate))
                    .Where(name => name.Contains(search, StringComparison.InvariantCultureIgnoreCase));

                await _listView.SetSourceAsync(names.ToList());
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

        public void Search(Action onCompletion)
        {
            _listView.Height = Dim.Fill(1);

            TextField textField = new TextField()
            {
                X = 0,
                Y = Pos.Bottom(_listView),
                Width  = Dim.Fill(),
                Height = 1
            };

            void TextField_KeyDown(KeyEventEventArgs obj)
            {
                if (obj.KeyEvent.Key == Key.Esc || obj.KeyEvent.Key == Key.Enter || obj.KeyEvent.Key == Key.ControlH)
                {
                    if (obj.KeyEvent.Key == Key.Enter)
                    {
                        SetListViewSource(false, textField.Text.ToString());
                    }
                    else if (obj.KeyEvent.Key == Key.ControlH)
                    {
                        SetListViewSource(false);
                    }

                    base.Remove(textField);
                    _listView.Height = Dim.Fill();

                    textField.KeyDown -= TextField_KeyDown;

                    onCompletion();
                }
            }

            textField.KeyDown += TextField_KeyDown;

            base.Add(textField);
            textField.SetFocus();
        }

        private void OnListViewItemSelected(ListViewItemEventArgs args)
        {
            string gameName = args.Value.ToString();

            if (!string.IsNullOrWhiteSpace(gameName))
            {
                MirrorWindow mirrorWindow = new MirrorWindow(_fitGirlCache, gameName);
                mirrorWindow.Run(false);
            }
        }
    }
}