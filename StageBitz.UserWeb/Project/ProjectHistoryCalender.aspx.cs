using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web page for project history calender.
    /// </summary>
    public partial class ProjectHistoryCalender : PageBase
    {
        /// <summary>
        /// Gets the company identifier.
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
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            private set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    int ProjectId = 0;

                    if (Request["ProjectId"] != null)
                    {
                        int.TryParse(Request["ProjectId"], out ProjectId);
                    }

                    ViewState["ProjectId"] = ProjectId;
                }

                return (int)ViewState["ProjectId"];
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">project not found</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StageBitz.Data.Project project = (from p in DataContext.Projects
                                                  where p.ProjectId == ProjectId
                                                  select p).FirstOrDefault();
                if (project == null)
                {
                    throw new Exception("project not found");
                }

                CompanyId = project.CompanyId;
                Response.Redirect(string.Format("~/Company/CompanyFinancialDetails.aspx?companyid={0}", CompanyId));
            }
        }
    }
}