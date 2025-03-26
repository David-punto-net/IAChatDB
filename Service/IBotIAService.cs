using IAChatDB.DTOs;
using IAChatDB.Models;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IAChatDB.Service;

public interface IBotIAService
{
    Task<AIQueryModel> GetIA_SQLQueryAsync(string userMensaje, List<string> schemaRaw);
    Task<string> GetIA_ChatAsync(ChatHistory chatHistory);
    Task<ChartDataDto> GetIA_ChartAsync(List<List<string>> DataDB);
}