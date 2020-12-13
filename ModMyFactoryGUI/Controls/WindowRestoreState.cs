//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia;
using System;
using System.Text;

namespace ModMyFactoryGUI.Controls
{
    internal readonly struct WindowRestoreState : IEquatable<WindowRestoreState>
    {
        public static readonly WindowRestoreState Undefined = default;

        public readonly PixelPoint Position;

        public readonly Size Size;

        public readonly bool IsDefined;

        public WindowRestoreState(PixelPoint position, Size size)
        {
            IsDefined = true;
            (Position, Size) = (position, size);
        }

        public static bool operator ==(WindowRestoreState first, WindowRestoreState second)
            => first.Equals(second);

        public static bool operator !=(WindowRestoreState first, WindowRestoreState second)
            => !first.Equals(second);

        public static bool TryParse(string? s, out WindowRestoreState result)
        {
            result = Undefined;

            if (string.IsNullOrEmpty(s)) return false;
            if (string.Equals(s, nameof(Undefined), StringComparison.OrdinalIgnoreCase)) return true;

            var parts = s.Split(';');
            if (parts.Length != 4) return false;

            int[] bounds = new int[4];
            for (int i = 0; i < bounds.Length; i++)
            {
                if (!int.TryParse(parts[i], out int val))
                    return false;

                bounds[i] = val;
            }

            var pos = new PixelPoint(bounds[0], bounds[1]);
            var size = new Size(bounds[2], bounds[3]);
            result = new WindowRestoreState(pos, size);
            return true;
        }

        public static WindowRestoreState Parse(string? s)
        {
            if (TryParse(s, out var result)) return result;
            else throw new FormatException();
        }

        public WindowRestoreState WithPosition(PixelPoint position)
            => new WindowRestoreState(position, Size);

        public WindowRestoreState WithSize(Size size)
            => new WindowRestoreState(Position, size);

        public bool Equals(WindowRestoreState other)
        {
            // The undefined state overrides the other properties
            if (this.IsDefined != other.IsDefined) return false;
            if (!this.IsDefined && !other.IsDefined) return true;

            return (this.Position == other.Position) && (this.Size == other.Size);
        }

        public override bool Equals(object? obj)
        {
            if (obj is WindowRestoreState other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
        {
            if (!IsDefined) return int.MinValue; // The undefined state overrides the other properties
            else return Position.GetHashCode() ^ Size.GetHashCode();
        }

        public override string ToString()
        {
            if (!IsDefined) return nameof(Undefined);

            var sb = new StringBuilder();
            sb.AppendFormat("{0};{1};{2};{3}", Position.X, Position.Y, (int)Size.Width, (int)Size.Height);
            return sb.ToString();
        }
    }

    internal sealed class WindowRestoreStateConverter : StringJsonConverter<WindowRestoreState>
    {
        protected override WindowRestoreState Create(string? token)
            => WindowRestoreState.Parse(token);

        protected override string Tokenize(WindowRestoreState value)
            => value.ToString();
    }
}
