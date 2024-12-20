using FluentAssertions;
using System.Collections;

namespace Advent2024Tests;

public class DayOutputValidationTestData : IEnumerable<object?[]> {
    private static readonly (Type type, object? part1, object? part2)[] validationData = {
            (typeof(Advent2024.Day01.Day01), 1666427, 24316233),
            (typeof(Advent2024.Day02.Day02), 490, 536),
            (typeof(Advent2024.Day03.Day03), 196826776L, 106780429L),
            (typeof(Advent2024.Day04.Day04), 2524, 1873),
            (typeof(Advent2024.Day05.Day05), 4637, 6370),
            (typeof(Advent2024.Day06.Day06), 5564, 1976),
            (typeof(Advent2024.Day07.Day07), 5512534574980L, 328790210468594L),
            (typeof(Advent2024.Day08.Day08), 348, 1221),
            (typeof(Advent2024.Day09.Day09), 6334655979668, 6349492251099),
            (typeof(Advent2024.Day10.Day10), 489, 1086),
            (typeof(Advent2024.Day11.Day11), 182081, 216318908621637),
            (typeof(Advent2024.Day12.Day12), 1550156, 946084),
            (typeof(Advent2024.Day13.Day13), 35574, 80882098756071),
            (typeof(Advent2024.Day14.Day14), 225648864, 7847),
            //(typeof(Advent2024.Day15.Day15), default, default),
            //(typeof(Advent2024.Day16.Day16), default, default),
            //(typeof(Advent2024.Day17.Day17), default, default),
            //(typeof(Advent2024.Day18.Day18), default, default),
            //(typeof(Advent2024.Day19.Day19), default, default),
            //(typeof(Advent2024.Day20.Day20), default, default),
            //(typeof(Advent2024.Day21.Day21), default, default),
            //(typeof(Advent2024.Day22.Day22), default, default),
            //(typeof(Advent2024.Day23.Day23), default, default),
            //(typeof(Advent2024.Day24.Day24), default, default),
            //(typeof(Advent2024.Day25.Day25), default, default),
        };

    public IEnumerator<object?[]> GetEnumerator() =>
        validationData.Select(x => new object?[] { x.type, x.part1, x.part2 }).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

public class DayOutputValidationTests {
    [Theory]
    [ClassData(typeof(DayOutputValidationTestData))]
    public void ValidateDay(Type type, object? part1, object? part2) {
        var day = Activator.CreateInstance(type) as Advent2024.IDay ?? throw new Exception($"Unable to instantiate object of type {type}");
        using (new FluentAssertions.Execution.AssertionScope()) {
            day.Part1(false).Should().Be(part1);
            day.Part2(false).Should().Be(part2);
        }
    }
}