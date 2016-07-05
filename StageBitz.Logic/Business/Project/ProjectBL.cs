using StageBitz.Common;
using StageBitz.Common.Google;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Data.DataTypes.Analytics;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Finance;
using StageBitz.Logic.Finance.Project;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;

namespace StageBitz.Logic.Business.Project
{
    /// <summary>
    ///  Business layer for projects
    /// </summary>
    public class ProjectBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public ProjectBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public Data.Project GetProject(int projectId)
        {
            return DataContext.Projects.Where(p => p.ProjectId == projectId && p.IsActive == true).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether given project is closed.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public bool IsProjectClosed(int projectId)
        {
            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            Data.Project project = GetProject(projectId);
            return (project != null && project.ProjectStatusCodeId == closedProjectStatusCodeId);
        }

        /// <summary>
        /// Gets the project closed by.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public User GetProjectClosedBy(int projectId)
        {
            return (from pa in DataContext.ProjectArchives
                    join u in DataContext.Users on pa.ProjectClosedBy equals u.UserId
                    where pa.ProjectId == projectId
                    select u).FirstOrDefault();
        }

        /// <summary>
        /// Gets the project users.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public List<User> GetProjectUsers(int projectId)
        {
            return (from u in DataContext.Users
                    join pu in DataContext.ProjectUsers on u.UserId equals pu.UserId
                    where pu.ProjectId == projectId && pu.IsActive == true
                    select u).ToList<User>();
        }

