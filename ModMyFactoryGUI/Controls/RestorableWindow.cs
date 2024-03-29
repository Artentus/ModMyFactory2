//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ModMyFactoryGUI.Controls
{
    internal abstract class RestorableWindow : WindowBase
    {
        private volatile bool _stateRestoreInProgress;

        private WindowRestoreState _restoreState;

        public static readonly DirectProperty<RestorableWindow, WindowRestoreState> RestoreStateProperty
            = AvaloniaProperty.RegisterDirect<RestorableWindow, WindowRestoreState>(nameof(RestoreState),
                w => w.RestoreState, (w, v) => w.RestoreState = v, WindowRestoreState.Undefined, BindingMode.TwoWay);

        public WindowRestoreState RestoreState
        {
            get => _restoreState;
            set
            {
                if (value != WindowRestoreState.Undefined)
                    SetBounds(value.Position, value.Size);
                else
                    SetAndRaise(RestoreStateProperty, ref _restoreState, value);
            }
        }

        protected RestorableWindow()
        {
            PlatformImpl.PositionChanged += OnPositionChanged;
            PlatformImpl.Resized += OnSizeChanged;
        }

        private static bool SizeEqualsInt(Size first, Size second)
            => ((int)first.Width == (int)second.Width) && ((int)first.Height == (int)second.Height);

        private void SetBoundsInternal(PixelPoint position, Size size)
        {
            if (position != PlatformImpl.Position)
                PlatformImpl.Move(position);

            if (!SizeEqualsInt(size, PlatformImpl.ClientSize))
                PlatformImpl.Resize(size);

            var newState = new WindowRestoreState(position, size);
            SetAndRaise(RestoreStateProperty, ref _restoreState, newState);
        }

        protected void SetBounds(PixelPoint position, Size size)
        {
            // This is a workaround to an issue where sometimes the Window
            // position is set to -32000,-32000 for an unknown reason.
            // The chance of this position being real is pretty much 0 in
            // practice, so we can filter it out.
            if ((position.X == -32_000) && (position.Y == -32_000)) return;

            // We only care about bounds if the window is neither minimized nor maximized
            if (WindowState == WindowState.Normal)
            {
                if (!_stateRestoreInProgress) // Prevent infinite loop
                {
                    _stateRestoreInProgress = true;
                    SetBoundsInternal(position, size);
                    _stateRestoreInProgress = false;
                }
            }
        }

        protected virtual void OnPositionChanged(PixelPoint position)
            => SetBounds(position, PlatformImpl.ClientSize);

        protected virtual void OnSizeChanged(Size size)
            => SetBounds(PlatformImpl.Position, size);
    }
}
