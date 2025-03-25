using IAChatDB.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor.Charts;
using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IAChatDB.Components.Pages;

public partial class MudChartView
{
    [Parameter]
    public MudChartSpecDTO ChartData { get; set; }

    private ChartType GetChartType()
    {
        return ChartData.ChartType.ToLower() switch
        {
            "bar" => ChartType.Bar,
            "line" => ChartType.Line,
            "pie" => ChartType.Pie,
            _ => ChartType.Bar
        };
    }
}