#if DEBUG
//#define __USE_INLINE_CONNSTR__
#endif

using System;
using System.Collections.Generic;
using Console = Common.LogConsole;

namespace DefQed.Data
{
    internal static class MySQLDriver
    {
        private static MySql.Data.MySqlClient.MySqlConnection? conn;

#if __USE_INLINE_CONNSTR__
        // TODO: Remove the inline connstr when releasing.
        // Should use for debug only.
        // Remove this line of code when release.
        public static string connStr = @"server=127.0.0.1;uid=DefQed;pwd=oClg2%[TenbL86V+rsC3;database=defqed";
#else
        public static string connStr = "";
#endif

        public static bool Initialize()
        {
            try
            {
                conn = new();
                {
                    conn.ConnectionString = connStr;
                }
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.Log(Common.LogLevel.Error, "Fatal error encountered when attempting to connect to database.");
                Console.Log(Common.LogLevel.Error, ex.Message);
                return false;
            }

            Console.Log(Common.LogLevel.Information, $"Connection state: {conn.State}");

            if (!PerformTableCheck())
            {
                Console.Log(Common.LogLevel.Warning, "Warning: Table structure may contain error.");
            }

            return true;
        }

        public static bool Terminate()
        {
            if (conn != null)
            {
                conn.Close();
                Console.Log(Common.LogLevel.Information, $"Connection state: {conn.State}");
                return true;
            }
            else
            {
                Console.Log(Common.LogLevel.Error, "Cannot terminate connection as connection is null.");
                return false;
            }
        }

        private static List<string> GetTables()
        {
            MySql.Data.MySqlClient.MySqlCommand cmd = new("SHOW TABLES;", conn);
            MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

            List<string> tables = new();

            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            reader.Close();

            return tables;
        }

        private static bool PerformTableCheck()
        {
            List<string> tables = GetTables();
            if (tables.Contains("notations") && tables.Contains("reflections") && tables.Contains("registries")) return true;
            else return false;
        }

        public static List<List<string>> QueryTable(TableType tableType, string keyColumn, string keyValue, List<string> askedColumns)
        {
            List<List<string>> result = new();

            // if has quotes here MySQL will return something useless.
            string sql = $"SELECT {List2Str(askedColumns, false)} FROM {TableType2Str(tableType)} WHERE {keyColumn} = \"{keyValue}\";";
            MySql.Data.MySqlClient.MySqlCommand cmd = new(sql, conn);
            MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                List<string> row = new();
                for (int i = 0; i < askedColumns.Count; i++)
                {
                    row.Add(reader.GetString(i));
                }

                result.Add(row);
            }

            reader.Close();

            return result;
        }

        public static List<List<string>> AcquireWholeTable(TableType tableType)
        {
            List<List<string>> result = new();

            string sql = $"SELECT * FROM {TableType2Str(tableType)};";
            MySql.Data.MySqlClient.MySqlCommand cmd = new(sql, conn);
            MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                List<string> row = new();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetString(i));
                }
                result.Add(row);
            }

            reader.Close();

            return result;
        }

        public static int GetMaxId(TableType tableType)
        {
            string sql = $"SELECT MAX(ID) FROM {TableType2Str(tableType)};";
            MySql.Data.MySqlClient.MySqlCommand cmd = new(sql, conn);
            MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

            reader.Read();
            int t;
            try
            {
                t = int.Parse(reader.GetString(0));
            }
            catch (System.Data.SqlTypes.SqlNullValueException ex)
            {
                //int choice = MessageBox.ErrorQuery(ex.ToString(), $"Exception details:\n{ex}\n\nAbort: end process; Ignore: continue (diagnostic only).", new ustring[] { "Abort", "Ignore" });
                int choice;
                Console.Log(Common.LogLevel.Error, ex.ToString() + $"Exception details:\n{ex}");
                System.Console.Write("A = Abort; I = Ignore");
              choice = System.Console.ReadLine()  switch
            {
                null => 1,
                "A" => 0,
                "a" => 0,
                "I" => 1,
                "i" => 1,
                _ => 1
            };
            if (choice == 0)
                {
                    // I admit that this error handling is a bit ugly.
                    t = -2;
                }
                else
                {
                    t = -1;
                }
            }
            reader.Close();
            return t;
        }

        public static void InsertRow(TableType tableType, List<string> columns, List<string> values)
        {
            string sql = $"INSERT INTO {TableType2Str(tableType)} ({List2Str(columns)}) VALUES ({List2Str(values)});";

            MySql.Data.MySqlClient.MySqlCommand cmd = new(sql, conn);
            _ = cmd.ExecuteNonQuery();

            Console.Log(Common.LogLevel.Diagnostic, $"Insert row. SQL: {sql}");
        }

        private static string List2Str(List<string> list, bool quotes = true)
        {
            string res = "";
            for (int i = 0; i < list.Count; i++)
            {
                if ((!int.TryParse(list[i].Trim(), out _)) && (!double.TryParse(list[i].Trim(), out _)) && quotes)
                {
                    // Is a string
                    res += $"\"{list[i].Trim()}\"";
                }
                else
                {
                    res += list[i].Trim();
                }

                if (i != list.Count - 1)
                {
                    res += ",";
                }
            }
            return res;
        }

        private static string TableType2Str(TableType tableType) => tableType switch
        {
            TableType.Notations => "notations",
            TableType.Reflections => "reflections",
            TableType.Registries => "registries",
            _ => throw new ArgumentOutOfRangeException($"Table {tableType} is undefined.")
        };
    }

    internal enum TableType
    {
        Notations,
        Reflections,
        Registries
    }
}