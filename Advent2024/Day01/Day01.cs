namespace Advent2024.Day01;

internal class Day01 : DayBase {
    private readonly int[] left = new int[1000];
    private readonly int[] right = new int[1000];
    public Day01() {
        using var reader = GetDataReader();
        for (var write = 0; !reader.EndOfStream; ++write) {
            left[write] = reader.ReadNextInt();
            right[write] = reader.ReadNextInt();
        }
        Array.Sort(left);
        Array.Sort(right);
    }

    public override object? Part1() {
        var diff = 0L;
        for (var i = 0; i < left.Length; ++i)
            diff += Math.Abs(left[i] - right[i]);
        Console.WriteLine($"The total difference is: {diff}");
        return diff;
    }

    public override object? Part2() {
        var score = 0L;
        for (int lIdx = 0, rIdx = 0; lIdx < left.Length & rIdx < right.Length;) {
            var (l, r) = (left[lIdx], right[rIdx]);
            if (l < r) {
                ++lIdx;
            } else {
                score += l & ~-(l ^ r) >> 31;
                ++rIdx;
            }
        }
        Console.WriteLine($"The total similarity score is: {score}");
        return score;
    }
}
