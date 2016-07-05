using StageBitz.Data;
using StageBitz.Data.DataTypes.Finance;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;

namespace StageBitz.Common
{
    /// <summary>
    /// The referance data cache for the StageBitz system.
    /// This is an in-memory cache each individual application.
    /// </summary>
    public static class SystemCache
    {
        #region Private Cache Item Accessors

        /// <summary>
        /// Gets or sets the system values cache item.
        /// </summary>
        /// <value>
        /// The system values cache item.
        /// </value>
        private static StringDictionary SystemValuesCacheItem
        {
            get
            {
                return (StringDictionary)MemoryCache.Default["SystemValues"];
            }
            set
            {
                MemoryCache.Default["SystemValues"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the countries cache item.
        /// </summary>
        /// <value>
        /// The countries cache item.
        /// </value>
        private static List<Country> CountriesCacheItem
        {
            get
            {
                return (List<Country>)MemoryCache.Default["Countries"];
            }
            set
            {
                MemoryCache.Default["Countries"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item types cache item.
        /// </summary>
        /// <value>
        /// The item types cache item.
        /// </value>
        private static List<ItemType> ItemTypesCacheItem
        {
            get
            {
                return (List<ItemType>)MemoryCache.Default["ItemTypes"];
            }
            set
            {
                MemoryCache.Default["ItemTypes"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment package type list.
        /// </summary>
        /// <value>
        /// The payment package type list.
        /// </value>
        private static List<PaymentPackageType> PaymentPackageTypeList
        {
            get
            {
                return (List<PaymentPackageType>)MemoryCache.Default["PaymentPackageTypeList"];
            }
            set
            {
                MemoryCache.Default["PaymentPackageTypeList"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the inventory payment package details from cache list.
        /// </summary>
        /// <value>
        /// The inventory payment package details cache list.
        /// </value>
        private static List<InventoryPaymentPackageDetails> InventoryPaymentPackageDetailsCacheList
        {
            get
            {
                return (List<InventoryPaymentPackageDetails>)MemoryCache.Default["InventoryPaymentPackageDetailsCacheList"];
            }
            set
            {
                MemoryCache.Default["InventoryPaymentPackageDetailsCacheList"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project package details cache list.
        /// </summary>
        /// <value>
        /// The project package details cache list.
        /// </value>
        private static List<ProjectPaymentPackageDetails> ProjectPackageDetailsCacheList
        {
            get
            {
                return (List<ProjectPaymentPackageDetails>)MemoryCache.Default["ProjectPackageDetailsCacheList"];
            }
            set
            {
                MemoryCache.Default["ProjectPackageDetailsCacheList"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the code headers cache item.
        /// </summary>
        /// <value>
        /// The code headers cache item.
        /// </value>
        private static Dictionary<string, Dictionary<string, Code>> CodeHeadersCacheItem
        {
            get
            {
                return (Dictionary<string, Dictionary<string, Code>>)MemoryCache.Default["CodeHeaders"];
            }
            set
            {
                MemoryCache.Default["CodeHeaders"] = value;
            }
        }

        /// <summary>
        /// Gets or sets all codes cache item.
        /// </summary>
        /// <value>
        /// All codes cache item.
        /// </value>
        private static Dictionary<int, Code> AllCodesCacheItem
        {
            get
            {
                return (Dictionary<int, Code>)MemoryCache.Default["AllCodes"];
            }
            set
            {
                MemoryCache.Default["AllCodes"] = value;
            }
        }

        #endregion Private Cache Item Accessors

        #region Public Properties

        /// <summary>
        /// Gets the system values.
        /// </summary>
        /// <value>
        /// The system values.
        /// </value>
        public static StringDictionary SystemValues
        {
            get
            {
                if (SystemValuesCacheItem == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        SystemValuesCacheItem = new StringDictionary();

                        foreach (Data.SystemValue dbSystemValue in dataContext.SystemValues)
                        {
                            SystemValuesCacheItem[dbSystemValue.Name] = dbSystemValue.Value;
                        }
                    }
                }

                return SystemValuesCacheItem;
            }
        }

        /// <summary>
        /// Gets the inventory payment package detail list.
        /// </summary>
        /// <value>
        /// The inventory payment package detail list.
        /// </value>
        public static List<InventoryPaymentPackageDetails> InventoryPaymentPackageDetailList
        {
            get
            {
                if (InventoryPaymentPackageDetailsCacheList == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        InventoryPaymentPackageDetailsCacheList = (from pt in PaymentPackageTypes
                                                                   join ipd in dataContext.InventoryPaymentPackageDetails on pt.PaymentPackageTypeId equals ipd.PaymentPackageTypeId
                                                                   where ipd.EndDate == null && pt.IsActive == true
                                                                   select new InventoryPaymentPackageDetails
                                                                   {
                                                                       PackageTypeId = pt.PaymentPackageTypeId,
                                                                       InventoryPaymentPackageDetailId = ipd.InventoryPaymentPackageDetailId,
                                                                       Amount = ipd.Amount,
                                                                       AnualAmount = ipd.AnualAmount,
                                                                       ItemCount = (ipd.ItemCount == null ? (int?)null : ipd.ItemCount.Value),
                                                                       PackageName = pt.Name,
                                                                       PackageDisplayName = pt.PackageDisplayName,
                                                                       PackageDisplayText = pt.DisplayText,
                                                                   }).ToList<InventoryPaymentPackageDetails>();
                    }
                }
                return InventoryPaymentPackageDetailsCacheList;
            }
        }

        /// <summary>
        /// Gets the project package detail list.
        /// </summary>
        /// <value>
        /// The project package detail list.
        /// </value>
        public static List<ProjectPaymentPackageDetails> ProjectPackageDetailList
        {
            get
            {
                if (ProjectPackageDetailsCacheList == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        ProjectPackageDetailsCacheList = (from pt in PaymentPackageTypes
                                                          join ppd in dataContext.ProjectPaymentPackageDetails on pt.PaymentPackageTypeId equals ppd.PaymentPackageTypeId
                                                          where ppd.EndDate == null && pt.IsActive == true
                                                          select new ProjectPaymentPackageDetails
                                                          {
                                                              PackageTypeId = pt.PaymentPackageTypeId,
                                                              ProjectPaymentPackageDetailId = ppd.ProjectPaymentPackageDetailId,
                                                              Amount = ppd.Amount,
                                                              AnualAmount = ppd.AnualAmount,
                                                              ProjectCount = ppd.ProjectCount,
                                                              HeadCount = ppd.HeadCount,
                                                              PackageName = pt.Name,
                                                              PackageDisplayName = pt.PackageDisplayName,
                                                              PackageTitleDiscription = pt.PackageTitleDiscription,
                                                              PackageDisplayText = pt.DisplayText
                                                          }).ToList<ProjectPaymentPackageDetails>();
                    }
                }
                return ProjectPackageDetailsCacheList;
            }
        }

        /// <summary>
        /// Gets the payment package types.
        /// </summary>
        /// <value>
        /// The payment package types.
        /// </value>
        public static List<PaymentPackageType> PaymentPackageTypes
        {
            get
            {
                if (PaymentPackageTypeList == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        PaymentPackageTypeList = (from ppt in dataContext.PaymentPackageTypes
                                                  select ppt).ToList<PaymentPackageType>();
                    }
                }
                return PaymentPackageTypeList;
            }
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <value>
        /// The countries.
        /// </value>
        public static List<Country> Countries
        {
            get
            {
                if (CountriesCacheItem == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        CountriesCacheItem = (from it in dataContext.Countries
                                              select it).ToList<Country>();
                    }
                }
                return CountriesCacheItem;
            }
        }

        /// <summary>
        /// Gets the item types.
        /// </summary>
        /// <value>
        /// The item types.
        /// </value>
        public static List<ItemType> ItemTypes
        {
            get
            {
                if (ItemTypesCacheItem == null)
                {
                    //Query the database and load the system values
                    using (StageBitzDB dataContext = new StageBitzDB())
                    {
                        ItemTypesCacheItem = (from it in dataContext.ItemTypes
                                              select it).ToList<ItemType>();
                    }
                }
                return ItemTypesCacheItem;
            }
        }

        /// <summary>
        /// Gets the code headers.
        /// </summary>
        /// <value>
        /// The code headers.
        /// </value>
        public static Dictionary<string, Dictionary<string, Code>> CodeHeaders
        {
            get
            {
                if (CodeHeadersCacheItem == null)
                {
                    InitializeCodeTables();
                }

                return CodeHeadersCacheItem;
            }
        }

        /// <summary>
        /// Gets the codes.
        /// </summary>
        /// <value>
        /// The codes.
        /// </value>
        public static Dictionary<int, Code> Codes
        {
            get
            {
                if (AllCodesCacheItem == null)
                {
                    InitializeCodeTables();
                }

                return AllCodesCacheItem;
            }
        }

        #endregion Public Properties

        /// <summary>
        /// Clears the referance data cache for the calling application.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var element in MemoryCache.Default)
            {
                MemoryCache.Default.Remove(element.Key);
            }
        }

        /// <summary>
        /// Initializes the code tables.
        /// </summary>
        private static void InitializeCodeTables()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                CodeHeadersCacheItem = new Dictionary<string, Dictionary<string, Code>>();
                AllCodesCacheItem = new Dictionary<int, Code>();

                foreach (Data.CodeHeader dbCodeHeader in dataContext.CodeHeaders)
                {
                    Dictionary<string, Code> childCodes = new Dictionary<string, Code>();
                    CodeHeadersCacheItem[dbCodeHeader.Name] = childCodes;

                    foreach (Data.Code dbCode in dbCodeHeader.Codes)
                    {
                        Code code = new Code();
                        code.CodeId = dbCode.CodeId;
                        code.Value = dbCode.Value;
                        code.Description = dbCode.Description;
                        code.SortOrder = dbCode.SortOrder;

                        childCodes[dbCode.Value] = code;

                        //Add a reference to the global code dictionary as well.
                        AllCodesCacheItem[dbCode.CodeId] = code;
                    }
                }
            }
        }
    }
}