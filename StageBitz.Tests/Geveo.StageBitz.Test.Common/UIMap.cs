namespace Geveo.StageBitz.Test.Common
{
    using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using System.Configuration;



    public partial class UIMap
    {
        public void SetSBUserWebURL()
        {
            string URL = ConfigurationManager.AppSettings["URL"].ToString();
            this.LaunchSBUserWebURLParams.UIGoogleWindowsInterneWindowUrl = URL;

        }

        public void SetLoginDetails(string username, string password)
        {
            this.LoginUserPortalParams.UIEmailAddressEditText = username;
            this.LoginUserPortalParams.UIPasswordEditPassword = password;
        }

        /// LaunchSBUserWebURL - Use 'LaunchSBUserWebURLParams' to pass parameters into this method.
        public void LaunchSBUserWebURL()
        {
            
            // Go to web page 'http://localhost/StageBitz.DevUserWeb/' using new browser instance
            BrowserWindow window;
            window = UIGoogleWindowsInterneWindow.Launch(new Uri(this.LaunchSBUserWebURLParams.UIGoogleWindowsInterneWindowUrl));
            window.CloseOnPlaybackCleanup = false;
        }

        public virtual LaunchSBUserWebURLParams LaunchSBUserWebURLParams
        {
            get
            {
                if ((this.mLaunchSBUserWebURLParams == null))
                {
                    this.mLaunchSBUserWebURLParams = new LaunchSBUserWebURLParams();
                }
                return this.mLaunchSBUserWebURLParams;
            }
        }

        private LaunchSBUserWebURLParams mLaunchSBUserWebURLParams;
    }
    /// <summary>
    /// Parameters to be passed into 'LaunchSBUserWebURL'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class LaunchSBUserWebURLParams
    {

        #region Fields
        /// <summary>
        /// Go to web page 'http://localhost/StageBitz.DevUserWeb/'
        /// </summary>
        public string UIGoogleWindowsInterneWindowUrl = "http://localhost/StageBitz.DevUserWeb/";
        #endregion
}
}
