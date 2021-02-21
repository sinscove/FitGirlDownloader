using System;
using Terminal.Gui;

namespace FitGirlDownloader.Ui
{
    public static class RatelimitErrorDialog
    {
        private static Dialog _internalDialog;
        private static Label  _dialogLabel;
        
        private static Application.RunState _runState;
        
        public static void Show(int waitTimeInSeconds)
        {
            string message      = $"The program will wait {waitTimeInSeconds} seconds and then retry the request.";
            int    messageWidth = TextFormatter.MaxWidth(message, 50);

            if (_internalDialog != null)
            {
                _internalDialog.Height = TextFormatter.MaxLines(message, messageWidth) + 2;
                _internalDialog.Width  = Math.Max(50, Math.Max(_internalDialog.Title.RuneCount + 8, messageWidth + 4 + 8));
                _dialogLabel.Text      = message;
                Application.RunLoop(_runState, false);
                return;
            }

            _internalDialog = new Dialog
            {
                Title       = "You are being ratelimited",
                Height      = TextFormatter.MaxLines(message, messageWidth) + 2,
                ColorScheme = Colors.Error,
            };

            _internalDialog.Width = Math.Max(50, Math.Max(_internalDialog.Title.RuneCount + 8, messageWidth + 4 + 8));

            _dialogLabel = new Label(0, 1, message)
            {
                LayoutStyle   = LayoutStyle.Computed,
                TextAlignment = TextAlignment.Centered,
                X             = Pos.Center(),
                Y             = Pos.Center(),
                Width         = Dim.Fill(2),
                Height        = Dim.Fill()
            };

            _internalDialog.Add(_dialogLabel);

            _runState = Application.Begin(_internalDialog);
            Application.RunLoop(_runState, false);
        }

        public static void End()
        {
            if (_internalDialog == null) return;

            Application.End(_runState);
            _internalDialog.Dispose(); // For some reason disposing does not make _internalDialog null
            _internalDialog = null;
        }
    }
}