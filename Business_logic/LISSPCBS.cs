using Newtonsoft.Json.Linq;
using QA_LISSummary.Models;
using QA_LISSummary.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static QA_LISSummary.Models.LISSPCModels;
namespace QA_LISSummary.Business_logic
{
    public class LISSPCBS
    {
        private Database database;

        public LISSPCBS()
        {
            database = new Database(Database.Source.REDBOW, Database.Catalog.Thailis);
        }


        // Query Limits 
        public List<PART_TEST_LIMITS> GetPartTestLimitsByTaskAndTestResultLIS(string taskNo)
        {
            const string query = @"
;WITH AllLimits AS (
    -- ========= BASE =========
    SELECT
        'BASE' AS data_source,
        p.part,
        p.[description] AS part_desc,
        pt.lower_limit_value,
        pt.upper_limit_value,
        1 AS priority
    FROM part p
    INNER JOIN part_test pt ON pt.part = p.part
    INNER JOIN part_issue pi ON pi.part = p.part
    INNER JOIN test_result_clean trl ON trl.test_part = p.part
    WHERE trl.task = @TaskNo
      AND pi.part_issue = pt.part_issue
      AND pi.eff_start <= GETDATE()
      AND pi.eff_close >= GETDATE()

    UNION ALL

    -- ========= SPAREB =========
    SELECT
        'SPAREB' AS data_source,
        p.part,
        p.[description] AS part_desc,
        pt.lower_limit_value,
        pt.upper_limit_value,
        2 AS priority
    FROM LISBOM_part p
    INNER JOIN LISBOM_part_test pt ON pt.part = p.part
    INNER JOIN LISBOM_part_issue pi ON pi.part = p.part
    INNER JOIN test_result_clean trl ON trl.test_part = p.part
    WHERE trl.task = @TaskNo
      AND pi.part_issue = pt.part_issue
      AND pi.eff_start <= GETDATE()
      AND pi.eff_close >= GETDATE()
)
, Deduped AS (
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY part ORDER BY priority) AS rn
    FROM AllLimits
)
SELECT data_source, part, part_desc, lower_limit_value, upper_limit_value
FROM Deduped
WHERE rn = 1
ORDER BY part;
";

            return ExecuteQuery<PART_TEST_LIMITS>(
                query,
                row => new PART_TEST_LIMITS
                {
                    data_source = GetString(row, "data_source"), // BASE or SPAREB
                    part = GetString(row, "part"),
                    part_desc = GetString(row, "part_desc"),
                    lower = GetString(row, "lower_limit_value"),
                    upper = GetString(row, "upper_limit_value")
                },
                new SqlParameter("@TaskNo", taskNo)
            );
        }

        public List<PART_TEST_LIMITS> GetPartTestLimitsByPartTestAndTestResultLIS(string partTest)
        {
            const string query = @"
SELECT DISTINCT
       p.part,
       p.[description] AS part_desc,
       pt.lower_limit_value,
       pt.upper_limit_value
FROM part p
INNER JOIN part_test pt ON pt.part = p.part
INNER JOIN part_issue pi ON pi.part = p.part AND pi.part_issue = pt.part_issue
WHERE EXISTS (
    SELECT 1
    FROM test_result_clean trl
    WHERE trl.test_part = @partTest
      AND trl.test_part = p.part
)
AND pi.eff_start <= GETDATE()
AND pi.eff_close >= GETDATE()";

            return ExecuteQuery<PART_TEST_LIMITS>(query, row => new PART_TEST_LIMITS
            {
                part = GetString(row, "part"),
                part_desc = GetString(row, "part_desc"),
                lower = GetString(row, "lower_limit_value"),
                upper = GetString(row, "upper_limit_value")
            },
            new SqlParameter("@partTest", partTest));
        }

