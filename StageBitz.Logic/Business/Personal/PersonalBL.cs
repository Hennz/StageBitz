using StageBitz.Common;
using StageBitz.Common.Google;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Business.Personal
{
    /// <summary>
    ///  Business layer for user related operations
    /// </summary>
    public class PersonalBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public PersonalBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public User GetUser(int userId)
        {
            return DataContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the name of the user by log in.
        /// </summary>
        /// <param name="logInName">Name of the log in.</param>
        /// <returns></returns>
        public User GetUserByLogInName(string logInName)
        {
            return DataContext.Users.Where(u => u.LoginName.Equals(logInName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the user by login name and password hash.
        /// </summary>
        /// <param name="logInName">Name of the login.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns>The User.</returns>
        public User GetNormalUserByLogInNameAndPasswordHash(string logInName, string passwordHash)
        {
            return (from u in DataContext.Users
                    join code in DataContext.Codes on u.UserAccountTypeCodeID equals code.CodeId
                    where u.LoginName == logInName && u.Password == passwordHash && code.Value == "USER"
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Gets the user by log in name and password hash.
        /// </summary>
        /// <param name="logInName">Name of the log in.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns></returns>
        public User GetUserByLogInNameAndPasswordHash(string logInName, string passwordHash)
        {
            return (from u in DataContext.Users
                    join code in DataContext.Codes on u.UserAccountTypeCodeID equals code.CodeId
                    where u.LoginName == logInName && u.Password == passwordHash && u.IsActive == true
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Gets the user by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The User.</returns>
        public User GetUserByEmail(string email)
        {
            return (from u in DataContext.Users
                    where u.LoginName.Equals(email, StringComparison.InvariantCultureIgnoreCase)
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Gets the user by user identifier and password hash.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns></returns>
        public User GetUserByUserIdAndPasswordHash(int userId, string passwordHash)
        {
            return DataContext.Users.Where(u => u.UserId == userId && u.Password == passwordHash).FirstOrDefault();
        }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public string GetUserFullName(int userId)
        {
            User user = GetUser(userId);
            return (user != null) ? (user.FirstName + " " + user.LastName).Trim() : string.Empty;
        }

        /// <summary>
        /// Returns the user with the specified username and password hash.
        /// </summary>
        public StageBitz.Data.User AuthenticateUser(string loginName, string passwordHash)
        {
            var user = (from u in DataContext.Users
                        where u.LoginName == loginName && u.Password == passwordHash
                        select u).FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Gets the pending invitation for user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="invitationTypeCodeId">The invitation type code identifier.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public Invitation GetPendingInvitationForUser(User user, int invitationTypeCodeId, int relatedId)
        {
            //int currentInvitationTypeCodeId = GetInvitationTypeCodeId(DisplayMode);
            int pendingInvitationCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            //int relatedId = DisplayMode == ViewMode.CompanyAdministrators ? CompanyId : ProjectId;

            return (from inv in DataContext.Invitations
                    where (inv.ToUserId == user.UserId || inv.ToEmail.Equals(user.LoginName, StringComparison.InvariantCultureIgnoreCase)) &&
                       inv.InvitationTypeCodeId == invitationTypeCodeId && inv.InvitationStatusCodeId == pendingInvitationCodeId &&
                       inv.RelatedId == relatedId
                    select inv).FirstOrDefault();
        }

        /// <summary>
        /// Gets the pending invitation for email.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="invitationTypeCodeId">The invitation type code identifier.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public Invitation GetPendingInvitationForEmail(string emailAddress, int invitationTypeCodeId, int relatedId)
        {
            //int currentInvitationTypeCodeId = GetInvitationTypeCodeId(DisplayMode);
            int pendingInvitationCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            //int relatedId = DisplayMode == ViewMode.CompanyAdministrators ? CompanyId : ProjectId;

            return (from inv in DataContext.Invitations
                    where (inv.ToEmail.Equals(emailAddress, StringComparison.InvariantCultureIgnoreCase) &&
                       inv.InvitationTypeCodeId == invitationTypeCodeId && inv.InvitationStatusCodeId == pendingInvitationCodeId) &&
                       inv.RelatedId == relatedId
                    select inv).FirstOrDefault();
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns></returns>
        public List<UserListForAdmin> GetAllUsers()
        {
            int adminCodeID = Utils.GetCodeByValue("UserAccountType", "ADMIN").CodeId;

            return (from u in DataContext.Users
                    join c in DataContext.Countries on u.CountryId equals c.CountryId
                    orderby u.FirstName, u.LastName
                    select new UserListForAdmin
                    {
                        UserId = u.UserId,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Status = u.IsActive == true ? "Active" : "Pending Activation",
                        Country = c.CountryName,
                        City = u.City,
                        State = u.State,
                        Email = u.Email1,
                        IsStageBitzAdmin = (u.UserAccountTypeCodeID == adminCodeID ? true : false),
                        LastLogIn = u.LastLoggedInDate.Value,
                        RegisteredDate = u.CreatedDate.Value
                    }).ToList<UserListForAdmin>();
        }

        /// <summary>
        /// Accepts the invitation.
        /// </summary>
        /// <param name="invitationId">The invitation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void AcceptInvitation(int invitationId, int userId)
        {
            Invitation invitation = DataContext.Invitations.Where(inv => inv.InvitationId == invitationId).FirstOrDefault();
            var invitationUserRoles = DataContext.InvitationUserRoles.Where(iur => iur.InvitationId == invitationId && iur.IsActive);

            Code invitationTypeCode = Utils.GetCodeByCodeId(invitation.InvitationTypeCodeId);
            User user = GetUser(userId);

            #region Create project/company user

            if (invitationTypeCode.Value == "PROJECTTEAM")
            {
                int projectId = invitation.RelatedId;
                InvitationUserRole projectInvitationUserRole = invitationUserRoles.FirstOrDefault();

                if (projectInvitationUserRole != null)
                {
                    //Create a new project user and save
                    ProjectUser projUser = new ProjectUser();
                    projUser.ProjectId = projectId;
                    projUser.UserId = invitation.ToUserId.Value;
                    projUser.ProjectUserTypeCodeId = projectInvitationUserRole.UserTypeCodeId;
                    projUser.Role = invitation.ProjectRole;
                    projUser.IsActive = true;
                    projUser.CreatedByUserId = projUser.LastUpdatedByUserId = userId;
                    projUser.CreatedDate = projUser.LastUpdatedDate = Utils.Now;
                    projUser.CanSeeBudgetSummary = invitation.CanSeeProjectBudgetSummary;
                    DataContext.ProjectUsers.AddObject(projUser);

                    #region Notifications

                    Data.Notification nf = new Data.Notification();
                    nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "PROJTEAM");
                    nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "ADD");
                    nf.RelatedId = invitation.RelatedId;
                    nf.ProjectId = invitation.RelatedId;
                    nf.Message = string.Format("{0} accepted the Project invitation.", (user.FirstName + " " + user.LastName).Trim());
                    nf.CreatedByUserId = nf.LastUpdatedByUserId = userId;
                    nf.CreatedDate = nf.LastUpdatedDate = Utils.Now;
                    DataContext.Notifications.AddObject(nf);

                    #endregion Notifications

                    #region Project Notification Setting

                    //Get the current latest notification Id for the project.
                    int maxNotificationId = DataContext.Notifications.Where(pnf => pnf.ProjectId == projectId).Max(pnf => pnf.NotificationId);

                    //Create a project notification setting for this user (if it doesn't exist already)
                    UserNotificationSetting unf = DataContext.UserNotificationSettings.Where(s => s.RelatedTable == "Project" && s.RelatedId == projectId && s.UserID == userId).FirstOrDefault();
                    if (unf == null)
                    {
                        unf = new UserNotificationSetting();
                        unf.RelatedTable = "Project";
                        unf.RelatedId = projectId;
                        unf.UserID = userId;
                        unf.LastNotificationId = maxNotificationId;
                        unf.CreatedBy = unf.LastUpdatedBy = userId;
                        unf.CreatedDate = unf.LastUpdatedDate = Utils.Now;
                        DataContext.UserNotificationSettings.AddObject(unf);
                    }

                    #endregion Project Notification Setting

                    //Update Project Daily Usage Summary
                    //ProjectUsageHandler.UpdateProjectUsage(DataContext.Projects.Where(p => p.ProjectId == projectId).FirstOrDefault(), userId, projUser.UserId, false, Utils.Today, DataContext);
                }
            }
            else if (invitationTypeCode.Value == "COMPANYADMIN" || invitationTypeCode.Value == "INVENTORYTEAM")
            {
                CompanyBL companyBL = new CompanyBL(DataContext);
                CompanyUser companyUser = companyBL.GetCompanyUserByUserIdAndCompanyId(userId, invitation.RelatedId);

                bool addNewCompanyUser = false;
                if (companyUser == null)
                {
                    companyUser = new CompanyUser();
                    companyUser.CompanyId = invitation.RelatedId;
                    companyUser.UserId = invitation.ToUserId.Value;
                    companyUser.CreatedDate = companyUser.LastUpdatedDate = Utils.Now;
                    addNewCompanyUser = true;
                }

                //companyUser.CompanyUserTypeCodeId = invitation.UserTypeCodeId;
                companyUser.IsActive = true;
                companyUser.CreatedByUserId = companyUser.LastUpdatedByUserId = userId;

                foreach (InvitationUserRole invitationUserRole in invitationUserRoles)
                {
                    companyUser.CompanyUserRoles.Add(
                        new CompanyUserRole
                        {
                            IsActive = true,
                            CompanyUserTypeCodeId = invitationUserRole.UserTypeCodeId,
                            CreatedByUserId = userId,
                            CreatedDate = Utils.Now,
                            LastUpdatedByUserId = userId,
                            LastUpdatedDate = Utils.Now,
                            LocationId = invitationUserRole.LocationId
                        }
                    );
                }

                if (addNewCompanyUser)
                {
                    DataContext.CompanyUsers.AddObject(companyUser);
                }
            }

            #endregion Create project/company user

            //Mark the invitation as Accepted
            invitation.InvitationStatusCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "ACCEPTED");
            invitation.LastUpdatedByUserId = userId;
            invitation.LastUpdatedDate = Utils.Now;

            DataContext.SaveChanges();

            #region Check for pending invitations to the same project/company and discard them

            int pendingInvitationCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            var simillarInvitations = DataContext.Invitations.Where(inv => inv.ToUserId == userId && inv.InvitationStatusCodeId == pendingInvitationCodeId && inv.RelatedId == invitation.RelatedId && inv.InvitationTypeCodeId == invitation.InvitationTypeCodeId);

            foreach (Invitation inv in simillarInvitations)
            {
                DataContext.DeleteInvitation(inv.InvitationId);
            }

            #endregion Check for pending invitations to the same project/company and discard them
        }

        /// <summary>
        /// Gets the registered user for the invitation.
        /// </summary>
        /// <param name="invitationCode">The invitation code.</param>
        /// <returns></returns>
        public User GetRegisteredUserFortheInvitation(string invitationCode)
        {
            int invitationId = 0;
            int.TryParse(Utils.DecryptStringAES(System.Web.HttpServerUtility.UrlTokenDecode(invitationCode)), out invitationId);

            StageBitz.Data.Invitation inv = DataContext.Invitations.Where(i => i.InvitationId == invitationId).FirstOrDefault();
            //User has already registerted
            if (inv != null)
            {
                User user = DataContext.Users.Where(u => u.LoginName == inv.ToEmail).FirstOrDefault();
                return user;
            }

            return null;
        }

        /// <summary>
        /// Gets the user activities from google analytics.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        public List<Data.DataTypes.Analytics.UserActivity> GetUserActivitiesFromGoogleAnalytics(int userId, DateTime startDate,
                DateTime endDate, int pageSize, int pageIndex, string sortField, bool isAscending, out int totalRecords)
        {
            AnalyticsManager analyticsManager = new AnalyticsManager();
            List<Data.DataTypes.Analytics.UserActivity> userActivities = analyticsManager.GetUserActivities(userId, startDate, endDate, pageSize, pageIndex, sortField, isAscending, out totalRecords);

            var userActivityList = from ua in userActivities
                                   join c in DataContext.Companies on ua.CompanyId equals c.CompanyId
                                   from cu in DataContext.CompanyUsers.Where(cu => cu.UserId == userId && cu.CompanyId == ua.CompanyId).Take(1).DefaultIfEmpty()
                                   join p in DataContext.Projects on ua.ProjectId equals p.ProjectId
                                   from pu in DataContext.ProjectUsers.Where(pu => pu.UserId == userId && pu.ProjectId == p.ProjectId).Take(1).DefaultIfEmpty()
                                   select new Data.DataTypes.Analytics.UserActivity
                                   {
                                       Date = ua.Date,
                                       ProjectName = p.ProjectName,
                                       Role = pu != null ? pu.Role.Replace("N/A", string.Empty) : string.Empty,
                                       Permission = pu != null ? pu.Code.Description : cu != null ? "Company Administrator" : string.Empty,
                                       CompanyName = c.CompanyName,
                                       CompanyId = c.CompanyId,
                                       SessionTotal = ua.SessionTotal
                                   };

            return userActivityList.ToList();
        }
    }
}