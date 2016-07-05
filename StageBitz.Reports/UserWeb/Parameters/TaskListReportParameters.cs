namespace StageBitz.Reports.UserWeb.Parameters
{
    public class TaskListReportParameters
    {
        public int ProjectId { get; set; }

        public string SortExpression { get; set; }

        public int UserId { get; set; }

        public int TaskListId { get; set; }

        public string CultureName { get; set; }
    }
}