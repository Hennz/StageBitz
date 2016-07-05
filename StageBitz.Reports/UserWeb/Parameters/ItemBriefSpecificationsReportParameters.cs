namespace StageBitz.Reports.UserWeb.Parameters
{
    public class ItemBriefSpecificationsReportParameters
    {
        public int ItemBriefId { get; set; }

        public int ProjectId { get; set; }

        public int UserId { get; set; }

        public byte[] NoImagePDFBytes { get; set; }
    }
}