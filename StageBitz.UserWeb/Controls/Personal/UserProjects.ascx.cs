using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Personal
{
    /// <summary>
    /// User control for user projects.
    /// </summary>
    public partial class UserProjects : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the view user identifier.
        /// </summary>
        /// <value>
        /// The view user identifier.
        /// </value>
        public int ViewUserId
        {
            get
            {
                if (ViewState["ViewUserId"] == null)
                {
                    ViewState["ViewUserId"] = 0;
                }

                return (int)ViewState["ViewUserId"];
            }
            set
            {
                ViewState["ViewUserId"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region Grid View

        /// <summary>
        /// Handles the SortCommand event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvProjects.MasterTableView.SortExpressions.Clear();
                gvProjects.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvProjects.Rebind();
            }
        }

        /// <summary>
        /// Handles the NeedDataSource event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            gvProjects.DataSource = GetProjects();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the gvProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvProjects_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic projData = (dynamic)e.Item.DataItem;

                #region Project

                HyperLink lnkProject = (HyperLink)dataItem.FindControl("lnkProject");

                if (Support.CanAccessProject(projData.Project))
                {
                    lnkProject.Visible = true;
                    lnkProject.Text = Support.TruncateString(projData.Project.ProjectName, 20);
                    if (projData.Project.ProjectName.Length > 20)
                    {
                        lnkProject.ToolTip = projData.Project.ProjectName;
                    }

                    lnkProject.NavigateUrl = "~/Project/ProjectDashboard.aspx?projectId=" + projData.Project.ProjectId;
                }
                else
                {
                    dataItem["Project"].Text = Support.TruncateString(projData.Project.ProjectName, 20);
                    if (projData.Project.ProjectName.Length > 20)
                    {
                        dataItem["Project"].ToolTip = projData.Project.ProjectName;
                    }
                }

                #endregion Project

                #region Company

                HyperLink lnkCompany = (HyperLink)dataItem.FindControl("lnkCompany");

                if (Support.IsCompanyAdministrator(projData.Company.CompanyId))
                {
                    lnkCompany.Visible = true;
                    lnkCompany.Text = Support.TruncateString(projData.Company.CompanyName, 20);
                    if (projData.Company.CompanyName.Length > 20)
                    {
                        lnkCompany.ToolTip = projData.Company.CompanyName;
                    }

                    lnkCompany.NavigateUrl = "~/Company/CompanyDashboard.aspx?companyId=" + projData.Company.CompanyId;
                }
                else
                {
                    dataItem["Company"].Text = Support.TruncateString(projData.Company.CompanyName, 20);
                    if (projData.Company.CompanyName.Length > 20)
                    {
                        dataItem["Company"].ToolTip = projData.Company.CompanyName;
                    }
                }

                #endregion Company

                #region Permission Type

                dataItem["Permission"].Text = projData.PermissionType.Description;

                #endregion Permission Type

                #region Role

                if (!string.IsNullOrEmpty(projData.ProjectRole))
                {
                    dataItem["ProjectRole"].Text = Support.TruncateString(projData.ProjectRole, 20);
                    if (projData.ProjectRole.Length > 20)
                    {
                        dataItem["ProjectRole"].ToolTip = projData.ProjectRole;
                    }
                }

                #endregion Role
            }
        }

        #endregion Grid View

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            gvProjects.DataSource = GetProjects();
            gvProjects.DataBind();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the projects.
        /// </summary>
        /// <returns></returns>
        private List<dynamic> GetProjects()
        {
            var projects = from pu in DataContext.ProjectUsers
                           join proj in DataContext.Projects on pu.ProjectId equals proj.ProjectId
                           join comp in DataContext.Companies on proj.CompanyId equals comp.CompanyId
                           join permissionType in DataContext.Codes on pu.ProjectUserTypeCodeId equals permissionType.CodeId
                           where pu.UserId == ViewUserId && pu.IsActive == true
                           select new { Project = proj, Company = comp, PermissionType = permissionType, ProjectRole = pu.Role };

            return projects.ToList<dynamic>();
        }

        #endregion Private Methods
    }
}