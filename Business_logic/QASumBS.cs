using Newtonsoft.Json.Linq;
using QA_LISSummary.Models;
using QA_LISSummary.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
namespace QA_LISSummary.Business_logic
{
    public class QASumBS
    {
        private Database database;

        public QASumBS()
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
        INNER JOIN test_result_lis trl ON trl.test_part = p.part
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
    FROM test_result_lis trl
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


        public List<PART_TEST_LIMITS> GetPartTestLimitsByParameter()
        {
            const string query = @"
        SELECT '999999' AS part, 'Volt' AS part_desc, '0' AS lower_limit_value, '999' AS upper_limit_value
        UNION
        SELECT '999998' AS part, 'Current' AS part_desc, '0' AS lower_limit_value, '999' AS upper_limit_value";

            return ExecuteQuery<PART_TEST_LIMITS>(query, row => new PART_TEST_LIMITS
            {
                part = GetString(row, "part"),
                part_desc = GetString(row, "part_desc"),
                lower = GetString(row, "lower_limit_value"),
                upper = GetString(row, "upper_limit_value")
            });
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


        public List<XY_LABEL_CHARTS_STR> GenerateDateLabel(int dayStart, int dayEnd, int monthStart, int monthEnd)
        {
            var result = new List<XY_LABEL_CHARTS_STR>();

            for (int month = monthStart; month <= monthEnd; month++)
            {
                string monthStr = month.ToString("D2");
                string monthLabel = SetMonthLabel(monthStr);

                int startDay = (month == monthStart) ? dayStart : 1;
                int endDay = (month == monthEnd) ? dayEnd : DateTime.DaysInMonth(DateTime.Now.Year, month);

                for (int day = startDay; day <= endDay; day++)
                {
                    string dayStr = day.ToString("D2");
                    result.Add(new XY_LABEL_CHARTS_STR
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

        //ALL Result PART-TEST
        public List<LABEL_VALUE_CHARTS_STR> GetResultPartTests(
      DateTime startDate,
      DateTime endDate,
      string taskCheck,
      string partTestsNo,
      string market,
      string status,
      string stringSpare2)
        {
            // Prepare part list for query
            string partList = string.IsNullOrEmpty(partTestsNo)
                ? null
                : string.Join(",", partTestsNo.Split(',').Select(p => $"'{p.Trim()}'"));

            string queryStr;

            // Query when no specific parts are provided
            if (string.IsNullOrEmpty(partList))
            {
                queryStr = @"
            SELECT tr.test_part,
                   COUNT(tr.test_status) AS VALUE,
                   p.[description] AS LABEL,
                   tr.test_status
            FROM test_result tr
            INNER JOIN part p ON p.part = tr.test_part
            WHERE tr.date_tested >= @StartDate
              AND tr.date_tested < DATEADD(day, 1, @EndDate)
              AND tr.test_status = @Status
            GROUP BY tr.test_part, p.[description], tr.test_status";
            }
            else
            {
                // Query when specific parts are provided
                queryStr = $@"
            SELECT tr.test_part,
                   COUNT(tr.test_status) AS VALUE,
                   p.[description] AS LABEL,
                   tr.test_status
            FROM test_result tr
            INNER JOIN part p ON p.part = tr.test_part
            WHERE tr.date_tested >= @StartDate
              AND tr.date_tested < DATEADD(day, 1, @EndDate)
              AND tr.test_status = @Status
              AND p.part IN ({partList})";

                if (!string.IsNullOrEmpty(taskCheck))
                    queryStr += " AND tr.task = @TaskCheck";

                queryStr += " GROUP BY tr.test_part, p.[description], tr.test_status";
            }

            // Prepare SQL parameters
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", startDate),
        new SqlParameter("@EndDate", endDate),
        new SqlParameter("@Status", status ?? "F"),
        new SqlParameter("@TaskCheck", string.IsNullOrEmpty(taskCheck) ? (object)DBNull.Value : taskCheck)
    };

            // Execute query using QASUM_BS instance
            var result = ExecuteQuery<LABEL_VALUE_CHARTS_STR>(
                queryStr,
                row => new LABEL_VALUE_CHARTS_STR
                {
                    part_no = GetString(row, "test_part"),
                    label = GetString(row, "LABEL"),
                    value = GetString(row, "VALUE"),
                    Status = GetString(row, "test_status"),
                    link = $"ResultPartTestDetailGrid?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&TaskNo={taskCheck}&PartTestsNo={GetString(row, "test_part")}&Status={GetString(row, "test_status")}"
                },
                parameters.ToArray()
            );

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

        public List<LABEL_VALUE_CHARTS_STR> GetResultNew(
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

            string queryStr;

            // If no specific parts, use UNION logic for P and F statuses
            if (string.IsNullOrEmpty(partList))
            {
                string testStatus1 = status == "P" ? "P" : "F";
                string testStatus2 = status == "P" ? "F" : "P";

                queryStr = $@"
        WITH Dat1 AS (
            SELECT tr.test_part,
                   COUNT(tr.test_status) AS VALUE,
                   p.[description] AS LABEL,
                   tr.test_status
            FROM test_result tr
            INNER JOIN part p ON p.part = tr.test_part
            WHERE tr.date_tested >= @StartDate
              AND tr.date_tested < DATEADD(day, 1, @EndDate)
              AND tr.task = @TaskCheck
              AND tr.test_status = '{testStatus1}'
            GROUP BY tr.test_part, p.[description], tr.test_status
        ),
        Dat2 AS (
            SELECT tr.test_part,
                   '0' AS VALUE,
                   p.[description] AS LABEL,
                   '{testStatus1}' AS test_status
            FROM test_result tr
            INNER JOIN part p ON p.part = tr.test_part
            WHERE tr.date_tested >= @StartDate
              AND tr.date_tested < DATEADD(day, 1, @EndDate)
              AND tr.task = @TaskCheck
              AND tr.test_status = '{testStatus2}'
            GROUP BY tr.test_part, p.[description], tr.test_status
        )
        SELECT dt1.test_part, dt1.VALUE, dt1.LABEL, dt1.test_status
        FROM Dat1 dt1
        UNION ALL
        SELECT dt2.test_part, dt2.VALUE, dt2.LABEL, dt2.test_status
        FROM Dat2 dt2
        WHERE dt2.test_part NOT IN (SELECT dt1.test_part FROM Dat1 dt1)";
            }
            else
            {
                // If specific parts, simpler query
                queryStr = $@"
        SELECT tr.test_part,
               COUNT(tr.test_status) AS VALUE,
               tr.test_status,
               p.[description] AS LABEL
        FROM test_result tr
        INNER JOIN part p ON p.part = tr.test_part
        WHERE tr.date_tested >= @StartDate
          AND tr.date_tested < DATEADD(day, 1, @EndDate)
          AND tr.test_status = @Status
          AND p.part IN ({partList})";

                if (!string.IsNullOrEmpty(taskCheck))
                    queryStr += " AND tr.task = @TaskCheck";

                queryStr += " GROUP BY tr.test_part, tr.test_status, p.[description]";
            }

            // Prepare parameters
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", startDate),
        new SqlParameter("@EndDate", endDate),
        new SqlParameter("@Status", status ?? "F"),
        new SqlParameter("@TaskCheck", string.IsNullOrEmpty(taskCheck) ? (object)DBNull.Value : taskCheck)
    };

            // Execute query
            var result = ExecuteQuery<LABEL_VALUE_CHARTS_STR>(
                queryStr,
                row => new LABEL_VALUE_CHARTS_STR
                {
                    part_no = GetString(row, "test_part"),
                    label = GetString(row, "LABEL"),
                    value = GetString(row, "VALUE"),
                    Status = GetString(row, "test_status"),
                    link = $"ResultPartTestDetailGrid?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&TaskNo={taskCheck}&PartTestsNo={GetString(row, "test_part")}&Status={GetString(row, "test_status")}"
                },
                parameters.ToArray()
            );

            return result;
        }


        public List<LABEL_VALUE_CHARTS_STR> GetResultTask(DateTime startDate, DateTime endDate, string TaskCheck, string Status, string StringSpare2)
        {
            List<LABEL_VALUE_CHARTS_STR> result = new List<LABEL_VALUE_CHARTS_STR>();
            DataTable tmpTable = new DataTable();

            // --- SQL with INNER JOIN + date range ---
            string queryStr1 = @"
        SELECT 
            tk.[description] AS 'LABEL',
            COUNT(tr.serial) AS VALUE,
            tr.task_status AS test_status,
            tr.task AS test_part 
        FROM task_result tr
        INNER JOIN task tk ON tk.task = tr.task
        WHERE tr.task = @TaskCheck
          AND tr.task_status = @Status
          AND tr.date_tested BETWEEN @StartDate AND @EndDate
        GROUP BY tk.[description],tr.task_status, tr.task
    ";

            using (var conn = new SqlConnection(database.Connection.ConnectionString))
            using (var cmd = new SqlCommand(queryStr1, conn))
            {
                // --- Parameters ---
                cmd.Parameters.AddWithValue("@TaskCheck", TaskCheck ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tmpTable);
                }
            }

            // --- Build result list ---
            foreach (DataRow row in tmpTable.Rows)
            {
                string partNoTmp = row["test_part"].ToString();
                string statusTmp = row["test_status"].ToString();

                string link = $"ResultPartTestDetailGrid?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&TaskNo={TaskCheck}&PartTestsNo=&Status={statusTmp}";

                result.Add(new LABEL_VALUE_CHARTS_STR
                {
                    label = row["LABEL"].ToString(),
                    value = row["VALUE"].ToString(),
                    Status = statusTmp,
                    part_no = partNoTmp,
                    link = link
                });
            }

            return result;
        }
        public List<TEST_RESULT_DETAIL> GetResultPartTestsDetail(DateTime startDate, DateTime endDate, String TaskCheck, String PartTestsNo, String Status, String StringSpare2, String StringSpare3)
        {

            if (PartTestsNo == null || PartTestsNo == "")
            {
                PartTestsNo = "''";
            }


            PartTestsNo = PartTestsNo.Replace(",", "','");
            PartTestsNo = "'" + PartTestsNo + "'";


            DataTable tmpTable = new DataTable();
            List<TEST_RESULT_DETAIL> result = new List<TEST_RESULT_DETAIL>();
            List<SqlParameter> queryParms = new List<SqlParameter>();
            string queryStr1, queryStr2;

            queryStr1 = "select tr.part, tr.serial,tr.task,tr.test_part, p.[description] as part_desc ,tr.test_result,tr.test_fault," +
                " tr.test_status,tr.run_number,tr.station,tr.date_tested " +
                 "FROM   test_result tr " +
                   "INNER JOIN part p on p.part= tr.test_part ";


            queryStr2 = "WHERE ";
            if (TaskCheck != null && TaskCheck != "")
                queryStr2 = queryStr2 += " tr.task = '" + TaskCheck + "' AND";
            queryStr2 = queryStr2 += " tr.date_tested >= '" + startDate.ToString("dd-MMMM-yyyy") + "' " +
                            "AND tr.date_tested <=  DATEADD(day, 1, '" + endDate.ToString("dd-MMMM-yyyy") + "') ";
            if (PartTestsNo != null && PartTestsNo != "''''")
            {
                queryStr2 = queryStr2 += "AND p.part in(" + PartTestsNo + ") ";
            }
            if (Status != null && Status != "''" && Status != "")
            {
                queryStr2 = queryStr2 += "AND tr.test_status = '" + Status + "' ";
            }
            queryStr2 = queryStr2 += "order by tr.serial,tr.test_part";
            queryStr1 += queryStr2;



            using (var query = new SqlDataAdapter(queryStr1, database.Connection))
            {
                query.Fill(tmpTable);

                foreach (DataRow row in tmpTable.Rows)
                {
                    result.Add(new TEST_RESULT_DETAIL
                    {
                        part = GetString(row, "part"),
                        serial = GetString(row, "serial"),
                        task = GetString(row, "task"),
                        test_part = GetString(row, "test_part"),
                        part_desc = GetString(row, "part_desc"),
                        test_result = GetString(row, "test_result"),
                        test_fault = GetString(row, "test_fault"),
                        test_status = GetString(row, "test_status"),
                        run_number = GetString(row, "run_number"),
                        station = GetString(row, "station"),
                        date_tested = row["date_tested"].ToString(),



                    });


                }


            }




            return result;
        }
        public List<XY_LABEL_CHARTS_STR> GetDataXY(DateTime startDate, DateTime endDate, string PartNo, string Market, string TaskNo, string ModelName, string StrSpare3)
        {
            DataTable tmpTable = new DataTable();
            List<XY_LABEL_CHARTS_STR> result = new List<XY_LABEL_CHARTS_STR>();
            if (ModelName == null || ModelName == "")
            {
                ModelName = "ALL";
            }
            if (PartNo == null || PartNo == "")
            {
                PartNo = "";
            }
            string query = @"
SELECT 
    ROW_NUMBER() OVER (ORDER BY tr.date_tested ASC) AS RUNNO,
    DATEDIFF(SECOND, CONVERT(date, tr.date_tested), GETDATE()) AS sec,
    tr.test_value, 
    tr.date_tested,
    SUBSTRING(REPLACE(CONVERT(CHAR(10), tr.date_tested, 101), '/', ''), 0, 5) AS condate,
    tr.test_unit_id,
    tr.priority_set,
    tr.limit_adjust1, tr.limit_adjust_type1, tr.limit_adjust_lw1, tr.limit_adjust_up1,
    tr.limit_adjust2, tr.limit_adjust_type2, tr.limit_adjust_lw2, tr.limit_adjust_up2,
    tr.test_info1, tr.test_info2
FROM test_result_lis tr
INNER JOIN part_property_data ptd ON ptd.part = tr.part
/***OPTIONAL_JOINS***/
WHERE tr.test_part = @PartNo
  AND tr.date_tested >= @StartDate
  AND tr.date_tested < DATEADD(day, 1, @EndDate)  -- less than next day is enough
  AND tr.test_result NOT IN ('B:-', 'T:-', 'T:- B:-')
  AND (tr.priority_set IN ('', '1') OR tr.priority_set IS NULL)
/***OPTIONAL_FILTERS***/
ORDER BY tr.date_tested ASC;

";

            // Build dynamic WHERE conditions
            string optionalJoins = "";
            string optionalFilters = "";

            if (Market != "ALL")
            {
                optionalJoins += " INNER JOIN part_property_data ptd2 ON ptd2.part = tr.part ";
                optionalFilters += " AND ptd2.property = 'MARKET' AND ptd2.property_value = @Market ";
            }

            if (ModelName != "ALL")
            {
                optionalJoins += " INNER JOIN part_property_data ptd3 ON ptd3.part = tr.part ";
                optionalFilters += " AND ptd3.property = 'MODEL' AND ptd3.property_value = @ModelName ";
            }

            query = query.Replace("/***OPTIONAL_JOINS***/", optionalJoins)
                         .Replace("/***OPTIONAL_FILTERS***/", optionalFilters);

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@PartNo", PartNo);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                if (Market != "ALL")
                    cmd.Parameters.AddWithValue("@Market", Market);

                if (ModelName != "ALL")
                    cmd.Parameters.AddWithValue("@ModelName", ModelName);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tmpTable);
                }
            }

            foreach (DataRow row in tmpTable.Rows)
            {
                result.Add(new XY_LABEL_CHARTS_STR
                {
                    label = row["date_tested"].ToTypeDateTIme().ToString("dd-MM-yyyy"),
                    x = GetString(row, "condate"),
                    showverticalline = "1",
                    y = GetString(row, "test_value"),
                    unit = GetString(row, "test_unit_id"),
                    limit_adjust1 = GetString(row, "limit_adjust1"),
                    limit_adjust2 = GetString(row, "limit_adjust2"),
                    piorSet = GetString(row, "priority_set"),
                    limit_adjust_type1 = GetString(row, "limit_adjust_type1"),
                    limit_adjust_lw1 = GetString(row, "limit_adjust_lw1"),
                    limit_adjust_up1 = GetString(row, "limit_adjust_up1"),
                    limit_adjust_lw2 = GetString(row, "limit_adjust_lw2"),
                    limit_adjust_up2 = GetString(row, "limit_adjust_up2"),
                    test_info1 = GetString(row, "test_info1"),
                    test_info2 = GetString(row, "test_info2"),
                });
            }

            return result;
        }


