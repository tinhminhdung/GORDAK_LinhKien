using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Antigravity.ECommerce.Services
{
    public class BaseConnectionSql
    {
        // Khởi tạo ở Program.cs
        public static string ConnectionString { get; set; }

        public static SqlConnection Connection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        // Tự động map IDataReader sang đối tượng T
        public static List<T> Bind_List_Reader<T>(IDataReader reader) where T : new()
        {
            var res = new List<T>();
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                var obj = new T();
                foreach (var prop in properties)
                {
                    if (columnNames.Contains(prop.Name))
                    {
                        var val = reader[prop.Name];
                        if (val != DBNull.Value)
                        {
                            // Parse types handles nullables & enums
                            Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            object safeValue = (val == null) ? null : Convert.ChangeType(val, t);
                            prop.SetValue(obj, safeValue, null);
                        }
                    }
                }
                res.Add(obj);
            }
            return res;
        }

        public static List<T> ExecuteStoredProcedure<T>(string storedProcedureName, params SqlParameter[] parameters) where T : new()
        {
            using (var conn = Connection())
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    return Bind_List_Reader<T>(reader);
                }
            }
        }

        // --- RAW SQL METHODS (CommandType.Text) ---

        public static List<T> Query<T>(string sql, params SqlParameter[]? parameters) where T : new()
        {
            using (var conn = Connection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    return Bind_List_Reader<T>(reader);
                }
            }
        }

        public static T? QuerySingle<T>(string sql, params SqlParameter[]? parameters) where T : new()
        {
            var list = Query<T>(sql, parameters);
            return list.Count > 0 ? list[0] : default;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[]? parameters)
        {
            using (var conn = Connection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                // Tự động phát hiện nếu là Stored Procedure (không có khoảng trắng) hoặc Text
                cmd.CommandType = sql.Trim().Contains(" ") ? CommandType.Text : CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object? ExecuteScalar(string sql, params SqlParameter[]? parameters)
        {
            using (var conn = Connection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = sql.Trim().Contains(" ") ? CommandType.Text : CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }
    }
}
