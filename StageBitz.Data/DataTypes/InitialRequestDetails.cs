using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StageBitz.Data.DataTypes
{
    public class InitialRequestDetails
    {
        public string Email { get; set; }
        public string Pwd { get; set; }
        public string Version { get; set; }
        public byte[] Token { get; set; }
    }
}