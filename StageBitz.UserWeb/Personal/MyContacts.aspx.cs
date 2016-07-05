using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Personal
{
    /// <summary>
    /// Web page for my cotacts.
    /// </summary>
    public partial class MyContacts : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the first name of the find.
        /// </summary>
        /// <value>
        /// The first name of the find.
        /// </value>
        private string FindFirstName
        {
            get
            {
                if (ViewState["FindFirstName"] == null)
                {
                    ViewState["FindFirstName"] = string.Empty;
                }

                return ViewState["FindFirstName"].ToString();
            }
            set
            {
                ViewState["FindFirstName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name of the find.
        /// </summary>
        /// <value>
        /// The last name of the find.
        /// </value>
        private string FindLastName
        {
            get
            {
                if (ViewState["FindLastName"] == null)
                {
                    ViewState["FindLastName"] = string.Empty;
                }

                return ViewState["FindLastName"].ToString();
            }
            set
            {
                ViewState["FindLastName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the find email.
        /// </summary>
        /// <value>
        /// The find email.
        /// </value>
        private string FindEmail
        {
            get
            {
                if (ViewState["FindEmail"] == null)
                {
                    ViewState["FindEmail"] = string.Empty;
                }

                return ViewState["FindEmail"].ToString();
            }
            set
            {
                ViewState["FindEmail"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();

                displaySettings.Module = ListViewDisplaySettings.ViewSettingModule.MyContacts;
                displaySettings.LoadControl();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFind_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                FindFirstName = txtFirstName.Text.Trim().ToLower();
                FindLastName = txtLastName.Text.Trim().ToLower();
                FindEmail = txtEmail.Text.Trim().ToLower();

                SearchInMyContacts();
                txtFirstName.Focus();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                txtFirstName.Text = string.Empty;
                txtLastName.Text = string.Empty;
                txtEmail.Text = string.Empty;

                ShowAllContacts();
            }
        }

        /// <summary>
        /// Handles the DisplayModeChanged event of the displaySettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void displaySettings_DisplayModeChanged(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                SearchInMyContacts();
            }
        }

        #region Grid View

        /// <summary>
        /// Handles the SortCommand event of the gvContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvContacts_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvContacts.MasterTableView.SortExpressions.Clear();
                gvContacts.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvContacts.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvContacts_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            gvContacts.DataSource = GetContacts();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvContacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                StageBitz.Data.User user = ((dynamic)e.Item.DataItem).User;

                #region Fulle Name

                HyperLink lnkUser = (HyperLink)e.Item.FindControl("lnkUser");
                string userFullName = (user.FirstName + " " + user.LastName).Trim();
                lnkUser.Text = Support.TruncateString(userFullName, 25);
                if (userFullName.Length > 25)
                {
                    lnkUser.ToolTip = userFullName;
                }
                lnkUser.NavigateUrl = "~/Personal/UserDetails.aspx?userId=" + user.UserId;

                #endregion Fulle Name

                #region Position

                if (!string.IsNullOrEmpty(user.Position))
                {
                    dataItem["Position"].Text = Support.TruncateString(user.Position, 20);
                    if (user.Position.Length > 20)
                    {
                        dataItem["Position"].ToolTip = user.Position;
                    }
                }

                #endregion Position

                #region Primary Email

                HyperLink lnkEmail = (HyperLink)e.Item.FindControl("lnkEmail");
                if (user.IsEmailVisible)
                {
                    lnkEmail.Text = Support.TruncateString(user.Email1, 20);
                    lnkEmail.NavigateUrl = "mailto:" + user.Email1;
                    if (user.Email1.Length > 20)
                    {
                        lnkEmail.ToolTip = user.Email1;
                    }
                }
                else
                {
                    lnkEmail.Visible = false;
                }

                #endregion Primary Email
            }
        }

        #endregion Grid View

        #region List View

        /// <summary>
        /// Handles the ItemDataBound event of the lvContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvContacts_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            dynamic userData = (dynamic)e.Item.DataItem;

            ImageDisplay userThumbDisplay = (ImageDisplay)e.Item.FindControl("userThumbDisplay");
            userThumbDisplay.DocumentMediaId = (int)userData.DocumentMediaId;

            HyperLink lnkUser = (HyperLink)e.Item.FindControl("lnkUser");
            Literal ltrlUsername = (Literal)e.Item.FindControl("ltrlUsername");
            Literal ltrlPosition = (Literal)e.Item.FindControl("ltrlPosition");
            Literal ltrlCompany = (Literal)e.Item.FindControl("ltrlCompany");

            lnkUser.NavigateUrl = "~/Personal/UserDetails.aspx?userId=" + userData.UserId;

            string userFullName = (userData.User.FirstName + " " + userData.User.LastName).Trim();
            ltrlUsername.Text = Support.TruncateString(userFullName, 15);
            if (userFullName.Length > 15)
            {
                lnkUser.ToolTip = userFullName;
            }

            ltrlPosition.Text = Support.TruncateString(userData.User.Position, 15);
            ltrlCompany.Text = Support.TruncateString(userData.User.Company, 15);

            if (!string.IsNullOrEmpty(userData.User.Position) && !string.IsNullOrEmpty(userData.User.Company))
            {
                ltrlPosition.Text += "<br />";
            }
        }

        #endregion List View

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Shows all contacts.
        /// </summary>
        private void ShowAllContacts()
        {
            FindFirstName = string.Empty;
            FindLastName = string.Empty;
            FindEmail = string.Empty;

            SearchInMyContacts();
        }

        /// <summary>
        /// Searches and displays users in my contacts
        /// </summary>
        private void SearchInMyContacts()
        {
            bool hasSearchCriteria = !(FindFirstName.Length == 0 && FindLastName.Length == 0 && FindEmail.Length == 0);

            List<dynamic> userData = GetContacts();

            DisplayResults(userData, hasSearchCriteria);
        }

        /// <summary>
        /// Gets the contacts.
        /// </summary>
        /// <returns></returns>
        private List<dynamic> GetContacts()
        {
            var userData = (from uc in DataContext.UserContacts
                            join u in DataContext.Users on uc.ContactUserId equals u.UserId
                            from mediaId in
                                (from m in DataContext.DocumentMedias
                                 where m.RelatedTableName == "User" && m.RelatedId == u.UserId && m.SortOrder == 1
                                 select m.DocumentMediaId).DefaultIfEmpty() //get user's profile picture

                            where uc.UserId == UserID &&
                                (FindFirstName.Length == 0 || u.FirstName.ToLower().StartsWith(FindFirstName)) &&
                                (FindLastName.Length == 0 || u.LastName.ToLower().StartsWith(FindLastName)) &&
                                (FindEmail.Length == 0 || u.LoginName.ToLower() == FindEmail) &&
                                u.IsActive == true
                            select new { User = u, FullName = (u.FirstName + " " + u.LastName).Trim(), UserId = u.UserId, DocumentMediaId = mediaId }).OrderBy(u => u.FullName);

            return userData.ToList<dynamic>();
        }

        /// <summary>
        /// Displays the specified users in a grid.
        /// </summary>
        private void DisplayResults(List<dynamic> users, bool hasSearchCriteria)
        {
            if (hasSearchCriteria)
            {
                ltrlResultsTitle.Text = string.Format("{0} Contact{1} found", users.Count, users.Count == 1 ? string.Empty : "s");
            }
            else
            {
                ltrlResultsTitle.Text = string.Format("All Contacts ({0})", users.Count);
            }

            if (users.Count == 0)
            {
                divNoContacts.Visible = true;
                if (hasSearchCriteria)
                {
                    divNoContacts.InnerText = "No contacts found for the given criteria.";
                }
                else
                {
                    divNoContacts.InnerText = "No contacts available.";
                }

                divContactThumbs.Visible = false;
                lvContacts.DataSource = null;
                lvContacts.DataBind();

                gvContacts.Visible = false;
                gvContacts.DataSource = null;
                gvContacts.DataBind();
            }
            else
            {
                divNoContacts.Visible = false;

                if (displaySettings.DisplayMode == ListViewDisplaySettings.ViewSettingValue.ThumbnailView)
                {
                    gvContacts.Visible = false;
                    divContactThumbs.Visible = true;
                    lvContacts.DataSource = users;
                    lvContacts.DataBind();
                }
                else
                {
                    divContactThumbs.Visible = false;
                    gvContacts.Visible = true;
                    gvContacts.DataSource = users;
                    gvContacts.DataBind();
                }
            }

            upnlSearchResults.Update();
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }

        #endregion Private Methods
    }
}