        // Query DD PropertyValue
        public List<PART_PROPERTY_DATA> GetDDGroupPartPropertyData(string property)
        {
            const string query = @"
SELECT property_value
FROM part_property_data
WHERE property = @Property
  AND part IN (
      SELECT part
      FROM part_property_data
      WHERE property = 'PRODUCT CODE'
        AND property_value = 'DISHDRAWER'
  )
GROUP BY property_value";

            return ExecuteQuery<PART_PROPERTY_DATA>(
                query,
                row => new PART_PROPERTY_DATA
                {
                    PropertyValue = GetString(row, "property_value")
                },
                new SqlParameter("@Property", property)
            );
        }

        public List<PART_PROPERTY_DATA> GetCommonPartPropertyDataByTask(string property, string taskNo)
        {
            const string query = @"
SELECT property_value
FROM part_property_data
WHERE property = @Property
  AND part IN (
      SELECT part
      FROM part_structure
      WHERE task = @Task
  )
GROUP BY property_value";

            return ExecuteQuery<PART_PROPERTY_DATA>(
                query,
                row => new PART_PROPERTY_DATA
                {
                    PropertyValue = GetString(row, "property_value")
                },
                new SqlParameter("@Property", property),
                new SqlParameter("@Task", taskNo)
            );
        }

        public double CalculateStandardDeviation(IEnumerable<double> values)
        {
            if (!values.Any())
                return 0;

            double avg = values.Average();
            double sumSquares = values.Sum(d => Math.Pow(d - avg, 2));

            // Using sample standard deviation formula (n-1)
            return Math.Sqrt(sumSquares / (values.Count() - 1));
        }
       
        public List<XY_LABEL_CHARTS_CLEAN_STR> GenerateDateLabel(int dayStart, int dayEnd, int monthStart, int monthEnd)
        {
            var result = new List<XY_LABEL_CHARTS_CLEAN_STR>();

            for (int month = monthStart; month <= monthEnd; month++)
            {
                string monthStr = month.ToString("D2");
                string monthLabel = SetMonthLabel(monthStr);

                int startDay = (month == monthStart) ? dayStart : 1;
                int endDay = (month == monthEnd) ? dayEnd : DateTime.DaysInMonth(DateTime.Now.Year, month);

                for (int day = startDay; day <= endDay; day++)
                {
                    string dayStr = day.ToString("D2");
                    result.Add(new XY_LABEL_CHARTS_CLEAN_STR
                    {
                        x = monthStr + dayStr,
                        label = dayStr + monthLabel
                    });
                }
            }

            return result;
        }

        public string SetMonthLabel(string month)
        {
            switch (month)
            {
                case "01": return "Jan";
                case "02": return "Feb";
                case "03": return "Mar";
                case "04": return "Apr";
                case "05": return "May";
                case "06": return "Jun";
                case "07": return "Jul";
                case "08": return "Aug";
                case "09": return "Sep";
                case "10":
                case "010": return "Oct";
                case "11":
                case "011": return "Nov";
                case "12":
                case "012": return "Dec";
                default: return "";
            }
        }

        public static string GetNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return new string(input.Where(char.IsDigit).ToArray());
        }

        public int mod(int a, int n)
        {
            int result = a % n;
            if ((result < 0 && n > 0) || (result > 0 && n < 0))
            {
                result += n;
            }
            return result;
        }


        public List<LABEL_VALUE_CHARTS_STR> GetLabelHeader(
       DateTime startDate,
       DateTime endDate,
       string taskCheck,
       string partTestsNo,
       string market,
       string status,
       string stringSpare2)
        {
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", startDate),
        new SqlParameter("@EndDate", endDate)
    };

            string query = @"
