using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for Project Administration.
    /// </summary>
    public partial class ProjectAdministration : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }

                return (int)ViewState["ProjectID"];
            }
            set
            {
                ViewState["ProjectID"] = value;
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
            if (!IsPostBack)
            {
                if (ProjectID > 0)
                {
                    LoadProjectLocations();
                    LoadProjectTeam();
                    budgetList.ProjectID = ProjectID;
                    if (Support.CanSeeBudgetSummary(UserID, ProjectID))
                    {
                        budgetList.BudgetListViewMode = ItemBrief.BudgetList.ViewMode.ProjectDashboard;
                    }
                    else
                    {
                        divBudgetPanel.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the OnRowDataBound event of the gvLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gvLocations_OnRowDataBound(Object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                dynamic projectLocation = e.Row.DataItem as dynamic;

                Label lblName = (Label)e.Row.FindControl("lblName");
                Label lblLocation = (Label)e.Row.FindControl("lblLocation");

                lblName.Text = Support.TruncateString(projectLocation.Name, 12);
                lblLocation.Text = Support.TruncateString(projectLocation.Location, 12);

                if (projectLocation.Name.Length > 12)
                {
                    lblName.ToolTip = projectLocation.Name;
                }

                if (projectLocation.Location.Length > 12)
                {
                    lblLocation.ToolTip = projectLocation.Location;
                }
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the project locations.
        /// </summary>
        private void LoadProjectLocations()
        {
            //Load the project locations
            var projectLocations = (from pl in DataContext.ProjectLocations
                                    where pl.ProjectId == ProjectID
                                    orderby pl.Name
                                    select pl);
            int projectLocationCount = projectLocations.Count();

            if (projectLocationCount > 0)
            {
                gvLocations.DataSource = projectLocations.Take(5);
                gvLocations.DataBind();

                if (projectLocationCount > 5)
                {
                    ltlMsg.Text = "This project has more locations. View these locations using the Project Details link above.";
                }
            }
            else
            {
                divLocationGrid.Visible = false;
            }

            lnkProjectDetails.HRef = string.Format("~/Project/ProjectDetails.aspx?ProjectID={0}", ProjectID);
        }

        /// <summary>
        /// Loads the project team.
        /// </summary>
        private void LoadProjectTeam()
        {
            lnkManageTeam.HRef = "~/Project/ProjectTeam.aspx?ProjectId=" + ProjectID;

            int projAdminTypeCodeId = Support.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");

            var projectUsers = DataContext.ProjectUsers.Where(pu => pu.ProjectId == ProjectID);
            int memberCount = projectUsers.Count();

            string adminUserName = (from u in DataContext.Users
                                    join pu in projectUsers on u.UserId equals pu.UserId
                                    where pu.ProjectUserTypeCodeId == projAdminTypeCodeId
                                    select (u.FirstName + " " + u.LastName).Trim()).FirstOrDefault();

            ltrlProjectTeam.Text = string.Format("<p>There {0} {1} {2} in your team.</p><br/><br/><p>Project Administrator is<br/>{3}.</p>",
                                    memberCount == 1 ? "is" : "are",
                                    memberCount,
                                    memberCount == 1 ? "person" : "people",
                                    Support.TruncateString(adminUserName, 30));
        }

        #endregion Private Methods
    }
}