using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Common.Exceptions;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;

namespace StageBitz.UserWeb.Project
{
    /// <summary>
    /// Web page for project notifications.
    /// </summary>
    public partial class ProjectNotifications : PageBase
    {
        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectID
        {
            get
            {
                if (ViewState["ProjectID"] == null)
                {
                    ViewState["ProjectID"] = 0;
                }
                return (int)ViewState["ProjectID"];
            }
            private set
            {
                ViewState["ProjectID"] = value;
            }
        }

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            private set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last notification identifier.
        /// </summary>
        /// <value>
        /// The last notification identifier.
        /// </value>
        private int LastNotificationId
        {
            get
            {
                if (ViewState["LastNotificationId"] == null)
                {
                    ViewState["LastNotificationId"] = 0;
                }
                return (int)ViewState["LastNotificationId"];
            }
            set
            {
                ViewState["LastNotificationId"] = value;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">project not found</exception>
        /// <exception cref="System.ApplicationException">
        /// Permission denied for this project
        /// or
        /// Invalid request.
        /// </exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["projectid"] != null)
                {
                    ProjectID = Convert.ToInt32(Request.QueryString["projectid"].ToString());
                    projectWarningPopup.ProjectId = ProjectID;

                    StageBitz.Data.Project project = (from p in DataContext.Projects
                                                      where p.ProjectId == ProjectID && p.IsActive == true
                                                      select p).FirstOrDefault();
                    if (project == null)
                    {
                        throw new Exception("project not found");
                    }

                    this.CompanyId = project.CompanyId;
                    DisplayTitle = string.Format("{0}'s Updates Report", Support.TruncateString(project.ProjectName, 32));

                    if (!Support.CanAccessProject(project))
                    {
                        if (GetBL<ProjectBL>().IsProjectClosed(project.ProjectId))
                        {
                            StageBitzException.ThrowException(new StageBitzException(ExceptionOrigin.ProjectClose, project.ProjectId, "This Project Is Closed."));
                        }

                        throw new ApplicationException("Permission denied for this project ");
                    }

                    warningDisplay.ProjectID = ProjectID;
                    warningDisplay.LoadData();

                    LoadConfigurationSettings();

                    #region SET LINKS

                    lnkCompanyInventory.HRef = string.Format("~/Inventory/CompanyInventory.aspx?companyid={0}&relatedtable={1}&relatedid={2}", project.CompanyId, (int)BookingTypes.Project, ProjectID);
                    lnkBookings.HRef = string.Format("~/Project/ProjectBookingDetails.aspx?projectid={0}", ProjectID);
                    linkTaskManager.HRef = ResolveUrl(string.Format("~/ItemBrief/TaskManager.aspx?projectid={0}", ProjectID));
                    reportList.ShowReportLinks(string.Format("~/ItemBrief/ItemisedPurchaseReport.aspx?projectid={0}", ProjectID), string.Format("~/ItemBrief/BudgetSummaryReport.aspx?projectid={0}", ProjectID), ProjectID);
                    projectUpdatesLink.ProjectID = ProjectID;
                    projectUpdatesLink.LoadData(false);

                    #endregion SET LINKS

                    LoadBreadCrumbs(project);
                }
                else //UNCOMMENT this
                {
                    throw new ApplicationException("Invalid request.");
                }
            }
        }

        /// <summary>
        /// Redirects the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Redirect(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the configuration settings.
        /// </summary>
        private void LoadConfigurationSettings()
        {
            //Load the dropdownlist
            StageBitz.Data.UserNotificationSetting userNotificationSetting = (from un in DataContext.UserNotificationSettings
                                                                              where un.RelatedTable == "Project" && un.RelatedId == ProjectID && un.UserID == UserID
                                                                              select un).FirstOrDefault();
            if (userNotificationSetting != null)
            {
                chkIncludeMyNotifications.Checked = (bool)userNotificationSetting.ShowMyNotifications;
            }
        }

