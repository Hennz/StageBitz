using StageBitz.Logic.Business.Personal;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web;
using System.Web.Security;

namespace StageBitz.UserWeb.Public
{
    /// <summary>
    /// Landing page for accept invitations
    /// </summary>
    public partial class Invitation : PageBase
    {
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
                if (Request["invitationCode"] == null)
                {
                    return string.Empty;
                }

                return Request["invitationCode"];
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
                string invitationParam = (InvitationCode == string.Empty) ? string.Empty : ("?invitationCode=" + InvitationCode);

                //If the user is logged in, perform a force logout and navigate to the login page.
                //Then navigate to Login page.

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    FormsAuthentication.SignOut();
                    Session.Abandon();
                }

                if (InvitationCode != string.Empty)
                {
                    Data.User user = this.GetBL<PersonalBL>().GetRegisteredUserFortheInvitation(InvitationCode);

                    if (user != null)
                    {
                        //Check whether User is Pending activation
                        if (!user.IsActive)
                        {
                            user.IsActive = true;
                            this.GetBL<PersonalBL>().SaveChanges();
                        }
                        Response.Redirect(FormsAuthentication.LoginUrl + invitationParam);
                    }
                    else
                        Response.Redirect("~/Account/Register.aspx" + invitationParam);
                }
                else
                    Response.Redirect("~/Account/Register.aspx" + invitationParam);
            }
        }
    }
}