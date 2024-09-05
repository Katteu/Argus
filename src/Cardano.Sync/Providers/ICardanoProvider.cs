using PallasDotnet.Models;

namespace Cardano.Sync.Providers;

public interface ICardanoProvider
{
    IAsyncEnumerable<NextResponse> ChainSyncAsync();
}