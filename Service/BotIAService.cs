using Azure;
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
    public DatabaseSchemaModel AIConnectionDBModel { get; set; } = new();
    public MudChartConverter MudChartConverter = new();

    public BotIAService(IConfiguration configuration, IDatabaseService databaseService)
    {
        _endpoint = new Uri(configuration["GITHUTModels:Endpoint"]!);
        _credential = new ApiKeyCredential(configuration["GITHUTModels:Token"]!);
        _model = configuration["GITHUTModels:Model"]!;
    }

    public async Task<AIQueryModel> GetIA_SQLQueryAsync(string userMensaje, List<string> schemaRaw)
    {
        try
        {
            var client = new OpenAIClient(_credential, new OpenAIClientOptions { Endpoint = _endpoint });

            var builder = Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion(_model, client);

            var kernel = builder.Build();

            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = new();

            var msjSystem = GetSystemChatMessage(schemaRaw);

            chatHistory.AddSystemMessage(msjSystem.Result.ToString());

            chatHistory.AddUserMessage(userMensaje);

            PromptExecutionSettings settings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            ChatMessageContent message = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

            var response = message.ToString();
            var responseContent = response.Replace("```json", "").Replace("```", "").Replace("\\n", "");

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

        strbuilder.AppendLine("Include column name headers in the query results.");
        strbuilder.AppendLine("Always provide your answer in the JSON format below:");
        strbuilder.AppendLine(@"{ ""summary"": ""your-summary"", ""query"":  ""your-query"" }");
        strbuilder.AppendLine("Output ONLY JSON formatted on a single line. Do not use new line characters.");
        strbuilder.AppendLine(@"In the preceding JSON response, substitute ""your-query"" with Microsoft SQL Server Query to retrieve the requested data.");
        strbuilder.AppendLine(@"In the preceding JSON response, substitute ""your-summary"" with an explanation of each step you took to create this query in a detailed paragraph.");
        strbuilder.AppendLine(@"You should NOT generate queries that make changes to the database such as: INSERT, UPDATE, DELETE, DROP. .");
        strbuilder.AppendLine("Do not use MySQL syntax.");
        strbuilder.AppendLine("Always include all of the table columns and details.");

        return Task.FromResult(strbuilder);
    }

    public async Task<string> GetIA_ChatAsync(ChatHistory chatHistory)
    {

        var client = new OpenAIClient(_credential, new OpenAIClientOptions { Endpoint = _endpoint });

        var builder = Kernel.CreateBuilder()
                            .AddOpenAIChatCompletion(_model, client);

        var kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        ChatMessageContent message = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

        var response = message.ToString();

        return response;
    }

    public async Task<MudChartSpecDTO> GetIA_ChartAsync(List<List<string>> Datos)
    {
   
            var client = new OpenAIClient(_credential, new OpenAIClientOptions { Endpoint = _endpoint });

            var builder = Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion(_model, client);

            var kernel = builder.Build();


            var prompt = $$"""
                        Analiza los siguientes datos y genera especificaciones para un gráfico. 
                        Devuelve solo la respuesta en formato JSON sin explicaciones.

                        Datos de entrada:
                        "{{JsonSerializer.Serialize(Datos)}}"

                        Instrucciones:
                        1. Determina el tipo de gráfico más adecuado según los datos (ej: bar, line, pie).
                        2. Identifica los ejes X/Y o categorías si aplica.
                        3. Sugiere una paleta de colores y opciones de diseño.
                        4. Devuelve la configuración en formato JSON.

                        Output:
                        {
                          "chartType": "bar|line|pie...",
                          "xAxis": {
                            "label": "Nombre del eje X",
                            "data": ["valor1", "valor2", ...]
                          },
                          "yAxis": {
                            "label": "Nombre del eje Y",
                            "data": [valor1, valor2, ...]
                          }
                        }
                        """;

            var chartSpecs = await kernel.InvokePromptAsync(prompt);

            var response = chartSpecs.ToString().Replace("```json", "").Replace("```", "").Replace("\\n", "");

            return MudChartConverter.ConvertFromJson(response);

    }

}