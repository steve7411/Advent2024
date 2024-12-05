namespace Advent2024.Day03;

internal class Day03 : DayBase {
    private const uint MUL = 0x6D_75_6C_28;
    private const uint DO = 0x64_6F_28_29;
    private const ulong DONT = 0x64_6F_6E_27_74_28_29;

    private const ulong SEVEN_MASK = 0xFF_FF_FF_FF_FF_FF_FF;

    private readonly long[] sumOfProducts = [0, 0];

    public Day03() {
        using var reader = GetDataReader();
        var (recents, enabled) = (0UL, 1);
        while (!reader.EndOfStream) {
            recents = recents << 8 | (uint)reader.Read();
            if ((uint)recents == MUL)
                sumOfProducts[enabled] += AttemptMulOp(reader);
            else if ((uint)recents == DO)
                enabled = 1;
            else if ((recents & SEVEN_MASK) == DONT)
                enabled = 0;
        }
    }

    private static long AttemptMulOp(StreamReader reader) {
        var (l, comma) = reader.ReadNextNumber<long>();
        if (comma != ',')
            return 0;
        var (r, close) = reader.ReadNextNumber<long>();
        if (close != ')')
            return 0;
        return l * r;
    }

    public override object? Part1(bool print = true) {
        var total = sumOfProducts[0] + sumOfProducts[1];
        if (print)
            Console.WriteLine($"The sum of all products is: {total}");
        return total;
    }

    public override object? Part2(bool print = true) {
        if (print)
            Console.WriteLine($"The sum of enabled products is: {sumOfProducts[1]}");
        return sumOfProducts[1];
    }
}
