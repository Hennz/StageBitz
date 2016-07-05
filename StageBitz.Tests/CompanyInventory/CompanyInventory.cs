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


namespace CompanyInventory
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CompanyInventory
    {
        public CompanyInventory()
        {
        }

        //Open the Company Inventory Page From the User's Personal Dashboard.
        [TestMethod]
        public void InvokeCompanyInventoryPage()
        {
            
            this.UIMap.InvokeCompanyInventoryPage();
            Thread.Sleep(300);

        }

        //Add an Item to the inventory using Add Panel.
        [TestMethod]
        public void AddInventoryItem()
        {
            this.UIMap.SetAddItemDetails("Xenon", "Comes under lighting", "100", "Lighting");
            this.UIMap.AddInventoryItem();
            Thread.Sleep(300);
        }
        
        //Search for an Item by The Item Name.
        [TestMethod]
        public void SearchItembyName()
        {
            this.UIMap.SetItemNameforSearch("Neon");
            this.UIMap.SearchItembyName();
            Thread.Sleep(300);

        }

        //Switch to watchlist mode from search mode.
        [TestMethod]
        public void SwitchbetweenSearchandWatclist()
        {

            this.UIMap.SwitchbetweenSearchandWatchlist();
            Thread.Sleep(300);

        }

        //Switch back to search mode from watchlist.
        [TestMethod]
        public void SwitchtoSearchfromWatchlist()
        {

            this.UIMap.SwitchtoSearchfromWatchlist();
            Thread.Sleep(300);

        }

        //Create Inventory Search PDF Report.
        [TestMethod]
        public void CreateInventoryPDF()
        {

            this.UIMap.CreateInventoryPDF();
            Thread.Sleep(300);
        }

        //Create Inventory Search Excel.
        [TestMethod]
        public void CreateInventoryExcel()
        {

            this.UIMap.CreateInventoryExcel();

        }

        //Filter Inventory Items by Item Type.
        [TestMethod]
        public void ItemTypeFilteration()
        {

            this.UIMap.ItemTypeFilteration();
            Thread.Sleep(300);
            this.UIMap.ResetItemTypeFilteration();
            Thread.Sleep(300);

        }

        //Filter Inventory Items by Locations.
        [TestMethod]
        public void LocationFilteration()
        {

            this.UIMap.LocationFilteration();
            Thread.Sleep(300);
            this.UIMap.ResetLocationFilteration();
            Thread.Sleep(300);

        }

        //Viewing Shared Inventory Items.
        [TestMethod]
        public void ViewSharedInventory()
        {

            this.UIMap.ViewSharedInventory();
            Thread.Sleep(300);
            this.UIMap.ViewAllSharedInventories();
            Thread.Sleep(300);
            this.UIMap.NavigatebacktoMyInventory();
            Thread.Sleep(300);

        }

        //Bulk Import Inventory Items.
        [TestMethod]
        public void InventoryItemBulkImport()
        {

            this.UIMap.InventoryItemsBulkImport(); 
            Thread.Sleep(300);

        }

        //Invoke Item Details page.
        [TestMethod]
        public void InvokeItemDetails()
        {

            this.UIMap.InvokeItemDetailsPage();
            Thread.Sleep(300);

        }

        //Navigate to Inventory Search Using Breadcrumb links on the Item Details page.
        [TestMethod]
        public void NavigatebacktoInventoryusingBreadcrumb()
        {

            
            this.UIMap.NavigatebacktoInventoryusingBreadcrumb();
            Thread.Sleep(300);

        }

        //Edit Input Fields in Item Details page.
        [TestMethod]
        public void EditItemDetails()
        {
            this.UIMap.InvokeItemDetailsPage();
            Thread.Sleep(300);
            this.UIMap.SetEditItemDetails("Edited", "10", "Edited", "Edited");
            this.UIMap.EditItemDetails();
            Thread.Sleep(300);
            //this.UIMap.InvokeItemDetailsPage();
            //Thread.Sleep(300);
            //this.UIMap.UploadItemAttachments();
            //Thread.Sleep(300);
            //this.UIMap.ChangePreviewImageforItem();
            //Thread.Sleep(300);
            //this.UIMap.NavigatebacktoInventoryusingBreadcrumb();
            //Thread.Sleep(300);
        }

        //Switching between thumbnail and list view of Inventory Search Results.
        [TestMethod]
        public void SearchViewSwtching()
        {

            this.UIMap.SwitchfromListtoThumbnail();
            Thread.Sleep(300);
            this.UIMap.SwitchfromThumbnailtoList();
            Thread.Sleep(300);
        }

        //Invoke Manager mode of the inventory.
        [TestMethod]
        public void InvokeManagerMode()
        {

            this.UIMap.InvokeManagerMode();
            Thread.Sleep(300);
        }

        //Change Location from Manager Mode.
        [TestMethod]
        public void EditLocationManagerMode()
        {

            this.UIMap.EditItemLocationfromManagerMode();
            Thread.Sleep(300);
        }

        //Change Visibility Settings from Manager Mode.
        [TestMethod]
        public void EditVisibilitySettings()
        {

            this.UIMap.EditVisibilitySettings();
            Thread.Sleep(300);
        }

        //Navigate Back to Inventory from Manager Mode.
        [TestMethod]
        public void SwitchbacktoInventoryfromManagerMode()
        {

            this.UIMap.SwitchbacktoInventoryfromManagerMode();
            Thread.Sleep(300);
        }

        //Create a non project Booking.
        [TestMethod]
        public void CreateNonProjectBooking()
        {
            this.UIMap.SetNPBName("Victoria");
            this.UIMap.CreateNonProjectBooking();
            Thread.Sleep(300);
        }

        //Pin Item to a Project Booking.
        [TestMethod]
        public void PinItemProjectBooking()
        {
            this.UIMap.SelectaProject();
            Thread.Sleep(300);
            this.UIMap.SelectFromDateforBooking();
            Thread.Sleep(300);
            this.UIMap.SelectToDateforBooking();
            Thread.Sleep(300);
            this.UIMap.SelectItemforBooking();
            Thread.Sleep(300);
            this.UIMap.PinItemtoProjectBooking();
            Thread.Sleep(300);
        }

        //Pin Item to a non Project Booking.
        [TestMethod]
        public void PinItemNPB()
        {

            this.UIMap.SelectNPB();
            Thread.Sleep(300);
            this.UIMap.SelectFromDateforBooking();
            Thread.Sleep(300);
            this.UIMap.SelectToDateforBooking();
            Thread.Sleep(300);
            this.UIMap.SelectItemforBooking();
            Thread.Sleep(300);
            this.UIMap.PinItemtoNPB();
        }


        //Reset the Search Results.
        [TestMethod]
        public void ResetSearch()
        {

            this.UIMap.ResetSearchResults();

        }

        //Search For a Specific Item and delete it.
        [TestMethod]
        public void DeleteItem()
        {
            this.UIMap.InvokeItemDetailsPage();
            Thread.Sleep(300);
            this.UIMap.DeleteInventoryItem();
            Thread.Sleep(300);
        }

        //Search for a Specific Item with future booking and delete it.
        [TestMethod]
        public void DeleteItemwithFutureBooking()
        {
            this.UIMap.InvokeItemDetailsPage();
            this.UIMap.DeleteItemwithFutureBooking();

        }

        

        
        //
        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
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
