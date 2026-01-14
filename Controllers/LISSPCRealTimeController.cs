using QA_LISSummary.Business_logic;
using QA_LISSummary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static QA_LISSummary.Models.LISSPCModels;

namespace QA_LISSummary.Controllers
{
    public class LISSPCRealTimeController : Controller
    {

        private LISSPCBS LISSPC_BS; //Business Logic

        public LISSPCRealTimeController()
        {
            LISSPC_BS = new LISSPCBS();

        }




        // ---------------- API (Real-time) ----------------

        [Route("LISSPCRealTime")]
        public ActionResult RealTimeChartsDbBatch()
        {
            return View("RealTimeChartsDbBatch");
        }

        // ===============================
        // BATCH API (REAL-TIME)
        // ===============================
        [Route("LISSPCRealTimeData")]
        public ActionResult GetRealtimeDbDataBatch(string testParts, string chartIds)
        {
            // 1️⃣ Validate input
            if (string.IsNullOrWhiteSpace(testParts) || string.IsNullOrWhiteSpace(chartIds))
                return new HttpStatusCodeResult(400);

            string[] parts = testParts.Split(',').Select(p => p.Trim()).ToArray();
            string[] charts = chartIds.Split(',').Select(c => c.Trim()).ToArray();

            if (parts.Length != charts.Length)
                return new HttpStatusCodeResult(400);

            // 2️⃣ Optional: get last timestamps per part
            // Could be stored in memory, database, or passed from frontend
            // For simplicity, assuming empty (no filtering)
            Dictionary<string, DateTime> lastTimestamps = new Dictionary<string, DateTime>();

            // 3️⃣ Fetch data using optimized TVP method
            var results = LISSPC_BS.GetDataXYRealTimeBatch(parts, lastTimestamps);
            if (results == null) results = new List<XY_LABEL_CHARTS_STR_REALTIME>();

            List<object> response = new List<object>();

            // 4️⃣ Keep track of last value per chart to filter duplicates
            var lastValuePerChart = new Dictionary<string, double>();

            foreach (var lastData in results)
            {
                if (string.IsNullOrWhiteSpace(lastData.test_part) || string.IsNullOrWhiteSpace(lastData.y))
                    continue;

                // Map part → chart index
                int chartIndex = Array.FindIndex(parts, p => string.Equals(p, lastData.test_part.Trim(), StringComparison.OrdinalIgnoreCase));
                if (chartIndex < 0)
                    continue;

                string chartId = charts[chartIndex];

                // Apply limit adjustments if needed
                ApplyLimitAdjust(lastData);

                if (!double.TryParse(lastData.y.Trim(), out double currentValue))
                    continue;

                // Filter duplicate value per chart
                if (lastValuePerChart.TryGetValue(chartId, out double lastValue) && lastValue == currentValue)
                    continue;

                lastValuePerChart[chartId] = currentValue;

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
            }

            // 5️⃣ Always return JSON
            return Json(response, JsonRequestBehavior.AllowGet);
        }


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
        [Route("LISSPCRealTimePreload")]
        public ActionResult GetPreloadData(string testParts, string chartIds, int count = 30)
        {
            if (string.IsNullOrWhiteSpace(testParts) || string.IsNullOrWhiteSpace(chartIds))
                return new HttpStatusCodeResult(400);

            var parts = testParts.Split(',').Select(p => p.Trim()).ToArray();
            var charts = chartIds.Split(',').Select(c => c.Trim()).ToArray();

            if (parts.Length != charts.Length)
                return new HttpStatusCodeResult(400, "Mismatch between testParts and chartIds");

            var allData = new List<object>();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                string chartId = charts[i];

                // fetch last 'count' records for this part
                var data = LISSPC_BS.GetDataXYPreloadBatch(new[] { part }, count);

                foreach (var d in data)
                    ApplyLimitAdjust(d);

                // Map the chartId properly
                allData.AddRange(data.Select(d => new
                {
                    chartId = chartId,  // <-- use chartId here
                    Time = d.label,
                    Value = d.y,
                    unit = d.unit,
                    lower = d.lower,
                    upper = d.upper,
                    partDesc = d.partDesc
                }));
            }

            return Json(allData, JsonRequestBehavior.AllowGet);
        }




    }



}