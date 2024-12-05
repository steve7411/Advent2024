using System.Numerics;

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

    private int GetSortedMid(Span<ulong> set, byte[] update) {
        Span<ushort> counts = stackalloc ushort[100];
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
        var diff0 = set[0] ^ allDescendendents[0] & set[0];
        var next = diff0 == 0 ? 64 | BitOperations.TrailingZeroCount(set[1] ^ allDescendendents[1] & set[1]) : BitOperations.TrailingZeroCount(diff0);
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
        Span<ulong> set = stackalloc ulong[2];
        var sum = 0;
        foreach (var update in updates) {
            (set[0], set[1]) = (0, 0);
            for (var i = update.Length - 1; i >= 0; --i) {
                var curr = update[i];
                var offset = curr << 1;
                if ((adjacency[offset] & set[0]) != set[0] || (adjacency[offset + 1] & set[1]) != set[1])
                    goto skip;
                set[curr >>> 6] |= 1UL << curr;
            }
            sum += update[update.Length >>> 1];
        skip:;
        }

        Console.WriteLine($"The sum of sorted center values is {sum}");
        return sum;
    }

    public override object? Part2() {
        Span<ulong> currentSet = stackalloc ulong[2];
        var sum = 0;
        foreach (var update in updates) {
            (currentSet[0], currentSet[1]) = (0, 0);
            var isOrdered = true;
            var midIdx = update.Length >>> 1;

            for (var i = update.Length - 1; i >= 0; --i) {
                var curr = update[i];
                var offset = curr << 1;
                isOrdered &= (adjacency[offset] & currentSet[0]) == currentSet[0];
                isOrdered &= (adjacency[offset + 1] & currentSet[1]) == currentSet[1];
                currentSet[curr >>> 6] |= 1UL << curr;
            }

            if (!isOrdered)
                sum += GetSortedMid(currentSet, update);
        }

        Console.WriteLine($"The sum of unsorted center values is {sum}");
        return sum;
    }
}
