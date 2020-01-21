using Avalonia;
using Avalonia.Markup.Xaml;
using ModMyFactoryGUI.Controls;
using System;

namespace ModMyFactoryGUI.Views
{
    partial class AboutWindow : WindowBase
    {
        public AboutWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            //this.PlatformImpl.Handle.Handle
        }
    }
}
