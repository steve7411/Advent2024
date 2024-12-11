using System.Runtime.InteropServices;

using IntType = long;
using FloatType = double;
using System.Runtime.CompilerServices;

namespace Advent2024.Day11;

internal class Day11 : DayBase {
    private static ReadOnlySpan<IntType> powTens => [1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000, 100_000_000_000, 1_000_000_000_000, 10_000_000_000_000];

    private readonly IntType[] nums;
    private readonly Dictionary<IntType, IntType>[] memos = new Dictionary<IntType, IntType>[76];

    public Day11() {
        GC.TryStartNoGCRegion(1024L * 10_000);
        using var reader = GetDataReader();
        Span<IntType> buffer = stackalloc IntType[10];
        var span = reader.ReadAllNumbersInLine(buffer);
        nums = span.ToArray();
        for (var i = 1; i < memos.Length; ++i)
            memos[i] = new(3788);
    }

    private IntType Simulate(IntType n, int iterations) {
        if (iterations == 0)
            return 1;

        ref var cached = ref CollectionsMarshal.GetValueRefOrAddDefault(memos[iterations], n, out _);
        if (cached != 0)
            return cached;

        if (n == 0)
            return cached = Simulate(1, iterations - 1);

        var digitCount = DigitCount(n);
        if ((digitCount & 1) == 0) {
            var half = digitCount >>> 1;
            var pow = powTens[(int)half];
            var (l, r) = IntType.DivRem(n, pow);
            return cached = Simulate(l, iterations - 1) + Simulate(r, iterations - 1);
        }
        return cached = Simulate(n * 2024, iterations - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IntType DigitCount(IntType n) {
        return n switch {
            < 10 => 1,
            < 100 => 2,
            < 1_000 => 3,
            < 10_000 => 4,
            < 100_000 => 5,
            < 1_000_000 => 6,
            < 10_000_000 => 7,
            < 100_000_000 => 8,
            < 1_000_000_000 => 9,
            < 10_000_000_000 => 10,
            < 100_000_000_000 => 11,
            < 1_000_000_000_000 => 12,
            < 10_000_000_000_000 => 13,
            _ => 14,
        };
    }

    private IntType RunSimulations(int simulationCount) {
        IntType count = 0;
        foreach (var n in nums)
            count += Simulate(n, simulationCount);
        return count;
    }

    public override object? Part1(bool print = true) {
        const int ITERS = 25;
        var count = RunSimulations(ITERS);

        if (print)
            Console.WriteLine($"The count after {ITERS} iterations is: {count}");
        return count;
    }

    public override object? Part2(bool print = true) {
        const int ITERS = 75;
        var count = RunSimulations(ITERS);
        GC.EndNoGCRegion();
        if (print)
            Console.WriteLine($"The count after {ITERS} iterations is: {count}");
        return count;
    }
}
