using System;

namespace Advent2024;

public enum Direction : byte {
    North,
    East,
    South,
    West,
}

public static class DirectionExtensions {
    private static readonly int directionCount = Enum.GetValues<Direction>().Length;

    public static Direction LeftOf(this Direction direction) {
        return (Direction)(((int)direction + directionCount - 1) % directionCount);
    }

    public static Direction RightOf(this Direction direction) {
        return (Direction)(((int)direction + 1) % directionCount);
    }

    public static Direction Inverse(this Direction direction) {
        return (Direction)(((int)direction + 2) % directionCount);
    }

    public static Direction TurnByDegrees(this Direction direction, int degrees) {
        var turnCount= Math.DivRem(degrees, 90, out var remainder) % 4;
        if (remainder != 0)
            throw new Exception("Unable to convert degrees to turns");
        return Math.Abs(turnCount) switch {
            0 => direction,
            1 => turnCount > 0 ? direction.LeftOf() : direction.RightOf(),
            2 => direction.Inverse(),
            3 => turnCount < 0 ? direction.LeftOf() : direction.RightOf(),
            _ => throw new Exception("How did this happen?")
        };
    }
}