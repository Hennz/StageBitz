using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for breadcrumbs.
    /// </summary>
    public partial class BreadCrumbs : UserControlBase
    {
        #region Fields

        /// <summary>
        /// The links
        /// </summary>
        private List<Tuple<string, string, string>> links = new List<Tuple<string, string, string>>();

        #endregion Fields

        #region Events

        /// <summary>
        /// Handles the ItemDataBound event of the repeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Tuple<string, string, string> link = (Tuple<string, string, string>)e.Item.DataItem;
                Literal ltrlTitle = (Literal)e.Item.FindControl("ltrlTitle");
                HyperLink hypLink = (HyperLink)e.Item.FindControl("hypLink");

                string title = link.Item1;
                string url = link.Item2;
                string cssClass = link.Item3;

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
                    hypLink.CssClass = cssClass;
                }
            }
        }

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Updates the bread crumb.
        /// </summary>
        public void UpdateBreadCrumb()
        {
            upnlBreadCrumbs.Update();
        }

        /// <summary>
        /// Clears the links.
        /// </summary>
        public void ClearLinks()
        {
            links.Clear();
        }

        /// <summary>
        /// Adds the link.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cssClass">The CSS class.</param>
        public void AddLink(string title, string url, string cssClass = "")
        {
            links.Add(new Tuple<string, string, string>(title, url, cssClass));
        }

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            links.Insert(0, new Tuple<string, string, string>("Home", "~/Default.aspx", string.Empty));
            repeater.DataSource = links;
            repeater.DataBind();
        }

        #endregion Public Methods
    }
}