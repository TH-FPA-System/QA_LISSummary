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
        SELECT p.part,
               p.[description] AS part_desc,
               pt.lower_limit_value,
               pt.upper_limit_value
        FROM part p
        INNER JOIN part_test pt ON pt.part = p.part
        INNER JOIN part_issue pi ON pi.part = p.part
        INNER JOIN test_result_clean trl ON trl.test_part = p.part
        WHERE trl.task = @TaskNo
          AND pi.part_issue = pt.part_issue
          AND pi.eff_start <= GETDATE()
          AND pi.eff_close >= GETDATE()
        GROUP BY p.part, p.[description], pt.lower_limit_value, pt.upper_limit_value";

            return ExecuteQuery<PART_TEST_LIMITS>(query, row => new PART_TEST_LIMITS
            {
                part = GetString(row, "part"),
                part_desc = GetString(row, "part_desc"),
                lower = GetString(row, "lower_limit_value"),
                upper = GetString(row, "upper_limit_value")
            },
            new SqlParameter("@TaskNo", taskNo));
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
            string query = $@"
        SELECT property_value
        FROM part_property_data
        WHERE property = '{property}'
          AND part IN (
              SELECT part
              FROM part_property_data
              WHERE property = 'PRODUCT CODE'
                AND property_value = 'DISHDRAWER'
          )
        GROUP BY property_value";

            return ExecuteQuery<PART_PROPERTY_DATA>(query, row => new PART_PROPERTY_DATA
            {
                PropertyValue = GetString(row, "property_value")
            });
        }
       
        public List<PART_PROPERTY_DATA> GetCommonPartPropertyDataByTask(string property, string taskNo)
        {
            string query = $@"
        SELECT property_value
        FROM part_property_data
        WHERE property = '{property}'
          AND part IN (
              SELECT part
              FROM part_structure
              WHERE task = '{taskNo}'
          )
        GROUP BY property_value";

            return ExecuteQuery<PART_PROPERTY_DATA>(query, row => new PART_PROPERTY_DATA
            {
                PropertyValue = GetString(row, "property_value")
            });
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
            // Prepare part list
            string partList = string.IsNullOrEmpty(partTestsNo)
                ? null
                : string.Join(",", partTestsNo.Split(',').Select(p => $"'{p.Trim()}'"));

            // Base query
            string query = @"
        SELECT tr.test_part, p.[description] AS LABEL
        FROM test_result tr
        INNER JOIN part p ON p.part = tr.test_part
        WHERE tr.date_tested >= @StartDate
          AND tr.date_tested < DATEADD(day, 1, @EndDate)";

            // Add optional TaskCheck condition
            if (!string.IsNullOrEmpty(taskCheck))
                query += " AND tr.task = @TaskCheck";

            // Add optional part list condition
            if (!string.IsNullOrEmpty(partList))
                query += $" AND p.part IN ({partList})";

            query += " GROUP BY tr.test_part, p.[description]";

            // Prepare parameters
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", startDate),
        new SqlParameter("@EndDate", endDate),
        new SqlParameter("@TaskCheck", string.IsNullOrEmpty(taskCheck) ? (object)DBNull.Value : taskCheck)
    };

            // Execute query using ExecuteQuery
            var result = ExecuteQuery(query, row => new LABEL_VALUE_CHARTS_STR
            {
                part_no = GetString(row, "test_part"),
                label = GetString(row, "LABEL")
            }, parameters.ToArray());

            return result;
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
INNER JOIN part_property_data ptd ON ptd.part = tr.part
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


        public LIMIT_ADJUST GetLimit_Adjust(String PartTest)
        {


            DataTable tmpTable = new DataTable();
            LIMIT_ADJUST result = new LIMIT_ADJUST();
            List<SqlParameter> queryParms = new List<SqlParameter>();
            string queryStr1;

            queryStr1 = "select top 1 id_limit_adjust, test_part, task, limit_adjust_type, limit_adjust_value From test_result_lis_limit_adjust where test_part='" + PartTest + "' ";




            using (var query = new SqlDataAdapter(queryStr1, database.Connection))
            {
                query.Fill(tmpTable);

                foreach (DataRow row in tmpTable.Rows)
                {

                    result.limit_adjust_type = GetString(row, "limit_adjust_type");
                    result.limit_adjust_value = GetString(row, "limit_adjust_value");


                }
            }


            return result;
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


    }
}