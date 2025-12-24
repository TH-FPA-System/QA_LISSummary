using QA_LISSummary.Business_logic;
using QA_LISSummary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Concurrent;
using Microsoft.Ajax.Utilities; // <-- add this
namespace DDCFASerialController.Controllers
{

    [RoutePrefix("")]
    public class QASumController : Controller
    {
        private QASumBS QASUM_BS; //Business Logic

        public QASumController()
        {
            QASUM_BS = new QASumBS();

        }



        //Result PART-TEST ROOT
        [Route("")]
        public ActionResult GetManualPartTest()
        {
            double totalDays = 0;


            List<LABEL_VALUE_CHARTS_STR> PartTestData = new List<LABEL_VALUE_CHARTS_STR>();

            List<List<LABEL_VALUE_CHARTS_STR>> listModels = new List<List<LABEL_VALUE_CHARTS_STR>>();
            List<LABEL_VALUE_CHARTS_STR> ResultTestPass = new List<LABEL_VALUE_CHARTS_STR>();
            List<LABEL_VALUE_CHARTS_STR> ResultTestFail = new List<LABEL_VALUE_CHARTS_STR>();
            List<PART_PROPERTY_DATA> Part_Markets, Part_DDPhase = new List<PART_PROPERTY_DATA>();
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            Part_Models = QASUM_BS.GetCommonPartPropertyDataByTask("MODEL", "0");
            Part_DDPhase = QASUM_BS.GetDDGroupPartPropertyData("PROJECT");
            PartTestData = QASUM_BS.GetResultPartTests(DateTime.Now, DateTime.Now.AddDays(1), "", "", "", "ALL", "");





            for (int i = 0; i < PartTestData.Count; i++)
            {
                if (PartTestData[i].Status == "P")
                    ResultTestPass.Add(PartTestData[i]);
                else
                    ResultTestFail.Add(PartTestData[i]);
            }


            ViewBag.TaskNo = "";
            ViewBag.PartTestsNo = "";
            ViewBag.Part_Markets = Part_Markets;
            ViewBag.Part_Models = Part_Models;
            ViewBag.DPNMarket = "ALL";
            ViewBag.Selected = "Summary By Test";

            ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            totalDays = (DateTime.Now.AddDays(1) - DateTime.Now).TotalDays;
            ViewBag.TotalDays = totalDays;
            listModels.Add(ResultTestPass);
            listModels.Add(ResultTestFail);

            if (totalDays > 365)
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            else
                return View("ResultPartTest", listModels);

        }


        //Result PART-TEST
        [Route("ResultPartTest")]
        public ActionResult GetManualPartTest(DateTime startDate, DateTime endDate, String TaskNo, String DPNMarket, String DPNPhase, String PartTestsNo)
        {
            double totalDays = 0;


            List<LABEL_VALUE_CHARTS_STR> PartTestData = new List<LABEL_VALUE_CHARTS_STR>();
            List<List<LABEL_VALUE_CHARTS_STR>> listModels = new List<List<LABEL_VALUE_CHARTS_STR>>();

            List<LABEL_VALUE_CHARTS_STR> LabelHeader = new List<LABEL_VALUE_CHARTS_STR>();
            List<LABEL_VALUE_CHARTS_STR> ResultTestPass = new List<LABEL_VALUE_CHARTS_STR>();


            List<LABEL_VALUE_CHARTS_STR> ResultTestFail = new List<LABEL_VALUE_CHARTS_STR>();
            List<PART_PROPERTY_DATA> Part_Markets, Part_DDPhase = new List<PART_PROPERTY_DATA>();
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            Part_DDPhase = QASUM_BS.GetDDGroupPartPropertyData("PROJECT");
            if (PartTestsNo == "")
            {
                LabelHeader = QASUM_BS.GetLabelHeader(startDate, endDate, TaskNo, PartTestsNo, "", "", "");
                ResultTestPass = QASUM_BS.GetResultNew(startDate, endDate, TaskNo, PartTestsNo, "", "P", "");
                ResultTestFail = QASUM_BS.GetResultNew(startDate, endDate, TaskNo, PartTestsNo, "", "F", "");
            }
            else
            {
                LabelHeader = QASUM_BS.GetLabelHeader(startDate, endDate, TaskNo, PartTestsNo, "", "", "");
                ResultTestPass = QASUM_BS.GetResultNew(startDate, endDate, TaskNo, PartTestsNo, "", "P", "");
                ResultTestFail = QASUM_BS.GetResultNew(startDate, endDate, TaskNo, PartTestsNo, "", "F", "");

            }

            ResultTestPass = ResultTestPass.OrderBy(o => o.part_no).ToList();
            ResultTestFail = ResultTestFail.OrderBy(o => o.part_no).ToList();
            LabelHeader = LabelHeader.OrderBy(o => o.part_no).ToList();

            ViewBag.Selected = "Summary By Test";
            ViewBag.TaskNo = TaskNo;
            ViewBag.PartTestsNo = PartTestsNo;
            ViewBag.Part_Markets = Part_Markets;
            for (int i = 0; i < Part_Markets.Count; i++)
                if (Part_Markets[i].PropertyValue == DPNMarket)
                    ViewBag.DPNMarket = Part_Markets[i].PropertyValue;

            ViewBag.Part_DDPhase = Part_DDPhase;
            for (int i = 0; i < Part_DDPhase.Count; i++)
                if (Part_DDPhase[i].PropertyValue == DPNPhase)
                    ViewBag.DPNPhase = Part_DDPhase[i].PropertyValue;

            if (DPNMarket == "ALL")
                ViewBag.DPNMarket = "ALL";

            if (DPNPhase == "ALL")
                ViewBag.DPNPhase = "ALL";

            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            totalDays = (endDate - startDate).TotalDays;
            ViewBag.TotalDays = totalDays;
            listModels.Add(ResultTestPass);
            listModels.Add(ResultTestFail);
            if (LabelHeader.Count > 0)
                listModels.Add(LabelHeader);

            if (totalDays > 365)
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            else
                return View("ResultPartTest", listModels);

        }

