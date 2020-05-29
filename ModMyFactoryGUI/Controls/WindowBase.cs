// -----------------------------------------------------------------------------------------------------------------------------------------------
// Large parts of this file are taken from
// https://github.com/VitalElement/AvalonStudio.Shell/blob/bb220b049fd2537e864624fe23d24a1a63865986/src/AvalonStudio.Shell/Controls/MetroWindow.cs
// -----------------------------------------------------------------------------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using ModMyFactoryGUI.MVVM;
using System;
using System.Runtime.InteropServices;

namespace ModMyFactoryGUI.Controls
{
    internal abstract class WindowBase : Window, IView, IStyleable
    {
        #region Win32

        private enum ClassLongIndex : int
        {
            GCLP_MENUNAME = -8,
            GCLP_HBRBACKGROUND = -10,
            GCLP_HCURSOR = -12,
            GCLP_HICON = -14,
            GCLP_HMODULE = -16,
            GCL_CBWNDEXTRA = -18,
            GCL_CBCLSEXTRA = -20,
            GCLP_WNDPROC = -24,
            GCL_STYLE = -26,
            GCLP_HICONSM = -34,
            GCW_ATOM = -32
        }

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
        private static extern IntPtr SetClassLong64(IntPtr hWnd, ClassLongIndex nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLong")]
        private static extern IntPtr SetClassLong32(IntPtr hWnd, ClassLongIndex nIndex, IntPtr dwNewLong);

        private static IntPtr SetClassLong(IntPtr hWnd, ClassLongIndex nIndex, IntPtr dwNewLong)
        {
            if (Environment.Is64BitProcess)
                return SetClassLong64(hWnd, nIndex, dwNewLong);

            return SetClassLong32(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        private static extern IntPtr GetClassLong64(IntPtr hWnd, ClassLongIndex nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        private static extern uint GetClassLong32(IntPtr hWnd, ClassLongIndex nIndex);

        private static IntPtr GetClassLong(IntPtr hWnd, ClassLongIndex nIndex)
        {
            if (Environment.Is64BitProcess)
                return GetClassLong64(hWnd, nIndex);

            return new IntPtr(GetClassLong32(hWnd, nIndex));
        }

        #endregion Win32


        private Grid _bottomHorizontalGrip;

        private Grid _bottomLeftGrip;

        private Grid _bottomRightGrip;

        private Button _closeButton;

        private Image _icon;

        private Grid _leftVerticalGrip;

        private Button _minimiseButton;

        private Button _restoreButton;

        private Grid _rightVerticalGrip;

        private DockPanel _titleBar;

        private Grid _topHorizontalGrip;

        private Grid _topLeftGrip;

        private Grid _topRightGrip;

        public static readonly StyledProperty<Control> TitleBarContentProperty =
            AvaloniaProperty.Register<WindowBase, Control>(nameof(TitleBarContent));

        public static readonly StyledProperty<bool> HasClientDecorationsProperty =
            AvaloniaProperty.Register<WindowBase, bool>(nameof(HasClientDecorations));

        public bool HasClientDecorations
        {
            get => GetValue(HasClientDecorationsProperty);
            set => SetValue(HasClientDecorationsProperty, value);
        }

        public Control TitleBarContent
        {
            get => GetValue(TitleBarContentProperty);
            set => SetValue(TitleBarContentProperty, value);
        }

        object IView.ViewModel
        {
            get => DataContext;
            set => DataContext = value;
        }

        Type IStyleable.StyleKey => typeof(WindowBase);

        protected WindowBase()
        {
            App.Current.Themes.EnableThemes(this);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // do this in code or we get a delay in osx.
                HasSystemDecorations = false;
                HasClientDecorations = true;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var classes = (int)GetClassLong(PlatformImpl.Handle.Handle, ClassLongIndex.GCL_STYLE);
                    classes |= 0x00020000;
                    SetClassLong(PlatformImpl.Handle.Handle, ClassLongIndex.GCL_STYLE, new IntPtr(classes));
                }
            }
            else
            {
                HasSystemDecorations = true;
                HasClientDecorations = false;
            }
        }

        static WindowBase()
        {
            PseudoClass<WindowBase, WindowState>(WindowStateProperty, x => x == WindowState.Maximized, ":maximised");
        }

        private void ToggleWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;

                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_topHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.North, e);
            }
            else if (_bottomHorizontalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.South, e);
            }
            else if (_leftVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.West, e);
            }
            else if (_rightVerticalGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.East, e);
            }
            else if (_topLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthWest, e);
            }
            else if (_bottomLeftGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthWest, e);
            }
            else if (_topRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.NorthEast, e);
            }
            else if (_bottomRightGrip.IsPointerOver)
            {
                BeginResizeDrag(WindowEdge.SouthEast, e);
            }
            else if (_titleBar.IsPointerOver)
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            }

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            _titleBar = e.NameScope.Find<DockPanel>("titlebar");
            _minimiseButton = e.NameScope.Find<Button>("minimiseButton");
            _restoreButton = e.NameScope.Find<Button>("restoreButton");
            _closeButton = e.NameScope.Find<Button>("closeButton");
            _icon = e.NameScope.Find<Image>("icon");

            _topHorizontalGrip = e.NameScope.Find<Grid>("topHorizontalGrip");
            _bottomHorizontalGrip = e.NameScope.Find<Grid>("bottomHorizontalGrip");
            _leftVerticalGrip = e.NameScope.Find<Grid>("leftVerticalGrip");
            _rightVerticalGrip = e.NameScope.Find<Grid>("rightVerticalGrip");

            _topLeftGrip = e.NameScope.Find<Grid>("topLeftGrip");
            _bottomLeftGrip = e.NameScope.Find<Grid>("bottomLeftGrip");
            _topRightGrip = e.NameScope.Find<Grid>("topRightGrip");
            _bottomRightGrip = e.NameScope.Find<Grid>("bottomRightGrip");

            _minimiseButton.Click += (sender, ee) => { WindowState = WindowState.Minimized; };

            _restoreButton.Click += (sender, ee) => { ToggleWindowState(); };

            _titleBar.DoubleTapped += (sender, ee) => { ToggleWindowState(); };

            _closeButton.Click += (sender, ee) => { Close(); };

            _icon.DoubleTapped += (sender, ee) => { Close(); };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _topHorizontalGrip.IsVisible = false;
                _bottomHorizontalGrip.IsHitTestVisible = false;
                _leftVerticalGrip.IsHitTestVisible = false;
                _rightVerticalGrip.IsHitTestVisible = false;
                _topLeftGrip.IsHitTestVisible = false;
                _bottomLeftGrip.IsHitTestVisible = false;
                _topRightGrip.IsHitTestVisible = false;
                _bottomRightGrip.IsHitTestVisible = false;

                BorderThickness = new Thickness();
            }
        }
    }
}
