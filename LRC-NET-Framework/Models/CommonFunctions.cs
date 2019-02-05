using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LRC_NET_Framework.Models
{
    public class CommonFunctions
    {
        public static SelectList AddFirstItem(SelectList origList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = origList.ToList();
            newList.Insert(0, firstItem);

            //var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItem = newList.First();
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }

        public static SelectList AddFirstItem(List<SelectListItem> origList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = origList;
            newList.Insert(0, firstItem);

            //var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItem = newList.First();
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }

        public static SelectList GetDepartments(int CollegeID, int DepartmentID, bool isActual, SelectListItem firstItem)
        {
            List<tb_Department> departments = new List<tb_Department>();
            IQueryable<tb_Department> Departments;
            using (LRCEntities context = new LRCEntities())
            {
                if (CollegeID == 0)
                    Departments = context.tb_Department.Include(x => x.tb_MemberMaster);
                else
                    Departments = context.tb_Department.Where(x => x.CollegeID == CollegeID).Include(x => x.tb_MemberMaster);
                foreach (var dep in Departments)
                {
                    if (dep.tb_MemberMaster.Any(c => c.DepartmentID == dep.DepartmentID))
                        departments.Add(dep);
                }
            }
            var deps = departments.Select(i => new SelectListItem()
            {
                Text = i.DepartmentName,
                Value = i.DepartmentID.ToString()
            }).ToList();


            //List<SelectListItem> newList = origList;
            deps.Insert(0, firstItem);

            var selectedItem = deps.First();
            if (DepartmentID > 0)
                selectedItem = deps.Where(s => s.Value == DepartmentID.ToString()).First();
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(deps, "Value", "Text", selectedItemValue);
        }

        public static string GetExportString(int CollegeID, int DepartmentID, string searchString)
        {
            //int CollegeID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("CollegeID");
            //int DepartmentID = (int)System.Web.HttpContext.Current.Profile.GetPropertyValue("DepartmentID");
            //string searchString = (string)System.Web.HttpContext.Current.Profile.GetPropertyValue("SearchString");
            List<tb_Department> deps = new List<tb_Department>();
            List<tb_MemberMaster> members = new List<tb_MemberMaster>();
            var sb = new StringBuilder();
            using (LRCEntities context = new LRCEntities())
            {
                if (DepartmentID > 0)
                    deps = context.tb_Department.Where(c => c.DepartmentID == DepartmentID).ToList();
                else
                    deps = context.tb_Department.Where(c => c.CollegeID == CollegeID).ToList();

                foreach (var dep in deps)
                {
                    List<tb_MemberMaster> membersInDep = context.tb_MemberMaster.Where(t => t.DepartmentID == dep.DepartmentID).ToList();
                    if (membersInDep.Count > 0)
                    {
                        foreach (var item in membersInDep)
                        {
                            if (String.IsNullOrEmpty(searchString))
                            {
                                members.Add(item);
                            }
                            //Searching @ Filtering
                            else if (item.LastName.ToUpper().Contains(searchString.ToUpper())
                                || item.FirstName.ToUpper().Contains(searchString.ToUpper()))
                            {
                                members.Add(item);
                            }
                        }
                    }
                }

                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                    "ID",
                    "INSTRCTR",
                    "CAMPUS",
                    "LOCATION",
                    "BUILDING",
                    "ROOM",
                    "DIV",
                    "CLASS#",
                    "SECT",
                    "SUBJCD",
                    "CATBR",
                    "LEC LAB",
                    "SBTM",
                    "ATT TP",
                    "BEGTIME",
                    "ENDTIME",
                    "DAYS",
                    "CLASSEND DT",
                    Environment.NewLine);

                foreach (var item in members)
                {
                    List<tb_SemesterTaught> semesterTaught = context.tb_SemesterTaught.Where(t => t.MemberID == item.MemberID).ToList();
                    foreach (var taught in semesterTaught)
                    {
                        sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                        item.MemberIDNumber ?? String.Empty, //ID
                        item.FirstName + " " + item.LastName + " " + item.MiddleName, //INSTRCTR
                        taught.tb_Building.tb_Campus.CollegeCode ?? String.Empty, //CAMPUS
                        taught.tb_Building.tb_Campus.CollegeCode + " MAIN", //LOCATION
                        taught.tb_Building.BuildingName ?? String.Empty, //BUILDING
                        taught.Room ?? String.Empty, //ROOM
                        item.tb_Division.DivisionName ?? String.Empty, //DIV
                        taught.Class ?? String.Empty, //CLASS#
                        "?", //SECT
                        "?", //SUBJCD
                        "?", //CATBR
                        "?", //LEC LAB
                        "?", //SBTM
                        "?", //ATT TP
                        taught.ClassStart.ToString(@"hh\:mm"), //BEGTIME
                        taught.ClassEnd.ToString(@"hh\:mm"), //ENDTIME
                        taught.tb_WeekDay.WeekDayName, //DAYS
                        "?", //CLASSEND DT
                        Environment.NewLine);
                    }
                }
            }
            return sb.ToString();
        }
    }
}