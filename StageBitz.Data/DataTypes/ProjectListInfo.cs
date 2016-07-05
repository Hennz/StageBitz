using System;

namespace StageBitz.Data.DataTypes
{
    public class ProjectListInfo
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? ProjectTypeCodeId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ProjectRole { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProjectStatusCodeId { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool PaymentsSpecified { get; set; }
        public int ItemCount { get; set; }
        public int CompletedItemCount { get; set; }
        public int InProgressItemCount { get; set; }
        public int NotStartedItemCount { get; set; }
        public int InvitationId { get; set; }
        public bool IsCompanyAdmin { get; set; }
        public int NotificationCount { get; set; }
        public int ClosedByUserId { get; set; }
        public string ClosedByName { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}
