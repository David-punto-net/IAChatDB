using IAChatDB.DTOs;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IAChatDB.Service;

public class MudChartConverter
{

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ChartDataDto ConvertFromJson(string json)
    {
        return JsonSerializer.Deserialize<ChartDataDto>(json, _options)!;
    }

    public static string ConvertToJson(ChartDataDto data)
    {
        return JsonSerializer.Serialize(data, _options);
    }
}

