using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using Telerik.Web.UI;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;

namespace StageBitz.AdminWeb.User
{
    public partial class ActivationsPending : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {                
                LoadData();
                LoadBreadCrumbs();
            }
        }

        private void LoadData()
        {
            var usersList = from u in DataContext.Users
                            join c in DataContext.Countries on u.CountryId equals c.CountryId
                            where u.IsActive == false
                            orderby u.CreatedDate descending
                            select new
                            {
                                UserID = u.UserId,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                RegisteredDate = u.CreatedDate,
                                Email = u.Email1,
                                Status = u.IsActive == true ? "Active" : "Pending Activation",
                            };
            gvUsers.DataSource = usersList;

            litPendingInvitationsCount.Text = string.Format("{0} pending user activation(s)", usersList.Count());
        }


        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Users", "~/User/Users.aspx");
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        protected void gvUsers_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName == "Resend")
            {
                if (e.Item is GridDataItem)
                {                   
                    int userId = int.Parse(e.CommandArgument.ToString());
                    //GridDataItem dataItem = (GridDataItem)e.Item;
                    ResendActivationEmail(userId);
                }
            }
        }

        private void ResendActivationEmail(int userId)
        {
            try
            {
                var user = (from u in DataContext.Users
                            join c in DataContext.Countries on u.CountryId equals c.CountryId
                            where u.UserId == userId && u.IsActive == false
                            orderby u.FirstName, u.LastName
                            select new
                            {
                                UserID = u.UserId,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Email = u.Email1,                                
                                Password = u.Password,
                                Status = u.IsActive == true ? "Active" : "Pending Activation",
                            }).FirstOrDefault();

                //Send activation emails
                if (user != null)
                {

                    string activationLink = Support.GetUserActivationLink(user.Email, user.Password);
                    StageBitz.Common.EmailSender.StageBitzUrl = Utils.GetSystemValue("SBUserWebURL");
                    StageBitz.Common.EmailSender.ReSendUserActivationLink(user.Email, activationLink, user.FirstName);

                }

                lblInfo.Text = string.Format("Activation email sent to {0}", user.Email);
                popupResendSucess.ShowPopup();                
            }
            catch (Exception ex)
            {   
                lblError.Text = string.Format("Error occured. Email sending failed. Error: {0}", ex.Message);
                popupResendFailed.ShowPopup();
            }
        }
    }
}