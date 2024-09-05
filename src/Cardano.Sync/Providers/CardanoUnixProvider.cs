using PallasDotnet;
using PallasDotnet.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Channels;

namespace Cardano.Sync.Providers;

public class CardanoUnixProvider(
    IConfiguration configuration,
    string socketPath,
    ulong networkMagic
) : ICardanoProvider
{
    private readonly NodeClient nodeClient = new();

    public async IAsyncEnumerable<NextResponse> ChainSyncAsync()
    {
        await nodeClient.ConnectAsync(socketPath, networkMagic);
        // await nodeClient.StartChainSyncAsync(new(
        //     startSlot,
        //     Hash.FromHex(startHash!)
        // ));

        var responseChannel = Channel.CreateUnbounded<NextResponse>(); 
        //NextResponse? nextResponse = null;

        nodeClient.ChainSyncNextResponse += (sender, args) =>
        {
            responseChannel.Writer.TryWrite(args.NextResponse);
            //nextResponse = args.NextResponse;
        };

        // yield return nextResponse;
        await foreach (NextResponse response in responseChannel.Reader.ReadAllAsync())
        {
            yield return response;
        }

        responseChannel.Writer.Complete();


        // var tcs = new TaskCompletionSource<NextResponse>();
        // bool isDisconnected = false;

        // nodeClient.ChainSyncNextResponse += (sender, args) =>
        // {
        //     if (args.NextResponse.Action != NextResponseAction.Await)
        //     {
        //         tcs.TrySetResult(args.NextResponse);
        //     }
        // };

        // nodeClient.Disconnected += (sender, e) =>
        // {
        //     isDisconnected = true; 
        //     tcs.TrySetResult(null); 
        // };

        // while (!isDisconnected)
        // {
        //     var nextResponse = await tcs.Task; 
        //     if (nextResponse == null) break;

        //     yield return nextResponse;

        //     tcs = new TaskCompletionSource<NextResponse>();
        // }

    }
}