        /// <summary>
        /// Loads the bread crumbs.
        /// </summary>
        /// <param name="project">The project.</param>
        private void LoadBreadCrumbs(StageBitz.Data.Project project)
        {
            BreadCrumbs bc = GetBreadCrumbsControl();

            StageBitz.Data.Company company = project.Company;

            string companyUrl = Support.IsCompanyAdministrator(company.CompanyId) ? string.Format("~/Company/CompanyDashboard.aspx?CompanyId={0}", company.CompanyId) : null;
            bc.AddLink(company.CompanyName, companyUrl);
            bc.AddLink(project.ProjectName, string.Format("~/Project/ProjectDashboard.aspx?projectid={0}", project.ProjectId));
            bc.AddLink("Updates Report", null);
            bc.LoadControl();
        }

        #endregion Private Methods

        #region Web Methods

        /// <summary>
        /// Gets the new notification count.
        /// </summary>
        /// <param name="ShowMyNotifications">The show my notifications.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string GetNewNotificationCount(string ShowMyNotifications, string projectId)
        {
            using (PageBase pb = new PageBase())
            {
                int projectID = Convert.ToInt32(projectId);

                StageBitz.Data.UserNotificationSetting userNotificationSetting = (from un in pb.DataContext.UserNotificationSettings
                                                                                  where un.RelatedTable == "Project" && un.RelatedId == projectID && un.UserID == pb.UserID
                                                                                  select un).FirstOrDefault();
                int lastNotificationID = -1;
                if (userNotificationSetting != null)
                {
                    lastNotificationID = (int)userNotificationSetting.LastNotificationId;
                }

                var notifications = (from p in pb.DataContext.Projects
                                     join n in pb.DataContext.Notifications on p.ProjectId equals n.ProjectId
                                     join u in pb.DataContext.Users on n.CreatedByUserId equals u.UserId
                                     join cModules in pb.DataContext.Codes on n.ModuleTypeCodeId equals cModules.CodeId
                                     join cOperations in pb.DataContext.Codes on n.OperationTypeCodeId equals cOperations.CodeId
                                     orderby n.NotificationId descending
                                     where p.ProjectId == projectID && n.NotificationId > lastNotificationID
                                     select new
                                     {
                                         NotificationId = n.NotificationId,
                                         CreatedbyUserID = u.UserId,
                                     });

                int count = notifications.Count();
                if (!Convert.ToBoolean(ShowMyNotifications))
                {
                    count = notifications.Where(n => n.CreatedbyUserID != pb.UserID).Count();
                }

                if (lastNotificationID == -1)
                    count = 0;
                return count.ToString();
            }
        }

