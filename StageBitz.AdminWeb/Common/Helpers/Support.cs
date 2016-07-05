using System.Web;
using System.Linq;
using System.Web.Security;
using StageBitz.Data;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System;
using System.Drawing.Imaging;
using System.Configuration;
using StageBitz.Common;
using System.Web.UI.WebControls;

namespace StageBitz.AdminWeb.Common.Helpers
{
    public static class Support
    {
        #region Properties

        #region Logged-in User related

        /// <summary>
        /// Gets or sets user ID for the logged in user
        /// </summary>
        public static int UserID
        {
            get
            {
                if (HttpContext.Current.Session["UserID"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return (int)HttpContext.Current.Session["UserID"];
            }
            private set
            {
                HttpContext.Current.Session["UserID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the loginname for the logged in user
        /// </summary>
        public static string LoginName
        {
            get
            {
                if (HttpContext.Current.Session["LoginName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["LoginName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["LoginName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the first name for the logged in user
        /// </summary>
        public static string UserFirstName
        {
            get
            {
                if (HttpContext.Current.Session["UserFirstName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserFirstName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserFirstName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name for the logged in user
        /// </summary>
        public static string UserLastName
        {
            get
            {
                if (HttpContext.Current.Session["UserLastName"] == null)
                {
                    InitializeUserSessionFromAuthCookie();
                }

                return HttpContext.Current.Session["UserLastName"].ToString();
            }
            private set
            {
                HttpContext.Current.Session["UserLastName"] = value;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Tries to initialize the user session using the asp.net authentication cookie.
        /// If authentication cookie is unavailable, user is logged out forcefully.
        /// </summary>
        private static void InitializeUserSessionFromAuthCookie()
        {
            bool isAuthenticated = false;

            //User must be re-authenticated and the session must be reinitialized

            //If remember me cookie is available, setup the session automaitcally by
            //requesting user details from the DB.
            //If the cookie is unavailable, perform a forced Logout.
            try
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    //We are storing the user ID as the username inside asp.net auth cookie (See Login.aspx).
                    //Check whether we can find it inside the cookie.
                    string cookieUserName = FormsAuthentication.Decrypt(authCookie.Value).Name;
                    int rememberedUserID = 0;
                    int.TryParse(cookieUserName, out rememberedUserID);

                    if (rememberedUserID > 0)
                    {
                        //If a remembered user ID exists, get user details from the DB
                        using (StageBitzDB dataContext = new StageBitzDB())
                        {
                            StageBitz.Data.User user = GetActiveUserById(rememberedUserID, dataContext);

                            if (user != null)
                            {
                                //If authentication is successful, set the session variables
                                isAuthenticated = true;
                                SetUserSessionData(user);
                            }
                        }
                    }
                }
            }
            catch
            {
                HttpContext.Current.Response.Redirect("~/Account/Logout.aspx", true);
            }

            //If all attempts to authenticate the user has failed, perform a forced logout
            if (!isAuthenticated)
            {
                HttpContext.Current.Response.Redirect("~/Account/Logout.aspx", true);
            }
        }

        /// <summary>
        /// Returns the user with the specified id if that user is active
        /// </summary>
        private static StageBitz.Data.User GetActiveUserById(int userId, StageBitzDB dataContext)
        {
            var user = from u in dataContext.Users
                       where u.UserId == userId && u.IsActive == true
                       select u;

            if (user.Count<StageBitz.Data.User>() == 1)
            {
                return user.First<StageBitz.Data.User>();
            }

            return null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the session variables with logged in user data
        /// </summary>
        /// <param name="user">The logged in user</param>
        public static void SetUserSessionData(StageBitz.Data.User user)
        {
            UserID = user.UserId;
            LoginName = user.LoginName;
            UserFirstName = user.FirstName;
            UserLastName = user.LastName;
        }

        /// <summary>
        /// Activation resending 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public static string GetUserActivationLink(string email, string passwordHash)
        {
            //Password is sent to improve security. Unless someone else can activate the account.
            return string.Format("{0}/Account/Activation.aspx?email={1}&activationkey={2}", Utils.GetSystemValue("SBUserWebURL"), HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(passwordHash));
        }

        #region Date time formatting

        public static string FormatDate(Object dt)
        {
            if (dt != null)
                return FormatDatetime((DateTime)dt, false);
            else
                return string.Empty;
        }

        public static string FormatTime(Object dt)
        {
            if (dt != null)
                return FormatDatetime((DateTime)dt, true);
            else
                return string.Empty;
        }

        public static string FormatDatetime(DateTime dt, bool isTimeNeed)
        {
            try
            {
                if (isTimeNeed)
                {
                    return dt.ToString("dd MMM yyyy hh:mm tt");
                }
                else
                {
                    return dt.ToString("dd MMM yyyy");
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Common

        public static void AssignTextToLabel(Label label, string text, int truncateLength)
        {
            if (text != null)
            {
                label.Text = Support.TruncateString(text, truncateLength);
                if (text.Length > truncateLength)
                {
                    label.ToolTip = text;
                }
            }
            else
            {
                label.Text = string.Empty;
            }
        }

        /// <summary>
        /// Truncates a string and appends '...' to the end
        /// </summary>
        public static string TruncateString(object text, int maxLength)
        {
            if (text == null)
            {
                return string.Empty;
            }

            return Utils.Ellipsize(text.ToString(), maxLength);
        }

        public static List<Country> GetAllCountries()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                List<Country> countries = (from c in dataContext.Countries
                                           where c.IsActive == true
                                           orderby c.SortOrder, c.CountryName
                                           select c).ToList<Country>();
                //Default selection
                Country country = new Country();
                country.CountryName = "-- Please select --";
                country.CountryId = -1;//This is to track whether a country is not being selected
                countries.Insert(0, country);
                return countries.ToList<Country>();
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

        /// <summary>
        /// Returns the current application Url
        /// </summary>
        public static string GetSystemUrl()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath;
        }

        /// <summary>
        /// Returns the current application image Url for Emails
        /// </summary>
        public static string GetSystemLogoPhysicalPath()
        {
            return HttpContext.Current.Server.MapPath("~/Common/Images/StageBitzLogo_small.png");
        }

        #endregion

        #region Currency

        public static string FormatCurrency(object amount)
        {
            return String.Format("{0:C}", amount);

        }
        #endregion

        #endregion
    }
}
