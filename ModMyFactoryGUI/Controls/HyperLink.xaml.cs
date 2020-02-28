using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Helpers;
using System;

namespace ModMyFactoryGUI.Controls
{
    public class HyperLink : UserControl
    {
        public static readonly DirectProperty<HyperLink, string> TextProperty
            = TextBlock.TextProperty.AddOwner<HyperLink>(GetText, SetText, string.Empty);

        static string GetText(HyperLink hyperLink)
            => hyperLink.Text;

        static void SetText(HyperLink hyperLink, string value)
            => hyperLink.Text = value;

        public static readonly DirectProperty<HyperLink, string> UrlProperty
            = AvaloniaProperty.RegisterDirect<HyperLink, string>("Url", GetUrl, SetUrl, string.Empty);

        static string GetUrl(HyperLink hyperLink)
            => hyperLink.Url;

        static void SetUrl(HyperLink hyperLink, string value)
            => hyperLink.Url = value;


        readonly TextBlock _linkText;
        string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _linkText.Text = value;
            }
        }

        public string Url { get; set; }

        public HyperLink()
        {
            this.InitializeComponent();

            _linkText = this.FindControl<TextBlock>("LinkText");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Cursor = new Cursor(StandardCursorType.Hand);
        }

        static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var tmp)) return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (IsValidUrl(Url))
                PlatformHelper.OpenWebUrl(Url);
        }
    }
}