        /// <summary>
        /// Gets the project user.
        /// </summary>
        /// <param name="projectUserId">The project user identifier.</param>
        /// <returns></returns>
        public Data.ProjectUser GetProjectUser(int projectUserId)
        {
            return DataContext.ProjectUsers.Where(pu => pu.ProjectUserId == projectUserId).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether given user is a project user.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool IsProjectUser(int projectId, int userId)
        {
            Data.ProjectUser projectUser = DataContext.ProjectUsers.Where(pu => pu.ProjectId == projectId && pu.UserId == userId).FirstOrDefault();
            if (projectUser != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether given user is project administrator.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool IsProjectAdministrator(int projectID, int userId)
        {
            int projAdminCodeID = Utils.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN").CodeId;
            var projectUser = (from p in DataContext.Projects
                               join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                               where p.ProjectId == projectID && pu.UserId == userId && pu.IsActive == true && pu.ProjectUserTypeCodeId == projAdminCodeID
                               select pu).FirstOrDefault();

            return (projectUser != null);
        }

        /// <summary>
        /// Gets the company administrators.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<User> GetCompanyAdministrators(int companyId)
        {
            int companyAdminTypeCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;
            int companySecAdminTypeCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "SECADMIN").CodeId;

            return (from u in DataContext.Users
                    join cu in DataContext.CompanyUsers on u.UserId equals cu.UserId
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where cu.CompanyId == companyId && cu.IsActive == true && (cur.CompanyUserTypeCodeId == companyAdminTypeCodeId || cur.CompanyUserTypeCodeId == companySecAdminTypeCodeId)
                    select u).ToList<User>();
        }

        /// <summary>
        /// Gets the project team member list.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<ProjectUserData> GetProjectTeamMemberList(int projectId, int userId)
        {
            int acceptedInvitationCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "ACCEPTED");
            int projectInvitationCodeId = Utils.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");
            int projAdminTypeCodeId = Utils.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");

            int companyId = (from p in DataContext.Projects where p.ProjectId == projectId select p.CompanyId).SingleOrDefault();

            //Get invitations for existing project users
            List<ProjectUserData> projectUserData = (from pu in DataContext.ProjectUsers
                                                     from uc in DataContext.UserContacts.Where(contact => contact.ContactUserId == pu.UserId && contact.UserId == userId).Take(1).DefaultIfEmpty()
                                                     from compuser in DataContext.CompanyUsers.Where(cu => cu.UserId == pu.UserId && cu.CompanyId == companyId).Take(1).DefaultIfEmpty()
                                                     join u in DataContext.Users on pu.UserId equals u.UserId
                                                     join puType in DataContext.Codes on pu.ProjectUserTypeCodeId equals puType.CodeId
                                                     join invStatus in DataContext.Codes on acceptedInvitationCodeId equals invStatus.CodeId
                                                     where pu.ProjectId == projectId
                                                     select new ProjectUserData { UserId = pu.UserId, FullName = (u.FirstName + " " + u.LastName).Trim(), IsContact = (uc != null), ProjectUserId = pu.ProjectUserId, InvitationId = 0, Role = pu.Role, UserTypeCode = puType, StatusCode = invStatus, IsActive = pu.IsActive, IsMember = true, IsCompanyAdmin = (compuser != null) }).ToList<ProjectUserData>();

            //Get invitations for newly invited users
            List<ProjectUserData> invitationData = (from inv in DataContext.Invitations
                                                    join iur in DataContext.InvitationUserRoles on inv.InvitationId equals iur.InvitationId
                                                    from u in DataContext.Users.Where(user => user.UserId == inv.ToUserId).Take(1).DefaultIfEmpty()
                                                    from compuser in DataContext.CompanyUsers.Where(cu => cu.UserId == inv.ToUserId && cu.CompanyId == companyId).Take(1).DefaultIfEmpty()
                                                    join puType in DataContext.Codes on iur.UserTypeCodeId equals puType.CodeId
                                                    join invStatus in DataContext.Codes on inv.InvitationStatusCodeId equals invStatus.CodeId
                                                    where inv.InvitationTypeCodeId == projectInvitationCodeId && inv.RelatedId == projectId && inv.InvitationStatusCodeId != acceptedInvitationCodeId
                                                    select new ProjectUserData { UserId = (inv.ToUserId == null ? 0 : inv.ToUserId.Value), FullName = ((u == null ? inv.ToName : (u.FirstName + " " + u.LastName).Trim()) + " (" + inv.ToEmail + ")"), IsContact = false, ProjectUserId = 0, InvitationId = inv.InvitationId, Role = inv.ProjectRole, UserTypeCode = puType, StatusCode = invStatus, IsActive = false, IsMember = false, IsCompanyAdmin = (compuser != null) }).ToList<ProjectUserData>();

            return projectUserData.Union(invitationData).OrderByDescending(u => u.IsMember).ThenBy(u => u.StatusCode.SortOrder).ThenBy(u => u.UserTypeCode.SortOrder).ThenBy(pu => pu.FullName).ToList<ProjectUserData>();

            //return allUsers;
        }

        /// <summary>
        /// Chanages the project administrator.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="newAdminUserId">The new admin user identifier.</param>
        public void ChanageProjectAdministrator(int projectId, int userId, int newAdminUserId)
        {
            int projStaffTypeCodeId = Utils.GetCodeIdByCodeValue("ProjectUserTypeCode", "STAFF");

            Code projectAdmin = Utils.GetCodeByValue("ProjectUserTypeCode", "PROJADMIN");

            int projAdminTypeCodeId = projectAdmin.CodeId;

            //Make the current admin a staff member
            ProjectUser projectAdminUser = DataContext.ProjectUsers.Where(pu => pu.ProjectId == projectId && pu.ProjectUserTypeCodeId == projAdminTypeCodeId).FirstOrDefault();
            projectAdminUser.ProjectUserTypeCodeId = projStaffTypeCodeId;
            projectAdminUser.LastUpdatedByUserId = userId;
            projectAdminUser.LastUpdatedDate = Utils.Now;

            //Make the specified user the project admin
            ProjectUser projUser = DataContext.ProjectUsers.Where(pu => pu.ProjectUserId == newAdminUserId).FirstOrDefault();
            projUser.ProjectUserTypeCodeId = projAdminTypeCodeId;
            projUser.LastUpdatedByUserId = userId;
            projUser.LastUpdatedDate = Utils.Now;

            //Create Notification for changing permission of the project

            #region Project Notification

            string projectUserName = projUser.User.FirstName + " " + projUser.User.LastName;
            //DataContext.Notifications.AddObject(CreateNotification(Support.GetCodeIdByCodeValue("OperationType", "EDIT"), string.Format("{0} changed the permission of {1} as {2}.", Support.UserFullName, projectUserName, projectAdmin.Description)));

            #endregion Project Notification

            DataContext.SaveChanges();
        }

        /// <summary>
        /// Gets the projects by name.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="isActive">The is active.</param>
        /// <returns></returns>
        public List<Data.Project> GetProjectsByName(string projectName, int companyId, bool? isActive)
        {
            return (from p in DataContext.Projects
                    where p.ProjectName.Equals(projectName, StringComparison.InvariantCultureIgnoreCase) &&
                        p.CompanyId == companyId && (!isActive.HasValue || isActive.HasValue && p.IsActive == isActive.Value)
                    select p).ToList<Data.Project>();
        }

        /// <summary>
        /// Gets the project by last updated date time.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="lastUpdatedDateTime">The last updated date time.</param>
        /// <returns></returns>
        public Data.Project GetProjectByLastUpdatedDateTime(int projectId, DateTime lastUpdatedDateTime)
        {
            StageBitz.Data.Project project = (from p in DataContext.Projects
                                              where p.ProjectId == projectId && p.LastUpdatedDate == lastUpdatedDateTime
                                              select p).FirstOrDefault();
            return project;
        }

        /// <summary>
        /// Gets the pending invitations count.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public int GetPendingInvitationsCount(int userId)
        {
            int pendingInvitationStatusCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            return DataContext.Invitations.Where(i => i.ToUserId == userId && i.InvitationStatusCodeId == pendingInvitationStatusCodeId).Count();
        }

        /// <summary>
        /// Determines whether user has responded to welcom message.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool HasRespondedToWelcomMessage(int userId)
        {
            Data.User user = DataContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
            return user.HasRespondedToWelcomeMessage;
        }

        /// <summary>
        /// Gets the project archive.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public ProjectArchive GetProjectArchive(int projectId)
        {
            return DataContext.ProjectArchives.Where(pa => pa.ProjectId == projectId).FirstOrDefault();
        }

        /// <summary>
        /// Creates the first company and first project.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="countryId">The country identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void CreateFirstCompanyAndFirstProject(String companyName, string projectName, int countryId, int userId)
        {
            #region Company

            StageBitz.Data.Company company = new Data.Company();
            company.CompanyName = companyName;
            company.IsActive = true;
            company.CountryId = countryId;
            company.CreatedDate = Utils.Now;
            company.LastUpdatedDate = Utils.Now;
            company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
            company.ExpirationDate = Utils.Today.AddDays(int.Parse(Utils.GetSystemValue("FreeTrialDays"))); //Expires from 3 weeks
            //company.IsCompanyVisibleForSearchInInventory = true;
            DataContext.Companies.AddObject(company);

            #endregion Company

            #region Company User

            CompanyUser companyUser = new CompanyUser();
            companyUser.CreatedDate = Utils.Now;
            companyUser.LastUpdatedDate = Utils.Now;
            //companyUser.CompanyUserTypeCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            companyUser.UserId = userId;
            companyUser.IsActive = true;

            companyUser.CompanyUserRoles.Add(
                new CompanyUserRole
                {
                    IsActive = true,
                    CompanyUserTypeCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN"),
                    CreatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedByUserId = userId,
                    LastUpdatedDate = Utils.Now
                }
            );

            companyUser.CompanyUserRoles.Add(
                new CompanyUserRole
                {
                    IsActive = true,
                    CompanyUserTypeCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN"),
                    CreatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedByUserId = userId,
                    LastUpdatedDate = Utils.Now
                }
            );

            DataContext.CompanyUsers.AddObject(companyUser);

            #endregion Company User

            int freetrialCodeId = Utils.GetCodeByValue("ProjectStatus", "FREETRIAL").CodeId;
            int freeTrialProjectTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;

            #region Project

            StageBitz.Data.Project project = new StageBitz.Data.Project();
            project.ProjectName = projectName;
            project.ProjectStatusCodeId = freetrialCodeId;
            project.ProjectTypeCodeId = freeTrialProjectTypeCodeId;
            project.ExpirationDate = Utils.Today.AddDays(int.Parse(Utils.GetSystemValue("FreeTrialDays"))); //Expires from 3 weeks
            project.IsActive = true;
            project.CreatedDate = Utils.Now;
            project.LastUpdatedDate = Utils.Now;
            project.CountryId = countryId;
            DataContext.Projects.AddObject(project);

            #endregion Project

            #region Project User

            ProjectUser projectUser = new ProjectUser();
            projectUser.ProjectUserTypeCodeId = Utils.GetCodeIdByCodeValue("ProjectUserTypeCode", "PROJADMIN");
            projectUser.UserId = userId;
            projectUser.Role = "N/A";
            projectUser.IsActive = true;
            projectUser.CreatedDate = Utils.Now;
            projectUser.LastUpdatedDate = Utils.Now;
            DataContext.ProjectUsers.AddObject(projectUser);

            #endregion Project User

            //Update Project Daily Usage Summary
            //ProjectUsageHandler.UpdateProjectUsage(project, userId, userId, false, Utils.Today, DataContext);

            //Do not call SaveChanges(), because it is being called from SetRespondToWelcomeMessage().
            SetRespondToWelcomeMessage(userId);
        }

        /// <summary>
        /// Creates the first company and inventory.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="countryId">The country identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void CreateFirstCompanyAndInventory(String companyName, int countryId, int userId)
        {
            #region Company

            StageBitz.Data.Company company = new Data.Company();
            company.CompanyName = companyName;
            company.IsActive = true;
            company.CountryId = countryId;
            company.CreatedDate = Utils.Now;
            company.LastUpdatedDate = Utils.Now;
            company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
            //company.IsCompanyVisibleForSearchInInventory = true;
            DataContext.Companies.AddObject(company);

            #endregion Company

            #region Company User

            CompanyUser companyUser = new CompanyUser();
            companyUser.CreatedDate = Utils.Now;
            companyUser.LastUpdatedDate = Utils.Now;
            //companyUser.CompanyUserTypeCodeId = Support.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            companyUser.UserId = userId;
            companyUser.IsActive = true;

            companyUser.CompanyUserRoles.Add(
                new CompanyUserRole
                {
                    IsActive = true,
                    CompanyUserTypeCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN"),
                    CreatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedByUserId = userId,
                    LastUpdatedDate = Utils.Now
                }
            );

            companyUser.CompanyUserRoles.Add(
                new CompanyUserRole
                {
                    IsActive = true,
                    CompanyUserTypeCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN"),
                    CreatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedByUserId = userId,
                    LastUpdatedDate = Utils.Now
                }
            );

            DataContext.CompanyUsers.AddObject(companyUser);

            #endregion Company User

            #region Payment Package

            PricePlanDetails pricePlanDetails = new PricePlanDetails
            {
                CompanyId = company.CompanyId,
                DiscountCodeUsage = null,
                InventoryPaymentPackageTypeId = Utils.GetFreeSystemInventoryPackageDetail().PackageTypeId,
                ProjectPaymentPackageTypeId = Utils.GetFreeSystemProjectPackageDetail().PackageTypeId,
                PaymentDurationCodeId = Utils.GetCodeIdByCodeValue("PaymentPackageDuration", "ANUAL"),
                TotalAmount = 0,
                TotalAmountForPeriod = 0,
                TotalAmountWithEducationalPackage = 0,
                DiscountCode = null,
                IsEducationalPackage = false
            };

            FinanceBL financeBL = new FinanceBL(DataContext);
            financeBL.SaveCompanyPackage(userId, null, pricePlanDetails, false);

            #endregion Payment Package

            SetRespondToWelcomeMessage(userId);
        }

        /// <summary>
        /// Sets the responded flag to welcome message.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void SetRespondToWelcomeMessage(int userId)
        {
            var user = DataContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
            user.HasRespondedToWelcomeMessage = true;
            SaveChanges();
        }

        /// <summary>
        /// Determines whether project is in grace period.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <returns></returns>
        public bool IsProjectInGracePeriod(int projectID)
        {
            StageBitz.Data.Project project = GetProject(projectID);

            if (project != null)
            {
                ProjectStatusHandler.ProjectWarningInfo projectWarning = ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);

                if (projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.GracePeriod)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Should the stop processing.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool ShouldStopProcessing(string relatedTable, int relatedId, int userId)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            bool shouldStopProcess = false;

            if (relatedTable == "Project")
            {
                var project = GetProject(relatedId);
                if (project != null)
                {
                    if (IsProjectClosed(relatedId) || project.ProjectStatusCodeId == Utils.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId)
                    {
                        shouldStopProcess = true;
                    }
                }
            }
            else if (relatedTable == "Company")
            {
                if (!Utils.CanAccessInventory(relatedId, userId) || companyBL.HasCompanySuspendedbySBAdmin(relatedId) || companyBL.IsCompanySuspended(relatedId))
                {
                    shouldStopProcess = true;
                }
            }
            return shouldStopProcess;
        }

        /// <summary>
        /// Checks the removability with project status.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public bool CheckRemovabilityWithProjectStatus(int projectId)
        {
            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            int suspendedProjectStatusId = Utils.GetCodeByValue("ProjectStatus", "SUSPENDED").CodeId;

            var project = (from p in DataContext.Projects where p.ProjectId == projectId && (p.ProjectStatusCodeId != closedProjectStatusCodeId && p.ProjectStatusCodeId != suspendedProjectStatusId) select p);

            if (project == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Gets the active project list for user dashboard.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<ProjectListInfo> GetProjectListUserDashboardActiveProjectList(int userId)
        {
            int primaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int secondaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");

            int completedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            return (from p in DataContext.Projects
                    from cu in
                        (
                            from cuTemp in DataContext.CompanyUsers
                            join cur in DataContext.CompanyUserRoles on cuTemp.CompanyUserId equals cur.CompanyUserId
                            where cuTemp.CompanyId == p.CompanyId && cuTemp.IsActive && cuTemp.UserId == userId
                                 && (cur.CompanyUserTypeCodeId == primaryAdminCodeId || cur.CompanyUserTypeCodeId == secondaryAdminCodeId) && cuTemp.IsActive && cur.IsActive
                            select cuTemp).Take(1).DefaultIfEmpty()
                    from unf in DataContext.UserNotificationSettings.Where(us => us.UserID == userId && us.RelatedTable == "Project" && us.RelatedId == p.ProjectId).Take(1).DefaultIfEmpty()
                    join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                    where pu.UserId == userId && pu.IsActive && p.ProjectStatusCodeId != closedProjectStatusCodeId
                    select new ProjectListInfo
                                {
                                    ProjectId = p.ProjectId,
                                    ProjectName = p.ProjectName,
                                    ProjectTypeCodeId = p.ProjectTypeCodeId,
                                    CompanyId = p.CompanyId,
                                    CompanyName = p.Company.CompanyName,
                                    //Get project role
                                    ProjectRole = pu.Role,
                                    EndDate = p.EndDate,
                                    ProjectStatusCodeId = p.ProjectStatusCodeId,
                                    ExpirationDate = p.ExpirationDate,
                                    PaymentsSpecified = false,
                                    ItemCount = p.ItemBriefs.Count,
                                    CompletedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == completedItemStatusCodeId select new { i.Quantity }).Count(),
                                    InProgressItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == inProgressItemStatusCodeId select new { i.Quantity }).Count(),
                                    NotStartedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == notStartedItemStatusCodeId select new { i.Quantity }).Count(),
                                    InvitationId = 0,
                                    IsCompanyAdmin = (cu != null), //This is to track whether user is a company admin.
                                    NotificationCount = (from nf in DataContext.Notifications
                                                         where nf.ProjectId == p.ProjectId
                                                             && (unf == null || nf.NotificationId > unf.LastNotificationId.Value)
                                                             && ((unf != null && unf.ShowMyNotifications) || nf.CreatedByUserId != userId)
                                                         select nf).Count(),
                                    ClosedByUserId = 0,
                                    ClosedByName = string.Empty,
                                    ClosedOn = null
                                }).ToList();
        }

