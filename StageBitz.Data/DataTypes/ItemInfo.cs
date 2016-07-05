using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StageBitz.Data.DataTypes
{
    /// <summary>
    /// Item Detail Poco class for expose over services.
    /// </summary>
    public class ItemDetails
    {
        public string DisplayMarkUp { get; set; }

        public List<ValuesInfo> ItemValues { get; set; }

        public bool IsReadOnly { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public int ItemBriefId { get; set; }

        public int ItemId { get; set; }

        public int UserId { get; set; }

        public string DocumentMediaIds { get; set; }

        public int DefaultImageId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? Quantity { get; set; }

        public int MinQuantity { get; set; }

        public int? LocationId { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public string ItemStatus { get; set; }

        public int CountryId { get; set; }

        public string RelatedTable { get; set; }

        public bool CanEditInItemBrief { get; set; }

        public int? ItemTypeId { get; set; }

        public string ItemTypeName { get; set; }

        public string CreatedFor { get; set; }

        public int AvailableQty { get; set; }

        [DefaultValue(true)]
        public bool IsEditableToAdminOnly { get; set; }

        public string BookingManagerName { get; set; }

        public string BookingManagerEmail { get; set; }
    }

    /// <summary>
    /// Item Value info poco class for expose over services.
    /// </summary>
    public class ValuesInfo
    {
        public int Id { get; set; }

        public int FieldId { get; set; }

        public string Value { get; set; }

        public int? FieldOptionId { get; set; }
    }
}