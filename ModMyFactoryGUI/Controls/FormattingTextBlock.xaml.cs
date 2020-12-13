//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

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
    internal class FormattingTextBlock : UserControl, IWeakSubscriber<EventArgs>
    {
        private const string RegexGroupName = "ResourceKey";
        private static readonly string _css;

        private HtmlLabel _renderer;

        public static readonly StyledProperty<string> MarkdownTextProperty
            = AvaloniaProperty.Register<FormattingTextBlock, string>(nameof(MarkdownText), string.Empty);

        public string MarkdownText
        {
            get => GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        static FormattingTextBlock()
        {
            _css = LoadCss();
        }

        public FormattingTextBlock()
        {
            InitializeComponent();

            _renderer = this.FindControl<HtmlLabel>("Renderer");
            _renderer.LinkClicked += (_, e) => PlatformHelper.OpenWebUrl(e.Event.Link);
            _renderer.BaseStylesheet = ParseCss(_css);
            ThemeViewModel.SubscribeWeak(this);
        }

        private static string ParseMarkdownToHtml(string? markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;

            var builder = new MarkdownPipelineBuilder()
                .ConfigureNewLine(Environment.NewLine)
                .DisableHtml()
                .UseAutoLinks();

            var pipeline = builder.Build();
            return Markdown.ToHtml(markdown, pipeline);
        }

        private static string LoadCss()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var stream = assets.Open(new Uri("avares://ModMyFactoryGUI/Assets/html.css"));
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private string GetColorResource(string key)
        {
            if (App.Current.TryGetThemeResource<Color>(key, out var color))
                return $"rgba({color.R},{color.G},{color.B},{color.A})";

            return "#000000";
        }

        private string EvaluateMatch(Match match)
        {
            var key = match.Groups[RegexGroupName].Value;
            return GetColorResource(key);
        }

        private string ParseCss(string css)
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
