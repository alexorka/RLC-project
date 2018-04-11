using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelImport.Models
{

    public class ExcelMembers
    {
        public string Location { get; set; }
        public string Name { get; set; }
        public string Descr { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string St { get; set; }
        public string Postal { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string EmployeeID { get; set; }
    }

    public class MemberFromExcel
    {
        //Extract from Excel 'Location' field
        public int CampusID { get; set; }
        public string CampusCode { get; set; }
        public string CampusName { get; set; }

        //Extract from Excel 'Descr' field
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        //Extract from Excel 'Name' field
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        //Extract from Excel 'EmployeeID' field
        public string MemberIDNumber { get; set; }

        //Extract from Excel 'Address' field
        public int MemberAddressID { get; set; }
        public string HomeStreet1 { get; set; }
        //Extract from Excel 'Postal' field
        public string ZipCode { get; set; }

        //Extract from Excel 'City' field
        public int CityID { get; set; }
        public string CityName { get; set; }

        //Extract from Excel 'St' field
        public string StateCodeID { get; set; }
        public string StateCode { get; set; }

        //Extract from Excel 'Phone' field
        public string PhoneRecID { get; set; }
        public string PhoneNumber { get; set; }

    }
}