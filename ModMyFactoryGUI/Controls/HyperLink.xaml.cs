//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Helpers;
using System;

namespace ModMyFactoryGUI.Controls
{
    internal class HyperLink : UserControl
    {
        private readonly TextBlock _linkText;

        private string _text;
        private string _url;

        public static readonly DirectProperty<HyperLink, string> TextProperty
            = TextBlock.TextProperty.AddOwner<HyperLink>(GetText, SetText, string.Empty);

        public static readonly DirectProperty<HyperLink, string> UrlProperty
            = AvaloniaProperty.RegisterDirect<HyperLink, string>("Url", GetUrl, SetUrl, string.Empty);

        public string Text
        {
            get => _text;
            set
            {
                if (SetAndRaise(TextProperty, ref _text, value))
                    _linkText.Text = value;
            }
        }

        public string Url
        {
            get => _url;
            set => SetAndRaise(UrlProperty, ref _url, value);
        }

        public HyperLink()
        {
            InitializeComponent();

            _linkText = this.FindControl<TextBlock>("LinkText");
            _text = string.Empty;
            _url = string.Empty;
        }

        private static string GetText(HyperLink hyperLink)
            => hyperLink.Text;

        private static void SetText(HyperLink hyperLink, string value)
            => hyperLink.Text = value;

        private static string GetUrl(HyperLink hyperLink)
            => hyperLink.Url;

        private static void SetUrl(HyperLink hyperLink, string value)
            => hyperLink.Url = value;

        private static bool IsValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var tmp)) return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Cursor = new Cursor(StandardCursorType.Hand);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (IsValidUrl(Url))
                PlatformHelper.OpenWebUrl(Url);
        }
    }
}
