using BenchmarkDotNet.Attributes;
using static Advent2024.DayRunner;

namespace Advent2024Benchmarks;

[MemoryDiagnoser]
public class DayBenchmarks {
    [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
    public int day;

    [Benchmark]
    public void Day() {
        RunDayNoPrint(day);
    }
}
