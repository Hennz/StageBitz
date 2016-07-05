using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class ItemStatusInformationForUser
    {
        public bool IsCompanyAdmin { get; set; }
        public bool IsInventoryManager { get; set; }
        public bool IsItemInUse { get; set; }
        public bool HasPaymentSetuped { get; set; }
        public bool HasSuspended { get; set; }
        public bool IsReadOnly { get; set; }

        public bool CanEditQuantity { get; set; }


    }
}
