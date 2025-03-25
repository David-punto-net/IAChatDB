using IAChatDB.Models;

namespace IAChatDB.Service;

public interface IDatabaseService
{
    Task<DatabaseSchemaModel> GenerateSchemaAsync();
    Task<List<List<string>>> GetDataDB(string sqlQuery);
}