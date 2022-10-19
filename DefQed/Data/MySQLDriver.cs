#if DEBUG
//#define __USE_INLINE_CONNSTR__
#endif

using System;
using System.Collections.Generic;
using Console = Common.LogConsole;

namespace DefQed.Data
{
    /// <summary>
    /// <c>MySQLDriver</c> provides a way for the program to connect to the MySQL database.
    /// </summary>
    /// <remarks>
    /// The MySql.Data package must be installed to use this class.
    /// </remarks>
    internal static class MySQLDriver
    {
        /// <summary>
        /// (field) This field stores the actual MySQL connection.
        /// </summary>
        private static MySql.Data.MySqlClient.MySqlConnection? conn;

#if __USE_INLINE_CONNSTR__
        // TODO: Remove the inline connstr when releasing.
        // Should use for debug only.
        // Remove this line of code when release.
        public static string connStr = @"server=127.0.0.1;uid=DefQed;pwd=oClg2%[TenbL86V+rsC3;database=defqed";
#else
        /// <summary>
        /// (field) This field stores the connection string. Default is blank.
        /// </summary>
        private static string connStr = "";

        /// <summary>
        /// This property gives access to the connection string used by the driver.
        /// </summary>
        /// <value>
        /// The <c>ConnStr</c> property must be set to the connection string before connecting.
        /// </value>
        public static string ConnStr { get => connStr; set => connStr = value; }
#endif

        /// <summary>
        /// Initializes the connection to the MySQL server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note: Currently, it can only connect to MySQL servers on the local host.
        /// </para>
        /// <para>
        /// However, as a TODO, remote server accessibility will be added later.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A boolean representing whether the initialization is successful.
        /// </returns>
        public static bool Initialize()
        {
            try
            {
                conn = new();
                {
                    conn.ConnectionString = ConnStr;
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

        /// <summary>
        /// Safely closes the MySql connection.
        /// </summary>
        /// <remarks>
        /// I admit that in the <c>v0.01</c> version the connections are not terminated
        /// safely that they are all closed in the 'hard' way. (forcely)
        /// </remarks>
        /// <returns>
        /// A boolean representing whether the termination is done successfully.
        /// </returns>
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

        /// <summary>
        /// Get the names of all the data tables in the openned database.
        /// </summary>
        /// <remarks>
        /// Pay attenetion that the returning object is a list of names, not a list of tables.
        /// </remarks>
        /// <returns>
        /// A list of the names of tables in the database.
        /// </returns>
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

        /// <summary>
        /// Performs a check of whether the DefQed data tables are created (or imported) correctly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be used to assess whether the installation is completed successfully.
        /// </para>
        /// <para>
        /// To succeed, a database must have these tables: notations, reflections, registries. This will
        /// not check the further structures and data types of the table.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A boolean representing if the check is successful.
        /// </returns>
        private static bool PerformTableCheck()
        {
            List<string> tables = GetTables();
            if (tables.Contains("notations") && tables.Contains("reflections") && tables.Contains("registries")) return true;
            else return false;
        }

        /// <summary>
        /// Actually perform a query of the table and get a list of a list of results.
        /// </summary>
        /// <param name="tableType">The type of the table.</param>
        /// <param name="keyColumn">The known column name.</param>
        /// <param name="keyValue">The known column's value.</param>
        /// <param name="askedColumns">The columns to query.</param>
        /// <returns>
        /// A list of list of string. Each sub-list represents a row queryed. Each list-of-sub-list represents
        /// a result from the <c>MySqlDataReader.Read()</c> method.
        /// </returns>
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

        /// <summary>
        /// Get everything by row from an entire table.
        /// </summary>
        /// <param name="tableType">The type of the table.</param>
        /// <returns>
        /// A list of list of string. Each sub list represents a row. Each list-of-sub-list represents
        /// a line of result of the <c>MySqlDataReader.Read()</c> method.
        /// </returns>
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

        /// <summary>
        /// Gets the maxi value of the <c>id</c> column of the given table.
        /// </summary>
        /// <param name="tableType">The type of the table.</param>
        /// <returns>
        /// An integer, that is, the maxi <c>id</c> of the given table's rows.
        /// </returns>
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

        /// <summary>
        /// This method inserts a new row into the targeted table.
        /// </summary>
        /// <param name="tableType">The type of the table to insert.</param>
        /// <param name="columns">The list of the columns to give value to.</param>
        /// <param name="values">The list of the values of the new row.</param>
        public static void InsertRow(TableType tableType, List<string> columns, List<string> values)
        {
            string sql = $"INSERT INTO {TableType2Str(tableType)} ({List2Str(columns, false)}) VALUES ({List2Str(values)});";

            Console.WriteLine(sql);

            MySql.Data.MySqlClient.MySqlCommand cmd = new(sql, conn);
            _ = cmd.ExecuteNonQuery();

            Console.Log(Common.LogLevel.Diagnostic, $"Insert row. SQL: {sql}");
        }

        /// <summary>
        /// Converts a list of string into a string to be used to construct the SQL.
        /// </summary>
        /// <param name="list">The list of strings.</param>
        /// <param name="quotes">
        /// <para>
        /// Controls whether to add a pair of double quotes outside the output string.
        /// </para>
        /// <para>
        /// This parameter is optional and its default value is <c>true</c>.
        /// </para>
        /// </param>
        /// <returns>
        /// A string that will be used in constructing the SQL command.
        /// </returns>
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

        /// <summary>
        /// A big switch expression to convert <c>TableType</c> to the regarding string.
        /// </summary>
        /// <param name="tableType">The <c>TableType</c> value.</param>
        /// <returns>
        /// The string regarding to the <c>TableType</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// This exception occurs when an illegal value of TableType is given, for example 7.
        /// </exception>
        private static string TableType2Str(TableType tableType) => tableType switch
        {
            TableType.Notations => "notations",
            TableType.Reflections => "reflections",
            TableType.Registries => "registries",
            _ => throw new ArgumentOutOfRangeException($"Table {tableType} is undefined.")
        };
    }

    /// <summary>
    /// This enumeration has three types: Notations, Reflections, Registries.
    /// </summary>
    internal enum TableType
    {
        Notations,
        Reflections,
        Registries
    }
}