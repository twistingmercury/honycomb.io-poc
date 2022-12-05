using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
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
    internal async Task<IResult> GetById(IDataRepository<Widget> repo, string bsonId)
    {
        var results = await repo.GetByIdAsync(bsonId);
        return Results.Ok(results);
    }

    [Trace]
    internal async Task<IResult> Get(IDataRepository<Widget> repo)
    {
        Tracer.CurrentSpan.AddEventInfo(
            name: "my event",
            data: new SpanAttribute[]
            {
                new(key: "data point 1", values: new object[] { "point 1", 42 }),
                new(key: "data point 2", values: new object[] { true, false }),
                new(key: "data point 3", values: 1.0f / 137.0f)
            });
        var results = await repo.GetAsync();
        Tracer.CurrentSpan.AddEventInfo(
            name: "results info",
            data: new SpanAttribute[]
            {
                new(key: "results count", values: results.Count)
            });
        return Results.Ok(results);
    }

    [Trace]
    internal async Task<IResult> Create(IDataRepository<Widget> repo,
        IPublisher<Widget> publisher, [FromBody] Widget w)
    {
        await publisher.Publish(w);
        return Results.Accepted();
    }

    [Trace]
    internal async Task<IResult> Delete(IDataRepository<Widget> repo,
        [FromBody] Widget w)
    {
        if (w.Id != null) await repo.DeleteAsync(w.Id);
        return Results.NoContent();
    }
}