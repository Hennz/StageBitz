using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Personal
{
    /// <summary>
    /// User control for configure user email notifications.
    /// </summary>
    public partial class UserEmailNotifications : UserControlBase
    {
        #region Enums

        /// <summary>
        /// UI section.
        /// </summary>
        private enum Section
        {
            Project = 1,
            Company = 2
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the viewing user id.
        /// </summary>
        /// <value>
        /// The view user id.
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers and Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                List<Code> codes = Utils.GetCodesByCodeHeader("UserEmailNotificationType");
                int weeklyNotificationCodeId = Utils.GetCodeIdByCodeValue("UserEmailNotificationType", "WEEKLY");
                string weeklyEmailSendDay = Utils.GetSystemValue("EmailNotificationDayOfWeek");

                var radioButtionsDataSource = (from c in codes
                                               select new
                                               {
                                                   Text = (c.CodeId == weeklyNotificationCodeId ? string.Concat(c.Description, " (each ", weeklyEmailSendDay, ")") : c.Description),
                                                   CodeId = c.CodeId
                                               });

                rbtnListUpdatesForCompanies.DataSource = rbtnListUpdatesForProjects.DataSource = radioButtionsDataSource;
                rbtnListUpdatesForCompanies.DataTextField = rbtnListUpdatesForProjects.DataTextField = "Text";
                rbtnListUpdatesForCompanies.DataValueField = rbtnListUpdatesForProjects.DataValueField = "CodeId";
                rbtnListUpdatesForCompanies.DataBind();
                rbtnListUpdatesForProjects.DataBind();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rbtnListUpdatesForCompanies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rbtnListUpdatesForCompanies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.PageBase.StopProcessing)
            {
                int companyEmailNotificationCodeId;
                if (int.TryParse(rbtnListUpdatesForCompanies.SelectedValue, out companyEmailNotificationCodeId))
                {
                    SaveEmailNotification(Section.Company, companyEmailNotificationCodeId);
                    ShowNotification("emailNotifications");
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rbtnListUpdatesForProjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rbtnListUpdatesForProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.PageBase.StopProcessing)
            {
                int projectEmailNotificationCodeId;
                if (int.TryParse(rbtnListUpdatesForProjects.SelectedValue, out projectEmailNotificationCodeId))
                {
                    SaveEmailNotification(Section.Project, projectEmailNotificationCodeId);
                    ShowNotification("emailNotifications");
                }
            }
        }

        #endregion Event Handlers and Overrides

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            User user = GetBL<PersonalBL>().GetUser(this.ViewUserId);
            if (user != null)
            {
                rbtnListUpdatesForCompanies.SelectedValue = user.CompanyEmailNotificationCodeId.ToString(CultureInfo.InvariantCulture);
                rbtnListUpdatesForProjects.SelectedValue = user.ProjectEmailNotificationCodeId.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                upnlEmailNotifications.Visible = false;
            }

            rbtnListUpdatesForCompanies.Enabled = rbtnListUpdatesForProjects.Enabled = !IsReadOnly;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Saves the user email notification.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="emailNotificationCodeId">The email notification code id.</param>
        private void SaveEmailNotification(Section section, int emailNotificationCodeId)
        {
            User user = GetBL<PersonalBL>().GetUser(this.ViewUserId);
            switch (section)
            {
                case Section.Company:
                    user.CompanyEmailNotificationCodeId = emailNotificationCodeId;
                    break;

                case Section.Project:
                    user.ProjectEmailNotificationCodeId = emailNotificationCodeId;
                    break;
            }

            GetBL<PersonalBL>().SaveChanges();
        }

        #endregion Private Methods
    }
}