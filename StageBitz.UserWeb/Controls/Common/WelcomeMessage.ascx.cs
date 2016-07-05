using StageBitz.Common;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for welcome message.
    /// </summary>
    public partial class WelcomeMessage : UserControlBase
    {
        #region Constants and enums

        /// <summary>
        /// The free trial option session key
        /// </summary>
        public const string FreeTrialOptionSessionKey = "FreeTrialOptionKey";

        /// <summary>
        /// All Posible free trial options.
        /// </summary>
        public enum FreeTrialOption
        {
            None = 0,
            ExpectingInvitation = 1,
            CreateNewProject = 2,
            CreateInventory = 3,
            CreateProjectAndInventory = 4
        }

        #endregion Constants and enums

        #region Properties

        /// <summary>
        /// Gets the free trial weeks.
        /// </summary>
        /// <value>
        /// The free trial weeks.
        /// </value>
        public int FreeTrialWeeks
        {
            get
            {
                int weeks = 0;
                Int32.TryParse(Utils.GetSystemValue("FreeTrialDays"), out weeks);
                return weeks / 7;
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
                if (!this.GetBL<ProjectBL>().HasRespondedToWelcomMessage(UserID))
                {
                    if (this.GetBL<ProjectBL>().GetPendingInvitationsCount(UserID) > 0)
                    {
                        popupFirstTimeLoginInvited.ShowPopup();
                    }
                    else
                    {
                        popupFirstTimeLoginDirect.ShowPopup();
                    }
                }
            }

            ScriptManager.RegisterStartupScript(this.Page, GetType(), string.Concat("InitializeRadioButtons_", this.ClientID),
                         string.Concat("InitializeRadioButtons_", this.ClientID, "();"), true);
        }

        /// <summary>
        /// Handles the Click event of the btnGetStartedInvited control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetStartedInvited_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                this.GetBL<ProjectBL>().SetRespondToWelcomeMessage(UserID);
                popupFirstTimeLoginInvited.HidePopup();
                Response.Redirect("~/Default.aspx");
            }
        }

        /// <summary>
        /// Handles the Click event of the FinishButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FinishButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid && !PageBase.StopProcessing)
            {
                FreeTrialOption freeTrialOption = FreeTrialOption.None;
                if (rbtnExpectingInvitation.Checked)
                {
                    freeTrialOption = FreeTrialOption.ExpectingInvitation;
                }
                else if (rbtnCreateNewProject.Checked)
                {
                    this.GetBL<ProjectBL>().CreateFirstCompanyAndFirstProject(txtCompanyName.Text.Trim(), txtProjectName.Text.Trim(), ucCountryList.CountryID, UserID);
                    popupFirstTimeLoginDirect.HidePopup();
                    freeTrialOption = FreeTrialOption.CreateNewProject;
                }
                else if (rbtnCreateInventory.Checked)
                {
                    this.GetBL<ProjectBL>().CreateFirstCompanyAndInventory(txtCompanyName.Text.Trim(), ucCountryList.CountryID, UserID);
                    popupFirstTimeLoginDirect.HidePopup();
                    freeTrialOption = FreeTrialOption.CreateInventory;
                }
                else if (rbtnCreateProjectAndInventory.Checked)
                {
                    this.GetBL<ProjectBL>().CreateFirstCompanyAndFirstProject(txtCompanyName.Text.Trim(), txtProjectName.Text.Trim(), ucCountryList.CountryID, UserID);
                    popupFirstTimeLoginDirect.HidePopup();
                    freeTrialOption = FreeTrialOption.CreateProjectAndInventory;
                }

                SaveFreeTrialOptionToSession(freeTrialOption);
                Response.Redirect("~/Default.aspx");
            }
        }

        /// <summary>
        /// Handles the Click event of the StartNextButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void StartNextButton_Click(object sender, EventArgs e)
        {
            pnlPopupInner.Width = 600;
            if (rbtnExpectingInvitation.Checked)
            {
                popupFirstTimeLoginDirect.HidePopup();
                this.GetBL<ProjectBL>().SetRespondToWelcomeMessage(UserID);
                SaveFreeTrialOptionToSession(FreeTrialOption.ExpectingInvitation);
                Response.Redirect("~/Default.aspx");
            }
            else if (rbtnCreateNewProject.Checked)
            {
                divProjectCreateArea.Visible = true;
                divCurrencyNote.Visible = true;
                lblCompanyCreateDescription.Text = "First you need a Company that will own the Project.";
                lblPackageInfo.Text = string.Format("<b>The free trial gives you {0} Project for {1} weeks.</b>", Utils.GetSystemValue("DefaultProjectCountForCompany"), FreeTrialWeeks);
                lblHelpTipInfo2.Text = "For help on how to invite other people to be Adminstrators for your Company," +
                    " or Team members on your Projects, just use the Feedback and Support tab at the bottom edge of each screen.";
                lblHelpTipInfo1.Text = "This gives you control over your Company, the Projects that are created for it and the team you invite to those Projects.";
                popupFirstTimeLoginDirect.Title = "Let’s get your free trial started and create your first Project.";
                ltrlWizardStep2Footer.Text = "Project and you can invite the rest of your team to work on your Project with you.";
            }
            else if (rbtnCreateInventory.Checked)
            {
                divProjectCreateArea.Visible = false;
                divCurrencyNote.Visible = false;
                lblPackageInfo.Text = string.Format("<b>You can create up to {0} Items for free and start sharing them with other Companies.</b>", Utils.GetSystemValue("DefaultInventoryCountForCompany")); ;
                lblCompanyCreateDescription.Text = "First let's create a Company.";
                lblHelpTipInfo2.Text = "For help on how to invite other people to be Adminstrators for your Company," +
                        " or Inventory Administrator, just use the Feedback and Support tab at the bottom edge of each screen.";
                lblHelpTipInfo1.Text = "This gives you control over your Company, its Inventory and the team you invite to help manage the Company.";
                popupFirstTimeLoginDirect.Title = "Let’s create a Company that will own the Inventory.";
                ltrlWizardStep2Footer.Text = "your Inventory and start sharing them with your team and other Companies." +
                        "<br /><br /> <p>Remember, you can download the StageBitz app from the <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppITuneUrl") + "'>App Store</a> or the <a target='_blank' href='" +
                        Utils.GetSystemValue("MobileAppGooglePlayUrl") + "'>Google Play</a> to add " +
                        "Inventory Items directly from the camera on your mobile device and get your Inventory up and running!</p>";
            }
            else if (rbtnCreateProjectAndInventory.Checked)
            {
                divProjectCreateArea.Visible = true;
                divCurrencyNote.Visible = true;

                lblPackageInfo.Text = string.Format("<b>Here’s what you get for your free trial:</b><br/>" +
                        "{0} weeks free subscription to a Project (which you can keep if you continue your subscription).<br/>" +
                        "Up to {1} Inventory Items free (which will stay free after the trial).",
                        FreeTrialWeeks, Utils.GetSystemValue("DefaultInventoryCountForCompany"));
                lblCompanyCreateDescription.Text = "First you need a Company that will own the Project and Inventory.";
                lblHelpTipInfo2.Text = "For help on how to invite other people to be Adminstrators for your Company," +
                        " Inventory Administrator or Team members on your Projects, just use the Feedback and Support tab at the bottom edge of each screen.";
                lblHelpTipInfo1.Text = "This gives you control over your Company, the Projects that are created for it and the team you invite to those Projects and to help manage the Company.";
                popupFirstTimeLoginDirect.Title = "Let’s get your free trial started and create your first Project and Inventory.";
                ltrlWizardStep2Footer.Text = "Project and Inventory, invite the rest of your team to work on your Project with" +
                        " you and share your Inventory with your team and other Companies.";
            }
        }

        /// <summary>
        /// Handles the Click event of the FinishPreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FinishPreviousButton_Click(object sender, EventArgs e)
        {
            popupFirstTimeLoginDirect.Title = "Welcome to StageBitz";
            pnlPopupInner.Width = 850;

            Control container = wizard.FindControl("StartNavigationTemplateContainerID") as Control;
            Button StepNextButton = container.FindControl("StartNextButton") as Button;
            StepNextButton.Enabled = true;
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Saves the free trial option to session.
        /// </summary>
        /// <param name="freeTrialOption">The free trial option.</param>
        private void SaveFreeTrialOptionToSession(FreeTrialOption freeTrialOption)
        {
            Session[FreeTrialOptionSessionKey] = freeTrialOption;
        }

        #endregion Private Methods
    }
}