using IAChatDB.DTOs;
using Microsoft.AspNetCore.Components;

namespace IAChatDB.Components.Pages;

public partial class MudChartView
{
    [Parameter] public ChartDataDto ChartData { get; set; }



}