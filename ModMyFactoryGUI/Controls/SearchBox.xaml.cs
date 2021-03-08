//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ModMyFactoryGUI.Controls
{
    internal class SearchBox : UserControl
    {
        private string _text;

        public static readonly DirectProperty<SearchBox, string> TextProperty
            = TextBox.TextProperty.AddOwner<SearchBox>(GetText, SetText, string.Empty);

        public string Text
        {
            get => _text;
            set => SetAndRaise(TextProperty, ref _text, value);
        }

        public SearchBox()
        {
            InitializeComponent();
            _text = string.Empty;
        }

        private static string GetText(SearchBox box)
            => box.Text;

        private static void SetText(SearchBox box, string value)
            => box.Text = value;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
