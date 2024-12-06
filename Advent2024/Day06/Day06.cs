using System.Runtime.CompilerServices;

namespace Advent2024.Day06;

using Vec2D = Vector2D<int>;

internal class Day06 : DayBase {
    private unsafe struct Map {
        [InlineArray(130)]
        public struct Rows {
            public unsafe struct Row { public fixed uint cells[5]; }
            public Row first;
        }

        public Rows rows;

        public bool this[Vec2D vec] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this[vec.x, vec.y];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[vec.x, vec.y] = value;
        }

        public bool this[int x, int y] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (rows[y].cells[x >>> 5] & 1U << x) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => rows[y].cells[x >>> 5] |= Convert.ToUInt32(value) << x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Map(StreamReader reader, int width, ref Vec2D startPos) {
            var cellBlockCount = (width >>> 5) + 1;
            for (var y = 0; !reader.EndOfStream; ++y) {
                ref var row = ref rows[y];
                var x = 0;
                for (var i = 0; i < cellBlockCount; ++i) {
                    var ub = Math.Min(x + 32, width);
                    for (ref var currCells = ref row.cells[i]; x < ub; ++x) {
                        var curr = reader.Read();
                        if (curr == '^')
                            startPos = new(x, y);
                        currCells |= ((uint)curr & 1) << x;
                    }
                }
                reader.ConsumeFullNewLine(reader.Read());
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

    private readonly int width;
    private readonly int height;
    private readonly Map map;
    private readonly Vec2D startPos;
    private readonly (Vec2D pos, Direction dir)[] path = new(Vec2D pos, Direction dir)[7_300];
    private readonly int pathLen = 0;
    private readonly int uniquePathCount = 0;

    public Day06() {
        using var reader = GetDataReader();
        (width, height) = reader.BaseStream.GetLineInfoForRegularFile();
        map = new(reader, width, ref startPos);
        (pathLen, uniquePathCount) = TravelPath();
    }

    private (int pathLen, int uniqueCount) TravelPath() {
        Map visited = new();
        var (pos, dir, write, uniqueCount) = (startPos, Direction.North, -1, 0);
        while ((uint)pos.x < (uint)width & (uint)pos.y < (uint)height) {
            if (map[pos]) {
                pos -= dir.ToDirVector<int>(false);
                dir = dir.RightOf();
                path[write] = (pos, dir);
            }
            
            path[++write] = (pos, dir);
            uniqueCount += (int)visited.Add(pos);

            pos += dir.ToDirVector<int>(false);
        }

        return (write + 1, uniqueCount);
    }

    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The number of visited cells is: {uniquePathCount}");
        return uniquePathCount;
    }

    public override object? Part2(bool print = true) {
        var count = 0;
        HashSet<Vec2D> blockages = [];
        for (var i = 1; i < pathLen; ++i) {
            var (pos, dir) = path[i];
            var blockage = pos + dir.ToDirVector<int>(false);
            if ((uint)blockage.x >= (uint)width | (uint)blockage.y >= (uint)height || !blockages.Add(blockage))
                continue;
            count += IsCycle(pos, blockage, dir.RightOf());
        }

        if (print)
            Console.WriteLine($"The number of possible loop locations is: {count}");
        return count;
    }

    private int IsCycle(Vec2D pos, Vec2D blockage, Direction dir) {
        HashSet<(Vec2D pos, Direction dir)> visited = [(pos, dir)];
        while (true) {
            var next = pos + dir.ToDirVector<int>(false);
            if ((uint)next.x >= (uint)width | (uint)next.y >= (uint)height)
                return 0;
            
            if (map[next] | next == blockage) {
                dir = dir.RightOf();
                next = pos;
            }

            if (!visited.Add((next, dir)))
                return 1;

            pos = next;
        }
    }
}
