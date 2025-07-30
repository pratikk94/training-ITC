using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SCTMS.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public DatabaseHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            
            return result is T ? (T)result : default(T);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            await connection.OpenAsync();
            adapter.Fill(dataTable);
            
            return dataTable;
        }

        public async Task<List<T>> ExecuteReaderAsync<T>(string sql, Func<SqlDataReader, T> mapFunction, params SqlParameter[] parameters)
        {
            var results = new List<T>();
            
            using var connection = GetConnection();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(mapFunction(reader));
            }
            
            return results;
        }

        public SqlParameter CreateParameter(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public SqlParameter CreateParameter(string name, SqlDbType type, object? value)
        {
            return new SqlParameter(name, type) { Value = value ?? DBNull.Value };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 