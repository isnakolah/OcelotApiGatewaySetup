namespace System.Net.Http;

public static class DecoratedServiceExtensions
{
    private static readonly Dictionary<Type, List<Type>> _decoratorChains = new();
    
    public static IServiceCollection AddDecoratedService<TInterface, TConcreteService>(this IServiceCollection services)
        where TInterface: class
        where TConcreteService: class, TInterface
    {
        _decoratorChains[typeof(TInterface)] = new List<Type> { typeof(TConcreteService) };
        
        services.AddTransient<TConcreteService>();

        return services;
    }
    
    public static IServiceCollection AddDecorator<TDecorator>(this IServiceCollection services)
        where TDecorator: class
    {
        var decoratedType = _decoratorChains.Keys.SingleOrDefault(t => t.IsAssignableFrom(typeof(TDecorator)));

        if (decoratedType is null)
        {
            throw new InvalidOperationException($"No decorated type found for decorator {typeof(TDecorator).Name}");
        }

        _decoratorChains[decoratedType].Insert(0, typeof(TDecorator));

        return services;
    }
    
    public static void BuildDecorators(this IServiceCollection services)
    {
        foreach (var (decoratedType, decoratorTypes) in _decoratorChains)
        {
            Func<IServiceProvider, object> factory = sp =>
            {
                var instance = sp.GetRequiredService(decoratorTypes.Last());

                return decoratorTypes
                    .SkipLast(1)
                    .Aggregate(instance, (current, type) => ActivatorUtilities.CreateInstance(sp, type, current));
            };

            services.AddTransient(decoratedType, factory);
        }
    }
}