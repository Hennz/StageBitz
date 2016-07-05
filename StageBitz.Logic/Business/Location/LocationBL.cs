using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Business.Location
{
    /// <summary>
    ///  Business layer for location
    /// </summary>
    public class LocationBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public LocationBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets all the active location in a Company.
        /// </summary>
        /// <param name="companyId">The item brief identifier.</param>
        /// <returns>Location List</returns>
        public IEnumerable<Data.Location> GetLocations(int companyId, int userId, bool showAll = false)
        {
            return (from l in DataContext.Locations
                    join lTree in DataContext.GetAllSubLocatons(null, companyId, userId, showAll) on l.LocationId equals lTree.LocationId
                    orderby lTree.LocationName
                    select l);
        }

        /// <summary>
        /// Gets the active location by the its ID.
        /// </summary>
        /// <param name="locationId">The LocationId.</param>
        /// <returns></returns>
        public Data.Location GetLocation(int locationId)
        {
            return DataContext.Locations.Where(l => l.LocationId == locationId && l.IsActive).FirstOrDefault();
        }

        /// <summary>
        /// Checks if the new location already exists for the given location level.
        /// </summary>
        /// <param name="parentLocationId">The LocationId</param>
        /// <param name="newLocationName">The location name.</param>
        /// <returns></returns>
        public bool HasDuplcateLocations(int companyId, int? parentLocationId, string newLocationName, int locationIdToEdit)
        {
            var duplicateLocation = (from l in DataContext.Locations
                                     where l.CompanyId == companyId && l.IsActive &&
                                     (parentLocationId.HasValue ? l.ParentLocationId == parentLocationId : l.ParentLocationId == null) && (locationIdToEdit == 0 || l.LocationId != locationIdToEdit)
                                     && l.LocationName == newLocationName
                                     select l).FirstOrDefault();
            //For existing locations need to skip its location object which is in the DB.
            return (duplicateLocation != null);
        }

        /// <summary>
        /// Determines whether duplcate locations exists.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="parentLocationId">The parent location identifier.</param>
        /// <param name="newLocationName">New name of the location.</param>
        /// <returns></returns>
        public bool HasDuplcateLocations(int companyId, int? parentLocationId, string newLocationName)
        {
            return HasDuplcateLocations(companyId, parentLocationId, newLocationName, 0);
        }

        /// <summary>
        /// Gets all items in location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<Data.Item> GetAllItemsInLocation(int locationId, int companyId, int userId)
        {
            var items = from i in DataContext.Items
                        join l in DataContext.GetAllSubLocatons(locationId, companyId, userId, false) on i.LocationId equals l.LocationId
                        where i.IsActive
                        select i;

            return items;
        }

        /// <summary>
        /// Gets all sub locations.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public int?[] GetAllSubLocations(int? locationId, int companyId, int userId)
        {
            int?[] locationIDList = (from l in DataContext.GetAllSubLocatons(locationId, companyId, userId, false)
                                     select l.LocationId).ToArray();
            return locationIDList;
        }

        /// <summary>
        /// Deletes the locaton.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteLocaton(Data.Location location, int userId)
        {
            InventoryBL inventoryBL = new InventoryBL(DataContext);

            location.IsActive = false;
            location.LastUpdatedByUserId = userId;
            location.LastUpdatedDate = Utils.Now;

            if (!location.ParentLocationId.HasValue)
            {
                List<CompanyUserRole> locationUserRoles = inventoryBL.GetInventoryRolesByLocationId(location.LocationId);
                foreach (CompanyUserRole locationUserRole in locationUserRoles)
                {
                    locationUserRole.IsActive = false;
                }
            }

            DataContext.SaveChanges();
        }

        /// <summary>
        /// Performs the location Save.
        /// </summary>
        /// <param name="parentLocationId">The LocationId</param>
        /// <param name="newLocationName">The location name.</param>
        /// <returns></returns>
        public void SaveLocation(int? locationId, string newLocationName, int userId, int companyId, bool isAddNew = true)
        {
            if (!isAddNew)
            {
                //update existing location
                Data.Location locaton = GetLocation(locationId.Value);
                if (locaton != null)
                {
                    locaton.LocationName = newLocationName;
                    locaton.LastUpdatedByUserId = userId;
                    locaton.LastUpdatedDate = Utils.Now;
                }
            }
            else
            {
                //Insert a new location
                Data.Location location = new Data.Location()
                {
                    LocationName = newLocationName,
                    CompanyId = companyId,
                    ParentLocationId = locationId,
                    CreatedByUserId = userId,
                    CreatedDate = Utils.Now,
                    LastUpdatedByUserId = userId,
                    LastUpdatedDate = Utils.Now,
                    IsActive = true
                };
                DataContext.Locations.AddObject(location);
            }
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Gets the location path.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public string GetLocationPath(int? locationId, int companyId)
        {
            return DataContext.Companies.Where(c => c.CompanyId == companyId).Select(c => DataContext.GetLocationPath(locationId, c.CompanyId)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the tier2 locations.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="accessingUserId">The accessing user identifier.</param>
        /// <param name="viewingUserId">The viewing user identifier.</param>
        /// <returns></returns>
        public List<Tier2Location> GetTier2Locations(int companyId, int accessingUserId, bool isReadOnly)
        {
            int inventoryStaffCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVSTAFF").CodeId;
            int inventoryObserverCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "INVOBSERVER").CodeId;
            int locationManagerCodeId = Utils.GetCodeByValue("CompanyUserTypeCode", "LOCATIONMANAGER").CodeId;

            var locations = from l in DataContext.Locations
                            where l.ParentLocationId == null && l.CompanyId == companyId && l.IsActive
                            select new Tier2Location
                            {
                                Location = l,
                                CanEdit = isReadOnly ? false : DataContext.HasLocationManagerPermission(companyId, accessingUserId, l.LocationId)
                            };

            return locations.ToList<Tier2Location>();
        }

        /// <summary>
        /// Gets the tier2 location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public Data.Location GetTier2Location(int locationId)
        {
            return DataContext.Locations.Where(l => l.LocationId == DataContext.GetTier2Location(locationId)).FirstOrDefault();
        }
    }
}