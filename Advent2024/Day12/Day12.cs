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
            get => cells[index >>> 6] >>> index & 1;

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
        public uint GetBitPair(int index) {
            // Only works on little endian
            var byteOffset = index >>> 3;
            var shift = index & 7;
            // Take the hit of unaligned reads to get the CPU to shift out
            // any bit pairs that span a 64 bit boundary
            fixed (ulong* ptr = cells)
                return *(uint*)((byte*)ptr + byteOffset) >>> shift & 3;
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
        Span<ulong> visitedSpan = stackalloc ulong[(grid.Length >>> 6) + (-(grid.Length & 0x3F) >>> 63)];
        ref var visited = ref visitedSpan[0];
        for (var y = 0; y < height; ++y) {
            var row = grid.AsSpan(y * width);
            var prev = 0;
            for (var x = 0; x < width; ++x) {
                int curr = row[x];
                if (prev != curr) {
                    var (p, a, c) = ((Num)0, (Num)0, (Num)0);
                    Flood(x, y, ref visited, ref p, ref a, ref c);
                    costs.normal += p * a;
                    costs.bulk += c * a;
                }
                prev = curr;
            }
        }
        return costs;
    }

    private (Num corners, ulong edges) GetCornerCountAndEdges(int x, int y) {
        var aboveVert = verticalDiffs[y][x];
        var belowVert = verticalDiffs[y + 1][x];
        var aboveVertSpread = aboveVert | aboveVert << 1;
        var belowVertSpread = belowVert | belowVert << 1;

        var sides = horizontalDiffs[y].GetBitPair(x);

        var aboveInsideBits = insideCorners[y].GetBitPair(x);
        var belowInsideBits = insideCorners[y + 1].GetBitPair(x);
        var aboveExterior = aboveVertSpread & sides;
        var belowExterior = belowVertSpread & sides;
        var aboveInterior = aboveInsideBits & ~(aboveVertSpread | sides);
        var belowInterior = belowInsideBits & ~(belowVertSpread | sides);
        return (BitOperations.PopCount(aboveExterior | aboveInterior | (belowInterior | belowExterior) << 2), aboveVert | belowVert << 1 | sides << 2);
    }

    private unsafe void Flood(int x, int y, ref ulong visited, ref Num perim, ref Num area, ref Num corners) {
        var idx = y * width + x;

        ref var setBits = ref Unsafe.Add(ref visited, idx >>> 6);
        var currBit = 1UL << idx;
        if ((setBits & currBit) != 0)
            return;

        setBits |= currBit;
        var (cornerCount, edges) = GetCornerCountAndEdges(x, y);

        corners += cornerCount;
        ++area;
        perim += BitOperations.PopCount(edges);
        if ((edges & 1) == 0)
            Flood(x, y - 1, ref visited, ref perim, ref area, ref corners);
        if ((edges & 4) == 0)
            Flood(x - 1, y, ref visited, ref perim, ref area, ref corners);
        if ((edges & 8) == 0)
            Flood(x + 1, y, ref visited, ref perim, ref area, ref corners);
        if ((edges & 2) == 0)
            Flood(x, y + 1, ref visited, ref perim, ref area, ref corners);
    }

    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The cost to fence the garden: {normalCost}");
        return normalCost;
    }

    public override object? Part2(bool print = true) {
        if (print)
            Console.WriteLine($"The cost to fence the garden with bulk pricing: {bulkCost}");
        return bulkCost;
    }
}
