using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Personal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Business.Notification
{
    /// <summary>
    /// BL class for Notifications.
    /// </summary>
    public class NotificationBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public NotificationBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Enumeration for operation mode.
        /// </summary>
        public enum OperationMode
        {
            Add,
            Edit,
            Delete
        }

        /// <summary>
        /// Enumeration for booking action.
        /// </summary>
        public enum BookingAction
        {
            Pin,
            Keep,
            Remove,
            RemoveWithSnapshot
        }

        /// <summary>
        /// Adds the notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="shouldSave">if set to <c>true</c> [should save].</param>
        public void AddNotification(Data.Notification notification, bool shouldSave = true)
        {
            DataContext.Notifications.AddObject(notification);
            if (shouldSave)
            {
                this.SaveChanges();
            }
        }

        /// <summary>
        /// Generates the notifications for media.
        /// </summary>
        /// <param name="media">The media.</param>
        /// <param name="userID">The user ID.</param>
        /// <param name="projectId">The project id.</param>
        /// <param name="operationMode">The operation mode.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="newURL">The new URL.</param>
        public void GenerateNotificationsForMedia(DocumentMedia media, int userID, int projectId, OperationMode operationMode, string newName = "", string newURL = "")
        {
            string hyperlinkExtension = "Hyperlink";
            PersonalBL personalBL = new PersonalBL(DataContext);
            User user = personalBL.GetUser(userID);
            string userName = string.Concat(user.FirstName + " " + user.LastName).Trim();
            StageBitz.Data.Notification nf = new StageBitz.Data.Notification();
            nf.CreatedByUserId = nf.LastUpdatedByUserId = userID;
            nf.CreatedDate = nf.LastUpdatedDate = Utils.Now;
            nf.RelatedId = media.RelatedId;
            nf.ProjectId = projectId;

            string message = string.Empty;
            string fileName = string.Empty;
            if (media.FileExtension.ToUpper() != hyperlinkExtension.ToUpper())
            {
                if (operationMode == OperationMode.Edit)
                {
                    fileName = string.Format("file '{0}'", string.Concat(newName, ".", media.FileExtension != null ? media.FileExtension : string.Empty));
                }
                else
                {
                    fileName = string.Format("file '{0}'", string.Concat(media.Name, ".", media.FileExtension != null ? media.FileExtension : string.Empty));
                }
            }
            else
            {
                if (operationMode == OperationMode.Edit)
                {
                    fileName = this.GetContentForEditHyperlink(media, newName, newURL);
                }
                else
                {
                    fileName = string.Format("Hyperlink '{0}'", (media.Name != null && media.Name != string.Empty) ? media.Name : media.Description);
                }
            }

            switch (operationMode)
            {
                case OperationMode.Add:
                    message = "{0} added the {1}.";
                    nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "ADD");
                    break;

                case OperationMode.Edit:
                    message = "{0} edited the {1}.";
                    nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "EDIT");
                    break;

                case OperationMode.Delete:
                    message = "{0} deleted the {1}.";
                    nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "DELETE");
                    break;
            }

            // Set module type/ops type based on the related table.
            switch (media.RelatedTableName)
            {
                case "Project":
                    nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "PROJECT");
                    break;

                case "ItemBrief":
                    nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEFMEDIA");
                    break;
            }

            nf.Message = string.Format(message, userName, fileName);
            this.AddNotification(nf);
        }

        /// <summary>
        /// Generates the notifications for bookings.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="action">The action.</param>
        public void GenerateNotificationsForBookings(int userID, int itemBriefId, int itemId, int projectId, BookingAction action)
        {
            PersonalBL personalBL = new PersonalBL(DataContext);
            User user = personalBL.GetUser(userID);
            string userName = string.Concat(user.FirstName + " " + user.LastName).Trim();

            InventoryBL inventoryBL = new InventoryBL(DataContext);
            Data.Item item = inventoryBL.GetItem(itemId);

            ItemBriefBL itemBriefBL = new ItemBriefBL(DataContext);
            Data.ItemType itemType = itemBriefBL.GetItemBriefType(itemBriefId).ItemType;

            if (itemType != null && item != null)
            {
                StageBitz.Data.Notification nf = new StageBitz.Data.Notification();
                nf.CreatedByUserId = nf.LastUpdatedByUserId = userID;
                nf.CreatedDate = nf.LastUpdatedDate = Utils.Now;
                nf.RelatedId = itemBriefId;
                nf.ProjectId = projectId;
                nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEFBOOKING");

                string message = string.Empty;

                switch (action)
                {
                    case BookingAction.Pin:
                        message = "{0} pinned {1} from the Company Inventory to this {2} Brief.";
                        nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "PIN");
                        break;

                    case BookingAction.Keep:
                        message = "{0} confirmed {1} from the Company Inventory to use for this {2} Brief.";
                        nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "KEEP");
                        break;

                    case BookingAction.Remove:
                        message = "{0} returned {1} to the Company Inventory and removed it as an option for this {2} Brief.";
                        nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "REMOVE");
                        break;
                    case BookingAction.RemoveWithSnapshot:
                        message = "{0} released {1} to the Company Inventory and kept a record for this {2} Brief.";
                        nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "REMOVE");
                        break;
                }

                nf.Message = string.Format(message, userName, item.Name, itemType.Name);
                this.AddNotification(nf);
            }
        }

        /// <summary>
        /// Gets the user email notifications for all projects that has notifications.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <param name="durationInDays">The duration in days.</param>
        /// <returns>List of user email notifications.</returns>
        public List<ProjectNotificationDetails> GetUserEmailNotifications(DateTime dateToConsider, int durationInDays)
        {
            TimeSpan ts;
            TimeSpan.TryParse(Utils.GetSystemValue("AgentExecutionTime"), out ts);
            DateTime startDate = dateToConsider.Date.AddDays(-(durationInDays)) + ts;
            DateTime endDate = dateToConsider.Date + ts;
            DateTime todayStart = endDate.AddDays(-1);

            int noEmailNotificationCodeId = Utils.GetCodeIdByCodeValue("UserEmailNotificationType", "NOTATALL");

            int projectTeamModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "PROJTEAM");
            int itemBriefModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
            int scheduleModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "SCHEDULE");
            int projectModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "PROJECT");
            int taskModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "TASK");
            int itemListModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMLIST");
            int itemBriefMediaModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEFMEDIA");
            int itemBriefBookingModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEFBOOKING");

            return (from ns in
                        (from n in DataContext.Notifications
                         where n.CreatedDate >= startDate && n.CreatedDate < endDate
                         group n by n.ProjectId into g
                         select new { ProjectId = g.Key, Notifications = g })
                    select new ProjectNotificationDetails
                    {
                        ProjectId = ns.ProjectId.Value,
                        ProjectTeamUpdatesStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications where n.CreatedDate >= todayStart && n.CreatedDate < endDate && n.ModuleTypeCodeId == projectTeamModuleTypeCodeId select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications where n.ModuleTypeCodeId == projectTeamModuleTypeCodeId select n).Count() > 0,
                            UserList = (from n in ns.Notifications where n.ModuleTypeCodeId == projectTeamModuleTypeCodeId && n.CreatedByUserId.HasValue select n.CreatedByUserId.Value).Distinct()
                        },
                        ScheduleUpdateStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications where n.CreatedDate >= todayStart && n.CreatedDate < endDate && n.ModuleTypeCodeId == scheduleModuleTypeCodeId select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications where n.ModuleTypeCodeId == scheduleModuleTypeCodeId select n).Count() > 0,
                            UserList = (from n in ns.Notifications where n.ModuleTypeCodeId == scheduleModuleTypeCodeId && n.CreatedByUserId.HasValue select n.CreatedByUserId.Value).Distinct()
                        },
                        ItemBriefUpdateStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications
                                                      where n.CreatedDate >= todayStart && n.CreatedDate < endDate
                                                            && (n.ModuleTypeCodeId == itemBriefModuleTypeCodeId || 
                                                                n.ModuleTypeCodeId == itemBriefMediaModuleTypeCodeId ||
                                                                n.ModuleTypeCodeId == itemBriefBookingModuleTypeCodeId)
                                                      select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications
                                                         where (n.ModuleTypeCodeId == itemBriefModuleTypeCodeId || 
                                                                n.ModuleTypeCodeId == itemBriefMediaModuleTypeCodeId ||
                                                                n.ModuleTypeCodeId == itemBriefBookingModuleTypeCodeId)
                                                         select n).Count() > 0,
                            UserList = (from n in ns.Notifications
                                        where (n.ModuleTypeCodeId == itemBriefModuleTypeCodeId || 
                                                n.ModuleTypeCodeId == itemBriefMediaModuleTypeCodeId ||
                                                n.ModuleTypeCodeId == itemBriefBookingModuleTypeCodeId) && n.CreatedByUserId.HasValue
                                        select n.CreatedByUserId.Value).Distinct()
                        },
                        ProjectUpdateStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications where n.CreatedDate >= todayStart && n.CreatedDate < endDate && n.ModuleTypeCodeId == projectModuleTypeCodeId select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications where n.ModuleTypeCodeId == projectModuleTypeCodeId select n).Count() > 0,
                            UserList = (from n in ns.Notifications where n.ModuleTypeCodeId == projectModuleTypeCodeId && n.CreatedByUserId.HasValue select n.CreatedByUserId.Value).Distinct()
                        },
                        TaskUpdateStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications where n.CreatedDate >= todayStart && n.CreatedDate < endDate && n.ModuleTypeCodeId == taskModuleTypeCodeId select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications where n.ModuleTypeCodeId == taskModuleTypeCodeId select n).Count() > 0,
                            UserList = (from n in ns.Notifications where n.ModuleTypeCodeId == taskModuleTypeCodeId && n.CreatedByUserId.HasValue select n.CreatedByUserId.Value).Distinct()
                        },
                        ItemListUpdateStatus = new UpdateStatus
                        {
                            HasUpdatesForGivenDate = (from n in ns.Notifications where n.CreatedDate >= todayStart && n.CreatedDate < endDate && n.ModuleTypeCodeId == itemListModuleTypeCodeId select n).Count() > 0,
                            HasUpdatesForFullDuration = (from n in ns.Notifications where n.ModuleTypeCodeId == itemListModuleTypeCodeId select n).Count() > 0,
                            UserList = (from n in ns.Notifications where n.ModuleTypeCodeId == itemListModuleTypeCodeId && n.CreatedByUserId.HasValue select n.CreatedByUserId.Value).Distinct()
                        }
                    }).ToList();
        }

        /// <summary>
        /// Gets the content for edit hyperlink.
        /// </summary>
        /// <param name="media">The media.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="newURL">The new URL.</param>
        /// <returns>The notification text for edit hyperlink.</returns>
        private string GetContentForEditHyperlink(DocumentMedia media, string newName, string newURL)
        {
            string editLinkText = "Hyperlink url '{0}'";
            string editLabelText = "Hyperlink label {0}";
            string editBoth = "Hyprlink url '{0}' and label {1}";

            if (media.Name != newName && media.Description != newURL)
            {
                return string.Format(editBoth, newURL, newName != string.Empty ? string.Concat("'", newName, "'") : newName);
            }
            else if (media.Name != newName)
            {
                return string.Format(editLabelText, newName != string.Empty ? string.Concat("'", newName, "'") : newName);
            }
            else
            {
                return string.Format(editLinkText, newURL);
            }
        }
    }
}