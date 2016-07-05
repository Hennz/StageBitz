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
using System.Threading;
using System.IO;


namespace Geveo.StageBitz.Test.CompanyMaintenance
{
    /// <summary>
    /// Summary description for CompanyMaintenance
    /// </summary>
    [CodedUITest]
    public class CompanyMaintenance
    {
        public CompanyMaintenance()
        {
        }

        /*[TestMethod]
        public void InvokeMyCompaniesPage()
        {
            //This test method assumes that the user is in Personal Dashboard
            this.UIMap.InvokeMyCompaniesPage();
            Thread.Sleep(1000);

        */

        [TestMethod]
        public void CreateNewCompany()
        {
            string fname = string.Empty;
            
            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    fname = cells[0];
                  
                }
            }

            string company = string.Empty;

            string CompanyDetailsFilePath = ConfigurationManager.AppSettings["CompanyDetailsFilePath"].ToString();

            if (File.Exists(CompanyDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(CompanyDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    company = cells[0];

                }
            }

            this.UIMap.InvokeCreateCompanyPage();
            this.UIMap.SetCreateCompanyDetails("z"+fname+" "+company, "89", "Crisp ct", "Bruce", "ACT", "2617", "Australia", "0412 123654", "www.autotest.com");
            this.UIMap.CreateCompany();


        }

        [TestMethod]
        public void InvokeCompanyDashboard() 
        {
            this.UIMap.InvokeCompanyDashboardFromMyCompanies();
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void EditCompanyDetails()
        {
            string fname = string.Empty;

            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    fname = cells[0];

                }
            }
            string company = string.Empty;

            string CompanyDetailsFilePath = ConfigurationManager.AppSettings["CompanyDetailsFilePath"].ToString();

            if (File.Exists(CompanyDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(CompanyDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    company = cells[0];

                }
            }

            this.UIMap.InvokeCompanyDashboardFromMyCompanies();
            this.UIMap.InvokeEditCompanyDetailsPage();
            this.UIMap.SetEditCompanyDetails("Test" + fname + " " + company);
            this.UIMap.InvokeChangeImagePopup();
            //this.UIMap.UploadCompanyProfileImage();
            Thread.Sleep(1000);
            this.UIMap.ChangeCompanyProfileImage();
            this.UIMap.EditCompanyDetails();

        }

        [TestMethod]
        public void InvokeCompanyDashboardFromMyCompanies()
        {
            this.UIMap.InvokeCompanyDashboardFromMyCompanies();
            Thread.Sleep(1000);
            

        }

        [TestMethod]
        public void InvokeManageCompanyTeamPage()
        {
            this.UIMap.InvokeManageAdministratorPage();
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void SearchInStageBitzForInvitation()
        {

            this.UIMap.SearchInStageBitzForInvitation();
            Thread.Sleep(1000);

        }

        [TestMethod]
        public void InviteUser()
        {

            this.UIMap.InviteUser();
            Thread.Sleep(1000);

        }

        [TestMethod]
        public void LoginAsInvitee()
        {

            
            string fname2 = string.Empty;
            string username2 = string.Empty;


            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and company line
                {
                    string[] cells = lines[1].Split(',');
                    fname2 = cells[1];
                    username2 = fname2 + "@mailinator.com";

                }
            }

            this.UIMap.SetInviteeLoginDetails(username2, "ycUhFwSm1VMlhHwbjt3WBO0kbpageHHu");
            this.UIMap.LoginAsInvitee();
            Thread.Sleep(5000);
        }

        [TestMethod]
        public void RespondToCompanyInvitation()
        {

            this.UIMap.InvokeCompanyInvitation();
            this.UIMap.RespondToCompanyInvitation();

        }

        [TestMethod]
        public void ChangeCompanyPermission()
        {

            this.UIMap.InvokePermissionSettingspopup();
            this.UIMap.ChangeCompanyPermission();

        }

        [TestMethod]
        public void RemoveCompanyUser()
        {

            this.UIMap.InvokeCompanyUserRemoveConfirmation();
            this.UIMap.RemoveCompanyUser();

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
