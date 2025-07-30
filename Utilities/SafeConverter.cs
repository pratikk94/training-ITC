using System.Data.SqlClient;

namespace SCTMS.Utilities
{
    public static class SafeConverter
    {
        /// <summary>
        /// Safely converts string to int with default value
        /// </summary>
        public static int ToInt(string? value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value, out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely converts object to int with default value
        /// </summary>
        public static int ToInt(object? value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (value is int intValue)
                return intValue;

            if (value is string stringValue)
                return ToInt(stringValue, defaultValue);

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely gets nullable int from SqlDataReader
        /// </summary>
        public static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(columnName))
                return null;

            var value = reader[columnName];
            if (value == null || value == DBNull.Value)
                return null;

            if (value is int intValue)
                return intValue;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return null;
        }

        /// <summary>
        /// Safely gets int from SqlDataReader with default value
        /// </summary>
        public static int GetInt(SqlDataReader reader, string columnName, int defaultValue = 0)
        {
            if (reader.IsDBNull(columnName))
                return defaultValue;

            var value = reader[columnName];
            return ToInt(value, defaultValue);
        }

        /// <summary>
        /// Safely converts string to decimal
        /// </summary>
        public static decimal ToDecimal(string? value, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely converts string to bool
        /// </summary>
        public static bool ToBool(string? value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            // Handle common boolean string representations
            value = value.Trim().ToLowerInvariant();
            
            if (value == "1" || value == "true" || value == "yes" || value == "on")
                return true;
                
            if (value == "0" || value == "false" || value == "no" || value == "off")
                return false;

            if (bool.TryParse(value, out bool result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely gets string from SqlDataReader
        /// </summary>
        public static string GetString(SqlDataReader reader, string columnName, string defaultValue = "")
        {
            if (reader.IsDBNull(columnName))
                return defaultValue;

            var value = reader[columnName];
            return value?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Safely gets DateTime from SqlDataReader
        /// </summary>
        public static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(columnName))
                return null;

            var value = reader[columnName];
            if (value is DateTime dateTime)
                return dateTime;

            if (DateTime.TryParse(value?.ToString(), out DateTime result))
                return result;

            return null;
        }

        /// <summary>
        /// Safely gets boolean from SqlDataReader
        /// </summary>
        public static bool GetBool(SqlDataReader reader, string columnName, bool defaultValue = false)
        {
            if (reader.IsDBNull(columnName))
                return defaultValue;

            var value = reader[columnName];
            if (value is bool boolValue)
                return boolValue;

            return ToBool(value?.ToString(), defaultValue);
        }
    }
} 