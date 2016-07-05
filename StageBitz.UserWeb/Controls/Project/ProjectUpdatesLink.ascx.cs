using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Project
{
    /// <summary>
    /// User control Project Updates Link.
    /// </summary>
    public partial class ProjectUpdatesLink : UserControlBase
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

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                if (ViewState["CSSClass"] == null)
                {
                    return string.Empty;
                }
                return (string)ViewState["CSSClass"];
            }
            set
            {
                ViewState["CSSClass"] = value;
            }
        }

        #endregion Properties

        #region Event handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #endregion Event handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            LoadData(true);
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="displayCount">if set to <c>true</c> [display count].</param>
        public void LoadData(bool displayCount)
        {
            int notificationCount = 0;

            if (displayCount)
            {
                //Check whether this user is an active project member.
                ProjectUser puser = DataContext.ProjectUsers.Where(pu => pu.UserId == UserID && pu.ProjectId == ProjectID && pu.IsActive == true).FirstOrDefault();

                //Only display notification count if the user is an active project member
                if (puser != null)
                {
                    UserNotificationSetting unf = DataContext.UserNotificationSettings.Where(us => us.UserID == UserID && us.RelatedTable == "Project" && us.RelatedId == ProjectID).FirstOrDefault();
                    int lastNotificationId = unf == null ? 0 : unf.LastNotificationId.Value;
                    bool showMyNotifications = unf == null ? false : unf.ShowMyNotifications;

                    notificationCount = (from nf in DataContext.Notifications
                                         where nf.ProjectId == ProjectID
                                             && (nf.NotificationId > lastNotificationId)
                                             && (showMyNotifications || nf.CreatedByUserId != UserID)
                                         select nf).Count();
                }
            }

            lnkProjectNotifications.HRef = string.Format("~/Project/ProjectNotifications.aspx?projectid={0}", ProjectID);
            if (CssClass != string.Empty)
            {
                lnkProjectNotifications.Attributes["class"] = CssClass;
            }

            if (notificationCount > 0)
            {
                lnkNotificationCount.Visible = true;
                lnkNotificationCount.InnerText = notificationCount.ToString();
                lnkNotificationCount.HRef = lnkProjectNotifications.HRef;
            }
            else
            {
                lnkNotificationCount.Visible = false;
            }
        }

        #endregion Public Methods
    }
}