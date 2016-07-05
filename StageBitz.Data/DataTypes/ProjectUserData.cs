using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ProjectUserData
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public bool IsContact { get; set; }
        public int ProjectUserId { get; set; }
        public int InvitationId { get; set; }
        public string Role { get; set; }
        public  Data.Code UserTypeCode { get; set; }
        public Data.Code StatusCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsMember { get; set; }
        public bool IsCompanyAdmin { get; set; }
    }
}
