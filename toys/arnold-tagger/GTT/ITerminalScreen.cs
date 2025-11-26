using System.Runtime.CompilerServices;
using System.Threading;

namespace GTT;

public interface ITerminalScreen {
    public bool RequestClear { get; }

    public Task ShowAsync( CancellationToken? token );
}

public static class ITerminalScreenExtensions {
    public static Task ShowAsync( this ITerminalScreen screen, bool? clear = null, CancellationToken? token = null ) {
        clear ??= screen.RequestClear;
        if( clear.Value ) Terminal.ClearScreen();

        return screen.ShowAsync(token);
    }
}