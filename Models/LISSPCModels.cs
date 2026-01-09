using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QA_LISSummary.Models
{
    public class LISSPCModels
    {
        public class XY_LABEL_CHARTS_CLEAN_STR
        {
            public int runNo { get; set; }          // ROW_NUMBER()
            public int sec { get; set; }            // DATEDIFF(SECOND,...)

            public string test_value { get; set; } = "0";
            public DateTime date_tested { get; set; }

            public string condate { get; set; } = "0";   // MMdd
            public string test_unit { get; set; } = "0";

            public string test_info1 { get; set; } = "";
            public string test_info2 { get; set; } = "";


            //Label
            public string x { get; set; } = "0";
            public string y { get; set; } = "0";
            public string label { get; set; } = "0";
            public string showverticalline { get; set; } = "0";
        }

        public class TaskList
        {
            public int Task { get; set; }        
            public string TaskName { get; set; }           
        }


    }
}