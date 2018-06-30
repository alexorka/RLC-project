using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace ExcelImport.Models
{
    public class ExcelMembers
    {
        public string Location { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string EmployeeID { get; set; }
    }

    public class ExcelSchedules
    {
        public string Id { get; set; }
        public string Instructor { get; set; }
        public string Campus { get; set; }
        public string Location { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string Division { get; set; }
        public string ClassNumber { get; set; }
        public string CAT_NBR { get; set; }
        public string Sect { get; set; }
        public string Subject { get; set; }
        public string LecOrLab { get; set; }
        public string SB_TM { get; set; }
        public string ATT_TP { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string Days { get; set; }
        public string ClassEndDate { get; set; }

    }

    public class WeekDay
    {
        public string codeInExcel { get; set; }

        //WeekDays from DB matching Excel Days
        public string codeInDB { get; set; }

    }

    public class WeekDaysMatch
    {
        public const string Monday = "M";
        public const string Tuesday = "TU";
        public const string Wednesday = "W";
        public const string Thursday = "TH";
        public const string Friday = "FR";
        public const string Saturday = "SA";
        public const string Sunday = "SU";
        public const string MW = "MW";
        public const string MWF = "MWF";
        public const string TTH = "TTH";
        public const string MTWR = "MTWR";
        public const string TR = "TR";
        public const string TBA = "TBA";
        public const string T = "T";
        public const string R = "R";
        public const string MTWRF = "MTWRF";

        private static Hashtable DaysMatch = new Hashtable();

        //--------------------------------------------------------------------------
        /// <summary>
        /// Constructor - add error code and message in hash table
        /// </summary>
        static WeekDaysMatch()
        {
            DaysMatch.Add(Monday, "Monday");
            DaysMatch.Add(Tuesday, "Tuesday");
            DaysMatch.Add(Wednesday, "Wednesday");
            DaysMatch.Add(Thursday, "Tuesday");
            DaysMatch.Add(Friday, "Friday");
            DaysMatch.Add(Saturday, "Saturday");
            DaysMatch.Add(Sunday, "Sunday");
            DaysMatch.Add(MW, "M W");
            DaysMatch.Add(MWF, "M W F");
            DaysMatch.Add(TTH, "T TH");
            DaysMatch.Add(MTWR, "M T W R");
            DaysMatch.Add(TR, "T R");
            DaysMatch.Add(TBA, "T B A");
            DaysMatch.Add(T, "T");
            DaysMatch.Add(R, "R");
            DaysMatch.Add(MTWRF, "M T W R F");
        }
        public static String GetDaysCode(string hResult)
        {
            String eventMessage = DaysMatch[hResult] as String;

            if (eventMessage == null)
            {
                eventMessage = "Error";
            }

            return eventMessage;
        }

    }
    //public class MemberFromExcel
    //{
    //    //Extract from Excel 'Location' field
    //    public int CampusID { get; set; }
    //    public string CampusCode { get; set; }
    //    public string CampusName { get; set; }

    //    //Extract from Excel 'Descr' field
    //    public int DepartmentID { get; set; }
    //    public string DepartmentName { get; set; }

    //    //Extract from Excel 'Name' field
    //    public string LastName { get; set; }
    //    public string FirstName { get; set; }
    //    public string MiddleName { get; set; }
    //    //Extract from Excel 'EmployeeID' field
    //    public string MemberIDNumber { get; set; }

    //    //Extract from Excel 'Address' field
    //    public int MemberAddressID { get; set; }
    //    public string HomeStreet1 { get; set; }
    //    //Extract from Excel 'Postal' field
    //    public string ZipCode { get; set; }

    //    //Extract from Excel 'City' field
    //    public int CityID { get; set; }
    //    public string CityName { get; set; }

    //    //Extract from Excel 'St' field
    //    public string StateCodeID { get; set; }
    //    public string StateCode { get; set; }

    //    //Extract from Excel 'Phone' field
    //    public string PhoneRecID { get; set; }
    //    public string PhoneNumber { get; set; }
    //}
}