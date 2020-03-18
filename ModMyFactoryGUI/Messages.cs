using ModMyFactoryGUI.Controls;
using Serilog;
using System;
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

            public Task<DialogResult> Show()
            {
                string titleKey = _key + "_Title";
                string messageKey = _key + "_Message";
                string title = (string)App.Current.LocaleManager.GetResource(titleKey);
                string message = (string)App.Current.LocaleManager.GetResource(messageKey);

                return MessageBox.Show(title, message, _kind, _options);
            }
        }

        private class ErrorMessage : Message, IErrorMessage
        {
            private readonly string _messageTemplate;

            internal ErrorMessage(string key, string messageTemplate)
                : base(key, MessageKind.Error, DialogOptions.Ok)
                => _messageTemplate = messageTemplate;

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
    }
}
