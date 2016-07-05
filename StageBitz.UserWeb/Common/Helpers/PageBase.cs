using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic;
using StageBitz.Logic.Business;
using StageBitz.UserWeb.Controls.Common;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace StageBitz.UserWeb.Common.Helpers
{
    /// <summary>
    /// Base class for web pages.
    /// </summary>
    public class PageBase : Page
    {
        /// <summary>
        /// The data context
        /// </summary>
        private StageBitzDB dataContext = null;

        /// <summary>
        /// The BL list
        /// </summary>
        private Dictionary<Type, BaseBL> blList = null;

        /// <summary>
        /// The stop processing
        /// </summary>
        private bool stopProcessing;

        #region Properties

        /// <summary>
        /// Gets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
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
        /// <value>
        /// The userId.
        /// </value>
        public int UserID
        {
            get
            {
                return Support.UserID;
            }
        }

        /// <summary>
        /// Gets or sets the display title.
        /// </summary>
        /// <value>
        /// The display title.
        /// </value>
        public string DisplayTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the page dirty flag maintained between javascript and server-side.
        /// Indicates whether any user inputs inside 'dirtyValidationArea' containers have been changed. (see global.js)
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is page dirty; otherwise, <c>false</c>.
        /// </value>
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

        // <summary>
        /// <summary>
        /// Sets a value indicating whether this instance is large content area.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is large content area; otherwise, <c>false</c>.
        /// </value>
        public bool IsLargeContentArea
        {
            set
            {
                ((MasterBase)this.Master).IsLarge = value;
            }
        }

        /// <summary>
        /// Gets the application version string.
        /// </summary>
        /// <value>
        /// The application version string.
        /// </value>
        public string ApplicationVersionString
        {
            get
            {
                return ((MasterBase)this.Master).ApplicationVersionString;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [stop processing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [stop processing]; otherwise, <c>false</c>.
        /// </value>
        public bool StopProcessing
        {
            get
            {
                return stopProcessing;
            }

            set
            {
                stopProcessing = value;
            }
        }

        /// <summary>
        /// Gets the today.
        /// </summary>
        /// <value>
        /// The today.
        /// </value>
        public static DateTime Today
        {
            get
            {
                return Utils.Today;
            }
        }

        /// <summary>
        /// Gets the now.
        /// </summary>
        /// <value>
        /// The now.
        /// </value>
        public static DateTime Now
        {
            get
            {
                return Utils.Now;
            }
        }

        /// <summary>
        /// Gets the BL list.
        /// </summary>
        /// <value>
        /// The BL list.
        /// </value>
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

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// The page title.
        /// </value>
        public string PageTitle
        {
            get
            {
                string sortBy = string.Empty;
                if (ViewState["PageTitle"] != null)
                {
                    sortBy = ViewState["PageTitle"].ToString();
                }

                return sortBy;
            }

            set
            {
                ViewState["PageTitle"] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets a reference to the BreadCrumbs control in the content master page.
        /// </summary>
        /// <returns>The BreadCrumbs.</returns>
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

        /// <summary>
        /// Gets the BL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetBL<T>() where T : BaseBL
        {
            return (T)BLList[typeof(T)];
        }

        /// <summary>
        /// Shows the error popup.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public void ShowErrorPopup(ErrorCodes errorCode)
        {
            string script = string.Format("showErrorPopup({0});", (int)errorCode);
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ShowErrorPopupBox" + ClientID, script, true);
        }

        #endregion Public Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Unload" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            if (dataContext != null)
            {
                dataContext.Dispose();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreInit" /> event at the beginning of page initialization.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            BLList = BLBuilder.GetBLList(DataContext);
        }

        #endregion Overrides
    }
}