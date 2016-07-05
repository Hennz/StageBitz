using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.IO;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.Project;
using StageBitz.Logic.Finance.Project;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace StageBitz.Logic.Business.Company
{
    /// <summary>
    /// Business layer for company
    /// </summary>
    public class CompanyBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public CompanyBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the company by item brief id.
        /// </summary>
        /// <param name="itemBriefId">The item brief identifier.</param>
        /// <returns></returns>
        public Data.Company GetCompanyByItemBriefId(int itemBriefId)
        {
            return (from ib in DataContext.ItemBriefs
                    join p in DataContext.Projects on ib.ProjectId equals p.ProjectId
                    join c in DataContext.Companies on p.CompanyId equals c.CompanyId
                    where ib.ItemBriefId == itemBriefId
                    select c).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether this company is suspendedby SB admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool HasCompanySuspendedbySBAdmin(int companyId)
        {
            return DataContext.Companies.Where(c => c.CompanyId == companyId && c.HasSuspended).FirstOrDefault() != null;
        }

        /// <summary>
        /// Determines whether payments are failed for specified company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool IsCompanyPaymentFailed(int companyId)
        {
            int paymentFailedCompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORPAYMENTFAILED");
            return DataContext.Companies.Where(c => c.CompanyId == companyId && c.CompanyStatusCodeId == paymentFailedCompanyStatusCodeId).FirstOrDefault() != null;
        }

        /// <summary>
        /// Determines whether payments are failed and specified company is in grace period.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool IsCompanyInPaymentFailedGracePeriod(int companyId)
        {
            int gracePeriodCompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "GRACEPERIOD");
            bool isPaymentFailedInvoiceExists = ProjectFinanceHandler.IsPaymentFailedInvoicesExistForCompany(companyId);

            return DataContext.Companies.Where(c => c.CompanyId == companyId && c.CompanyStatusCodeId == gracePeriodCompanyStatusCodeId).FirstOrDefault() != null && isPaymentFailedInvoiceExists;
        }

        /// <summary>
        /// Gets the company discount expire notified record.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public CompanyDiscountNotificatonHistory GetCompanyDiscountExpireNotifiedRecord(int companyId, StageBitzDB dataContext)
        {
            return dataContext.CompanyDiscountNotificatonHistories.Where(cdnh => cdnh.CompanyId == companyId && cdnh.IsActive).FirstOrDefault();
        }

        /// <summary>
        /// Updates the discount expire notified record for company.
        /// </summary>
        /// <param name="discountCodeUsage">The discount code usage.</param>
        /// <param name="company">The company.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dataContext">The data context.</param>
        public void UpdateDiscountExpireNotifiedRecordForCompany(DiscountCodeUsage discountCodeUsage, Data.Company company, int userId, StageBitzDB dataContext)
        {
            CompanyDiscountNotificatonHistory companyDiscountNotificatonHistory = GetCompanyDiscountExpireNotifiedRecord(company.CompanyId, DataContext);

            if (companyDiscountNotificatonHistory != null)
            {
                // If it is a 100% discount code
                if (discountCodeUsage != null && discountCodeUsage.DiscountCode.Discount == 100)
                {
                    companyDiscountNotificatonHistory.IsActive = false;
                    companyDiscountNotificatonHistory.LastUpdatedDate = Utils.Today;
                    companyDiscountNotificatonHistory.LastUpdatedByUserId = userId;

                    // Also set the Company Status to Active if suspended;
                    if (company.CompanyStatusCodeId == Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTOPTIONS"))
                    {
                        company.CompanyStatusCodeId = Utils.GetCodeIdByCodeValue("CompanyStatus", "ACTIVE");
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether company is suspended.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="shouldConsiderSBAdminSuspention">if set to <c>true</c> [should consider sb admin suspention].</param>
        /// <returns></returns>
        public bool IsCompanySuspended(int companyId, bool shouldConsiderSBAdminSuspention = false)
        {
            List<int> suspendedCompanyStatusList = this.GetSuspendedCompanyStatusList();

            return DataContext.Companies.Where(c => c.CompanyId == companyId &&
                (suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId) || (shouldConsiderSBAdminSuspention && c.HasSuspended))).FirstOrDefault() != null;
        }

        /// <summary>
        /// Gets the suspended company status code id list.
        /// </summary>
        /// <returns></returns>
        public List<int> GetSuspendedCompanyStatusList()
        {
            List<int> suspendedCompanyStatusList = new List<int>();
            suspendedCompanyStatusList.Add(Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTOPTIONS"));
            suspendedCompanyStatusList.Add(Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORPAYMENTFAILED"));
            suspendedCompanyStatusList.Add(Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORNOPAYMENTPACKAGE"));
            return suspendedCompanyStatusList;
        }

        /// <summary>
        /// Suspends the company by SB admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void SuspendCompanybySBAdmin(int companyId, int userId)
        {
            Data.Company company = DataContext.Companies.Where(c => c.CompanyId == companyId).FirstOrDefault();

            if (company != null)
            {
                company.HasSuspended = true;
                ProjectUsageHandler.UpdatePaymentSummaryForFreeTrialCompanyBySBAdmin(companyId, null, true, userId, DataContext);
            }

            SuspendActiveProjectsByCompany(companyId, userId);
        }

        /// <summary>
        /// Suspends the active projects by company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void SuspendActiveProjectsByCompany(int companyId, int userId)
        {
            int projectSuspendedCode = Utils.GetCodeIdByCodeValue("ProjectStatus", "SUSPENDED");
            int projectActiveCode = Utils.GetCodeIdByCodeValue("ProjectStatus", "ACTIVE");
            int projectFreeTrialCode = Utils.GetCodeIdByCodeValue("ProjectStatus", "FREETRIAL");
            int projectGracePeriodCode = Utils.GetCodeIdByCodeValue("ProjectStatus", "GRACEPERIOD");
            int projectPaymentFailedCode = Utils.GetCodeIdByCodeValue("ProjectStatus", "PAYMENTFAILED");

            List<Data.Project> projects = (from p in DataContext.Projects
                                           where p.CompanyId == companyId &&
                                           (p.ProjectStatusCodeId == projectActiveCode || p.ProjectStatusCodeId == projectFreeTrialCode
                                                || p.ProjectStatusCodeId == projectGracePeriodCode || p.ProjectStatusCodeId == projectPaymentFailedCode)
                                           select p).ToList<Data.Project>();

            foreach (Data.Project project in projects)
            {
                project.ProjectStatusCodeId = projectSuspendedCode;
                // ProjectUsageHandler.UpdateFutureDailyUsageProjectState(project, false, userId, projectSuspendedCode, Utils.Today);
                project.LastUpdatedByUserId = userId;
                project.LastUpdatedDate = Utils.Now;
            }

            DataContext.SaveChanges();
        }

        /// <summary>
        /// Reactivates the company by SB admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void ReactivateCompanybySBAdmin(int companyId, int userId)
        {
            Data.Company company = DataContext.Companies.Where(c => c.CompanyId == companyId).FirstOrDefault();
            if (company != null)
            {
                company.HasSuspended = false;
                // This only gets executed by FT company.
                ProjectUsageHandler.UpdatePaymentSummaryForFreeTrialCompanyBySBAdmin(companyId, null, false, userId, DataContext);
                // For a FT company, this does not affects, but for a normal company.
                ActivateUnProcessedSummaries(companyId, userId);
            }
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Activates the unprocessed summaries.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void ActivateUnProcessedSummaries(int companyId, int userId)
        {
            // if there are summaries which were set to should not process if the company is reactivated or payment made sucess during this period
            // these records will be activated for that period.
            CompanyPaymentSummary companyPackageSummary = (from cps in DataContext.CompanyPaymentSummaries
                                                           where cps.IsMonthlyAgentProcessed == false && cps.ShouldProcess == false
                                                           && cps.CompanyId == companyId && cps.FromDate <= Utils.Today && cps.ToDate >= Utils.Today
                                                           select cps).OrderByDescending(cps => cps.CompanyPaymentSummaryId).FirstOrDefault();
            if (companyPackageSummary != null)
            {
                companyPackageSummary.ShouldProcess = true;
                companyPackageSummary.LastUpdatedBy = userId;
                companyPackageSummary.LastUpdatedDate = Utils.Today;
            }
        }

        /// <summary>
        /// Returns whether the currently logged in user is an Administrator of the specified Company.
        /// </summary>
        public bool IsCompanyAdministrator(int companyID, int userId)
        {
            int companyAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "SECADMIN").CodeId;
            int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;

            CompanyUser companyUser = (from cu in DataContext.CompanyUsers
                                       join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                       where cu.UserId == userId && cu.CompanyId == companyID && cu.IsActive && cur.IsActive
                                            && (cur.CompanyUserTypeCodeId == companyAdminCodeID || cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID)
                                       select cu).FirstOrDefault<CompanyUser>();
            if (companyUser == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the company inventory admin.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public User GetCompanyInventoryAdmin(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "INVADMIN").CodeId;

                return (from cu in dataContext.CompanyUsers
                        join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                        where cu.CompanyId == companyId && cu.IsActive == true && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                        select cu.User).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the company primary administrator.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public StageBitz.Data.User GetCompanyPrimaryAdministrator(int companyId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                int companyPrimaryAdminCodeID = Utils.GetCodeByValue("CompanyUserTypeCode", "ADMIN").CodeId;

                CompanyUser companyUser = (from cu in dataContext.CompanyUsers
                                           join cur in dataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                           where cu.CompanyId == companyId && cu.IsActive == true && cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID && cur.IsActive
                                           select cu).SingleOrDefault<CompanyUser>();

                return companyUser.User;
            }
        }

        /// <summary>
        /// Determines whether [has edit permission for inventory staff].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public bool HasEditPermissionForInventoryStaff(int companyId, int userId, int? locationId)
        {
            FinanceBL financeBL = new FinanceBL(DataContext);
            InventoryBL inventoryBL = new InventoryBL(DataContext);

            bool hasPackageSelected = financeBL.HasPackageSelectedForFreeTrailEndedCompany(companyId);

            return (Utils.IsCompanyInventoryAdmin(companyId, userId) ||
                        (!locationId.HasValue && inventoryBL.IsCompanyInventoryStaffMemberAnyLocation(companyId, userId)) ||
                        (locationId.HasValue && Utils.IsCompanyInventoryStaffMember(companyId, userId, locationId, DataContext)))
                    && !this.HasCompanySuspendedbySBAdmin(companyId) && hasPackageSelected && !IsCompanySuspended(companyId);
        }

        /// <summary>
        /// Gets the company list of given user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="shouldIncludeSuspendedCompanies">if set to <c>true</c> [should include suspended companies].</param>
        /// <param name="shouldConsiderPendingCompanies">if set to <c>true</c> [should consider pending companies].</param>
        /// <param name="shouldIncludeActiveProjectsOnly">if set to <c>true</c> [should include active projects only].</param>
        /// <returns></returns>
        public List<CompanyListInfo> GetCompanyList(int userId, bool shouldIncludeSuspendedCompanies = true, bool shouldConsiderPendingCompanies = true, bool shouldIncludeActiveProjectsOnly = false)
        {
            int companyAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "SECADMIN");
            int companyPrimaryAdminCodeID = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "ADMIN");
            int inventoryStaffCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVSTAFF");
            int inventoryAdminCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "INVADMIN");
            int locationManagerCodeId = Utils.GetCodeIdByCodeValue("CompanyUserTypeCode", "LOCATIONMANAGER");

            int invitationStatusPendingCodeID = Utils.GetCodeIdByCodeValue("InvitationStatus", "PENDING");
            int closedProjectStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "CLOSED");
            int gracePeriodStatusCodeId = Utils.GetCodeByValue("ProjectStatus", "GRACEPERIOD").CodeId;
            int ftProjectStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "FREETRIAL");
            int activeProjectStatusCodeId = Utils.GetCodeIdByCodeValue("ProjectStatus", "ACTIVE");
            List<int> suspendedCompanyStatusList = this.GetSuspendedCompanyStatusList();

            // this takes the list of companies which the user is acting as an administrator or inventory staff
            List<CompanyListInfo> companies = (from c in DataContext.Companies
                                               join cu in DataContext.CompanyUsers on c.CompanyId equals cu.CompanyId
                                               join cur in DataContext.CompanyUserRoles on cu.CompanyUserId equals cur.CompanyUserId
                                               where cu.UserId == userId && cur.IsActive
                                                     && (shouldIncludeSuspendedCompanies || (!suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId) && c.HasSuspended == false))
                                               orderby c.CompanyName
                                               group new { c.CompanyName, cur } by c.CompanyId into grp
                                               select new CompanyListInfo
                                               {
                                                   CompanyId = grp.Key,
                                                   CompanyName = grp.FirstOrDefault().CompanyName,
                                                   InvitationId = 0,
                                                   IsCompanyUser = grp.Max(gx => (gx.cur.CompanyUserTypeCodeId == companyAdminCodeID || gx.cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID) ? 1 : 0) == 1,
                                                   IsInventoryStaff = grp.Max(gx => (gx.cur.CompanyUserTypeCodeId == companyAdminCodeID || 
                                                       gx.cur.CompanyUserTypeCodeId == companyPrimaryAdminCodeID || gx.cur.CompanyUserTypeCodeId == inventoryStaffCodeId ||
                                                       gx.cur.CompanyUserTypeCodeId == inventoryAdminCodeId || gx.cur.CompanyUserTypeCodeId == locationManagerCodeId) ? 1 : 0) == 1,
                                               }).ToList<CompanyListInfo>();

            // this takes the list of companies which the user is acting as a project user
            List<CompanyListInfo> projectCompanies = (from p in DataContext.Projects
                                                      join pu in DataContext.ProjectUsers on p.ProjectId equals pu.ProjectId
                                                      join c in DataContext.Companies on p.CompanyId equals c.CompanyId
                                                      where pu.UserId == userId && pu.IsActive && p.ProjectStatusCodeId != closedProjectStatusCodeId &&
                                                      (!shouldIncludeActiveProjectsOnly || (p.ProjectStatusCodeId == gracePeriodStatusCodeId || p.ProjectStatusCodeId == ftProjectStatusCodeId
                                                      || p.ProjectStatusCodeId == activeProjectStatusCodeId))
                                                      && (shouldIncludeSuspendedCompanies || (!suspendedCompanyStatusList.Contains(c.CompanyStatusCodeId) && c.HasSuspended == false))
                                                      select new CompanyListInfo
                                                      {
                                                          CompanyId = c.CompanyId,
                                                          CompanyName = c.CompanyName,
                                                          InvitationId = 0,
                                                          IsCompanyUser = false,
                                                          ProjectId = pu.ProjectId,
                                                          IsInventoryStaff = false
                                                      }).Distinct().ToList<CompanyListInfo>();

            List<CompanyListInfo> allActiveCompanies = companies.Union(projectCompanies).ToList<CompanyListInfo>().Distinct(CompanyListInfoComparer.Instance).ToList<CompanyListInfo>();

            if (shouldConsiderPendingCompanies)
            {
                // this will take the company invitations count for the user
                List<CompanyListInfo> companiesPending = (from i in DataContext.Invitations
                                                          join c in DataContext.Companies on i.RelatedId equals c.CompanyId
                                                          where i.RelatedTable == StageBitz.Common.Constants.GlobalConstants.RelatedTables.UserRoleTypes.Companies
                                                            && i.ToUserId == userId && i.InvitationStatusCodeId == invitationStatusPendingCodeID
                                                          orderby i.CreatedDate
                                                          select new CompanyListInfo
                                                          {
                                                              CompanyId = c.CompanyId,
                                                              CompanyName = c.CompanyName,
                                                              InvitationId = i.InvitationId,
                                                          }).ToList<CompanyListInfo>();

                return allActiveCompanies.Union(companiesPending).ToList<CompanyListInfo>();
            }
            else
            {
                return allActiveCompanies;
            }
        }

        /// <summary>
        /// Gets the company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Data.Company GetCompany(int companyId)
        {
            return (from c in DataContext.Companies where c.CompanyId == companyId select c).FirstOrDefault();
        }

        /// <summary>
        /// Gets the inventory activity data.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<InventoryActivityData> GetInventoryActivityData(int companyId, DateTime startDate, DateTime endDate)
        {
            List<InventoryActivityData> inventoryActivityData =
                    (from it in DataContext.ItemTypes
                     select new InventoryActivityData
                     {
                         ItemType = it.Name,
                         Quantity = (from i in DataContext.Items
                                     where i.CompanyId == companyId && i.IsActive && i.CreatedDate <= endDate && i.ItemTypeId == it.ItemTypeId
                                     select i).Count(),
                         ManuallyAdded = (from i in DataContext.Items
                                          where i.CompanyId == companyId && i.IsActive && i.CreatedDate <= endDate && i.ItemTypeId == it.ItemTypeId && i.IsManuallyAdded
                                          select i).Count(),
                         CreatedInProject = (from i in DataContext.Items
                                             where i.CompanyId == companyId && i.IsActive && i.CreatedDate <= endDate && i.ItemTypeId == it.ItemTypeId && !i.IsManuallyAdded
                                             select i).Count(),
                         Booked = (from ia in DataContext.InventoryActivities
                                   join ibs in DataContext.ItemBookings on ia.ItemBookingId equals ibs.ItemBookingId
                                   where ibs.Item.CompanyId == companyId && ibs.Item.ItemTypeId == it.ItemTypeId && ibs.Item.IsActive && !ibs.Item.IsHidden &&
                                   ((ia.StartDate >= startDate && ia.StartDate <= endDate) || (ia.StartDate < startDate && (ia.EndDate >= startDate || ia.EndDate == null)))
                                   select ia).Count()
                     }).ToList<InventoryActivityData>();

            InventoryActivityData allItemType = new InventoryActivityData { ItemType = "All", Quantity = inventoryActivityData.Sum(iad => iad.Quantity), ManuallyAdded = inventoryActivityData.Sum(iad => iad.ManuallyAdded), CreatedInProject = inventoryActivityData.Sum(iad => iad.CreatedInProject), Booked = inventoryActivityData.Sum(iad => iad.Booked) };
            inventoryActivityData.Insert(0, allItemType);

            return inventoryActivityData;
        }

        /// <summary>
        /// Gets the company inventory details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<CompanyInventoryDetails> GetCompanyInventoryDetails(int companyId)
        {
            int pinnedItemBriefStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "PINNED");
            int inUseItemBriefStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSE");
            int inUseCompleteItemBriefStatusCodeId = Utils.GetCodeIdByCodeValue("ItemBookingStatusCode", "INUSECOMPLETE");

            List<CompanyInventoryDetails> companyInventoryDetails =
                (from it in DataContext.ItemTypes
                 select new
                 {
                     ItemType = it.Name,
                     TotalItems = (from i in DataContext.Items where i.CompanyId == companyId && i.IsActive && !i.IsHidden && i.ItemTypeId == it.ItemTypeId select i).Count(),
                     PinnedItems = (from ibs in DataContext.ItemBookings
                                    where ibs.Item.CompanyId == companyId && ibs.IsActive && ibs.Item.ItemTypeId == it.ItemTypeId && ibs.ItemBookingStatusCodeId == pinnedItemBriefStatusCodeId
                                    select ibs).Count(),
                     InUseItems = (from ibs in DataContext.ItemBookings
                                   where ibs.Item.CompanyId == companyId && ibs.IsActive && ibs.Item.ItemTypeId == it.ItemTypeId &&
                                   (ibs.ItemBookingStatusCodeId == inUseItemBriefStatusCodeId || ibs.ItemBookingStatusCodeId == inUseCompleteItemBriefStatusCodeId)
                                   select ibs).Count()
                 }
                     into tempInventoryDetails
                     where tempInventoryDetails.TotalItems > 0  // remove item types which dosn't have any items
                     select new CompanyInventoryDetails
                     {
                         ItemType = tempInventoryDetails.ItemType,
                         TotalItems = tempInventoryDetails.TotalItems,
                         PinnedItems = tempInventoryDetails.PinnedItems,
                         InUseItems = tempInventoryDetails.InUseItems,
                         AvailableItems = tempInventoryDetails.TotalItems - tempInventoryDetails.InUseItems - tempInventoryDetails.PinnedItems
                     }
                 ).ToList<CompanyInventoryDetails>();

            return companyInventoryDetails;
        }

        /// <summary>
        /// Gets the company user roles.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<CompanyUserRole> GetCompanyUserRoles(int userId, int companyId)
        {
            List<CompanyUserRole> companyUserRoles = (from cur in DataContext.CompanyUserRoles
                                                      join cu in DataContext.CompanyUsers on cur.CompanyUserId equals cu.CompanyUserId
                                                      where cu.UserId == userId && cu.CompanyId == companyId && cur.IsActive && cu.IsActive
                                                      select cur).ToList<CompanyUserRole>();
            return companyUserRoles;
        }

        /// <summary>
        /// Gets the company profile image id.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public int GetCompanyProfileImageId(int companyId)
        {
            return (from m in DataContext.DocumentMedias
                    where m.RelatedTableName == "Company" && m.RelatedId == companyId && m.SortOrder == 1
                    select m.DocumentMediaId).FirstOrDefault();
        }

        /// <summary>
        /// Adds the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        public void AddCompany(Data.Company company, bool commit)
        {
            DataContext.Companies.AddObject(company);
            if (commit)
            {
                base.SaveChanges();
            }
        }

        /// <summary>
        /// Determines whether free trial company for current date.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>
        ///   <c>true</c> if [is free trial company] [the specified company id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFreeTrialCompany(int companyId)
        {
            return this.IsFreeTrialCompany(companyId, Utils.Now);
        }

        /// <summary>
        /// Determines whether payments are failed for specified company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool IsPaymentFailed(int companyId)
        {
            return (from c in DataContext.Companies
                    where c.CompanyStatusCodeId == Utils.GetCodeIdByCodeValue("CompanyStatus", "SUSPENDEDFORPAYMENTFAILED")
                    && c.CompanyId == companyId
                    select c).Count() > 0;
        }

        /// <summary>
        /// Determines whether free trial company.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="time">The time.</param>
        /// <returns>
        ///   <c>true</c> if [is free trial company] [the specified company id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFreeTrialCompany(int companyId, DateTime time)
        {
            ProjectBL projectBL = new ProjectBL(DataContext);
            return projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId, time).Count > 0;
        }

        /// <summary>
        /// Determines whether free trial ended company.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="date">The date.</param>
        /// <returns>
        ///   <c>true</c> if [is free trial ended company] [the specified company id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFreeTrialEndedCompany(int companyId)
        {
            ProjectBL projectBL = new ProjectBL(DataContext);

            if (this.IsFreeTrialCompany(companyId))
            {
                List<Data.Project> freeTrailProjects = projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId);

                return projectBL.GetFreeTrialEndedProjects(companyId).Count > 0 && freeTrailProjects.Count == 0;
            }
            else
            {
                return projectBL.GetFreeTrialEndedProjects(companyId).Count > 0;
            }
        }

        /// <summary>
        /// Determines whether free trial status included for the given date.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <returns></returns>
        public bool IsFreeTrialStatusIncludedFortheDay(int companyId, DateTime dateToConsider)
        {
            // this will return whether the company is in free trial including the last hour of the free trial day. At the last day after 11 pm the free trial gets over. Is Free Trial Company returns false.
            // But we need to return true during this period. The additional checking is done for this matter.
            FinanceBL financeBL = new FinanceBL(DataContext);
            var currentPackage = financeBL.GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(companyId);
            return (this.IsFreeTrialCompany(companyId) || currentPackage != null && (this.IsFreeTrialEndedCompany(companyId) && currentPackage.StartDate == dateToConsider.AddDays(1)));
        }

        /// <summary>
        /// Gets the free trial project end date.
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>Free Trial Project end date</returns>
        public DateTime GetFreeTrialProjectEndDate(int companyId)
        {
            ProjectBL projectBL = new ProjectBL(DataContext);
            var freeTrialProjects = projectBL.GetFreeTrialProjectsNotInClosedStatus(companyId);
            Data.Project freeTrialProject = freeTrialProjects.FirstOrDefault();

            if (freeTrialProject != null && freeTrialProject.ExpirationDate.HasValue)
            {
                return freeTrialProject.ExpirationDate.Value;
            }

            return Utils.Today;
        }

        /// <summary>
        /// Gets the export file details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public List<ExportFileDetails> GetExportFileDetails(int companyId)
        {
            int deletedExportFileStatusCodeId = Utils.GetCodeByValue("ExportFileStatus", "DELETED").CodeId;
            var exportfileInfor = (from p in DataContext.Projects
                                   from ef in DataContext.ExportFiles.Where(ef => ef.RelatedTable == "Project" && ef.RelatedId == p.ProjectId && ef.IsActive && ef.ExportFileStatusCodeId != deletedExportFileStatusCodeId).DefaultIfEmpty().Take(1)
                                   where p.CompanyId == companyId
                                   select new ExportFileDetails
                                   {
                                       SortOrder = 1,
                                       ExportFileId = ef != null ? ef.ExportFileId : 0,
                                       EntityName = p.ProjectName,
                                       RelatedId = p.ProjectId,
                                       RelatedTable = "Project",
                                       ExportFileStatusCodeId = ef != null ? ef.ExportFileStatusCodeId : 0,
                                       FileSize = ef != null ? ef.FileSize : 0
                                   })
                                   .Concat(
                                   from c in DataContext.Companies
                                   from ef in DataContext.ExportFiles.Where(ef => ef.RelatedTable == "Company" && ef.RelatedId == c.CompanyId && ef.IsActive && ef.ExportFileStatusCodeId != deletedExportFileStatusCodeId).DefaultIfEmpty().Take(1)
                                   where c.CompanyId == companyId
                                   select new ExportFileDetails
                                   {
                                       SortOrder = 0,
                                       ExportFileId = ef != null ? ef.ExportFileId : 0,
                                       EntityName = "Company Inventory",
                                       RelatedId = c.CompanyId,
                                       RelatedTable = "Company",
                                       ExportFileStatusCodeId = ef != null ? ef.ExportFileStatusCodeId : 0,
                                       FileSize = ef != null ? ef.FileSize : 0
                                   }).OrderBy(ef => ef.SortOrder).ThenBy(ef => ef.EntityName).ToList();

            return exportfileInfor;
        }

        /// <summary>
        /// Generates the export files.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void GenerateExportFiles(string relatedTable, int relatedId, int userId)
        {
            int deletedExpotFileStatusCodeId = Utils.GetCodeByValue("ExportFileStatus", "DELETED").CodeId;
            // check if a record already exisit
            Data.ExportFile exportFile = DataContext.ExportFiles.Where(ef => ef.RelatedId == relatedId &&
                ef.RelatedTable == relatedTable && ef.IsActive
                && ef.ExportFileStatusCodeId != deletedExpotFileStatusCodeId).FirstOrDefault();
            if (exportFile == null)
            {
                exportFile = new ExportFile
                {
                    RelatedTable = relatedTable,
                    RelatedId = relatedId,
                    ExportFileStatusCodeId = Utils.GetCodeByValue("ExportFileStatus", "QUEUED").CodeId,
                    CreatedByUserId = userId,
                    LastUpdatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedDate = Utils.Now,
                    IsActive = true
                };
                DataContext.ExportFiles.AddObject(exportFile);
                DataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the exported file.
        /// </summary>
        /// <param name="exportFileId">The export file identifier.</param>
        /// <returns></returns>
        public ExportFile GetExportedFile(int exportFileId)
        {
            return DataContext.ExportFiles.Where(ef => ef.ExportFileId == exportFileId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the export file location.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="ShouldIncludeZipExtension">if set to <c>true</c> [should include zip extension].</param>
        /// <param name="downLoadFileName">Name of down load file.</param>
        /// <returns></returns>
        public string GetExportFileLocation(string relatedTable, int relatedId, bool ShouldIncludeZipExtension, out string downLoadFileName)
        {
            string baseFolder = string.Empty;

            string basePath = Utils.GetSystemValue("ExportFileDirectoryLocation");

            switch (relatedTable)
            {
                case Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Project:
                    ProjectBL projectBL = new ProjectBL(DataContext);
                    Data.Project project = projectBL.GetProject(relatedId);

                    if (project != null)
                    {
                        downLoadFileName = string.Concat(FileHandler.GetSafeFileName(Utils.Ellipsize(project.ProjectName, 50)), ".zip");
                        baseFolder = Path.Combine(
                            basePath,
                            project.CompanyId.ToString(CultureInfo.InvariantCulture),
                            string.Concat("P-",
                            !ShouldIncludeZipExtension ? project.ProjectId.ToString(CultureInfo.InvariantCulture) : string.Concat(project.ProjectId, ".zip")));
                        return baseFolder;
                    }
                    break;

                case Common.Constants.GlobalConstants.RelatedTables.ExportFiles.Company:
                    CompanyBL companyBL = new CompanyBL(DataContext);
                    Data.Company company = companyBL.GetCompany(relatedId);

                    if (company != null)
                    {
                        downLoadFileName = string.Concat(string.Format("{0} Inventory", FileHandler.GetSafeFileName(Utils.Ellipsize(company.CompanyName, 50))), ".zip");
                        baseFolder = Path.Combine(
                                basePath,
                                relatedId.ToString(CultureInfo.InvariantCulture),
                                string.Concat("C-",
                                !ShouldIncludeZipExtension ? company.CompanyId.ToString(CultureInfo.InvariantCulture) : string.Concat(company.CompanyId, ".zip")));
                        return baseFolder;
                    }
                    break;
            }

            downLoadFileName = string.Empty;
            return baseFolder;
        }

        /// <summary>
        /// Gets the export file location.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public string GetExportFileLocation(string relatedTable, int relatedId)
        {
            string fileName = string.Empty;
            return GetExportFileLocation(relatedTable, relatedId, false, out fileName);
        }

        /// <summary>
        /// Deletes the exported file.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteExportedFile(string relatedTable, int relatedId, int userId)
        {
            int deletedExpotFileStatusCodeId = Utils.GetCodeByValue("ExportFileStatus", "DELETED").CodeId;
            // check if a record already exisit
            Data.ExportFile exportFile = DataContext.ExportFiles.Where(ef => ef.RelatedId == relatedId
                && ef.RelatedTable == relatedTable
                && ef.IsActive && ef.ExportFileStatusCodeId != deletedExpotFileStatusCodeId).FirstOrDefault();
            if (exportFile != null)
            {
                //IsActive is not set to false. It get set after the agent physically delete the file
                exportFile.ExportFileStatusCodeId = Utils.GetCodeByValue("ExportFileStatus", "DELETED").CodeId;
                exportFile.LastUpdatedByUserId = userId;
                exportFile.LastUpdatedDate = Utils.Now;
                DataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Removes the generated export files.
        /// </summary>
        public void RemoveGeneratedExportFiles()
        {
            //Get the directory name where the files reside
            var exportFiles = DataContext.ExportFiles.Where(ef => ef.IsActive).ToList<ExportFile>();
            foreach (ExportFile exportFile in exportFiles)
            {
                exportFile.IsActive = false;
                //Build the file URl to locate
            }
        }

        /// <summary>
        /// Gets all queued export file requests.
        /// </summary>
        /// <returns>Queued export file requests.</returns>
        public IEnumerable<Data.ExportFile> GetAllQueuedExportFileRequests()
        {
            //check if a record already exisit
            int completedStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "COMPLETED");
            return DataContext.ExportFiles.Where(ef => ef.ExportFileStatusCodeId != completedStatusCodeId && ef.IsActive);
        }

        /// <summary>
        /// Gets all pending delete exported files after a defined period.
        /// </summary>
        /// <returns>pending delete file requests.</returns>
        public IEnumerable<Data.ExportFile> GetGeneratedOldExportFiles()
        {
            //check if a record already exisit
            int completedStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "COMPLETED");
            int exportedFilesRemainingDays = int.Parse(Utils.GetSystemValue("ExportedFilesRemainingDays"));
            //Get completed records that are older than the defined days in "ExportedFilesRemainingDays" as a systemvalue

            return DataContext.ExportFiles.Where(ef => ef.ExportFileStatusCodeId == completedStatusCodeId &&
                ef.IsActive && EntityFunctions.DiffDays(EntityFunctions.AddDays(ef.CreatedDate, exportedFilesRemainingDays), Utils.Now) >= 0);
        }

        /// <summary>
        /// Gets the company user by user identifier and company identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Data.CompanyUser GetCompanyUserByUserIdAndCompanyId(int userId, int companyId)
        {
            return DataContext.CompanyUsers.Where(cu => cu.CompanyId == companyId && cu.UserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the days difference.
        /// </summary>
        public void GetDaysDiff()
        {
            int exportedFilesRemainingDays = int.Parse(Utils.GetSystemValue("ExportedFilesRemainingDays"));
            int completedStatusCodeId = Utils.GetCodeIdByCodeValue("ExportFileStatus", "COMPLETED");
            var x = (from ef in DataContext.ExportFiles
                     where ef.ExportFileStatusCodeId == completedStatusCodeId && ef.IsActive
                     select new
                    {
                        cd = ef.CreatedDate,
                        ad = EntityFunctions.AddDays(ef.CreatedDate, exportedFilesRemainingDays),
                        x = (EntityFunctions.DiffDays(EntityFunctions.AddDays(ef.CreatedDate, exportedFilesRemainingDays), Utils.Now))
                    });
        }

        /// <summary>
        /// Gets the item type document media by company.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<ItemTypeDocumentMedia> GetItemTypeDocumentMediaByCompany(int companyId)
        {
            return (from i in DataContext.Items.Where(i => i.CompanyId == companyId)
                    join it in DataContext.ItemTypes on i.ItemTypeId equals it.ItemTypeId
                    from dm in DataContext.DocumentMedias.Where(dm => dm.RelatedTableName == "Item" && dm.RelatedId == i.ItemId && dm.DocumentMediaContent != null).DefaultIfEmpty()
                    group new
                    {
                        DocumentMediaId = dm != null ? dm.DocumentMediaId : 0,
                        ItemTypeName = it.Name,
                        ItemTypeId = it.ItemTypeId,
                        ItemName = i.Name,
                        ItemId = i.ItemId
                    } by it.ItemTypeId into grp
                    select new ItemTypeDocumentMedia
                    {
                        ItemTypeName = grp.FirstOrDefault().ItemTypeName,
                        ItemTypeId = grp.FirstOrDefault().ItemTypeId,
                        DocumentMedias = grp.Where(dm => dm.DocumentMediaId > 0).Select(g => new DocumentMediaInfo
                        {
                            DocumentMediaId = g.DocumentMediaId,
                            EntityId = g.ItemId,
                            EntityName = g.ItemName
                        })
                    });
        }

        /// <summary>
        /// Gets the company address by company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>The company address.</returns>
        public string GetCompanyAddress(Data.Company company)
        {
            StringBuilder address = new StringBuilder();

            if (company.AddressLine1 != string.Empty)
            {
                address.Append(company.AddressLine1);
                address.Append(", ");
            }

            if (company.AddressLine2 != string.Empty)
            {
                address.Append(company.AddressLine2);
                address.Append(", ");
            }

            if (company.City != string.Empty)
            {
                address.Append(company.City);
                address.Append(", ");
            }

            if (company.Country.CountryName != string.Empty)
            {
                address.Append(company.Country.CountryName);
                address.Append(".");
            }

            return address.ToString();
        }
    }
}