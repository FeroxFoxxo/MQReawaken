using System;
using System.Runtime.InteropServices;

namespace AssetStudio;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Color : IEquatable<Color>
{
    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public override int GetHashCode() => ((Vector4)this).GetHashCode();

    public override bool Equals(object other)
    {
        if (!(other is Color))
            return false;
        return Equals((Color)other);
    }

    public bool Equals(Color other) => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public static Color operator +(Color a, Color b) => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);

    public static Color operator -(Color a, Color b) => new(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);

    public static Color operator *(Color a, Color b) => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);

    public static Color operator *(Color a, float b) => new(a.R * b, a.G * b, a.B * b, a.A * b);

    public static Color operator *(float b, Color a) => new(a.R * b, a.G * b, a.B * b, a.A * b);

    public static Color operator /(Color a, float b) => new(a.R / b, a.G / b, a.B / b, a.A / b);

    public static bool operator ==(Color lhs, Color rhs) => (Vector4)lhs == (Vector4)rhs;

    public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

    public static implicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);
}
