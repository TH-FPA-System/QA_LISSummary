using QA_LISSummary.Business_logic;
using QA_LISSummary.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QA_LISSummary.Controllers
{
    public class LISSPCController : Controller
    {
       
            private QASumBS QASUM_BS; //Business Logic

            public LISSPCController()
            {
                QASUM_BS = new QASumBS();

            }
        // GET: LISSPC

        [Route("LISSPC")]
        public ActionResult Index()
        {
       
            var model = new List<List<XY_LABEL_CHARTS_STR>>
    {
        new List<XY_LABEL_CHARTS_STR>(),
        new List<XY_LABEL_CHARTS_STR>(),
        new List<XY_LABEL_CHARTS_STR>(),
        new List<XY_LABEL_CHARTS_STR>(),
        new List<XY_LABEL_CHARTS_STR>()
    };

            ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string TaskNo = "4611";
            string DropDownS = "ALL";
            string DPNMarket = "ALL";
       
            List<PART_TEST_LIMITS> Part_Tests = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets = new List<PART_PROPERTY_DATA>();
            List<List<XY_LABEL_CHARTS_STR>> listModels = new List<List<XY_LABEL_CHARTS_STR>>();
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();

            Part_Tests = QASUM_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);
 
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
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
            double STDV = 0, AVGX = 0, SAMPLES, CP, CPU, CPL, CPK;

            double totalDays = 0, USL = 0, LSL = 0;
            List<PART_TEST_LIMITS> Part_Test_Limits = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets = new List<PART_PROPERTY_DATA>();
            List<XY_LABEL_CHARTS_STR> DataQuery, LabelScale = new List<XY_LABEL_CHARTS_STR>();
            List<List<XY_LABEL_CHARTS_STR>> listModels = new List<List<XY_LABEL_CHARTS_STR>>();
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();

            Part_Test_Limits = QASUM_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);

            if (TaskNo == "1205")
            {
                Part_Models = QASUM_BS.GetCommonPartPropertyDataByTask("MODEL", TaskNo);
            }
            else
            {

            }
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            if (DropDownS == "ALL")
                DropDownS = Part_Test_Limits[0].part;
            DataQuery = QASUM_BS.GetDataXY(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");
            List<XY_LABEL_CHARTS_STR> DataQuerySub1 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub2 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub3 = new List<XY_LABEL_CHARTS_STR>();




            // New average data series
            List<XY_LABEL_CHARTS_STR> DataQueryAvg = QASUM_BS.GetDataXY_Avg(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");


            // Get Limit Adjust by table "test_result_lis_limit_adjust"
            LIMIT_ADJUST lIMIT_ADJUST = new LIMIT_ADJUST();
            lIMIT_ADJUST = QASUM_BS.GetLimit_Adjust(DropDownS);

            // Original DataQuery
            ApplyLimitAdjust(DataQuery, DropDownS);

            // New DataQueryAvg
            ApplyLimitAdjust(DataQueryAvg, DropDownS);


            if (DataQuery.Count != 0)
            {
                for (int i = 0; i < DataQuery.Count; i++)
                {

                    if (DataQuery[i].piorSet == "1")
                        DataQuerySub1.Add(DataQuery[i]);
                    else if (DataQuery[i].piorSet == "2")
                        DataQuerySub2.Add(DataQuery[i]);
                    else if (DataQuery[i].piorSet == "3")
                        DataQuerySub3.Add(DataQuery[i]);
                    else
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
                STDV = QASUM_BS.CalculateStandardDeviation(resultData);
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
            LabelScale = QASUM_BS.GenerateDateLabel(DaysRunS, DaysRunE, MonthsRunS, MonthsRunE);


            if (DataQuerySub1.Count > 0)
            {
                ViewBag.UNIT1 = DataQuerySub1[0].unit;
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


            //SUB VIEW NOT USE NOW, FINDING SOLUTION FOR 
            if (DataQuerySub2.Count > 0)
            {
                ViewBag.UNIT2 = DataQuerySub2[0].unit;
                listModels.Add(DataQuerySub2);
                ViewBag.LowerLim2 = DataQuerySub2[0].limit_adjust_lw2;
                ViewBag.UpperLim2 = DataQuerySub2[0].limit_adjust_up2;
            }

            if (DataQuerySub3.Count > 0)
            {
                ViewBag.UNIT3 = DataQuerySub3[0].unit;
                listModels.Add(DataQuerySub3);
            }



            if (totalDays > 365)
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            else
                return View("GetDataAllTest", listModels);

        }


        private static Dictionary<string, Tuple<string, double>> DuplicateCache = new Dictionary<string, Tuple<string, double>>();
        private static Dictionary<string, DateTime> CacheLastTimestamp = new Dictionary<string, DateTime>();


        // ===============================
        // Helpers
        // ===============================
        private static readonly ConcurrentDictionary<string, (string Time, double Value)>
    _lastSentByChart = new ConcurrentDictionary<string, (string, double)>();

        private void ApplyLimitAdjust(List<XY_LABEL_CHARTS_STR> dataList, string part)
        {
            if (dataList == null || dataList.Count == 0) return;

            var limitAdjust = QASUM_BS.GetLimit_Adjust(part);

            foreach (var data in dataList)
            {
                string adjustValue = limitAdjust.limit_adjust_value;
                string adjustType = limitAdjust.limit_adjust_type;

                if (!string.IsNullOrEmpty(adjustValue))
                {
                    data.limit_adjust1 = adjustValue;
                    data.limit_adjust_type1 = adjustType;
                }

                if (!string.IsNullOrEmpty(data.limit_adjust1))
                {
                    double y = Convert.ToDouble(data.y);
                    double val = Convert.ToDouble(data.limit_adjust1);

                    switch (data.limit_adjust_type1)
                    {
                        case "MUL":
                            y *= val;
                            data.unit += $" ( / {data.limit_adjust1} )";
                            break;
                        case "DIV":
                            y /= val;
                            data.unit += $" ( x {data.limit_adjust1} )";
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

    }
}