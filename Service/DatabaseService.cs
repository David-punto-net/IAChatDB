using IAChatDB.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace IAChatDB.Service;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnectionDB")!;
    }

    public async Task<DatabaseSchemaModel> GenerateSchemaAsync()
    {
        DatabaseSchemaModel aiCon = new() { SchemaStructured = new List<TableSchemaModel>(), SchemaRaw = new List<string>() };

        var tables = new List<TableSchemaModel>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string[] restrictions = new string[4];
            restrictions[3] = "BASE TABLE";

            var dataTable = await connection.GetSchemaAsync("Tables", restrictions);
            foreach (DataRow row in dataTable.Rows)
            {
                var tableName = row["TABLE_NAME"].ToString();
                var tableSchema = await GetTableSchemaAsync(connection, tableName!);
                tables.Add(tableSchema);
            }
        }

        foreach (var group in tables)
        {
            aiCon.SchemaStructured.Add(new TableSchemaModel() { TableName = group.TableName, Columns = group.Columns });
        }

        var textLines = new List<string>();

        foreach (var table in aiCon.SchemaStructured)
        {
            var schemaLine = $"- {table.TableName} (";

            foreach (var column in table.Columns)
            {
                schemaLine += column.ColumnName + " " + column.ColumnType + ", ";
            }

            schemaLine += ")";
            schemaLine = schemaLine.Replace(", )", " )");

            Console.WriteLine(schemaLine);
            textLines.Add(schemaLine);
        }

        aiCon.SchemaRaw = textLines;

        return aiCon;
    }

    private async Task<TableSchemaModel> GetTableSchemaAsync(SqlConnection connection, string tableName)
    {
        var columns = new List<ColumnsSchemaModel>();

        using (var command = new SqlCommand($"SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName", connection))
        {
            command.Parameters.AddWithValue("@tableName", tableName);

            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnsSchemaModel
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString()!,
                        ColumnType = reader["DATA_TYPE"].ToString()!
                    });
                }
            }
        }

        return new TableSchemaModel
        {
            TableName = tableName,
            Columns = columns
        };
    }

    public async Task<List<List<string>>> GetDataDB(string sqlQuery)
    {
        var rows = new List<List<string>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    bool headersAdded = false;
                    while (await reader.ReadAsync())
                    {
                        var cols = new List<string>();
                        var headerCols = new List<string>();
                        if (!headersAdded)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                headerCols.Add(reader.GetName(i));
                            }
                            headersAdded = true;
                            rows.Add(headerCols);
                        }

                        for (int i = 0; i <= reader.FieldCount - 1; i++)
                        {
                            try
                            {
                                var value = reader.GetValue(i);
                                cols.Add(value is DBNull ? "NULL" : value.ToString()!);
                            }
                            catch
                            {
                                cols.Add("DataTypeConversionError");
                            }
                        }
                        rows.Add(cols);
                    }
                }
            }
        }

        return rows;
    }
}