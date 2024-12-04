using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Advent2024.Day04;

internal class Day04 : DayBase {
    private readonly byte[] grid;
    private readonly int width;
    private readonly int height;

    public Day04() {
        using var reader = GetDataReader();
        (width, height) = reader.BaseStream.GetLineInfoForRegularFile();
        grid = GC.AllocateUninitializedArray<byte>(width * height);
        Span<byte> span = grid;
        while (!reader.EndOfStream) {
            reader.Read(span[..width]);
            reader.ConsumeFullNewLine(reader.Read());
            span = span[width..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int IsLinearMatch(ReadOnlySpan<byte> str, Vector2D<int> pos, Vector2D<int> dir) {
        var end = pos + dir * (str.Length - 1);
        if (end.x < 0 | end.y < 0 | end.x >= width | end.y >= height)
            return 0;

        var delta = dir.y * width + dir.x;
        for (int i = 0, idx = pos.y * width + pos.x; i < str.Length; ++i, idx += delta) {
            if (grid[idx] != str[i])
                return 0;
        }
        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int IsCrossMas(int idx) {
        const uint CROSS_MASK = 0xFF_00_FF;
        const uint SWAP = 0x1E_00_1E;

        var above = Unsafe.As<byte, uint>(ref grid[idx - width - 1]) & CROSS_MASK;
        if (above is not (0x4D_00_4D or 0x4D_00_53 or 0x53_00_4D or 0x53_00_53))
            return 0;

        var below = Unsafe.As<byte, uint>(ref grid[idx + width - 1]) & CROSS_MASK;
        return ((above << 16 | above >>> 16) & CROSS_MASK ^ SWAP) == below ? 1 : 0;
    }

    public override object? Part1() {
        const uint FORWARD = 0x53_41_4D_58;
        const uint BACKWARD = 0x58_4D_41_53;

        var xmasCount = 0;
        var maxXForward = width - 3;
        ReadOnlySpan<Vector2D<int>> dirs = [(-1, -1), (0, -1), (1, -1), (-1, 1), (0, 1), (1, 1)];
        for (int y = 0, rowStart = 0; y < height; ++y, rowStart += width) {
            for (var x = 0; x < width; ++x) {
                var idx = rowStart + x;
                if (grid[idx] == 'X') {
                    foreach (var dir in dirs)
                        xmasCount += IsLinearMatch("MAS"u8, (x, y) + dir, dir);
                    xmasCount += x < maxXForward && Unsafe.As<byte, uint>(ref grid[idx]) == FORWARD ? 1 : 0;
                    xmasCount += x >= 3 && Unsafe.As<byte, uint>(ref grid[idx - 3]) == BACKWARD ? 1 : 0;
                }
            }
        }

        Console.WriteLine($"The total XMAS count is: {xmasCount}");
        return xmasCount;
    }

    public override object? Part2() {
        var crossMasCount = 0;
        var (maxX, maxStart) = (width - 1, grid.Length - width);
        for (var rowStart = width; rowStart < maxStart; rowStart += width) {
            var maxIdx = rowStart + maxX;
            for (var idx = rowStart + 1; idx < maxIdx; ++idx) {
                if (grid[idx] == 'A')
                    crossMasCount += IsCrossMas(idx);
            }
        }

        Console.WriteLine($"The total cross MAS count is: {crossMasCount}");
        return crossMasCount;
    }
}
