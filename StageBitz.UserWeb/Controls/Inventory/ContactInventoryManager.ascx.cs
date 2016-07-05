using StageBitz.Common;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// Delegate for inform inventory to reload
    /// </summary>
    public delegate void InformInventoryToReLoad();

    public partial class ContactInventoryManager : UserControlBase
    {
        /// <summary>
        /// The inform inventory to reload
        /// </summary>
        public InformInventoryToReLoad InformInventoryToReLoad;

        #region Public Properties

        /// <summary>
        /// Gets or sets the watch list header identifier.
        /// </summary>
        /// <value>
        /// The watch list header identifier.
        /// </value>
        public int WatchListHeaderId
        {
            get
            {
                if (ViewState["WatchListHeaderId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["WatchListHeaderId"];
            }
            set
            {
                ViewState["WatchListHeaderId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user company identifier.
        /// </summary>
        /// <value>
        /// The user company identifier.
        /// </value>
        public int UserCompanyId
        {
            get
            {
                if (ViewState["UserCompanyId"] == null)
                {
                    return 0;
                }
                return (int)ViewState["UserCompanyId"];
            }
            set
            {
                ViewState["UserCompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        public DateTime? FindFromDate
        {
            get
            {
                if (ViewState["FindFromDate"] != null)
                {
                    return (DateTime)ViewState["FindFromDate"];
                }

                return null;
            }

            set
            {
                ViewState["FindFromDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the find by.
        /// </summary>
        /// <value>
        /// The name of the find by.
        /// </value>
        public DateTime? FindToDate
        {
            get
            {
                if (ViewState["FindToDate"] != null)
                {
                    return (DateTime)ViewState["FindToDate"];
                }

                return null;
            }

            set
            {
                ViewState["FindToDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort expressions.
        /// </summary>
        /// <value>
        /// The sort expressions.
        /// </value>
        public GridSortExpressionCollection SortExpressions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the contact booking manager identifier.
        /// </summary>
        /// <value>
        /// The contact booking manager identifier.
        /// </value>
        private int? ContactBookingManagerId
        {
            get
            {
                return (int?)ViewState["ContactBookingManagerId"];
            }

            set
            {
                ViewState["ContactBookingManagerId"] = value;
            }
        }

        #endregion Public Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SendEmail(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (IsValidToSendEmail())
                {
                    // Construct the Email
                    string companyName = string.Empty;
                    Data.Company company = DataContext.Companies.Where(c => c.CompanyId == UserCompanyId).FirstOrDefault();
                    if (company != null)
                    {
                        companyName = company.CompanyName;
                    }

                    StringBuilder generalQuestionBuilder = new StringBuilder();
                    if (txtGeneralQuestion.Text.Trim().Length > 0)
                    {
                        generalQuestionBuilder.Append("<p><b>General questions:</b></p>");
                        generalQuestionBuilder.AppendLine("<p>");
                        string generalQuestion = txtGeneralQuestion.Text.Replace("\n", "<br />");
                        generalQuestionBuilder.AppendLine(generalQuestion.Trim());
                        generalQuestionBuilder.AppendLine("</p>");
                    }

                    string userInterestText = string.Empty;
                    bool isUserInterestTextAdded = false;

                    StringBuilder questionBuilder = new StringBuilder(userInterestText);

                    foreach (GridDataItem item in gvItemDetails.MasterTableView.Items)
                    {
                        TextBox txtQuestion = (TextBox)item.FindControl("txtQuestion");
                        HiddenField hdnIsDeleted = (HiddenField)item.FindControl("hdnIsDeleted");
                        if (hdnIsDeleted.Value == "false")
                        {
                            //This should only appears once. This is the title of the questions
                            if (!isUserInterestTextAdded)
                            {
                                userInterestText = string.Format("<p><b>Items {0} is interested in:</b></p>", Support.UserFullName);
                                questionBuilder.Append(userInterestText);
                                questionBuilder.Append("<ul>");
                                isUserInterestTextAdded = true;
                            }
                            string itemUrl = string.Format("{0}/Inventory/ItemDetails.aspx?ItemId={1}&CompanyId={2}", Support.GetSystemUrl(), item["ItemId"].Text, CompanyId);
                            questionBuilder.AppendLine(string.Format("<li><a href=\" {0} \"> {1}</a><ul>", itemUrl, item["ItemFullName"].Text));
                            if (string.Empty != txtQuestion.Text.Trim())
                                questionBuilder.AppendLine(string.Format("<li style=\"margin-left: -25px;\">{0}</li>", txtQuestion.Text.Trim()));
                            questionBuilder.AppendLine("</ul></li>");
                        }
                    }
                    questionBuilder.Append("</ul>");

                    Data.User userDetails = DataContext.Users.Where(u => u.UserId == UserID && u.IsActive == true).FirstOrDefault();
                    string supportEmail = Utils.GetSystemValue("FeedbackEmail");

                    //Get the Inventory Administrator
                    Data.User contactBookingManager = ContactBookingManagerId.HasValue ? GetBL<PersonalBL>().GetUser(ContactBookingManagerId.Value) :
                                GetBL<InventoryBL>().GetInventoryAdmin(CompanyId);
                    string toList = string.Concat(contactBookingManager.Email1, "," + Support.UserPrimaryEmail);

                    //Send the email to the Sender
                    EmailSender.QueryInventoryManager(toList, contactBookingManager.FirstName, ContactBookingManagerId.HasValue, Support.UserFullName,
                        companyName, generalQuestionBuilder.ToString(), questionBuilder.ToString(), supportEmail);

                    if (!Support.IsEmailSentToInventoryManager(CompanyId))
                    {
                        Data.InventoryManagerSentEmail inventoryManagerSentEmail = new Data.InventoryManagerSentEmail();
                        inventoryManagerSentEmail.UserId = UserID;
                        inventoryManagerSentEmail.CompanyId = CompanyId;
                        inventoryManagerSentEmail.LastUpdatedByUserId = inventoryManagerSentEmail.CreatedByUserId = UserID;
                        inventoryManagerSentEmail.CreatedDate = inventoryManagerSentEmail.LastUpdatedDate = Today;
                        DataContext.InventoryManagerSentEmails.AddObject(inventoryManagerSentEmail);
                        DataContext.SaveChanges();
                    }

                    popupContactInventoryManager.HidePopup();
                    popupEmailSent.ShowPopup();

                    //Inform Inventory to Reload
                    if (InformInventoryToReLoad != null)
                    {
                        InformInventoryToReLoad();
                    }
                }
                else
                {
                    popupError.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvItemDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvItemDetails_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic itemData = (dynamic)dataItem.DataItem;
                string itemName = itemData.Name;
                string description = itemData.Description;

                string itemId = string.Empty;

                dataItem["ItemId"].Text = itemData.ItemId.ToString();
                dataItem["ItemFullName"].Text = itemData.Name;

                dataItem["Description"].Text = Support.TruncateString(description, 20);
                if (!string.IsNullOrEmpty(description) && description.Length > 20)
                {
                    dataItem["Description"].ToolTip = description;
                }

                dataItem["Name"].Text = Support.TruncateString(itemName, 20);
                if (!string.IsNullOrEmpty(itemName) && itemName.Length > 20)
                {
                    dataItem["Name"].ToolTip = itemName;
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Shows the contact poppup.
        /// </summary>
        public void ShowContactPoppup()
        {
            ResetUI();
            LoadData();
            ScriptManager.RegisterClientScriptBlock(upnlContactIM, GetType(), "EnableSendEmail", "EnableSendEmail()", true);
            popupContactInventoryManager.ShowPopup();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            var itemDetails = GetBL<InventoryBL>().GetWatchListItems(CompanyId, UserID, FindFromDate, FindToDate, false, WatchListHeaderId, true);
            int maxContactBookingManagerId = itemDetails.Max(bd => bd.ContactBookingManagerId);
            int minContactBookingManagerId = itemDetails.Min(bd => bd.ContactBookingManagerId);
            this.ContactBookingManagerId = maxContactBookingManagerId == minContactBookingManagerId ? (int?)minContactBookingManagerId : null;
            Data.User contactBookingManager = ContactBookingManagerId.HasValue ? GetBL<PersonalBL>().GetUser(ContactBookingManagerId.Value) :
                    GetBL<InventoryBL>().GetInventoryAdmin(CompanyId);

            gvItemDetails.DataSource = itemDetails;
            gvItemDetails.MasterTableView.SortExpressions.Clear();
            foreach (GridSortExpression sortExpression in SortExpressions)
            {
                gvItemDetails.MasterTableView.SortExpressions.Add(sortExpression);
            }

            gvItemDetails.DataBind();

            Data.Company company = GetBL<CompanyBL>().GetCompany(CompanyId);
            string inventoryAdminName = contactBookingManager.FirstName + " " + contactBookingManager.LastName;

            lblInventoryManager.Text = Support.TruncateString(inventoryAdminName, 50);
            lblInventoryManager.ToolTip = string.Empty;
            if (inventoryAdminName.Length > 50)
            {
                lblInventoryManager.ToolTip = inventoryAdminName;
            }

            lblCompany.Text = Support.TruncateString(company.CompanyName, 50);
            if (company.CompanyName.Length > 50)
            {
                lblCompany.ToolTip = company.CompanyName;
            }
        }

        /// <summary>
        /// Resets the UI.
        /// </summary>
        private void ResetUI()
        {
            txtGeneralQuestion.Text = string.Empty;
        }

        /// <summary>
        /// Determines whether [is valid to send email].
        /// </summary>
        /// <returns></returns>
        private bool IsValidToSendEmail()
        {
            //If thers is at least a General question, then you can send the email
            bool canSendEmail = txtGeneralQuestion.Text.Trim().Length > 0;
            if (!canSendEmail)
            {
                foreach (GridDataItem item in gvItemDetails.MasterTableView.Items)
                {
                    TextBox txtQuestion = (TextBox)item.FindControl("txtQuestion");
                    if (txtQuestion.Text.Trim().Length > 0)
                    {
                        canSendEmail = true;
                        break;
                    }
                }
            }
            return canSendEmail;
        }

        #endregion Private Methods
    }
}