using DotNetOpenAuth.OAuth2;
using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Services;
using Google.Apis.Util;
using StageBitz.Common.Exceptions;
using StageBitz.Data.DataTypes.Analytics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace StageBitz.Common.Google
{
    /// <summary>
    /// Google Analytics manager.
    /// </summary>
    public class AnalyticsManager
    {
        #region Properties

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        private string Scope
        {
            get
            {
                return AnalyticsService.Scopes.AnalyticsReadonly.GetStringValue();
            }
        }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        private string ClientId
        {
            get
            {
                return ConfigurationManager.AppSettings["Google.ClientId"];
            }
        }

        /// <summary>
        /// Gets the profile identifier.
        /// </summary>
        /// <value>
        /// The profile identifier.
        /// </value>
        private string ProfileId
        {
            get
            {
                return ConfigurationManager.AppSettings["Google.ProfileId"];
            }
        }

        /// <summary>
        /// Gets the key file.
        /// </summary>
        /// <value>
        /// The key file.
        /// </value>
        private string KeyFile
        {
            get
            {
                return ConfigurationManager.AppSettings["Google.KeyFile"];
            }
        }

        /// <summary>
        /// Gets the key pass.
        /// </summary>
        /// <value>
        /// The key pass.
        /// </value>
        private string KeyPass
        {
            get
            {
                return ConfigurationManager.AppSettings["Google.KeyPass"];
            }
        }

        /// <summary>
        /// Gets or sets the google analytics service.
        /// </summary>
        /// <value>
        /// The google analytics service.
        /// </value>
        private AnalyticsService GoogleAnalyticsService
        {
            get;
            set;
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsManager"/> class.
        /// </summary>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while initializing google analytics.</exception>
        public AnalyticsManager()
        {
            try
            {
                AuthorizationServerDescription desc = GoogleAuthenticationServer.Description;
                X509Certificate2 key = new X509Certificate2(KeyFile, KeyPass, X509KeyStorageFlags.Exportable);
                AssertionFlowClient client = new AssertionFlowClient(desc, key) { ServiceAccountId = ClientId, Scope = Scope };
                OAuth2Authenticator<AssertionFlowClient> auth =
                    new OAuth2Authenticator<AssertionFlowClient>(client, AssertionFlowClient.GetState);
                GoogleAnalyticsService = new AnalyticsService(new BaseClientService.Initializer() { Authenticator = auth });
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while initializing google analytics.", ex);
            }
        }

        #endregion Constructor

        #region InventoryPageViews

        /// <summary>
        /// Gets the inventory page views.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public InventoryPageViews GetInventoryPageViews(int companyId, DateTime startDate, DateTime endDate)
        {
            InventoryPageViews inventoryPageViews = new InventoryPageViews();
            inventoryPageViews.PageViews = GetInventoryPageViewCount(companyId, startDate, endDate);
            inventoryPageViews.UserViews = GetInventoryUserViewCount(companyId, startDate, endDate);
            return inventoryPageViews;
        }

        /// <summary>
        /// Gets the inventory user view count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetInventoryUserViewCount(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue3";
                request.Filters = string.Format("ga:customVarValue1==Inventory;ga:customVarValue2=={0}", companyId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        /// <summary>
        /// Gets the inventory page view count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetInventoryPageViewCount(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");
                int pageViews = 0;

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1";
                request.Filters = string.Format("ga:customVarValue1==Inventory;ga:customVarValue2=={0}", companyId);
                request.MaxResults = 1;
                GaData data = request.Fetch();

                if (data != null && data.Rows != null && data.Rows[0] != null)
                {
                    int.TryParse(data.Rows[0][1], out pageViews);
                }

                return pageViews;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        #endregion InventoryPageViews

        #region ProjectTeamActivitySummary

        /// <summary>
        /// Gets the project team activity summary.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public ProjectTeamActivitySummary GetProjectTeamActivitySummary(int companyId, DateTime startDate, DateTime endDate)
        {
            ProjectTeamActivitySummary projectTeamActivitySummary = new ProjectTeamActivitySummary();
            projectTeamActivitySummary.DaysCount = GetAllProjectTeamActivityDaysCount(companyId, startDate, endDate);
            projectTeamActivitySummary.ProjectCount = GetAllProjectTeamActivityProjectCount(companyId, startDate, endDate);
            projectTeamActivitySummary.UserCount = GetAllProjectTeamActivityUserCount(companyId, startDate, endDate);
            return projectTeamActivitySummary;
        }

        /// <summary>
        /// Gets all project team activity days count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetAllProjectTeamActivityDaysCount(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:date";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue2=={0}", companyId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        /// <summary>
        /// Gets all project team activity project count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetAllProjectTeamActivityProjectCount(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue4";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue2=={0}", companyId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        /// <summary>
        /// Gets all project team activity user count.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetAllProjectTeamActivityUserCount(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue3";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue2=={0}", companyId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        #endregion ProjectTeamActivitySummary

        #region ProjectTeamActivities

        /// <summary>
        /// Gets the project team activities.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        public List<ProjectTeamActivity> GetProjectTeamActivities(int companyId, DateTime startDate, DateTime endDate,
            int pageSize, int pageIndex, string sortBy, bool isAscending, out int totalRecords)
        {
            try
            {
                int startIndex = (pageSize * pageIndex) + 1;
                totalRecords = 0;
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:timeOnSite");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue2, ga:customVarValue3, ga:customVarValue4, ga:date";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue2=={0}", companyId);
                request.MaxResults = pageSize;
                request.StartIndex = startIndex;
                request.Sort = GetSortingValue(sortBy, isAscending);
                GaData data = request.Fetch();

                List<ProjectTeamActivity> activities = new List<ProjectTeamActivity>();
                if (data != null && data.Rows != null)
                {
                    activities = (from pa in data.Rows
                                  select new
                                  {
                                      Date = DateTime.ParseExact(pa[4], "yyyyMMdd", CultureInfo.InvariantCulture),
                                      ProjectId = int.Parse(pa[3]),
                                      TimeSpan = TimeSpan.FromSeconds(double.Parse(pa[5])),
                                      CompanyId = int.Parse(pa[1]),
                                      UserId = int.Parse(pa[2])
                                  }
                                      into tempActivities
                                      select new ProjectTeamActivity
                                      {
                                          Date = tempActivities.Date,
                                          ProjectId = tempActivities.ProjectId,
                                          SessionTotal = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                                            tempActivities.TimeSpan.Hours,
                                                                            tempActivities.TimeSpan.Minutes,
                                                                            tempActivities.TimeSpan.Seconds),
                                          UserId = tempActivities.UserId
                                      }).ToList();

                    totalRecords = (int)data.TotalResults;
                }

                return activities;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        #endregion ProjectTeamActivities

        #region UserActivities

        /// <summary>
        /// Gets the user activities.
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
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        public List<UserActivity> GetUserActivities(int userId, DateTime startDate, DateTime endDate, int pageSize, int pageIndex, string sortField, bool isAscending, out int totalRecords)
        {
            try
            {
                int startIndex = (pageSize * pageIndex) + 1;
                totalRecords = 0;
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:timeOnSite");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue2, ga:customVarValue3, ga:customVarValue4, ga:date";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue3=={0}", userId);
                request.MaxResults = pageSize;
                request.StartIndex = startIndex;
                request.Sort = GetSortingValue(sortField, isAscending);
                GaData data = request.Fetch();

                List<UserActivity> activities = new List<UserActivity>();
                if (data != null && data.Rows != null)
                {
                    activities = (from pa in data.Rows
                                  select new
                                  {
                                      Date = DateTime.ParseExact(pa[4], "yyyyMMdd", CultureInfo.InvariantCulture),
                                      ProjectId = int.Parse(pa[3]),
                                      TimeSpan = TimeSpan.FromSeconds(double.Parse(pa[5])),
                                      CompanyId = int.Parse(pa[1]),
                                      UserId = int.Parse(pa[2])
                                  }
                                      into tempActivities
                                      select new UserActivity
                                      {
                                          Date = tempActivities.Date,
                                          ProjectId = tempActivities.ProjectId,
                                          CompanyId = tempActivities.CompanyId,
                                          SessionTotal = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                                            tempActivities.TimeSpan.Hours,
                                                                            tempActivities.TimeSpan.Minutes,
                                                                            tempActivities.TimeSpan.Seconds)
                                      }).ToList();

                    totalRecords = (int)data.TotalResults;
                }

                return activities;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        #endregion UserActivities

        #region Common Methods

        /// <summary>
        /// Gets the sorting value.
        /// </summary>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <returns></returns>
        private string GetSortingValue(string sortBy, bool isAscending)
        {
            string sortDim = "ga:date";
            string sortDirection = isAscending ? string.Empty : "-";
            switch (sortBy)
            {
                case "Date":
                    sortDim = "ga:date";
                    break;

                case "SessionTotal":
                    sortDim = "ga:timeOnSite";
                    break;
            }

            return string.Concat(sortDirection, sortDim);
        }

        #endregion Common Methods

        #region UserActivitySummary

        /// <summary>
        /// Gets the user activity summary.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public UserActivitySummary GetUserActivitySummary(int userId, DateTime startDate, DateTime endDate)
        {
            UserActivitySummary userActivitySummary = new UserActivitySummary();
            userActivitySummary.DaysCount = GetUserActivityDaysCount(userId, startDate, endDate);
            userActivitySummary.ProjectCount = GetUserActivityProjectCount(userId, startDate, endDate);
            userActivitySummary.CompanyCount = GetUserActivityCompanyCount(userId, startDate, endDate);
            return userActivitySummary;
        }

        /// <summary>
        /// Gets the company count user activities.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetUserActivityCompanyCount(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue2";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue3=={0}", userId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        /// <summary>
        /// Gets the project count of user activities.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetUserActivityProjectCount(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:customVarValue4";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue3=={0}", userId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        /// <summary>
        /// Gets the user activity days count.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="StageBitz.Common.Exceptions.StageBitzException">0;Error occurred while connecting to google analytics server.</exception>
        private int GetUserActivityDaysCount(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string profileId = string.Concat("ga:", ProfileId);
                string startDateString = startDate.ToString("yyyy-MM-dd");
                string endDateString = endDate.ToString("yyyy-MM-dd");

                DataResource.GaResource.GetRequest request = GoogleAnalyticsService.Data.Ga.Get(profileId, startDateString, endDateString, "ga:visits");

                request.Dimensions = "ga:customVarValue1, ga:date";
                request.Filters = string.Format("ga:customVarValue1==Project;ga:customVarValue3=={0}", userId);
                request.MaxResults = 1;
                GaData data = request.Fetch();
                return data.Rows != null ? (int)data.TotalResults : 0;
            }
            catch (Exception ex)
            {
                throw new StageBitzException(ExceptionOrigin.WebAnalytics, 0, "Error occurred while connecting to google analytics server.", ex);
            }
        }

        #endregion UserActivitySummary
    }
}