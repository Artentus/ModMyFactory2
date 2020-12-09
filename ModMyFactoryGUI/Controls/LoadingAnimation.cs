//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using ModMyFactoryGUI.Helpers;
using System;
using System.Globalization;

namespace ModMyFactoryGUI.Controls
{
    internal class LoadingAnimation : TemplatedControl
    {
        private static readonly TimeSpan MinDuration = TimeSpan.FromMilliseconds(100);

        private double _dotSize;
        private int _dotCount;
        private double _radius;
        private TimeSpan _revolutionDuration;
        private Canvas _dotCanvas;

        public static readonly DirectProperty<LoadingAnimation, double> DotSizeProperty
            = AvaloniaProperty.RegisterDirect<LoadingAnimation, double>(
                nameof(DotSize), c => c.DotSize, (c, v) => c.DotSize = v, 1);

        public static readonly DirectProperty<LoadingAnimation, int> DotCountProperty
            = AvaloniaProperty.RegisterDirect<LoadingAnimation, int>(
                nameof(DotCount), c => c.DotCount, (c, v) => c.DotCount = v, 10);

        public static readonly DirectProperty<LoadingAnimation, double> RadiusProperty
            = AvaloniaProperty.RegisterDirect<LoadingAnimation, double>(
                nameof(Radius), c => c.Radius, (c, v) => c.Radius = v, 40);

        public static readonly DirectProperty<LoadingAnimation, TimeSpan> RevolutionDurationProperty
            = AvaloniaProperty.RegisterDirect<LoadingAnimation, TimeSpan>(
                nameof(RevolutionDuration), c => c.RevolutionDuration, (c, v) => c.RevolutionDuration = v, TimeSpan.FromSeconds(1));

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

        public TimeSpan RevolutionDuration
        {
            get => _revolutionDuration;
            set => SetAndRaise(RevolutionDurationProperty, ref _revolutionDuration, value.Max(MinDuration));
        }

        private static (double, double) PointOnCircle((double, double) center, double radius, double angle)
        {
            double x = Math.Cos(angle) * radius + center.Item1;
            double y = Math.Sin(angle) * radius + center.Item2;
            return (x, y);
        }

        private Animation CreateAnimation(TimeSpan delay)
        {
            var start = new KeyFrame { Cue = Cue.Parse("1%", CultureInfo.InvariantCulture) };
            start.Setters.Add(new Setter(OpacityProperty, 1.0));
            var end = new KeyFrame { Cue = Cue.Parse("100%", CultureInfo.InvariantCulture) };
            end.Setters.Add(new Setter(OpacityProperty, 0.0));

            var anim = new Animation
            {
                Duration = RevolutionDuration,
                IterationCount = IterationCount.Infinite,
                Delay = delay
            };
            anim.Children.Add(start);
            anim.Children.Add(end);

            return anim;
        }

        private void PerformLayout()
        {
            if (_dotCanvas is null) return;
            _dotCanvas.Children.Clear();

            var center = (Width / 2, Height / 2);
            for (int i = 0; i < DotCount; i++)
            {
                double angle = (2 * Math.PI) / DotCount * i;
                var (x, y) = PointOnCircle(center, Radius, angle);
                var ellipse = new Ellipse() { Width = DotSize, Height = DotSize, Opacity = 0 };

                double offset = DotSize / 2;
                // Coordinates swapped on purpose
                Canvas.SetLeft(ellipse, y - offset);
                Canvas.SetTop(ellipse, x - offset);

                var delay = RevolutionDuration.Divide(DotCount).Multiply(i);
                var style = new Style(s => s.OfType(typeof(Ellipse)));
                style.Animations.Add(CreateAnimation(delay));
                ellipse.Styles.Add(style);

                _dotCanvas.Children.Add(ellipse);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if ((e.Property == DotSizeProperty)
                || (e.Property == DotCountProperty)
                || (e.Property == RadiusProperty))
            {
                PerformLayout();
            }
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            _dotCanvas = e.NameScope.Find<Canvas>("PART_DotCanvas");
            PerformLayout();
        }
    }
}
