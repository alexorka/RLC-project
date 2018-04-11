using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ExcelImport.Models;
using LinqToExcel;
using System.Data.SqlClient;

namespace LRC_NET_Framework.Controllers
{
    public class ImportExcelController : Controller
    {
        private LRCEntities db = new LRCEntities();
        /// <summary>  
        /// This function is used to download excel format.  
        /// </summary>  
        /// <param name="Path"></param>  
        /// <returns>file</returns>  
        public FileResult DownloadExcel()
        {
            string path = "/ImportExcel/CBU.xlsx";
            return File(path, "application/vnd.ms-excel", "CBU.xlsx");
        }

        // Extract Last, First, Middle Names from FullName 
        private string SplitFullName(string _fullName, out string _lastName, out string _firstName, out string _middleName)
        {
            string result = "Success";
            _lastName = String.Empty;
            _firstName = String.Empty;
            _middleName = String.Empty;
            var namesComma = _fullName.Split(',');
            if (namesComma.Length == 0)
                result = "Empty 'Name' field";
            else if (namesComma.Length == 1)
                result = "Comma is absent in 'Name' field";
            else if (namesComma.Length == 2)
            {
                _lastName = namesComma[0];
                var namesSpace = namesComma[1].Split(' ');
                if (namesSpace.Length == 1)
                    _firstName = namesSpace[0];
                else if (namesSpace.Length == 2)
                {
                    _firstName = namesSpace[0];
                    _middleName = namesSpace[1];
                }
            }
            return result;
        }

        // GET: ImportExcel
        public ActionResult UploadExcel()
        {
            return View();
        }

        // POST: ImportExcel
        [HttpPost]
        //[HttpPost]
        //public ActionResult UploadExcel(FormCollection collection)
        public JsonResult UploadExcel(HttpPostedFileBase FileUpload)
        {
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {


                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/ImportExcel/1/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Full time$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    string sheetName = "Full time";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var members = from a in excelFile.Worksheet<ExcelMembers>(sheetName) select a;

                    foreach (var a in members)
                    {
                        try
                        {
                            if (a.Name != "" && a.EmployeeID != "")
                            {
                                string lastName = String.Empty;
                                string firstName = String.Empty;
                                string middleName = String.Empty;
                                tb_MemberMaster TU = new tb_MemberMaster();
                                if (SplitFullName(a.Name, out lastName, out firstName, out middleName) == "Success")
                                {
                                    TU.LastName = lastName;
                                    TU.FirstName = firstName;
                                    TU.MiddleName = middleName;
                                    TU.MemberIDNumber = a.EmployeeID;
                                }
                                db.tb_MemberMaster.Add(TU);
                                db.SaveChanges();
                            }
                            else
                            {
                                data.Add("<ul>");
                                if (a.Name == "" || a.Name == null) data.Add("<li> name is required</li>");
                                if (a.EmployeeID == "" || a.EmployeeID == null) data.Add("<li>ContactNo is required</li>");

                                data.Add("</ul>");
                                data.ToArray();
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }
                        }

                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {

                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {

                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);

                                }

                            }
                        }
                    }
                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //alert message for invalid file format  
                    data.Add("<ul>");
                    data.Add("<li>Only Excel file format is allowed</li>");
                    data.Add("</ul>");
                    data.ToArray();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                data.Add("<ul>");
                if (FileUpload == null) data.Add("<li>Please choose Excel file</li>");
                data.Add("</ul>");
                data.ToArray();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
