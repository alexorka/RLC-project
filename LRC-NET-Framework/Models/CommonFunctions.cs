using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}