namespace IAChatDB.Models;

public class DatabaseSchemaModel
{
    public List<TableSchemaModel> SchemaStructured { get; set; }
    public List<string> SchemaRaw { get; set; }
}