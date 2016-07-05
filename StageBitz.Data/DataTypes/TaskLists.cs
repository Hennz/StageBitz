using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class TaskLists
    {
        public int TaskListId { get; set; }
        public string Name { get; set; }
        public int TaskCount { get; set; }
        public int CompletedItemCount { get; set; }
        public int InprogressItemCount { get; set; }
    }
}
