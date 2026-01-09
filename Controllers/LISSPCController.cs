using QA_LISSummary.Business_logic;
using QA_LISSummary.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static QA_LISSummary.Models.LISSPCModels;

namespace QA_LISSummary.Controllers
{
    public class LISSPCController : Controller
    {
       
            private LISSPCBS LISSPC_BS; //Business Logic

            public LISSPCController()
            {
            LISSPC_BS = new LISSPCBS();

            }
        // GET: LISSPC

        [Route("LISSPC")]
        public ActionResult Index()
        {
       
            var model = new List<List<XY_LABEL_CHARTS_CLEAN_STR>>
    {
        new List<XY_LABEL_CHARTS_CLEAN_STR>(),
        new List<XY_LABEL_CHARTS_CLEAN_STR>(),
        new List<XY_LABEL_CHARTS_CLEAN_STR>(),
        new List<XY_LABEL_CHARTS_CLEAN_STR>(),
        new List<XY_LABEL_CHARTS_CLEAN_STR>()
    };
            List<TaskList> taskLists = new List<TaskList>();

            taskLists = LISSPC_BS.GetTasks();
            string TaskNo = taskLists[0].Task.ToString();
            ViewBag.TaskList = taskLists;
            ViewBag.TaskNo = TaskNo;   // already exists

            ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
           
            string DropDownS = "ALL";
            string DPNMarket = "ALL";
       
            List<PART_TEST_LIMITS> Part_Tests = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets = new List<PART_PROPERTY_DATA>();
            List<List<XY_LABEL_CHARTS_CLEAN_STR>> listModels = new List<List<XY_LABEL_CHARTS_CLEAN_STR>>();
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();

            Part_Tests = LISSPC_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);
 
            Part_Markets = LISSPC_BS.GetDDGroupPartPropertyData("MARKET");
            if (DropDownS == "ALL")
                DropDownS = Part_Tests[0].part;


            ViewBag.TaskNo = TaskNo;
            ViewBag.Part_Array = Part_Tests;
            //ViewBag.Selected = "Fix";
            ViewBag.Part_Markets = Part_Markets;
            for (int i = 0; i < Part_Markets.Count; i++)
                if (Part_Markets[i].PropertyValue == DPNMarket)
                    ViewBag.DPNMarket = Part_Markets[i].PropertyValue;
            if (DPNMarket == "ALL")
                ViewBag.DPNMarket = "ALL";

            return View("GetDataAllTest", model);
        }



        [Route("LISSPCDateSelect")]
        public ActionResult GetDataCommon(DateTime startDate, DateTime endDate, String DropDownS, String DPNMarket, String TaskNo, String PageSelected, String ModelName)
        {
            List<TaskList> taskLists = LISSPC_BS.GetTasks();
            ViewBag.TaskList = taskLists;
            ViewBag.TaskNo = TaskNo;


            double STDV = 0, AVGX = 0, SAMPLES, CP, CPU, CPL, CPK;

            double totalDays = 0, USL = 0, LSL = 0;
            List<PART_TEST_LIMITS> Part_Test_Limits = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets = new List<PART_PROPERTY_DATA>();
            List<XY_LABEL_CHARTS_CLEAN_STR> DataQuery, LabelScale = new List<XY_LABEL_CHARTS_CLEAN_STR>();
            List<List<XY_LABEL_CHARTS_CLEAN_STR>> listModels = new List<List<XY_LABEL_CHARTS_CLEAN_STR>>();
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();

            Part_Test_Limits = LISSPC_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);

