using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Company;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemTypes;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Personal;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Business.Report;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;

namespace StageBitz.Logic.Business.ItemBrief
{
    /// <summary>
    /// Business layer for Item Briefs
    /// </summary>
    public class ItemBriefBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemBriefBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public ItemBriefBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public Data.ItemBrief GetItemBrief(int itemBriefId)
        {
            return (from ib in DataContext.ItemBriefs
                    where ib.ItemBriefId == itemBriefId
                    select ib).FirstOrDefault();
        }

        /// <summary>
        /// Adds the item brief.
        /// </summary>
        /// <param name="itemBrief">The item brief.</param>
        public void AddItemBrief(Data.ItemBrief itemBrief)
        {
            DataContext.ItemBriefs.AddObject(itemBrief);
            SaveChanges();
        }

        /// <summary>
        /// Determines whether item brief is in complete status.
        /// </summary>
        /// <param name="itemBrief">The item brief.</param>
        /// <returns></returns>
        public bool IsItemBriefComplete(Data.ItemBrief itemBrief)
        {
            return (itemBrief.ItemBriefStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "STOREDININVENTORY") || itemBrief.ItemBriefStatusCodeId == Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "DISPOSEDOF"));
        }

        /// <summary>
        /// Gets the item brief list.
        /// </summary>
        /// <param name="findName">Name of the find.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ItemBriefListInfo> GetItemBriefList(string findName, int projectId, int itemTypeId)
        {
            return (from ib in DataContext.ItemBriefs
                    from mediaId in
                        (from m in DataContext.DocumentMedias
                         where m.RelatedTableName == "ItemBrief" && m.RelatedId == ib.ItemBriefId && m.SortOrder == 1
                         select m.DocumentMediaId).DefaultIfEmpty() //get item brief's default image
                    join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                    join ibt in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibt.ItemBriefId
                    where ib.ProjectId == projectId && ibt.ItemTypeId == itemTypeId && ibt.IsActive == true
                    && (findName == null || ib.Name.ToLower().Contains(findName))
                    orderby ib.Name
                    select new ItemBriefListInfo
                    {
                        ItemBrief = ib,
                        ThumbnailMediaId = mediaId,
                        StatusSortOrder = statusCode.SortOrder,
                        Status = statusCode.Description
                    }).ToList();
        }

        /// <summary>
        /// Gets the company by item brief id.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public Data.Company GetCompanyByItemBriefId(int itemBriefId)
        {
            return DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == itemBriefId).FirstOrDefault().Project.Company;
        }

        /// <summary>
        /// Gets the country identifier by item brief id.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public int GetCountryIdByItemBriefId(int itemBriefId)
        {
            return (from p in DataContext.Projects
                    join ib in DataContext.ItemBriefs on p.ProjectId equals ib.ProjectId
                    where ib.ItemBriefId == itemBriefId
                    select p.CountryId.Value).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="originalLastUpdatedDate">The original last updated date.</param>
        /// <returns></returns>
        public Data.ItemBrief GetItemBrief(int itemBriefId, DateTime originalLastUpdatedDate)
        {
            return DataContext.ItemBriefs.FirstOrDefault(ib => ib.ItemBriefId == itemBriefId && ib.LastUpdatedDate == originalLastUpdatedDate);
        }

        /// <summary>
        /// Gets the inuse item booking.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public ItemBooking GetInUseItemBooking(int itemBriefId)
        {
            int inuseStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inuseCompleteStatusCode = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");
            return DataContext.ItemBookings.Where(ibs => ibs.RelatedId == itemBriefId && ibs.RelatedTable == "ItemBrief" &&
                    (ibs.ItemBookingStatusCodeId == inuseStatusCodeId || ibs.ItemBookingStatusCodeId == inuseCompleteStatusCode) &&
                    ibs.IsActive == true).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item brief count by item type identifier.
        /// </summary>
        /// <param name="ItemTypeId">The item type identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public ItemTypeSummary GetItemBriefCountByItemTypeId(int ItemTypeId, int projectId)
        {
            int primaryItemBriefTypeCodeId = Utils.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");
            int completedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            return (from p in DataContext.Projects
                    join pit in DataContext.ProjectItemTypes on p.ProjectId equals pit.ProjectId
                    where p.ProjectId == projectId && pit.ItemTypeId == ItemTypeId
                    orderby pit.ItemType.Name
                    select new ItemTypeSummary
                    {
                        ItemCount = (from i in DataContext.ItemBriefs join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId where ibt.ItemTypeId == pit.ItemTypeId && ibt.ItemBriefTypeCodeId == primaryItemBriefTypeCodeId && i.ProjectId == p.ProjectId select new { i.Quantity }).Count(),
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item brief details.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="findName">Name of the find.</param>
        /// <returns></returns>
        public List<Data.ItemBrief> GetItemBriefDetails(int projectId, int itemTypeId, string findName)
        {
            return (from ib in DataContext.ItemBriefs
                    join ibt in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibt.ItemBriefId
                    where ib.ProjectId == projectId && ibt.ItemTypeId == itemTypeId && ibt.IsActive == true
                    && (findName == null || ib.Name.ToLower().Contains(findName))
                    select ib).ToList<Data.ItemBrief>();
        }

        /// <summary>
        /// Gets the ordered items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public List<Data.ItemBrief> GetOrderedItems(List<Data.ItemBrief> items, int order)
        {
            List<Data.ItemBrief> orderedItems = null;
            switch (order)
            {
                case 0:
                    return (from ib in items
                            join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                            orderby statusCode.SortOrder ascending
                            select ib).ToList<Data.ItemBrief>();

                case 1:
                    return (from ib in items
                            join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                            orderby statusCode.SortOrder descending
                            select ib).ToList<Data.ItemBrief>();
            }
            return orderedItems;
        }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public Data.ItemType GetItemType(int itemTypeId)
        {
            return (from it in DataContext.ItemTypes
                    where it.ItemTypeId == itemTypeId
                    select it).FirstOrDefault();
        }

        /// <summary>
        /// Gets the item type list.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<Data.ItemType> GetItemTypeList(int projectId, int itemTypeId)
        {
            return (from it in DataContext.ItemTypes
                    join pit in DataContext.ProjectItemTypes on it.ItemTypeId equals pit.ItemTypeId
                    where pit.ProjectId == projectId && it.ItemTypeId != itemTypeId
                    select it).ToList<Data.ItemType>();
        }

        /// <summary>
        /// Gets the item brief tasks budget.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public List<ItemBriefTaskBudget> GetItemBriefTasksBudget(int itemBriefId)
        {
            return (from ib in DataContext.ItemBriefs
                    join ibt in DataContext.ItemBriefTasks on ib.ItemBriefId equals ibt.ItemBriefId
                    where ib.ItemBriefId == itemBriefId
                    select new ItemBriefTaskBudget
                    {
                        EstimatedCost = ibt.EstimatedCost != null ? (decimal?)ibt.EstimatedCost.Value : null,
                        TotalCost = (decimal?)(ibt.TotalCost),
                        ItemBriefTaskStatusCodeId = ibt.ItemBriefTaskStatusCodeId
                    }).ToList<ItemBriefTaskBudget>();
        }

        /// <summary>
        /// Gets the sum expend.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="completeStatusCodeID">The complete status code identifier.</param>
        /// <returns></returns>
        public decimal? GetSumExpend(int itemBriefId, int completeStatusCodeID)
        {
            List<ItemBriefTaskBudget> itemBriefTasksBudget = GetItemBriefTasksBudget(itemBriefId);
            return (from pb in itemBriefTasksBudget
                    where pb.ItemBriefTaskStatusCodeId == completeStatusCodeID
                    select (decimal?)pb.TotalCost).Sum();
        }

        /// <summary>
        /// Gets the sum remaining.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="inprogressStatusCodeID">The inprogress status code identifier.</param>
        /// <returns></returns>
        public decimal? GetSumRemaining(int itemBriefId, int inprogressStatusCodeID)
        {
            List<ItemBriefTaskBudget> itemBriefTasksBudget = GetItemBriefTasksBudget(itemBriefId);
            return (from pb in itemBriefTasksBudget
                    where pb.ItemBriefTaskStatusCodeId == inprogressStatusCodeID
                    select (decimal?)pb.EstimatedCost).Sum();
        }

        /// <summary>
        /// Determines whether estimate cost is empty for the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public bool HasEmptyEstimateCostInItemBrief(int itemBriefId)
        {
            int itemBriefInprogressTaskStatusCodeId = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;
            List<ItemBriefTask> ibTaskList = DataContext.ItemBriefTasks.Where(ibt => ibt.EstimatedCost == null && ibt.ItemBriefTaskStatusCodeId == itemBriefInprogressTaskStatusCodeId && ibt.ItemBriefId == itemBriefId).ToList();
            return (ibTaskList.Count() > 0);
        }

        /// <summary>
        /// Determines whether  estimate cost is empty for project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public bool HasEmptyEstimateCostInProject(int projectId, int itemTypeId)
        {
            int itemBriefInprogressTaskStatusCodeId = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;
            return (from ibt in DataContext.ItemBriefTasks
                    join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                    join ibtypes in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtypes.ItemBriefId
                    where ib.ProjectId == projectId && ibt.ItemBriefTaskStatusCodeId == itemBriefInprogressTaskStatusCodeId && ibt.EstimatedCost == null && (itemTypeId == 0 || itemTypeId == -1 || itemTypeId == ibtypes.ItemTypeId)
                    select ibt).ToList<ItemBriefTask>().Count() > 0;
        }

        /// <summary>
        /// Gets the item brief export details.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public IEnumerable<ItemBriefExportDetails> GetItemBriefExportDetails(int projectId, int itemTypeId)
        {
            var allItemBriefDetails = DataContext.GetALLItemBriefs(projectId, itemTypeId);
            if (allItemBriefDetails != null)
            {
                var allItemBriefs = (from ib in allItemBriefDetails
                                     select new ItemBriefExportDetails
                                     {
                                         ItemBriefId = ib.ItemBriefId,
                                         ItemBriefName = ib.ItemBriefName,
                                         FieldId = ib.FieldId,
                                         FieldName = ib.FieldName,
                                         Value = ib.value,
                                         Description = ib.Description,
                                         ItemTypeName = ib.ItemTypeName,
                                         Quantity = ib.Quantity.HasValue ? ib.Quantity.Value : ib.Quantity,
                                         Category = ib.Category,
                                         Considerations = ib.Considerations,
                                         Brief = ib.Brief,
                                         Approver = ib.Approver,
                                         Act = ib.Act,
                                         Page = ib.Page,
                                         Preset = ib.Preset,
                                         RehearsalItem = ib.RehearsalItem,
                                         Scene = ib.Scene,
                                         Source = ib.source,
                                         Usage = ib.Usage,
                                         FieldGroupId = ib.FieldGroupId,
                                         FieldGroupName = ib.FieldGroupName,
                                         Character = ib.Character
                                     });
                return allItemBriefs;
            }
            return null;
        }

        #region ItemBriefTask

        /// <summary>
        /// Gets the item brief tasks.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public List<ItemBriefTaskDetails> GetItemBriefTasks(int itemBriefId)
        {
            return (from ibt in DataContext.ItemBriefTasks
                    join c in DataContext.Codes on ibt.ItemBriefTaskStatusCodeId equals c.CodeId
                    join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                    where ibt.ItemBriefId == itemBriefId
                    orderby c.SortOrder
                    select new ItemBriefTaskDetails
                    {
                        ItemBriefTask = ibt,
                        ItemBriefName = ib.Name,
                        SortOrder = c.SortOrder,
                        Total = (ibt.TotalCost != null ? ibt.TotalCost : 0)
                    }).ToList<ItemBriefTaskDetails>();
        }

        /// <summary>
        /// Gets the item brief tasks.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ItemBriefTaskDetails> GetItemBriefTasks(int projectId, int itemTypeId)
        {
            int ItemBriefTaskCompletedStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");

            return (from ibt in DataContext.ItemBriefTasks
                    join c in DataContext.Codes on ibt.ItemBriefTaskStatusCodeId equals c.CodeId
                    join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                    join ibtyp in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtyp.ItemBriefId
                    orderby ibt.CompletedDate
                    where ib.ProjectId == projectId
                          && ibt.ItemBriefTaskStatusCodeId == ItemBriefTaskCompletedStatusCodeId && ibtyp.ItemTypeId == itemTypeId
                    select new ItemBriefTaskDetails
                    {
                        ItemBriefTask = ibt,
                        ItemBriefName = ib.Name,
                        Total = (ibt.TotalCost != null ? ibt.TotalCost : 0)
                    }).ToList();
        }

        /// <summary>
        /// Gets the item brief incomplte tasks count.
        /// </summary>
        /// <param name="itemBriefTasks">The item brief tasks.</param>
        /// <returns></returns>
        public int GetItemBriefIncomplteTasksCount(List<ItemBriefTaskDetails> itemBriefTasks)
        {
            int itemBriefInprogressTaskStatusCodeId = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;
            return (from ibt in itemBriefTasks
                    where ibt.ItemBriefTask.ItemBriefTaskStatusCodeId == itemBriefInprogressTaskStatusCodeId
                    select ibt).Count();
        }

        /// <summary>
        /// Gets the item brief task.
        /// </summary>
        /// <param name="itemBriefTaskId">The item brief task identifier.</param>
        /// <param name="originalLastUpdatedDate">The original last updated date.</param>
        /// <returns></returns>
        public ItemBriefTask GetItemBriefTask(int itemBriefTaskId, DateTime originalLastUpdatedDate)
        {
            return DataContext.ItemBriefTasks.FirstOrDefault(ibt => ibt.ItemBriefTaskId == itemBriefTaskId && ibt.LastUpdatedDate == originalLastUpdatedDate);
        }

        /// <summary>
        /// Deletes the item brief task from all list.
        /// </summary>
        /// <param name="itemBriefTaskId">The item brief task identifier.</param>
        public void DeleteItemBriefTaskFromAllList(int itemBriefTaskId)
        {
            List<TaskListsItemBriefTask> taskListItemBriefTask1 = (from tlib in DataContext.TaskListsItemBriefTasks
                                                                   where tlib.ItemBriefTaskId == itemBriefTaskId
                                                                   select tlib).ToList<TaskListsItemBriefTask>();

            foreach (TaskListsItemBriefTask tlib in taskListItemBriefTask1)
            {
                DataContext.TaskListsItemBriefTasks.DeleteObject(tlib);
            }
            SaveChanges();
        }

        /// <summary>
        /// Deletes the item brief task.
        /// </summary>
        /// <param name="itemBriefTask">The item brief task.</param>
        public void DeleteItemBriefTask(ItemBriefTask itemBriefTask)
        {
            DataContext.ItemBriefTasks.DeleteObject(itemBriefTask);
            DeleteItemBriefTaskFromAllList(itemBriefTask.ItemBriefTaskId);
        }

        /// <summary>
        /// Gets the item brief task.
        /// </summary>
        /// <param name="itemBriefTaskId">The item brief task identifier.</param>
        /// <returns></returns>
        public ItemBriefTask GetItemBriefTask(int itemBriefTaskId)
        {
            return DataContext.ItemBriefTasks.Where(ib => ib.ItemBriefTaskId == itemBriefTaskId).FirstOrDefault();
        }

        /// <summary>
        /// Loads the complete and inprogress task list.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<TaskLists> LoadCompleteAndInprogressTaskList(int projectId, int itemTypeId)
        {
            int ItemBriefTaskInprogressStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");
            int ItemBriefTaskCompletedTaskStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");

            return (from tl in DataContext.TaskLists
                    where tl.ProjectId == projectId && tl.ItemTypeId == itemTypeId
                    orderby tl.Name
                    select new TaskLists
                    {
                        TaskListId = tl.TaskListId,
                        Name = tl.Name,
                        TaskCount = tl.TaskListsItemBriefTasks.Count(),
                        CompletedItemCount = (from tlib in DataContext.TaskListsItemBriefTasks
                                              join ib in DataContext.ItemBriefTasks on tlib.ItemBriefTaskId equals ib.ItemBriefTaskId
                                              where tlib.TaskListId == tl.TaskListId && ib.ItemBriefTaskStatusCodeId == ItemBriefTaskCompletedTaskStatusCodeId
                                              select tlib.TaskListId).Count(),
                        InprogressItemCount = (from tlib in DataContext.TaskListsItemBriefTasks
                                               join ib in DataContext.ItemBriefTasks on tlib.ItemBriefTaskId equals ib.ItemBriefTaskId
                                               where tlib.TaskListId == tl.TaskListId && ib.ItemBriefTaskStatusCodeId == ItemBriefTaskInprogressStatusCodeId
                                               select tlib.TaskListId).Count(),
                    }).ToList<TaskLists>();
        }

        /// <summary>
        /// Loads the active tasks.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ActiveTasks> LoadActiveTasks(int projectId, int itemTypeId)
        {
            int ItemBriefTaskStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");

            return (from t in DataContext.ItemBriefTasks
                    join i in DataContext.ItemBriefs on t.ItemBriefId equals i.ItemBriefId
                    join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId
                    where t.ItemBriefTaskStatusCodeId == ItemBriefTaskStatusCodeId
                          && i.ProjectId == projectId && ibt.ItemTypeId == itemTypeId
                    select new ActiveTasks
                    {
                        ItemBriefTaskId = t.ItemBriefTaskId,
                        ItemBriefId = t.ItemBriefId,
                        EstimatedCost = t.EstimatedCost,
                        ItemTypeId = ibt.ItemTypeId,
                        itemBriefName = i.Name,
                        taskDescription = t.Description,
                        taskListId = ((from ibtl in DataContext.TaskListsItemBriefTasks //Check whether this task is already assigned to a task list.
                                       where ibtl.ItemBriefTaskId == t.ItemBriefTaskId
                                       select ibtl.TaskListId).Count() > 0 ? 1 : 0),
                    }).OrderBy(j => j.itemBriefName).ThenBy(p => p.taskDescription).ToList<ActiveTasks>();
        }

        /// <summary>
        /// Gets the task lists item brief task.
        /// </summary>
        /// <param name="taskListId">The task list identifier.</param>
        /// <param name="itemBriefTaskid">The item brief taskid.</param>
        /// <returns></returns>
        public TaskListsItemBriefTask GetTaskListsItemBriefTask(int taskListId, int itemBriefTaskid)
        {
            return (from tit in DataContext.TaskListsItemBriefTasks
                    where tit.TaskListId == taskListId && tit.ItemBriefTaskId == itemBriefTaskid
                    select tit).FirstOrDefault();
        }

        /// <summary>
        /// Gets the task list.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <returns></returns>
        public List<TaskList> GetTaskList(int taskId)
        {
            return (from t in DataContext.TaskLists
                    join ibtl in DataContext.TaskListsItemBriefTasks on t.TaskListId equals ibtl.TaskListId
                    where ibtl.ItemBriefTaskId == taskId
                    select t).ToList<TaskList>();
        }

        /// <summary>
        /// Gets the task list by task list identifier.
        /// </summary>
        /// <param name="taskListId">The task list identifier.</param>
        /// <returns></returns>
        public TaskList GetTaskListByTaskListId(int taskListId)
        {
            return (from tl in DataContext.TaskLists where tl.TaskListId == taskListId select tl).FirstOrDefault();
        }

        /// <summary>
        /// Gets the active tasks for active task list.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ActiveTaskListTasks> GetActiveTasksForActiveTaskList(int projectId, int itemTypeId)
        {
            int ItemBriefTaskInprogressStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");

            return (from ibt in DataContext.ItemBriefTasks
                    join i in DataContext.ItemBriefs on ibt.ItemBriefId equals i.ItemBriefId
                    join ibtypes in DataContext.ItemBriefTypes on i.ItemBriefId equals ibtypes.ItemBriefId
                    where ibt.ItemBriefTaskStatusCodeId == ItemBriefTaskInprogressStatusCodeId
                          && i.ProjectId == projectId && ibtypes.ItemTypeId == itemTypeId
                    select new ActiveTaskListTasks
                         {
                             CompletedDate = ibt.CompletedDate,
                             ItemName = i.Name,
                             TaskDescription = ibt.Description,
                             Vendor = ibt.Vendor,
                             NetCost = ibt.NetCost,
                             EstimatedCost = ibt.EstimatedCost,
                             Tax = ibt.Tax,
                             Total = (ibt.TotalCost != null ? ibt.TotalCost : 0),
                             IsEstimatedCostNullForActiveTask = (ibt.EstimatedCost == null && ibt.ItemBriefTaskStatusCodeId == ItemBriefTaskInprogressStatusCodeId)
                         }).OrderBy(j => j.ItemName).ThenBy(p => p.TaskDescription).ToList<ActiveTaskListTasks>();
        }

        /// <summary>
        /// Gets the shopping list by task list identifier.
        /// </summary>
        /// <param name="taskListId">The task list identifier.</param>
        /// <returns></returns>
        public List<ShoppingList> GetShoppingListByTaskListId(int taskListId)
        {
            int ItemBriefTaskCompletedStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "COMPLETED");
            int ItemBriefTaskInprogressStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");
            string completedStatusCodeDescription = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED").Description;
            string inProgressStatusCodeDescription = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").Description;

            return (from tl in DataContext.TaskLists
                    join ibtl in DataContext.TaskListsItemBriefTasks on tl.TaskListId equals ibtl.TaskListId
                    join ibt in DataContext.ItemBriefTasks on ibtl.ItemBriefTaskId equals ibt.ItemBriefTaskId
                    join ib in DataContext.ItemBriefs on ibt.ItemBriefId equals ib.ItemBriefId
                    join c in DataContext.Codes on ibt.ItemBriefTaskStatusCodeId equals c.CodeId
                    where tl.TaskListId == taskListId
                    orderby c.SortOrder
                    select new ShoppingList
                    {
                        Status = (ibt.ItemBriefTaskStatusCodeId != ItemBriefTaskCompletedStatusCodeId ? inProgressStatusCodeDescription : completedStatusCodeDescription),
                        ItemName = ib.Name,
                        TaskDescription = ibt.Description,
                        EstimatedCost = ibt.EstimatedCost,
                        Vendor = ibt.Vendor,
                        NetCost = ibt.NetCost,
                        Tax = ibt.Tax,
                        Total = (ibt.TotalCost != null ? ibt.TotalCost : 0),
                        IsEstimatedCostNullForShoppingList = (ibt.EstimatedCost == null && ibt.ItemBriefTaskStatusCodeId == ItemBriefTaskInprogressStatusCodeId)
                    }).ToList<ShoppingList>();
        }

        /// <summary>
        /// Removes the shopping list.
        /// </summary>
        /// <param name="taskListId">The task list identifier.</param>
        public void RemoveShoppingList(int taskListId)
        {
            //Tasks will be removed from the list. If they are completed, they won't be shown in the active task list

            //Remove task from task in the list
            var taskListItemBriefTask1 = from tlib in DataContext.TaskListsItemBriefTasks
                                         where tlib.TaskListId == taskListId
                                         select tlib;

            //If no tasks were added skip this part
            if (taskListItemBriefTask1.Count() > 0)
            {
                foreach (var tlib in taskListItemBriefTask1)
                {
                    DataContext.TaskListsItemBriefTasks.DeleteObject(tlib);
                }
            }

            //Remove the task list
            var taskList = GetTaskListByTaskListId(taskListId);

            DataContext.TaskLists.DeleteObject(taskList);

            DataContext.SaveChanges();
        }

        /// <summary>
        /// Gets the item brief tasks budget.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ItemBriefTaskBudget> GetItemBriefTasksBudget(int projectId, int itemTypeId)
        {
            return (from ib in DataContext.ItemBriefs
                    join ibt in DataContext.ItemBriefTasks on ib.ItemBriefId equals ibt.ItemBriefId
                    join ibtypes in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtypes.ItemBriefId
                    where ib.ProjectId == projectId && (itemTypeId == 0 || itemTypeId == -1 || itemTypeId == ibtypes.ItemTypeId)
                    select new ItemBriefTaskBudget { EstimatedCost = ibt.EstimatedCost, TotalCost = (decimal?)(ibt.TotalCost), ItemBriefTaskStatusCodeId = ibt.ItemBriefTaskStatusCodeId }).ToList<ItemBriefTaskBudget>();
        }

        /// <summary>
        /// Gets the item brief total budget.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public decimal? GetItemBriefTotalBudget(int projectId, int itemTypeId)
        {
            decimal? budget = (from ib in DataContext.ItemBriefs
                               join ibtypes in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtypes.ItemBriefId
                               where ib.ProjectId == projectId && (itemTypeId == 0 || itemTypeId == -1 || itemTypeId == ibtypes.ItemTypeId)
                               select ib.Budget).Sum();
            return budget;
        }

        /// <summary>
        /// Gets the budget list information.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public BudgetListInfo GetBudgetListInfo(int projectId, int itemTypeId)
        {
            var itemBriefTasksBudget = GetItemBriefTasksBudget(projectId, itemTypeId);
            var totalBudget = GetItemBriefTotalBudget(projectId, itemTypeId);

            int completeStatusCodeID = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED").CodeId;
            int inprogressStatusCodeID = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;

            var sumExpened = (from pb in itemBriefTasksBudget
                              where pb.ItemBriefTaskStatusCodeId == completeStatusCodeID
                              select (decimal?)pb.TotalCost).Sum();

            var sumRemaining = (from pb in itemBriefTasksBudget
                                where pb.ItemBriefTaskStatusCodeId == inprogressStatusCodeID
                                select (decimal?)pb.EstimatedCost).Sum();

            decimal? sumbalance = (totalBudget == null ? 0 : totalBudget) - ((sumExpened == null ? 0 : sumExpened) + (sumRemaining == null ? 0 : sumRemaining));

            return new BudgetListInfo
            {
                GetItemTypeTotalBudget = totalBudget,
                ItemBriefTaskBudgetList = itemBriefTasksBudget,
                SumBalance = sumbalance,
                SumExpened = sumExpened,
                SumRemaining = sumRemaining
            };
        }

        #endregion ItemBriefTask

        #region ItemBriefAttachments

        /// <summary>
        /// Determines whether item allready exists.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public bool IsItemAllreadyExist(int itemBriefId)
        {
            //Check for "Kept" items
            int inuseCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;
            int inuseCompleteCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;
            InventoryBL inventoryBL = new InventoryBL(DataContext);

            ItemBooking itemBooking = inventoryBL.GetInUseOrCompleteItemBooking(itemBriefId);

            Data.Item item = null;
            if (itemBooking != null)
            {
                item = itemBooking.Item;
            }

            return item != null;
        }

        /// <summary>
        /// Gets the item brief attachments.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public List<ItemBriefAttachment> GetItemBriefAttachments(int itemBriefId)
        {
            string fileExtension = "Hyperlink";
            List<ItemBriefAttachment> combinedList = (from m in DataContext.DocumentMedias
                                                      from pit in DataContext.ItemBriefItemDocumentMedias.Where(pit => pit.ItemBriefDocumentMediaId == m.DocumentMediaId).DefaultIfEmpty()
                                                      where m.RelatedTableName == "ItemBrief" && m.RelatedId == itemBriefId
                                                      select new ItemBriefAttachment
                                                      {
                                                          DocumentMediaId = m.DocumentMediaId,
                                                          Description = m.Description,
                                                          Name = m.Name,
                                                          IsImageFile = m.IsImageFile,
                                                          FileExtension = m.FileExtension.ToUpper(),
                                                          ItemBriefItemDocumentMediaId = pit == null ? 0 : pit.ItemBriefItemDocumentMediaId,
                                                          SourceTable = pit == null ? string.Empty : pit.SourceTable
                                                      }).ToList<ItemBriefAttachment>();

            List<ItemBriefAttachment> hyperlinkList = combinedList.Where(cl => cl.FileExtension == fileExtension.ToUpper()).ToList();
            List<ItemBriefAttachment> attachmntList = combinedList.Where(cl => cl.FileExtension != fileExtension.ToUpper()).ToList();

            hyperlinkList.AddRange(attachmntList);
            return hyperlinkList;
        }

        /// <summary>
        /// Shares the attachment with item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="documentMediaId">The document media identifier.</param>
        /// <param name="itemBriefItemDocumentMediaId">The item brief item document media identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
        public void ShareAttachmentWithItemBrief(int itemBriefId, int documentMediaId, int itemBriefItemDocumentMediaId, int userId, bool isChecked)
        {
            Data.Item item = DataContext.ItemBookings.Where(ibs => ibs.RelatedId == itemBriefId && ibs.RelatedTable == "ItemBrief" && ibs.IsActive == true).FirstOrDefault().Item;
            if (isChecked)
            {
                //To Share with the Item
                DataContext.CopyMediaFiles(documentMediaId, "Item", item.ItemId, userId);
            }
            else
            {
                //To be excluded
                DataContext.DeleteDocumentMediaFromItemBriefItem(itemBriefItemDocumentMediaId, 0);
            }
        }

        #endregion ItemBriefAttachments

        #region ItemBriefPinnedItems

        /// <summary>
        /// Gets all pinned items.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public List<ItemBooking> GetAllPinnedItems(int itemBriefId)
        {
            return (from ibs in DataContext.ItemBookings
                    where ibs.IsActive == true && ibs.RelatedId == itemBriefId && ibs.RelatedTable == "ItemBrief"
                    select ibs).ToList<ItemBooking>();
        }

        /// <summary>
        /// Determines whether item brief has pinned items.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public bool HasPinnedItems(int itemBriefId)
        {
            List<ItemBooking> pinnedItems = GetAllPinnedItems(itemBriefId);
            return pinnedItems != null && pinnedItems.Count() > 0 ? true : false;
        }

        #endregion ItemBriefPinnedItems

        #region ItemBriefType

        /// <summary>
        /// Gets the type of the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public ItemBriefType GetItemBriefType(int itemBriefId)
        {
            return DataContext.ItemBriefTypes.Where(ibt => ibt.ItemBriefId == itemBriefId).SingleOrDefault();
        }

        /// <summary>
        /// Gets the item brief type summary.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public List<ItemTypeSummary> GetItemBriefTypeSummary(int projectId)
        {
            int primaryItemBriefTypeCodeId = Utils.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");
            int completedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
            int inProgressItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "INPROGRESS");
            int notStartedItemStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "NOTSTARTED");

            return (from p in DataContext.Projects
                    join pit in DataContext.ProjectItemTypes on p.ProjectId equals pit.ProjectId
                    where p.ProjectId == projectId
                    orderby pit.ItemType.Name
                    select new ItemTypeSummary
                    {
                        ItemTypeId = pit.ItemTypeId,
                        ItemTypeName = pit.ItemType.Name,
                        ItemCount = (from i in DataContext.ItemBriefs join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId where ibt.ItemTypeId == pit.ItemTypeId && ibt.ItemBriefTypeCodeId == primaryItemBriefTypeCodeId && i.ProjectId == p.ProjectId select new { i.Quantity }).Count(),
                        CompletedItemCount = (from i in DataContext.ItemBriefs join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId where ibt.ItemTypeId == pit.ItemTypeId && ibt.ItemBriefTypeCodeId == primaryItemBriefTypeCodeId && i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == completedItemStatusCodeId select new { i.Quantity }).Count(),
                        InProgressItemCount = (from i in DataContext.ItemBriefs join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId where ibt.ItemTypeId == pit.ItemTypeId && ibt.ItemBriefTypeCodeId == primaryItemBriefTypeCodeId && i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == inProgressItemStatusCodeId select new { i.Quantity }).Count(),
                        NotStartedItemCount = (from i in DataContext.ItemBriefs join ibt in DataContext.ItemBriefTypes on i.ItemBriefId equals ibt.ItemBriefId where ibt.ItemTypeId == pit.ItemTypeId && ibt.ItemBriefTypeCodeId == primaryItemBriefTypeCodeId && i.ProjectId == p.ProjectId && i.ItemBriefStatusCodeId == notStartedItemStatusCodeId select new { i.Quantity }).Count(),
                    }).ToList<ItemTypeSummary>();
        }

        /// <summary>
        /// Compares the two item brief objects to identify and generate a comma separated list of edited fields.
        /// </summary>
        /// <param name="itemBriefOriginal">The item brief original.</param>
        /// <param name="itemBriefTmp">The item brief temporary.</param>
        /// <returns></returns>
        public string GenerateEditedFieldListForItemBrief(StageBitz.Data.ItemBrief itemBriefOriginal, StageBitz.Data.ItemBrief itemBriefTmp)
        {
            StringBuilder sbFields = new StringBuilder();
            StringCollection fieldList = new StringCollection();

            #region Compare fields

            if (!IsStringsEqual(itemBriefOriginal.Name, itemBriefTmp.Name))
                fieldList.Add("Name");
            if (itemBriefOriginal.Quantity != itemBriefTmp.Quantity)
                fieldList.Add("Quantity");
            if (itemBriefOriginal.DueDate != itemBriefTmp.DueDate)
                fieldList.Add("Due Date");
            if (itemBriefOriginal.Budget != itemBriefTmp.Budget)
                fieldList.Add("Budget");
            if (!IsStringsEqual(itemBriefOriginal.Description, itemBriefTmp.Description))
                fieldList.Add("Description");
            if (!IsStringsEqual(itemBriefOriginal.Brief, itemBriefTmp.Brief))
                fieldList.Add("Brief");
            if (!IsStringsEqual(itemBriefOriginal.Act, itemBriefTmp.Act))
                fieldList.Add("Act");
            if (!IsStringsEqual(itemBriefOriginal.Scene, itemBriefTmp.Scene))
                fieldList.Add("Scene");
            if (!IsStringsEqual(itemBriefOriginal.Page, itemBriefTmp.Page))
                fieldList.Add("Page");
            if (!IsStringsEqual(itemBriefOriginal.Category, itemBriefTmp.Category))
                fieldList.Add("Category");
            if (!IsStringsEqual(itemBriefOriginal.Character, itemBriefTmp.Character))
                fieldList.Add("Character");
            if (!IsStringsEqual(itemBriefOriginal.Preset, itemBriefTmp.Preset))
                fieldList.Add("Preset");
            if (!IsStringsEqual(itemBriefOriginal.Approver, itemBriefTmp.Approver))
                fieldList.Add("Approver");
            if (!IsStringsEqual(itemBriefOriginal.RehearsalItem, itemBriefTmp.RehearsalItem))
                fieldList.Add("Rehearsal Item");
            if (!IsStringsEqual(itemBriefOriginal.Usage, itemBriefTmp.Usage))
                fieldList.Add("Usage");
            if (!IsStringsEqual(itemBriefOriginal.Considerations, itemBriefTmp.Considerations))
                fieldList.Add("Notes");

            #endregion Compare fields

            #region Generate comma separated sentence

            if (fieldList.Count > 0)
            {
                for (int i = 0; i <= fieldList.Count - 2; i++)
                {
                    sbFields.Append(fieldList[i]);

                    if (i < fieldList.Count - 2)
                    {
                        sbFields.Append(", ");
                    }
                    else if (i == fieldList.Count - 2)
                    {
                        sbFields.Append(" and ");
                    }
                }

                sbFields.Append(fieldList[fieldList.Count - 1]);
            }

            #endregion Generate comma separated sentence

            return sbFields.ToString();
        }

        /// <summary>
        /// Determines whether [is strings equal] [the specified s1].
        /// </summary>
        /// <param name="s1">The s1.</param>
        /// <param name="s2">The s2.</param>
        /// <returns></returns>
        private bool IsStringsEqual(string s1, string s2)
        {
            if (s2 == null || (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)))
            {
                return true;
            }
            else
            {
                return s1 == s2;
            }
        }

        /// <summary>
        /// Gets the type of the item brief.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public ItemBriefType GetItemBriefType(int itemBriefId, int projectId)
        {
            int itemBriefTypePrimaryCodeId = Utils.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");

            return (from ibt in DataContext.ItemBriefTypes
                    join pit in DataContext.ProjectItemTypes on ibt.ItemTypeId equals pit.ItemTypeId
                    where ibt.ItemBriefId == itemBriefId
                        && ibt.ItemBriefTypeCodeId == itemBriefTypePrimaryCodeId
                        && pit.ProjectId == projectId
                    select ibt).FirstOrDefault();
        }

        /// <summary>
        /// Removes the item brief tasks.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        public void RemoveItemBriefTasks(int itemBriefId)
        {
            var taskListTasks = from ibt in DataContext.ItemBriefTasks
                                join tlibt in DataContext.TaskListsItemBriefTasks on ibt.ItemBriefTaskId equals tlibt.ItemBriefTaskId
                                where ibt.ItemBriefId == itemBriefId
                                select tlibt;

            foreach (Data.TaskListsItemBriefTask taskListTask in taskListTasks)
            {
                DataContext.TaskListsItemBriefTasks.DeleteObject(taskListTask);
            }
        }

        /// <summary>
        /// Saves the item brief.
        /// </summary>
        /// <param name="itemBriefDetails">The item brief details.</param>
        /// <returns></returns>
        public ItemBriefResulstObject SaveItemBrief(ItemBriefDetails itemBriefDetails)
        {
            ItemBriefResulstObject itemBriefResulstObject = new ItemBriefResulstObject();
            if (itemBriefDetails != null && itemBriefDetails.ItemBriefInfo != null)
            {
                ItemTypesBL itemTypesBL = new ItemTypesBL(DataContext);
                ProjectBL projectBL = new ProjectBL(DataContext);
                CompanyBL companyBL = new CompanyBL(DataContext);

                int userId = itemBriefDetails.UserId;
                int itemBriefId = itemBriefDetails.ItemBriefInfo.ItemBriefId;
                Data.ItemBrief itemBrief = GetItemBrief(itemBriefId);

                if (itemBrief != null)
                {
                    int projectId = itemBrief.ProjectId;
                    int companyId = itemBrief.Project.CompanyId;

                    if (!projectBL.ShouldStopProcessing("Project", projectId, userId))
                    {
                        //Check if the user can edit the ItemBrief
                        bool isReadOnlyRightsForProject = projectBL.IsReadOnlyRightsForProject(projectId, userId);
                        if (!isReadOnlyRightsForProject)
                        {
                            if (itemBriefDetails.ItemBriefInfo != null)
                            {
                                ItemBriefInfo itemBriefInfo = itemBriefDetails.ItemBriefInfo;

                                if (itemBriefInfo.LastUpdatedDate == itemBrief.LastUpdatedDate)
                                {
                                    if (itemBriefId > 0)
                                    {
                                        StageBitz.Data.ItemBrief tmpItemBrief = new Data.ItemBrief();

                                        #region Assign input values to the temporary item brief object

                                        tmpItemBrief.Name = itemBriefInfo.Name;
                                        tmpItemBrief.Quantity = itemBriefInfo.Quantity;
                                        if (!string.IsNullOrEmpty(itemBriefInfo.DueDate))
                                        {
                                            DateTime dueDate = Convert.ToDateTime(itemBriefInfo.DueDate);
                                            tmpItemBrief.DueDate = dueDate;
                                        }
                                        else
                                        {
                                            tmpItemBrief.DueDate = null;
                                        }
                                        tmpItemBrief.Budget = itemBriefInfo.Budget;
                                        tmpItemBrief.Description = itemBriefInfo.Description;
                                        tmpItemBrief.Brief = itemBriefInfo.Brief;
                                        tmpItemBrief.Act = itemBriefInfo.Act;
                                        tmpItemBrief.Scene = itemBriefInfo.Scene;
                                        tmpItemBrief.Page = itemBriefInfo.Page;
                                        tmpItemBrief.Category = itemBriefInfo.Category;
                                        tmpItemBrief.Character = itemBriefInfo.Character;
                                        tmpItemBrief.Preset = itemBriefInfo.Preset;
                                        tmpItemBrief.Approver = itemBriefInfo.Approver;
                                        tmpItemBrief.RehearsalItem = itemBriefInfo.RehearsalItem;
                                        tmpItemBrief.Usage = itemBriefInfo.Usage;
                                        tmpItemBrief.Considerations = itemBriefInfo.Considerations;

                                        #endregion Assign input values to the temporary item brief object

                                        #region Compare fields and copy the item brief object to save

                                        //Compare and generate the edited field list.
                                        string editedFieldList = GenerateEditedFieldListForItemBrief(itemBrief, tmpItemBrief);

                                        //Save the Notifications
                                        if (editedFieldList != string.Empty)
                                        {
                                            PersonalBL personalBL = new PersonalBL(DataContext);
                                            Data.User user = personalBL.GetUser(userId);
                                            NotificationBL notificationBL = new NotificationBL(DataContext);
                                            Data.Notification nf = new Data.Notification();
                                            nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
                                            nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "EDIT");
                                            nf.RelatedId = itemBrief.ItemBriefId;
                                            nf.ProjectId = projectId;
                                            nf.Message = string.Format("{0} edited the Item Brief - {1}.", personalBL.GetUserFullName(userId), editedFieldList);
                                            nf.CreatedByUserId = nf.LastUpdatedByUserId = userId;
                                            nf.CreatedDate = nf.LastUpdatedDate = Utils.Now;

                                            notificationBL.AddNotification(nf, false);
                                        }

                                        #endregion Compare fields and copy the item brief object to save

                                        itemBrief.Name = itemBriefInfo.Name;
                                        itemBrief.Page = itemBriefInfo.Page;
                                        itemBrief.Description = itemBriefInfo.Description;
                                        itemBrief.Preset = itemBriefInfo.Preset;
                                        itemBrief.Quantity = itemBriefInfo.Quantity;
                                        itemBrief.RehearsalItem = itemBriefInfo.RehearsalItem;
                                        itemBrief.Scene = itemBriefInfo.Scene;
                                        itemBrief.Usage = itemBriefInfo.Usage;
                                        itemBrief.Act = itemBriefInfo.Act;
                                        itemBrief.Approver = itemBriefInfo.Approver;
                                        itemBrief.Brief = itemBriefInfo.Brief;

                                        if (!string.IsNullOrEmpty(itemBriefInfo.DueDate))
                                        {
                                            DateTime dueDate = Convert.ToDateTime(itemBriefInfo.DueDate);
                                            itemBrief.DueDate = dueDate;
                                        }
                                        else
                                        {
                                            itemBrief.DueDate = null;
                                        }

                                        itemBrief.Budget = itemBriefInfo.Budget;
                                        itemBrief.Category = itemBriefInfo.Category;
                                        itemBrief.Character = itemBriefInfo.Character;
                                        itemBrief.Considerations = itemBriefInfo.Considerations;

                                        #region Save Item Type

                                        if (companyBL.IsCompanyAdministrator(companyId, userId) || projectBL.IsProjectAdministrator(projectId, userId))
                                        {
                                            var itemBriefType = GetItemBriefType(itemBriefId, projectId);

                                            if (itemBriefType != null && itemBriefType.ItemTypeId != itemBriefInfo.ItemBriefItemTypeId)
                                            {
                                                // Change item brief item type
                                                itemBriefType.ItemTypeId = itemBriefInfo.ItemBriefItemTypeId;

                                                // Remove item brief tasks for task list
                                                RemoveItemBriefTasks(itemBriefId);
                                            }
                                        }

                                        #endregion Save Item Type
                                    }
                                }
                                else
                                {
                                    itemBriefResulstObject.Status = "NOTOK";
                                    itemBriefResulstObject.Message = "Can not update ItemBrief due to conflicts.";
                                    return itemBriefResulstObject;
                                }

                                if (itemBriefDetails.ItemBriefValues != null)
                                {
                                    foreach (ValuesInfo ibValue in itemBriefDetails.ItemBriefValues)
                                    {
                                        if (ibValue.Id == 0)
                                        {
                                            //It is an Insert
                                            ItemBriefValue ibValueNew = new ItemBriefValue();
                                            ibValueNew.ItemBriefId = itemBriefId;
                                            ibValueNew.FieldId = ibValue.FieldId;
                                            ibValueNew.Value = ibValue.Value;
                                            ibValueNew.FieldOptionId = ibValue.FieldOptionId;
                                            DataContext.ItemBriefValues.AddObject(ibValueNew);
                                        }
                                        else
                                        {
                                            //Update
                                            ItemBriefValue itemBriefValue = itemTypesBL.GetItemBriefFieldValue(ibValue.Id);
                                            if (itemBriefValue != null)
                                            {
                                                if (ibValue.Value == string.Empty && ibValue.FieldOptionId == null)
                                                    DataContext.ItemBriefValues.DeleteObject(itemBriefValue);
                                                else
                                                {
                                                    itemBriefValue.FieldOptionId = ibValue.FieldOptionId;
                                                    itemBriefValue.Value = ibValue.Value;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (itemBriefDetails.ItemDetails != null && itemBriefDetails.ItemDetails.ItemId > 0 && itemBriefDetails.ItemDetails.CanEditInItemBrief)
                                {
                                    itemBriefResulstObject.ItemResultObject = itemTypesBL.SaveItemDetails(itemBriefDetails.ItemDetails);
                                }

                                //Has to be in the end because of field changes
                                itemBrief.LastUpdatedByUserId = userId;
                                itemBrief.LastUpdatedDate = Utils.Today;
                                DataContext.SaveChanges();
                                itemBriefResulstObject.Status = "OK";
                                itemBriefResulstObject.ItemBriefId = itemBriefId;
                            }
                        }
                        else
                        {
                            itemBriefResulstObject.Status = "NOTOK";
                            itemBriefResulstObject.Message = "Can not edit Item Brief.";
                        }
                    }
                    else
                    {
                        itemBriefResulstObject.Status = "STOPPROCESS";
                    }
                }
                else
                {
                    itemBriefResulstObject.Status = "NOTOK";
                    itemBriefResulstObject.Message = "Can not find Item Brief.";
                }
            }
            return itemBriefResulstObject;
        }

        /// <summary>
        /// Gets the item brief information by item brief.
        /// </summary>
        /// <param name="itemBrief">The item brief.</param>
        /// <returns></returns>
        public ItemBriefInfo GetItemBriefInfoByItemBrief(Data.ItemBrief itemBrief)
        {
            int itemBriefTypeId = GetItemBriefType(itemBrief.ItemBriefId).ItemTypeId;

            return new ItemBriefInfo
            {
                Act = itemBrief.Act,
                Approver = itemBrief.Approver,
                Brief = itemBrief.Brief,
                Budget = itemBrief.Budget,
                Category = itemBrief.Category,
                Character = itemBrief.Character,
                Considerations = itemBrief.Considerations,
                Description = itemBrief.Description,
                DueDate = itemBrief.DueDate.HasValue ? Utils.FormatDate(itemBrief.DueDate.Value) : string.Empty,
                ItemBriefId = itemBrief.ItemBriefId,
                LastUpdatedDate = itemBrief.LastUpdatedDate,
                Name = itemBrief.Name,
                ItemBriefItemTypeId = itemBriefTypeId,
                Page = itemBrief.Page,
                Preset = itemBrief.Preset,
                Quantity = itemBrief.Quantity,
                RehearsalItem = itemBrief.RehearsalItem,
                Scene = itemBrief.Scene,
                StatusCodeDescription = itemBrief.Code.Description,
                StatusCodeValue = itemBrief.Code.Value,
                Usage = itemBrief.Usage
            };
        }

        /// <summary>
        /// Determines whether this instance [can complete item brief] the specified item brief identifier.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        private bool CanCompleteItemBrief(int itemBriefId, int itemId)
        {
            Data.ItemBrief itemBrief = GetItemBrief(itemBriefId);
            if (itemBrief != null)
            {
                if (IsItemBriefComplete(itemBrief))
                {
                    return false;
                }

                InventoryBL inventoryBL = new InventoryBL(DataContext);
                List<ItemBooking> itemBookingList = inventoryBL.GetAllItemBookingByRelatedTable(itemBriefId, "ItemBrief", true);

                //If no item is Pinned, the user should be able to complete the ItemBrief.
                //if there are Pinned items exist and if they are yet being confirmed
                if (itemBookingList.Count() > 0 && itemBookingList.Where(ibi => ibi.ItemBookingStatusCodeId == Utils.GetCodeByValue("ItemBookingStatusCode", "PINNED").CodeId).Count() == itemBookingList.Count)
                {
                    return false;
                }
                else if (itemId > 0)
                {
                    if (itemBookingList.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        ItemBooking itembooking = inventoryBL.GetItemBookingByItemID(itemId, itemBriefId, "ItemBrief");
                        if (itembooking != null)
                        {
                            int delayedCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "DELAYED");

                            Code statusCode = inventoryBL.GetItemBookingStatus(itembooking.ItemBookingId);
                            if (statusCode != null && statusCode.CodeId == delayedCodeId)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the item brief list for report.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <returns></returns>
        public List<ItemBriefListInfo> GetItemBriefListForReport(int projectId, int itemTypeId)
        {
            int ItemBriefTaskInprogressStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefTaskStatusCode", "INPROGRESS");

            return (from ib in DataContext.ItemBriefs
                    from ibTasks in DataContext.ItemBriefTasks.Where(ibTasks => ib.ItemBriefId == ibTasks.ItemBriefId && ibTasks.EstimatedCost == null && ibTasks.ItemBriefTaskStatusCodeId == ItemBriefTaskInprogressStatusCodeId).DefaultIfEmpty().Take(1)
                    join statusCode in DataContext.Codes on ib.ItemBriefStatusCodeId equals statusCode.CodeId
                    join ibt in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibt.ItemBriefId
                    where ib.ProjectId == projectId && ibt.ItemTypeId == itemTypeId
                    orderby ib.Name
                    select new ItemBriefListInfo
                    {
                        ItemBrief = ib,
                        StatusSortOrder = statusCode.SortOrder,
                        Status = statusCode.Description,
                        IsEstimatedCostNullForActiveTask = (ibTasks != null)
                    }).ToList();
        }

        /// <summary>
        /// Completes the item brief.
        /// </summary>
        /// <param name="itemDetails">The item details.</param>
        /// <returns></returns>
        public ItemResultObject CompleteItemBrief(ItemDetails itemDetails)
        {
            ItemResultObject itemResultObject = new ItemResultObject();
            ProjectBL projectBL = new ProjectBL(DataContext);
            int projectId = 0; int companyId = 0;
            Data.ItemBrief itemBrief = GetItemBrief(itemDetails.ItemBriefId);
            if (itemBrief != null)
            {
                projectId = itemBrief.ProjectId;
                companyId = itemBrief.Project.CompanyId;
            }

            string relatedTable = "Company";
            int relatedId = companyId;

            if (itemDetails.RelatedTable == "Project")
            {
                relatedTable = itemDetails.RelatedTable;
                relatedId = projectId;
            }

            if (!projectBL.ShouldStopProcessing(relatedTable, relatedId, itemDetails.UserId))
            {
                if (!CanCompleteItemBrief(itemDetails.ItemBriefId, itemDetails.ItemId))
                {
                    //Send Error
                    itemResultObject.Status = "CONCURRENCY";
                    itemResultObject.ErrorCode = (int)ErrorCodes.NoEditPermissionForItemInItemBrief;
                    itemResultObject.Message = "Can not complete the ItemBrief";
                    return itemResultObject;
                }

                int inuseCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSE").CodeId;
                int inuseCompleteCodeId = Utils.GetCodeByValue("ItemBookingStatusCode", "INUSECOMPLETE").CodeId;

                InventoryBL inventoryBL = new InventoryBL(DataContext);
                FinanceBL financeBL = new FinanceBL(DataContext);
                ItemTypesBL itemTypesBL = new ItemTypesBL(DataContext);

                //Get already decided to select "kept" item or already "booked" item
                ItemBooking itemBooking = inventoryBL.GetInUseOrCompleteItemBooking(itemDetails.ItemBriefId);

                Data.Item item = null;
                if (itemBooking != null)
                {
                    item = itemBooking.Item;
                }

                //You can not complete an ItemBrief unless you have an InUse Item or if no Item associate at all
                bool IsNewItemCompletion = false;
                int itemId = 0;

                if (item == null || (itemBooking != null && (itemBooking.ItemBookingStatusCodeId == inuseCodeId)
                    || (itemBooking.ItemBookingStatusCodeId == inuseCompleteCodeId)))
                {
                    IsNewItemCompletion = (item == null);
                    itemId = (item == null) ? 0 : item.ItemId;
                }

                bool hasCompanyReachedInventoryLimit = financeBL.HasCompanyReachedInventoryLimit(companyId);

                if (IsNewItemCompletion)
                {
                    Nullable<int> iReturnValue;
                    if (hasCompanyReachedInventoryLimit) //if the company has reached the inventory limits the newly created item will be hidden
                    {
                        iReturnValue = DataContext.CompleteItemBrief(itemDetails.ItemBriefId, itemId, itemDetails.UserId, (itemDetails.DocumentMediaIds == null ? string.Empty : itemDetails.DocumentMediaIds), itemDetails.DefaultImageId, true).SingleOrDefault();
                    }
                    else
                    {
                        iReturnValue = DataContext.CompleteItemBrief(itemDetails.ItemBriefId, itemId, itemDetails.UserId, (itemDetails.DocumentMediaIds == null ? string.Empty : itemDetails.DocumentMediaIds), itemDetails.DefaultImageId, false).SingleOrDefault();
                    }

                    //Now Insert the dynamic values
                    if (iReturnValue.HasValue && iReturnValue.Value > 0)
                    {
                        int itemIdNew = (int)iReturnValue;
                        if (itemDetails.ItemValues != null)
                        {
                            foreach (ValuesInfo valuesInfo in itemDetails.ItemValues)
                            {
                                ItemValue newItemValue = new ItemValue();
                                newItemValue.ItemId = itemIdNew;
                                newItemValue.FieldId = valuesInfo.FieldId;
                                newItemValue.Value = valuesInfo.Value;
                                if (valuesInfo.FieldOptionId > 0)
                                    newItemValue.FieldOptionId = valuesInfo.FieldOptionId;
                                DataContext.ItemValues.AddObject(newItemValue);
                            }
                        }
                        itemDetails.ItemId = itemIdNew;
                        itemResultObject = itemTypesBL.SaveItemDetails(itemDetails, true);
                        if (itemResultObject.Status != "OK")
                        {
                            return itemResultObject;
                        }
                    }
                    else
                    {
                        itemResultObject.Status = "NOTOK";
                        itemResultObject.Message = "Item brief could not be completed";
                    }
                }
                else
                {
                    //update the status to Complete.
                    if (itemBrief != null)
                    {
                        itemBrief.ItemBriefStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBriefStatusCode", "COMPLETED");
                        itemBrief.LastUpdatedByUserId = itemDetails.UserId;
                        itemBrief.LastUpdatedDate = Utils.Today;
                        itemBooking.ItemBookingStatusCodeId = inuseCompleteCodeId;

                        if (itemDetails.CanEditInItemBrief)
                        {
                            itemResultObject = itemTypesBL.SaveItemDetails(itemDetails);
                            if (itemResultObject.Status != "OK")
                            {
                                return itemResultObject;
                            }
                        }

                        DataContext.RemoveOutDatedPinnedItemsFromItemBriefs(itemDetails.ItemId);
                    }

                    //if the item is hidden and if there is enough space in inventory, the update process will release the item to the inventory.

                    Data.Item itemInItemBrief = inventoryBL.GetItemBookingByRelatedTable(itemDetails.ItemBriefId, "ItemBrief", true).Item;

                    if (!hasCompanyReachedInventoryLimit && itemInItemBrief != null && itemInItemBrief.IsHidden)
                    {
                        itemInItemBrief.IsHidden = false;
                    }

                    if (itemDetails.ItemValues != null)
                    {
                        foreach (ValuesInfo valuesInfo in itemDetails.ItemValues)
                        {
                            if (valuesInfo.Id > 0)
                            {
                                ItemValue itemValue = DataContext.ItemValues.Where(iv => iv.ItemValueId == valuesInfo.Id).FirstOrDefault();
                                //Check if it is a Delete by the value
                                if (valuesInfo.Value == string.Empty && valuesInfo.FieldOptionId == 0)
                                {
                                    //Delete
                                    DataContext.ItemValues.DeleteObject(itemValue);
                                }
                                else
                                {
                                    //an Update
                                    itemValue.Value = valuesInfo.Value;
                                    itemValue.FieldOptionId = valuesInfo.FieldOptionId;
                                }
                            }
                            else
                            {
                                ItemValue newItemValue = new ItemValue();
                                newItemValue.ItemId = itemId;
                                newItemValue.FieldId = valuesInfo.FieldId;
                                newItemValue.Value = valuesInfo.Value;
                                if (valuesInfo.FieldOptionId > 0)
                                    newItemValue.FieldOptionId = valuesInfo.FieldOptionId;
                                DataContext.ItemValues.AddObject(newItemValue);
                            }
                        }
                    }
                }

                #region Generate 'Completed' Notification

                Data.Notification nf = new Data.Notification();
                nf.ModuleTypeCodeId = Utils.GetCodeIdByCodeValue("ModuleType", "ITEMBRIEF");
                nf.OperationTypeCodeId = Utils.GetCodeIdByCodeValue("OperationType", "EDIT");
                nf.RelatedId = itemDetails.ItemBriefId;
                nf.ProjectId = projectId;
                PersonalBL personalBL = new PersonalBL(DataContext);
                nf.Message = string.Format("{0} completed the Item Brief.", personalBL.GetUserFullName(itemDetails.UserId));
                nf.CreatedByUserId = nf.LastUpdatedByUserId = itemDetails.UserId;
                nf.CreatedDate = nf.LastUpdatedDate = Utils.Today;

                DataContext.Notifications.AddObject(nf);

                #endregion Generate 'Completed' Notification

                inventoryBL.SaveChanges();

                itemResultObject.Status = "OK";
                itemResultObject.ItemId = itemDetails.ItemId;
                return itemResultObject;
            }
            else
            {
                itemResultObject.Status = "STOPPROCESS";
                return itemResultObject;
            }
        }

        /// <summary>
        /// Gets the budget details.
        /// </summary>
        /// <param name="itemTypeId">The item type identifier.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public DataTable GetBudgetDetails(int itemTypeId, int projectId)
        {
            #region Private Constants

            const string colItemBriefID = "ItemBriefID";
            const string colItemType = "ItemType";
            const string colItemName = "ItemName";
            const string colBudget = "Budget";
            const string colExpended = "Expended";
            const string colRemaining = "Remaining";
            const string colBalance = "Balance";
            const string colIsEstimatedCostNull = "IsEstimatedCostNull";

            #endregion Private Constants

            ReportBL reportBL = new ReportBL(this.DataContext);

            var itemBriefTasksBudget = from ib in DataContext.ItemBriefs
                                       join ibt in DataContext.ItemBriefTasks on ib.ItemBriefId equals ibt.ItemBriefId
                                       join ibtyp in DataContext.ItemBriefTypes on ib.ItemBriefId equals ibtyp.ItemBriefId
                                       where ib.ProjectId == projectId && (itemTypeId == -1 || ibtyp.ItemTypeId == itemTypeId)
                                       select new { ib.ItemBriefId, ibt.EstimatedCost, TotalCost = (decimal?)(ibt.TotalCost), ibt.ItemBriefTaskStatusCodeId };

            var itemBriefBudget = reportBL.GetBudgetSummaryBodyDetails(projectId, itemTypeId);

            int completeStatusCodeID = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "COMPLETED").CodeId;
            int inprogressStatusCodeID = Utils.GetCodeByValue("ItemBriefTaskStatusCode", "INPROGRESS").CodeId;

            using (DataTable dtItems = new DataTable("tblItems"))
            {
                dtItems.Columns.Add(colItemBriefID);
                dtItems.Columns.Add(colItemType);
                dtItems.Columns.Add(colItemName);
                dtItems.Columns.Add(colBudget, typeof(decimal));
                dtItems.Columns.Add(colExpended, typeof(decimal));
                dtItems.Columns.Add(colRemaining, typeof(decimal));
                dtItems.Columns.Add(colBalance, typeof(decimal));
                dtItems.Columns.Add(colIsEstimatedCostNull, typeof(bool));

                foreach (Data.DataTypes.BudgetSummaryBody itemBrief in itemBriefBudget)
                {
                    var sumExpened = (from pb in itemBriefTasksBudget
                                      where pb.ItemBriefTaskStatusCodeId == completeStatusCodeID && pb.ItemBriefId == itemBrief.ItemBriefId
                                      select (decimal?)pb.TotalCost).Sum();

                    var sumRemaining = (from pb in itemBriefTasksBudget
                                        where pb.ItemBriefTaskStatusCodeId == inprogressStatusCodeID && pb.ItemBriefId == itemBrief.ItemBriefId
                                        select (decimal?)pb.EstimatedCost).Sum();

                    decimal? balance = (itemBrief.Budget == null ? 0 : itemBrief.Budget) - ((sumExpened == null ? 0 : sumExpened) + (sumRemaining == null ? 0 : sumRemaining));

                    DataRow dtRow = dtItems.NewRow();
                    dtRow[colItemBriefID] = itemBrief.ItemBriefId;
                    dtRow[colItemType] = itemBrief.ItemType;
                    dtRow[colItemName] = itemBrief.ItemName;
                    dtRow[colBudget] = itemBrief.Budget == null ? 0 : itemBrief.Budget;
                    dtRow[colExpended] = sumExpened == null ? 0 : sumExpened;
                    dtRow[colRemaining] = sumRemaining == null ? 0 : sumRemaining;
                    dtRow[colBalance] = balance;
                    dtRow[colIsEstimatedCostNull] = HasEmptyEstimateCostInItemBrief(itemBrief.ItemBriefId);
                    dtItems.Rows.Add(dtRow);
                }

                return dtItems;
            }
        }

        #endregion ItemBriefType
    }
}