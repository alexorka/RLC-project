using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace LRC_NET_Framework.Helpers
{
    public static class ErrorHelper
    {
        public static MvcHtmlString CreateList(this HtmlHelper html, List<string> errList)
        {
            var divMain = new TagBuilder("div");
            divMain.AddCssClass("col-md-12 alert-warning");
            divMain.Attributes.Add("style", "padding-left:30px; padding-top:0px;");

            if (errList != null && errList[0] != "Empty")
            {
                var strong1 = new TagBuilder("strong");
                strong1.InnerHtml = "&nbsp; &nbsp; &nbsp; Errors / Warnings List:";

                var img = new TagBuilder("img");
                string domainName = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                img.MergeAttribute("src", domainName +"/Images/warning.gif");
                img.MergeAttribute("alt", "Warning!...");
                img.MergeAttribute("height", "48");

                var ul = new TagBuilder("ul");
                foreach (string item in errList)
                {
                    var error = item.Split('!');
                    var li = new TagBuilder("li");
                    var strong2 = new TagBuilder("strong");
                    var italian = new TagBuilder("i");
                    strong2.InnerHtml = error[0] + ":    ";
                    italian.InnerHtml = error[1];
                    li.InnerHtml += strong2.ToString(TagRenderMode.Normal);
                    li.InnerHtml += italian.ToString(TagRenderMode.Normal);

                    //li.SetInnerText(strong2 + error[1]);
                    ul.InnerHtml += li.ToString();
                }

                divMain.InnerHtml += img.ToString(TagRenderMode.Normal);
                divMain.InnerHtml += strong1.ToString(TagRenderMode.Normal);
                divMain.InnerHtml += ul.ToString(TagRenderMode.Normal);
                return new MvcHtmlString(divMain.ToString());
            }
            return new MvcHtmlString(String.Empty);
        }
    }
}