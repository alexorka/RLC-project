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
            divMain.AddCssClass("col-md-12");
            divMain.Attributes.Add("style", "padding:5px 30px 5px 30px;");

            var divLeft = new TagBuilder("div");
            divLeft.AddCssClass("col-md-1 alert-warning");
            var divRight = new TagBuilder("div");
            divRight.AddCssClass("col-md-11 alert-warning");

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

                divLeft.InnerHtml += img.ToString(TagRenderMode.Normal);
                divRight.InnerHtml += strong1.ToString(TagRenderMode.Normal);
                divRight.InnerHtml += ul.ToString(TagRenderMode.Normal);
                divMain.InnerHtml += divLeft.ToString(TagRenderMode.Normal);
                divMain.InnerHtml += divRight.ToString(TagRenderMode.Normal);
                return new MvcHtmlString(divMain.ToString());
            }
            return new MvcHtmlString(String.Empty);
        }
    }
}