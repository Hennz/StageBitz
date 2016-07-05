using StageBitz.Common;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Personal
{
    /// <summary>
    /// Web page for user details.
    /// </summary>
    public partial class UserDetails : PageBase
    {
        #region Properties

        /// <summary>
        /// Gets the view user identifier.
        /// </summary>
        /// <value>
        /// The view user identifier.
        /// </value>
        private int ViewUserId
        {
            get
            {
                if (ViewState["ViewUserId"] == null)
                {
                    int userId = UserID;

                    if (Request["userId"] != null)
                    {
                        int.TryParse(Request["userId"], out userId);
                    }

                    ViewState["ViewUserId"] = userId;
                }

                return (int)ViewState["ViewUserId"];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        private bool IsReadOnly
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

        /// <summary>
        /// Gets the last updated date of the original record which was used to load data on the page.
        /// Used for concurrency handling.
        /// </summary>
        private DateTime OriginalLastUpdatedDate
        {
            get
            {
                return (DateTime)ViewState["LastUpdatedDate"];
            }
            set
            {
                ViewState["LastUpdatedDate"] = value;
            }
        }

        #endregion Properties

        #region Fields

        /// <summary>
        /// The pending code identifier
        /// </summary>
        private readonly int pendingCodeId = Support.GetCodeByValue("EmailChangeRequestStatus", "PENDING").CodeId;

        /// <summary>
        /// The canceled code identifier
        /// </summary>
        private readonly int canceledCodeId = Support.GetCodeByValue("EmailChangeRequestStatus", "CANCELED").CodeId;

        #endregion Fields

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">Permission denied.</exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                User user = DataContext.Users.Where(u => u.UserId == ViewUserId).FirstOrDefault();

                #region Permission check and Set Read Only flag

                //If the viewing-user is the currently logged in user load the page in edit mode.
                //Else if the viewing user is in contacts of the logged-in user, load the page in readonly mode.
                //Else throw 'no permission' exception

                if (ViewUserId == UserID)
                {
                    IsReadOnly = false;
                }
                else
                {
                    if (!Support.CanAccessUser(user))
                    {
                        throw new Exception("Permission denied.");
                    }
                    else
                    {
                        IsReadOnly = true;
                    }
                }

                #endregion Permission check and Set Read Only flag

                DisplayTitle = IsReadOnly ? "User Details" : "Personal Profile";
                divPrivacyNotice.Visible = !IsReadOnly;

                if (!IsReadOnly)
                {
                    pnlPersonalDetails.DefaultButton = "btnSavePersonalDetails";
                }

                #region Setup Image Upload control

                //Setup image upload control
                upnlImageUpload.Visible = !IsReadOnly;
                if (!IsReadOnly)
                {
                    fileUpload.RelatedTableName = "User";
                    fileUpload.RelatedId = ViewUserId;
                    fileUpload.UploadMediaType = UserWeb.Controls.Common.FileUpload.MediaType.Images;
                    fileUpload.IsReadOnly = IsReadOnly;
                    fileUpload.LoadUI();
                }

                #endregion Setup Image Upload control

                LoadData(user);

                userProjects.ViewUserId = ViewUserId;
                userProjects.LoadData();

                userSkills.ViewUserId = ViewUserId;
                userSkills.IsReadOnly = IsReadOnly;
                userSkills.LoadData();

                sbUserEmailNotifications.ViewUserId = UserID;
                sbUserEmailNotifications.IsReadOnly = IsReadOnly;
                sbUserEmailNotifications.LoadData();

                LoadBreadCrumbs();
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fileUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fileUpload_FileUploaded(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                LoadProfileThumbnail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveImageConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveImageConfirm_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                int profileImageId = (from m in DataContext.DocumentMedias
                                      where m.RelatedTableName == "User" && m.RelatedId == ViewUserId && m.SortOrder == 1
                                      select m.DocumentMediaId).FirstOrDefault();

                DataContext.DeleteDocumentMedia(profileImageId);
                popupImageRemoveConfirmation.HidePopup();

                LoadProfileThumbnail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSavePersonalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSavePersonalDetails_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                User user = DataContext.Users.Where(u => u.UserId == ViewUserId && u.LastUpdatedDate == OriginalLastUpdatedDate).FirstOrDefault();

                if (user == null)
                {
                    StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.UserDetails, ViewUserId));
                }

                if (this.IsValid)
                {
                    if (this.IsPageDirty)
                    {
                        user.FirstName = txtFirstName.Text.Trim();
                        user.LastName = txtLastName.Text.Trim();
                        user.Position = txtPosition.Text.Trim();
                        user.Company = txtCompany.Text.Trim();
                        user.Email2 = txtEmail2.Text.Trim();
                        user.IsEmailVisible = chkEmailVisible.Checked;
                        user.Phone1 = txtPhone1.Text.Trim();
                        user.Phone2 = txtPhone2.Text.Trim();
                        user.AddressLine1 = txtAddressLine1.Text.Trim();
                        user.AddressLine2 = txtAddressLine2.Text.Trim();
                        user.City = txtCity.Text.Trim();
                        user.State = txtState.Text.Trim();
                        user.PostCode = txtPostCode.Text.Trim();
                        user.CountryId = countryList.CountryID;

                        DataContext.SaveChanges();

                        LoadBasicDetails(user);

                        Support.SetUserSessionData(user);
                        ((Content)Master).UpdateUserNameDisplay();

                        //Save skills
                        userSkills.SaveChanges();

                        this.IsPageDirty = false;
                    }

                    ShowNotification("personalDetailsSavedNotice");
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdatePassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Page.Validate("changePasswordFields");
                if (!Page.IsValid)
                {
                    txtCurrentPassword.Text = string.Empty;
                    txtNewPassword.Text = string.Empty;
                    txtConfirmPassword.Text = string.Empty;
                    return;
                }

                string currentPasswordHash = Utils.HashPassword(txtCurrentPassword.Text);
                string newPassword = txtNewPassword.Text;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    StageBitz.Data.User user = DataContext.Users.Where(u => u.UserId == UserID && u.Password == currentPasswordHash).FirstOrDefault();

                    if (user == null) //Invalid password
                    {
                    }
                    else
                    {
                        user.Password = Utils.HashPassword(newPassword);
                        DataContext.SaveChanges();

                        txtCurrentPassword.Text = string.Empty;
                        txtNewPassword.Text = string.Empty;
                        txtConfirmPassword.Text = string.Empty;

                        ShowNotification("passwordUpdatedNotice");
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the cusvalCurrentPassword control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cusvalCurrentPassword_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string currentPasswordHash = Utils.HashPassword(txtCurrentPassword.Text);
            StageBitz.Data.User user = DataContext.Users.Where(u => u.UserId == UserID && u.Password == currentPasswordHash).FirstOrDefault();

            args.IsValid = (user != null);
        }

        /// <summary>
        /// Handles the Click event of the lbtnChangePrimaryEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbtnChangePrimaryEmail_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                popupEditPrimaryEmail.Title = "Change Primary Email";
                txtNewEmail.Text = string.Empty;
                ltrlEmailRequestError.Text = string.Empty;
                divEmailRequestError.Visible = false;
                popupEditPrimaryEmail.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSavePrimaryEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSavePrimaryEmail_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                divEmailRequestError.Visible = false;
                ltrlEmailRequestError.Text = string.Empty;
                string email = txtNewEmail.Text.Trim();

                User user = DataContext.Users.Where(u => u.UserId == UserID).FirstOrDefault();
                if (!ValidateEmail(user, email))
                {
                    return;
                }

                if (user != null && !string.IsNullOrEmpty(email))
                {
                    EmailChangeRequest emailChangeRequest = new EmailChangeRequest
                    {
                        Email = email,
                        StatusCode = pendingCodeId,
                        UserId = UserID
                    };

                    DataContext.EmailChangeRequests.AddObject(emailChangeRequest);
                    DataContext.SaveChanges();

                    string activationLink = Support.GetUserPrimaryEmailChangeLink(email, user.Password,
                            emailChangeRequest.EmailChangeRequestId);

                    //Activation link will be sent to the user.
                    StageBitz.Common.EmailSender.StageBitzUrl = Support.GetSystemUrl();
                    StageBitz.Common.EmailSender.SendUserPrimaryEmailChange(
                            email, activationLink, user.FirstName);

                    iBtnEmailAlreadySent.Visible = true;
                    iBtnEmailAlreadySent.ToolTip = string.Concat("Verification pending for email address '",
                            emailChangeRequest.Email, "'. ", "Click icon for more details.");
                    lbtnChangePrimaryEmail.Visible = false;

                    popupEditPrimaryEmail.HidePopup();
                }
                else
                {
                    divEmailRequestError.Visible = true;
                    ltrlEmailRequestError.Text = "User not exists.";
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the iBtnEmailAlreadySent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void iBtnEmailAlreadySent_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                         join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                         where r.StatusCode == pendingCodeId && r.UserId == UserID
                                                         select r).FirstOrDefault();

                if (emailChangeRequest != null)
                {
                    popupResendEmail.Title = "Primary Email Verification";
                    ltrlResendEmail.Text = "Verification pending for email address '" + emailChangeRequest.Email + "'.";
                    ltrlResendError.Text = string.Empty;
                    divResendError.Visible = false;

                    popupResendEmail.ShowPopup();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnResendEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnResendEmail_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                ltrlResendError.Text = string.Empty;
                divResendError.Visible = false;

                User user = DataContext.Users.Where(u => u.UserId == ViewUserId).FirstOrDefault();
                if (user != null)
                {
                    EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                             join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                             where r.StatusCode == pendingCodeId && r.UserId == UserID
                                                             select r).FirstOrDefault();

                    if (emailChangeRequest != null)
                    {
                        emailChangeRequest.StatusCode = canceledCodeId;
                        EmailChangeRequest newRequest = new EmailChangeRequest
                        {
                            Email = emailChangeRequest.Email,
                            UserId = emailChangeRequest.UserId,
                            StatusCode = pendingCodeId
                        };

                        DataContext.EmailChangeRequests.AddObject(newRequest);
                        DataContext.SaveChanges();

                        string activationLink = Support.GetUserPrimaryEmailChangeLink(
                                emailChangeRequest.Email, user.Password, newRequest.EmailChangeRequestId);

                        //Activation link will be sent to the user.
                        StageBitz.Common.EmailSender.StageBitzUrl = Support.GetSystemUrl();
                        StageBitz.Common.EmailSender.SendUserPrimaryEmailChange(
                                emailChangeRequest.Email, activationLink, user.FirstName);
                        popupResendEmail.HidePopup();
                    }
                    else
                    {
                        ltrlResendError.Text = "Verification email resend failed.";
                        divResendError.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelRequest_Click(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                ltrlResendError.Text = string.Empty;
                divResendError.Visible = false;

                EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                         join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                         where r.StatusCode == pendingCodeId && r.UserId == UserID
                                                         select r).FirstOrDefault();
                if (emailChangeRequest != null)
                {
                    emailChangeRequest.StatusCode = canceledCodeId;
                    DataContext.SaveChanges();
                    popupResendEmail.HidePopup();

                    iBtnEmailAlreadySent.Visible = false;
                    iBtnEmailAlreadySent.ToolTip = string.Empty;
                    lbtnChangePrimaryEmail.Visible = true;
                }
                else
                {
                    ltrlResendError.Text = "Primary email change request cancelation failed.";
                    divResendError.Visible = true;
                }
            }
        }

        #endregion Events

        #region Support Methods

        /// <summary>
        /// Validates the email.
        /// </summary>
        /// <param name="email">The email.</param>
        private bool ValidateEmail(User user, string email)
        {
            bool isValied = true;

            if (user.Email1 == email)
            {
                divEmailRequestError.Visible = true;
                ltrlEmailRequestError.Text = "Please enter a different email address.";
                isValied = false;
            }

            EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                     join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                     where r.StatusCode == pendingCodeId && r.UserId == user.UserId
                                                     select r).FirstOrDefault();

            var usersWithSameEmail = (from u in DataContext.Users
                                      where u.LoginName.Equals(email, StringComparison.InvariantCultureIgnoreCase)
                                      select u);

            if (usersWithSameEmail.Count() > 0)
            {
                divEmailRequestError.Visible = true;
                ltrlEmailRequestError.Text = "This email address is already in use.";
                isValied = false;
            }

            if (emailChangeRequest != null)
            {
                divEmailRequestError.Visible = true;
                ltrlEmailRequestError.Text = "Primary email change request already sent.";
                isValied = false;
            }

            return isValied;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="user">The user.</param>
        private void LoadData(User user)
        {
            OriginalLastUpdatedDate = user.LastUpdatedDate.Value;

            LoadImageArea(user);
            LoadPersonalDetails(user);

            profileTabStrip.FindTabByValue("ChangePassword").Visible = !IsReadOnly;
            profileTabStrip.FindTabByValue("Notifications").Visible = !IsReadOnly;
        }

        /// <summary>
        /// Loads the profile thumbnail.
        /// </summary>
        private void LoadProfileThumbnail()
        {
            int profileImageId = (from m in DataContext.DocumentMedias
                                  where m.RelatedTableName == "User" && m.RelatedId == ViewUserId && m.SortOrder == 1
                                  select m.DocumentMediaId).FirstOrDefault();

            bool hasProfileImage = (profileImageId != 0);
            divRemoveImage.Visible = hasProfileImage;
            popupImageRemoveConfirmation.Visible = hasProfileImage;

            profileImage.DocumentMediaId = profileImageId;

            upnlThumbnail.Update();
            upnlImageUpload.Update();
        }

        /// <summary>
        /// Loads the basic details.
        /// </summary>
        /// <param name="user">The user.</param>
        private void LoadBasicDetails(User user)
        {
            ltrlName.Text = Support.TruncateString((user.FirstName + " " + user.LastName).Trim(), 100);

            ltrlHeadingPosition.Text = string.IsNullOrEmpty(user.Position) ? string.Empty : string.Format("<strong>{0}</strong><br />", Support.TruncateString(user.Position, 100));
            ltrlHeadingCompany.Text = string.IsNullOrEmpty(user.Company) ? string.Empty : string.Format("<strong>{0}</strong><br />", Support.TruncateString(user.Company, 100));

            ltrlMembershipPeriod.Text = string.Format("Member of StageBitz™ since {0}", user.CreatedDate.Value.Year);

            upnlBasicDetails.Update();
        }

        /// <summary>
        /// Loads the image area.
        /// </summary>
        /// <param name="user">The user.</param>
        private void LoadImageArea(User user)
        {
            LoadProfileThumbnail();
            LoadBasicDetails(user);
        }

        /// <summary>
        /// Loads the personal details.
        /// </summary>
        /// <param name="user">The user.</param>
        private void LoadPersonalDetails(User user)
        {
            divPersonalDetailsEditable.Visible = divPersonalDetailsEditableLabels.Visible = !IsReadOnly;
            divPersonalDetailsReadOnly.Visible = divPersonalDetailsReadOnlyLabels.Visible = IsReadOnly;
            btnSavePersonalDetails.Visible = !IsReadOnly;

            EmailChangeRequest emailChangeRequest = (from r in DataContext.EmailChangeRequests
                                                     join c in DataContext.Codes on r.StatusCode equals c.CodeId
                                                     where r.StatusCode == pendingCodeId && r.UserId == UserID
                                                     select r).FirstOrDefault();

            if (emailChangeRequest != null)
            {
                iBtnEmailAlreadySent.Visible = true;
                iBtnEmailAlreadySent.ToolTip = string.Concat("Verification pending for email address '",
                        emailChangeRequest.Email, "'. ", "Click icon for more details.");

                lbtnChangePrimaryEmail.Visible = false;
            }

            if (IsReadOnly)
            {
                plcEmailLabels.Visible = user.IsEmailVisible;
                plcEmailTexts.Visible = user.IsEmailVisible;

                ltrlFirstName.Text = Support.TruncateString(user.FirstName, 50);
                ltrlLastName.Text = Support.TruncateString(user.LastName, 50);
                ltrlPosition.Text = Support.TruncateString(user.Position, 50);
                ltrlCompany.Text = Support.TruncateString(user.Company, 50);

                if (user.IsEmailVisible)
                {
                    hypEmail1.Text = Support.TruncateString(user.Email1, 50);
                    hypEmail1.NavigateUrl = "mailto:" + user.Email1;
                }

                hypEmail2.Text = Support.TruncateString(user.Email2, 50);
                hypEmail2.NavigateUrl = "mailto:" + user.Email2;

                ltrlPhone1.Text = Support.TruncateString(user.Phone1, 50);
                ltrlPhone2.Text = Support.TruncateString(user.Phone2, 50);

                #region Build Address string

                StringBuilder sbAddress = new StringBuilder();
                if (!string.IsNullOrEmpty(user.AddressLine1))
                    sbAddress.Append(Support.TruncateString(user.AddressLine1, 50) + "<br />");
                if (!string.IsNullOrEmpty(user.AddressLine2))
                    sbAddress.Append(Support.TruncateString(user.AddressLine2, 50) + "<br />");
                if (!string.IsNullOrEmpty(user.City))
                    sbAddress.Append(Support.TruncateString(user.City, 50) + "<br />");
                if (!string.IsNullOrEmpty(user.State))
                    sbAddress.Append(Support.TruncateString(user.State, 50) + "<br />");
                if (user.CountryId != null)
                    sbAddress.Append(Support.TruncateString(user.Country.CountryName, 50) + "<br />");
                if (!string.IsNullOrEmpty(user.PostCode))
                    sbAddress.Append(Support.TruncateString(user.PostCode, 50) + "<br />");

                ltrlAddress.Text = sbAddress.ToString();

                #endregion Build Address string
            }
            else
            {
                txtFirstName.Text = user.FirstName;
                txtLastName.Text = user.LastName;
                txtPosition.Text = user.Position;
                txtCompany.Text = user.Company;
                ltrlEmail1.Text = user.Email1;
                txtCurrentEmail.Text = user.Email1;
                txtEmail2.Text = user.Email2;
                chkEmailVisible.Checked = user.IsEmailVisible;
                txtPhone1.Text = user.Phone1;
                txtPhone2.Text = user.Phone2;
                txtAddressLine1.Text = user.AddressLine1;
                txtAddressLine2.Text = user.AddressLine2;
                txtCity.Text = user.City;
                txtState.Text = user.State;
                txtPostCode.Text = user.PostCode;

                if (user.CountryId.HasValue)
                    countryList.CountryID = user.CountryId.Value;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadcrumbs = GetBreadCrumbsControl();
            breadcrumbs.AddLink(DisplayTitle, null);
            breadcrumbs.LoadControl();
        }

        #endregion Support Methods
    }
}