using System;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;

namespace StageBitz.Data
{
    /// <summary>
    /// Partial class for StageBitzDB. Contains SQL functions.
    /// </summary>
    public partial class StageBitzDB : ObjectContext
    {
        /// <summary>
        /// Determines whether is item overdue on today.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>
        ///   <c>true</c> if item overdue; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsItemOverdue")]
        public bool IsItemOverdue(int itemId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Gets the Available Items for given duration
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>
        ///   <c>true</c> if item overdue; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetItemAvailableQuantity")]
        public int GetItemAvailableQuantity(int itemId, DateTime fromDate, DateTime toDate, int itemBookingId = 0)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is item brief item overdue] [the specified item brief item id].
        /// </summary>
        /// <param name="itemBookingId">The item brief item id.</param>
        /// <returns>
        ///   <c>true</c> if [is item brief item overdue] [the specified item brief item id]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsItemBookingOverdue")]
        public bool IsItemBookingOverdue(int itemBookingId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Get the margin date which you can extend the From or ToDate
        /// </summary>
        /// <param name="itemBookingId">The item brief item id and if it is a From or To Edit</param>
        /// <returns>
        /// Margin Date
        /// </returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetMaxMarginDateOfItemAvailable")]
        public DateTime GetMaxMarginDateOfItemAvailable(int itemBookingId, bool isToDateEdit)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is item brief item delayed] [the specified item brief item identifier].
        /// </summary>
        /// <param name="itemBookingId">The item brief item identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsItemBookingDelayed")]
        public bool IsItemBookingDelayed(int itemBookingId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is item brief item delayed by date] [the specified item brief item identifier].
        /// </summary>
        /// <param name="itemBookingId">The item brief item identifier.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsItemBookingDelayedByDate")]
        public bool IsItemBookingDelayedByDate(int itemBookingId, DateTime dateToConsider)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is item brief item overdue by date] [the specified item brief item identifier].
        /// </summary>
        /// <param name="itemBookingId">The item brief item identifier.</param>
        /// <param name="dateToConsider">The date to consider.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsItemBookingOverdueByDate")]
        public bool IsItemBookingOverdueByDate(int itemBookingId, DateTime dateToConsider)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Gets the item brief item status.
        /// </summary>
        /// <param name="itemBookingId">The item brief item identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetItemBookingStatus")]
        public int GetItemBookingStatus(int itemBookingId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether this instance [can access project] the specified project identifier.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "CanAccessProject")]
        public bool CanAccessProject(int projectId, int userId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Gets the location path.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetLocationPath")]
        public string GetLocationPath(int? locationId, int companyId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Gets the user inventory visibility level.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetUserInventoryVisibilityLevel")]
        public int GetUserInventoryVisibilityLevel(int companyId, int userId, int? locationId, bool shouldValidateSharedInventory)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is company inventory staff member] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsCompanyInventoryStaffMember")]
        public bool IsCompanyInventoryStaffMember(int companyId, int userId, int? locationId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether this instance [can access inventory] the specified company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "CanAccessInventory")]
        public bool CanAccessInventory(int companyId, int userId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is company inventory admin] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsCompanyInventoryAdmin")]
        public bool IsCompanyInventoryAdmin(int companyId, int userId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [has location manager permission] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "HasLocationManagerPermission")]
        public bool HasLocationManagerPermission(int companyId, int userId, int? locationId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Determines whether [is company inventory team member] [the specified company identifier].
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "IsCompanyInventoryTeamMember")]
        public bool IsCompanyInventoryTeamMember(int companyId, int userId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Gets the tier2 location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Direct calls are not supported.</exception>
        [EdmFunction("StageBitzModel.Store", "GetTier2Location")]
        public int GetTier2Location(int locationId)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
    }
}