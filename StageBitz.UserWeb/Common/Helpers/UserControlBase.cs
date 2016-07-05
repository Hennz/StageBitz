using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business;
using System;
using System.Web.UI;

namespace StageBitz.UserWeb.Common.Helpers
{
    /// <summary>
    /// Base class for user controls.
    /// </summary>
    public class UserControlBase : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets the page base.
        /// </summary>
        /// <value>
        /// The page base.
        /// </value>
        public PageBase PageBase
        {
            get
            {
                return (PageBase)this.Page;
            }
        }

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
                return PageBase.DataContext;
            }
        }

        /// <summary>
        /// Currently logged-in userID
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public int UserID
        {
            get
            {
                return PageBase.UserID;
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
                return PageBase.ApplicationVersionString;
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Displays the element with the specified client id for a short period of time and then hides it.
        /// </summary>
        /// <param name="elementId">Html client id of the element</param>
        public void ShowNotification(string elementId)
        {
            PageBase.ShowNotification(elementId);
        }

        /// <summary>
        /// Gets the BL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List of BLs</returns>
        public T GetBL<T>() where T : BaseBL
        {
            return (T)PageBase.BLList[typeof(T)];
        }

        #endregion Methods
    }
}