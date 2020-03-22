using ModMyFactory.BaseTypes;
using ModMyFactoryGUI.Controls;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ModMyFactoryGUI
{
    internal interface IMessage
    {
        Task<DialogResult> Show();
    }

    internal interface IErrorMessage : IMessage
    {
        Task<DialogResult> Show(Exception exception);
    }

    internal static class Messages
    {
        private class Message : IMessage
        {
            private readonly string _key;
            private readonly MessageKind _kind;
            private readonly DialogOptions _options;

            internal Message(string key, MessageKind kind, DialogOptions options)
                => (_key, _kind, _options) = (key, kind, options);

            public virtual Task<DialogResult> Show()
            {
                string titleKey = _key + "_Title";
                string messageKey = _key + "_Message";
                string title = (string)App.Current.Locales.GetResource(titleKey);
                string message = (string)App.Current.Locales.GetResource(messageKey);

                return MessageBox.Show(title, message, _kind, _options);
            }
        }

        private class WarningMessage : Message
        {
            private readonly string _messageTemplate;

            internal WarningMessage(string key, string messageTemplate)
                : base(key, MessageKind.Warning, DialogOptions.Ok)
                => _messageTemplate = messageTemplate;

            public override Task<DialogResult> Show()
            {
                Log.Warning(_messageTemplate);
                return base.Show();
            }
        }

        private class ErrorMessage : Message, IErrorMessage
        {
            private readonly string _messageTemplate;

            internal ErrorMessage(string key, string messageTemplate)
                : base(key, MessageKind.Error, DialogOptions.Ok)
                => _messageTemplate = messageTemplate;

            public override Task<DialogResult> Show()
            {
                Log.Error(_messageTemplate);
                return base.Show();
            }

            public Task<DialogResult> Show(Exception exception)
            {
                Log.Error(exception, _messageTemplate);
                return Show();
            }
        }

        public static readonly IErrorMessage ConnectionError
            = new ErrorMessage("ConnectionError", "Failed to connect to server");
        public static readonly IErrorMessage TimeoutError
            = new ErrorMessage("TimeoutError", "Timeout while trying to connect to server");
        public static readonly IErrorMessage ServerError
            = new ErrorMessage("ServerError", "An unknown server error occurred");

        public static IErrorMessage FileIntegrityError(FileInfo file, SHA1Hash expected, SHA1Hash actual)
            => new ErrorMessage("FileIntegrityError", $"Checksum mismatch for file {file.Name}, expected {expected} but got {actual}");

        public static IMessage InvalidModFile(FileInfo file)
            => new WarningMessage("InvalidModFile", $"File {file.Name} is not a valid mod");
    }
}
