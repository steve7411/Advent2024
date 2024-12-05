using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Advent2024.Day05;

internal class Day05 : DayBase {
    private readonly int sortedMidSum = 0;
    private readonly int unsortedMidSum = 0;

    public Day05() {
        using var reader = GetDataReader();
        var newLine = Environment.NewLine[0];
        Span<ulong> adjacency = stackalloc ulong[200];
        while (reader.Peek() != newLine) {
            var left = reader.ReadNextInt();
            var right = reader.ReadNextInt();
            adjacency[(left << 1) + (right >>> 6)] |= 1UL << right;
        }

        reader.ReadLine();

        Span<byte> buffer = stackalloc byte[23];
        Span<ulong> set = stackalloc ulong[2];
        while (!reader.EndOfStream) {
            (set[0], set[1]) = (0, 0);
            var update = reader.ReadAllNumbersInLine(buffer);
            if (IsSorted(set, update, adjacency))
                sortedMidSum += update[update.Length >>> 1];
            else
                unsortedMidSum += GetSortedMid(set, update, adjacency);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSorted(Span<ulong> set, Span<byte> update, Span<ulong> adjacency) {
        bool isSorted = true;
        for (var i = update.Length - 1; i >= 0; --i) {
            var curr = update[i];
            if (isSorted) {
                var adjSet = Vector128.LoadUnsafe(ref adjacency[curr << 1]);
                var setVec = Vector128.LoadUnsafe(ref set[0]);
                isSorted &= (adjSet & setVec) == setVec;
            }
            set[curr >>> 6] |= 1UL << curr;
        }
        return isSorted;
    }

    private int GetSortedMid(Span<ulong> set, Span<byte> update, Span<ulong> adjacency) {
        Span<byte> counts = stackalloc byte[100];
        Span<ulong> allDescendendents = stackalloc ulong[2];
        foreach (var n in update) {
            var adjOffset = n << 1;
            for (var i = 0; i < 2; ++i) {
                var bits = adjacency[adjOffset + i];
                var idxOffset = i << 6;
                foreach (var bit in new BitEnumerable<ulong>(bits & set[i]))
                    ++counts[idxOffset | BitOperations.TrailingZeroCount(bit)];
                allDescendendents[i] |= bits;
            }
        }

        var midIdx = update.Length >>> 1;
        var diff0 = set[0] & ~allDescendendents[0];
        var next = diff0 == 0 ? 64 | BitOperations.TrailingZeroCount(set[1] & ~allDescendendents[1]) : BitOperations.TrailingZeroCount(diff0);
        var curr = -1;
        for (var j = midIdx; j >= 0; --j) {
            curr = next;
            var adjOffset = curr << 1;
            for (var i = 0; i < 2; ++i) {
                var bits = adjacency[adjOffset + i];
                var idxOffset = i << 6;
                foreach (var bit in new BitEnumerable<ulong>(bits & set[i])) {
                    var idx = idxOffset | BitOperations.TrailingZeroCount(bit);
                    if (--counts[idx] == 0)
                        next = idx;
                }
                allDescendendents[i] |= bits;
            }
        }
        return curr;
    }

    public override object? Part1() {
        Console.WriteLine($"The sum of sorted center values is {sortedMidSum}");
        return sortedMidSum;
    }

    public override object? Part2() {
        Console.WriteLine($"The sum of unsorted center values is {unsortedMidSum}");
        return unsortedMidSum;
    }
}