        [Route("ResultPartTestDetailGrid")]
        public ActionResult GetResultPartTestDetail(DateTime startDate, DateTime endDate, string TaskNo, string DPNMarket, string Status, string PartTestsNo)
        {
            double totalDays = (endDate - startDate).TotalDays;

            // Assign values to ViewBag
            ViewBag.TaskNo = TaskNo;
            ViewBag.Status = Status;
            ViewBag.PartTestsNo = PartTestsNo;
            ViewBag.Selected = "Test Result";
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

            // Check date range
            if (totalDays > 90)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 90 Days!!'); history.back();</script>");
            }

            // Check for null or empty parameters
            if (string.IsNullOrWhiteSpace(TaskNo) &&
                string.IsNullOrWhiteSpace(DPNMarket) &&
                string.IsNullOrWhiteSpace(Status) &&
                string.IsNullOrWhiteSpace(PartTestsNo))
            {
                // Return empty view if all inputs are empty
                ViewBag.Message = "No search criteria provided.";
                return View("ResultPartTestDetailGrid");
            }

            // Proceed with SQL query here
            // var data = YourRepository.GetData(TaskNo, DPNMarket, Status, PartTestsNo, startDate, endDate);

            return View("ResultPartTestDetailGrid" /*, data*/);
        }

        [Route("GetDataTestR")]
        public JsonResult GetDataTestR(DateTime startDate, DateTime endDate,
      string TaskNo, string DPNMarket, string Status, string PartTestsNo)
        {
            ViewBag.TaskNo = TaskNo;
            ViewBag.PartTestsNo = PartTestsNo;
            ViewBag.Selected = "Summary Bar Charts Detail";
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

            // NULL / EMPTY CHECK
            if (string.IsNullOrWhiteSpace(TaskNo) &&
                string.IsNullOrWhiteSpace(DPNMarket) &&
                string.IsNullOrWhiteSpace(Status) &&
                string.IsNullOrWhiteSpace(PartTestsNo))
            {
                return Json(new List<TEST_RESULT_DETAIL>(), JsonRequestBehavior.AllowGet);
            }

            // Query SQL
            List<TEST_RESULT_DETAIL> PartTestDataDetail =
                QASUM_BS.GetResultPartTestsDetail(startDate, endDate, TaskNo, PartTestsNo, Status, "", "");

            return new JsonResult
            {
                Data = PartTestDataDetail,
                MaxJsonLength = Int32.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }



        // Result TASK-TEST 05-11-2025
        [Route("ResultTaskTest")]
        public ActionResult GetManualTaskTest(DateTime startDate, DateTime endDate, string TaskNo, string DPNMarket, string DPNPhase, string PartTestsNo)
        {
            double totalDays = (endDate - startDate).TotalDays;

            // --- Create lists ---
            List<LABEL_VALUE_CHARTS_STR> LabelHeader = new List<LABEL_VALUE_CHARTS_STR>();
            List<LABEL_VALUE_CHARTS_STR> ResultTestPass = new List<LABEL_VALUE_CHARTS_STR>();
            List<LABEL_VALUE_CHARTS_STR> ResultTestFail = new List<LABEL_VALUE_CHARTS_STR>();
            List<List<LABEL_VALUE_CHARTS_STR>> listModels = new List<List<LABEL_VALUE_CHARTS_STR>>();

            // --- Fetch data from business logic layer ---
            ResultTestPass = QASUM_BS.GetResultTask(startDate, endDate, TaskNo, "P", "");
            ResultTestFail = QASUM_BS.GetResultTask(startDate, endDate, TaskNo, "F", "");

            // --- Handle empty results safely ---
            if (ResultTestPass.Count == 0)
                ResultTestPass.Add(new LABEL_VALUE_CHARTS_STR { label = "PASS", value = "0", Status = "P", link = "", part_no = "" });

            if (ResultTestFail.Count == 0)
                ResultTestFail.Add(new LABEL_VALUE_CHARTS_STR { label = "FAIL", value = "0", Status = "F", link = "", part_no = "" });

            // --- Build Label Header ---
            LabelHeader.Add(new LABEL_VALUE_CHARTS_STR
            {
                label = ResultTestPass[0].label,
                value = "0",
                link = "",
                part_no = "0000"
            });

            LabelHeader = LabelHeader.OrderBy(o => o.part_no).ToList();

            // --- Safe total calculation ---
            int passValue = 0;
            int failValue = 0;

            int.TryParse(ResultTestPass[0].value, out passValue);
            int.TryParse(ResultTestFail[0].value, out failValue);

            ViewBag.PassCount = passValue;
            ViewBag.FailCount = failValue;
            ViewBag.Total = passValue + failValue;
            ViewBag.TaskNo = TaskNo;
            ViewBag.Selected = "Summary By Task";

            // --- Market / Phase filters ---
            ViewBag.DPNMarket = string.IsNullOrEmpty(DPNMarket) ? "ALL" : DPNMarket;
            ViewBag.DPNPhase = string.IsNullOrEmpty(DPNPhase) ? "ALL" : DPNPhase;

            // --- Dates ---
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            ViewBag.TotalDays = totalDays;

            // --- Combine results for view ---
            listModels.Add(ResultTestPass);
            listModels.Add(ResultTestFail);
            listModels.Add(LabelHeader);

            // --- Validate range ---
            if (totalDays > 365)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            }
            else
            {
                return View("ResultTaskTest", listModels);
            }
        }


