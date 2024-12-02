using System;
using System.Numerics;

namespace Advent2024;
public static class PointTupleExtensions {
    public static (float x, float y) Add(this (float x, float y) a, (float x, float y) b) {
        return (a.x + b.x, a.y + b.y);
    }

    public static (float x, float y) Subtract(this (float x, float y) a, (float x, float y) b) {
        return (a.x - b.x, a.y - b.y);
    }

    public static (float x, float y) Subtract(this (float x, float y) a, Vector2D<int> b) {
        return (a.x - b.x, a.y - b.y);
    }

    public static Vector2D<int> Round(this (float x, float y) a) {
        return new((int)Math.Round(a.x), (int)Math.Round(a.y));
    }

    public static Vector2D<int> Truncate(this (float x, float y) a) {
        return new((int)a.x, (int)a.y);
    }

    public static (float x, float y) Abs(this (float x, float y) a) {
        return new(Math.Abs(a.x), Math.Abs(a.y));
    }

    public static float Dot(this (float x, float y) a, (float x, float y) b) {
        return a.x * b.x + a.y * b.y;
    }

    public static (float, float) Multiply(this (float x, float y) a, float scalar) {
        return (a.x * scalar, a.y * scalar);
    }

    public static (float, float) Multiply(this (float x, float y) a, (float x, float y) scale) {
        return (a.x * scale.x, a.y * scale.y);
    }

    public static (float x, float y, float z) Subtract(this (float x, float y, float z) a, (float x, float y, float z) b) {
        return (a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static float Dot(this (float x, float y, float z) a, (float x, float y, float z) b) {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static (int x, int y) Swap(this (int x, int y) a) => (a.y, a.x);

    public static T ManhattanDistanceTo<T>(this (T x, T y) a, (T x, T y) b) where T : INumber<T> => T.Abs(a.x - b.x) + T.Abs(a.y - b.y);
}
