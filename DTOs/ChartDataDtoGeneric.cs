using MudBlazor;

namespace IAChatDB.DTOs;

public class ChartDataDtoGeneric
{
    public string Title { get; set; }
    public ChartType ChartType { get; set; } 
    public List<string> Categories { get; set; }
    public List<ChartSeriesDtoGeneric> Series { get; set; }
}

public class ChartSeriesDtoGeneric
{
    public string Name { get; set; }
    public List<double> Data { get; set; }
    public string Color { get; set; }
}