SELECT tr.test_part, p.[description] AS LABEL
FROM test_result tr
INNER JOIN part p ON p.part = tr.test_part
WHERE tr.date_tested >= @StartDate
  AND tr.date_tested < DATEADD(day, 1, @EndDate)";

            if (!string.IsNullOrEmpty(taskCheck))
            {
                query += " AND tr.task = @TaskCheck";
                parameters.Add(new SqlParameter("@TaskCheck", taskCheck));
            }

            if (!string.IsNullOrEmpty(partTestsNo))
            {
                var parts = partTestsNo.Split(',').Select(p => p.Trim()).ToList();
                var inParams = new List<string>();

                for (int i = 0; i < parts.Count; i++)
                {
                    string paramName = "@Part" + i;
                    inParams.Add(paramName);
                    parameters.Add(new SqlParameter(paramName, parts[i]));
                }

                query += $" AND p.part IN ({string.Join(",", inParams)})";
            }

            query += " GROUP BY tr.test_part, p.[description]";

            return ExecuteQuery(query, row => new LABEL_VALUE_CHARTS_STR
            {
                part_no = GetString(row, "test_part"),
                label = GetString(row, "LABEL")
            }, parameters.ToArray());
        }



        public List<XY_LABEL_CHARTS_CLEAN_STR> GetDataXY(
            DateTime startDate,
            DateTime endDate,
            string PartNo,
            string Market,
            string TaskNo,
            string ModelName,
            string StrSpare3)
        {
            DataTable tmpTable = new DataTable();
            List<XY_LABEL_CHARTS_CLEAN_STR> result = new List<XY_LABEL_CHARTS_CLEAN_STR>();

            if (string.IsNullOrEmpty(PartNo))
                PartNo = "";

            string query = @"
SELECT 
    ROW_NUMBER() OVER (ORDER BY tr.date_tested ASC) AS RUNNO,
    DATEDIFF(SECOND, CONVERT(date, tr.date_tested), GETDATE()) AS sec,
    tr.test_value, 
    tr.date_tested,
    SUBSTRING(REPLACE(CONVERT(CHAR(10), tr.date_tested, 101), '/', ''), 0, 5) AS condate,
    tr.test_unit,
    tr.test_info1,
    tr.test_info2
FROM test_result_clean tr
WHERE tr.test_part = @PartNo
  AND tr.date_tested >= @StartDate
  AND tr.date_tested < DATEADD(day, 1, @EndDate)
ORDER BY tr.date_tested ASC;
";

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@PartNo", PartNo);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tmpTable);
                }
            }

            foreach (DataRow row in tmpTable.Rows)
            {
                result.Add(new XY_LABEL_CHARTS_CLEAN_STR
                {
                    label = row["date_tested"].ToTypeDateTIme().ToString("dd-MM-yyyy"),
                    x = GetString(row, "condate"),
                    showverticalline = "1",
                    y = GetString(row, "test_value"),
                    test_unit = GetString(row, "test_unit")

                });
            }

            return result;
        }
    public List<XY_LABEL_CHARTS_CLEAN_STR> GetDataXY_Avg(
    DateTime startDate,
    DateTime endDate,
    string PartNo,
    string Market,
    string TaskNo,
    string ModelName,
    string StrSpare3)
{
    DataTable tmpTable = new DataTable();
    List<XY_LABEL_CHARTS_CLEAN_STR> result = new List<XY_LABEL_CHARTS_CLEAN_STR>();

    if (string.IsNullOrEmpty(PartNo))
        PartNo = "";

    string query = @"
WITH AvgData AS
(
    SELECT
        FORMAT(tr.date_tested, 'MMdd') AS condate,
        AVG(CAST(tr.test_value AS float)) AS avg_test_value,
        MIN(tr.date_tested) AS date_tested,
        MAX(tr.test_unit) AS test_unit
    FROM test_result_clean tr
    WHERE tr.test_part = @PartNo
      AND tr.date_tested >= @StartDate
      AND tr.date_tested < DATEADD(day, 1, @EndDate)
    GROUP BY FORMAT(tr.date_tested, 'MMdd')
)
SELECT
    ROW_NUMBER() OVER (ORDER BY condate) AS RUNNO,
    0 AS sec,
    avg_test_value,
    date_tested,
    condate,
    test_unit,
    NULL AS test_info1,
    NULL AS test_info2
FROM AvgData
ORDER BY condate;
";

    using (SqlCommand cmd = new SqlCommand(query, database.Connection))
    {
        cmd.Parameters.AddWithValue("@PartNo", PartNo);
        cmd.Parameters.AddWithValue("@StartDate", startDate);
        cmd.Parameters.AddWithValue("@EndDate", endDate);

        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
        {
            adapter.Fill(tmpTable);
        }
    }

    foreach (DataRow row in tmpTable.Rows)
    {
        result.Add(new XY_LABEL_CHARTS_CLEAN_STR
        {
            label = GetString(row, "condate"),
            x = GetString(row, "condate"),
            y = GetString(row, "avg_test_value"),
            showverticalline = "1"
        });
    }

    return result;
}



        public LIMIT_ADJUST GetLimit_Adjust(string partTest, string task)
        {
            const string query = @"
SELECT TOP 1
    id_limit_adjust,
    test_part,
    task,
    limit_adjust_type,
    limit_adjust_value
FROM test_result_lis_limit_adjust
WHERE test_part = @PartTest
  AND task = @Task";

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.Add("@PartTest", SqlDbType.VarChar).Value = partTest;
                cmd.Parameters.Add("@Task", SqlDbType.VarChar).Value = task;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    if (table.Rows.Count == 0)
                        return new LIMIT_ADJUST
                        {
                            limit_adjust_type = "NONE",
                            limit_adjust_value = "0"
                        };

                    DataRow row = table.Rows[0];

                    return new LIMIT_ADJUST
                    {
                        limit_adjust_type = GetString(row, "limit_adjust_type"),
                        limit_adjust_value = GetString(row, "limit_adjust_value")
                    };
                }
            }
        }


        private static string GetString(DataRow row, string column)
        {
            return row[column]?.ToString() ?? string.Empty;
        }

        private List<T> ExecuteQuery<T>(string query, Func<DataRow, T> mapFunc, params SqlParameter[] parameters)
        {
            var result = new List<T>();
            using (var cmd = new SqlCommand(query, database.Connection))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var table = new DataTable();
                    adapter.Fill(table);

                    foreach (DataRow row in table.Rows)
                        result.Add(mapFunc(row));
                }
            }
            return result;
        }



        // Query TASK List
        public List<TaskList> GetTasks()
        {
            string query = $@"
        select task, description From task where task in(
select task From test_result_lis group by task) order by description";

            return ExecuteQuery<TaskList>(query, row => new TaskList
            {
                Task = row["task"].ToTypeInteger(),
                TaskName = GetString(row, "description")
            });
        }


        public List<LimitAdjustVM> GetLimitAdjustList()
        {
            const string query = @"
SELECT 
    p.part,
    p.description AS part_desc,
    pt.lower_limit_value,
    pt.upper_limit_value,
    t.task,
    t.description AS task_desc,
    lmad.limit_adjust_type,
    lmad.limit_adjust_value
FROM test_result_lis_limit_adjust lmad
INNER JOIN task t ON lmad.task = t.task
INNER JOIN part p ON lmad.test_part = p.part
INNER JOIN part_issue pii ON p.part = pii.part
INNER JOIN part_test pt ON lmad.test_part = pt.part
WHERE pii.eff_start <= GETDATE()
  AND pii.eff_close >= GETDATE()
 order by t.task, p.description";

            return ExecuteQuery(query, row => new LimitAdjustVM
            {
                test_part = GetString(row, "part"),
                part_desc = GetString(row, "part_desc"),
                task = GetString(row, "task"),
                task_desc = GetString(row, "task_desc"),
                lower_limit_value = GetString(row, "lower_limit_value"),
                upper_limit_value = GetString(row, "upper_limit_value"),
                limit_adjust_type = GetString(row, "limit_adjust_type"),
                limit_adjust_value = GetString(row, "limit_adjust_value")
            });
        }
 
        public void InsertLimitAdjust(LIMIT_ADJUST model)
        {
            const string query = @"
INSERT INTO test_result_lis_limit_adjust
(test_part, task, limit_adjust_type, limit_adjust_value)
VALUES
(@Part, @Task, @Type, @Value)";

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@Part", model.test_part);
                cmd.Parameters.AddWithValue("@Task", model.task);
                cmd.Parameters.AddWithValue("@Type", model.limit_adjust_type);
                cmd.Parameters.AddWithValue("@Value", model.limit_adjust_value ?? "");
                if (database.Connection.State != ConnectionState.Open)
                    database.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLimitAdjust(LIMIT_ADJUST model)
        {
            const string query = @"
UPDATE test_result_lis_limit_adjust
SET limit_adjust_type = @Type,
    limit_adjust_value = @Value
WHERE test_part = @Part
  AND task = @Task";
          

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@Type", model.limit_adjust_type);
                cmd.Parameters.AddWithValue("@Value", model.limit_adjust_value ?? "");
                cmd.Parameters.AddWithValue("@Part", model.test_part);
                cmd.Parameters.AddWithValue("@Task", model.task);

                if (database.Connection.State != ConnectionState.Open)
                    database.Connection.Open();

                cmd.ExecuteNonQuery();
            }
        }


        public void DeleteLimitAdjust(string part, string task)
        {
            const string query = @"
DELETE FROM test_result_lis_limit_adjust
WHERE test_part = @Part
  AND task = @Task";

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@Part", part);
                cmd.Parameters.AddWithValue("@Task", task);
                if (database.Connection.State != ConnectionState.Open)
                    database.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }



        //REAL-TIME
        // INIT NEED CREATE TYPE BELOW
        //CREATE TYPE dbo.TVP_TestParts AS TABLE(test_part NVARCHAR(50) PRIMARY KEY);
        //CREATE TYPE dbo.TVP_LastTimestamps AS TABLE(test_part NVARCHAR(50) PRIMARY KEY, last_date DATETIME2);
        public List<XY_LABEL_CHARTS_STR_REALTIME> GetDataXYRealTimeBatch(
    string[] testParts,
    Dictionary<string, DateTime> lastTimestamps = null)
        {
            var result = new List<XY_LABEL_CHARTS_STR_REALTIME>();
            if (testParts == null || testParts.Length == 0)
                return result;

            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;

            var distinctParts = testParts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToArray();

            if (distinctParts.Length == 0)
                return result;

            // -----------------------------
            // TVP: TestParts
            var dtTestParts = new DataTable();
            dtTestParts.Columns.Add("test_part", typeof(string));
            foreach (var p in distinctParts)
                dtTestParts.Rows.Add(p);

            // -----------------------------
            // TVP: LastTimestamps
            var dtLastTimestamps = new DataTable();
            dtLastTimestamps.Columns.Add("test_part", typeof(string));
            dtLastTimestamps.Columns.Add("last_date", typeof(DateTime));

            if (lastTimestamps != null)
            {
                foreach (var kv in lastTimestamps)
                    dtLastTimestamps.Rows.Add(kv.Key, kv.Value);
            }

            string sql = @"
;WITH LatestBase AS (
    SELECT
        r.test_part,
        r.test_value,
        r.date_tested,
        r.test_unit,
        p.[description] AS part_desc,
        pt.upper_limit_value AS USL,
        pt.lower_limit_value AS LSL,
        la.limit_adjust_value,
        la.limit_adjust_type
    FROM @TestParts tp
    OUTER APPLY (
        SELECT TOP (1)
            r.test_part,
            r.test_value,
            r.date_tested,
            r.test_unit
        FROM test_result_clean r
        LEFT JOIN @LastTimestamps lt
            ON lt.test_part = r.test_part
        WHERE r.test_part = tp.test_part
          AND (lt.last_date IS NULL OR r.date_tested > lt.last_date)
        ORDER BY r.date_tested DESC
    ) r
    INNER JOIN part p
        ON p.part = tp.test_part
    INNER JOIN part_test pt
        ON pt.part = p.part
    LEFT JOIN test_result_lis_limit_adjust la
        ON la.test_part = r.test_part
    WHERE EXISTS (
        SELECT 1
        FROM part_issue pi
        WHERE pi.part = p.part
          AND pi.part_issue = pt.part_issue
          AND pi.eff_start <= SYSDATETIME()
          AND pi.eff_close >= SYSDATETIME()
    )
),
LatestSpare AS (
    SELECT
        r.test_part,
        r.test_value,
        r.date_tested,
        r.test_unit,
        p.[description] AS part_desc,
        pt.upper_limit_value AS USL,
        pt.lower_limit_value AS LSL,
        la.limit_adjust_value,
        la.limit_adjust_type
    FROM @TestParts tp
    OUTER APPLY (
        SELECT TOP (1)
            r.test_part,
            r.test_value,
            r.date_tested,
            r.test_unit
        FROM test_result_clean r
        LEFT JOIN @LastTimestamps lt
            ON lt.test_part = r.test_part
        WHERE r.test_part = tp.test_part
          AND (lt.last_date IS NULL OR r.date_tested > lt.last_date)
        ORDER BY r.date_tested DESC
    ) r
    INNER JOIN LISBOM_part p
        ON p.part = tp.test_part
    INNER JOIN LISBOM_part_test pt
        ON pt.part = p.part
    LEFT JOIN test_result_lis_limit_adjust la
        ON la.test_part = r.test_part
    WHERE EXISTS (
        SELECT 1
        FROM LISBOM_part_issue pi
        WHERE pi.part = p.part
          AND pi.part_issue = pt.part_issue
          AND pi.eff_start <= SYSDATETIME()
          AND pi.eff_close >= SYSDATETIME()
    )
),
AllSources AS (
    SELECT 1 AS priority, 'BASE' AS data_source, * FROM LatestBase
    UNION ALL
    SELECT 2 AS priority, 'SPAREB' AS data_source, * FROM LatestSpare
),
Deduped AS (
    SELECT *,
           ROW_NUMBER() OVER (
               PARTITION BY test_part
               ORDER BY priority
           ) AS rn
    FROM AllSources
)
SELECT *
FROM Deduped
WHERE rn = 1;
";

            using (var conn = new SqlConnection(database.Connection.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add(new SqlParameter("@TestParts", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TVP_TestParts",
                        Value = dtTestParts
                    });

                    cmd.Parameters.Add(new SqlParameter("@LastTimestamps", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TVP_LastTimestamps",
                        Value = dtLastTimestamps
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dateTested = reader.GetDateTime(reader.GetOrdinal("date_tested"));
                            double testValue = reader.GetDouble(reader.GetOrdinal("test_value"));

                            result.Add(new XY_LABEL_CHARTS_STR_REALTIME
                            {
                                label = dateTested.ToString("dd-MM-yyyy HH:mm:ss"),
                                x = dateTested.ToString("HHmm"),
                                y = testValue.ToString(),
                                test_part = reader["test_part"].ToString().Trim(),
                                unit = reader["test_unit"]?.ToString()?.Trim() ?? "",
                                partDesc = reader["part_desc"]?.ToString()?.Trim() ?? "",
                                upper = reader["USL"] != DBNull.Value ? Convert.ToDouble(reader["USL"]) : 0,
                                lower = reader["LSL"] != DBNull.Value ? Convert.ToDouble(reader["LSL"]) : 0,
                                limit_adjust_value = reader["limit_adjust_value"]?.ToString()?.Trim() ?? "",
                                limit_adjust_type = reader["limit_adjust_type"]?.ToString()?.Trim() ?? "",
                                date_tested = dateTested,
                                data_source = reader["data_source"]?.ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }
        public List<XY_LABEL_CHARTS_STR_REALTIME> GetDataXYPreloadBatch(
    string[] testParts,
    int preloadCount)
        {
            var result = new List<XY_LABEL_CHARTS_STR_REALTIME>();
            if (testParts == null || testParts.Length == 0 || preloadCount <= 0)
                return result;

            var distinctParts = testParts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToArray();

            if (distinctParts.Length == 0)
                return result;

            // TVP: TestParts
            var dtTestParts = new DataTable();
            dtTestParts.Columns.Add("test_part", typeof(string));
            foreach (var p in distinctParts)
                dtTestParts.Rows.Add(p);

            string sql = @"
;WITH LatestBase AS (
    SELECT
        r.test_part,
        r.test_value,
        r.date_tested,
        r.test_unit,
        p.[description] AS part_desc,
        pt.upper_limit_value AS USL,
        pt.lower_limit_value AS LSL,
        la.limit_adjust_value,
        la.limit_adjust_type,
        ROW_NUMBER() OVER (PARTITION BY r.test_part ORDER BY r.date_tested DESC) AS rn
    FROM @TestParts tp
    OUTER APPLY (
        SELECT TOP (@PreloadCount)
            r.test_part,
            r.test_value,
            r.date_tested,
            r.test_unit
        FROM test_result_clean r
        WHERE r.test_part = tp.test_part
        ORDER BY r.date_tested DESC
    ) r
    INNER JOIN part p ON p.part = tp.test_part
    INNER JOIN part_test pt ON pt.part = p.part
    LEFT JOIN test_result_lis_limit_adjust la
        ON la.test_part = r.test_part
    WHERE EXISTS (
        SELECT 1
        FROM part_issue pi
        WHERE pi.part = p.part
          AND pi.part_issue = pt.part_issue
          AND pi.eff_start <= SYSDATETIME()
          AND pi.eff_close >= SYSDATETIME()
    )
),
LatestSpare AS (
    SELECT
        r.test_part,
        r.test_value,
        r.date_tested,
        r.test_unit,
        p.[description] AS part_desc,
        pt.upper_limit_value AS USL,
        pt.lower_limit_value AS LSL,
        la.limit_adjust_value,
        la.limit_adjust_type,
        ROW_NUMBER() OVER (PARTITION BY r.test_part ORDER BY r.date_tested DESC) AS rn
    FROM @TestParts tp
    OUTER APPLY (
        SELECT TOP (@PreloadCount)
            r.test_part,
            r.test_value,
            r.date_tested,
            r.test_unit
        FROM test_result_clean r
        WHERE r.test_part = tp.test_part
        ORDER BY r.date_tested DESC
    ) r
    INNER JOIN LISBOM_part p ON p.part = tp.test_part
    INNER JOIN LISBOM_part_test pt ON pt.part = p.part
    LEFT JOIN test_result_lis_limit_adjust la
        ON la.test_part = r.test_part
    WHERE EXISTS (
        SELECT 1
        FROM LISBOM_part_issue pi
        WHERE pi.part = p.part
          AND pi.part_issue = pt.part_issue
          AND pi.eff_start <= SYSDATETIME()
          AND pi.eff_close >= SYSDATETIME()
    )
),
AllSources AS (
    SELECT 1 AS priority, 'BASE' AS data_source, * FROM LatestBase
    UNION ALL
    SELECT 2 AS priority, 'SPAREB' AS data_source, * FROM LatestSpare
),
Deduped AS (
    SELECT *,
           ROW_NUMBER() OVER (
               PARTITION BY test_part
               ORDER BY priority, rn
           ) AS final_rn
    FROM AllSources
)
SELECT *
FROM Deduped
WHERE final_rn <= @PreloadCount
ORDER BY test_part, date_tested ASC;
";

            using (var conn = new SqlConnection(database.Connection.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add(new SqlParameter("@TestParts", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TVP_TestParts",
                        Value = dtTestParts
                    });

                    cmd.Parameters.AddWithValue("@PreloadCount", preloadCount);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dateTested = reader.GetDateTime(reader.GetOrdinal("date_tested"));
                            double testValue = reader.GetDouble(reader.GetOrdinal("test_value"));

                            result.Add(new XY_LABEL_CHARTS_STR_REALTIME
                            {
                                label = dateTested.ToString("dd-MM-yyyy HH:mm:ss"),
                                x = dateTested.ToString("HHmm"),
                                y = testValue.ToString(),
                                test_part = reader["test_part"].ToString().Trim(),
                                unit = reader["test_unit"]?.ToString()?.Trim() ?? "",
                                partDesc = reader["part_desc"]?.ToString()?.Trim() ?? "",
                                upper = reader["USL"] != DBNull.Value ? Convert.ToDouble(reader["USL"]) : 0,
                                lower = reader["LSL"] != DBNull.Value ? Convert.ToDouble(reader["LSL"]) : 0,
                                limit_adjust_value = reader["limit_adjust_value"]?.ToString()?.Trim() ?? "",
                                limit_adjust_type = reader["limit_adjust_type"]?.ToString()?.Trim() ?? "",
                                date_tested = dateTested,
                                data_source = reader["data_source"]?.ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }


    }
}