using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Project;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace StageBitz.Logic.Support
{
    /// <summary>
    /// Handler class for send email notifications.
    /// </summary>
    public static class UserEmailNotificationHandler
    {
        /// <summary>
        /// The project team email text
        /// </summary>
        private const string ProjectTeamEmailText = "Project Team";

        /// <summary>
        /// The item briefs email text
        /// </summary>
        private const string ItemBriefsEmailText = "Item Briefs";

        /// <summary>
        /// The schedule email text
        /// </summary>
        private const string ScheduleEmailText = "Schedule";

        /// <summary>
        /// The project email text
        /// </summary>
        private const string ProjectEmailText = "Project Details";

        /// <summary>
        /// The task email text
        /// </summary>
        private const string TaskEmailText = "Task";

        /// <summary>
        /// The item list email text
        /// </summary>
        private const string ItemListEmailText = "Item List";

        /// <summary>
        /// The project notification details list
        /// </summary>
        private static List<ProjectNotificationDetails> ProjectNotificationDetailsList = new List<ProjectNotificationDetails>();

        /// <summary>
        /// The daily email notification code id
        /// </summary>
        private static int DailyEmailNotificationCodeId;

        /// <summary>
        /// The weekly email notification code id
        /// </summary>
        private static int WeeklyEmailNotificationCodeId;

        /// <summary>
        /// The not at all email notification code id
        /// </summary>
        private static int NoEmailNotificationCodeId;

        /// <summary>
        /// Sends the email notifications.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        public static void SendEmailNotifications(DateTime dateToConsider)
        {
            // Handles the concurrent issues. (this class should not access concurrently)
            if (ProjectNotificationDetailsList.Count == 0)
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    // Set code values.
                    DailyEmailNotificationCodeId = Utils.GetCodeIdByCodeValue("UserEmailNotificationType", "DAILY");
                    WeeklyEmailNotificationCodeId = Utils.GetCodeIdByCodeValue("UserEmailNotificationType", "WEEKLY");
                    NoEmailNotificationCodeId = Utils.GetCodeIdByCodeValue("UserEmailNotificationType", "NOTATALL");

                    bool isWeeklyEmailSendDate = false;

                    NotificationBL notificationBL = new NotificationBL(dataContext);
                    ProjectBL projectBL = new ProjectBL(dataContext);

                    DateTime endDate = dateToConsider;
                    int dateDiff = 1;

                    // If weekly email day set date diff as 7 days.
                    if (dateToConsider.DayOfWeek == (DayOfWeek)Enum.Parse(typeof(DayOfWeek), Utils.GetSystemValue("EmailNotificationDayOfWeek")))
                    {
                        dateDiff = 7;
                        isWeeklyEmailSendDate = true;
                    }

                    // Get project notifications static list.
                    ProjectNotificationDetailsList = notificationBL.GetUserEmailNotifications(dateToConsider, dateDiff);

                    // Get all project ids with notifications.
                    List<int> projectIds = ProjectNotificationDetailsList.Select(pnd => pnd.ProjectId).Distinct().ToList<int>();

                    // Get all user ids need to send email notifications.
                    List<int> allUsers = projectBL.GetAllProjectUserAndCompanyUserIds(projectIds);

                    // Send emails for all users.
                    foreach (int userId in allUsers)
                    {
                        // Get project user infos
                        List<ProjectUserInfo> projectUserInfoList = projectBL.GetProjectUserInfo(userId);

                        // Get company admin user infos
                        List<ProjectUserInfo> companyAdminProjectUserInfoList = projectBL.GetCompanyAdminProjectUserInfo(projectIds, userId);

                        // remove duplicate project user infos from company admin project list.
                        companyAdminProjectUserInfoList.RemoveAll(capui => projectUserInfoList.Exists(pui => (capui.ProjectId == pui.ProjectId && capui.UserId == pui.UserId)));

                        if (projectUserInfoList.Count > 0 || companyAdminProjectUserInfoList.Count > 0)
                        {
                            ProjectUserInfo user = null;

                            // Get first one.
                            if (projectUserInfoList.Count > 0)
                            {
                                user = projectUserInfoList.FirstOrDefault();
                            }
                            else if (companyAdminProjectUserInfoList.Count > 0)
                            {
                                user = companyAdminProjectUserInfoList.FirstOrDefault();
                            }

                            if (user != null)
                            {
                                // Filter project user infos.
                                List<ProjectUserInfo> projectUserInfos = GetFilteredProjectUserInfoForUser(user.UserId, user.ProjectEmailNotificationCodeId, projectUserInfoList, isWeeklyEmailSendDate, true);

                                // Filter company admin user infos.
                                List<ProjectUserInfo> companyAdminProjectUserInfos = GetFilteredProjectUserInfoForUser(user.UserId, user.CompanyEmailNotificationCodeId, companyAdminProjectUserInfoList, isWeeklyEmailSendDate, false);

                                if (projectUserInfos.Count > 0 || companyAdminProjectUserInfos.Count > 0)
                                {
                                    // Gets the email content.
                                    string emailText = GetEmailText(user, projectUserInfos, companyAdminProjectUserInfos);

                                    // Send the email.
                                    EmailSender.SendNotificationEmail(user.UserEmail, user.UserName, emailText);
                                }
                            }
                        }
                    }
                }

                // Clear static project notification details list.
                ProjectNotificationDetailsList.Clear();
            }
        }

        /// <summary>
        /// Gets the project user info for user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="emailNotificationCodeId">The email notification code id.</param>
        /// <param name="projectUserInfoList">The project user info list.</param>
        /// <param name="isWeeklyEmailSendDate">if set to <c>true</c> [is weekly email send date].</param>
        /// <returns></returns>
        private static List<ProjectUserInfo> GetFilteredProjectUserInfoForUser(int userId, int emailNotificationCodeId, List<ProjectUserInfo> projectUserInfoList, bool isWeeklyEmailSendDate, bool getFullList)
        {
            List<ProjectUserInfo> projectUserInfoListReturn = new List<ProjectUserInfo>();
            List<ProjectUserInfo> projectUserInfoListFiltered = new List<ProjectUserInfo>();
            if (emailNotificationCodeId == DailyEmailNotificationCodeId)
            {
                projectUserInfoListFiltered = (from pui in projectUserInfoList
                                               join pnd in ProjectNotificationDetailsList on pui.ProjectId equals pnd.ProjectId
                                               where (pnd.ProjectTeamUpdatesStatus.HasUpdatesForGivenDate && (pnd.ProjectTeamUpdatesStatus.UserList.Count() > 1 || (pnd.ProjectTeamUpdatesStatus.UserList.Count() == 1 && !pnd.ProjectTeamUpdatesStatus.UserList.Contains(userId))))
                                                    || (pnd.ScheduleUpdateStatus.HasUpdatesForGivenDate && (pnd.ScheduleUpdateStatus.UserList.Count() > 1 || (pnd.ScheduleUpdateStatus.UserList.Count() == 1 && !pnd.ScheduleUpdateStatus.UserList.Contains(userId))))
                                                    || (pnd.ItemBriefUpdateStatus.HasUpdatesForGivenDate && (pnd.ItemBriefUpdateStatus.UserList.Count() > 1 || (pnd.ItemBriefUpdateStatus.UserList.Count() == 1 && !pnd.ItemBriefUpdateStatus.UserList.Contains(userId))))
                                                    || (pnd.ProjectUpdateStatus.HasUpdatesForGivenDate && (pnd.ProjectUpdateStatus.UserList.Count() > 1 || (pnd.ProjectUpdateStatus.UserList.Count() == 1 && !pnd.ProjectUpdateStatus.UserList.Contains(userId))))
                                                    || (pnd.TaskUpdateStatus.HasUpdatesForGivenDate && (pnd.TaskUpdateStatus.UserList.Count() > 1 || (pnd.TaskUpdateStatus.UserList.Count() == 1 && !pnd.TaskUpdateStatus.UserList.Contains(userId))))
                                                    || (pnd.ItemListUpdateStatus.HasUpdatesForGivenDate && (pnd.ItemListUpdateStatus.UserList.Count() > 1 || (pnd.ItemListUpdateStatus.UserList.Count() == 1 && !pnd.ItemListUpdateStatus.UserList.Contains(userId))))
                                               select pui).ToList();
            }
            else if (isWeeklyEmailSendDate && emailNotificationCodeId == WeeklyEmailNotificationCodeId)
            {
                projectUserInfoListFiltered = (from pui in projectUserInfoList
                                               join pnd in ProjectNotificationDetailsList on pui.ProjectId equals pnd.ProjectId
                                               where (pnd.ProjectTeamUpdatesStatus.HasUpdatesForFullDuration && (pnd.ProjectTeamUpdatesStatus.UserList.Count() > 1 || (pnd.ProjectTeamUpdatesStatus.UserList.Count() == 1 && !pnd.ProjectTeamUpdatesStatus.UserList.Contains(userId))))
                                                  || (pnd.ScheduleUpdateStatus.HasUpdatesForFullDuration && (pnd.ScheduleUpdateStatus.UserList.Count() > 1 || (pnd.ScheduleUpdateStatus.UserList.Count() == 1 && !pnd.ScheduleUpdateStatus.UserList.Contains(userId))))
                                                  || (pnd.ItemBriefUpdateStatus.HasUpdatesForFullDuration && (pnd.ItemBriefUpdateStatus.UserList.Count() > 1 || (pnd.ItemBriefUpdateStatus.UserList.Count() == 1 && !pnd.ItemBriefUpdateStatus.UserList.Contains(userId))))
                                                  || (pnd.ProjectUpdateStatus.HasUpdatesForFullDuration && (pnd.ProjectUpdateStatus.UserList.Count() > 1 || (pnd.ProjectUpdateStatus.UserList.Count() == 1 && !pnd.ProjectUpdateStatus.UserList.Contains(userId))))
                                                  || (pnd.TaskUpdateStatus.HasUpdatesForFullDuration && (pnd.TaskUpdateStatus.UserList.Count() > 1 || (pnd.TaskUpdateStatus.UserList.Count() == 1 && !pnd.TaskUpdateStatus.UserList.Contains(userId))))
                                                  || (pnd.ItemListUpdateStatus.HasUpdatesForFullDuration && (pnd.ItemListUpdateStatus.UserList.Count() > 1 || (pnd.ItemListUpdateStatus.UserList.Count() == 1 && !pnd.ItemListUpdateStatus.UserList.Contains(userId))))
                                               select pui).ToList();
            }

            if (projectUserInfoListFiltered.Count > 0)
            {
                projectUserInfoListReturn = getFullList ? projectUserInfoList : projectUserInfoListFiltered;
            }

            return projectUserInfoListReturn;
        }

        /// <summary>
        /// Gets the email text.
        /// </summary>
        /// <param name="userInfo">The user info.</param>
        /// <param name="projectUserInfos">The project user infos.</param>
        /// <param name="companyAdminProjectUserInfos">The company admin project user infos.</param>
        /// <returns></returns>
        private static string GetEmailText(ProjectUserInfo userInfo, List<ProjectUserInfo> projectUserInfos, List<ProjectUserInfo> companyAdminProjectUserInfos)
        {
            StringBuilder sbEmailText = new StringBuilder();
            using (StringWriter swEmailText = new StringWriter(sbEmailText, CultureInfo.InvariantCulture))
            {
                // Html text writer for generate email html content.
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(swEmailText))
                {
                    string projectNotificationDuration = string.Empty;

                    htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "700px");
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                    if (projectUserInfos.Count > 0)
                    {
                        if (userInfo.ProjectEmailNotificationCodeId == DailyEmailNotificationCodeId)
                        {
                            projectNotificationDuration = "yesterday";
                        }
                        else if (userInfo.ProjectEmailNotificationCodeId == WeeklyEmailNotificationCodeId)
                        {
                            projectNotificationDuration = "in the last 7 days";
                        }

                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                        htmlWriter.Write(string.Format("This is what’s been happening on all your StageBitz projects since {0}.", projectNotificationDuration));
                        htmlWriter.RenderEndTag();

                        // Sort by projects that has notifications and project name.
                        Dictionary<ProjectUserInfo, ProjectNotificationDetails> sortedProjectUserInfos = (from pui in projectUserInfos
                                                                                                          from pnd in
                                                                                                              (from pnd in ProjectNotificationDetailsList
                                                                                                               where pui.ProjectId == pnd.ProjectId
                                                                                                               select pnd).DefaultIfEmpty().Take(1)
                                                                                                          orderby pnd == null, pui.ProjectName
                                                                                                          select new
                                                                                                          {
                                                                                                              ProjectUserInfo = pui,
                                                                                                              ProjectNotificationDetails = pnd
                                                                                                          }).ToDictionary(d => d.ProjectUserInfo, d => d.ProjectNotificationDetails);

                        // Loop project user info list.
                        foreach (ProjectUserInfo projectUserInfo in sortedProjectUserInfos.Keys)
                        {
                            ProjectNotificationDetails projectNotificationDetails = sortedProjectUserInfos[projectUserInfo];

                            // Add P tag with padding
                            htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "15px");
                            htmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

                            // Bold the project name
                            htmlWriter.RenderBeginTag(HtmlTextWriterTag.B);
                            htmlWriter.Write(projectUserInfo.ProjectName);
                            htmlWriter.RenderEndTag();

                            if (projectNotificationDetails != null && ValidateNotificationDetails(projectNotificationDetails, projectUserInfo))
                            {
                                htmlWriter.Write(string.Format(", by {0}, has had updates to the:", projectUserInfo.CompanyName));
                                AddNotificationListForProject(htmlWriter, projectUserInfo, projectNotificationDetails, true);
                            }
                            else
                            {
                                htmlWriter.Write(string.Format(", by {0}, has had no updates.", projectUserInfo.CompanyName));
                            }

                            htmlWriter.RenderEndTag(); // end of P
                        }
                    }

                    if (companyAdminProjectUserInfos.Count > 0)
                    {
                        if (userInfo.CompanyEmailNotificationCodeId == DailyEmailNotificationCodeId)
                        {
                            projectNotificationDuration = "yesterday";
                        }
                        else if (userInfo.CompanyEmailNotificationCodeId == WeeklyEmailNotificationCodeId)
                        {
                            projectNotificationDuration = "in the last 7 days";
                        }

                        string companyText = companyAdminProjectUserInfos.Count > 1 ? "Companies" : "Company’s";
                        string projectUserAndCompanyUserTextOther = projectUserInfos.Count > 0 ? "other " : string.Empty;
                        string projectUserAndCompanyUserTextAlso = projectUserInfos.Count > 0 ? "also " : string.Empty;

                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

                        htmlWriter.Write(string.Format("You are {0}a Company Administrator.  Here’s what’s been happening on your {1} {2}Projects since {3}.", projectUserAndCompanyUserTextAlso, companyText, projectUserAndCompanyUserTextOther, projectNotificationDuration));
                        htmlWriter.RenderEndTag();

                        // Group by company id and sort by project name.
                        Dictionary<int, List<ProjectUserInfo>> groupedCompanyAdminProjectUserInfos = companyAdminProjectUserInfos.OrderBy(capui => capui.CompanyName).
                                GroupBy(capui => capui.CompanyId).ToDictionary(g => g.Key, g => g.OrderBy(pui => pui.ProjectName).ToList());

                        foreach (int companyId in groupedCompanyAdminProjectUserInfos.Keys)
                        {
                            List<ProjectUserInfo> sortedCompanyAdminProjectUserInfos = groupedCompanyAdminProjectUserInfos[companyId];
                            string companyName = sortedCompanyAdminProjectUserInfos[0].CompanyName;

                            // Add P tag with padding
                            htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "15px");
                            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                            htmlWriter.RenderBeginTag(HtmlTextWriterTag.B);
                            htmlWriter.Write(string.Concat(companyName, ":"));
                            htmlWriter.RenderEndTag();

                            foreach (ProjectUserInfo projectUserInfo in sortedCompanyAdminProjectUserInfos)
                            {
                                ProjectNotificationDetails projectNotificationDetails = ProjectNotificationDetailsList.Where(pnd => pnd.ProjectId == projectUserInfo.ProjectId).FirstOrDefault();
                                if (projectNotificationDetails != null)
                                {
                                    // Add P tag with padding
                                    htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "15px");
                                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

                                    // Bold the project name
                                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.B);
                                    htmlWriter.Write(projectUserInfo.ProjectName);
                                    htmlWriter.RenderEndTag();

                                    htmlWriter.Write(" has had updates to the:");

                                    AddNotificationListForProject(htmlWriter, projectUserInfo, projectNotificationDetails, false);

                                    htmlWriter.RenderEndTag(); // end of P
                                }
                            }

                            htmlWriter.RenderEndTag(); // end of P
                        }
                    }

                    htmlWriter.RenderEndTag();
                    return sbEmailText.ToString();
                }
            }
        }

        /// <summary>
        /// Validates the notification details.
        /// </summary>
        /// <param name="projectNotificationDetails">The project notification details.</param>
        /// <param name="projectUserInfo">The project user info.</param>
        /// <returns></returns>
        private static bool ValidateNotificationDetails(ProjectNotificationDetails projectNotificationDetails, ProjectUserInfo projectUserInfo)
        {
            bool isValid = true;
            if (projectUserInfo.ProjectEmailNotificationCodeId == DailyEmailNotificationCodeId)
            {
                isValid = HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, true) ||
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, true) ||
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, true) ||
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, true) ||
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, true) ||
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, true);
            }
            else if (projectUserInfo.ProjectEmailNotificationCodeId == WeeklyEmailNotificationCodeId)
            {
                isValid = HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, false) ||
                        HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, false) ||
                        HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, false) ||
                        HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, false) ||
                        HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, false) ||
                        HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, false);
            }

            return isValid;
        }

        /// <summary>
        /// Adds the notification type list.
        /// </summary>
        /// <param name="hasProjectTeamUpdates">if set to <c>true</c> [has project team updates].</param>
        /// <param name="hasItemBriefUpdates">if set to <c>true</c> [has item brief updates].</param>
        /// <param name="hasScheduleUpdates">if set to <c>true</c> [has schedule updates].</param>
        /// <param name="htmlWriter">The HTML writer.</param>
        private static void AddNotificationTypeList(bool hasProjectTeamUpdates, bool hasItemBriefUpdates, bool hasScheduleUpdates,
                bool hasProjectUpdates, bool hasTaskUpdates, bool hasItemListUpdates, HtmlTextWriter htmlWriter)
        {
            if (hasProjectTeamUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(ProjectTeamEmailText);
                htmlWriter.RenderEndTag();
            }

            if (hasItemBriefUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(ItemBriefsEmailText);
                htmlWriter.RenderEndTag();
            }

            if (hasScheduleUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(ScheduleEmailText);
                htmlWriter.RenderEndTag();
            }

            if (hasProjectUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(ProjectEmailText);
                htmlWriter.RenderEndTag();
            }

            if (hasTaskUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(TaskEmailText);
                htmlWriter.RenderEndTag();
            }

            if (hasItemListUpdates)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Li);
                htmlWriter.Write(ItemListEmailText);
                htmlWriter.RenderEndTag();
            }
        }

        /// <summary>
        /// Adds the notification list for project.
        /// </summary>
        /// <param name="htmlWriter">The HTML writer.</param>
        /// <param name="projectUserInfo">The project user info.</param>
        /// <param name="projectNotificationDetails">The project notification details.</param>
        private static void AddNotificationListForProject(HtmlTextWriter htmlWriter, ProjectUserInfo projectUserInfo, ProjectNotificationDetails projectNotificationDetails, bool isProjectNotification)
        {
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Ul);

            // Add notification details
            if (isProjectNotification)
            {
                if (projectUserInfo.ProjectEmailNotificationCodeId == DailyEmailNotificationCodeId)
                {
                    AddNotificationTypeList(HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, true),
                            htmlWriter);
                }
                else if (projectUserInfo.ProjectEmailNotificationCodeId == WeeklyEmailNotificationCodeId)
                {
                    AddNotificationTypeList(HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, false),
                            htmlWriter);
                }
            }
            else
            {
                if (projectUserInfo.CompanyEmailNotificationCodeId == DailyEmailNotificationCodeId)
                {
                    AddNotificationTypeList(HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, true),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, true),
                            htmlWriter);
                }
                else if (projectUserInfo.CompanyEmailNotificationCodeId == WeeklyEmailNotificationCodeId)
                {
                    AddNotificationTypeList(HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectTeamUpdatesStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemBriefUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ScheduleUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ProjectUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.TaskUpdateStatus, false),
                            HasNotifications(projectUserInfo.UserId, projectNotificationDetails.ItemListUpdateStatus, false),
                            htmlWriter);
                }
            }

            htmlWriter.RenderEndTag(); // end of UL

            // Add Link to notification page
            htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
            htmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

            string url = string.Format("{0}/Project/ProjectNotifications.aspx?projectid={1}", Utils.GetSystemValue("SBUserWebURL"), projectUserInfo.ProjectId);
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, url);
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
            htmlWriter.Write("View Updates Report");
            htmlWriter.RenderEndTag();

            htmlWriter.RenderEndTag();
        }

        /// <summary>
        /// Determines whether the specified user has notifications.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updateStatus">The update status.</param>
        /// <param name="isDaily">if set to <c>true</c> [is daily].</param>
        /// <returns></returns>
        private static bool HasNotifications(int userId, UpdateStatus updateStatus, bool isDaily)
        {
            bool hasNotifications = false;
            if (isDaily)
            {
                hasNotifications = updateStatus.HasUpdatesForGivenDate && (updateStatus.UserList.Count() > 1 || (updateStatus.UserList.Count() == 1 && !updateStatus.UserList.Contains(userId)));
            }
            else
            {
                hasNotifications = updateStatus.HasUpdatesForFullDuration && (updateStatus.UserList.Count() > 1 || (updateStatus.UserList.Count() == 1 && !updateStatus.UserList.Contains(userId)));
            }

            return hasNotifications;
        }
    }
}