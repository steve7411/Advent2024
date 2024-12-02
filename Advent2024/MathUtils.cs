using System.Numerics;

namespace Advent2024;

internal static class MathUtils {
    public static T LCM<T>(T a, T b) where T : INumber<T> =>
        T.Abs(a * b) / GCD(a, b);

    public static T GCD<T>(T a, T b) where T : INumber<T> =>
        b == T.Zero ? a : GCD(b, a % b);

    public static T GetNextWithSamePopulation<T>(T n) where T : IBinaryInteger<T> {
        var lsb = n & -n;
        var ripple = n + lsb;
        var newLsb = ripple & -ripple;
        var ones = ((newLsb / lsb) >> 1) - T.One;
        return ripple | ones;
    }

    public static T CombinationCount<T>(T n, T r) where T : INumber<T> {
        var ans = T.One;
        for (T i = T.One, j = n - r + T.One; i <= r; ++i, ++j)
            ans = ans * j / i;
        return ans;
    }

    public static uint ReverseBits(uint n) {
        const uint EVEN_BITS = 0x55555555;
        const uint EVEN_ADJACENT_PAIR_BITS = 0x33333333;
        const uint EVEN_NIBBLES = 0x0F0F0F0F;
        const uint EVEN_BYTES = 0x00FF00FF;
        const uint EVEN_ADJACENT_PAIR_BYTES = 0x0000FFFF;

        // Swap even and odd bits, then even and odd pairs, then even and odd nibbles, etc
        n = (n & EVEN_BITS) << 1 | (n & ~EVEN_BITS) >> 1;
        n = (n & EVEN_ADJACENT_PAIR_BITS) << 2 | (n & ~EVEN_ADJACENT_PAIR_BITS) >> 2;
        n = (n & EVEN_NIBBLES) << 4 | (n & ~EVEN_NIBBLES) >> 4;
        n = (n & EVEN_BYTES) << 8 | (n & ~EVEN_BYTES) >> 8;
        n = (n & EVEN_ADJACENT_PAIR_BYTES) << 16 | (n & ~EVEN_ADJACENT_PAIR_BYTES) >> 16;
        return n;
    }

    public static ulong ReverseBits(ulong n) {
        const ulong EVEN_BITS = 0x5555555555555555;
        const ulong EVEN_ADJACENT_PAIR_BITS = 0x3333333333333333;
        const ulong EVEN_NIBBLES = 0x0F0F0F0F0F0F0F0F;
        const ulong EVEN_BYTES = 0x00FF00FF00FF00FF;
        const ulong EVEN_ADJACENT_PAIR_BYTES = 0x0000FFFF0000FFFF;
        const ulong EVEN_ADJACENT_QUAD_BYTES = 0x00000000FFFFFFFF;

        // Swap even and odd bits, then even and odd pairs, then even and odd nibbles, etc
        n = (n & EVEN_BITS) << 1 | (n & ~EVEN_BITS) >> 1;
        n = (n & EVEN_ADJACENT_PAIR_BITS) << 2 | (n & ~EVEN_ADJACENT_PAIR_BITS) >> 2;
        n = (n & EVEN_NIBBLES) << 4 | (n & ~EVEN_NIBBLES) >> 4;
        n = (n & EVEN_BYTES) << 8 | (n & ~EVEN_BYTES) >> 8;
        n = (n & EVEN_ADJACENT_PAIR_BYTES) << 16 | (n & ~EVEN_ADJACENT_PAIR_BYTES) >> 16;
        n = (n & EVEN_ADJACENT_QUAD_BYTES) << 32 | (n & ~EVEN_ADJACENT_QUAD_BYTES) >> 32;
        return n;
    }
}
