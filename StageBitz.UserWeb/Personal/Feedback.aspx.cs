using StageBitz.Common;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Personal
{
    /// <summary>
    /// Web page Feedbacks.
    /// </summary>
    public partial class Feedback : PageBase
    {
        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        /// <value>
        /// The user email.
        /// </value>
        public string UserEmail
        {
            get
            {
                return ViewState["UserEmail"].ToString();
            }
            set
            {
                ViewState["UserEmail"] = value;
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StageBitz.Data.User user = DataContext.Users.Where(u => u.UserId == UserID).FirstOrDefault();
                UserEmail = user.Email1;

                ltrlName.Text = Support.TruncateString((user.FirstName + " " + user.LastName).Trim(), 100);
                ltrlEmail.Text = Support.TruncateString(user.Email1, 50);

                LoadBreadCrumbs();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendFeedback control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendFeedback_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!Page.IsValid)
                    return;

                string feedbackText = txtFeedback.Text.Trim().Replace("\n", "<br />");

                EmailSender.StageBitzUrl = Support.GetSystemUrl();
                EmailSender.SendFeedback(
                    (Support.UserFirstName + " " + Support.UserLastName).Trim(),
                    UserEmail,
                    feedbackText);

                plcFeedbackInput.Visible = false;
                plcFeedbackSent.Visible = true;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs bc = GetBreadCrumbsControl();
            bc.AddLink(DisplayTitle, null);
            bc.LoadControl();
        }
    }
}