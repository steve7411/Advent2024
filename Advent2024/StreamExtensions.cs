﻿namespace Advent2024;

public static class StreamExtensions {
    public static (int charsInLine, int linesInFile) GetLineInfoForRegularFile(this Stream stream) {
        long startingPosition = stream.Position;
        var (lineLength, firstCharPosition) = stream.GetRemainingCharacterCountInLine();
        stream.Seek(-1, SeekOrigin.End);
        var endsInNewLine = stream.ReadByte() == Environment.NewLine[^1];
        stream.Seek(startingPosition, SeekOrigin.Begin);
        var beginBytes = (int)firstCharPosition;
        return (lineLength, (int)(stream.Length - beginBytes + (endsInNewLine ? 0 : Environment.NewLine.Length)) / (lineLength + Environment.NewLine.Length));
    }

    public static (int lineLength, long firstCharacterPosition) GetRemainingCharacterCountInLine(this Stream stream) {
        long startingPosition = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        static bool isPrintableAsciiCharacter(int x) => x >= ' ' && x <= '~';
        int lastChar = -1;
        //Read past byte order mark (BOM), if present
        while (stream.Position < stream.Length && !isPrintableAsciiCharacter(lastChar = stream.ReadByte())) ;
        long firstCharPosition = stream.Position - 1;
        while (stream.Position < stream.Length && (lastChar = stream.ReadByte()) is not '\r' and not '\n') ;
        //Read to end of newline sequence if it's more than one byte
        if (lastChar == Environment.NewLine[0]) {
            var newLineIndex = 1;
            while (stream.Position < stream.Length && (lastChar = stream.ReadByte()) != -1 && newLineIndex < Environment.NewLine.Length && lastChar == Environment.NewLine[newLineIndex++]) ;
        }
        long newPosition = stream.Position - (stream.Position == stream.Length - 1 ? 0 : 1);
        var lineLength = (int)(newPosition - firstCharPosition);
        stream.Seek(startingPosition, SeekOrigin.Begin);
        return (lineLength - Environment.NewLine.Length, firstCharPosition);
    }
}
