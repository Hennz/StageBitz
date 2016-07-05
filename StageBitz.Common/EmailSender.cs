using StageBitz.Data;
using StageBitz.Data.DataTypes;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace StageBitz.Common
{
    /// <summary>
    /// Helper class for email send.
    /// </summary>
    public static class EmailSender
    {
        /// <summary>
        /// Enum for team user type.
        /// </summary>
        public enum ProjectTeamUserType
        {
            Observer,
            Staff
        }

        /// <summary>
        /// Gets or sets the stage bitz image path.
        /// </summary>
        /// <value>
        /// The stage bitz image path.
        /// </value>
        public static string StageBitzImagePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the stage bitz URL.
        /// </summary>
        /// <value>
        /// The stage bitz URL.
        /// </value>
        public static string StageBitzUrl
        {
            get;
            set;
        }

        #region EMAIL TYPES

        #region Registeration emails

        /// <summary>
        /// Sends an account activation email to the newly registered user.
        /// </summary>
        /// <param name="toEmail">Email address of the receiver</param>
        /// <param name="link">Link that will relates to as "Activate account"</param>
        /// <param name="username">Fistname of the registered user</param>
        public static void SendUserActivationLink(string toEmail, string link, string username)
        {
            int userActivationEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "USERACTIVATION");
            Hashtable parameters = new Hashtable();
            parameters["Link"] = link;
            parameters["Username"] = username;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(userActivationEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends an user registration email to stagebitz admins.
        /// </summary>
        /// <param name="toEmail">Email address of the receiver</param>
        /// <param name="link">Link that will relates to as "Admin portial"</param>
        /// <param name="username">Fistname of the receiver</param>
        /// /// <param name="registeredUser">FullName of the registered user</param>
        public static void SendUserRegistrationMailToStageBitzAdmin(string toEmail, string link, string registeredUser)
        {
            int userRegistrationEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "USERREGISTRATION");
            Hashtable parameters = new Hashtable();
            parameters["Link"] = link;
            parameters["RegisteredUser"] = registeredUser;
            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(userRegistrationEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends a company registration email to stagebitz admins.
        /// </summary>
        /// <param name="toEmail">Email address of the receiver</param>
        /// <param name="link">Link that will relates to the "Admin portial"</param>
        /// <param name="username">Fistname of the receiver</param>
        /// <param name="companyName">Name of the company</param>
        /// <param name="registeredUser">FullName of the registered user</param>
        public static void SendCompanyRegistrationMailToStageBitzAdmin(string toEmail, string link, string companyName, string createdUser)
        {
            int companyRegistrationEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "COMPANYREGISTRATION");
            Hashtable parameters = new Hashtable();
            parameters["Link"] = link;
            parameters["CompanyName"] = companyName;
            parameters["CreatedUser"] = createdUser;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(companyRegistrationEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends an account activation email to the newly registered user.
        /// </summary>
        /// <param name="toEmail">Email address of the receiver</param>
        /// <param name="link">Link that will relates to as "Activate account"</param>
        /// <param name="username">Fistname of the registered user</param>
        public static void ReSendUserActivationLink(string toEmail, string link, string username)
        {
            int userActivationEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "RESENDACTIVATION");
            string feedbackEmail = Utils.GetSystemValue("FeedbackEmail");
            Hashtable parameters = new Hashtable();
            parameters["Link"] = link;
            parameters["Username"] = username;
            parameters["FeedbackEmail"] = feedbackEmail;
            parameters["FeedBackAndTechSupportURL"] = Utils.GetSystemValue("FeedBackAndTechSupportURL");
            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(userActivationEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the forgot password link.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="link">The link.</param>
        /// <param name="username">The username.</param>
        public static void SendForgotPasswordLink(string toEmail, string link, string username)
        {
            int forgotPasswordEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "FORGOTPASSWORD");
            Hashtable parameters = new Hashtable();
            parameters["Link"] = link;
            parameters["Username"] = username;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(forgotPasswordEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        #endregion Registeration emails

        #region Invitations

        /// <summary>
        /// Invites the project team new user.
        /// </summary>
        /// <param name="userType">Type of the user.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="projectRole">The project role.</param>
        /// <param name="invitationUrl">The invitation URL.</param>
        public static void InviteProjectTeamNewUser(ProjectTeamUserType userType, string toEmail, string toPersonName, string fromPersonName, string fromPersonEmail, string companyName, string projectName, string projectRole, string invitationUrl)
        {
            int inviteProjectTeamEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", (userType == ProjectTeamUserType.Observer ? "PROJECTINV_OBSERVER_NEWUSER" : "PROJECTINV_STAFF_NEWUSER"));
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["FromPersonEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["ProjectName"] = projectName;
            parameters["ProjectRole"] = projectRole;
            parameters["InvitationUrl"] = invitationUrl;

            //Load email text contents
            GetEmailContentFromTemplate(inviteProjectTeamEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Invites the project team existing user.
        /// </summary>
        /// <param name="userType">Type of the user.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="projectRole">The project role.</param>
        /// <param name="dashboardUrl">The dashboard URL.</param>
        public static void InviteProjectTeamExistingUser(ProjectTeamUserType userType, string toEmail, string toPersonName, string fromPersonName, string fromPersonEmail, string companyName, string projectName, string projectRole, string dashboardUrl)
        {
            int inviteProjectTeamEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", (userType == ProjectTeamUserType.Observer ? "PROJECTINV_OBSERVER_EXUSER" : "PROJECTINV_STAFF_EXUSER"));
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["FromPersonEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["ProjectName"] = projectName;
            parameters["ProjectRole"] = projectRole;
            parameters["DashboardUrl"] = dashboardUrl;
            parameters["FeedBackAndTechSupportURL"] = Utils.GetSystemValue("FeedBackAndTechSupportURL");

            //Load email text contents
            GetEmailContentFromTemplate(inviteProjectTeamEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Invites the company admin new user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonFirstName">First name of from person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="invitationUrl">The invitation URL.</param>
        public static void InviteCompanyAdminNewUser(string toEmail, string toPersonName, string fromPersonFirstName, string fromPersonName, string fromPersonEmail,
                string companyName, string invitationUrl)
        {
            int inviteCompanyEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "COMPANYADMININV_NEWUSER");

            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["FromPersonFirstName"] = fromPersonFirstName;
            parameters["FromPersonEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["InvitationUrl"] = invitationUrl;

            //Load email text contents
            GetEmailContentFromTemplate(inviteCompanyEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Invites the company admin existing user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonFirstName">First name of from person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="dashboardUrl">The dashboard URL.</param>
        public static void InviteCompanyAdminExistingUser(string toEmail, string toPersonName, string fromPersonFirstName, string fromPersonName,
                string fromPersonEmail, string companyName, string dashboardUrl)
        {
            int inviteCompanyEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "COMPANYADMININV_EXUSER");

            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["FromPersonFirstName"] = fromPersonFirstName;
            parameters["FromPersonEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["DashboardUrl"] = dashboardUrl;
            parameters["FeedBackAndTechSupportURL"] = Utils.GetSystemValue("FeedBackAndTechSupportURL");

            //Load email text contents
            GetEmailContentFromTemplate(inviteCompanyEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Invites the inventory staff existing user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="invitationUrl">The invitation URL.</param>
        public static void InviteInventoryUserExistingUser(string toEmail, string toPersonName, string fromPersonName,
               string fromPersonEmail, string companyName, string invitationUrl,
               string inventoryStaffContent, string inventoryObserverContent, string allNoAccessContent)
        {
            int inviteCompanyEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "INVENTORYUSERINV_EXUSER");

            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["FromUserName"] = fromPersonName;
            parameters["FromUserEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["InvitationUrl"] = invitationUrl;
            parameters["InventoryStaffContent"] = inventoryStaffContent;
            parameters["InventoryObserverContent"] = inventoryObserverContent;
            parameters["AllNoAccessContent"] = allNoAccessContent;

            //Load email text contents
            GetEmailContentFromTemplate(inviteCompanyEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Invites the inventory user new user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="invitationUrl">The dashboard URL.</param>
        public static void InviteInventoryUserNewUser(string toEmail, string toPersonName, string fromPersonName,
               string fromPersonEmail, string companyName, string invitationUrl,
               string inventoryStaffContent, string inventoryObserverContent, string allNoAccessContent)
        {
            int inviteCompanyEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "INVENTORYUSERINV_NEWUSER");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["FromUserName"] = fromPersonName;
            parameters["FromUserEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["InvitationUrl"] = invitationUrl;
            parameters["InventoryStaffContent"] = inventoryStaffContent;
            parameters["InventoryObserverContent"] = inventoryObserverContent;
            parameters["AllNoAccessContent"] = allNoAccessContent;

            //Load email text contents
            GetEmailContentFromTemplate(inviteCompanyEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Changes the permission inventory user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        public static void UpgradeToInventoryAdmin(string toEmail, string toPersonName, string fromPersonName,
               string fromPersonEmail, string companyName)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "UPGRADEINVENTORYADMIN");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["AdminName"] = fromPersonName;
            parameters["AdminEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Downgrades the inventory admin.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="locationContent">Content of the location.</param>
        public static void DowngradeInventoryAdmin(string toEmail, string toPersonName, string fromPersonName,
               string fromPersonEmail, string companyName, string locationContent)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "DOWNGRADEINVENTORYADMIN");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["AdminName"] = fromPersonName;
            parameters["AdminEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["LocationContent"] = locationContent;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Send email for downgrades the location manager.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="location">The location.</param>
        public static void DowngradeLocationManager(string toEmail, string toPersonName, string fromPersonName,
              string fromPersonEmail, string companyName, string location)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "DOWNGRADELOCATIONMANAGER");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["AdminName"] = fromPersonName;
            parameters["AdminEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["Location"] = location;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Changes the permission of inventory user.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="noChange">The no change.</param>
        /// <param name="upgradeToLM">The upgrade to lm.</param>
        /// <param name="upgradeToIS">The upgrade to is.</param>
        /// <param name="upgradeToIO">The upgrade to io.</param>
        /// <param name="downgradeToIS">The downgrade to is.</param>
        /// <param name="downgradeToIO">The downgrade to io.</param>
        /// <param name="downgradeToNoAccess">The downgrade to no access.</param>
        public static void ChangePermissionInventoryUser(string toEmail, string toPersonName, string fromPersonName,
               string fromPersonEmail, string companyName, string noChange,
               string upgradeToLM, string upgradeToIS, string upgradeToIO,
               string downgradeToIS, string downgradeToIO, string downgradeToNoAccess)
        {
            int inviteCompanyEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "CHANGEPERMISSIONINVENTORYUSER");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = toPersonName;
            parameters["AdminName"] = fromPersonName;
            parameters["AdminEmail"] = fromPersonEmail;
            parameters["CompanyName"] = companyName;
            parameters["UpgradeToLM"] = upgradeToLM;
            parameters["UpgradeToIS"] = upgradeToIS;
            parameters["UpgradeToIO"] = upgradeToIO;
            parameters["DowngradeToIS"] = downgradeToIS;
            parameters["DowngradeToIO"] = downgradeToIO;
            parameters["DowngradeToNoAccess"] = downgradeToNoAccess;
            parameters["NoChange"] = noChange;
            //Load email text contents
            GetEmailContentFromTemplate(inviteCompanyEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the company admin invitation declined notice.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="invitedPersonName">Name of the invited person.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="templateCodeId">The template code identifier.</param>
        public static void SendCompanyAdminInvitationDeclinedNotice(string toEmail, string toPersonName, string invitedPersonName, string companyName, int templateCodeId)
        {
            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["InvitedPersonName"] = invitedPersonName;
            parameters["CompanyName"] = companyName;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(templateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the project team invitation declined notice.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="invitedPersonName">Name of the invited person.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="projectRole">The project role.</param>
        /// <param name="companyName">Name of the company.</param>
        public static void SendProjectTeamInvitationDeclinedNotice(string toEmail, string toPersonName, string invitedPersonName, string projectName, string projectRole, string companyName)
        {
            int templateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "PROJECTINV_DECLINED");
            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["InvitedPersonName"] = invitedPersonName;
            parameters["ProjectName"] = projectName;
            parameters["ProjectRole"] = projectRole;
            parameters["CompanyName"] = companyName;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(templateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        #endregion Invitations

        /// <summary>
        /// Sends the feedback.
        /// </summary>
        /// <param name="personName">Name of the person.</param>
        /// <param name="personEmail">The person email.</param>
        /// <param name="feedbackText">The feedback text.</param>
        public static void SendFeedback(string personName, string personEmail, string feedbackText)
        {
            Hashtable parameters = new Hashtable();
            parameters["PersonName"] = personName;
            parameters["PersonEmail"] = personEmail;
            parameters["FeedbackText"] = feedbackText;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Send feedback email to admin
            string adminEmail = Utils.GetSystemValue("FeedbackEmail");
            int adminTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "FEEDBACK_ADMIN");
            GetEmailContentFromTemplate(adminTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(adminEmail, subject, messageBody);

            subject = messageBody = string.Empty;

            //Send a copy of the feedback to the user
            int userTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "FEEDBACK_USER");
            GetEmailContentFromTemplate(userTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(personEmail, subject, messageBody);
        }

        /// <summary>
        /// Queries the inventory manager.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="isSendToLocationManager">if set to <c>true</c> [is send to location manager].</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="generalquestions">The generalquestions.</param>
        /// <param name="itemQuestuions">The item questuions.</param>
        /// <param name="supportEmail">The support email.</param>
        public static void QueryInventoryManager(string toEmail, string toPersonName, bool isSendToLocationManager, string fromPersonName, string companyName, string generalquestions, string itemQuestuions, string supportEmail)
        {
            string contentToInventoryAdmin = string.Empty;
            if (isSendToLocationManager)
            {
                contentToInventoryAdmin = string.Empty;
            }
            else
            {
                contentToInventoryAdmin = @"<p>An enquiry has come through that relates to items from more than one Managed Location in your inventory.
                                            If enquiries only have one Managed Location in them, we send them directly to that Location Manager.
                                            However, if the enquiry is related to more than one Managed Location, we send the enquiry to you so you
                                            can decide who should take the lead on responding to the customer.</p>";
            }

            int queryInventoryManagerEmailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "INVENTORYMGRQUERYFORITEMDETAILS");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToInventoryAdminContent"] = contentToInventoryAdmin;
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["CompanyName"] = companyName;
            parameters["generalquestions"] = generalquestions;
            parameters["itemQuestuions"] = itemQuestuions;
            parameters["SupportEmail"] = supportEmail;

            //Load email text contents
            GetEmailContentFromTemplate(queryInventoryManagerEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "INVENTORYQUERY", null);
        }

        /// <summary>
        /// Requests to share inventory.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="fromPersonEmail">From person email.</param>
        /// <param name="fromcompanyName">Name of the fromcompany.</param>
        /// <param name="fromCompanyAddress">From company address.</param>
        /// <param name="emailTemplateCodeId">The email template code identifier.</param>
        /// <param name="feedbackAndTechSupport">The feedback and tech support.</param>
        /// <param name="inventorySharingPageURL">The inventory sharing page URL.</param>
        public static void RequestToShareInventory(string toEmail, string toPersonName, string fromPersonName, string fromPersonEmail, string fromcompanyName, string fromCompanyAddress, int emailTemplateCodeId, string feedbackAndTechSupport, string inventorySharingPageURL)
        {
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["FromPersonName"] = fromPersonName;
            parameters["FromPersonEmail"] = fromPersonEmail;
            parameters["FromCompany"] = fromcompanyName;
            parameters["CompanyAddress"] = fromCompanyAddress;
            parameters["FeedBackAndTechSupportURL"] = feedbackAndTechSupport;
            if (inventorySharingPageURL != null)
                parameters["InventorySharingLink"] = inventorySharingPageURL;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "REQUESTTOSHAREINVENTORY", null);
        }

        /// <summary>
        /// Sends the item removal email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="itemBriefName">Name of the item brief.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="contactPersonName">Name of the contact person.</param>
        /// <param name="contactPersonEmail">The contact person email.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="feedbackAndTechSupport">The feedback and tech support.</param>
        public static void SendItemRemovalEmail(string toEmail, string toPersonName, string itemName, string itemBriefName, string projectName, string companyName, string contactPersonName, string contactPersonEmail, string startDate, string endDate, string feedbackAndTechSupport)
        {
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["ItemName"] = itemName;
            parameters["ItemBrief"] = itemBriefName;
            parameters["Project"] = projectName;
            parameters["Company"] = companyName;
            parameters["ContactPersonName"] = contactPersonName;
            parameters["ContactPersonEmail"] = contactPersonEmail;
            parameters["StartDate"] = startDate;
            parameters["EndDate"] = endDate;
            parameters["FeedBackAndTechSupportURL"] = feedbackAndTechSupport;

            //Load email text contents
            GetEmailContentFromTemplate(Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "ITEMREMOVAL"), parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "ITEMREMOVAL", null);
        }

        /// <summary>
        /// Sends the item removal email for non project booking.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="bookingName">Name of the booking.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="contactPersonName">Name of the contact person.</param>
        /// <param name="contactPersonEmail">The contact person email.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="feedbackAndTechSupport">The feedback and tech support.</param>
        public static void SendItemRemovalEmailForNonProjectBooking(string toEmail, string toPersonName, string itemName, string bookingName, string companyName, string contactPersonName, string contactPersonEmail, string startDate, string endDate, string feedbackAndTechSupport)
        {
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["ItemName"] = itemName;
            parameters["BookingName"] = bookingName;
            parameters["Company"] = companyName;
            parameters["ContactPersonName"] = contactPersonName;
            parameters["ContactPersonEmail"] = contactPersonEmail;
            parameters["StartDate"] = startDate;
            parameters["EndDate"] = endDate;
            parameters["FeedBackAndTechSupportURL"] = feedbackAndTechSupport;

            //Load email text contents
            GetEmailContentFromTemplate(Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "ITEMREMOVALNONPROJECTBOOKING"), parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "ITEMREMOVALNONPROJECTBOOKING", null);
        }

        /// <summary>
        /// Stops the share inventory.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="viewingCompanyName">Name of the viewing company.</param>
        /// <param name="feedbackAndTechSupport">The feedback and tech support.</param>
        /// <param name="emailTemplateCodeId">The email template code identifier.</param>
        public static void StopShareInventory(string toEmail, string toPersonName, string companyName, string viewingCompanyName, string feedbackAndTechSupport, int emailTemplateCodeId)
        {
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["CompanyName"] = companyName;
            parameters["ViewingCompanyName"] = viewingCompanyName;
            parameters["FeedBackAndTechSupportURL"] = feedbackAndTechSupport;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "STOPINVENTORYSHARING", null);
        }

        #region Project Expiration Notices

        /// <summary>
        /// Sends the project expiration notice to company admin.
        /// </summary>
        /// <param name="emailTemplateType">Type of the email template.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="companyAdminName">Name of the company admin.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="expirationDate">The expiration date.</param>
        /// <param name="companyBillingUrl">The company billing URL.</param>
        /// <param name="createNewProjectUrl">The create new project URL.</param>
        /// <param name="supportEmail">The support email.</param>
        /// <param name="companyPricingPlanUrl">The company pricing plan URL.</param>
        public static void SendProjectExpirationNoticeToCompanyAdmin(string emailTemplateType, int projectId, string toEmail, string companyAdminName, string projectName, string expirationDate, string companyBillingUrl, string createNewProjectUrl, string supportEmail, string companyPricingPlanUrl = "")
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", emailTemplateType);
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["CompanyAdminName"] = companyAdminName;
            parameters["ProjectName"] = projectName;
            parameters["ExpirationDate"] = expirationDate;
            parameters["CompanyBillingUrl"] = companyBillingUrl;
            parameters["CreateProjectUrl"] = createNewProjectUrl;
            parameters["CompanyPricingPlanUrl"] = companyPricingPlanUrl;
            parameters["SupportEmail"] = supportEmail;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, emailTemplateType, projectId);
        }

        #endregion Project Expiration Notices

        #region Company Expiration Notices

        /// <summary>
        /// Sends the company expiration notice to company admin.
        /// </summary>
        /// <param name="emailTemplateType">Type of the email template.</param>
        /// <param name="compnayId">The compnay id.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="companyAdminName">Name of the company admin.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="companyBillingUrl">The company billing URL.</param>
        /// <param name="supportEmail">The support email.</param>
        public static void SendCompanyExpirationNoticeToCompanyAdmin(string emailTemplateType, int compnayId, string toEmail,
            string companyAdminName, string companyName, string companyBillingUrl, string supportEmail)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", emailTemplateType);
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["CompanyAdminName"] = companyAdminName;
            parameters["CompanyName"] = companyName;
            parameters["CompanyBillingUrl"] = companyBillingUrl;
            parameters["SupportEmail"] = supportEmail;

            //Load email text contents
            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, emailTemplateType, compnayId);
        }

        #endregion Company Expiration Notices

        #region UserPrimaryEmailChange

        /// <summary>
        /// Sends the user primary email change email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="link">The link.</param>
        /// <param name="username">The username.</param>
        public static void SendUserPrimaryEmailChange(string toEmail, string link, string username)
        {
            int userPrimaryEmailChangeEmailTemplateCodeId =
                    Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "USERPRIMARYEMAILCHANGE");
            string feedbackEmail = Utils.GetSystemValue("FeedbackEmail");

            Hashtable parameters = new Hashtable();
            parameters["ActivationLink"] = link;
            parameters["Username"] = username;
            parameters["NewEmail"] = toEmail;
            parameters["FeedbackEmail"] = feedbackEmail;

            string subject = string.Empty;
            string messageBody = string.Empty;

            //Load email body contents
            GetEmailContentFromTemplate(userPrimaryEmailChangeEmailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        #endregion UserPrimaryEmailChange

        /// <summary>
        /// Informs the users project is closed.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="toPersonName">Name of to person.</param>
        /// <param name="personProjectClosedBy">The person project closed by.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="projectName">Name of the project.</param>
        public static void InformUsersProjectIsClosed(string toEmail, int projectId, string toPersonName, string personProjectClosedBy, string companyName, string projectName)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "PROJECTCLOSED");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ToPersonName"] = toPersonName;
            parameters["PersonProjectClosedBy"] = personProjectClosedBy;
            parameters["ProjectName"] = projectName;
            parameters["CompanyName"] = companyName;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody, "PROJECTCLOSED", projectId);
        }

        /// <summary>
        /// Sends the invoice request to sb admin.
        /// </summary>
        /// <param name="emailContent">Content of the email.</param>
        public static void SendInvoiceRequestToSBAdmin(PackageConfirmationEmailContent emailContent)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "PAYMENTDETAILSCONFIRMATION");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["UserName"] = emailContent.UserName;
            parameters["UserPosition"] = emailContent.Position != string.Empty ? string.Concat("the ", emailContent.Position) : string.Empty;
            parameters["CompanyName"] = emailContent.CompanyName;
            parameters["Date"] = Utils.FormatDate(Utils.Today);
            parameters["AuthorizationText"] = emailContent.FormattedAuthText;
            parameters["PlanStatus"] = emailContent.IsInventryProjectOrDurationChanged ? "new" : "current";  //If there is a package change, "new" will be added infront of text "pricing plan"
            parameters["ProjectPackage"] = emailContent.ProjectPackage;
            parameters["projectPeriodPrice"] = emailContent.ProjectPeriodPrice;
            parameters["InventoryPackage"] = emailContent.InventoryPackage;
            parameters["InventoryPeriodPrice"] = emailContent.InventoryPeriodPrice;
            parameters["TotalPrice"] = emailContent.TotalPriceString;
            parameters["ConfirmedDate"] = Utils.FormatDate(Utils.Today);
            parameters["PromotionalString"] = emailContent.Discount != string.Empty ? string.Format("<p>This includes a promotional code at {0} until {1}.</p>", emailContent.Discount, emailContent.PromotionalCodeExpireDate) : string.Empty;
            parameters["PositionString"] = emailContent.EducationalPosition != string.Empty ? string.Format("<p>Their position at school/university/college is {0}.</p>", emailContent.EducationalPosition) : string.Empty;
            parameters["EducationalString"] = emailContent.IsEducational ? "<p>This includes the Educational Discount.</P>" : string.Empty;
            parameters["CompanyAddress"] = emailContent.CompanyURL;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(emailContent.ToEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the inventory limit upgrade request close project.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="primaryAdminName">Name of the primary admin.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="pricePlanUrl">The price plan URL.</param>
        /// <param name="noOfItemsinInventory">The no of itemsin inventory.</param>
        /// <param name="noOfNewItems">The no of new items.</param>
        /// <param name="totalItems">The total items.</param>
        /// <param name="senderEmail">The sender email.</param>
        /// <param name="senderName">Name of the sender.</param>
        public static void SendInventoryLimitUpgradeRequestCloseProject(string toEmail, string primaryAdminName, string projectName, string pricePlanUrl,
            string noOfItemsinInventory, string noOfNewItems, string totalItems, string senderEmail, string senderName)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "PROJECTCLOSEINVENTORYLIMITUPGRADEREQUEST");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["CompanyAdminName"] = primaryAdminName;
            parameters["ProjectName"] = projectName;
            parameters["NoOfItemsinInventory"] = noOfItemsinInventory;
            parameters["NoOfNewItems"] = noOfNewItems;
            parameters["TotalItems"] = totalItems;
            parameters["PricingPlanUrl"] = pricePlanUrl;
            parameters["SenderEmail"] = senderEmail;
            parameters["SenderName"] = senderName;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the inventory limit upgrade request.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="primaryAdminName">Name of the primary admin.</param>
        /// <param name="inventoryUserName">Name of the inventory user.</param>
        /// <param name="pricingPlanUrl">The pricing plan URL.</param>
        /// <param name="feedbackEmail">The feedback email.</param>
        /// <param name="companyName">Name of the company.</param>
        public static void SendInventoryLimitUpgradeRequest(string toEmail, string primaryAdminName, string inventoryUserName, string pricingPlanUrl, string feedbackEmail, string companyName)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "INVENTORYLIMITUPGRADEREQUEST");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["CompanyName"] = companyName;
            parameters["PrimaryAdminName"] = primaryAdminName;
            parameters["InventoryUserName"] = inventoryUserName;
            parameters["PricingPlanUrl"] = pricingPlanUrl;
            parameters["FeedbackEmail"] = feedbackEmail;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the user limit upgrade request.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="primaryAdminName">Name of the primary admin.</param>
        /// <param name="projectAdminName">Name of the project admin.</param>
        /// <param name="pricingPlanUrl">The pricing plan URL.</param>
        /// <param name="feedbackEmail">The feedback email.</param>
        /// <param name="projectName">Name of the project.</param>
        public static void SendUserLimitUpgradeRequest(string toEmail, string primaryAdminName, string projectAdminName, string pricingPlanUrl, string feedbackEmail, string projectName)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "USERLIMITUPGRADEREQUEST");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["ProjectName"] = projectName;
            parameters["PrimaryAdminName"] = primaryAdminName;
            parameters["ProjectAdminName"] = projectAdminName;
            parameters["PricingPlanUrl"] = pricingPlanUrl;
            parameters["FeedbackEmail"] = feedbackEmail;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the notification email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="emailContent">Content of the email.</param>
        public static void SendNotificationEmail(string toEmail, string userName, string emailContent)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "EMAILNOTIFICATION");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["UserName"] = userName;
            parameters["FeedBackAndTechSupportURL"] = Utils.GetSystemValue("FeedBackAndTechSupportURL");
            parameters["Content"] = emailContent;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the booking notification email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="bookingNumber">The booking number.</param>
        /// <param name="url">The URL.</param>
        public static void SendBookingNotificationEmail(string toEmail, string userName, string bookingNumber, string url)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGNOTIFICATION");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["Url"] = url;
            parameters["FirstName"] = userName;
            parameters["BookingNumber"] = bookingNumber;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the booking overdue email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="htmlLinks">The HTML links.</param>
        public static void SendBookingOverdueEmail(string toEmail, string userName, string htmlLinks)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGOVERDUE");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = userName;
            parameters["HtmlLinks"] = htmlLinks;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the booking delayed email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="itemBriefName">Name of the item brief.</param>
        /// <param name="itemBriefUrl">The item brief URL.</param>
        /// <param name="inventoryAdminName">Name of the inventory admin.</param>
        /// <param name="inventoryAdminEmail">The inventory admin email.</param>
        public static void SendBookingDelayedEmail(string toEmail, string userName, string itemBriefName, string itemBriefUrl, string inventoryAdminName, string inventoryAdminEmail)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGDELAYED");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = userName;
            parameters["ItemBriefUrl"] = itemBriefUrl;
            parameters["ItemBriefName"] = itemBriefName;
            parameters["InventoryAdminEmail"] = inventoryAdminEmail;
            parameters["InventoryAdminName"] = inventoryAdminName;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the booking delayed email for non project.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="bookingName">Name of the booking.</param>
        /// <param name="inventoryAdminName">Name of the inventory admin.</param>
        /// <param name="inventoryAdminEmail">The inventory admin email.</param>
        public static void SendBookingDelayedEmailForNonProject(string toEmail, string userName, string bookingName, string inventoryAdminName, string inventoryAdminEmail)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGDELAYEDNONPROJECT");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["FirstName"] = userName;
            parameters["BookingName"] = bookingName;
            parameters["InventoryAdminName"] = inventoryAdminName;
            parameters["InventoryAdminEmail"] = inventoryAdminEmail;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        /// <summary>
        /// Sends the contact inventory manager for item changes dueto booking overlap email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="inventoryAdminName">Name of the inventory admin.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="itemUrl">The item URL.</param>
        /// <param name="bookingNumber">The booking number.</param>
        /// <param name="bookingUrl">The booking URL.</param>
        /// <param name="htmlContent">Content of the HTML.</param>
        public static void SendContactInventoryManagerForItemChangesDuetoBookingOverlapEmail(string toEmail, string inventoryAdminName, string userName, string itemName,
                string itemUrl, string bookingNumber, string bookingUrl, string htmlContent)
        {
            int emailTemplateCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "CONTACT_INVENTORYMANAGER_FOR_ITEMCHANGES");
            string subject = string.Empty;
            string messageBody = string.Empty;

            Hashtable parameters = new Hashtable();
            parameters["InventoryAdminName"] = inventoryAdminName;
            parameters["UserName"] = userName;
            parameters["ItemName"] = itemName;
            parameters["BookingNumber"] = bookingNumber;
            parameters["HtmlContent"] = htmlContent;
            parameters["ItemUrl"] = itemUrl;
            parameters["BookingUrl"] = bookingUrl;

            GetEmailContentFromTemplate(emailTemplateCodeId, parameters, out subject, out messageBody);
            QueueEmail(toEmail, subject, messageBody);
        }

        #endregion EMAIL TYPES

        /// <summary>
        /// Queues the email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="emailType">Type of the email.</param>
        /// <param name="relatedId">The related identifier.</param>
        public static void QueueEmail(string toEmail, string subject, string body, string emailType, int? relatedId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                Email email = new Email();
                email.ToAddress = toEmail;
                email.Subject = subject;
                email.Body = body;
                email.EmailType = emailType;
                email.RelatedId = relatedId;
                email.CreatedDate = Utils.Now;

                dataContext.Emails.AddObject(email);
                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Queues the email.
        /// </summary>
        /// <param name="toEmail">To email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public static void QueueEmail(string toEmail, string subject, string body)
        {
            QueueEmail(toEmail, subject, body, null, null);
        }

        /// <summary>
        /// Sends the queued emails.
        /// </summary>
        public static void SendQueuedEmails()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            using (SmtpClient client = new SmtpClient(Utils.GetSystemValue("SMTPServer")))
            {
                //SMTP Settings
                client.Port = Convert.ToInt32(Utils.GetSystemValue("SmtpPort"));
                client.Credentials = new System.Net.NetworkCredential(Utils.GetSystemValue("EmailUsername"), Utils.GetSystemValue("EmailPassword"));
                client.EnableSsl = Convert.ToBoolean(Utils.GetSystemValue("SMTPEnableSSL"));
                if (Utils.GetSystemValue("UseDefaultCredentials") != string.Empty)
                    client.UseDefaultCredentials = Convert.ToBoolean(Utils.GetSystemValue("UseDefaultCredentials"));

                StringBuilder sbErrors = new StringBuilder();

                foreach (Email email in dataContext.Emails.Where(em => em.SentDate == null))
                {
                    if (ShouldRetryAndSendEmail(email))
                    {
                        try
                        {
                            email.LastRetryDate = Utils.Now;
                            SendEmail(client, email.ToAddress, email.Subject, email.Body);
                            //Update email info after it is sent.
                            email.SentDate = Utils.Now;
                        }
                        catch (Exception ex)
                        {
                            email.FailureCount++;

                            sbErrors.AppendLine(string.Format("To:{0} Error:{1}", email.ToAddress, ex.Message));
                        }
                    }
                }

                dataContext.SaveChanges();

                //If there were errors, record them in error log.
                if (sbErrors.Length > 0)
                {
                    AgentErrorLog.WriteToErrorLog(sbErrors.ToString());
                }
            }
        }

        /// <summary>
        /// Checks whether the email should be sent or retried for sending, based on last retry time and failure count/
        /// </summary>
        private static bool ShouldRetryAndSendEmail(Email email)
        {
            bool shouldSendEmail = false;

            if (email.FailureCount == 0)
            {
                //If failure count is 0, then this is the first time we are sending this email.
                shouldSendEmail = true;
            }
            else
            {
                //Decide whether this email should be retried again.

                if (email.LastRetryDate == null)
                {
                    //Safety condition to handle LastRetryDate was not recorded for some reason.
                    shouldSendEmail = true;
                }
                else
                {
                    //Get the difference between email created date and the last retry date
                    TimeSpan retrySpan = email.LastRetryDate.Value - email.CreatedDate;

                    //Retry to send the email 3 times in the first 3 minutes.
                    //If it still fails, retry hourly for 3 hours.
                    //If it still fails, retry daily for 3 days.

                    //If retrySpan is less than 2 minutes, retry again now
                    if (retrySpan.TotalMinutes <= 1 || email.FailureCount < 3)
                    {
                        shouldSendEmail = true;
                    }
                    else
                    {
                        if (retrySpan.TotalHours <= 3 || email.FailureCount < 6)
                        {
                            //If retrySpan is between 2 minutes and 3 hours try it hourly
                            shouldSendEmail = ((Utils.Now - email.LastRetryDate.Value).TotalHours >= 1);
                        }
                        else
                        {
                            if (retrySpan.TotalDays <= 3 || email.FailureCount < 9)
                            {
                                shouldSendEmail = ((Utils.Now - email.LastRetryDate.Value).TotalDays >= 1);
                            }
                        }
                    }
                }
            }

            return shouldSendEmail;
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        private static void SendEmail(SmtpClient client, string toEmail, string subject, string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.To.Add(toEmail);
                mail.From = new MailAddress(string.Format("StageBitz<{0}>", Utils.GetSystemValue("FromEmail")));

                //Loaded from template
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                string htmlBody;
                htmlBody = body;

                if (StageBitzImagePath != null)
                {
                    using (Bitmap b = new Bitmap(StageBitzImagePath))
                    {
                        ImageConverter ic = new ImageConverter();
                        Byte[] ba = (Byte[])ic.ConvertTo(b, typeof(Byte[]));
                        using (MemoryStream logo = new MemoryStream(ba))
                        using (AlternateView foot = AlternateView.CreateAlternateViewFromString(body + "<p><a href=\"" + StageBitzUrl + "\"><img alt=\"" + StageBitzUrl + "\" src=\"cid:companyLogo\"/></a></p>", null, "text/html"))
                        {
                            LinkedResource footerImg = new LinkedResource(logo, "image/jpeg");
                            footerImg.ContentId = "companyLogo";

                            foot.LinkedResources.Add(footerImg);
                            mail.AlternateViews.Add(foot);

                            client.Send(mail);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the email subject and body text by comibining the specified Email Template and the parameter list.
        /// </summary>
        private static void GetEmailContentFromTemplate(int emailTemplateTypeCodeId, Hashtable parameters, out string subject, out string messageBody)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                //Read email template from the database
                var email = (from em in dataContext.EmailTemplates
                             where em.EmailTemplateTypeCodeId == emailTemplateTypeCodeId && em.IsActive == true
                             select em).FirstOrDefault();

                StringBuilder sbSubject = new StringBuilder(email.Subject);
                ReplaceStringWithParameterValues(sbSubject, parameters);
                subject = sbSubject.ToString();

                StringBuilder sbMessageBody = new StringBuilder(email.MessageBody);
                ReplaceStringWithParameterValues(sbMessageBody, parameters);
                messageBody = sbMessageBody.ToString();
            }
        }

        /// <summary>
        /// Replaces the string occurences matching the paramter keys, with their corresponding values
        /// </summary>
        private static void ReplaceStringWithParameterValues(StringBuilder sb, Hashtable parameters)
        {
            if (sb == null || parameters == null)
                return;

            foreach (string key in parameters.Keys)
            {
                string placeholder = "@" + key;
                sb.Replace(placeholder, parameters[key].ToString());
            }
        }
    }
}