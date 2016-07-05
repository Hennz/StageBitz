using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Configuration;
using System.IO;
using System.Threading;


namespace Geveo.StageBitz.Test.UserRegistration
{
    /// <summary>
    /// Summary description for UserRegistration
    /// </summary>
    [CodedUITest]
    public class UserRegistration
    {
        public UserRegistration()
        {
        }

        [TestMethod]
        public void InvokeUserRegistrationPage()
        {

            this.UIMap.InvokeUserRegistrationPage();

        }

        

        [TestMethod]
        public void RegisterUser()
        {
           
            string fname1 = string.Empty;
            
            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and company line
                {
                    string[] cells = lines[1].Split(',');
                    fname1 = cells[0];
                   
                }

            }
            this.UIMap.SetUserDetails(fname1,"Test",fname1+"@mailinator.com",fname1+"@mailinator.com","ycUhFwSm1VMlhHwbjt3WBO0kbpageHHu","ycUhFwSm1VMlhHwbjt3WBO0kbpageHHu", "Australia", "0412 123456", "0411 123456", true);
            this.UIMap.CreateUser();
            Thread.Sleep(500);
            
        }

        
        [TestMethod]
        public void ReturnToLoginFromRegisterSuccess()
        {

            this.UIMap.ReturnToLoginFromRegisterSuccess();
            Thread.Sleep(500);
        }

        [TestMethod]
        public void LaunchMailinator()
        {
            this.UIMap.SetMailinatorURL();
            this.UIMap.LaunchMailinator();
            Thread.Sleep(500);
        }

        [TestMethod]
        public void CheckMailinatorInbox()
        {
            //Get User Details from the CSV file
            string fname1 = string.Empty;

            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and data line
                {
                    string[] cells = lines[1].Split(',');
                    fname1 = cells[0];
                }
                
            }
            
            this.UIMap.SetMailinatorInboxDetails(fname1+"@mailinator.com");
            this.UIMap.SearchMailinatorInbox();
            Thread.Sleep(5000);
        }

        [TestMethod]
        public void InvokeRegistrationEmail()
        {

            this.UIMap.InvokeRegistrationEmail();

        }

        [TestMethod]
        public void ConfirmRegistration()
        {

            this.UIMap.ConfirmRegistration();

        }

        [TestMethod]
        public void ReturnToLoginFromActivationSuccess()
        {

            this.UIMap.ReturnToLoginFromActivationSuccessPage();

        }

        [TestMethod]
        public void FreeTrialCreation()
        {
            this.UIMap.SetMyFirstProjectDetails("My First Company", "My First Project");
            this.UIMap.FreeTrialCreationLogin();
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void GetInvitation()
        {

            this.UIMap.SelectGetInvitationOption();

        }

        [TestMethod]
        public void CreateFTProjectandCompany()
        {
            string fname1 = string.Empty;

            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and data line
                {
                    string[] cells = lines[1].Split(',');
                    fname1 = cells[0];
                }

            }
            this.UIMap.SetSelectCreateFTProjectandCompanyOptionDetails(fname1+" Company", "Australia", fname1+" Project");
            this.UIMap.SelectCreateFTProjectandCompanyOption();

        }

        [TestMethod]
        public void CreateCompanyandInventory()
        {

            this.UIMap.SelectCreateCompanyandInventoryOption();
            this.UIMap.CreateTrialCompanyandInventory();

        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
