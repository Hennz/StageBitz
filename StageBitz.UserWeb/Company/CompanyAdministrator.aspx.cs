using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Company
{
    /// <summary>
    /// Web page for Manage Company Team.
    /// </summary>
    public partial class CompanyAdministrator : PageBase
    {
        #region Fields

        private int PrimaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
        private int SecondaryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");
        private int InventoryAdminCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        private int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    int ProjectId = 0;

                    if (Request["CompanyId"] != null)
                    {
                        int.TryParse(Request["CompanyId"], out ProjectId);
                    }

                    ViewState["CompanyId"] = ProjectId;
                }

                return (int)ViewState["CompanyId"];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user is primary admin.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user is primary admin; otherwise, <c>false</c>.
        /// </value>
        private bool IsPrimaryAdmin
        {
            get
            {
                if (ViewState["IsPrimaryAdmin"] == null)
                {
                    ViewState["IsPrimaryAdmin"] = true;
                }

                return (bool)ViewState["IsPrimaryAdmin"];
            }
            set
            {
                ViewState["IsPrimaryAdmin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this page is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this page is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] != null)
                {
                    return (bool)ViewState["IsReadOnly"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ApplicationException">You do not have administrator rights to view this page.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!Support.IsCompanyAdministrator(CompanyId))
                {
                    throw new ApplicationException("You do not have administrator rights to view this page.");
                }

                IsReadOnly = Support.IsReadOnlyRightsForCompany(CompanyId);
                searchUsers.Visible = !IsReadOnly;

                LoadBreadCrumbs();
                ConfigureUI();

                bool hideNewProjectLink = this.GetBL<CompanyBL>().HasCompanySuspendedbySBAdmin(CompanyId)
                        || this.GetBL<CompanyBL>().IsCompanyPaymentFailed(CompanyId);
                spnNewProjectCreation.Visible = !hideNewProjectLink;

                lnkCreateNewProject.CompanyId = this.CompanyId;
                lnkCreateNewProject.LoadData();
                lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}", CompanyId);

                sbCompanyWarningDisplay.CompanyID = CompanyId;
                sbCompanyWarningDisplay.LoadData();

                sbPackageLimitsValidation.CompanyId = CompanyId;
                sbPackageLimitsValidation.DisplayMode = UserWeb.Controls.Company.PackageLimitsValidation.ViewMode.UserLimit;
                sbPackageLimitsValidation.LoadData();

                rbtnPrimaryAdmin.Attributes.Add("onClick", "EnableApplyPermissionButton()");
                rbtnSecondaryAdmin.Attributes.Add("onClick", "EnableApplyPermissionButton()");
            }
        }

        /// <summary>
        /// Handles the InvitationSent event of the searchUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void searchUsers_InvitationSent(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadAdministrators();
                gvCompanyAdministrators.DataBind();
                upnlProjectTeam.Update();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvCompanyAdministrators control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvCompanyAdministrators_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic user = (dynamic)dataItem.DataItem;

                int invitationId = user.InvitationId;
                int userID = user.UserId;

                ImageButton Deletebtn = dataItem["DeleteColumn"].Controls[0] as ImageButton;

                if (Deletebtn != null && (user.IsPrimaryAdmin || IsReadOnly))
                {
                    Deletebtn.Visible = false;
                }

                #region Name truncation

                HyperLink lnkUserName = (HyperLink)dataItem.FindControl("lnkUserName");
                Label lblUserName = (Label)dataItem.FindControl("lblUserName");

                if (lnkUserName != null && lblUserName != null)
                {
                    bool canViewContact = userID != this.UserID && invitationId == 0;
                    lnkUserName.Visible = canViewContact;
                    lblUserName.Visible = !canViewContact;

                    lnkUserName.NavigateUrl = string.Format("~/Personal/UserDetails.aspx?userId={0}", userID);
                    lnkUserName.Text = lblUserName.Text = Support.TruncateString(user.Name, 45);
                    if (user.Name.Length > 45)
                    {
                        lnkUserName.ToolTip = lblUserName.ToolTip = user.Name;
                    }
                }

                #endregion Name truncation

                #region Position truncation

                if (user.Position != null)
                {
                    dataItem["Position"].Text = Support.TruncateString(user.Position, 25);
                    if (user.Position.Length > 25)
                    {
                        dataItem["Position"].ToolTip = user.Position;
                    }
                }

                #endregion Position truncation

                ImageButton ibtnEditPermision = (ImageButton)dataItem.FindControl("ibtnEditPermision");
                if (ibtnEditPermision != null)
                {
                    ibtnEditPermision.Visible = !IsReadOnly && IsPrimaryAdmin && !user.IsPrimaryAdmin;
                }

                if (invitationId == 0)
                {
                    Literal litPermission = (Literal)dataItem.FindControl("litPermission");
                    litPermission.Text = user.UserPermission;
                }
                else
                {
                    ibtnEditPermision.Visible = false;
                }

                Image imgCompAdmin = (Image)dataItem.FindControl("imgCompAdmin");
                if (imgCompAdmin != null)
                {
                    List<CompanyUserRole> companyUserRoles = this.GetBL<CompanyBL>().GetCompanyUserRoles(userID, CompanyId);

                    var companyInventoryAdmin = companyUserRoles.Where(cur => cur.CompanyUserTypeCodeId == InventoryAdminCodeId && cur.IsActive).FirstOrDefault();

                    if (companyUserRoles.Count() > 1 && companyInventoryAdmin != null)
                    {
                        imgCompAdmin.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvCompanyAdministrators control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvCompanyAdministrators_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadAdministrators();
        }

        /// <summary>
        /// Handles the DeleteCommand event of the gvCompanyAdministrators control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        protected void gvCompanyAdministrators_DeleteCommand(object sender, GridCommandEventArgs e)
        {
            GridDataItem item = (GridDataItem)e.Item;
            int companyUserId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["CompanyUserId"];
            int invitationId = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["InvitationId"];
            if (companyUserId == 0) //This is an invitation
            {
                var invitation = DataContext.Invitations.FirstOrDefault(i => i.InvitationId == invitationId);
                if (invitation != null)
                {
                    DataContext.DeleteInvitation(invitation.InvitationId);
                }
            }
            else
            {
                DataContext.DeleteCompanyUser(companyUserId);
            }

            DataContext.SaveChanges();
            searchUsers.HideNotifications();
        }

        /// <summary>
        /// Handles the ItemCommand event of the gvCompanyAdministrators control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs" /> instance containing the event data.</param>
        protected void gvCompanyAdministrators_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (!StopProcessing)
            {
                if (e.CommandName == "EditPermission")
                {
                    GridDataItem dataItem = (GridDataItem)e.Item;
                    int companyUserId;

                    if (int.TryParse(e.CommandArgument.ToString(), out companyUserId) && companyUserId > 0)
                    {
                        InitializeChangePermissionPopup(companyUserId);
                        //Check whether this user is already in contacts before saving the new contact
                        List<CompanyUserRole> userRoles = DataContext.CompanyUserRoles.Where(ucr => ucr.CompanyUserId == companyUserId && ucr.IsActive).ToList<CompanyUserRole>();

                        foreach (CompanyUserRole companyUserRole in userRoles)
                        {
                            if (companyUserRole.CompanyUserTypeCodeId == PrimaryAdminCodeId)
                            {
                                rbtnPrimaryAdmin.Checked = true;
                            }
                            else if (companyUserRole.CompanyUserTypeCodeId == SecondaryAdminCodeId)
                            {
                                rbtnSecondaryAdmin.Checked = true;
                            }
                        }

                        popupChangePremission.ShowPopup();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApplyPermission control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyPermission_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!Page.IsValid || !sbPackageLimitsValidation.Validate())
                {
                    return;
                }
                int companyUserID;
                if (int.TryParse(hdnCompanyUserId.Value, out companyUserID))
                {
                    if (!sbPackageLimitsValidation.Validate())
                    {
                        popupChangePremission.HidePopup();
                    }
                    else
                    {
                        // Only possible operation is Secoundary Admin => Primary Admin
                        CompanyUserRole allCompanyUserPrimaryAdminRole =
                                                                    (from cu in DataContext.CompanyUsers
                                                                     join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                                     where cur.CompanyUserTypeCodeId == PrimaryAdminCodeId && cur.IsActive && cu.IsActive && cu.CompanyId == this.CompanyId
                                                                     select cur).FirstOrDefault();

                        CompanyUserRole currentSecondaryAdminRole = (from cu in DataContext.CompanyUsers
                                                                     join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                                                     where cur.CompanyUserTypeCodeId == SecondaryAdminCodeId && cur.IsActive && cu.IsActive
                                                                        && cu.CompanyId == this.CompanyId && cu.CompanyUserId == companyUserID
                                                                     select cur).FirstOrDefault();

                        if (rbtnPrimaryAdmin.Checked)
                        {
                            allCompanyUserPrimaryAdminRole.CompanyUserTypeCodeId = SecondaryAdminCodeId;
                            currentSecondaryAdminRole.CompanyUserTypeCodeId = PrimaryAdminCodeId;
                            DataContext.SaveChanges();
                        }

                        ConfigureUI();
                        LoadAdministrators();
                        gvCompanyAdministrators.DataBind();
                        popupChangePremission.HidePopup();
                        upnlProjectTeam.Update();
                    }
                }
            }
        }

        #endregion Events

        #region Private methods

        /// <summary>
        /// Configures the UI based on Primary admin rights
        /// </summary>
        private void ConfigureUI()
        {
            IsPrimaryAdmin = Support.IsCompanyPrimaryAdministrator(CompanyId);
            if (IsPrimaryAdmin)//Invitations can be made by Primary admin only.
            {
                searchUsers.DisplayMode = SearchUsers.ViewMode.CompanyAdministrators;
                searchUsers.CompanyId = CompanyId;
                searchUsers.LoadControl();
            }
            else
            {
                searchUsers.Visible = false;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            StageBitz.Data.Company company = DataContext.Companies.Where(c => c.CompanyId == CompanyId).FirstOrDefault();
            bc.AddLink(company.CompanyName, string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", company.CompanyId));
            bc.AddLink("Manage Company Team", null);

            bc.LoadControl();
        }

        /// <summary>
        /// Loads the administrators.
        /// </summary>
        private void LoadAdministrators()
        {
            #region Load Codevalues

            int compAdminInvitationTypeCodeId = Support.GetCodeByValue("InvitationType", "COMPANYADMIN").CodeId;

            int compAdminTypeCodeId = Support.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;
            int secondaryAdminCode = Support.GetCodeByValue("CompanyUserTypeCode", "SECADMIN").CodeId;

            List<Code> invitationStatusList = Support.GetCodesByCodeHeader("InvitationStatus");
            int invitationStatusPendingCodeID = invitationStatusList.Where(c => c.Value == "PENDING").FirstOrDefault().CodeId;
            int invitationStatusRejectedCodeID = invitationStatusList.Where(c => c.Value == "REJECTED").FirstOrDefault().CodeId;

            Code invitationStatusAcceptedCode = invitationStatusList.Where(c => c.Value == "ACCEPTED").FirstOrDefault();

            #endregion Load Codevalues

            #region Current Administrators

            var companyadminList = from cu in DataContext.CompanyUsers
                                   from cur in
                                       (
                                           from curTemp in DataContext.CompanyUserRoles
                                           join cut in DataContext.Codes on curTemp.CompanyUserTypeCodeId equals cut.CodeId
                                           where cu.CompanyUserId == curTemp.CompanyUserId && curTemp.IsActive
                                                && (curTemp.CompanyUserTypeCodeId == secondaryAdminCode || curTemp.CompanyUserTypeCodeId == compAdminTypeCodeId)
                                           orderby cut.SortOrder
                                           select curTemp).Take(1)
                                   join u in DataContext.Users on cu.UserId equals u.UserId
                                   join cut in DataContext.Codes on cur.CompanyUserTypeCodeId equals cut.CodeId
                                   where cu.CompanyId == CompanyId && cu.IsActive == true

                                   select new
                                   {
                                       UserId = u.UserId,
                                       CompanyUserId = cu.CompanyUserId,
                                       InvitationId = 0,
                                       Name = (u.FirstName + " " + u.LastName),
                                       Position = u.Position,
                                       Status = invitationStatusAcceptedCode.Description,
                                       StatusSortOrder = invitationStatusAcceptedCode.SortOrder,
                                       CompanyUserTypeStatusSortOrder = cut.SortOrder,
                                       IsPrimaryAdmin = (compAdminTypeCodeId == cur.CompanyUserTypeCodeId),
                                       CreatedDate = cu.CreatedDate,
                                       UserPermission = cut.Description
                                   };

            #endregion Current Administrators

            #region Pending invitations

            var companyadminInvitationList = from i in DataContext.Invitations
                                             from u in DataContext.Users.Where(user => user.UserId == i.ToUserId).Take(1).DefaultIfEmpty()
                                             join invSta in DataContext.Codes on i.InvitationStatusCodeId equals invSta.CodeId
                                             where i.RelatedTable == StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies && i.RelatedId == CompanyId
                                             && (i.InvitationStatusCodeId == invitationStatusPendingCodeID || i.InvitationStatusCodeId == invitationStatusRejectedCodeID)
                                             && i.InvitationTypeCodeId == compAdminInvitationTypeCodeId
                                             orderby i.CreatedDate
                                             select new
                                             {
                                                 UserId = 0,
                                                 CompanyUserId = 0,
                                                 InvitationId = i.InvitationId,
                                                 Name = ((u == null ? i.ToName : (u.FirstName + " " + u.LastName).Trim()) + " (" + i.ToEmail + ")"),
                                                 Position = string.Empty,
                                                 Status = invSta.Description,
                                                 StatusSortOrder = invSta.SortOrder,
                                                 CompanyUserTypeStatusSortOrder = 0,
                                                 IsPrimaryAdmin = false,
                                                 CreatedDate = i.CreatedDate,
                                                 UserPermission = string.Empty
                                             };

            #endregion Pending invitations

            var allUsers = companyadminInvitationList.Union(companyadminList).OrderBy(u1 => u1.StatusSortOrder).ThenBy(u2 => u2.CompanyUserTypeStatusSortOrder);
            gvCompanyAdministrators.DataSource = allUsers;

            if (!IsPrimaryAdmin)
            {
                foreach (GridColumn col in gvCompanyAdministrators.MasterTableView.RenderColumns)
                {
                    if (col.UniqueName == "DeleteColumn")
                    {
                        col.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the change permission popup.
        /// </summary>
        /// <param name="companyUserId">The company user identifier.</param>
        private void InitializeChangePermissionPopup(int companyUserId)
        {
            rbtnPrimaryAdmin.Checked = false;
            rbtnSecondaryAdmin.Checked = false;
            btnApplyPermission.Enabled = false;
            hdnCompanyUserId.Value = companyUserId.ToString(CultureInfo.InvariantCulture);
        }

        #endregion Private methods
    }
}