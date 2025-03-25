namespace IAChatDB.Models
{
    public class TableSchemaModel
    {
        public string TableName { get; set; }
        public List<ColumnsSchemaModel> Columns { get; set; }
    }
}