        /// <summary>
        /// Gets the notification data.
        /// </summary>
        /// <param name="lastNotificationId">The last notification identifier.</param>
        /// <param name="ShowMyNotifications">The show my notifications.</param>
        /// <param name="GetLatestNotificationsOnly">The get latest notifications only.</param>
        /// <param name="IsScroll">The is scroll.</param>
        /// <param name="showMyNotificationsClicked">The show my notifications clicked.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        [WebMethod]
        public static List<NotificationDTD> GetNotificationData(string lastNotificationId, string ShowMyNotifications,
            string GetLatestNotificationsOnly, string IsScroll, string showMyNotificationsClicked, string projectId)
        {
            using (PageBase pb = new PageBase())
            {
                const int maximumRecordsToShow = 20;
                int lastNotificationIdValue = Convert.ToInt32(lastNotificationId);

                int projectIdValue = Convert.ToInt32(projectId);
                var notifications = (from p in pb.DataContext.Projects
                                     join n in pb.DataContext.Notifications on p.ProjectId equals n.ProjectId
                                     join u in pb.DataContext.Users on n.CreatedByUserId equals u.UserId
                                     join cModules in pb.DataContext.Codes on n.ModuleTypeCodeId equals cModules.CodeId
                                     join cOperations in pb.DataContext.Codes on n.OperationTypeCodeId equals cOperations.CodeId
                                     orderby n.NotificationId descending
                                     where p.ProjectId == projectIdValue
                                     select new
                                     {
                                         NotificationId = n.NotificationId,
                                         p.ProjectId,

                                         ProjectName = p.ProjectName,
                                         OperationType = cOperations.Value,
                                         RelatedId = n.RelatedId,
                                         Module = cModules.Value,
                                         Message = n.Message,

                                         EventDate = n.CreatedDate,
                                         CreatedbyUserID = u.UserId
                                     });

                #region Filters

                UserNotificationSetting userNotificationSetting = (from un in pb.DataContext.UserNotificationSettings
                                                                   where un.RelatedTable == "Project" && un.RelatedId == projectIdValue && un.UserID == pb.UserID
                                                                   select un).FirstOrDefault();
                //To apply styles for new notifications
                int usersLastViewdNotificationId = 0;
                if (userNotificationSetting != null)
                {
                    usersLastViewdNotificationId = (int)userNotificationSetting.LastNotificationId;
                }

                int maxNotificationID = 0;

                if (notifications.Count() > 0)
                {
                    maxNotificationID = (from n in notifications
                                         select n.NotificationId).Max();
                }

                if (!Convert.ToBoolean(ShowMyNotifications))
                {
                    notifications = (notifications.Where(n => n.CreatedbyUserID != pb.UserID));
                }

                bool onlyLatestNotifications = Convert.ToBoolean(GetLatestNotificationsOnly);
                if (onlyLatestNotifications)
                {
                    if (userNotificationSetting != null)
                    {
                        lastNotificationIdValue = (int)userNotificationSetting.LastNotificationId;
                    }
                    notifications = (notifications.Where(n => n.NotificationId > lastNotificationIdValue));
                }
                else
                {
                    if (lastNotificationIdValue > 0)
                        notifications = (notifications.Where(n => n.NotificationId < lastNotificationIdValue).Take(maximumRecordsToShow));
                    else
                        notifications = (notifications.Take(maximumRecordsToShow));
                }

                #endregion Filters

                #region Update UserNotification setting

                if (userNotificationSetting == null)
                {
                    //Add an UserNotificationsetting record
                    userNotificationSetting = new UserNotificationSetting();
                    userNotificationSetting.RelatedTable = "Project";
                    userNotificationSetting.UserID = pb.UserID;
                    userNotificationSetting.RelatedId = projectIdValue;
                    userNotificationSetting.LastNotificationId = maxNotificationID;
                    userNotificationSetting.ShowMyNotifications = Convert.ToBoolean(ShowMyNotifications);
                    userNotificationSetting.CreatedBy = pb.UserID;
                    userNotificationSetting.CreatedDate = Now;
                    userNotificationSetting.LastUpdatedBy = pb.UserID;
                    userNotificationSetting.LastUpdatedDate = Now;
                    pb.DataContext.UserNotificationSettings.AddObject(userNotificationSetting);
                }
                else if (!Convert.ToBoolean(IsScroll))
                {
                    if (onlyLatestNotifications)
                    {
                        userNotificationSetting.LastNotificationId = maxNotificationID;
                    }
                    else if (userNotificationSetting.LastNotificationId < maxNotificationID)
                    {
                        userNotificationSetting.LastNotificationId = maxNotificationID;
                    }
                    if (Convert.ToBoolean(showMyNotificationsClicked))
                        userNotificationSetting.ShowMyNotifications = Convert.ToBoolean(ShowMyNotifications);
                    userNotificationSetting.LastUpdatedBy = pb.UserID;
                    userNotificationSetting.LastUpdatedDate = Now;
                }
                pb.DataContext.SaveChanges();

                #endregion Update UserNotification setting

                //Initialize the list to be exported
                List<NotificationDTD> notificationsList = new List<NotificationDTD>();

                if (notifications.Count() > 0)
                {
                    foreach (var n in notifications)
                    {
                        NotificationDTD nDTD = new NotificationDTD();

                        nDTD.NotificationId = n.NotificationId;
                        switch (n.OperationType)
                        {
                            case "ADD":
                                nDTD.ImageURL = "../Common/Images/add.png";
                                break;

                            case "EDIT":
                                nDTD.ImageURL = "../Common/Images/edit.png";
                                break;

                            case "DELETE":
                                nDTD.ImageURL = "../Common/Images/delete.png";
                                break;

                            default:
                                break;
                        }

                        switch (n.Module)
                        {
                            case "PROJECT":
                                nDTD.ModuleName = string.Format("Project Details");
                                if (n.RelatedId != 0)//This is to redirect users to the project attachment tab. If there are notifications for other tabs, Add coulmn to notification table to cater sub module type
                                {
                                    nDTD.ModuleHref = string.Format("../Project/ProjectDetails.aspx?projectid={0}&TabId=1", n.ProjectId);
                                }
                                else
                                {
                                    nDTD.ModuleHref = string.Format("../Project/ProjectDetails.aspx?projectid={0}", n.ProjectId);
                                }
                                break;

                            case "SCHEDULE":
                                nDTD.ModuleName = "Project Schedule";
                                nDTD.ModuleHref = string.Format("../Project/ProjectSchedule.aspx?projectid={0}", n.ProjectId);
                                break;

                            case "PROJTEAM":
                                nDTD.ModuleName = "Project Team";
                                nDTD.ModuleHref = string.Format("../Project/ProjectTeam.aspx?projectid={0}", n.ProjectId);
                                break;

                            case "ITEMBRIEF":
                            case "TASK":
                            case "ITEMBRIEFBOOKING":
                            case "ITEMBRIEFMEDIA":
                                int itemBriefId = 0;
                                itemBriefId = (int)n.RelatedId;
                                if (n.OperationType != "DELETE" || n.Module == "TASK") //When the item brief is deleted, there is no itembrief record to get the name
                                {
                                    var itemBrief = (from i in pb.DataContext.ItemBriefs where i.ItemBriefId == itemBriefId select new { Name = i.Name }).FirstOrDefault();
                                    if (itemBrief != null)
                                    {
                                        nDTD.ModuleName = itemBrief.Name;
                                        if (n.Module == "ITEMBRIEF")
                                        {
                                            nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}", itemBriefId);
                                        }
                                        else if (n.Module == "TASK")
                                        {
                                            nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&selectedTab={1}", n.RelatedId, "Tasks");
                                        }
                                        else if (n.Module == "ITEMBRIEFMEDIA")
                                        {
                                            nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&TabId=2", itemBriefId);
                                        }
                                        else if (n.Module == "ITEMBRIEFBOOKING")
                                        {
                                            nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefDetails.aspx?ItemBriefId={0}&TabId=3", itemBriefId);
                                        }
                                    }
                                    else
                                    {
                                        nDTD.ModuleName = "Item Brief";
                                        nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefList.aspx?projectid={0}", n.ProjectId);
                                    }
                                }
                                else
                                {
                                    nDTD.ModuleName = "Item Brief";
                                    nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefList.aspx?projectid={0}", n.ProjectId);
                                }
                                break;

                            case "ITEMLIST":
                                nDTD.ModuleName = "Item Brief List";
                                nDTD.ModuleHref = string.Format("../ItemBrief/ItemBriefList.aspx?projectid={0}", n.ProjectId);
                                break;

                            default:
                                break;
                        }

                        nDTD.Message = n.Message;
                        nDTD.EventDate = Utils.FormatDatetime(Convert.ToDateTime(n.EventDate), true);

                        if (n.NotificationId > usersLastViewdNotificationId)
                        {
                            nDTD.Style = "NotificationBlockWithBackground";
                        }
                        else
                        {
                            nDTD.Style = "NotificationBlock";
                        }

                        notificationsList.Add(nDTD);
                    }
                }

                return notificationsList;
            }
        }

        #endregion Web Methods
    }
}