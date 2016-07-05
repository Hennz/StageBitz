using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Web;

namespace StageBitz.UserWeb
{
    /// <summary>
    /// Master page for content pages
    /// </summary>
    public partial class Content : MasterBase
    {
        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <value>
        /// The bread crumbs.
        /// </value>
        public BreadCrumbs BreadCrumbs
        {
            get
            {
                return breadCrumbs;
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string title = ((PageBase)Page).DisplayTitle;
                ltrlPageTitle.Text = title;

                if (string.IsNullOrEmpty(Page.Title) || Page.Title == "StageBitz")
                {
                    ((PageBase)Page).PageTitle = title;
                }
                else
                {
                    ((PageBase)Page).PageTitle = Page.Title;
                }

                UpdateUserNameDisplay();

                if (Page is Default)
                {
                    lnkHomePage.Attributes["class"] = "highlight";
                }
            }

            Page.Title = ((PageBase)Page).PageTitle;

            //mainNavi
            if (!IsLarge)
            {
                divMainNavi.Attributes.Add("class", "mainNavi");
            }
            else
            {
                divMainNavi.Attributes.Add("class", "largemainNavi");
            }
        }

        /// <summary>
        /// Updates the user name display.
        /// </summary>
        public void UpdateUserNameDisplay()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                divUserInfo.Visible = true;
                string userFullName = (Support.UserFirstName + " " + Support.UserLastName).Trim();
                lnkPersonalProfile.InnerText = Support.TruncateString(userFullName, 16);

                if (userFullName.Length > 16)
                {
                    lnkPersonalProfile.Title = userFullName;
                }
            }
            else
            {
                divUserInfo.Visible = false;
            }

            upnlUserName.Update();
        }
    }
}