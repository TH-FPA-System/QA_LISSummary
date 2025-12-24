using System;

namespace QA_LISSummary.Models
{



    public class XY_CHARTS
    {
        public XY_CHARTS()
        {
        }
       
        public double x { get; set; } = 0;
        public double y { get; set; } = 0;


    }
    public class XY_CHARTS_STR
    {
        public XY_CHARTS_STR()
        {
        }

        public string x { get; set; } = "0";
        public string y { get; set; } = "0";


    }
    public class XY_LABEL_CHARTS_STR
    {
        public XY_LABEL_CHARTS_STR()
        {
        }
        
        public string x { get; set; } = "0";
        public string y { get; set; } = "0";
        public string label { get; set; } = "0";
        public string showverticalline { get; set; } = "0";
        public string unit { get; set; } = "0";
        public string limit_adjust1 { get; set; } = "0";
        public string limit_adjust2 { get; set; } = "0";
        public string piorSet {  get; set; } = "0";
        public string limit_adjust_type1 { get; set; } = "0";
        public string limit_adjust_lw1 { get; set; } = "0";
        public string limit_adjust_up1 { get; set; } = "0";
        public string limit_adjust_type2 { get; set; } = "0";
        public string limit_adjust_lw2 { get; set; } = "0";
        public string limit_adjust_up2 { get; set; } = "0";
        public string test_info1 { get; set; } = "0";
        public string test_info2 { get; set; } = "0";

        public string test_part { get; set; } = "0";


    }

    public class XY_LABEL_CHARTS_STR_REALTIME
    {
        public XY_LABEL_CHARTS_STR_REALTIME()
        {
        }

        public string x { get; set; } = "0";
        public string y { get; set; } = "0";
        public string label { get; set; } = "0";
        public string unit { get; set; } = "0";
        public string piorSet { get; set; } = "0";
   
        public string test_part { get; set; } = "0";

        public string partDesc { get; set; } = "0";
        public double upper { get; set; } = 0;
        public double lower { get; set; } = 0;
        public string limit_adjust_value { get; set; } = "0";
        public string limit_adjust_type { get; set; } = "0";

        // Add this for caching last timestamp
        public DateTime date_tested { get; set; } = DateTime.MinValue;


    }
    public class ZoomLine_CHARTS_STR
    {
        public ZoomLine_CHARTS_STR()
        {
        }

        public string category { get; set; } = "0";
        public string seriesname { get; set; } = "0";
        public string data { get; set; } = "0";


    }

    public class XY_CHARTS_STR_DOUBLE
    {
        public XY_CHARTS_STR_DOUBLE()
        {
        }

        public string x { get; set; } = "0";
        public double y { get; set; } = 0;


    }

    public class LABEL_VALUE_CHARTS
    {
        public LABEL_VALUE_CHARTS()
        {
        }

        public string label { get; set; } = "0";
        public double value { get; set; } = 0;


    }
    public class LABEL_VALUE_CHARTS_STR
    {
        public LABEL_VALUE_CHARTS_STR()
        {
        }

        public string label { get; set; } = "0";
        public string value { get; set; } = "0";
        public string Status { get; set; } = "0";
        public string link {  get; set; } = "0";
        public string part_no { get; set; } = "0";  

    }
 
    public class TEST_RESULT_SINGLE_DD
    {
        public TEST_RESULT_SINGLE_DD()
        {
        }

        public string part { get; set; } = "0";
        public string serial { get; set; } = "0";
        public string test_part { get; set; } = "0";
        public DateTime date_tested { get; set; }
        public string test_status { get; set; } = "0";
        public string station { get; set; } = "0";
        public string test_result { get; set; } = "0";
        public string property_value { get; set; } = "0";



    }

    public class PART_TEST_LIMITS
    {
        public PART_TEST_LIMITS()
        {
        }

        public string part { get; set; } = "0";
        public string part_desc { get; set; } = "0";
        public string lower { get; set; } = "0";    
        public string upper { get; set; } = "0";



    }

    public class Part_Tests
    {
        public string test_part { get; set; }
        public string description { get; set; }
        public string class_name { get; set; }
        public string task { get; set; }
    }

    public class TEST_RESULT_DETAIL
    {
        public TEST_RESULT_DETAIL()
        {
        }

        public string part { get; set; } = "0";
        public string serial { get; set; } = "0";
        public string task { get; set; } = "0";
        public string test_part { get; set; } = "0";
        public string part_desc { get; set; } = "0";
        public string test_result { get; set; } = "0";
        public string test_fault { get; set; } = "0";
        public string test_status { get; set; } = "0";
        public string run_number { get; set; } = "0";
        public string station { get; set; } = "0";
        public string date_tested { get; set; } = DateTime.Now.ToString() ;



    }
    public class Car
    {
        public Car()
        {
        }
        public string Rank { get; set; } = "1";
        public string Model { get; set; } = "F-Series";
        public string Make { get; set; } = "Ford";
        public string UnitsSold { get; set; } = "896526";
        public string AssemblyLocation { get; set; } = "Claycomo, Mo.";
    }

    public class PART_MARKET
    {
        public PART_MARKET()
        {
        }

        public string Market { get; set; } = "NZ";

    }
    public class PART_DDPHASE
    {
        public PART_DDPHASE()
        {
        }

        public string DDPhase { get; set; } = "PH9";

    }

    public class PART_PROPERTY_DATA
    {
        public PART_PROPERTY_DATA()
        {
        }

        public string PropertyValue { get; set; } = "";

    } 


public class LIMIT_ADJUST
{
    public LIMIT_ADJUST()
    {
    }
    public string id_limit_adjust { get; set; } = "";
    public string test_part { get; set; } = "";
    public string task { get; set; } = "";
    public string limit_adjust_type { get; set; } = "";
    public string limit_adjust_value { get; set; } = "";
}




}