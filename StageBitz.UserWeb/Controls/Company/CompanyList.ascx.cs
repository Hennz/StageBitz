using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for company list.
    /// </summary>
    public partial class CompanyList : UserControlBase
    {
        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
            divNotification.Visible = false;
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvCompanies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvCompanies_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (e.CommandName == "ViewInvite")
                {
                    int invitationId = int.Parse(e.CommandArgument.ToString());

                    invitationViewer.ShowInvitation(invitationId);
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvCompanies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvCompanies_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic company = e.Item.DataItem as dynamic;

                PlaceHolder plcHolderPending = (PlaceHolder)e.Item.FindControl("plcHolderPending");
                PlaceHolder plcHolderCompanies = (PlaceHolder)e.Item.FindControl("plcHolderCompanies");

                int companyID = company.CompanyId;

                int profileImageId = (from m in DataContext.DocumentMedias
                                      where m.RelatedTableName == "Company" && m.RelatedId == companyID && m.SortOrder == 1
                                      select m.DocumentMediaId).FirstOrDefault();

                bool hasProfileImage = (profileImageId != 0);

                if (company.InvitationId == 0) //this is for the working company list
                {
                    plcHolderPending.Visible = false;
                    plcHolderCompanies.Visible = true;

                    Label lblCompanyName = (Label)e.Item.FindControl("lblCompanyName");
                    HtmlAnchor lnkInventory = (HtmlAnchor)e.Item.FindControl("lnkInventory");
                    HtmlGenericControl dashboardLink = (HtmlGenericControl)e.Item.FindControl("dashboardLink");

                    if (company.IsCompanyUser)
                    {
                        HtmlAnchor lnkCompanies = (HtmlAnchor)e.Item.FindControl("lnkCompanies");
                        Image imgCompAdmin = (Image)e.Item.FindControl("imgCompanyUsers");

                        lnkCompanies.HRef = string.Format("~/Company/CompanyDashboard.aspx?companyid={0}", company.CompanyId);

                        imgCompAdmin.Visible = true;

                        int companyAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");
                        int companyPrimaryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
                        int companyInventoryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");

                        List<CompanyUserRole> companyUserRoles = this.GetBL<CompanyBL>().GetCompanyUserRoles(UserID, companyID);

                        #region permissions for company

                        if (companyUserRoles != null)
                        {
                            if (companyUserRoles.Count() == 1)
                            {
                                int companyUserTypeCodeId = companyUserRoles.First().CompanyUserTypeCodeId;

                                if (companyUserTypeCodeId == companyPrimaryAdminCodeID)
                                {
                                    imgCompAdmin.ToolTip = "You are the Primary Administrator for this Company.";
                                }
                                else if (companyUserTypeCodeId == companyAdminCodeID)
                                {
                                    imgCompAdmin.ToolTip = "You are a Secondary Administrator for this Company.";
                                }
                                else if (companyUserTypeCodeId == companyInventoryAdminCodeID)
                                {
                                    imgCompAdmin.ToolTip = "You are an Inventory Administrator for this Company.";
                                }
                            }
                            else
                            {
                                var admin = companyUserRoles.Where(cur => cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID).FirstOrDefault();

                                if (admin != null)
                                {
                                    imgCompAdmin.ToolTip = "You are the Primary Administrator and an Inventory Administrator for this Company.";
                                }
                                else
                                {
                                    imgCompAdmin.ToolTip = "You are a Secondary Administrator and an Inventory Administrator for this Company.";
                                }
                            }
                        }

                        #endregion permissions for company

                        lnkInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", company.CompanyId);
                    }
                    else
                    {
                        if (company.ProjectId > 0)
                        {
                            lnkInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", company.CompanyId, (int)BookingTypes.Project, company.ProjectId);
                        }
                        else
                        {
                            lnkInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", company.CompanyId);
                        }

                        dashboardLink.Visible = false;
                    }

                    lblCompanyName.Text = Support.TruncateString(company.CompanyName, 18);
                    if (company.CompanyName != null && company.CompanyName.Length > 18)
                    {
                        lblCompanyName.ToolTip = company.CompanyName;
                    }

                    if (hasProfileImage)
                    {
                        ImageDisplay idCompanies = (ImageDisplay)e.Item.FindControl("idCompanies");
                        idCompanies.DocumentMediaId = profileImageId;
                        idCompanies.Visible = true;
                    }
                    else
                    {
                        Image imgCompanies = (Image)e.Item.FindControl("imgCompanies");
                        imgCompanies.Visible = true;
                    }
                }
                else // this is for the company invitations
                {
                    plcHolderPending.Visible = true;
                    plcHolderCompanies.Visible = false;
                    Label lblPendingCompanyName = (Label)e.Item.FindControl("lblPendingCompanyName");
                    LinkButton lnkbtnViewInvite = (LinkButton)e.Item.FindControl("lnkbtnViewInvite");
                    LinkButton linkViewInviteBorder = (LinkButton)e.Item.FindControl("linkViewInviteBorder");

                    lnkbtnViewInvite.CommandArgument = company.InvitationId.ToString();
                    linkViewInviteBorder.CommandArgument = company.InvitationId.ToString();
                    //set company name litCompanyName
                    lblPendingCompanyName.Text = Support.TruncateString(company.CompanyName, 20);

                    if (company.CompanyName != null && company.CompanyName.Length > 20)
                    {
                        lblPendingCompanyName.ToolTip = company.CompanyName;
                    }

                    if (hasProfileImage)
                    {
                        ImageDisplay idPendingCompanies = (ImageDisplay)e.Item.FindControl("idPendingCompanies");
                        idPendingCompanies.DocumentMediaId = profileImageId;
                        idPendingCompanies.Visible = true;
                    }
                    else
                    {
                        Image imgPendingCompanies = (Image)e.Item.FindControl("imgPendingCompanies");
                        imgPendingCompanies.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the InvitationStatusChanged event of the invitationViewer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StageBitz.UserWeb.Controls.Common.InvitationStatusChangedEventArgs"/> instance containing the event data.</param>
        protected void invitationViewer_InvitationStatusChanged(object sender, StageBitz.UserWeb.Controls.Common.InvitationStatusChangedEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                divNotification.Visible = true;

                if (e.Accepted)
                {
                    divNotification.InnerText = "You have accepted the Company invitation.";
                }
                else
                {
                    divNotification.InnerText = "You have declined the Company invitation.";
                }

                LoadData();
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            List<CompanyListInfo> companyList = this.GetBL<CompanyBL>().GetCompanyList(UserID);
            lvCompanies.DataSource = companyList.OrderBy(c => c.InvitationId > 0 ? 0 : 1).ThenBy(c => c.CompanyName); ;
            lvCompanies.DataBind();

            if (companyList.Count() > 0)
            {
                divNoCompanies.Visible = false;
            }
            else
            {
                divNoCompanies.Visible = true;
            }
            up.Update();
        }

        /// <summary>
        /// Initializes the welcome tool tips.
        /// </summary>
        /// <param name="freeTrialOption">The free trial option.</param>
        public void InitializeWelcomeToolTips(WelcomeMessage.FreeTrialOption freeTrialOption)
        {
            switch (freeTrialOption)
            {
                case WelcomeMessage.FreeTrialOption.None:
                    return;

                case WelcomeMessage.FreeTrialOption.ExpectingInvitation:
                    helptipWaitingForInvitation.Visible = true;
                    break;

                case WelcomeMessage.FreeTrialOption.CreateNewProject:
                    helpTipCompanyList.Visible = true;
                    lblTooltipInventoryInfo.Text = "- The Company Inventory should you decide to explore it later.";
                    break;

                case WelcomeMessage.FreeTrialOption.CreateInventory:
                    helpTipCompanyList.Visible = true;
                    lblTooltipInventoryInfo.Text =
                        "- The Company Inventory, where you can add Items and their details. For really quick Inventory creation," +
                        " why not try our mobile app available in the <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppITuneUrl") + "'>App Store</a> and on <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppGooglePlayUrl") + "'>Google Play</a>?";
                    break;

                case WelcomeMessage.FreeTrialOption.CreateProjectAndInventory:
                    helpTipCompanyList.Visible = true;
                    lblTooltipInventoryInfo.Text =
                        "- The Company Inventory, where you can add Items and their details. For really quick Inventory creation," +
                        " why not try our mobile app available in the <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppITuneUrl") + "'>App Store</a> and on <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppGooglePlayUrl") + "'>Google Play</a>?";
                    break;
            }
        }

        #endregion Public Methods
    }
}