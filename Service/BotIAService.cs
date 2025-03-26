using IAChatDB.DTOs;
using IAChatDB.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using System.ClientModel;
using System.Text;
using System.Text.Json;

namespace IAChatDB.Service;

public class BotIAService : IBotIAService
{
    private readonly Uri _endpoint;
    private readonly ApiKeyCredential _credential;
    private readonly string _model;

    private readonly OpenAIClient _client;
    private readonly Kernel _kernel;

    public DatabaseSchemaModel AIConnectionDBModel { get; set; } = new();
    public MudChartConverter MudChartConverter = new();

    public BotIAService(IConfiguration configuration, IDatabaseService databaseService)
    {
        _endpoint = new Uri(configuration["GITHUTModels:Endpoint"]!);
        _credential = new ApiKeyCredential(configuration["GITHUTModels:Token"]!);
        _model = configuration["GITHUTModels:Model"]!;

        _client = new OpenAIClient(_credential, new OpenAIClientOptions { Endpoint = _endpoint });
        _kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(_model, _client)
                        .Build();
    }

    public async Task<AIQueryModel> GetIA_SQLQueryAsync(string userMensaje, List<string> schemaRaw)
    {
        try
        {
            IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = new();

            var msjSystem = GetSystemChatMessage(schemaRaw);

            chatHistory.AddSystemMessage(msjSystem.Result.ToString());
            chatHistory.AddUserMessage(userMensaje);

            ChatMessageContent message = await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel);

            var response = message.ToString();
            var responseContent = response.Replace("```json", "")
                                          .Replace("```", "")
                                          .Replace("\\n", "");

            return JsonSerializer.Deserialize<AIQueryModel>(responseContent)!;
        }
        catch (Exception ex)
        {
            throw new Exception("No se pudo analizar la respuesta de IA como una consulta SQL: " + ex.Message);
        }
    }

    private Task<StringBuilder> GetSystemChatMessage(List<string> schemaRaw)
    {
        var strbuilder = new StringBuilder();

        strbuilder.AppendLine("You are a helpful and friendly database wizard who generates T-SQL queries. Do not respond with information unrelated to databases or queries. Use the following database schema when creating your responses:");

        foreach (var table in schemaRaw)
        {
            strbuilder.AppendLine(table.ToString());
        }

        strbuilder.AppendLine("Include column name headers in the query results.")
                  .AppendLine("Always provide your answer in the JSON format below:")
                  .AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }")
                  .AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.")
                  .AppendLine(@"In the preceding JSON response, substitute ""your-query"" with Microsoft SQL Server Query to retrieve the requested data.")
                  .AppendLine(@"In the preceding JSON response, substitute ""your-summary"" with an explanation of each step you took to create this query in a detailed paragraph.")
                  .AppendLine(@"You should NOT generate queries that make changes to the database such as: INSERT, UPDATE, DELETE, DROP. .")
                  .AppendLine("Do not use MySQL syntax.")
                  .AppendLine("Always include all of the table columns and details.");

        return Task.FromResult(strbuilder);
    }

    public async Task<string> GetIA_ChatAsync(ChatHistory chatHistory)
    {

        IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        ChatMessageContent message = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, _kernel);

        return message.ToString();
    }

    public async Task<ChartDataDto> GetIA_ChartAsync(List<List<string>> Datos)
    {

        var prompt = $$"""
                        Analiza los siguientes datos y genera especificaciones para un gráfico.
                        Devuelve solo la respuesta en formato JSON sin explicaciones.

                        Datos de entrada:
                        "{{JsonSerializer.Serialize(Datos)}}"

                        Instrucciones:
                        1. Determina el tipo de gráfico más adecuado según los datos (ej: Bar, Line, Pie, Donut).
                        2. Identifica los ejes X/Y o categorías si aplica.
                        3. Determina la cantiddad de Series adecuadas.
                        4. Devuelve la configuración en formato JSON.

                        Output:
                        {
                          "Title": " Titulo del grafico...",
                          "ChartType": "Bar|Line|Pie|Donut...",
                          "Categories": ["Categoria1", "Categoria2",...],
                          "Series": [
                            {
                              "Name": "Nombre de la serie 1 ",
                              "Data": [valor1, valor2, ...]
                            }
                          ]
                        }
                        """;

        var chartSpecs = await _kernel.InvokePromptAsync(prompt);
        var response = chartSpecs.ToString()
                                 .Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("\\n", "");

        return MudChartConverter.ConvertFromJson(response);
    }
}

/*

            var prompt = $$"""
                        Analiza los siguientes datos y genera especificaciones para un gráfico.
                        Devuelve solo la respuesta en formato JSON sin explicaciones.

                        Datos de entrada:
                        "{{JsonSerializer.Serialize(Datos)}}"

                        Instrucciones:
                        1. Determina el tipo de gráfico más adecuado según los datos (ej: Bar, Line, Pie, Donut).
                        2. Identifica los ejes X/Y o categorías si aplica.
                        3. Sugiere una paleta de colores y opciones de diseño.
                        4. Devuelve la configuración en formato JSON.

                        Output:
                        {
                          "Title": " Titulo del grafico...",
                          "ChartType": "Bar|Line|Pie|Donut...",
                          "Categories": ["Categoria1", "Categoria2",...],
                          "Series": [
                            {
                              "Name": "Nombre de la serie 1 ",
                              "Data": [valor1, valor2, ...],
                              "Color": "#3d5afe..."
                            },
                            {
                              "Name": "Nombre de la serie 2",
                              "Data": [valor1, valor2, ...],
                              "Color": "#ff4081..."
                            }
                          ]
                        }
                        """;
*/