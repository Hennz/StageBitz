using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Data;
using StageBitz.Common;
using StageBitz.Logic;
using StageBitz.Logic.Business;

namespace StageBitz.AdminWeb.Common.Helpers
{
    public class PageBase : Page
    {
        private StageBitzDB dataContext = null;
        private Dictionary<Type, BaseBL> blList = null;

        #region Properties

        public StageBitzDB DataContext
        {
            get
            {
                if (dataContext == null)
                {
                    dataContext = new StageBitzDB();
                }
                return dataContext;
            }
        }

        /// <summary>
        /// Gets the currently logged on userID
        /// </summary>
        public int UserID
        {
            get
            {
                return Support.UserID;
            }
        }

        public string DisplayTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the page dirty flag maintained between javascript and server-side.
        /// Indicates whether any user inputs inside 'dirtyValidationArea' containers have been changed. (see global.js)
        /// </summary>
        public bool IsPageDirty
        {
            get
            {
                if (this.Master != null)
                {
                    return ((MasterBase)this.Master).IsPageDirty;
                }
                return false;
            }
            set
            {
                if (this.Master != null)
                {
                    ((MasterBase)this.Master).IsPageDirty = value;
                }
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

        public Dictionary<Type, BaseBL> BLList
        {
            get
            {
                if (blList == null)
                {
                    blList = new Dictionary<Type, BaseBL>();
                }

                return blList;
            }

            private set
            {
                blList = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a reference to the BreadCrumbs control in the content master page.
        /// </summary>
        public BreadCrumbs GetBreadCrumbsControl()
        {
            if (this.Master is Content)
            {
                return ((Content)Master).BreadCrumbs;
            }

            return null;
        }

        /// <summary>
        /// Displays the element with the specified client id for a short period of time and then hides it.
        /// </summary>
        /// <param name="elementId">Html client id of the element</param>
        public void ShowNotification(string elementId)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowNotification", string.Format("showNotification('{0}');", elementId), true);
        }

        #endregion

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            if (dataContext != null)
            {
                dataContext.Dispose();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BLList = BLBuilder.GetBLList(DataContext);
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // base.OnInit(e);
            //BLList = BLBuilder.GetBLList(DataContext);
            LoadBL();
        }

        public T GetBL<T>() where T : BaseBL
        {
            return (T)BLList[typeof(T)];
        }


        public void LoadBL()
        {
            BLList = BLBuilder.GetBLList(DataContext);
        }

    }
}