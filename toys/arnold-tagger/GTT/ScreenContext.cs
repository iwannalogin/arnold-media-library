namespace GTT;

public class ScreenContext : IDisposable {
    public ScreenContext() {
        Console.Write( TerminalCodes.EnableAlternateBuffer );
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        Console.Write(TerminalCodes.DisableAlternateBuffer);
    }
}