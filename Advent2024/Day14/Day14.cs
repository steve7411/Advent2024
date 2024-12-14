//#define USE_TEST_DIMS

using System.Runtime.CompilerServices;
using System.Text;
using Num = int;
using Vec2D = Advent2024.Vector2D<int>;

namespace Advent2024.Day14;


internal class Day14 : DayBase {
#if USE_TEST_DIMS
    const Num WIDTH = 11;
    const Num HEIGHT = 7;
#else
    const Num WIDTH = 101;
    const Num HEIGHT = 103;
#endif


    private readonly List<(Vec2D pos, Vec2D vel)> robots = new(500);

    public Day14() {
        using var reader = GetDataReader();

        while (!reader.EndOfStream) {
            reader.SkipUntil('=');
            var pos = ReadVec(reader);
            reader.SkipUntil('=');
            var vel = ReadVec(reader);
            robots.Add((pos, vel));
        }
    }

    private static Vec2D ReadVec(StreamReader reader) {
        var x = reader.ReadNextNumber<Num>().parsed;
        var y = reader.ReadNextNumber<Num>().parsed;
        return new(x, y);
    }

    private static int GetQuadrant(Vec2D pos) {
        return pos switch {
            ( > 0, > 0) => 0,
            ( > 0, < 0) => 1,
            ( < 0, > 0) => 2,
            ( < 0, < 0) => 3,
            _ => 4,
        };
    }

    private string GetString(Num time) {
        StringBuilder sb = new(WIDTH * HEIGHT + HEIGHT * Environment.NewLine.Length);
        return string.Create(WIDTH * HEIGHT + HEIGHT * Environment.NewLine.Length, (time, robots), static (span, state) => {
            var (time, robots) = state;
            span.Fill(' ');
            var lineLen = WIDTH + Environment.NewLine.Length;
            for (var i = 0; i < HEIGHT; ++i)
                Environment.NewLine.CopyTo(span[(i * lineLen + WIDTH)..]);

            Vec2D dims = new(WIDTH, HEIGHT);
            foreach (var r in robots) {
                var pos = ((r.pos + r.vel * time) % dims + dims) % dims;
                span[pos.y * lineLen + pos.x] = '#';
            }
        });
    }

    public override object? Part1(bool print = true) {
        const Num DURATION = 100;

        Span<Num> quadrants = stackalloc Num[5];
        Vec2D dims = new(WIDTH, HEIGHT);
        Vec2D center = dims / 2;
        foreach (var r in robots) {
            var pos = ((r.pos + r.vel * DURATION) % dims + dims) % dims;
            ++quadrants[GetQuadrant(pos - center)];
        }

        Num safetyFactor = quadrants[0] * quadrants[1] * quadrants[2] * quadrants[3];
        if (print)
            Console.WriteLine($"The safety factor is: {safetyFactor}");
        return safetyFactor;
    }

    public override object? Part2(bool print = true) {
        const Num TILES = WIDTH * HEIGHT;
        const Num MAX_ITERS = 10_000;

        Vec2D dims = new(WIDTH, HEIGHT);
        const Num SET_LEN = (TILES >>> 6) + (-(TILES & 63) >>> 63);
        Span<ulong> set = stackalloc ulong[SET_LEN];

        Num seconds = Num.MaxValue;
        Parallel.For(6_000, MAX_ITERS, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, () => GC.AllocateUninitializedArray<ulong>(SET_LEN), (i, loopState, set) => {
            Array.Clear(set);
            Num count = 0;
            foreach (var r in robots) {
                var pos = ((r.pos + r.vel * i) % dims + dims) % dims;
                var idx = pos.y * WIDTH + pos.x;
                ref var bits = ref set[idx >>> 6];
                count += bits != (bits |= 1UL << idx) ? 1 : 0;
            }

            if (count == robots.Count) {
                if (i < seconds)
                    seconds = i;
                loopState.Break();
            }

            return set;
        }, s => { });

        //Console.WriteLine(GetString(seconds));

        if (print)
            Console.WriteLine($"The minimum number of seconds is: {seconds}");
        return seconds;
    }
}

