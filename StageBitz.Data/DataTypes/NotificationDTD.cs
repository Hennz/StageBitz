using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageBitz.Data.DataTypes
{
    public class NotificationDTD
    {    
        //NotificationId :int
        //ImageURL       : <img id="imgOperation" src="../Common/Images/add.png" />
        //Module         : <a href="../ItemBrief/ItemBriefList.aspx?projectid=19" id="Module" target="_blank" class="blueText">
        //             Item List</a>
        //Message        :String
        //EventDate      : FormattedDateTime

        public int NotificationId { get; set; }
        public string ImageURL { get; set; }
        public string ModuleHref { get; set; }
        public string ModuleName { get; set; }
        public string Message { get; set; }
        public string EventDate { get; set; }
        public string Style { get; set; }
    }
}
