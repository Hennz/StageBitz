using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;

namespace StageBitz.AdminWeb.Controls.Common
{
	public partial class BreadCrumbs : UserControlBase
	{
        private List<Tuple<string, string>> links = new List<Tuple<string, string>>();

        protected void repeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Tuple<string, string> link = (Tuple<string, string>)e.Item.DataItem;
                Literal ltrlTitle = (Literal)e.Item.FindControl("ltrlTitle");
                HyperLink hypLink = (HyperLink)e.Item.FindControl("hypLink");

                string title = link.Item1;
                string url = link.Item2;

                //If the Url isn't specified, show the title in plain text without the hyperlink
                if (string.IsNullOrEmpty(url))
                {
                    hypLink.Visible = false;
                    ltrlTitle.Visible = true;
                    ltrlTitle.Text = title;
                }
                else
                {
                    ltrlTitle.Visible = false;
                    hypLink.Visible = true;

                    if (title.Length > 20)
                    {
                        hypLink.ToolTip = title;
                        title = Support.TruncateString(title, 20);
                    }

                    hypLink.Text = title;
                    hypLink.NavigateUrl = url;
                }
            }
        }

        #region Methods

        public void AddLink(string title, string url)
        {
            links.Add(new Tuple<string, string>(title, url));
        }

        public void LoadControl()
        {
            links.Insert(0, new Tuple<string, string>("Home", "~/Default.aspx"));

            repeater.DataSource = links;
            repeater.DataBind();
        }

        #endregion
    }
}