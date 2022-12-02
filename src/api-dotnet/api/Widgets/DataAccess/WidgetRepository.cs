using MongoDB.Driver;
using TM.Decorators.Tracing;
using TM.PoC.API.Abstractions;
using TM.PoC.API.Widgets.Types;

namespace TM.PoC.API.Widgets.DataAccess;

public class WidgetRepository : IDataRepository<Widget>
{
    private readonly IMongoCollection<Widget> _widgets;

    public WidgetRepository(string dbConn, string dbName, string clName)
    {
        _widgets = new MongoClient(dbConn)
            .GetDatabase(dbName)
            .GetCollection<Widget>(clName);
    }

    [Trace]
    public async Task<List<Widget>> GetAsync()
    {
        return await _widgets.Find(_ => true).ToListAsync();
    }

    [Trace]
    public async Task<Widget> GetByIdAsync(string? id)
    {
        return await _widgets.Find(w => w.Id == id).FirstOrDefaultAsync();
    }

    [Trace]
    public async Task CreateAsync(Widget w)
    {
        w.CreatedOn = DateTimeOffset.Now;
        await _widgets.InsertOneAsync(w);
    }

    [Trace]
    public async Task DeleteAsync(string? bsonId)
    {
        await _widgets.DeleteOneAsync(w => w.Id == bsonId);
    }
}

public static class WidgetRepositoryExtensions
{
    public static IServiceCollection AddWidgetRepository(this IServiceCollection services)
    {
        var dbConn = Environment.GetEnvironmentVariable("DB_CONN")
                     ?? throw new Exception("missing 'DB_CONN'");

        var dbName = Environment.GetEnvironmentVariable("DB_NAME")
                     ?? throw new Exception("missing 'DB_NAME'");

        return services.AddSingleton<IDataRepository<Widget>>(_ =>
            new WidgetRepository(dbConn, dbName, "widgets"));
    }
}