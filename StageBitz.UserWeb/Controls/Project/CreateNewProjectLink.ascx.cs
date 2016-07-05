using StageBitz.UserWeb.Common.Helpers;
using System;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control for create new project link.
    /// </summary>
    public partial class CreateNewProjectLink : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the company id.
        /// </summary>
        /// <value>
        /// The company id.
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

            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the link text.
        /// </summary>
        /// <value>
        /// The link text.
        /// </value>
        public string LinkText
        {
            get
            {
                return lbtnCreateNewProject.Text;
            }

            set
            {
                lbtnCreateNewProject.Text = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbtnCreateNewProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbtnCreateNewProject_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (ucPackageLimitsValidation.Validate())
                {
                    Response.Redirect(string.Format("~/Project/AddNewProject.aspx?companyid={0}", CompanyId));
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            ucPackageLimitsValidation.CompanyId = CompanyId;
            ucPackageLimitsValidation.DisplayMode = Company.PackageLimitsValidation.ViewMode.CreateProjects;
            ucPackageLimitsValidation.LoadData();
        }

        #endregion Public Methods
    }
}