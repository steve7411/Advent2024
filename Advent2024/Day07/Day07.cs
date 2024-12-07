using Num = long;

namespace Advent2024.Day07;

internal sealed class Day07 : DayBase {
    private readonly Num possibleOperandSum = 0;
    private readonly Num possibleOperandSumConcat = 0;

    public Day07() {
        using var reader = GetDataReader();

        Span<(Num n, Num powTen)> buffer = stackalloc (Num n, Num powTen)[12];
        while (!reader.EndOfStream) {
            var target = reader.ReadNextNumber<Num>().parsed;
            var operands = ReadLine(reader, buffer);
            if (Search(target, operands))
                possibleOperandSum += target;
            else if (SearchWithConcat(target, operands))
                possibleOperandSumConcat += target;
        }
    }

    private static Span<(Num n, Num powTen)> ReadLine(StreamReader r, Span<(Num n, Num powTen)> buffer) {
        (Num parsed, int len, int lastRead) parseResult;
        ReadOnlySpan<Num> powTens = [1, 10, 100, 1000];
        var idx = -1;
        while ((parseResult = r.ReadNextNumberWithLen<Num>()).lastRead is not (-1 or '\r' or '\n'))
            buffer[++idx] = (parseResult.parsed, powTens[parseResult.len]);
        buffer[++idx] = (parseResult.parsed, powTens[parseResult.len]);
        r.ConsumeFullNewLine(parseResult.lastRead);
        return buffer[..(idx + 1)];
    }

    private static bool Search(Num target, Span<(Num n, Num powTen)> ops) {
        if (target == 0)
            return true;

        if (ops.Length == 0)
            return false;

        var next = ops[^1].n;

        if (target < next)
            return false;

        ops = ops[..^1];

        if (Search(target - next, ops))
            return true;

        var (q, r) = Num.DivRem(target, next);
        return r == 0 && Search(q, ops);
    }

    private static bool SearchWithConcat(Num target, Span<(Num n, Num powTen)> ops) {
        if (target == 0)
            return true;

        if (target < 0 | ops.Length == 0)
            return false;

        var (next, powTen) = ops[^1];

        if (target < next)
            return false;
        
        ops = ops[..^1];

        var delta = target - next;
        var (q, r) = Num.DivRem(target, next);
        if (r == 0 && SearchWithConcat(q, ops) || SearchWithConcat(delta, ops))
            return true;
        
        (q, r) = Num.DivRem(delta, powTen);
        return r == 0 && SearchWithConcat(q, ops);
    }

    public override object? Part1(bool print = true) {
        if (print)
            Console.WriteLine($"The sum of possible target values is: {possibleOperandSum}");
        return possibleOperandSum;
    }

    public override object? Part2(bool print = true) {
        var total = possibleOperandSum + possibleOperandSumConcat;
        if (print)
            Console.WriteLine($"The sum of possible target values with concat is: {total}");
        return total;
    }
}
