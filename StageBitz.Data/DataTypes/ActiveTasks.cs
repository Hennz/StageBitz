using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ActiveTasks
    {
        public int ItemBriefTaskId { get; set; }
        public int ItemBriefId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public int ItemTypeId { get; set; }
        public string itemBriefName { get; set; }
        public string taskDescription { get; set; }
        public int taskListId { get; set; }
    }
}
