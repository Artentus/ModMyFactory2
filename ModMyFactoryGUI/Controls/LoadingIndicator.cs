//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Controls.Primitives;
using ModMyFactoryGUI.Helpers;
using System;

namespace ModMyFactoryGUI.Controls
{
    internal class LoadingIndicator : TemplatedControl
    {
        private static readonly TimeSpan MinDuration = TimeSpan.FromMilliseconds(100);

        private string? _text;
        private double _dotSize;
        private int _dotCount;
        private double _radius;
        private double _animationSize;
        private TimeSpan _revolutionDuration;

        public static readonly DirectProperty<LoadingIndicator, string?> TextProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, string?>(
                nameof(Text), c => c.Text, (c, v) => c.Text = v);

        public static readonly DirectProperty<LoadingIndicator, double> DotSizeProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, double>(
                nameof(DotSize), c => c.DotSize, (c, v) => c.DotSize = v, 1);

        public static readonly DirectProperty<LoadingIndicator, int> DotCountProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, int>(
                nameof(DotCount), c => c.DotCount, (c, v) => c.DotCount = v, 10);

        public static readonly DirectProperty<LoadingIndicator, double> RadiusProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, double>(
                nameof(Radius), c => c.Radius, (c, v) => c.Radius = v, 40);

        public static readonly DirectProperty<LoadingIndicator, double> AnimationSizeProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, double>(
                nameof(AnimationSize), c => c.AnimationSize, (c, v) => c.AnimationSize = v, 100);

        public static readonly DirectProperty<LoadingIndicator, TimeSpan> RevolutionDurationProperty
            = AvaloniaProperty.RegisterDirect<LoadingIndicator, TimeSpan>(
                nameof(RevolutionDuration), c => c.RevolutionDuration, (c, v) => c.RevolutionDuration = v, TimeSpan.FromSeconds(1));

        public string? Text
        {
            get => _text;
            set => SetAndRaise(TextProperty, ref _text, value);
        }

        public double DotSize
        {
            get => _dotSize;
            set => SetAndRaise(DotSizeProperty, ref _dotSize, Math.Max(value, 1));
        }

        public int DotCount
        {
            get => _dotCount;
            set => SetAndRaise(DotCountProperty, ref _dotCount, Math.Max(value, 1));
        }

        public double Radius
        {
            get => _radius;
            set => SetAndRaise(RadiusProperty, ref _radius, value);
        }

        public double AnimationSize
        {
            get => _animationSize;
            set => SetAndRaise(AnimationSizeProperty, ref _animationSize, value);
        }

        public TimeSpan RevolutionDuration
        {
            get => _revolutionDuration;
            set => SetAndRaise(RevolutionDurationProperty, ref _revolutionDuration, value.Max(MinDuration));
        }
    }
}
