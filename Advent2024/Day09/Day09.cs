using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

using Checksum = ulong;

namespace Advent2024.Day09;

internal class Day09 : DayBase {
    private readonly byte[] diskMap;

    public Day09() {
        using var stream = GetDataStream();
        var (len, _) = stream.GetRemainingCharacterCountInLine();
        diskMap = new byte[len];
        var readCount = stream.Read(diskMap);
        Debug.Assert(readCount == len);
        TruncateToDigitValue(diskMap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TruncateToDigitValue(Span<byte> bytes) {
        Vector<byte> vec;
        Vector<byte> vec2;
        Vector<byte> mask = new(0xF);
        var remaining = bytes.Length;
        ref var first = ref bytes[0];
        for (var i = 0; remaining >= Vector<byte>.Count * 2; i += Vector<byte>.Count * 2, remaining -= Vector<byte>.Count * 2) {
            ref var curr = ref Unsafe.Add(ref first, i);
            ref var second = ref Unsafe.Add(ref curr, Vector<byte>.Count);
            vec = Unsafe.ReadUnaligned<Vector<byte>>(ref curr);
            vec2 = Unsafe.ReadUnaligned<Vector<byte>>(ref second);

            Unsafe.WriteUnaligned(ref curr, vec & mask);
            Unsafe.WriteUnaligned(ref second, vec2 & mask);
        }

        for (var i = bytes.Length - remaining; i < bytes.Length; ++i)
            bytes[i] &= 0xF;
    }

    private static Checksum CalculateFragmentedChecksum(ReadOnlySpan<byte> diskMap) {
        var (freeSize, dataSize, pos) = ((Checksum)diskMap[1], (Checksum)diskMap[^1], (Checksum)diskMap[0]);
        var (left, right) = (1, diskMap.Length - 1);
        Checksum checksum = 0;
        while (left < right - 1) {
            while (freeSize == 0 & left < right - 1) {
                var next = left + 1;
                checksum += CalculateChunkChecksum(pos, diskMap[next], (Checksum)next >>> 1);
                pos += diskMap[next];
                freeSize = diskMap[left += 2];
            }

            while (dataSize == 0 & left < right - 1)
                dataSize = diskMap[right -= 2];

            var chunkSize = Math.Min(freeSize, dataSize);
            checksum += CalculateChunkChecksum(pos, chunkSize, (Checksum)right >>> 1);
            pos += chunkSize;
            dataSize -= chunkSize;
            freeSize -= chunkSize;
        }

        return checksum + CalculateChunkChecksum(pos, dataSize, (Checksum)right >>> 1);
    }

    private static Checksum CalculateDefragmentedChecksum(ReadOnlySpan<byte> diskMap) {
        var qs = new PriorityQueue<int, int>[10];
        for (var i = 1; i < 10; ++i)
            qs[i] = new(1050);
        var readPos = 0;
        for (var i = 1; i < diskMap.Length; i += 2) {
            var (fileSize, freeSize) = (diskMap[i - 1], diskMap[i]);
            readPos += fileSize + freeSize;
            if (freeSize > 0) {
                var start = readPos - freeSize;
                qs[freeSize].Enqueue(start, start);
            }
        }

        Checksum checksum = 0;
        readPos += diskMap[^1];
        for (var i = diskMap.Length - 1; i > 0; i -= 2) {
            var (freeSize, fileSize) = (diskMap[i - 1], diskMap[i]);
            readPos -= fileSize;
            var pos = GetMinPosition(qs, readPos, fileSize);
            checksum += CalculateChunkChecksum((ulong)pos, fileSize, (Checksum)i >>> 1);
            readPos -= freeSize;
        }

        return checksum + CalculateChunkChecksum(0, diskMap[0], 0);
    }

    private static int GetMinPosition(PriorityQueue<int, int>[] qs, int currentPos, int fileLen) {
        var min = currentPos;
        var minLen = 10;
        for (var i = fileLen; i < qs.Length; ++i) {
            var q = qs[i];
            int pos; bool found;
            while ((found = q.TryPeek(out pos, out _)) && pos > currentPos)
                q.Dequeue();
            if (found & pos < min)
                (min, minLen) = (pos, i);
        }

        if (minLen != 10) {
            qs[minLen].Dequeue();
            if (minLen != fileLen) {
                var newPos = min + fileLen;
                qs[minLen - fileLen].Enqueue(newPos, newPos);
            }
        }
        return min;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Checksum CalculateChunkChecksum(Checksum pos, Checksum size, Checksum id) =>
        ((size * ((pos << 1) + size - 1)) >>> 1) * id;

    public override object? Part1(bool print = true) {
        var checksum = CalculateFragmentedChecksum(diskMap);
        if (print)
            Console.WriteLine($"The fragmented checksum is: {checksum}");
        return checksum;
    }

    public override object? Part2(bool print = true) {
        var checksum = CalculateDefragmentedChecksum(diskMap);
        if (print)
            Console.WriteLine($"The non-fragmented checksum is: {checksum}");
        return checksum;
    }
}
