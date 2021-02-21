using System;
using System.IO;
using System.Threading.Tasks;
using FitGirlDownloader.Ui;
using NStack;
using Terminal.Gui;

namespace FitGirlDownloader
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            FitGirlCache fitGirlCache = new FitGirlCache();
            await fitGirlCache.LoadCache(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitGirlCache.json"));

            Application.Init();

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Exception exception = (Exception)eventArgs.ExceptionObject;
                MessageBox.ErrorQuery($"Unhandled {exception.GetType().FullName}", exception.Message, "OK");
            };

            MainWindow mainWindow = new MainWindow(fitGirlCache);

            StatusBar statusBar = new StatusBar();

            statusBar.Items = new []
            {
                new StatusItem(Key.Unknown, "(Ctrl+Q) Quit", Application.RequestStop),
                new StatusItem(Key.ControlS, ustring.Make("(Ctrl+S) Search"), () =>
                {
                    Application.Top.Remove(statusBar);
                    mainWindow.Search(() => 
                    {
                        mainWindow.Height = Dim.Fill(1);
                        Application.Top.Add(statusBar);
                    });
                }),
                new StatusItem(Key.ControlH, ustring.Make("(Ctrl+H) Home"), () => mainWindow.SetListViewSource(false)),
                new StatusItem(Key.ControlR, ustring.Make("(Ctrl+R) Regenerate Cache"), () => mainWindow.SetListViewSource(true))
            };

            Application.Top.Add(mainWindow, statusBar);

            AboutWindow.Run();
            Application.Run();
            Application.Shutdown();
        }
    }
}