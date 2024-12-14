using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Num = long;

namespace Advent2024.Day12;

internal class Day12 : DayBase {
    private unsafe struct RowBits {
        public fixed ulong cells[4];

        public ulong this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (cells[index >>> 6] >>> index) & 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => cells[index >>> 6] |= value << index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void XorInto(ref RowBits other, ref RowBits dest) {
            if (Vector256.IsHardwareAccelerated && Vector256<ulong>.IsSupported) {
                fixed (ulong* lCells = cells, rCells = other.cells, dCells = dest.cells)
                    (Vector256.Load(lCells) ^ Vector256.Load(rCells)).Store(dCells);
            } else if (Vector.IsHardwareAccelerated && Vector<ulong>.IsSupported) {
                var count = Vector<ulong>.Count;
                fixed (ulong* lCells = cells, rCells = other.cells, dCells = dest.cells) {
                    ulong* l = lCells, r = rCells, d = dCells;
                    for (int i = 0, ub = 4 >>> count - 1; i < ub; ++i, l += count, r += count, d += count)
                        (Vector.Load(l) ^ Vector.Load(r)).Store(d);
                }
            } else {
                // The fourth entry is used as padding
                for (var i = 0; i < 3; ++i)
                    dest.cells[i] = cells[i] ^ other.cells[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AndInto(ref RowBits other, ref RowBits dest) {
            if (Vector256.IsHardwareAccelerated && Vector256<ulong>.IsSupported) {
                fixed (ulong* lCells = cells, rCells = other.cells, dCells = dest.cells)
                    (Vector256.Load(lCells) & Vector256.Load(rCells)).Store(dCells);
            } else if (Vector.IsHardwareAccelerated && Vector<ulong>.IsSupported) {
                var count = Vector<ulong>.Count;
                fixed (ulong* lCells = cells, rCells = other.cells, dCells = dest.cells) {
                    ulong* l = lCells, r = rCells, d = dCells;
                    for (int i = 0, ub = 4 >>> count - 1; i < ub; ++i, l += count, r += count, d += count)
                        (Vector.Load(l) & Vector.Load(r)).Store(d);
                }
            } else {
                for (var i = 0; i < 3; ++i)
                    dest.cells[i] = cells[i] & other.cells[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HorizontalDiffInto(ref RowBits dest) {
            if (Avx2.IsSupported) {
                fixed (ulong* cells = this.cells, dCells = dest.cells) {
                    var vec = Vector256.Load(cells);
                    var neighbors = Avx2.Permute4x64(vec, 0b10_01_00_11);
                    (vec ^ (vec << 1 | neighbors >>> 63)).Store(dCells);
                }
            } else {
                dest.cells[0] = cells[0] ^ cells[0] << 1;
                dest.cells[1] = cells[1] ^ (cells[0] >>> 63 | cells[1] << 1);
                dest.cells[2] = cells[2] ^ (cells[1] >>> 63 | cells[2] << 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtractInnerCornersInto(ref RowBits vertDiffs, ref RowBits horizDiffsAbove, ref RowBits horizDiffsBelow, ref RowBits dest) {
            if (Avx2.IsSupported) {
                fixed (ulong* vCells = vertDiffs.cells, hDACells = horizDiffsAbove.cells, hDBCells = horizDiffsBelow.cells, dCells = dest.cells) {
                    var vertVec = Vector256.Load(vCells);
                    var vertPermuteVec = Avx2.Permute4x64(vertVec, 0b10_01_00_11);
                    vertVec = (vertVec ^ (vertVec << 1 | vertPermuteVec >>> 63));

                    var horizVec = Vector256.Load(hDACells) ^ Vector256.Load(hDBCells);
                    (horizVec & vertVec).Store(dCells);
                }
            } else {
                RowBits horiz = new();
                RowBits vert = new();
                horizDiffsAbove.XorInto(ref horizDiffsBelow, ref horiz);
                vertDiffs.HorizontalDiffInto(ref vert);
                horiz.AndInto(ref vert, ref dest);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetBits(int index, uint mask = 3) {
            // Only works on little endian
            var byteOffset = index >>> 3;
            var shift = index & 7;
            // Take the hit of unaligned reads to get the CPU to shift out
            // any bit pairs that span a 64 bit boundary
            fixed (ulong* ptr = cells)
                return *(uint*)((byte*)ptr + byteOffset) >>> shift & mask;
        }

#if DEBUG
        public override string ToString() {
            return string.Concat($"{BitReverser<ulong>.ReverseBits(cells[0]):B64}",
                $"{BitReverser<ulong>.ReverseBits(cells[1]):B64}",
                $"{BitReverser<ulong>.ReverseBits(cells[2]):B13}");
        }
#endif
    }

    private readonly int width;
    private readonly int height;
    private readonly byte[] grid;
    private readonly RowBits[] verticalDiffs;
    private readonly RowBits[] horizontalDiffs;
    private readonly RowBits[] insideCorners;
    private readonly Num normalCost;
    private readonly Num bulkCost;

    public Day12() {
        using var reader = GetDataReader();
        (width, height) = reader.BaseStream.GetLineInfoForRegularFile();

        grid = GC.AllocateUninitializedArray<byte>(width * height);
        insideCorners = GC.AllocateUninitializedArray<RowBits>(height + 1);
        verticalDiffs = GC.AllocateUninitializedArray<RowBits>(height + 1);
        horizontalDiffs = GC.AllocateUninitializedArray<RowBits>(height);

        var gridSpan = grid.AsSpan();
        while (!reader.EndOfStream) {
            var gridRow = gridSpan[..width];
            reader.Read(gridRow);
            reader.ConsumeFullNewLine(reader.Read());
            gridSpan = gridSpan[width..];
        }

        PopulateDiffs();
        PopulateInsideCorners();

        (normalCost, bulkCost) = CalculateFenceCosts();
    }

    private void PopulateInsideCorners() {
        for (var y = 1; y < height; ++y)
            RowBits.ExtractInnerCornersInto(ref verticalDiffs[y], ref horizontalDiffs[y - 1], ref horizontalDiffs[y], ref insideCorners[y]);
    }

    private void PopulateDiffs() {
        var gridSpan = grid.AsSpan();
        ref var gridStart = ref grid[0];
        ref var firstVert = ref verticalDiffs[0];
        ref var firstHoriz = ref horizontalDiffs[0];
        Span<byte> zeros = stackalloc byte[width];
        ref var prevRow = ref zeros[0];
        for (var y = 0; y < height; ++y) {
            ref var rowStart = ref Unsafe.Add(ref gridStart, y * width);
            var toLeft = 0;
            ref var vert = ref Unsafe.Add(ref firstVert, y);
            ref var horiz = ref Unsafe.Add(ref firstHoriz, y);
            for (var x = 0; x < width; ++x) {
                int curr = Unsafe.Add(ref rowStart, x);
                vert[x] = (ulong)(-(Unsafe.Add(ref prevRow, x) ^ curr) >>> 31);
                horiz[x] = (ulong)(-(toLeft ^ curr) >>> 31);
                toLeft = curr;
            }
            horiz[width] = 1;
            prevRow = ref rowStart;
        }
        verticalDiffs[height] = verticalDiffs[0];
    }

    private (Num normal, Num bulk) CalculateFenceCosts() {
        var costs = (normal: (Num)0, bulk: (Num)0);
        Span<ulong> visited = stackalloc ulong[(grid.Length >>> 6) + (-(grid.Length & 0x3F) >>> 63)];
        for (var y = 0; y < height; ++y) {
            var row = grid.AsSpan(y * width);
            for (var x = 0; x < width; ++x) {
                var (p, a, c) = Flood(x, y, row[x], visited);
                costs.normal += p * a;
                costs.bulk += c * a;
            }
        }
        return costs;
    }

    private Num GetCornerCount(int x, int y) {
        var aboveVert = verticalDiffs[y].GetBits(x, 1);
        var belowVert = verticalDiffs[y + 1].GetBits(x, 1);
        var aboveVertSpread = aboveVert | aboveVert << 1;
        var belowVertSpread = belowVert | belowVert << 1;

        var sides = horizontalDiffs[y].GetBits(x);

        var aboveInner = insideCorners[y].GetBits(x);
        var belowInner = insideCorners[y + 1].GetBits(x);
        uint aboveExterior = aboveVertSpread & sides;
        uint belowExterior = belowVertSpread & sides;
        uint aboveInterior = aboveInner & ~(aboveVertSpread | sides);
        uint belowInterior = belowInner & ~(belowVertSpread | sides);
        return BitOperations.PopCount(aboveExterior | belowExterior << 2 | aboveInterior << 4 | belowInterior << 6);
    }

    private (Num perimeter, Num area, Num corners) Flood(int x, int y, byte expected, Span<ulong> visited) {
        if ((uint)x >= (uint)width | (uint)y >= (uint)height)
            return (1, 0, 0);

        var idx = y * width + x;
        if (grid[idx] != expected)
            return (1, 0, 0);

        ref var setBits = ref visited[idx >>> 6];
        var currBit = 1UL << idx;
        if ((setBits & currBit) != 0)
            return (0, 0, 0);

        setBits |= currBit;
        var cornerCount = GetCornerCount(x, y);

        var totals = Flood(x, y - 1, expected, visited)
            .Add(Flood(x - 1, y, expected, visited))
            .Add(Flood(x + 1, y, expected, visited))
            .Add(Flood(x, y + 1, expected, visited))
            .Add((0, 1, cornerCount));

        return totals;
    }

    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The cost to fence the garden: {normalCost}");
        return normalCost;
    }

    public override object? Part2(bool print = true) {
        if (print)
            Console.WriteLine($"The cost to fence the garden: {bulkCost}");
        return bulkCost;
    }
}