        //COMMON UI
        [Route("GetDataAllTest")]
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

        // POP 21-11-2024
        [Route("GetDataAllTestParameter")]
        public ActionResult GetDataParameter(DateTime startDate, DateTime endDate, String DropDownS, String DPNMarket, String TaskNo, String PageSelected)
        {
            double STDV = 0, AVGX = 0, SAMPLES, CP, CPU, CPL, CPK;

            double totalDays = 0, USL = 0, LSL = 0;
            List<PART_TEST_LIMITS> Part_Test_Limits = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets = new List<PART_PROPERTY_DATA>();
            List<XY_LABEL_CHARTS_STR> DataQuery, LabelScale = new List<XY_LABEL_CHARTS_STR>();
            List<List<XY_LABEL_CHARTS_STR>> listModels = new List<List<XY_LABEL_CHARTS_STR>>();

            Part_Test_Limits = QASUM_BS.GetPartTestLimitsByParameter();
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            if (DropDownS == "ALL")
                DropDownS = Part_Test_Limits[0].part;
            DataQuery = QASUM_BS.GetDataXYPara(startDate, endDate, DropDownS, DPNMarket, "", "", "");
            List<XY_LABEL_CHARTS_STR> DataQuerySub1 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub2 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub3 = new List<XY_LABEL_CHARTS_STR>();

            // Get Limit Adjust by table "test_result_lis_limit_adjust"
            LIMIT_ADJUST lIMIT_ADJUST = new LIMIT_ADJUST();
            lIMIT_ADJUST = QASUM_BS.GetLimit_Adjust(DropDownS);

            if (DataQuery.Count != 0)
            {
                for (int i = 0; i < DataQuery.Count; i++)
                {
                    if (lIMIT_ADJUST.limit_adjust_value != null && lIMIT_ADJUST.limit_adjust_value != "")
                    {
                        DataQuery[i].limit_adjust1 = lIMIT_ADJUST.limit_adjust_value;
                        DataQuery[i].limit_adjust_type1 = lIMIT_ADJUST.limit_adjust_type;

                        if (DataQuery[i].limit_adjust_type1 == "MUL")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) * Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( / " + DataQuery[i].limit_adjust1 + " )";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "DIV")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) / Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( x " + DataQuery[i].limit_adjust1 + ") ";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "MINUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) - Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                        else if (DataQuery[i].limit_adjust_type1 == "PLUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) + Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                    }
                    else // GET BY TEST RESULT LIS 
                    {
                        if (DataQuery[i].limit_adjust_type1 == "MUL")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) * Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( / " + DataQuery[i].limit_adjust1 + " )";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "DIV")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) / Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( x " + DataQuery[i].limit_adjust1 + ") ";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "MINUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) - Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                        else if (DataQuery[i].limit_adjust_type1 == "PLUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) + Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                    }
                }
            }
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
            listModels.Add(LabelScale);

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
                return View("GetDataParameter", listModels);

        }

        [Route("GetCapaHisto")]
        public ActionResult GetCapaHisto(DateTime startDate, DateTime endDate, String DropDownS, String DPNMarket, String TaskNo, String PageSelected)
        {
            double STDV = 0, AVGX = 0, SAMPLES, CP, CPU, CPL, CPK;

            double totalDays = 0, USL = 0, LSL = 0;
            List<PART_TEST_LIMITS> Part_Test_Limits = new List<PART_TEST_LIMITS>();
            List<PART_PROPERTY_DATA> Part_Markets, Part_DDPhase = new List<PART_PROPERTY_DATA>();
            List<XY_CHARTS_STR> CapHistoData = new List<XY_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataSum, DataSumFinal = new List<XY_LABEL_CHARTS_STR>();
            List<List<XY_CHARTS_STR>> listModels = new List<List<XY_CHARTS_STR>>();
            List<XY_CHARTS_STR> ColumnLimits = new List<XY_CHARTS_STR>();
            XY_CHARTS_STR LowerLimits = new XY_CHARTS_STR();
            XY_CHARTS_STR UpperLimits = new XY_CHARTS_STR();
            XY_CHARTS_STR SkipRun = new XY_CHARTS_STR();
            List<XY_CHARTS_STR> ListColumnLimits = new List<XY_CHARTS_STR>();


            Part_Test_Limits = QASUM_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);
            Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            CapHistoData = QASUM_BS.GetCapaHistoValueRaw(startDate, endDate, DropDownS, DPNMarket, TaskNo);

            DataSum = QASUM_BS.GetDataXY(startDate, endDate, DropDownS, DPNMarket, "", "", "");

            // Get Limit Adjust by table "test_result_lis_limit_adjust"
            LIMIT_ADJUST lIMIT_ADJUST = new LIMIT_ADJUST();
            lIMIT_ADJUST = QASUM_BS.GetLimit_Adjust(DropDownS);

            if (DataSum.Count != 0)
            {
                for (int i = 0; i < DataSum.Count; i++)
                {
                    if (lIMIT_ADJUST.limit_adjust_value != null && lIMIT_ADJUST.limit_adjust_value != "")
                    {

                        DataSum[i].limit_adjust1 = lIMIT_ADJUST.limit_adjust_value;
                        DataSum[i].limit_adjust_type1 = lIMIT_ADJUST.limit_adjust_type;
                        if (DataSum[i].limit_adjust_type1 == "MUL")
                        {
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) * Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                            DataSum[i].unit = DataSum[i].unit + " ( / " + DataSum[i].limit_adjust1 + " )";
                        }
                        else if (DataSum[i].limit_adjust_type1 == "DIV")
                        {
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) / Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                            DataSum[i].unit = DataSum[i].unit + " ( x " + DataSum[i].limit_adjust1 + ") ";
                        }
                        else if (DataSum[i].limit_adjust_type1 == "MINUS")
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) - Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                        else if (DataSum[i].limit_adjust_type1 == "PLUS")
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) + Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();

                    }
                    else // GET BY TEST RESULT LIS 
                    {
                        if (DataSum[i].limit_adjust_type1 == "MUL")
                        {
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) * Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                            DataSum[i].unit = DataSum[i].unit + " ( / " + DataSum[i].limit_adjust1 + " )";
                        }
                        else if (DataSum[i].limit_adjust_type1 == "DIV")
                        {
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) / Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                            DataSum[i].unit = DataSum[i].unit + " ( x " + DataSum[i].limit_adjust1 + ") ";
                        }
                        else if (DataSum[i].limit_adjust_type1 == "MINUS")
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) - Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                        else if (DataSum[i].limit_adjust_type1 == "PLUS")
                            DataSum[i].y = (Convert.ToDouble(DataSum[i].y) + Convert.ToDouble(DataSum[i].limit_adjust1)).ToString();
                    }
                }
            }

            if (DataSum.Count != 0)
            {
                for (int i = 0; i < DataSum.Count; i++)
                {

                    if (DataSum[i].piorSet == "1")
                        DataSumFinal.Add(DataSum[i]);
                    else if (DataSum[i].piorSet == null || DataSum[i].piorSet == "" || DataSum[i].piorSet == "0")
                        DataSumFinal.Add(DataSum[i]);

                }
            }

            for (int i = 0; i < Part_Test_Limits.Count; i++)
            {
                if (Part_Test_Limits[i].part == DropDownS)
                {
                    ViewBag.DropdownS = Part_Test_Limits[i].part;
                    ViewBag.TypeFAN = Part_Test_Limits[i].part_desc;
                    ViewBag.LowerLim = Part_Test_Limits[i].lower;
                    ViewBag.UpperLim = Part_Test_Limits[i].upper;

                    USL = Convert.ToDouble(Part_Test_Limits[i].upper);
                    LSL = Convert.ToDouble(Part_Test_Limits[i].lower);
                }

            }

            if (DataSumFinal.Count != 0)
            {


                var resultData = DataSumFinal.Select(v => (double)Convert.ToDouble(v.y));

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

            ViewBag.MaxY = USL + LSL;
            ViewBag.TaskNo = TaskNo;
            double tempLWlmt = 0, tempUPlmt = 0;
            tempLWlmt = LSL;
            tempUPlmt = USL;
            LowerLimits.x = LSL.ToString();
            LowerLimits.y = "50";
            UpperLimits.x = USL.ToString();
            UpperLimits.y = "50";

            CapHistoData.Add(LowerLimits);
            CapHistoData.Add(UpperLimits);
            ViewBag.Selected = PageSelected;
            ViewBag.Part_Array = Part_Test_Limits;

            ViewBag.Part_Markets = Part_Markets;
            for (int i = 0; i < Part_Markets.Count; i++)
                if (Part_Markets[i].PropertyValue == DPNMarket)
                    ViewBag.DPNMarket = Part_Markets[i].PropertyValue;

            if (DPNMarket == "ALL")
                ViewBag.DPNMarket = "ALL";

            SkipRun.x = "";
            SkipRun.y = "0";
            double tempLw = 0, tempUp = 0;
            int lwchk = 0, upchk = 0;
            int tempPositionILW = 98, tempPositionIUP = 99;

            for (int i = 0; i < CapHistoData.Count; i++)
            {

                // LIMIT ADJUST SET
                if (lIMIT_ADJUST.limit_adjust_value != null && lIMIT_ADJUST.limit_adjust_value != "")
                {


                    if (lIMIT_ADJUST.limit_adjust_type == "MUL")
                    {
                        CapHistoData[i].x = (Convert.ToDouble(CapHistoData[i].x) * Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value)).ToString();
                        if (i == 0)
                        {
                            tempLw = LSL * Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);
                            tempUp = USL * Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);

                        }
                    }
                    else if (lIMIT_ADJUST.limit_adjust_type == "DIV")
                    {
                        CapHistoData[i].x = (Convert.ToDouble(CapHistoData[i].x) / Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value)).ToString();
                        if (i == 0)
                        {
                            tempLw = LSL / Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);
                            tempUp = USL / Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);

                        }
                    }
                    else if (lIMIT_ADJUST.limit_adjust_type == "MINUS")
                    {
                        CapHistoData[i].x = (Convert.ToDouble(CapHistoData[i].x) - Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value)).ToString();
                        if (i == 0)
                        {
                            tempLw = LSL - Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);
                            tempUp = USL - Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);

                        }
                    }
                    else if (lIMIT_ADJUST.limit_adjust_type == "PLUS")
                    {
                        CapHistoData[i].x = (Convert.ToDouble(CapHistoData[i].x) + Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value)).ToString();
                        if (i == 0)
                        {
                            tempLw = LSL + Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);
                            tempUp = USL + Convert.ToDouble(lIMIT_ADJUST.limit_adjust_value);

                        }
                    }


                }//LIMIT ADJUST SET
                else
                {
                    if (i == 0)
                    {
                        tempLw = LSL;
                        tempUp = USL;
                    }
                }

                if (CapHistoData[i].x == tempLw.ToString() && CapHistoData[i].y == "50" && (lwchk == 0))
                {
                    LowerLimits.x = tempLWlmt.ToString();
                    ListColumnLimits.Add(LowerLimits);
                    ListColumnLimits[ListColumnLimits.Count - 1].y = "50";
                    tempPositionILW = i;

                    lwchk++;
                }
                else if (CapHistoData[i].x == tempUp.ToString() && CapHistoData[i].y == "50" && (upchk == 0))
                {
                    UpperLimits.x = tempUPlmt.ToString();
                    ListColumnLimits.Add(UpperLimits);
                    ListColumnLimits[ListColumnLimits.Count - 1].y = "50";
                    tempPositionIUP = i;

                    upchk++;
                }
                else
                    ListColumnLimits.Add(SkipRun);
            }
            if ((tempPositionILW != 0 && tempPositionIUP != 0) && (tempPositionILW != 98 && tempPositionIUP != 99))
            {
                CapHistoData.RemoveAt(tempPositionILW);
                CapHistoData.RemoveAt(tempPositionIUP - 1);
            }


            if (CapHistoData.Count != 0)
            {


                CapHistoData = CapHistoData.OrderBy(o => Convert.ToDouble(o.x)).ToList();

            }


            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            totalDays = (endDate - startDate).TotalDays;
            ViewBag.TotalDays = totalDays;

            listModels.Add(CapHistoData);

            listModels.Add(ListColumnLimits);

            if (totalDays > 365)
                return Content("<script language='javascript' type='text/javascript'>alert('Data more than 365 Days!!'); history.back() </script>");
            else
                return View("GetCapaHisto", listModels);

        }

        public JsonResult GetSubCharts(DateTime startDate, DateTime endDate, String DropDownS)
        {
            List<XY_LABEL_CHARTS_STR> DataQuery, LabelScale = new List<XY_LABEL_CHARTS_STR>();
            List<List<XY_LABEL_CHARTS_STR>> listModels = new List<List<XY_LABEL_CHARTS_STR>>();


            DataQuery = QASUM_BS.GetDataXY(startDate, endDate, DropDownS, "ALL", "", "", "");
            List<XY_LABEL_CHARTS_STR> DataQuerySub1 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub2 = new List<XY_LABEL_CHARTS_STR>();
            List<XY_LABEL_CHARTS_STR> DataQuerySub3 = new List<XY_LABEL_CHARTS_STR>();

            // Get Limit Adjust by table "test_result_lis_limit_adjust"
            LIMIT_ADJUST lIMIT_ADJUST = new LIMIT_ADJUST();
            lIMIT_ADJUST = QASUM_BS.GetLimit_Adjust(DropDownS);
            if (DataQuery.Count != 0)
            {
                for (int i = 0; i < DataQuery.Count; i++)
                {
                    if (lIMIT_ADJUST.limit_adjust_value != null && lIMIT_ADJUST.limit_adjust_value != "")
                    {
                        DataQuery[i].limit_adjust1 = lIMIT_ADJUST.limit_adjust_value;
                        DataQuery[i].limit_adjust_type1 = lIMIT_ADJUST.limit_adjust_type;
                        if (DataQuery[i].limit_adjust_type1 == "MUL")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) * Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( / " + DataQuery[i].limit_adjust1 + " )";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "DIV")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) / Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( x " + DataQuery[i].limit_adjust1 + ") ";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "MINUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) - Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                        else if (DataQuery[i].limit_adjust_type1 == "PLUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) + Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                    }
                    else // GET BY TEST RESULT LIS
                    {
                        if (DataQuery[i].limit_adjust_type1 == "MUL")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) * Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( / " + DataQuery[i].limit_adjust1 + " )";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "DIV")
                        {
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) / Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                            DataQuery[i].unit = DataQuery[i].unit + " ( x " + DataQuery[i].limit_adjust1 + ") ";
                        }
                        else if (DataQuery[i].limit_adjust_type1 == "MINUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) - Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                        else if (DataQuery[i].limit_adjust_type1 == "PLUS")
                            DataQuery[i].y = (Convert.ToDouble(DataQuery[i].y) + Convert.ToDouble(DataQuery[i].limit_adjust1)).ToString();
                    }
                }
            }

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
            if (DataQuerySub1.Count != 0)
            {
                var resultData = DataQuerySub1.Select(v => (double)Convert.ToDouble(v.y));

            }

            var jsonResult = Json(DataQuerySub1, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }


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


        // Charts Page
        [Route("Charts")]
        public ActionResult Charts()
        {
            // Example: You can pass initial data if needed
            ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            return View("Charts"); // Ensure /Views/QASum/Charts.cshtml exists
        }

        [Route("GetDataAllTestJson")]
        public JsonResult GetDataAllTestJson(DateTime startDate, DateTime endDate, string DropDownS, string DPNMarket, string TaskNo, string PageSelected, string ModelName)
        {
            double STDV = 0, AVGX = 0, SAMPLES = 0, CP = 0, CPU = 0, CPL = 0, CPK = 0;

            double USL = 0, LSL = 0;
            List<PART_TEST_LIMITS> Part_Test_Limits = QASUM_BS.GetPartTestLimitsByTaskAndTestResultLIS(TaskNo);
            List<PART_PROPERTY_DATA> Part_Markets = QASUM_BS.GetDDGroupPartPropertyData("MARKET");
            List<PART_PROPERTY_DATA> Part_Models = new List<PART_PROPERTY_DATA>();

            if (DropDownS == "ALL")
                DropDownS = Part_Test_Limits[0].part;

            // Fetch data
            var DataQuery = QASUM_BS.GetDataXY(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");
            var DataQueryAvg = QASUM_BS.GetDataXY_Avg(startDate, endDate, DropDownS, DPNMarket, TaskNo, ModelName, "");

            // Limit adjustments
            LIMIT_ADJUST limitAdjust = QASUM_BS.GetLimit_Adjust(DropDownS);
            ApplyLimitAdjust(DataQuery, DropDownS);
            ApplyLimitAdjust(DataQueryAvg, DropDownS);

            // Split DataQuery by priorSet
            var DataSub1 = DataQuery.Where(d => (d.piorSet == "1" || d.piorSet == "0") || string.IsNullOrEmpty(d.piorSet)).ToList();
            var DataSub2 = DataQuery.Where(d => d.piorSet == "2").ToList();
            var DataSub3 = DataQuery.Where(d => d.piorSet == "3").ToList();

            // Calculate statistics for sub1
            if (DataSub1.Count > 0)
            {
                var values = DataSub1.Select(v => Convert.ToDouble(v.y));
                STDV = QASUM_BS.CalculateStandardDeviation(values);
                AVGX = values.Average();
                SAMPLES = values.Count();
                string upperStr = Part_Test_Limits.FirstOrDefault(p => p.part == DropDownS)?.upper;
                USL = Convert.ToDouble(string.IsNullOrEmpty(upperStr) ? "0" : upperStr);

                string lowerStr = Part_Test_Limits.FirstOrDefault(p => p.part == DropDownS)?.lower;
                LSL = Convert.ToDouble(string.IsNullOrEmpty(lowerStr) ? "0" : lowerStr);



                CP = (USL - LSL) / (6 * STDV);
                CPU = (USL - AVGX) / (3 * STDV);
                CPL = (AVGX - LSL) / (3 * STDV);
                CPK = Math.Min(CPU, CPL);
            }

            // Generate date labels
            int DaysRunS = startDate.Day, DaysRunE = endDate.Day;
            int MonthsRunS = startDate.Month, MonthsRunE = endDate.Month;
            var LabelScale = QASUM_BS.GenerateDateLabel(DaysRunS, DaysRunE, MonthsRunS, MonthsRunE);

            // Prepare result object
            var result = new
            {
                DataSub1,
                DataSub2,
                DataSub3,
                DataAvg = DataQueryAvg,
                LabelScale,
                Stats = new
                {
                    STDV,
                    AVGX,
                    SAMPLES,
                    CP,
                    CPU,
                    CPL,
                    CPK,
                    USL,
                    LSL
                },
                TaskNo,
                DropDownS,
                DPNMarket,
                ModelName
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Route("CheckPartTests")]
        public ActionResult CheckPartTests()
        {
            var dataTestParts = QASUM_BS.GetPartTests();
            return View("CheckPartTests", dataTestParts); // <-- IMPORTANT
        }

        // ---------------- API (Real-time) ----------------

        [Route("RealTimeChartsDbBatch")]
        public ActionResult RealTimeChartsDbBatch()
        {
            return View("RealTimeChartsDbBatch");
        }

        // ===============================
        // Thread-safe duplicate protection
        // ===============================
        private static readonly ConcurrentDictionary<string, (string Time, double Value)>
            _lastSentByChart = new ConcurrentDictionary<string, (string, double)>();



        // ===============================
        // BATCH API (REAL-TIME)
        // ===============================
        [Route("GetRealtimeDbDataBatch")]
        public ActionResult GetRealtimeDbDataBatch(string testParts, string chartIds)
        {
            if (string.IsNullOrWhiteSpace(testParts) || string.IsNullOrWhiteSpace(chartIds))
                return new HttpStatusCodeResult(400);

            string[] parts = testParts.Split(',').Select(p => p.Trim()).ToArray();
            string[] charts = chartIds.Split(',').Select(c => c.Trim()).ToArray();

            if (parts.Length != charts.Length)
                return new HttpStatusCodeResult(400);

            // -----------------------------
            // Prepare last timestamp dictionary
            var lastTimestamps = new Dictionary<string, DateTime>();
            foreach (var part in parts)
            {
                DateTime last;
                if (CacheLastTimestamp.TryGetValue(part, out last))
                    lastTimestamps[part] = last;
            }

            // -----------------------------
            // Fetch real-time data
            var results = QASUM_BS.GetDataXYRealTimeBatch(parts, lastTimestamps);
            if (results == null) results = new List<XY_LABEL_CHARTS_STR_REALTIME>();

            var response = new List<object>();

            foreach (var lastData in results)
            {
                if (string.IsNullOrWhiteSpace(lastData.test_part) || string.IsNullOrWhiteSpace(lastData.y))
                    continue;

                int chartIndex = Array.FindIndex(parts, p => string.Equals(p, lastData.test_part.Trim(), StringComparison.OrdinalIgnoreCase));
                if (chartIndex < 0) continue;

                string chartId = charts[chartIndex];

                ApplyLimitAdjust(lastData);

                double currentValue;
                if (!double.TryParse(lastData.y.Trim(), out currentValue))
                    continue;

                // -----------------------------
                // Skip duplicates
                Tuple<string, double> lastPoint;
                if (DuplicateCache.TryGetValue(chartId, out lastPoint))
                {
                    if (lastPoint.Item1 == lastData.label && lastPoint.Item2 == currentValue)
                        continue;
                }

                DuplicateCache[chartId] = Tuple.Create(lastData.label, currentValue);

                // -----------------------------
                // Prepare JSON response
                response.Add(new
                {
                    chartId,
                    Time = lastData.label,
                    Value = currentValue,
                    unit = lastData.unit,
                    lower = lastData.lower,
                    upper = lastData.upper,
                    partDesc = lastData.partDesc
                });

                // Update last timestamp cache
                CacheLastTimestamp[lastData.test_part.Trim()] = lastData.date_tested;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // -----------------------------
        // In-memory caches
        // -----------------------------
        private static Dictionary<string, Tuple<string, double>> DuplicateCache = new Dictionary<string, Tuple<string, double>>();
        private static Dictionary<string, DateTime> CacheLastTimestamp = new Dictionary<string, DateTime>();


        // ===============================
        // Helpers
        // ===============================
        private void ApplyLimitAdjust(XY_LABEL_CHARTS_STR_REALTIME data)
        {
            if (string.IsNullOrEmpty(data.limit_adjust_value))
                return;

            if (!double.TryParse(data.y, out double val))
                return;

            double adj = Convert.ToDouble(data.limit_adjust_value);

            switch (data.limit_adjust_type)
            {
                case "MUL": val *= adj; break;
                case "DIV": val /= adj; break;
                case "PLUS": val += adj; break;
                case "MINUS": val -= adj; break;
            }

            data.y = val.ToString();
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


