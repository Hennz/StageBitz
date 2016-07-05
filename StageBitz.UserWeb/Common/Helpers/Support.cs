using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance.Project;
using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Common.Helpers
{
    /// <summary>
    /// Support class for User.Web
    /// </summary>
    public static class Support
    {
        #region Properties

        #region Logged-in User related

        /// <summary>
        /// Gets or sets user ID for the logged in user.
        /// </summary>
        /// <value>
        /// The user ide.
        /// </value>
        public static int UserID
        {
            get
            {
                if (HttpContext.Current.Session["UserID"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return (int)HttpContext.Current.Session["UserID"];
            }
            private set
            {
                HttpContext.Current.Session["UserID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the login name for the logged in user.
        /// </summary>
        /// <value>
        /// The name of the login user.
        /// </value>
        public static string LoginName
        {
            get
            {
                if (HttpContext.Current.Session["LoginName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["LoginName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["LoginName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the first name for the logged in user.
        /// </summary>
        public static string UserFirstName
        {
            get
            {
                if (HttpContext.Current.Session["UserFirstName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserFirstName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserFirstName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name for the logged in user
        /// </summary>
        /// <value>
        /// The last name of the user.
        /// </value>
        public static string UserLastName
        {
            get
            {
                if (HttpContext.Current.Session["UserLastName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserLastName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserLastName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name for the logged in user
        /// </summary>
        /// <value>
        /// The user primary email.
        /// </value>
        public static string UserPrimaryEmail
        {
            get
            {
                if (HttpContext.Current.Session["UserPrimaryEmail"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserPrimaryEmail"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserPrimaryEmail"] = value;
            }
        }

        /// <summary>
        /// Gets the full name of the user
        /// </summary>
        /// <value>
        /// The full name of the user.
        /// </value>
        public static string UserFullName
        {
            get
            {
                return (Support.UserFirstName + " " + Support.UserLastName).Trim();
            }
        }

        /// <summary>
        /// Gets or sets the country code for the logged in user.
        /// </summary>
        /// <value>
        /// The user country code.
        /// </value>
        public static string UserCountryCode
        {
            get
            {
                if (HttpContext.Current.Session["UserCountryCode"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserCountryCode"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserCountryCode"] = value;
            }
        }

        /// <summary>
        /// Gets the standard culture string (eg: 'en-AU') for the logged in user.
        /// </summary>
        /// <value>
        /// The user culture string.
        /// </value>
        public static string UserCultureString
        {
            get
            {
                return "en-" + UserCountryCode;
            }
        }

        /// <summary>
        /// Gets the name of the culture.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        /// <value>
        /// The name of the culture.
        /// </value>
        public static string GetCultureName(string countryCode)
        {
            var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.EndsWith(countryCode)).FirstOrDefault();
            if (cultureInfo == null)
            {
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                return globalizationSection.Culture;
            }
            else
            {
                return cultureInfo.Name;
            }
        }

        #endregion Logged-in User related

        #endregion Properties

        #region Private Methods

        /// <summary>
        /// Tries to initialize the user session using the asp.net authentication cookie.
        /// If authentication cookie is unavailable, user is logged out forcefully.
        /// </summary>
        private static void InitializeUserSessionFromAuthCookie()
        {
            bool isAuthenticated = false;

            //User must be re-authenticated and the session must be reinitialized

            //If remember me cookie is available, setup the session automaitcally by
            //requesting user details from the DB.
            //If the cookie is unavailable, perform a forced Logout.
            try
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    //We are storing the user ID as the username inside asp.net auth cookie (See Login.aspx).
                    //Check whether we can find it inside the cookie.
                    string cookieData = FormsAuthentication.Decrypt(authCookie.Value).Name;
                    string[] cookieDataParts = cookieData.Split(' ');

                    if (cookieDataParts.Length == 2)
                    {
                        string username = cookieDataParts[0];
                        string passwordHash = cookieDataParts[1];

                        //If a remembered user exists, get user details from the DB
                        using (StageBitzDB dataContext = new StageBitzDB())
                        {
                            PersonalBL personalBL = new PersonalBL(dataContext);
                            User user = personalBL.AuthenticateUser(username, passwordHash);

                            if (user != null)
                            {
                                //If authentication is successful, set the session variables
                                isAuthenticated = true;
                                SetUserSessionData(user);

                                //To Record the date where the user gets reset the session.
                                user.LastLoggedInDate = Now;
                                dataContext.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch
            {
                HttpContext.Current.Response.Redirect("~/Account/Logout.aspx", true);
            }

            //If all attempts to authenticate the user has failed, perform a forced logout
            if (!isAuthenticated)
            {
                HttpContext.Current.Response.Redirect("~/Account/Logout.aspx", true);
            }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Updates the session variables with logged in user data
        /// </summary>
        /// <param name="user">The logged in user</param>
        public static void SetUserSessionData(User user)
        {
            UserID = user.UserId;
            LoginName = user.LoginName;
            UserFirstName = user.FirstName;
            UserLastName = user.LastName;
            UserPrimaryEmail = user.Email1;
            if (user.Country != null)
            {
                UserCountryCode = user.Country.CountryCode;
            }
        }

        #region Code values and System values

        /// <summary>
        /// Gets the code by codeId.
        /// </summary>
        /// <param name="codeId">The code identifier.</param>
        /// <returns>The Code.</returns>
        public static Code GetCodeByCodeId(int codeId)
        {
            return Utils.GetCodeByCodeId(codeId);
        }

        /// <summary>
        /// Gets the code by value.
        /// </summary>
        /// <param name="codeHeader">The code header.</param>
        /// <param name="codeValue">The code value.</param>
        /// <returns>The Code.</returns>
        public static Code GetCodeByValue(string codeHeader, string codeValue)
        {
            return Utils.GetCodeByValue(codeHeader, codeValue);
        }

        /// <summary>
        /// Gets the codes by code header.
        /// </summary>
        /// <param name="codeHeader">The code header.</param>
        /// <returns>List of Codes.</returns>
        public static List<Code> GetCodesByCodeHeader(string codeHeader)
        {
            return Utils.GetCodesByCodeHeader(codeHeader);
        }

        /// <summary>
        /// Gets the code identifier by code value.
        /// </summary>
        /// <param name="codeHeaderName">Name of the code header.</param>
        /// <param name="codeValue">The code value.</param>
        /// <returns>The codeId.</returns>
        public static int GetCodeIdByCodeValue(string codeHeaderName, string codeValue)
        {
            return Utils.GetCodeIdByCodeValue(codeHeaderName, codeValue);
        }

        /// <summary>
        /// Gets the code description.
        /// </summary>
        /// <param name="codeId">The code identifier.</param>
        /// <returns></returns>
        public static string GetCodeDescription(object codeId)
        {
            return Utils.GetCodeByCodeId((int)codeId).Description;
        }

        /// <summary>
        /// Gets the system value.
        /// </summary>
        /// <param name="systemValueName">Name of the system value.</param>
        /// <returns></returns>
        public static string GetSystemValue(string systemValueName)
        {
            return Utils.GetSystemValue(systemValueName);
        }

        #endregion Code values and System values

        #region Date time formatting

        /// <summary>
        /// Formats the date.
        /// </summary>
        /// <param name="dt">The date object.</param>
        /// <returns>Date string.</returns>
        public static string FormatDate(Object dt)
        {
            if (dt != null)
                return Utils.FormatDatetime((DateTime)dt, false);
            else
                return string.Empty;
        }

        #endregion Date time formatting

        #region Currency

        /// <summary>
        /// Formats the currency.
        /// </summary>
        /// <param name="strCurrency">The string currency.</param>
        /// <param name="cultureInfor">The culture infor.</param>
        /// <returns>The currency string.</returns>
        public static string FormatCurrency(object strCurrency, string cultureInfor)
        {
            //return string.Format(new System.Globalization.CultureInfo(cultureInfor), "{0:C}", strCurrency);\
            return Utils.FormatCurrency(strCurrency, cultureInfor);
        }

        #endregion Currency

        #region Common

        /// <summary>
        /// Truncates a string and appends '...' to the end
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The truncated string.</returns>
        public static string TruncateString(object text, int maxLength)
        {
            if (text == null)
            {
                return string.Empty;
            }

            return Utils.Ellipsize(text.ToString(), maxLength);
        }

        /// <summary>
        /// Gets all countries.
        /// </summary>
        /// <returns>The country list.</returns>
        public static List<Country> GetAllCountries()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                List<Country> countries = (from c in dataContext.Countries
                                           where c.IsActive == true
                                           orderby c.SortOrder, c.CountryName
                                           select c).ToList<Country>();
                //Default selection
                Country country = new Country();
                country.CountryName = "-- Please select --";
                country.CountryId = -1;//This is to track whether a country is not being selected
                countries.Insert(0, country);
                return countries.ToList<Country>();
            }
        }

        /// <summary>
        /// Gets the company name by id.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>The company name.</returns>
        public static string GetCompanyNameById(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                StageBitz.Data.Company company = (from c in dataContext.Companies where c.CompanyId == companyId select c).FirstOrDefault();

                return string.Format("{0}", company.CompanyName);
            }
        }

        /// <summary>
        /// Gets the company by project id.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        /// <returns>The company name.</returns>
        public static StageBitz.Data.Company GetCompanyByProjectId(int projectId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                StageBitz.Data.Company company = (from c in dataContext.Companies
                                                  join p in dataContext.Projects on c.CompanyId equals p.CompanyId
                                                  where p.ProjectId == projectId
                                                  select c).FirstOrDefault();

                return company;
            }
        }

        /// <summary>
        /// Gets the length of the database field.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetDatabaseFieldLength<TEntity>(string field)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                var item = dataContext.MetadataWorkspace.GetItem<EntityType>(typeof(TEntity).FullName, DataSpace.CSpace);
                return (int)item.Properties[field].TypeUsage.Facets["MaxLength"].Value;
            }
        }

        /// <summary>
        /// Gets the today.
        /// </summary>
        /// <value>
        /// The today.
        /// </value>
        public static DateTime Today
        {
            get
            {
                return Utils.Today;
            }
        }

        /// <summary>
        /// Gets the now.
        /// </summary>
        /// <value>
        /// The now.
        /// </value>
        public static DateTime Now
        {
            get
            {
                return Utils.Now;
            }
        }

        /// <summary>
        /// Returns the current application Url without the ending '/'
        /// </summary>
        /// <returns>The system url.</returns>
        public static string GetSystemUrl()
        {
            string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath;

            //If the last character of the url is '/' remove it.
            if (url.Length > 0 && url[url.Length - 1] == '/')
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        /// <summary>
        /// Returns the current application image Url for Emails
        /// </summary>
        /// <returns>the current application image Url for Emails.</returns>
        public static string GetSystemLogoPhysicalPath()
        {
            return HttpContext.Current.Server.MapPath("~/Common/Images/StageBitzLogo_small.png");
        }

        #endregion Common

        #region User Access Security

        /// <summary>
        /// Check whether the currently logged in user has access to the specified project
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        public static bool CanAccessProject(StageBitz.Data.Project project)
        {
            if (IsCompanyAdministrator(project.CompanyId))//means a company admin
            {
                return true;
            }
            else
            {
                //Check whether the current user is a member of project users
                var projectUsers = (from pu in project.ProjectUsers
                                    where (pu.UserId == UserID && pu.IsActive)
                                    select pu).FirstOrDefault();
                return (projectUsers != null && project.ProjectStatusCodeId != Utils.GetCodeByValue("ProjectStatus", "CLOSED").CodeId);
            }
        }

        /// <summary>
        /// Check whether the currently logged in user has access to the specified project
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>Can access project.</returns>
        public static bool CanAccessProject(int projectId)
        {
            bool canAccess = false;
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                Data.Project project = dataContext.Projects.Where(p => p.ProjectId == projectId && p.IsActive).FirstOrDefault();
                if (project != null)
                {
                    canAccess = CanAccessProject(project);
                }
            }

            return canAccess;
        }

        /// <summary>
        /// Check whether the currently logged in user has access to the specified user's profile
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Can Access User</returns>
        public static bool CanAccessUser(User user)
        {
            //Check whether the specified user is the currently logged-in user
            if (user.UserId == Support.UserID)
            {
                return true;
            }
            else
            {
                //Check whether the currently logged-in user has the specified user in his contacts
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    bool isCompanyUser = dataContext.CompanyUsers.Where(cu => cu.UserId == user.UserId && cu.IsActive && cu.User.IsActive &&
                                                    dataContext.CompanyUsers.Where(cuT => cuT.UserId == Support.UserID && cuT.IsActive).Select(
                                                        cuT => cuT.CompanyId).Contains(cu.CompanyId)).Count() > 0;

                    bool isProjectUser = dataContext.ProjectUsers.Where(pu => pu.UserId == user.UserId && pu.IsActive && pu.User.IsActive &&
                                                    dataContext.ProjectUsers.Where(puT => puT.UserId == Support.UserID && puT.IsActive).Select(
                                                        puT => puT.ProjectId).Contains(pu.ProjectId)).Count() > 0;

                    return isCompanyUser || isProjectUser;
                }
            }
        }

        /// <summary>
        /// Check user access security for the media item depending on its related table and related id.
        /// </summary>
        /// <param name="media">The media.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns>Can Access Media.</returns>
        public static bool CanAccessMedia(DocumentMedia media, StageBitzDB dataContext)
        {
            switch (media.RelatedTableName)
            {
                case "ItemBrief":
                    StageBitz.Data.ItemBrief itemBrief = dataContext.ItemBriefs.Where(ib => ib.ItemBriefId == media.RelatedId).FirstOrDefault();
                    return CanAccessProject(itemBrief.Project);

                case "ItemVersionHistory":
                    Logic.Business.Inventory.InventoryBL inventoryBL = new Logic.Business.Inventory.InventoryBL(dataContext);
                    return CanAccessProject(inventoryBL.GetProjectByItemVersionHistoryId(media.RelatedId));

                case "Item":
                    return true;

                case "User":
                case "Company":
                    //No security for profile images
                    return true;

                case "Project":
                    Logic.Business.Project.ProjectBL projectBL = new Logic.Business.Project.ProjectBL(dataContext);
                    return CanAccessProject(projectBL.GetProject(media.RelatedId));

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether [is read only rights for project] [the specified project identifier].
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="shouldConsiderProjectStatus">if set to <c>true</c> [should consider project status].</param>
        /// <returns>Is Read Only Rights For Project.</returns>
        public static bool IsReadOnlyRightsForProject(int projectID, int userId = 0, bool shouldConsiderProjectStatus = true)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ProjectBL projectBL = new ProjectBL(dataContext);
                return projectBL.IsReadOnlyRightsForProject(projectID, userId > 0 ? userId : UserID, shouldConsiderProjectStatus);
            }
        }

        /// <summary>
        /// Determines whether is read only rights for company for the specified company id.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>
        ///   <c>true</c> if is read only rights for company for the specified company id; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReadOnlyRightsForCompany(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CompanyBL companyBL = new CompanyBL(dataContext);
                FinanceBL financeBL = new FinanceBL(dataContext);

                bool isAdmin = Support.IsCompanyAdministrator(companyId);
                bool hasCompanySuspended = companyBL.HasCompanySuspendedbySBAdmin(companyId) || companyBL.IsCompanySuspended(companyId);
                bool hasPaymentSetupForFreeTrailEndedCompany = financeBL.HasPackageSelectedForFreeTrailEndedCompany(companyId);

                if (hasCompanySuspended || !isAdmin || !hasPaymentSetupForFreeTrailEndedCompany)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether, is read only rights for pricing plan page for the specified company id.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>
        ///   <c>true</c> if, is read only rights for pricing plan page for the specified company id; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReadOnlyRightsForPricingPlanPage(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CompanyBL companyBL = new CompanyBL(dataContext);

                bool isAdmin = Support.IsCompanyAdministrator(companyId);
                bool hasCompanySuspended = companyBL.HasCompanySuspendedbySBAdmin(companyId) || companyBL.IsCompanyPaymentFailed(companyId) || companyBL.IsCompanyInPaymentFailedGracePeriod(companyId);

                if (hasCompanySuspended || !isAdmin)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether, is read only project by project status for the specified project identifier.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <returns></returns>
        public static bool IsReadOnlyProjectByProjectStatus(int projectID)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                StageBitz.Data.Project project = dataContext.Projects.Where(p => p.ProjectId == projectID).FirstOrDefault();

                if (project != null)
                {
                    ProjectStatusHandler.ProjectWarningInfo projectWarning = ProjectStatusHandler.GetProjectWarningStatus(project.ProjectStatusCodeId, project.ProjectTypeCodeId == GetCodeByValue("ProjectType", "FREETRIALOPTIN").CodeId, project.ExpirationDate);

                    if (projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Suspended
                        || projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.PaymentFailed || projectWarning.WarningStatus == ProjectStatusHandler.ProjectWarningStatus.Closed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Returns whether the given user can see the budget summaries or not
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public static bool CanSeeBudgetSummary(int userId, int projectId)
        {
            Data.Company company = GetCompanyByProjectId(projectId);
            int companyId = 0;
            if (company != null)
            {
                companyId = company.CompanyId;
            }
            if (IsProjectAdministrator(projectId, userId) || IsCompanyAdministrator(companyId, userId))
            {
                return true;
            }
            else
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    Data.ProjectUser projectUser = dataContext.ProjectUsers.Where(pu => pu.ProjectId == projectId && pu.UserId == userId).FirstOrDefault();
                    if (projectUser != null)
                    {
                        return projectUser.CanSeeBudgetSummary;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [has exclusive rights for project] [the specified project identifier].
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <returns></returns>
        public static bool HasExclusiveRightsForProject(int projectID)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int companyId = dataContext.Projects.Where(p => p.ProjectId == projectID).FirstOrDefault().CompanyId;
                //If it is not a Project admin and if payments are failed or the project is suspended.
                return (IsCompanyAdministrator(companyId) || IsProjectAdministrator(projectID)) && !IsReadOnlyProjectByProjectStatus(projectID);
            }
        }

        /// <summary>
        /// Returns whether the currently logged in user is the Administrator of the specified Project.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <returns></returns>
        public static bool IsProjectAdministrator(int projectID)
        {
            return IsProjectAdministrator(projectID, UserID);
        }

        /// <summary>
        /// Returns whether the given user is the Administrator of the specified Project.
        /// </summary>
        /// <param name="projectID">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static bool IsProjectAdministrator(int projectID, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ProjectBL projectBL = new ProjectBL(dataContext);
                return projectBL.IsProjectAdministrator(projectID, userId);
            }
        }

        /// <summary>
        /// Returns whether the currently logged in user is an Administrator of the specified Company.
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyAdministrator(int companyID)
        {
            return IsCompanyAdministrator(companyID, UserID);
        }

        /// <summary>
        /// Returns whether a given user is an Administrator of the specified Company.
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyAdministrator(int companyID, int userId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CompanyBL companyBL = new CompanyBL(dataContext);
                return companyBL.IsCompanyAdministrator(companyID, userId);
            }
        }

        /// <summary>
        /// Determines whether current user is company inventory staff(admin or staff member) for he specified company.
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyInventoryStaffMember(int companyID)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                CompanyUserRole highestRole = inventoryBL.GetHighestInventoryRole(companyID, UserID);
                return highestRole != null && highestRole.Code.SortOrder <= Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").SortOrder;
            }
        }

        /// <summary>
        /// Determines whether [is company primary administrator] [the specified company identifier].
        /// </summary>
        /// <param name="companyID">The company identifier.</param>
        /// <returns></returns>
        public static bool IsCompanyPrimaryAdministrator(int companyID)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int companyPrimaryAdminCodeID = GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;

                CompanyUser companyUser = (from cu in dataContext.CompanyUsers
                                           join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                           where cu.UserId == UserID && cu.CompanyId == companyID && cu.IsActive == true && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                           select cu).SingleOrDefault<CompanyUser>();
                if (companyUser == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion User Access Security

        #region Company

        #endregion Company

        #region Invitation Handling

        /// <summary>
        /// Validates and updates the specified invitation code with specified user ID.
        /// </summary>
        /// <param name="invitationCode">The invitation code query parameter string</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static bool ProcessInvitationCode(string invitationCode, int userId, out string message)
        {
            bool success = true;
            int invitationId = 0;
            int.TryParse(Utils.DecryptStringAES(HttpServerUtility.UrlTokenDecode(invitationCode)), out invitationId);

            if (invitationId == 0)
            {
                message = "Invalid Invitation Code.";
                success = false;
            }
            else
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    StageBitz.Data.Invitation inv = dataContext.Invitations.Where(i => i.InvitationId == invitationId).FirstOrDefault();
                    bool canUpdateInvitation = true;
                    message = string.Empty;

                    if (inv == null)
                    {
                        message = "Invalid invitation code.";
                        success = false;
                    }
                    else if (inv.ToUserId == userId)
                    {
                        //If the same user tries to process the same invitation code,
                        //generate messages to indicate that it cannot be processed again.

                        int pendingInvitationCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
                        if (inv.InvitationStatusCodeId == pendingInvitationCodeId)
                        {
                            message = "The invitation has already been added to your profile.";
                        }
                        else
                        {
                            message = "Invalid Invitation Code.";
                        }
                        success = false;
                    }
                    else if (inv.ToUserId.HasValue)
                    {
                        //Check if this invitation already belongs to another user
                        message = "Invalid Invitation Code.";
                        success = false;
                    }
                    else
                    {
                        int compAdmininvitationTypeCodeId = Support.GetCodeIdByCodeValue("InvitationType", "COMPANYADMIN");
                        int projTeamInvitationTypeCodeId = Support.GetCodeIdByCodeValue("InvitationType", "PROJECTTEAM");
                        int inventoryTeamInvitationTypeCodeId = Support.GetCodeIdByCodeValue("InvitationType", "INVENTORYTEAM");

                        #region Check for a simillar pending invitation for the same user

                        bool simillarInvitationFound = false;
                        int pendingInvitationCodeId = Support.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
                        Invitation existingInvitation = dataContext.Invitations.Where(i => i.InvitationId != inv.InvitationId && i.RelatedId == inv.RelatedId && i.InvitationStatusCodeId == pendingInvitationCodeId && i.ToUserId == userId).FirstOrDefault();

                        if (existingInvitation != null)
                        {
                            if (inv.InvitationTypeCodeId == compAdmininvitationTypeCodeId)
                            {
                                canUpdateInvitation = false;
                                simillarInvitationFound = true;

                                //Delete the unwanted invitation
                                dataContext.DeleteInvitation(inv.InvitationId);

                                message = "You already have an invitation for the same Company.";
                                success = false;
                            }
                            else if (inv.InvitationTypeCodeId == projTeamInvitationTypeCodeId)
                            {
                                canUpdateInvitation = false;
                                simillarInvitationFound = true;

                                //Delete the unwanted invitation
                                dataContext.DeleteInvitation(inv.InvitationId);

                                message = "You already have an invitation for the same Project.";
                                success = false;
                            }
                            else if (inv.InvitationTypeCodeId == inventoryTeamInvitationTypeCodeId)
                            {
                                canUpdateInvitation = false;
                                simillarInvitationFound = true;

                                //Delete the unwanted invitation
                                dataContext.DeleteInvitation(inv.InvitationId);

                                message = "You already have an invitation for the same Inventory.";
                                success = false;
                            }
                        }

                        #endregion Check for a simillar pending invitation for the same user

                        //If a simillar invitation was not found, check for existing user memberships.
                        if (!simillarInvitationFound)
                        {
                            #region Existing project/company user membership check

                            if (inv.InvitationTypeCodeId == compAdmininvitationTypeCodeId)
                            {
                                //If this invitation is a company admin invitation,
                                //check whether this user is already a company admin for that company.

                                if (IsCompanyAdministrator(inv.RelatedId, userId))
                                {
                                    canUpdateInvitation = false;

                                    //Delete the unwanted invitation
                                    dataContext.DeleteInvitation(inv.InvitationId);

                                    message = "You are already an administrator of the invited company.";
                                    success = false;
                                }
                            }
                            else if (inv.InvitationTypeCodeId == inventoryTeamInvitationTypeCodeId)
                            {
                                //If this invitation is a inventory team invitation,
                                //check whether this user is already a inventory team member for that inventory.

                                if (Utils.IsCompanyInventoryTeamMember(inv.RelatedId, userId))
                                {
                                    canUpdateInvitation = false;

                                    //Delete the unwanted invitation
                                    dataContext.DeleteInvitation(inv.InvitationId);

                                    message = "You are already an administrator of the invited company.";
                                    success = false;
                                }
                            }
                            else if (inv.InvitationTypeCodeId == projTeamInvitationTypeCodeId)
                            {
                                //If this invitation is a project team invitation,
                                //Check whether this user is already a project member for that project.
                                ProjectUser projUser = (from pu in dataContext.ProjectUsers
                                                        where pu.ProjectId == inv.RelatedId && pu.UserId == userId
                                                        select pu).FirstOrDefault();

                                if (projUser != null)
                                {
                                    canUpdateInvitation = false;

                                    //Delete the unwanted invitation
                                    dataContext.DeleteInvitation(inv.InvitationId);

                                    message = "You are already a team member of the invited project.";
                                    success = false;
                                }
                            }

                            #endregion Existing project/company user membership check
                        }

                        if (canUpdateInvitation)
                        {
                            inv.ToUserId = userId;
                            inv.LastUpdatedByUserId = userId;
                            inv.LastUpdatedDate = Now;

                            message = "The invitation has been added to your profile.";
                            success = true;
                        }

                        //save the updated or deleted invitation
                        dataContext.SaveChanges();
                    }
                }
            }

            return success;
        }

        #endregion Invitation Handling

        #region Common Url Generation

        /// <summary>
        /// Generates the user activation page url to include in the user activation email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns></returns>
        public static string GetUserActivationLink(string email, string passwordHash)
        {
            //Password is sent to improve security. Unless someone else can activate the account.
            return string.Format("{0}/Account/Activation.aspx?email={1}&activationkey={2}", Support.GetSystemUrl(), HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(passwordHash));
        }

        /// <summary>
        /// Generates the user primary email change link.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <param name="emailChangeRequestId">The email change request id.</param>
        /// <returns>
        /// The email change url.
        /// </returns>
        public static string GetUserPrimaryEmailChangeLink(string email, string passwordHash,
                int emailChangeRequestId)
        {
            //Password is sent to improve security. Unless someone else can activate the account.
            return string.Format("{0}/Public/EmailChange.aspx?email={1}&activationkey={2}&changerequestid={3}",
                    Support.GetSystemUrl(), HttpUtility.UrlEncode(email),
                    HttpUtility.UrlEncode(passwordHash), emailChangeRequestId);
        }

        /// <summary>
        /// Generates the Admin portal user details page url.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public static string GetAdminPortalUserDetailsLink(int userId)
        {
            return string.Format("{0}/User/UserDetails.aspx?ViewUserId={1}", GetSystemValue("SBAdminWebURL"), userId);
        }

        /// <summary>
        /// Generates the Admin portal Company details page url.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static string GetAdminPortalCompanyDetailsLink(int companyId)
        {
            return string.Format("{0}/Company/CompanyDetails.aspx?CompanyID={1}", GetSystemValue("SBAdminWebURL"), companyId);
        }

        /// <summary>
        /// Get image media file preview Url.
        /// </summary>
        /// <param name="documentMediaId">The documentMediaId.</param>
        /// <param name="previewSize">Size of the preview.</param>
        /// <returns>
        /// The image url.
        /// </returns>
        public static string GetImageFilePreviewUrl(int documentMediaId, int previewSize)
        {
            return string.Format("~/Common/GetMedia.aspx?documentMediaId={0}&size={1}", documentMediaId, previewSize);
        }

        /// <summary>
        /// Get image media file preview Url.
        /// </summary>
        /// <param name="documentMediaId">The documentMediaId.</param>
        /// <returns>
        /// The image url.
        /// </returns>
        public static string GetImageFileDownloadUrl(int documentMediaId)
        {
            return string.Format("~/Common/GetMedia.aspx?documentMediaId={0}&download=1", documentMediaId);
        }

        /// <summary>
        /// Get image media file thumbnail Url.
        /// </summary>
        /// <param name="documentMediaId">The documentMediaId.</param>
        /// <param name="isThumb">Is thumb image.</param>
        /// <returns>
        /// The image url.
        /// </returns>
        public static string GetImageFileThumbUrl(int documentMediaId, bool isThumb)
        {
            return string.Format("~/Common/GetMedia.aspx?documentMediaId={0}&thumb={1}", documentMediaId, isThumb ? 1 : 0);
        }

        #endregion Common Url Generation

        #region EMAIL

        /// <summary>
        /// Determines whether email is valid for the specified email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            //THIS IS CASE SENSITIVE IF U DONT WONT CASE SENSITIVE PLEASE USE FOLLOWING CODE
            // email = email.ToLower();

            string pattern = @"^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$";
            // ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
            Regex check = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            //boolean variable to return to calling method
            bool valid = false;
            //make sure an email address was provided
            if (string.IsNullOrEmpty(email))
            {
                valid = false;
            }
            else
            {
                //use IsMatch to validate the address
                valid = check.IsMatch(email);
            }

            //return the value to the calling method
            return valid;
        }

        #endregion EMAIL

        #region Server Agent

        /// <summary>
        /// Checks whether the considering date has passed based on the daily agent last execution date and time
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static bool HasConsideringDayEnded(DateTime date)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int dailyTaskTypeCodeId = Support.GetCodeIdByCodeValue("SystemTaskType", "DAILY");

                SystemTask task = dataContext.SystemTasks.Where(t => t.TaskTypeCodeId == dailyTaskTypeCodeId).FirstOrDefault();
                //If Considering date is same as the specified date and if the last run date is less than the considering date then the day is ended.
                return (task.LastRunDate.Value.Date == date.Date && task.LastRunDate.Value <= date);
            }
        }

        #endregion Server Agent

        #region Encode/Decode

        /// <summary>
        /// Gets to base64 string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The base64 text.
        /// </returns>
        public static string EncodeToBase64String(string text)
        {
            byte[] byt = System.Text.Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(byt);
        }

        /// <summary>
        /// Decodes the base64 string.
        /// </summary>
        /// <param name="base64">The base64.</param>
        /// <returns>
        /// The decoded value.
        /// </returns>
        public static string DecodeBase64String(string base64)
        {
            byte[] byt = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(byt);
        }

        #endregion Encode/Decode

        #region Inventory

        /// <summary>
        /// Determines whether email is sent to inventory manager for the specified company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static bool IsEmailSentToInventoryManager(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return (dataContext.InventoryManagerSentEmails.Where(imse => imse.UserId == UserID && imse.CompanyId == companyId).FirstOrDefault() != null);
            }
        }

        /// <summary>
        /// Gets the inventory navigate URL parameters.
        /// </summary>
        /// <param name="additemTypeId">The additem type identifier.</param>
        /// <param name="searchItemType">Type of the search item.</param>
        /// <param name="sharedInventoryCompanyId">The shared inventory company identifier.</param>
        /// <param name="searchLocationId">The search location identifier.</param>
        /// <param name="addLocationId">The add location identifier.</param>
        /// <returns></returns>
        public static string GetInventoryNavigateURLParams(string additemTypeId, string searchItemType, int sharedInventoryCompanyId, string searchLocationId, string addLocationId)
        {
            const char ParameterDelimiter = StageBitz.Common.Constants.GlobalConstants.parameterDelimiter;
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", additemTypeId, ParameterDelimiter,
                searchItemType, ParameterDelimiter,
                sharedInventoryCompanyId, ParameterDelimiter, searchLocationId,
                ParameterDelimiter, addLocationId);
        }

        #endregion Inventory

        #endregion Public Methods

        #region Company Inventory

        /// <summary>
        /// Determines whether item is in watch list for the specified item identifier.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static bool IsItemInWatchList(int itemId, int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                var item = (from wli in dataContext.WatchListItems
                            join wlh in dataContext.WatchListHeaders on wli.WatchListHeaderId equals wlh.WatchListHeaderId
                            where wlh.CompanyId == companyId && wlh.UserId == UserID && wli.ItemId == itemId
                            select wli).FirstOrDefault();
                return item != null;
            }
        }

        /// <summary>
        /// Adds to watch list.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        public static void AddToWatchList(int itemId, int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                var watchListHeader = (from wlh in dataContext.WatchListHeaders
                                       where wlh.UserId == UserID && wlh.CompanyId == companyId
                                       select wlh).FirstOrDefault();

                if (watchListHeader == null)
                {
                    watchListHeader = new Data.WatchListHeader();
                    watchListHeader.UserId = UserID;
                    watchListHeader.CompanyId = companyId;
                }

                //Check if the item is not already being added.
                if (!IsItemInWatchList(itemId, companyId))
                {
                    Data.WatchListItem watchListItem = new Data.WatchListItem();
                    watchListItem.Item = dataContext.Items.Where(i => i.ItemId == itemId).FirstOrDefault();
                    watchListItem.WatchListHeader = watchListHeader;
                    dataContext.WatchListItems.AddObject(watchListItem);
                    dataContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Determines whether this user can access inventory the specified company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public static bool CanAccessInventory(int companyId)
        {
            return Utils.CanAccessInventory(companyId, UserID);
        }

        /// <summary>
        /// Gets the full path of selected node of rad tree view.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// The full path
        /// </returns>
        public static string GetFullPath(RadTreeNode node)
        {
            string text = node.Text;
            RadTreeNode currentObject = node.ParentNode;
            while (currentObject != null)
            {
                if (currentObject.Parent != null)
                {
                    text = string.Concat(currentObject.Text, " > ", text);
                }

                currentObject = currentObject.ParentNode;
            }

            return text;
        }

        #endregion Company Inventory
    }
}