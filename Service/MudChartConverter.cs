using IAChatDB.DTOs;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IAChatDB.Service;

public class MudChartConverter
{
    public MudChartSpecDTO ConvertFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new AxisConverter() }
        };

        return JsonSerializer.Deserialize<MudChartSpecDTO>(json, options)!;
    }
}

public class AxisConverter : JsonConverter<ChartAxis>
{
    public override ChartAxis Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var values = new List<object>();

        foreach (var element in root.GetProperty("data").EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                values.Add(element.GetString()!);
            }
            else if (element.ValueKind == JsonValueKind.Number)
            {
                values.Add(element.GetDouble());
            }
        }

        return new ChartAxis
        {
            Label = root.GetProperty("label").GetString()!,
            Data = values
        };
    }
    public override void Write(Utf8JsonWriter writer, ChartAxis value, JsonSerializerOptions options)
    {
    }
}

public class ChartService
{
    public (string[] Categories, double[] Series) ConvertToMudChartData(MudChartSpecDTO chartSpec)
    {

        string[] categories = chartSpec.XAxis.Data.Select(v => v.ToString()).ToArray()!;

        double[] values = chartSpec.YAxis.Data
            .Select(v => Convert.ToDouble(v))
            .ToArray();

        return (categories, values);
    }
}