        /// <summary>
        /// Gets the invited project list for user dashboard.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<ProjectListInfo> GetProjectListUserDashboardInvitedProjectList(int userId)
        {
            int primaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int secondaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");

            int projectInvitationTypeCodeId = Utils.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");
            int pendingInvitationStatusCodeId = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");

            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            return (from inv in DataContext.Invitations
                    join p in DataContext.Projects on inv.RelatedId equals p.ProjectId
                    //from cu in DataContext.CompanyUsers.Where(compuser => compuser.CompanyId == p.CompanyId && compuser.UserId == UserID && compuser.IsActive == true).Take(1).DefaultIfEmpty()
                    from cu in
                        (
                            from cuTemp in DataContext.CompanyUsers
                            join cur in DataContext.CompanyUserRoles on cuTemp.CompanyUserId equals cur.CompanyUserId
                            where cuTemp.CompanyId == p.CompanyId && cuTemp.IsActive && cuTemp.UserId == userId
                                 && (cur.CompanyUserTypeCodeId == primaryAdminCodeId || cur.CompanyUserTypeCodeId == secondaryAdminCodeId) && cuTemp.IsActive && cur.IsActive
                            select cuTemp).Take(1).DefaultIfEmpty()
                    where inv.ToUserId == userId && inv.InvitationTypeCodeId == projectInvitationTypeCodeId
                            && inv.InvitationStatusCodeId == pendingInvitationStatusCodeId && p.ProjectStatusCodeId != closedProjectStatusCodeId
                    select new ProjectListInfo
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        ProjectTypeCodeId = p.ProjectTypeCodeId,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.CompanyName,
                        ProjectRole = inv.ProjectRole,
                        EndDate = p.EndDate,
                        ProjectStatusCodeId = p.ProjectStatusCodeId,
                        ExpirationDate = p.ExpirationDate,
                        PaymentsSpecified = false,
                        ItemCount = 0,
                        CompletedItemCount = 0,
                        InProgressItemCount = 0,
                        NotStartedItemCount = 0,
                        InvitationId = inv.InvitationId,
                        IsCompanyAdmin = (cu != null), //This is to track whether user is a company admin.
                        NotificationCount = 0,
                        ClosedByUserId = 0,
                        ClosedByName = string.Empty,
                        ClosedOn = null
                    }).ToList();
        }

