namespace Advent2024.Day02;

internal class Day02 : DayBase {
    private readonly int safeCount = 0;
    private readonly int safeCount1Removal = 0;

    public Day02() {
        using var reader = GetDataReader();
        Span<byte> buffer = stackalloc byte[8];
        while (!reader.EndOfStream) {
            var span = reader.ReadAllNumbersInLine(buffer);
            var expectedSign = (span[0] - span[1] >>> 31) + (span[1] - span[2] >>> 31) + (span[2] - span[3] >>> 31) << 30 & int.MinValue;
            var (validNoChange, failIdx) = IsValid(span, 1, span[0], expectedSign);
            safeCount += validNoChange;

            if (validNoChange == 1)
                continue;
            
            var (testIdx, delta) = failIdx == 1 ? (failIdx + 1, -1) : (failIdx, -2);
            var (isSafe1Remove, failIdx2) = IsValid(span, testIdx, span[testIdx + delta], expectedSign);
            if (failIdx + 1 >= failIdx2)
                (isSafe1Remove, _) = IsValid(span, failIdx + 1, span[failIdx - 1], expectedSign);
            safeCount1Removal += isSafe1Remove;
        }
    }

    private static (int valid, int idx) IsValid(Span<byte> span, int i, int prev, int expectedSign) {
        for (; i < span.Length; prev = span[i++]) {
            var diff = prev - span[i];
            if (diff == 0 | Math.Abs(diff) > 3 | (diff & int.MinValue) != expectedSign)
                return (0, i);
        }
        return (1, i);
    }

    public override object? Part1() {
        Console.WriteLine($"The number of safe reports is: {safeCount}");
        return safeCount;
    }

    public override object? Part2() {
        var total = safeCount + safeCount1Removal;
        Console.WriteLine($"The number of safe reports after max 1 removal is: {total}");
        return total;
    }
}
