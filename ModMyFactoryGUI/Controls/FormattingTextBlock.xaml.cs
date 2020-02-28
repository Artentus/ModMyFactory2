using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Html;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Utilities;
using Markdig;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.ViewModels;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ModMyFactoryGUI.Controls
{
    class FormattingTextBlock : UserControl, IWeakSubscriber<EventArgs>
    {
        const string RegexGroupName = "ResourceKey";
        static readonly string _css;

        public static readonly StyledProperty<string> MarkdownTextProperty
            = AvaloniaProperty.Register<FormattingTextBlock, string>(nameof(MarkdownText));

        static string ParseMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;

            var builder = new MarkdownPipelineBuilder()
                .ConfigureNewLine(Environment.NewLine)
                .DisableHtml()
                .UseAutoLinks();

            var pipeline = builder.Build();
            return Markdown.ToHtml(markdown, pipeline);
        }

        static string LoadCss()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var stream = assets.Open(new Uri("avares://ModMyFactoryGUI/Assets/html.css"));
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        static FormattingTextBlock()
        {
            _css = LoadCss();
        }


        HtmlLabel _renderer;

        public string MarkdownText
        {
            get => GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        public FormattingTextBlock()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _renderer = this.FindControl<HtmlLabel>("Renderer");
            _renderer.LinkClicked += (_, e) => PlatformHelper.OpenWebUrl(e.Event.Link);
            _renderer.BaseStylesheet = ParseCss(_css);
            ThemeViewModel.SubscribeWeak(this);
        }

        string GetColorResource(string key)
        {
            if (App.Current.TryGetThemeResource<Color>(key, out var color))
                return $"rgba({color.R},{color.G},{color.B},{color.A})";

            return "#000000";
        }

        string EvaluateMatch(Match match)
        {
            var key = match.Groups[RegexGroupName].Value;
            return GetColorResource(key);
        }

        string ParseCss(string css)
            => Regex.Replace(css, $"%_(?<{RegexGroupName}>.+)_%", EvaluateMatch);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == MarkdownTextProperty)
            {
                string markdown = (string)e.NewValue;
                string html = ParseMarkdownToHtml(markdown);
                _renderer.Text = html;
            }
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e)
        {
            _renderer.BaseStylesheet = ParseCss(_css);
            _renderer.InvalidateVisual();
        }
    }
}
