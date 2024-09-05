using Cardano.Sync.Reducers;
using Cardano.Sync.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Cardano.Sync.Extensions;

public static class ReducerExtensions
{
    public static void AddReducers<T>(this IServiceCollection services, string[]? optInList = null)
    {
        optInList ??= [];

        IEnumerable<Type> reducerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IReducer).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract);

        if (reducerTypes.Any())
        {
            foreach (Type reducerType in reducerTypes)
            {
                string typeName = ArgusUtils.GetTypeNameWithoutGenerics(reducerType);

                if (optInList.Contains(typeName))
                {
                    if (reducerType.IsGenericTypeDefinition)
                    {
                        Type closedType = reducerType.MakeGenericType(typeof(T));
                        services.AddSingleton(typeof(IReducer), closedType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IReducer), reducerType);
                    }
                }
            }
        }
    }
}