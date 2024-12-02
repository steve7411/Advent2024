using FluentAssertions;
using System.Collections;

namespace Advent2024Tests;

public class DayOutputValidationTestData : IEnumerable<object?[]> {
    private static readonly (Type type, object? part1, object? part2)[] validationData = {
            (typeof(Advent2024.Day01.Day01), 1666427, 24316233),
            //(typeof(Advent2024.Day02.Day02), 2331, 71585),
            //(typeof(Advent2024.Day03.Day03), 550064, 85010461),
            //(typeof(Advent2024.Day04.Day04), 23235, 5920640),
            //(typeof(Advent2024.Day05.Day05), 157211394L, 50855035L),
            //(typeof(Advent2024.Day06.Day06), 1083852L, 23501589L),
            //(typeof(Advent2024.Day07.Day07), 250453939L, 248652697L),
            //(typeof(Advent2024.Day08.Day08), 17287, 18625484023687L),
            //(typeof(Advent2024.Day09.Day09), 1637452029, 908),
            //(typeof(Advent2024.Day10.Day10), 6856, 501),
            //(typeof(Advent2024.Day11.Day11), 9274989L, 357134560737L),
            //(typeof(Advent2024.Day12.Day12), 7379L, 7732028747925L),
            //(typeof(Advent2024.Day13.Day13), 28651, 25450),
            //(typeof(Advent2024.Day14.Day14), 103614L, 83790L),
            //(typeof(Advent2024.Day15.Day15), 511215, 236057),
            //(typeof(Advent2024.Day16.Day16), 7242, 7572),
            //(typeof(Advent2024.Day17.Day17), 1065, 1249),
            //(typeof(Advent2024.Day18.Day18), 74074, 112074045986829L),
            //(typeof(Advent2024.Day19.Day19), 399284L, 121964982771486L),
            //(typeof(Advent2024.Day20.Day20), 919383692L, 247702167614647L),
            //(typeof(Advent2024.Day21.Day21), 3770, 628206330073385L),
            //(typeof(Advent2024.Day22.Day22), 457, 79122L),
            //(typeof(Advent2024.Day23.Day23), 1966, 6286),
            //(typeof(Advent2024.Day24.Day24), 25261, 549873212220117L),
            //(typeof(Advent2024.Day25.Day25), 606062, null),
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
            day.Part1().Should().Be(part1);
            day.Part2().Should().Be(part2);
        }
    }
}