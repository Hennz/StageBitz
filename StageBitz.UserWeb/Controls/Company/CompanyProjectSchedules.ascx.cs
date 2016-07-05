using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for company project schedules.
    /// </summary>
    public partial class CompanyProjectSchedules : UserControlBase
    {
        #region Properties

        /// <summary>
        /// The company identifier var.
        /// </summary>
        private int companyID = 0;

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyID
        {
            get { return companyID; }
            set { companyID = value; }
        }

        #endregion Properties

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (CompanyID > 0)
            {
                //Load the top 10 events that are not being expired.
                var eventList = (from p in DataContext.Projects
                                 join pe in DataContext.ProjectEvents on p.ProjectId equals pe.ProjectId
                                 where p.CompanyId == CompanyID && pe.EventDate > Now && p.IsActive == true
                                 orderby pe.EventDate
                                 select new { pe.EventName, EventDate = pe.EventDate, p.ProjectName, p.ProjectId }).Take(10);

                gvEventsLeft.DataSource = eventList.Take(5);
                gvEventsLeft.DataBind();

                gvEventsRight.DataSource = eventList.Skip(5);
                gvEventsRight.DataBind();
            }
        }
    }
}