            if (TaskNo == "1205")
            {
                Part_Models = LISSPC_BS.GetCommonPartPropertyDataByTask("MODEL", TaskNo);
            }
            Part_Markets = LISSPC_BS.GetDDGroupPartPropertyData("MARKET");
            if (DropDownS == "ALL")
                DropDownS = Part_Test_Limits[0].part;
            DataQuery = LISSPC_BS.GetDataXY(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");
            List<XY_LABEL_CHARTS_CLEAN_STR> DataQuerySub1 = new List<XY_LABEL_CHARTS_CLEAN_STR>();


            // New average data series xx
            List<XY_LABEL_CHARTS_CLEAN_STR> DataQueryAvg = LISSPC_BS.GetDataXY_Avg(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");


            // Get Limit Adjust by table "test_result_lis_limit_adjust"
            LIMIT_ADJUST lIMIT_ADJUST = new LIMIT_ADJUST();
            lIMIT_ADJUST = LISSPC_BS.GetLimit_Adjust(DropDownS);

            // Original DataQuery
            ApplyLimitAdjust(DataQuery, DropDownS);

            // New DataQueryAvg
            ApplyLimitAdjust(DataQueryAvg, DropDownS);

            if (DataQuery.Count != 0)
            {
                for (int i = 0; i < DataQuery.Count; i++)
                {
                        DataQuerySub1.Add(DataQuery[i]);

                }
            }

            for (int i = 0; i < Part_Test_Limits.Count; i++)
            {
                if (Part_Test_Limits[i].part == DropDownS)
                {
                    ViewBag.DropdownS = Part_Test_Limits[i].part;
                    ViewBag.PartDesc = Part_Test_Limits[i].part_desc;
                    ViewBag.LowerLim = Part_Test_Limits[i].lower;
                    ViewBag.UpperLim = Part_Test_Limits[i].upper;
                    USL = Convert.ToDouble(Part_Test_Limits[i].upper);
                    LSL = Convert.ToDouble(Part_Test_Limits[i].lower);
                }

            }

            if (DropDownS == "ALL")
            {
                ViewBag.DropdownS = "ALL";
                ViewBag.TypeFAN = "ALL";
                ViewBag.LowerLim = "0";
                ViewBag.UpperLim = "1500";
                USL = 1500;
                LSL = 0;
            }
            ViewBag.TaskNo = TaskNo;
            ViewBag.Part_Array = Part_Test_Limits;
            ViewBag.Selected = PageSelected;
            ViewBag.Part_Markets = Part_Markets;
            for (int i = 0; i < Part_Markets.Count; i++)
                if (Part_Markets[i].PropertyValue == DPNMarket)
                    ViewBag.DPNMarket = Part_Markets[i].PropertyValue;
            if (DPNMarket == "ALL")
                ViewBag.DPNMarket = "ALL";

            ViewBag.Part_Models = Part_Models;
            for (int i = 0; i < Part_Models.Count; i++)
                if (Part_Models[i].PropertyValue == ModelName)
                    ViewBag.DPModelName = Part_Models[i].PropertyValue;
            if (ModelName == "ALL")
                ViewBag.DPModelName = "ALL";

            if (DataQuerySub1.Count != 0)
            {
                var resultData = DataQuerySub1.Select(v => (double)Convert.ToDouble(v.y));
                STDV = LISSPC_BS.CalculateStandardDeviation(resultData);
                AVGX = resultData.Average();
                SAMPLES = resultData.Count();

                CP = (USL - LSL) / (6 * STDV);
                CPU = (USL - AVGX) / (3 * STDV);
                CPL = (AVGX - LSL) / (3 * STDV);
                CPK = Math.Min(CPU, CPL);

                ViewBag.STDV = STDV;
                ViewBag.AVGX = AVGX;
                ViewBag.SAMPLES = SAMPLES;
                ViewBag.CP = CP;
                ViewBag.CPU = CPU;
                ViewBag.CPL = CPL;
                ViewBag.CPK = CPK;

            }


            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            totalDays = (endDate - startDate).TotalDays;
            ViewBag.TotalDays = totalDays;
            int DaysRunS = 0, DaysRunE = 0, MonthsRunS = 0, MonthsRunE = 0;

            MonthsRunS = Convert.ToInt16(startDate.ToString("MM"));
            MonthsRunE = Convert.ToInt16(endDate.ToString("MM"));
            DaysRunS = Convert.ToInt16(startDate.ToString("dd"));
            DaysRunE = Convert.ToInt16(endDate.ToString("dd"));
            LabelScale = LISSPC_BS.GenerateDateLabel(DaysRunS, DaysRunE, MonthsRunS, MonthsRunE);


            if (DataQuerySub1.Count > 0)
            {
                ViewBag.UNIT1 = DataQuerySub1[0].test_unit;
                listModels.Add(DataQuerySub1);

            }

            if (listModels.Count > 0)
            {
                listModels.Add(LabelScale);
            }

            if (DataQueryAvg.Count > 0)
            {
                listModels.Add(DataQueryAvg);
            }





            if (totalDays > 365)
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            else
                return View("GetDataAllTest", listModels);

        }


        // Helpers
        // ===============================
        private static Dictionary<string, Tuple<string, double>> DuplicateCache = new Dictionary<string, Tuple<string, double>>();
        private static Dictionary<string, DateTime> CacheLastTimestamp = new Dictionary<string, DateTime>();
        private static readonly ConcurrentDictionary<string, (string Time, double Value)> _lastSentByChart = new ConcurrentDictionary<string, (string, double)>();
        private void ApplyLimitAdjust(List<XY_LABEL_CHARTS_CLEAN_STR> dataList, string part)
        {
            if (dataList == null || dataList.Count == 0) return;

            var limitAdjust = LISSPC_BS.GetLimit_Adjust(part);

            foreach (var data in dataList)
            {
                string adjustValue = limitAdjust.limit_adjust_value;
                string adjustType = limitAdjust.limit_adjust_type;

           

                if (!string.IsNullOrEmpty(adjustValue))
                {
                    double y = Convert.ToDouble(data.y);
                    double val = Convert.ToDouble(adjustValue);

                    switch (adjustType)
                    {
                        case "MUL":
                            y *= val;
                            data.test_unit += $" ( / {adjustValue} )";
                            break;
                        case "DIV":
                            y /= val;
                            data.test_unit += $" ( x {adjustValue} )";
                            break;
                        case "PLUS":
                            y += val;
                            break;
                        case "MINUS":
                            y -= val;
                            break;
                    }

                    data.y = y.ToString();
                }
            }
        }
        private double ApplyLimitAdjustOne(double data, LIMIT_ADJUST limitAdj)
        {
            if (data > 0)
            {
                double y = data;
                double val = Convert.ToDouble(limitAdj.limit_adjust_value);

                switch (limitAdj.limit_adjust_type)
                {
                    case "MUL": y *= val; break;
                    case "DIV": y /= val; break;
                    case "PLUS": y += val; break;
                    case "MINUS": y -= val; break;
                }

                return y;
            }
            else
                return 0;
        }
        private bool IsDuplicate(string chartId, string time, double value)
        {
            var last = _lastSentByChart.GetOrAdd(chartId, (time, value));

            if (last.Time == time && last.Value == value)
                return true;

            _lastSentByChart[chartId] = (time, value);
            return false;
        }
        // ===============================
    }
}