using Microsoft.AspNetCore.Mvc;
using TM.Decorators.Tracing;
using TM.PoC.API.Abstractions;
using TM.PoC.API.Widgets.Types;

namespace TM.PoC.API.Widgets.Endpoints;

public class WidgetEndpoints : IEndpointDefinition
{
    public void RegisterHandlers(WebApplication app)
    {
        app.MapGet("/widgets/{bsonId}", GetById);
        app.MapGet("/widgets", Get);
        app.MapPost("/widgets", Create);
        app.MapDelete("/widgets", Delete);
    }

    [Trace]
    internal async Task<IResult> GetById(ILogger<WidgetEndpoints> logger, IDataRepository<Widget> repo, string bsonId)
    {
        var results = await repo.GetByIdAsync(bsonId);
        logger.LogInformation("WidgetEndpoints.GetById completed successfully");
        return Results.Ok(results);
    }

    [Trace]
    internal async Task<IResult> Get(ILogger<WidgetEndpoints> logger, IDataRepository<Widget> repo)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        var results = await repo.GetAsync();
        logger.LogInformation("WidgetEndpoints.Get completed successfully");
        return Results.Ok(results);
    }

    [Trace]
    internal async Task<IResult> Create(ILogger<WidgetEndpoints> logger, IDataRepository<Widget> repo,
        IPublisher<Widget> publisher, [FromBody] Widget w)
    {
        await publisher.Publish(w);
        logger.LogInformation("WidgetEndpoints.Create completed successfully");
        return Results.Accepted();
    }

    [Trace]
    internal async Task<IResult> Delete(ILogger<WidgetEndpoints> logger, IDataRepository<Widget> repo,
        [FromBody] Widget w)
    {
        if (w.Id is not null) await repo.DeleteAsync(w.Id);
        logger.LogInformation("WidgetEndpoints.Delete completed successfully");
        return Results.NoContent();
    }
}