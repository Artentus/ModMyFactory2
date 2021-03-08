//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Controls
{
    internal class FolderBrowserTextBox : UserControl
    {
        private readonly TextBox _textBox;

        private string _text;

        public static readonly DirectProperty<FolderBrowserTextBox, string> TextProperty
            = TextBox.TextProperty.AddOwner<FolderBrowserTextBox>(GetText, SetText, string.Empty);

        public string Text
        {
            get => _text;
            set
            {
                if (SetAndRaise(TextProperty, ref _text, value))
                    _textBox.Text = value;
            }
        }

        public FolderBrowserTextBox()
        {
            InitializeComponent();

            _text = string.Empty;
            _textBox = this.FindControl<TextBox>("TextBox");
            var button = this.FindControl<Button>("Button");
            button.Command = ReactiveCommand.CreateFromTask(BrowseAsync);
        }

        private static string GetText(FolderBrowserTextBox hyperLink)
            => hyperLink.Text;

        private static void SetText(FolderBrowserTextBox hyperLink, string value)
            => hyperLink.Text = value;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task BrowseAsync()
        {
            var dialog = new OpenFolderDialog();
            var dir = await dialog.ShowAsync(App.Current.MainWindow);
            if (!string.IsNullOrWhiteSpace(dir)) Text = dir;
        }
    }
}
