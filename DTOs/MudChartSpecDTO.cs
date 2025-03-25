using MudBlazor;

namespace IAChatDB.DTOs;

public class MudChartSpecDTO
{
    public string ChartType { get; set; }
    public ChartAxis XAxis { get; set; }
    public ChartAxis YAxis { get; set; }
  
}

public class ChartAxis
{
    public string Label { get; set; }
    public List<object> Data { get; set; }
}


