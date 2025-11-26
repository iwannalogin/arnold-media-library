namespace GTT;

public static class Terminal {
    public static void SavePosition()
        => Console.Write( TerminalCodes.SavePosition );

    public static void RestorePosition()
        => Console.Write( TerminalCodes.RestorePosition );

    public static void ClearScreen()
        => Console.Write(
            TerminalCodes.CursorPosition(0,0)
            + TerminalCodes.ClearToEnd
        );
    
}