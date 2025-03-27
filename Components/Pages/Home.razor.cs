using IAChatDB.DTOs;
using IAChatDB.Models;
using IAChatDB.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel.ChatCompletion;
using MudBlazor;
using System.Text.Json;

namespace IAChatDB.Components.Pages;

public partial class Home
{
    private bool Loading = false;
    private bool LoadingChart = false;
    private bool chatLoading = false;
    private string LoadingMessage = String.Empty;
    private bool ChatActivo = false;
    private bool ChatGraficos = false;

    private bool _shouldScroll;

    [Inject] private IBotIAService BotIAService { get; set; } = null!;
    [Inject] private IDatabaseService DatabaseService { get; set; } = null!;

    public FormModel Model { get; set; } = new FormModel();
    public string? Respuesta { get; set; }

    public List<List<string>> DataDB = new();
    public DatabaseSchemaModel AIConnectionDBModel { get; set; } = new();

    public ChatHistory chatHistory = new();

    public ChartDataDto ChartData { get; set; } = new();

    private bool open = true;
    private Anchor anchor;
    private Color Color = Color.Success;

    private void ToggleDrawer(Anchor anchor)
    {
        open = !open;
        this.anchor = anchor;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldScroll)
        {
            _shouldScroll = false;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Respuesta = "No hay datos.";

        try
        {
            AIConnectionDBModel = await DatabaseService.GenerateSchemaAsync();
        }
        catch (Exception e)
        {
            Respuesta = e.Message;
        }
    }

    public async Task ObtenerDatosSql()
    {
        if (ValidaPromt(Model.PromptIASQL!))
        {
            Loading = true;
            LoadingMessage = "Obteniendo datos...";
            ChatActivo = false;

            var aiResponse = await BotIAService.GetIA_SQLQueryAsync(Model.PromptIASQL!, AIConnectionDBModel.SchemaRaw);

            DataDB.Clear();
            chatHistory!.Clear();

            Respuesta = aiResponse.query;

            if (string.IsNullOrEmpty(Respuesta))
            {
                Respuesta = "Intrucción NO valida.";
            }
            else
            {
                DataDB = await DatabaseService.GetDataDB(Respuesta!);

                ClearChat();
            }

            Loading = false;
            StateHasChanged();
        }
    }

    public async Task OnIAChat()
    {
        if (ValidaPromt(Model.PromptIAChat!))
        {
            ChatActivo = true;
            chatLoading = true;
            _shouldScroll = true;

            chatHistory!.AddUserMessage(Model.PromptIAChat!);
            StateHasChanged();
            Model.PromptIAChat = "";

            var aiResponse = await BotIAService.GetIA_ChatAsync(chatHistory);
            chatHistory.AddAssistantMessage(aiResponse);

            chatLoading = false;
            _shouldScroll = true;
            StateHasChanged();
        }
    }

    private void ClearChat()
    {
        chatHistory!.Clear();
        ChatActivo = false;
        chatLoading = false;
        _shouldScroll = false;

        var msjSystem = "You are an AI specialized in data analysis." +
        "Your task is to process, analyze and answer questions about the following data: " + JsonSerializer.Serialize(DataDB) +
        "Use basic Markdown code in your response with a minimalist and professional design.";

        chatHistory.AddSystemMessage(msjSystem);

        chatHistory.AddMessage(AuthorRole.User, "");
    }

    public async Task OnIAChart()
    {
        if (DataDB.Count > 0)
        {
            ChartData = new();
            ChatGraficos = false;

            LoadingChart = true;
            var aiResponse = await BotIAService.GetIA_ChartAsync(DataDB);

            ChatGraficos = true;
            ChartData = aiResponse;

            LoadingChart = false;

            StateHasChanged();
        }
    }

    private bool ValidaPromt(string promt)
    {
        if (!string.IsNullOrEmpty(promt))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}