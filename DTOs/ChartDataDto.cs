using MudBlazor;

namespace IAChatDB.DTOs;

public class ChartDataDto
{
    public string Title { get; set; }
    public ChartType ChartType { get; set; } 
    public string[] Categories { get; set; }
    public List<ChartSeries> Series { get; set; }
    //public ChartSeriesDto[] Series { get; set; }
}

public class ChartSeriesDto
{
    public string Name { get; set; }
    public double[] Data { get; set; }
    public string Color { get; set; }
}

