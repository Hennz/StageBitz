using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;

namespace StageBitz.UserWeb
{
    /// <summary>
    /// Person dashboard page.
    /// </summary>
    public partial class Default : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the invitation code.
        /// </summary>
        /// <value>
        /// The invitation code.
        /// </value>
        private string InvitationCode
        {
            get
            {
                if (ViewState["invitationCode"] == null)
                {
                    if (Request["invitationCode"] == null)
                    {
                        ViewState["invitationCode"] = string.Empty;
                    }
                    else
                    {
                        ViewState["invitationCode"] = Request["invitationCode"];
                    }
                }

                return ViewState["invitationCode"].ToString();
            }
        }

        /// <summary>
        /// Gets the free trial option.
        /// </summary>
        /// <value>
        /// The free trial option.
        /// </value>
        public WelcomeMessage.FreeTrialOption FreeTrialOption
        {
            get
            {
                if (Session[WelcomeMessage.FreeTrialOptionSessionKey] == null)
                {
                    return WelcomeMessage.FreeTrialOption.None;
                }
                else
                {
                    return (WelcomeMessage.FreeTrialOption)Session[WelcomeMessage.FreeTrialOptionSessionKey];
                }
            }
        }

        #endregion Properties

        #region Event Handlers and overrides

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            divError.Visible = false;
            divSuccess.Visible = false;

            if (!IsPostBack)
            {
                string username = Support.TruncateString((Support.UserFirstName + " " + Support.UserLastName).Trim(), 40);
                DisplayTitle = string.Format("{0}'s Dashboard", username);
                InitializeWelcomeToolTips();

                #region Invitation

                if (!string.IsNullOrEmpty(InvitationCode))
                {
                    string message = string.Empty;
                    if (Support.ProcessInvitationCode(InvitationCode, UserID, out message) == true)
                    {
                        divSuccess.Visible = true;
                        divSuccess.InnerText = message;
                    }
                    else
                    {
                        divError.Visible = true;
                        divError.InnerText = message;
                    }
                }

                #endregion Invitation
            }
            //this will update the companies section when an invitation is accepted
            myProjectList.InvitationAccepted += delegate
            {
                ucCompanyList.LoadData();
                scheduleList.LoadData();
                upnlScheduleList.Update();
            };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            scheduleList.Visible = myProjectList.ActiveProjectCount > 0;
            upnlScheduleList.Update();
        }

        #endregion Event Handlers and overrides

        #region Private Methods

        /// <summary>
        /// Initializes the welcome tool tips.
        /// </summary>
        private void InitializeWelcomeToolTips()
        {
            if (FreeTrialOption != WelcomeMessage.FreeTrialOption.None)
            {
                ucCompanyList.InitializeWelcomeToolTips(FreeTrialOption);
                myProjectList.InitializeWelcomeToolTips(FreeTrialOption);
                RemoveFreeTrailSession();
            }
        }

        /// <summary>
        /// Removes the saved free trail option from session.
        /// </summary>
        private void RemoveFreeTrailSession()
        {
            Session.Remove(WelcomeMessage.FreeTrialOptionSessionKey);
        }

        #endregion Private Methods
    }
}