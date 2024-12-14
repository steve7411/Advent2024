using Vec2D = Advent2024.Vector2D<long>;
using Num = long;
using System.Reflection.PortableExecutable;
using System;

namespace Advent2024.Day13;

internal class Day13 : DayBase {
    private readonly List<(Vec2D pos, Vec2D a, Vec2D b)> machines = new(320);

    public Day13() {
        using var reader = GetDataReader();
        while (!reader.EndOfStream) {
            reader.SkipUntil('+');
            var a = ReadVec(reader);
            reader.SkipUntil('+');
            var b = ReadVec(reader);
            reader.SkipUntil('=');
            machines.Add((ReadVec(reader), a, b));
            reader.ConsumeFullNewLine(reader.Read());
        }
    }

    private static Vec2D ReadVec(StreamReader reader) {
        var x = reader.ReadNextNumber<Num>().parsed;
        reader.Read(); reader.Read(); reader.Read();
        var y = reader.ReadNextNumber<Num>().parsed;
        return new(x, y);
    }

    private static Num GetMinCost(Vec2D pos, Vec2D a, Vec2D b) {
        // Cramer's Rule
        var dn = a.x * b.y - a.y * b.x;
        Vec2D det = new(pos.x * b.y - pos.y * b.x, a.x * pos.y - a.y * pos.x);
        var mult = det / dn;
        var mod = det % dn;
        if (mod != Vec2D.Zero | (mult.x | mult.y) < 0)
            return 0;
        return mult.x * 3 + mult.y;
    }

    public override object? Part1(bool print = true) {
        var sum = 0L;
        foreach (var (pos, a, b) in machines)
            sum += GetMinCost(pos, a, b);

        if (print)
            Console.WriteLine($"The min cost to win all possible machines is: {sum}");
        return sum;
    }

    public override object? Part2(bool print = true) {
        const Num DELTA = 10_000_000_000_000;
        var sum = 0L;
        Parallel.For(0, machines.Count, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, () => (Num)0, (i, _, runningTotal) => {
            var (pos, a, b) = machines[i];
            return runningTotal + GetMinCost(pos + new Vec2D(DELTA, DELTA), a, b);
        }, s => Interlocked.Add(ref sum, s));
        
        if (print)
            Console.WriteLine($"The min adjusted cost to win all possible machines is: {sum}");
        return sum;
    }
}
