using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TM.PoC.API.Widgets.Types;

public class Widget
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }

    public static Widget FromJson(string json)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        return JsonSerializer.Deserialize<Widget>(json)!;
    }

    public static Widget FromBytes(byte[] bytes)
    {
        if (bytes == null || !bytes.Any()) throw new ArgumentNullException(nameof(bytes));
        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<Widget>(json)!;
    }
}