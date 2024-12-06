﻿namespace Advent2024;

public abstract class DayBase : IDay {
    protected DayBase() {
    }

    public virtual object? Part1(bool print = true) => default;
    public virtual object? Part2(bool print = true) => default;

    protected string GetDataFilePath() {
        var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        return Path.Combine(directory ?? throw new Exception($"Directory returned was null"), Path.Combine(GetType().Name, "Data.txt"));
    }

    protected StreamReader GetDataReader() {
        var path = GetDataFilePath();
        if (!File.Exists(path))
            DownloadData();
        return new(path);
    }

    private void DownloadData() {
        var typeName = GetType().Name;
        var (tens, ones) = (typeName[^2], typeName[^1]);
        if (!char.IsAsciiDigit(tens) | !char.IsAsciiDigit(ones))
            throw new Exception("Unexpected type name: Unable to parse day number");

        var dayNum = (typeName[^2] & 0xF) * 10 + (typeName[^1] & 0xF);
        using var inStream = InputDownloader.DownloadInput(dayNum);
        var path = GetDataFilePath();
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        using (var writer = new StreamWriter(path, false)) {
            using var reader = new StreamReader(inStream);
            for (string? line; (line = reader.ReadLine()) != null;) {
                writer.Write(line);
                if (!reader.EndOfStream)
                    writer.WriteLine();
            }
                
        }
        ReadOnlySpan<string> paths = [dir, "..", "..", "..", "..", typeName];
        var copyDir = Path.Combine(paths);
        if (Directory.Exists(copyDir))
            File.Copy(path, Path.Combine(copyDir, "Data.txt"), true);
    }
}
