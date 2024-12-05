using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Advent2024.Day05;

internal class Day05 : DayBase {
    private readonly ulong[] adjacency = new ulong[200];
    private readonly List<byte[]> updates = new(190);

    public Day05() {
        using var reader = GetDataReader();
        var newLine = Environment.NewLine[0];
        while (reader.Peek() != newLine) {
            var left = reader.ReadNextInt();
            var right = reader.ReadNextInt();
            adjacency[(left << 1) + (right >>> 6)] |= 1UL << right;
        }

        reader.ReadLine();

        Span<byte> buffer = stackalloc byte[23];
        while (!reader.EndOfStream) {
            var span = reader.ReadAllNumbersInLine(buffer);
            updates.Add(span.ToArray());
        }
    }

    public override object? Part1() {
        Span<ulong> currentSet = stackalloc ulong[2];
        var sum = 0;
        foreach (var update in updates) {
            (currentSet[0], currentSet[1]) = (0, 0);
            for (var i = update.Length - 1; i >= 0; --i) {
                var curr = update[i];

                var offset = curr << 1;
                if ((adjacency[offset] & currentSet[0]) != currentSet[0])
                    goto skip;
                if ((adjacency[offset + 1] & currentSet[1]) != currentSet[1])
                    goto skip;

                currentSet[curr >>> 6] |= 1UL << curr;
            }

            sum += update[update.Length >>> 1];
        skip:;
        }

        Console.WriteLine($"The sum of sorted center values is {sum}");
        return sum;
    }

    public override object? Part2() {
        Span<ulong> currentSet = stackalloc ulong[2];
        Span<ulong> prereqs = stackalloc ulong[2];
        var sum = 0;
        foreach (var update in updates) {
            (currentSet[0], currentSet[1]) = (0, 0);
            (prereqs[0], prereqs[1]) = (0, 0);
            var isOrdered = true;
            var midIdx = update.Length >>> 1;

            for (var i = update.Length - 1; i >= 0; --i) {
                var curr = update[i];

                var offset = curr << 1;
                isOrdered &= (adjacency[offset] & currentSet[0]) == currentSet[0];
                isOrdered &= (adjacency[offset + 1] & currentSet[1]) == currentSet[1];

                prereqs[0] |= adjacency[offset];
                prereqs[1] |= adjacency[offset + 1];

                currentSet[curr >>> 6] |= 1UL << curr;
            }
            if (isOrdered)
                continue;

            prereqs[0] &= currentSet[0];
            prereqs[1] &= currentSet[1];
            sum += GetSortedMid(currentSet, prereqs, update);
        skip:;
        }

        Console.WriteLine($"The sum of unsorted center values is {sum}");
        return sum;
    }

    private void BuildPrereqs(Span<ulong> set, Span<ulong> prereqs) {
        (prereqs[0], prereqs[1]) = (0, 0);
        for (var offset = 0; offset <= 64; offset += 64) {
            foreach (var bit in new BitEnumerable<ulong>(set[offset >>> 6])) {
                var n = offset | BitOperations.TrailingZeroCount(bit);
                var adjOffset = n << 1;
                prereqs[0] |= adjacency[adjOffset];
                prereqs[1] |= adjacency[adjOffset + 1];
            }
        }
        prereqs[0] &= set[0];
        prereqs[1] &= set[1];
    }

    private int GetSortedMid(Span<ulong> set, Span<ulong> prereqs, byte[] update) {
        var next = -1;
        var midIdx = update.Length >>> 1;
        for (var i = 0; i <= midIdx; ++i) {
            for (var offset = 0; offset <= 64; offset += 64) {
                var idx = offset >>> 6;
                var diff = set[idx] ^ prereqs[idx];
                if (diff != 0) {
                    set[idx] ^= diff;
                    next = offset | BitOperations.TrailingZeroCount(diff);
                    BuildPrereqs(set, prereqs);
                    goto skip;
                }
            }
        skip:;
        }
        return next;
    }
}