        //POP 21-11-2024
        public List<XY_LABEL_CHARTS_STR> GetDataXYPara(DateTime startDate, DateTime endDate, String PartNo, String Market, String TaskNo, String StrSpare2, String StrSpare3)
        {


            DataTable tmpTable = new DataTable();
            List<XY_LABEL_CHARTS_STR> result = new List<XY_LABEL_CHARTS_STR>();
            List<SqlParameter> queryParms = new List<SqlParameter>();

            string queryStr1;

            //Specific Part Test
            queryStr1 = "SELECT '1' as RUNNO, '43762' as sec, '220'as test_value, GETDATE() as date_tested , '1121' as condate, " +
                "'V' as test_unit_id,'1'as priority_set,'0' as 'limit_adjust1','0' as 'limit_adjust_type1', " +
                "'0' as 'limit_adjust_lw1', '99' as 'limit_adjust_up1', '0' as'limit_adjust2','0' as 'limit_adjust_type2','0' as 'limit_adjust_lw2', '99'as 'limit_adjust_up2', 'Top'as 'test_info1','0' as 'test_info2'";




            using (var query = new SqlDataAdapter(queryStr1, database.Connection))
            {
                query.Fill(tmpTable);

                foreach (DataRow row in tmpTable.Rows)
                {
                    result.Add(new XY_LABEL_CHARTS_STR
                    {

                        label = row["date_tested"].ToTypeDateTIme().ToString("dd-MM-yyyy"),
                        x = GetString(row, "condate"),
                        showverticalline = "1",
                        y = GetString(row, "test_value"),
                        unit = GetString(row, "test_unit_id"),
                        limit_adjust1 = GetString(row, "limit_adjust1"),
                        limit_adjust2 = GetString(row, "limit_adjust2"),
                        piorSet = GetString(row, "priority_set"),
                        limit_adjust_type1 = GetString(row, "limit_adjust_type1"),
                        limit_adjust_lw1 = GetString(row, "limit_adjust_lw1"),
                        limit_adjust_up1 = GetString(row, "limit_adjust_up1"),
                        limit_adjust_lw2 = GetString(row, "limit_adjust_lw2"),
                        limit_adjust_up2 = GetString(row, "limit_adjust_up2"),
                        test_info1 = GetString(row, "test_info1"),
                        test_info2 = GetString(row, "test_info2"),

                    });

                }
            }



            return result;
        }
        public List<XY_CHARTS_STR> GetCapaHistoValueRaw(DateTime startDate, DateTime endDate, String PartNo, String Market, String TaskNo)
        {


            DataTable tmpTable = new DataTable();
            List<XY_CHARTS_STR> result = new List<XY_CHARTS_STR>();
            List<SqlParameter> queryParms = new List<SqlParameter>();
            string queryStr1;

            //Specific Part Test

            queryStr1 = "SELECT  tr.test_value    AS VALUE,Count(tr.test_value) AS LABEL " +
                    "FROM   test_result_lis tr ";
            if (Market != "ALL")
            {
                queryStr1 += "INNER JOIN part_property_data ptd2 ON ptd2.part = tr.part ";
            }
            queryStr1 += "WHERE tr.task = '" + TaskNo + "' " +
                        "AND tr.test_part = '" + PartNo + "' " +
                        "AND tr.task = '" + TaskNo + "' " +
                        "AND tr.date_tested >= '" + startDate.ToString("dd-MMMM-yyyy") + "' " +
                        "AND tr.date_tested <=  DATEADD(day, 1, '" + endDate.ToString("dd-MMMM-yyyy") + "') " +
                        "AND(tr.test_result<> 'B:-') AND(tr.test_result<> 'T:-') AND tr.test_result<>('T:- B:-') " +
                        "AND tr.priority_set < 2 ";
            if (Market != "ALL")
            {
                queryStr1 += "AND ptd2.property = 'MARKET' " +
                    "AND ptd2.property_value ='" + Market + "' ";
            }
            queryStr1 += "GROUP  BY tr.test_value ";



            using (var query = new SqlDataAdapter(queryStr1, database.Connection))
            {
                query.Fill(tmpTable);

                foreach (DataRow row in tmpTable.Rows)
                {

                    result.Add(new XY_CHARTS_STR
                    {

                        x = GetString(row, "VALUE"),
                        y = GetString(row, "LABEL"),

                    });

                }
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


        public List<XY_LABEL_CHARTS_STR> GetDataXY_Avg(DateTime startDate, DateTime endDate, string PartNo, string Market, string TaskNo, string ModelName, string StrSpare3)
        {
            DataTable tmpTable = new DataTable();
            List<XY_LABEL_CHARTS_STR> result = new List<XY_LABEL_CHARTS_STR>();

            if (ModelName == null || ModelName == "")
                ModelName = "ALL";

            if (PartNo == null || PartNo == "")
                PartNo = "";

            string query = @"
       SELECT 
    AVG(tr.test_value) AS avg_test_value,
    FORMAT(tr.date_tested, 'MMdd') AS condate  -- simpler and faster than SUBSTRING+REPLACE+CONVERT
FROM test_result_lis tr
INNER JOIN part_property_data ptd 
    ON ptd.part = tr.part
/***OPTIONAL_JOINS***/
WHERE tr.test_part = @PartNo
  AND tr.date_tested >= @StartDate
  AND tr.date_tested < DATEADD(day, 1, @EndDate)  -- better for index usage
  AND tr.test_result NOT IN ('B:-', 'T:-', 'T:- B:-')
  AND (tr.priority_set IN ('', '1') OR tr.priority_set IS NULL)
/***OPTIONAL_FILTERS***/
GROUP BY FORMAT(tr.date_tested, 'MMdd')
ORDER BY condate;
";

            // Optional joins and filters
            string optionalJoins = "";
            string optionalFilters = "";

            if (Market != "ALL")
            {
                optionalJoins += " INNER JOIN part_property_data ptd2 ON ptd2.part = tr.part ";
                optionalFilters += " AND ptd2.property = 'MARKET' AND ptd2.property_value = @Market ";
            }

            if (ModelName != "ALL")
            {
                optionalJoins += " INNER JOIN part_property_data ptd3 ON ptd3.part = tr.part ";
                optionalFilters += " AND ptd3.property = 'MODEL' AND ptd3.property_value = @ModelName ";
            }

            query = query.Replace("/***OPTIONAL_JOINS***/", optionalJoins)
                         .Replace("/***OPTIONAL_FILTERS***/", optionalFilters);

            using (SqlCommand cmd = new SqlCommand(query, database.Connection))
            {
                cmd.Parameters.AddWithValue("@PartNo", PartNo);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                if (Market != "ALL")
                    cmd.Parameters.AddWithValue("@Market", Market);

                if (ModelName != "ALL")
                    cmd.Parameters.AddWithValue("@ModelName", ModelName);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tmpTable);
                }
            }

            foreach (DataRow row in tmpTable.Rows)
            {
                result.Add(new XY_LABEL_CHARTS_STR
                {
                    label = GetString(row, "condate"),
                    x = GetString(row, "condate"),
                    y = GetString(row, "avg_test_value"),
                    showverticalline = "1"
                });
            }

            return result;
        }

