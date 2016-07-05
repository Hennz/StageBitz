using System;

namespace StageBitz.Data.DataTypes.Analytics
{
    public class ProjectTeamActivity
    {
        public DateTime Date { get; set; }

        public int ProjectId { get; set; }

        public int UserId { get; set; }

        public string SessionTotal { get; set; }

        public string ProjectName { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public string Permission { get; set; }
    }
}