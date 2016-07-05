using System;

namespace StageBitz.Data.DataTypes.Analytics
{
    public class UserActivity
    {
        public DateTime Date { get; set; }

        public int ProjectId { get; set; }

        public int CompanyId { get; set; }

        public string ProjectName { get; set; }

        public string Role { get; set; }

        public string Permission { get; set; }

        public string CompanyName { get; set; }

        public string SessionTotal { get; set; }
    }
}