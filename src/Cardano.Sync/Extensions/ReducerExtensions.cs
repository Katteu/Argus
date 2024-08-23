using Cardano.Sync.Reducers;
using Cardano.Sync.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ReducerExtensions
{
    public static void MapReducers<T>(this IServiceCollection services, string[]? optOutList = null)
    {
        optOutList ??= [];

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

                if (!optOutList.Contains(typeName))
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