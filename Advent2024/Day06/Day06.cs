using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Advent2024.Day06;

using Vec2D = Vector2D<int>;

[InlineArray(4)]
[DebuggerDisplay("(N: {north} E: {System.Runtime.CompilerServices.Unsafe.Add(ref north, 1)} S: {System.Runtime.CompilerServices.Unsafe.Add(ref north, 2)} W: {System.Runtime.CompilerServices.Unsafe.Add(ref north, 3)})")]
internal struct AdjacencyNode { public Vec2D north; }

internal unsafe struct Map {
    [InlineArray(130)]
    public struct Rows {
        public struct Row { public fixed uint cells[5]; }
        public Row first;
    }

    public Rows rows;

    public bool this[int x, int y] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (rows[y].cells[x >>> 5] & 1U << x) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => rows[y].cells[x >>> 5] |= Convert.ToUInt32(value) << x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Map(StreamReader reader, int width, Span<AdjacencyNode> adjacencies, ref Vec2D startPos) {
        var prev = adjacencies;
        for (var i = 0; i < width; ++i)
            prev[i].north = new(i, -1);

        var cellBlockCount = (width >>> 5) + 1;
        for (var y = 0; !reader.EndOfStream; ++y) {
            ref var row = ref rows[y];
            var x = 0;
            Vec2D left = new(-1, y);
            for (var i = 0; i < cellBlockCount; ++i) {
                var ub = Math.Min(x + 32, width);
                for (ref var currCells = ref row.cells[i]; x < ub; ++x) {
                    var curr = reader.Read();

                    if (curr == '^')
                        startPos = new(x, y);

                    var bit = (uint)curr & 1;
                    currCells |= bit << x;

                    ref var adj = ref adjacencies[x];
                    if (bit == 1) {
                        adj[(int)Direction.North] = new(x, y + 1);
                        adj[(int)Direction.West] = left = new(x + 1, y);
                    } else {
                        adj[(int)Direction.North] = prev[x].north;
                        adj[(int)Direction.West] = left;
                    }
                }
            }
            reader.ConsumeFullNewLine(reader.Read());
            prev = adjacencies;
            adjacencies = adjacencies[width..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Add(Vec2D vec) {
        ref var cells = ref rows[vec.y].cells[vec.x >>> 5];
        var bit = 1U << vec.x;
        var wasNotSet = (cells & bit) >>> vec.x;
        cells |= bit;
        return wasNotSet ^ 1;
    }
}


internal class Day06 : DayBase {
    private readonly int width;
    private readonly int height;
    private readonly Map map;
    private readonly Vec2D startPos;
    private readonly (Vec2D pos, int isFirst, Direction dir)[] path = new (Vec2D pos, int isFirst, Direction dir)[6_300];
    private readonly AdjacencyNode[,] adjacencies;
    private readonly int pathLen = 0;
    private readonly int uniquePathCount = 0;

    public Day06() {
        using var reader = GetDataReader();
        (width, height) = reader.BaseStream.GetLineInfoForRegularFile();
        adjacencies = new AdjacencyNode[height, width];
        map = new(reader, width, MemoryMarshal.CreateSpan(ref adjacencies[0, 0], adjacencies.Length), ref startPos);
        PopulateEastSouthAdjacencies();
        (pathLen, uniquePathCount) = TravelPath();
    }

    private void PopulateEastSouthAdjacencies() {
        var prev = MemoryMarshal.CreateSpan(ref adjacencies[height - 1, 0], width);
        for (var i = 0; i < width; ++i)
            prev[i][(int)Direction.South] = new(i, height);

        for (var y = height - 1; y >= 0; --y) {
            Vec2D right = new(width, y);
            for (var x = width - 1; x >= 0; --x) {
                ref var adj = ref adjacencies[y, x];
                if (map[x, y]) {
                    adj[(int)Direction.South] = new(x, y - 1);
                    adj[(int)Direction.East] = right = new(x - 1, y);
                } else {
                    adj[(int)Direction.South] = prev[x][(int)Direction.South];
                    adj[(int)Direction.East] = right;
                }
            }
            prev = MemoryMarshal.CreateSpan(ref adjacencies[y, 0], width);
        }
    }

    private (int pathLen, int uniqueCount) TravelPath() {
        Map visited = new();
        var (pos, dir, write, uniqueCount) = (startPos, Direction.North, -1, 0);
        while ((uint)pos.x < (uint)width & (uint)pos.y < (uint)height) {
            if (map[pos.x, pos.y]) {
                pos -= dir.ToDirVector<int>(false);
                path[write].dir = dir = dir.RightOf();
            } else {
                var isNew = (int)visited.Add(pos);
                path[++write] = (pos, isNew, dir);
                uniqueCount += isNew;
            }
            pos += dir.ToDirVector<int>(false);
        }
        return (write + 1, uniqueCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FallsBetween(Vec2D point1, Vec2D point2, Vec2D test, int dim) =>
        point1[dim ^ 1] == test[dim ^ 1] && Math.Sign(point1[dim] - test[dim]) != Math.Sign(point2[dim] - test[dim]);

    private int IsCycle(Vec2D pos, Vec2D blockage, Direction dir, HashSet<(Vec2D pos, Direction dir)> visited) {
        visited.Add((pos, dir));
        dir = dir.RightOf();
        while (true) {
            var next = adjacencies[pos.y, pos.x][(int)dir];
            if (FallsBetween(pos, next, blockage, ~(int)dir & 1))
                next = blockage - dir.ToDirVector<int>(false);

            if ((uint)next.x >= (uint)width | (uint)next.y >= (uint)height)
                return 0;

            if (!visited.Add((next, dir)))
                return 1;

            dir = dir.RightOf();
            pos = next;
        }
    }

    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The number of visited cells is: {uniquePathCount}");
        return uniquePathCount;
    }

    public unsafe override object? Part2(bool print = true) {
        var count = 0;
        Parallel.For(0, pathLen - 1, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, () => (total: 0, set: new HashSet<(Vec2D, Direction)>()), (i, _, state) => {
            var (pos, _, dir) = path[i];
            var (blockage, isFirst, _) = path[i + 1];

            var newTotal = state.total + (isFirst == 0 ? 0 : IsCycle(pos, blockage, dir, state.set));
            state.set.Clear();
            return (newTotal, state.set);
        }, s => Interlocked.Add(ref count, s.total));

        if (print)
            Console.WriteLine($"The number of possible loop locations is: {count}");
        return count;
    }
}