        //Check Part Tests
        public List<Part_Tests> GetPartTests()
        {
            const string query = @"
        SELECT tr.test_part,
               p.[description] AS part_desc,
               p.[class] AS part_class,
               tr.task
        FROM test_result_lis tr
        INNER JOIN part p ON tr.test_part = p.part
        GROUP BY tr.test_part, p.[description], p.[class], tr.task
        ORDER BY tr.task, p.[class]";

            return ExecuteQuery<Part_Tests>(query, row => new Part_Tests
            {
                test_part = GetString(row, "test_part"),
                description = GetString(row, "part_desc"),
                class_name = GetString(row, "part_class"),
                task = GetString(row, "task")
            });
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

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var distinctParts = testParts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToArray();

            if (distinctParts.Length == 0)
                return result;

            // -----------------------------
            // DataTable for TVP: TestParts
            var dtTestParts = new DataTable();
            dtTestParts.Columns.Add("test_part", typeof(string));
            foreach (var p in distinctParts)
                dtTestParts.Rows.Add(p);

            // DataTable for TVP: LastTimestamps
            var dtLastTimestamps = new DataTable();
            dtLastTimestamps.Columns.Add("test_part", typeof(string));
            dtLastTimestamps.Columns.Add("last_date", typeof(DateTime));

            if (lastTimestamps != null && lastTimestamps.Count > 0)
            {
                foreach (var kv in lastTimestamps)
                    dtLastTimestamps.Rows.Add(kv.Key, kv.Value);
            }
            // If empty, still pass empty table (cannot be null/DBNull)

            string sql = @"
SELECT
    r.test_part,
    r.test_value,
    r.date_tested,
    r.test_unit_id,
    p.[description] AS part_desc,
    pt.upper_limit_value AS USL,
    pt.lower_limit_value AS LSL,
    la.limit_adjust_value,
    la.limit_adjust_type
FROM @TestParts tp
INNER JOIN part p
    ON p.part = tp.test_part
INNER JOIN part_test pt
    ON pt.part = p.part
OUTER APPLY (
    SELECT TOP (1)
        r.test_part,
        r.test_value,
        r.date_tested,
        r.test_unit_id
    FROM test_result_lis r
    LEFT JOIN @LastTimestamps lt
        ON lt.test_part = r.test_part
    WHERE r.test_part = tp.test_part
      AND r.test_result NOT IN ('B:-','T:-','T:- B:-')
      AND (r.priority_set IN ('', '1') OR r.priority_set IS NULL)
      AND (lt.last_date IS NULL OR r.date_tested > lt.last_date)
    ORDER BY r.date_tested DESC
) r
LEFT JOIN test_result_lis_limit_adjust la
    ON la.test_part = r.test_part
WHERE r.date_tested IS NOT NULL
  AND EXISTS (
      SELECT 1
      FROM part_issue pi
      WHERE pi.part = p.part
        AND pi.part_issue = pt.part_issue
        AND pi.eff_start <= SYSDATETIME()
        AND pi.eff_close >= SYSDATETIME()
  );";

            using (var conn = new SqlConnection(database.Connection.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // TVP: TestParts
                    var tvpTestParts = new SqlParameter("@TestParts", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TVP_TestParts",
                        Value = dtTestParts
                    };
                    cmd.Parameters.Add(tvpTestParts);

                    // TVP: LastTimestamps (empty table if no values)
                    var tvpLastTimestamps = new SqlParameter("@LastTimestamps", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TVP_LastTimestamps",
                        Value = dtLastTimestamps
                    };
                    cmd.Parameters.Add(tvpLastTimestamps);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(reader.GetOrdinal("date_tested")) ||
                                reader.IsDBNull(reader.GetOrdinal("test_value")) ||
                                reader.IsDBNull(reader.GetOrdinal("test_part")))
                                continue;

                            DateTime dateTested = reader.GetDateTime(reader.GetOrdinal("date_tested"));
                            double testValue = reader.GetDouble(reader.GetOrdinal("test_value"));

                            result.Add(new XY_LABEL_CHARTS_STR_REALTIME
                            {
                                label = dateTested.ToString("dd-MM-yyyy HH:mm:ss"),
                                x = dateTested.ToString("HHmm"),
                                y = testValue.ToString(),
                                test_part = reader["test_part"].ToString().Trim(),
                                unit = reader["test_unit_id"]?.ToString()?.Trim() ?? "",
                                partDesc = reader["part_desc"]?.ToString()?.Trim() ?? "",
                                upper = reader["USL"] != DBNull.Value ? Convert.ToDouble(reader["USL"]) : 0,
                                lower = reader["LSL"] != DBNull.Value ? Convert.ToDouble(reader["LSL"]) : 0,
                                limit_adjust_value = reader["limit_adjust_value"]?.ToString()?.Trim() ?? "",
                                limit_adjust_type = reader["limit_adjust_type"]?.ToString()?.Trim() ?? "",
                                date_tested = dateTested
                            });
                        }
                    }
                }
            }

            return result;
        }



    }
}