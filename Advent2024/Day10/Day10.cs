using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Advent2024.Day10;

internal class Day10 : DayBase {
    private readonly int width;
    private readonly int height;
    private readonly int scoreSum;
    private readonly int ratingSum;

    public Day10() {
        using var stream = GetDataStream();
        (width, height) = stream.GetLineInfoForRegularFile();
        var grid = GC.AllocateUninitializedArray<byte>(width * height);
        Span<byte> newLineDump = stackalloc byte[Environment.NewLine.Length];
        Span<byte> span = grid;
        while (stream.Position < stream.Length) {
            _ = stream.Read(span[..width]);
            _ = stream.Read(newLineDump);
            span = span[width..];
        }
        (scoreSum, ratingSum) = Avx2.IsSupported ? CalculateScoresAndRatingsAvx2(grid) : CalculateScoresAndRatingsSerial(grid, 0, true);
    }

    private unsafe (int scores, int ratings) CalculateScoresAndRatingsAvx2(byte[] grid) {
        var resultAvx = (score: 0, rating: 0);
        void doAvx() {
            Parallel.For(0, grid.Length >>> 5, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, () => (set: new ulong[25], score: 0, rating: 0), (batchNum, _, state) => {
                var zero = (byte)'0';
                var zeroReg = Avx2.BroadcastScalarToVector256(&zero);
                var idx = batchNum << 5;
                var reg = Avx2.CompareEqual(Unsafe.ReadUnaligned<Vector256<byte>>(ref grid[idx]), zeroReg);
                var bits = Avx2.MoveMask(reg);
                var sums = (score: 0, rating: 0);
                foreach (var bit in new BitEnumerable<int>(bits)) {
                    var i = idx + BitOperations.TrailingZeroCount((uint)bit);
                    var (y, x) = Math.DivRem(i, width);
                    Array.Clear(state.set);
                    sums = sums.Add(CalculateScoreAndRating(grid, x, y, '0', state.set));
                }
                return (state.set, state.score + sums.score, state.rating + sums.rating);
            }, s => { Interlocked.Add(ref resultAvx.score, s.score); Interlocked.Add(ref resultAvx.rating, s.rating); });
        }

        var resultSerial = (score: 0, rating: 0);
        void doSerial() => resultSerial = CalculateScoresAndRatingsSerial(grid, (grid.Length >>> 5) * Vector256<byte>.Count);
        Parallel.Invoke(doAvx, doSerial);
        return resultAvx.Add(resultSerial);
    }

    private (int score, int rating) CalculateScoresAndRatingsSerial(byte[] grid, int start, bool parallel = false) {
        var result = (score: 0, rating: 0);
        if (parallel) {
            Parallel.For(start, grid.Length, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, () => (set: new ulong[25], score: 0, rating: 0), (i, _, state) => {
                var (y, x) = Math.DivRem(i, width);
                Array.Clear(state.set);
                var (score, rating) = CalculateScoreAndRating(grid, x, y, '0', state.set);
                return (state.set, state.score + score, state.rating + rating);
            }, s => { Interlocked.Add(ref result.score, s.score); Interlocked.Add(ref result.rating, s.rating); });
            return result;
        }

        Span<ulong> set = stackalloc ulong[25];
        for (var i = start; i < grid.Length; ++i) {
            set.Clear();
            var (y, x) = Math.DivRem(i, width);
            result = result.Add(CalculateScoreAndRating(grid, x, y, '0', set));
        }
        return result;
    }

    private (int score, int rating) CalculateScoreAndRating(byte[] grid, int x, int y, int val, Span<ulong> set) {
        if ((uint)x >= (uint)width | (uint)y >= (uint)height)
            return default;

        var idx = width * y + x;
        ref var cell = ref grid[idx];
        if (cell != val)
            return default;

        ref var bits = ref set[idx >>> 6];
        var isNew = Convert.ToInt32(bits != (bits |= 1UL << idx));
        if (cell == '9')
            return (isNew, 1);

        var next = val + 1;
        var res = CalculateScoreAndRating(grid, x, y - 1, next, set)
            .Add(CalculateScoreAndRating(grid, x - 1, y, next, set))
            .Add(CalculateScoreAndRating(grid, x + 1, y, next, set))
            .Add(CalculateScoreAndRating(grid, x, y + 1, next, set));
        return (-isNew & res.x, res.y);
    }

    [SkipLocalsInit]
    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The sum of the scores of all trailheads is: {scoreSum}");
        return scoreSum;
    }

    [SkipLocalsInit]
    public override object? Part2(bool print = true) {
        if (print)
            Console.WriteLine($"The sum of the ratings of all trailheads is: {ratingSum}");
        return ratingSum;
    }
}
