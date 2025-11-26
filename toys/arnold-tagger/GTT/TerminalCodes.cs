namespace GTT;

public static class TerminalCodes {
    //Escape = 1B

    //Cursor Positioning

    /// <summary>
    /// Move up one line, scroll if necessary
    /// </summary>
    public const string ReverseIndex = "\x1bM";

    /// <summary>
    /// Save cursor position
    /// </summary>
    public const string SavePosition = "\x1b7";

    /// <summary>
    /// Restore cursor to last saved position
    /// </summary>
    public const string RestorePosition = "\x1b8";

    public const string ClearToEnd = "\x1b0J";

    public const string ClearToBeginning = "\x1b1J";

    public const string ClearScreen = "\x1b2J";

    public const string ClearBuffer = "\x1b3J";

    public const string EnableAlternateBuffer = "\x1b?1049h";

    public const string DisableAlternateBuffer = "\x1b?1049l";

    public static string CursorPosition( int row, int column ) => $"\x1b{row};{column}H";
}