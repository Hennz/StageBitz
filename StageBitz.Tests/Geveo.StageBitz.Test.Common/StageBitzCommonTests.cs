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
using System.IO;
using System.Configuration;
using System.Threading;


namespace Geveo.StageBitz.Test.Common
{
    /// <summary>
    /// Summary description for StageBitzCommonTests
    /// </summary>
    [CodedUITest]
    public class StageBitzCommonTests
    {
        public StageBitzCommonTests()
        {
        }

        [TestMethod]
        public void LaunchUserWebURL()
        {
           
            //Open IE Browser Instance and Launch SB User Web URL
            this.UIMap.SetSBUserWebURL();
            this.UIMap.LaunchSBUserWebURL();
            Thread.Sleep(500);
           
        }

        [TestMethod]
        public void LoginUserPortal()
        {
            string fname1 = string.Empty;
            string username = string.Empty;


            string UsersDetailsFilePath = ConfigurationManager.AppSettings["UsersDetailsFilePath"].ToString();

            if (File.Exists(UsersDetailsFilePath))
            {
                string[] lines = File.ReadAllLines(UsersDetailsFilePath);

                if (lines.Length == 2) // header line and company line
                {
                    string[] cells = lines[1].Split(',');
                    fname1 = cells[0];
                    username = fname1+"@mailinator.com";

                }
            }

            this.UIMap.SetLoginDetails(username, "nAO6FE+d177p7uXi8Xo+YDTB4TP2UBvd");
            this.UIMap.LoginUserPortal();
            Thread.Sleep(5000);

        }

        [TestMethod]
        public void InvokePersonalDashboard()
        {

            this.UIMap.InvokePersonalDashboard();

        }

        [TestMethod]
        public void LogoutFromUserPortal()
        {

            this.UIMap.LogoutFromUserPortal();

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
