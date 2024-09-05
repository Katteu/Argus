using Cardano.Sync.Data;
using Cardano.Sync.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Cardano.Sync.Utils;

public static class ArgusUtils
{
    public static string GetTypeNameWithoutGenerics(Type type)
    {
        string typeName = type.Name;
        int genericCharIndex = typeName.IndexOf('`');
        if (genericCharIndex != -1)
        {
            typeName = typeName[..genericCharIndex];
        }
        return typeName;
    }

    public static async Task<ReducerState?> GetReducerStateAsync(
        CardanoDbContext dbContext,
        string reducerName,
        bool tracking,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ReducerState> baseQuery = dbContext.ReducerStates
            .Where(rs => rs.Name == reducerName);

        IQueryable<ReducerState> query = tracking ? baseQuery.AsNoTracking() : baseQuery;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}