        /// <summary>
        /// Gets the active project list for company dashboard.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<ProjectListInfo> GetProjectListCompanyDashboardActiveProjectList(int userId, int companyId)
        {
            int primaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int secondaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");

            int completedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            CreditCardToken companyCreditCardToken = FinanceSupport.GetCreditCardToken("Company", companyId);
            bool paymentsSpecified = (companyCreditCardToken != null);

            return (from p in DataContext.Projects
                    where p.CompanyId == companyId && p.IsActive == true && p.ProjectStatusCodeId != closedProjectStatusCodeId
                    from cu in
                        (
                            from cuTemp in DataContext.CompanyUsers
                            join cur in DataContext.CompanyUserRoles on cuTemp.CompanyUserId equals cur.CompanyUserId
                            where cuTemp.CompanyId == p.CompanyId && cuTemp.IsActive && cuTemp.UserId == userId
                                 && (cur.CompanyUserTypeCodeId == primaryAdminCodeId || cur.CompanyUserTypeCodeId == secondaryAdminCodeId) && cuTemp.IsActive && cur.IsActive
                            select cuTemp).Take(1).DefaultIfEmpty()
                    select new ProjectListInfo
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        ProjectTypeCodeId = p.ProjectTypeCodeId,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.CompanyName,
                        ProjectRole = (from projUser in p.ProjectUsers where projUser.UserId == userId select projUser.Role).FirstOrDefault(),
                        EndDate = p.EndDate,
                        ProjectStatusCodeId = p.ProjectStatusCodeId,
                        ExpirationDate = p.ExpirationDate,
                        PaymentsSpecified = paymentsSpecified,
                        ItemCount = p.ItemBriefs.Count,
                        CompletedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == completedItemStatusCodeId select new { i.Quantity }).Count(),
                        InProgressItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == inProgressItemStatusCodeId select new { i.Quantity }).Count(),
                        NotStartedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == notStartedItemStatusCodeId select new { i.Quantity }).Count(),
                        InvitationId = 0,
                        NotificationCount = 0,
                        IsCompanyAdmin = (cu != null), //This is to track whether user is a company admin.
                        ClosedByUserId = 0,
                        ClosedByName = string.Empty,
                        ClosedOn = null
                    }).ToList();
        }

        /// <summary>
        /// Gets the closed project list for company dashboard.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<ProjectListInfo> GetProjectListCompanyDashboardClosedProjectList(int userId, int companyId)
        {
            int primaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int secondaryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");

            int completedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            CreditCardToken companyCreditCardToken = FinanceSupport.GetCreditCardToken("Company", companyId);
            bool paymentsSpecified = (companyCreditCardToken != null);

            return (from p in DataContext.Projects
                    where p.CompanyId == companyId && p.IsActive == true && p.ProjectStatusCodeId == closedProjectStatusCodeId
                    //from cu in DataContext.CompanyUsers.Where(compuser => compuser.CompanyId == p.CompanyId && compuser.UserId == UserID && compuser.IsActive == true).Take(1).DefaultIfEmpty()
                    from cu in
                        (
                            from cuTemp in DataContext.CompanyUsers
                            join cur in DataContext.CompanyUserRoles on cuTemp.CompanyUserId equals cur.CompanyUserId
                            where cuTemp.CompanyId == p.CompanyId && cuTemp.IsActive && cuTemp.UserId == userId
                                 && (cur.CompanyUserTypeCodeId == primaryAdminCodeId || cur.CompanyUserTypeCodeId == secondaryAdminCodeId) && cuTemp.IsActive && cur.IsActive
                            select cuTemp).Take(1).DefaultIfEmpty()
                    join pArch in DataContext.ProjectArchives on p.ProjectId equals pArch.ProjectId
                    where cu != null // closed projects are only displayed for compnay admins
                    select new ProjectListInfo
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        ProjectTypeCodeId = p.ProjectTypeCodeId,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.CompanyName,
                        ProjectRole = (from projUser in p.ProjectUsers where projUser.UserId == userId select projUser.Role).FirstOrDefault(),
                        EndDate = p.EndDate,
                        ProjectStatusCodeId = p.ProjectStatusCodeId,
                        ExpirationDate = p.ExpirationDate,
                        PaymentsSpecified = paymentsSpecified,
                        ItemCount = p.ItemBriefs.Count,
                        CompletedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == completedItemStatusCodeId select new { i.Quantity }).Count(),
                        InProgressItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == inProgressItemStatusCodeId select new { i.Quantity }).Count(),
                        NotStartedItemCount = (from i in DataContext.ItemBriefs where i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == notStartedItemStatusCodeId select new { i.Quantity }).Count(),
                        InvitationId = 0,
                        NotificationCount = 0,
                        IsCompanyAdmin = (cu != null), //This is to track whether user is a company admin.
                        ClosedByUserId = pArch.ProjectClosedBy,
                        ClosedByName = (string.Concat(pArch.User.FirstName, " ", pArch.User.LastName)),
                        ClosedOn = pArch.ProjectClosedDate
                    }).ToList();
        }

        /// <summary>
        /// Gets the task list.
        /// </summary>
        /// <param name="taskListId">The task list identifier.</param>
        /// <returns></returns>
        public List<ItemBriefTaskDetails> GetTaskList(int taskListId)
        {
            return (from tl in DataContext.TaskLists
                    join ibtl in DataContext.TaskListsItemBriefTasks on tl.TaskListId equals ibtl.TaskListId
                    join ibt in DataContext.ItemBriefTasks on ibtl.ItemBriefTaskId equals ibt.ItemBriefTaskId
                    join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                    join c in DataContext.Codes on ibt.ItemBriefTaskStatusCodeId equals c.CodeId
                    where tl.TaskListId == taskListId
                    orderby c.SortOrder
                    select new ItemBriefTaskDetails
                    {
                        ItemBriefTask = ibt,
                        ItemBriefName = ib.Name,
                        SortOrder = c.SortOrder,
                        Total = ibt.TotalCost != null ? ibt.TotalCost : 0
                    }).ToList<ItemBriefTaskDetails>();
        }

        /// <summary>
        /// Removes the task from list.
        /// </summary>
        /// <param name="itemBriefTaskId">The item brief task identifier.</param>
        /// <param name="taskListId">The task list identifier.</param>
        public void RemoveTaskFromList(int itemBriefTaskId, int taskListId)
        {
            TaskListsItemBriefTask taskListItemBriefTask = DataContext.TaskListsItemBriefTasks.First(tlib => tlib.ItemBriefTaskId == itemBriefTaskId && tlib.TaskListId == taskListId);
            if (taskListItemBriefTask != null)
            {
                DataContext.TaskListsItemBriefTasks.DeleteObject(taskListItemBriefTask);
                SaveChanges();
            }
        }

        /// <summary>
        /// Gets the hidden item count for project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public int GetHiddenItemCountForProject(int projectId)
        {
            //double check whether we want to check the ibi status
            return (from ibs in DataContext.ItemBookings
                    join ib in DataContext.ItemBriefs on ibs.RelatedId equals ib.ItemBriefId
                    where ibs.RelatedTable == "ItemBrief" && ib.ProjectId == projectId && ibs.IsActive && ibs.Item.IsHidden
                    select ibs).Count();
        }

        /// <summary>
        /// Determines whether project has hidden items.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public bool HasHiddenItemsForProject(int projectId)
        {
            return GetHiddenItemCountForProject(projectId) > 0;
        }

        /// <summary>
        /// Closes the project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public int CloseProject(int projectId, int userId)
        {
            int status = DataContext.CloseProject(projectId, userId);
            Data.Project project = GetProject(projectId);
            DataContext.Refresh(RefreshMode.StoreWins, project);
            return status;
        }

        /// <summary>
        /// Gets the latest project archive by company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Data.ProjectArchive GetLatestProjectArchiveByCompanyId(int companyId, int userId)
        {
            StageBitz.Data.ProjectArchive projectArchive = (from p in DataContext.Projects
                                                            join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                                                            join pa in DataContext.ProjectArchives on p.ProjectId equals pa.ProjectId
                                                            where p.CompanyId == companyId && pu.UserId == userId
                                                            orderby pa.ProjectClosedDate descending
                                                            select pa).FirstOrDefault();
            return projectArchive;
        }

        /// <summary>
        /// Gets the top ten project events by company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<ProjectEvent> GetTopTenProjectEventsByCompanyId(int companyId)
        {
            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            return (from pe in DataContext.ProjectEvents
                    join proj in DataContext.Projects on pe.ProjectId equals proj.ProjectId
                    where pe.Project.CompanyId == companyId && pe.EventDate > Utils.Now && proj.IsActive == true && proj.ProjectStatusCodeId != closedProjectStatusCodeId
                    orderby pe.EventDate
                    select pe).Take(10).ToList<ProjectEvent>();
        }

        /// <summary>
        /// Gets the top ten project events by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<ProjectEvent> GetTopTenProjectEventsByUserId(int userId)
        {
            int closedProjectStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            return (from pe in DataContext.ProjectEvents
                    join proj in DataContext.Projects on pe.ProjectId equals proj.ProjectId
                    join pu in DataContext.ProjectUsers on proj.ProjectId equals pu.ProjectId
                    where pu.UserId == userId && pu.IsActive && proj.IsActive == true && pe.EventDate > Utils.Now && proj.ProjectStatusCodeId != closedProjectStatusCodeId
                    orderby pe.EventDate, pe.CreatedDate
                    select pe).Take(10).ToList<ProjectEvent>();
        }

        /// <summary>
        /// Gets the project team activities from google analytics.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        public List<ProjectTeamActivity> GetProjectTeamActivitiesFromGoogleAnalytics(int companyId, DateTime startDate, DateTime endDate,
            int pageSize, int pageIndex, string sortField, bool isAscending, out int totalRecords)
        {
            AnalyticsManager analyticsManager = new AnalyticsManager();
            List<ProjectTeamActivity> projectActivities = analyticsManager.GetProjectTeamActivities(companyId, startDate, endDate, pageSize, pageIndex, sortField, isAscending, out totalRecords);

            var projectTeamActivityList = from pa in projectActivities
                                          join u in DataContext.Users on pa.UserId equals u.UserId
                                          from cu in DataContext.CompanyUsers.Where(cu => cu.UserId == pa.UserId && cu.CompanyId == companyId).Take(1).DefaultIfEmpty()
                                          join p in DataContext.Projects on pa.ProjectId equals p.ProjectId
                                          from pu in DataContext.ProjectUsers.Where(pu => pu.UserId == u.UserId && pu.ProjectId == p.ProjectId).Take(1).DefaultIfEmpty()
                                          where p.CompanyId == companyId
                                          select new ProjectTeamActivity
                                          {
                                              Date = pa.Date,
                                              ProjectName = p.ProjectName,
                                              UserName = string.Concat(u.FirstName, " ", u.LastName),
                                              UserId = u.UserId,
                                              Role = pu != null ? pu.Role.Replace("N/A", string.Empty) : string.Empty,
                                              Permission = pu != null ? pu.Code.Description : cu != null ? "Company Administrator" : string.Empty,
                                              SessionTotal = pa.SessionTotal
                                          };

            return projectTeamActivityList.ToList();
        }

        /// <summary>
        /// Gets the free trial projects not in closed status.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public List<Data.Project> GetFreeTrialProjectsNotInClosedStatus(int companyId)
        {
            return GetFreeTrialProjectsNotInClosedStatus(companyId, Utils.Now);
        }

        /// <summary>
        /// Gets the free trial projects not in closed status.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="time">The date.</param>
        /// <returns></returns>
        public List<Data.Project> GetFreeTrialProjectsNotInClosedStatus(int companyId, DateTime time)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            // free trial project cannot be in grace period.(also ExpirationDate is not null during grae period)
            int gracePeriodStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;

            int freeTrialTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;
            int freeTrialOptinTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;

            TimeSpan ts;
            TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts);

            return DataContext.Projects.Where(p => p.CompanyId == companyId &&
                    p.ProjectStatusCodeId != closedStatusCodeId && p.ProjectStatusCodeId != gracePeriodStatusCodeId && p.ExpirationDate.HasValue &&
                    (EntityFunctions.CreateDateTime(p.ExpirationDate.Value.Year, p.ExpirationDate.Value.Month, p.ExpirationDate.Value.Day, ts.Hours, ts.Minutes, ts.Seconds)) > time &&
                    (p.ProjectTypeCodeId == freeTrialTypeCodeId || p.ProjectTypeCodeId == freeTrialOptinTypeCodeId)).ToList();
        }

        /// <summary>
        /// Gets all the active projects for a Company
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>Active Projects</returns>
        public List<Data.Project> GetAllActiveProjects(int companyId)
        {
            int activeStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "ACTIVE").CodeId;
            int gracePeriodStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;

            return DataContext.Projects.Where(p => p.CompanyId == companyId &&
                   (p.ProjectStatusCodeId == activeStatusCodeId || p.ProjectStatusCodeId == gracePeriodStatusCodeId)).ToList<Data.Project>();
        }

        /// <summary>
        /// Determines whether project is free trial project.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <param name="time">The time.</param>
        /// <returns>
        ///   <c>true</c> if [is free trial project] [the specified project id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFreeTrialProject(int projectId, DateTime time)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            int freeTrialTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;
            int freeTrialOptinTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;

            TimeSpan ts;
            TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts);

            return (DataContext.Projects.Where(p => p.ProjectId == projectId &&
                    p.ProjectStatusCodeId != closedStatusCodeId && p.ExpirationDate.HasValue &&
                    (EntityFunctions.CreateDateTime(p.ExpirationDate.Value.Year, p.ExpirationDate.Value.Month, p.ExpirationDate.Value.Day, ts.Hours, ts.Minutes, ts.Seconds)) > time &&
                    (p.ProjectTypeCodeId == freeTrialTypeCodeId || p.ProjectTypeCodeId == freeTrialOptinTypeCodeId))).Count() > 0;
        }

        /// <summary>
        /// Determines whether project is free trial project for current date.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns>
        ///   <c>true</c> if [is free trial project] [the specified project id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFreeTrialProject(int projectId)
        {
            return IsFreeTrialProject(projectId, Utils.Now);
        }

        /// <summary>
        /// Gets the free trial ended projects.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public List<Data.Project> GetFreeTrialEndedProjects(int companyId)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            int freeTrialTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIAL").CodeId;
            int freeTrialOptinTypeCodeId = Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId;

            return DataContext.Projects.Where(p => p.CompanyId == companyId &&
                    (p.ProjectStatusCodeId == closedStatusCodeId || !p.ExpirationDate.HasValue) &&
                    (p.ProjectTypeCodeId == freeTrialTypeCodeId || p.ProjectTypeCodeId == freeTrialOptinTypeCodeId)).ToList();
        }

        /// <summary>
        /// Gets the project user info for given user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public List<ProjectUserInfo> GetProjectUserInfo(int userId)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;

            return (from pu in DataContext.ProjectUsers
                    join u in DataContext.Users on pu.UserId equals u.UserId
                    join p in DataContext.Projects on pu.ProjectId equals p.ProjectId
                    join c in DataContext.Companies on p.CompanyId equals c.CompanyId
                    where u.IsActive && p.IsActive && pu.IsActive && c.IsActive && p.ProjectStatusCodeId != closedStatusCodeId
                     && u.UserId == userId
                    select new ProjectUserInfo
                    {
                        ProjectId = p.ProjectId,
                        CompanyEmailNotificationCodeId = u.CompanyEmailNotificationCodeId,
                        CompanyName = c.CompanyName,
                        CompanyId = c.CompanyId,
                        ProjectEmailNotificationCodeId = u.ProjectEmailNotificationCodeId,
                        ProjectName = p.ProjectName,
                        UserId = u.UserId,
                        UserName = u.FirstName,
                        UserEmail = u.Email1
                    }).ToList();
        }

        /// <summary>
        /// Gets the company admin project user info for given user.
        /// </summary>
        /// <param name="projectIds">The project ids.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public List<ProjectUserInfo> GetCompanyAdminProjectUserInfo(List<int> projectIds, int userId)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            int companyAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "SECADMIN").CodeId;
            int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;

            return (from cu in DataContext.CompanyUsers
                    join u in DataContext.Users on cu.UserId equals u.UserId
                    join c in DataContext.Companies on cu.CompanyId equals c.CompanyId
                    join p in DataContext.Projects on c.CompanyId equals p.CompanyId
                    join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                    where projectIds.Contains(p.ProjectId) && u.IsActive && p.IsActive && cu.IsActive && c.IsActive && p.ProjectStatusCodeId != closedStatusCodeId
                        && cur.IsActive && (cur.CompanyUserTypeCodeId == companyAdminCodeID || cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID)
                        && u.UserId == userId
                    select new ProjectUserInfo
                    {
                        ProjectId = p.ProjectId,
                        CompanyEmailNotificationCodeId = u.CompanyEmailNotificationCodeId,
                        CompanyName = c.CompanyName,
                        CompanyId = c.CompanyId,
                        ProjectEmailNotificationCodeId = u.ProjectEmailNotificationCodeId,
                        ProjectName = p.ProjectName,
                        UserId = u.UserId,
                        UserName = u.FirstName,
                        UserEmail = u.Email1
                    }
                ).ToList();
        }

        /// <summary>
        /// Gets all project user and company user ids for given project id list.
        /// </summary>
        /// <param name="projectIds">The project ids.</param>
        /// <returns></returns>
        public List<int> GetAllProjectUserAndCompanyUserIds(List<int> projectIds)
        {
            int closedStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId;
            int companyAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "SECADMIN").CodeId;
            int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;

            var projectUserUserIds = from pu in DataContext.ProjectUsers
                                     where projectIds.Contains(pu.ProjectId) && pu.IsActive
                                     select pu.UserId;

            var companyUserUserIds = from cu in DataContext.CompanyUsers
                                     join u in DataContext.Users on cu.UserId equals u.UserId
                                     join c in DataContext.Companies on cu.CompanyId equals c.CompanyId
                                     join p in DataContext.Projects on c.CompanyId equals p.CompanyId
                                     join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                     where projectIds.Contains(p.ProjectId) && u.IsActive && p.IsActive && cu.IsActive && c.IsActive && p.ProjectStatusCodeId != closedStatusCodeId
                                         && cur.IsActive && (cur.CompanyUserTypeCodeId == companyAdminCodeID || cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID)
                                     select cu.UserId;

            return projectUserUserIds.Union(companyUserUserIds).ToList();
        }

        /// <summary>
        /// Gets the project attachments.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public List<ProjectAttachment> GetProjectAttachments(int relatedId)
        {
            string fileExtension = "Hyperlink";
            List<ProjectAttachment> combinedList = (from m in DataContext.DocumentMedias
                                                    join u in DataContext.Users on m.LastUpdatedBy equals u.UserId
                                                    where m.RelatedTableName == "Project" && m.RelatedId == relatedId
                                                    select new ProjectAttachment
                                                    {
                                                        DocumentMediaId = m.DocumentMediaId,
                                                        Description = m.Description,
                                                        Name = m.Name,
                                                        IsImageFile = m.IsImageFile,
                                                        FileExtension = m.FileExtension.ToUpper(),
                                                        LastUpdatedDate = m.LastUpdatedDate,
                                                        LastUpdatedBy = string.Concat(u.FirstName, " ", u.LastName)
                                                    }).ToList<ProjectAttachment>();

            List<ProjectAttachment> hyperlinkList = combinedList.Where(cl => cl.FileExtension == fileExtension.ToUpper()).ToList();
            List<ProjectAttachment> attachmntList = combinedList.Where(cl => cl.FileExtension != fileExtension.ToUpper()).ToList();

            hyperlinkList.AddRange(attachmntList);
            return hyperlinkList;
        }

        /// <summary>
        /// Determines whether user has read only rights for project.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="shouldConsiderProjectStatus">if set to <c>true</c> [should consider project status].</param>
        /// <returns></returns>
        public bool IsReadOnlyRightsForProject(int projectID, int userId, bool shouldConsiderProjectStatus = true)
        {
            CompanyBL companyBL = new CompanyBL(DataContext);
            StageBitz.Data.Project project = DataContext.Projects.Where(p => p.ProjectId == projectID).FirstOrDefault();

            #region Check Read-only based on project status

            if (shouldConsiderProjectStatus && project != null)
            {
                ProjectStatusHandler.ProjectWarningInfo projectWarning =
                    ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == Utils.GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);

                if (projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Suspended
                    || projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.PaymentFailed || projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Closed)
                {
                    return true;
                }
            }

            #endregion Check Read-only based on project status

            //Company Admins will have the full rights for project
            if (companyBL.IsCompanyAdministrator(project.CompanyId, userId))
            {
                return false;
            }

            int observerCodeID = Utils.GetCodeByValue("ProjectUserTypeCode", "OBSERVER").CodeId;
            var projectUser = (from p in DataContext.Projects
                               join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                               where p.ProjectId == projectID && pu.UserId == userId && pu.IsActive == true
                               select pu).FirstOrDefault();

            if (projectUser == null || projectUser.ProjectUserTypeCodeId == observerCodeID) //only observers will have the read only permission
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the project document media ids.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public List<int> GetProjectDocumentMediaIds(int projectId)
        {
            return DataContext.DocumentMedias.Where(dm => dm.RelatedTableName == "Project" && dm.RelatedId == projectId).Select(dm => dm.DocumentMediaId).ToList();
        }

        /// <summary>
        /// Gets the item type document media by project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public IEnumerable<ItemTypeDocumentMedia> GetItemTypeDocumentMediaByProject(int projectId)
        {
            int primaryItemTypeCodeId = Utils.GetCodeByValue("ItemBriefType", "PRIMARY").CodeId;

            return (from ib in DataContext.ItemBriefs.Where(ib => ib.ProjectId == projectId)
                    from ibt in DataContext.ItemBriefTypes.Where(ibt => ibt.ItemBriefTypeCodeId == primaryItemTypeCodeId && ibt.IsActive && ib.ItemBriefId == ibt.ItemBriefId).Take(1)
                    join it in DataContext.ItemTypes on ibt.ItemTypeId equals it.ItemTypeId
                    from dm in DataContext.DocumentMedias.Where(dm => dm.RelatedTableName == "ItemBrief" && dm.RelatedId == ib.ItemBriefId && dm.DocumentMediaContent != null).DefaultIfEmpty()
                    group new
                    {
                        DocumentMediaId = dm != null ? dm.DocumentMediaId : 0,
                        ItemTypeName = it.Name,
                        ItemTypeId = it.ItemTypeId,
                        EntityName = ib.Name,
                        EntityId = ib.ItemBriefId
                    } by it.ItemTypeId into grp
                    select new ItemTypeDocumentMedia
                    {
                        ItemTypeName = grp.FirstOrDefault().ItemTypeName,
                        DocumentMedias = grp.Where(g => g.DocumentMediaId > 0).Select(g => new DocumentMediaInfo
                        {
                            DocumentMediaId = g.DocumentMediaId,
                            EntityId = g.EntityId,
                            EntityName = g.EntityName
                        }),
                        ItemTypeId = grp.FirstOrDefault().ItemTypeId
                    });
        }
    }
}