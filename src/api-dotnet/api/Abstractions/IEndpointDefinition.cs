namespace TM.PoC.API.Abstractions;

public interface IEndpointDefinition
{
    void RegisterHandlers(WebApplication app);
}

public static class EndpointDefinitionExtensions
{
    public static void AddEndpoints(this IServiceCollection services, params Type[] scanMarkers)
    {
        var endpointDefinitions = new List<IEndpointDefinition>();

        foreach (var marker in scanMarkers)
            endpointDefinitions.AddRange(
                marker.Assembly.ExportedTypes
                    .Where(x => typeof(IEndpointDefinition).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(Activator.CreateInstance)
                    .Cast<IEndpointDefinition>()
            );

        services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);
    }

    public static void UseEndpoints(this WebApplication app)
    {
        var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();
        foreach (var endpointDefinition in definitions) endpointDefinition.RegisterHandlers(app);
    }
}