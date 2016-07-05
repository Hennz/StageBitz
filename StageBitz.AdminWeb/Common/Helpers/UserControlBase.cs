using System.Web.UI;
using StageBitz.Data;
using System;
using StageBitz.Common;
using StageBitz.Logic.Business;

namespace StageBitz.AdminWeb.Common.Helpers
{
    public class UserControlBase : UserControl
    {      

        public PageBase PageBase
        {
            get
            {
                return (PageBase)this.Page;
            }
        }

        public StageBitzDB DataContext
        {
            get
            {
                return PageBase.DataContext;
            }
        }

        /// <summary>
        /// Currently logged-in userID
        /// </summary>
        public int UserID
        {
            get
            {
                return PageBase.UserID;
            }
        }
        
        public static DateTime Today
        {
            get
            {
                return Utils.Today;
            }
        }

        public static DateTime Now
        {
            get
            {
                return Utils.Now;
            }
        }

        public T GetBL<T>() where T : BaseBL
        {
            return (T)PageBase.BLList[typeof(T)];
        }
    }
}
