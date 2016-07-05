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


namespace Geveo.StageBitz.Test.ProjectMaintanance
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class ProjectMaintanance
    {
        public ProjectMaintanance()
        {
        }


        [TestMethod]
        public void NavigateToCompanyDashboard()
        {

            this.UIMap.NavigateToCompanyDashboard();

        }

        [TestMethod]
        public void NavigateToProjectDashboard()
        {

            this.UIMap.NavigateToProjectDashboard();


        }

        [TestMethod]
        public void ClickCreateNewProjectLink()
        {

            this.UIMap.clickCreateNewProjectLink();


        }

        [TestMethod]
        public void EnterProjectName()
        {

            string pname = string.Empty;

            string projectDetailsFilePath = ConfigurationManager.AppSettings["ProjectDetailsFilePath"].ToString();

            if (File.Exists(projectDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(projectDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    pname = cells[0];

                }
            }

            this.UIMap.SetProjectName(pname);
            this.UIMap.EnterProjectName();


        }

        [TestMethod]
        public void EnterProjectDetails()
        {

            string pname = string.Empty;
            string startDate = string.Empty;
            string endDate = string.Empty;
            string keyEvent = string.Empty;
            string keyDate = string.Empty;
            string lName = string.Empty;
            string location = string.Empty;
            int noOfPeople = 0;

            string projectDetailsFilePath = ConfigurationManager.AppSettings["ProjectDetailsFilePath"].ToString();

            if (File.Exists(projectDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(projectDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    pname = cells[0];
                    startDate = cells[1];
                    endDate = cells[2];
                    keyEvent = cells[3];
                    keyDate = cells[4];
                    lName = cells[5];
                    location = cells[6];
                    noOfPeople = 5;

                    //string[] cells = lines[1].Split(',');
                    //pname = "ProjectEdit";
                    //startDate = "6-Mar-13";
                    //endDate = "1-Mar-15";
                    //keyEvent = "Rehearsals starts";
                    //keyDate = "2-Mar-13";
                    //lName = "Studio1";
                    //location = "3-Smith St-Smithville-Australia";
                    //noOfPeople = 5;

                }
            }

            this.UIMap.SetProjectDetails(pname, startDate, endDate, keyEvent, keyDate, lName, location,noOfPeople);
            this.UIMap.EnterProjectDetails();

        }

        //CreditcardDetailsFilePath
        //[TestMethod]
        //public void EnterCreditCardDetails()
        //{

            
        //    string name = string.Empty;
        //    string cardNo = string.Empty;
        //    //string month = string.Empty;
        //    //string year = string.Empty;
        //    string code = string.Empty;

        //    string creditCardDetailsFilePath = ConfigurationManager.AppSettings["CreditcardDetailsFilePath"].ToString();

        //    if (File.Exists(creditCardDetailsFilePath))
        //    {
        //        string[] lines = File.ReadAllLines(creditCardDetailsFilePath);

        //        if (lines.Length == 2) // header line and 1st data line
        //        {
        //            string[] cells = lines[1].Split(',');
        //            name = cells[0];
        //            cardNo = cells[1];
        //            //month = cells[2];
        //            code = cells[2];

        //        }
        //    }

        //    this.UIMap.SetCreditCardDetails(name, cardNo, code);

        //    this.UIMap.EnterCreditCardDetails();

        //}

        [TestMethod]
        public void EnterCreditCardDetails()
        {

            string name = string.Empty;
            string cardNo = string.Empty;
            string month = string.Empty;
            string year = string.Empty;
            string code = string.Empty;

            string creditCardDetailsFilePath = ConfigurationManager.AppSettings["CreditcardDetailsFilePath"].ToString();

            if (File.Exists(creditCardDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(creditCardDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    name = cells[0];
                    cardNo = cells[1];
                    month = cells[2];
                    year = cells[3];
                    code = cells[4];

                }
            }

            //this.UIMap.SetCreditCardDetails(name, cardNo, "April", "2017", code);
            this.UIMap.SetCreditCardDetails(name, cardNo, month, "2017", year);
            this.UIMap.EnterCreditCardDetails();
            //this.UIMap.RecordedMethod1(); 
        }

        [TestMethod]
        public void NavigateToProjectDetailsPage()
        {

            this.UIMap.NavigateToProjectDetailsPage();

        }

        [TestMethod]
        public void NavigateToProjectDetailsPageFromAdminSection()
        {

            this.UIMap.NavigateToProjectDetailsPageFromAdminSection();


        }

        [TestMethod]
        public void EditProjectDetails()
        {

            
            string newPname = string.Empty;
            string NewLName = string.Empty;
            string NewLocation = string.Empty;

            string projectDetailsFilePath = ConfigurationManager.AppSettings["ProjectDetailsFilePath"].ToString();

            if (File.Exists(projectDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(projectDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    newPname = cells[8];
                    NewLName = cells[9];
                    NewLocation = cells[10];

                }
            }

            this.UIMap.SetEditProjectDetails(newPname, NewLName, NewLocation);
            this.UIMap.EditProjectDetails();

        }

        [TestMethod]
        public void EditAndRemoveLocation()
        {
            
            string inlineEditLName = string.Empty;
            string inlineEditLocation = string.Empty;

            string projectDetailsFilePath = ConfigurationManager.AppSettings["ProjectDetailsFilePath"].ToString();

            if (File.Exists(projectDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(projectDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    inlineEditLName = cells[11];
                    inlineEditLocation = cells[12];

                }
            }

            this.UIMap.SetInlineEditLocation(inlineEditLName,inlineEditLocation);
            this.UIMap.EditAndRemoveLocation();
          

        }

        [TestMethod]
        public void NavigateToManageShedulePage()
        {

            this.UIMap.NavigateToManageShedulePage();
        }

        [TestMethod]
        public void EditShedule()
        {

            
            string startDate = string.Empty;
            string endDate = string.Empty;
            string keyEvent = string.Empty;
            string keyDate = string.Empty;

            string projectDetailsFilePath = ConfigurationManager.AppSettings["ProjectDetailsFilePath"].ToString();

            if (File.Exists(projectDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(projectDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    startDate = cells[1];
                    endDate = cells[2];
                    keyEvent = cells[3];
                    keyDate = cells[4];

                }
            }

            this.UIMap.SetSheduleDetails(startDate, endDate, keyEvent, keyDate);
            this.UIMap.EditShedule();
        }

        [TestMethod]
        public void AddItemTypes()
        {

            this.UIMap.AddItemTypes();


        }

        [TestMethod]
        public void NavigateToManageTeamPage()
        {

            this.UIMap.NavigateToManageTeamPage();

        }


        [TestMethod]
        public void NavigateToManageProjectTeamPage()
        {

            this.UIMap.NavigateToManageProjectTeamPage();


        }

        //[TestMethod]
        //public void SearchInStageBitzAndInvite()
        //{

        //    string email = string.Empty;

        //    string inviteUserDetailsFilePath = ConfigurationManager.AppSettings["InviteUsersDetailsFilePath"].ToString();

        //    if (File.Exists(inviteUserDetailsFilePath))
        //    {
        //        string[] lines = File.ReadAllLines(inviteUserDetailsFilePath);

        //        if (lines.Length == 2) // header line and 1st data line
        //        {
        //            string[] cells = lines[1].Split(',');
        //            email = cells[2];

        //        }
        //    }

        //    this.UIMap.SetSearchInStageBitzDetails(email);
        //    this.UIMap.SearchInStageBitzAndInvite();

        //}

        [TestMethod]
        public void NavigateToProjectDashboardInBreadcrum()
        {

            this.UIMap.NavigateToProjectDashboardInBreadcrum();

        }

        [TestMethod]
        public void SearchInStageBitz()
        {

            string email = string.Empty;

            string inviteUserDetailsFilePath = ConfigurationManager.AppSettings["InviteUsersDetailsFilePath"].ToString();

            if (File.Exists(inviteUserDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(inviteUserDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    email = cells[2];

                }
            }

            this.UIMap.SetSearchInStageBitzDetails(email);
            this.UIMap.SearchInStageBitz();

            //NavigateToProjectDashboardInBreadcrum();
            //NavigateToManageTeamPage();
        }

        [TestMethod]
        public void EditMemberRole()
        {

            this.UIMap.EditMemberRole();

        }

        [TestMethod]
        public void RemoveTeamMember()
        {

            this.UIMap.RecordedMethod2();

        }

        [TestMethod]
        public void SendInvitation()
        {

            this.UIMap.SendInvitation();

        }

        [TestMethod]
        public void SearchInMyContacts()
        {

            string fname = string.Empty;
            string lname = string.Empty;

            string inviteUserDetailsFilePath = ConfigurationManager.AppSettings["InviteUsersDetailsFilePath"].ToString();

            if (File.Exists(inviteUserDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(inviteUserDetailsFilePath);

                if (lines.Length == 2) // header line and 1st data line
                {
                    string[] cells = lines[1].Split(',');
                    fname = cells[0];
                    lname = cells[1];

                }
            }

            this.UIMap.SetSearchInMyContactDetails(fname, lname);
            this.UIMap.SearchInMyContacts();
            //NavigateToProjectDashboardInBreadcrum();
        }

        [TestMethod]
        public void NavigateToProjectDashboardUsingBreadcrumb()
        {

            this.UIMap.NavigateToProjectDashboardUsingBreadcrumb();

        }

        [TestMethod]
        public void NavigateToPersonalDashboardUsingBreadcrumb()
        {

            this.UIMap.NavigateToPersonalDashboardUsingBreadcrumb();

        }


        [TestMethod]
        public void EnterStartEndDates()
        {

            this.UIMap.EnterStartEndDates();

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
