namespace Geveo.StageBitz.Test.UserRegistration
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
    using System.Text;
    using System.IO;



    public partial class UIMap
    {
        public void SetUserDetails(string fname1, string lname, string email, string cemail, string pwd, string cpwd, string country, string phone1, string phone2, bool primaryemail)
        {
            this.CreateUserParams.tbxFirstNameText = fname1;
            this.CreateUserParams.tbxLastNameText = lname;
            this.CreateUserParams.tbxEmailText = email;
            this.CreateUserParams.tbxConfirmEmailText = cemail;
            this.CreateUserParams.tbxPasswordPassword = pwd;
            this.CreateUserParams.tbxConfirmPasswordPassword = cpwd;
            this.CreateUserParams.cmbCountrySelectedItem = country;
            this.CreateUserParams.tbxPhone1Text = phone1;
            this.CreateUserParams.tbxPhone2Text = phone2;
            this.CreateUserParams.chkbxShowPrimaryEmailChecked = primaryemail;
        }

        //public void CreateUserDetailsFile()
        //{
        //    string userdetailsFilePath = ConfigurationManager.AppSettings["UserDetailsFilePath"].ToString();

        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("fname,lname,email");
        //    sb.AppendLine(string.Format("{0},{1},{2}",
        //                                this.CreateUserParams.tbxFirstNameText,
        //                                this.CreateUserParams.tbxLastNameText,
        //                                this.CreateUserParams.tbxEmailText));

        //    File.WriteAllText(userdetailsFilePath, sb.ToString());
        //}

        public void SetMailinatorURL()
        {
            string MailinatorURL = ConfigurationManager.AppSettings["MailinatorURL"].ToString();
            this.LaunchMailinatorParams.UIGoogleWindowsInterneWindowUrl = MailinatorURL;

        }

        public void SetMailinatorInboxDetails(string fname1)
        {
            this.SearchMailinatorInboxParams.UIInboxfieldEditText = fname1;
        }

        public void SetMyFirstProjectDetails(string companyname, string projectname)
        {
            this.FreeTrialCreationLoginParams.tbxCompanyNameText1 = companyname;
            this.FreeTrialCreationLoginParams.tbxProjectNameText = projectname;
        }

        public void SetSelectCreateFTProjectandCompanyOptionDetails(string companyname1, string country1, string projectname1)
        {
            this.SelectCreateFTProjectandCompanyOptionParams.tbxCompanyNameText = companyname1;
            this.SelectCreateFTProjectandCompanyOptionParams.cmbCountrySelectedItem = country1;
            this.SelectCreateFTProjectandCompanyOptionParams.tbxProjectNameText = projectname1;
        }

        /// <summary>
        /// LaunchMailinator - Use 'LaunchMailinatorParams' to pass parameters into this method.
        /// </summary>
        public void LaunchMailinator()
        {

            // Go to web page 'http://www.mailinator.com/' using new browser instance
            //this.UIGoogleWindowsInterneWindow.LaunchUrl(new System.Uri(this.LaunchMailinatorParams.UIGoogleWindowsInterneWindowUrl));
            

            // Go to web page 'http://www.mailinator.com/' using new browser instance
            BrowserWindow window;
            window = UIGoogleWindowsInterneWindow.Launch(new Uri(this.LaunchMailinatorParams.UIGoogleWindowsInterneWindowUrl));
            window.CloseOnPlaybackCleanup = false;
               
        }

        public virtual LaunchMailinatorParams LaunchMailinatorParams
        {
            get
            {
                if ((this.mLaunchMailinatorParams == null))
                {
                    this.mLaunchMailinatorParams = new LaunchMailinatorParams();
                }
                return this.mLaunchMailinatorParams;
            }
        }

        private LaunchMailinatorParams mLaunchMailinatorParams;
    }
    /// <summary>
    /// Parameters to be passed into 'LaunchMailinator'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class LaunchMailinatorParams
    {

        #region Fields
        /// <summary>
        /// Go to web page 'http://www.mailinator.com/' using new browser instance
        /// </summary>
        public string UIGoogleWindowsInterneWindowUrl = "http://www.mailinator.com/";
        #endregion
}
}
