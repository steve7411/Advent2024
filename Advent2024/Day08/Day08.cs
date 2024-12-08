using System.Runtime.CompilerServices;

using Vec2D = Advent2024.Vector2D<int>;
using Ray = Advent2024.Ray2D<int>;

namespace Advent2024.Day08;

internal sealed class Day08 : DayBase {
    private readonly int width;
    private readonly int height;

    private readonly List<Ray> rays = new(285);

    [SkipLocalsInit]
    public Day08() {
        Span<byte> indexLookup = stackalloc byte[123];
        indexLookup.Fill(255);

        Span<Vec2D> positions = stackalloc Vec2D[200];
        Span<byte> lens = stackalloc byte[50];
        lens.Clear();
        byte positionsLength = 0;

        using var reader = GetDataReader();
        (width, height) = reader.BaseStream.GetLineInfoForRegularFile();
        Span<byte> buffer = stackalloc byte[width];
        for (var y = 0; y < height; ++y) {
            reader.Read(buffer);
            reader.ConsumeFullNewLine(reader.Read());
            for (var x = 0; x < width; ++x) {
                var curr = buffer[x];
                if (curr == '.')
                    continue;

                ref var idx = ref indexLookup[curr];
                if (idx == 255)
                    idx = positionsLength++;

                ref var len = ref lens[idx];
                Vec2D pos = new(x, y);
                for (int i = idx << 2, ub = i + len; i < ub; ++i)
                    rays.Add(new(pos, positions[i] - pos));
                positions[(idx << 2) + len++] = pos;
            }
        }
    }

    public override object? Part1(bool print = true) {
        Span<ulong> set = stackalloc ulong[height];

        var count = 0;
        foreach (var ray in rays) {
            var a1 = ray.origin - ray.direction;
            if ((uint)a1.x < (uint)width & (uint)a1.y < (uint)height) {
                ref var row = ref set[a1.y];
                count += Convert.ToInt32(row != (row |= 1UL << a1.x));
            }

            var a2 = ray.origin + ray.direction * 2;
            if ((uint)a2.x < (uint)width & (uint)a2.y < (uint)height) {
                ref var row = ref set[a2.y];
                count += Convert.ToInt32(row != (row |= 1UL << a2.x));
            }
        }

        if (print)
            Console.WriteLine($"The number of antinodes is: {count}");
        return count;
    }

    public override object? Part2(bool print = true) {
        Span<ulong> set = stackalloc ulong[height];

        var count = 0;
        foreach (var ray in rays) {
            for (var p = ray.origin; (uint)p.x < (uint)width & (uint)p.y < (uint)height; p += ray.direction) {
                ref var row = ref set[p.y];
                count += Convert.ToInt32(row != (row |= 1UL << p.x));
            }
            for (var p = ray.origin - ray.direction; (uint)p.x < (uint)width & (uint)p.y < (uint)height; p -= ray.direction) {
                ref var row = ref set[p.y];
                count += Convert.ToInt32(row != (row |= 1UL << p.x));
            }
        }

        if (print)
            Console.WriteLine($"The number of antinodes with harmonics is: {count}");
        return count;
    }
}
