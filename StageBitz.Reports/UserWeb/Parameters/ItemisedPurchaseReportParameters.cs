namespace StageBitz.Reports.UserWeb.Parameters
{
    public class ItemisedPurchaseReportParameters
    {
        public int ProjectId { get; set; }

        public string SortExpression { get; set; }

        public int UserId { get; set; }

        public int ItemTypeId { get; set; }

        public string CultureName { get; set; }
    }
}