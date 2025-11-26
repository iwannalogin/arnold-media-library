namespace GTT;

public class TextScreen : ITerminalScreen {
    public bool RequestClear = true;

    public string? Heading { get; set; } = null;

    public async Task ShowAsync( CancellationToken? token = null ) {
        if( Terminal.GetColumn() != 0 ) {
            Terminal.NextLine();
        }
        var startRow = Terminal.GetRow();
        
    }
}