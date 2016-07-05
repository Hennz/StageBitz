namespace CompanyInventory
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
        public void SetItemNameforSearch(string searchname)
        {
            this.SearchItembyNameParams.UICboSearchEditText = searchname;
        }

        public void SetAddItemDetails(string additemname, string additemdesc, string additemqty, string additemitemtype)
        {
            this.AddInventoryItemParams.UITxtNameEditText = additemname;
            this.AddInventoryItemParams.UITxtDescriptionEditText = additemdesc;
            this.AddInventoryItemParams.UITxtQuantityEditText = additemqty;
            this.AddInventoryItemParams.UIDdlAddItemTypesComboBoxSelectedItem = additemitemtype;
        }

        public void SetEditItemDetails(string edititemname, string edititemqty, string edititemcreatedfor, string edititemdesc)
        {
            this.EditItemDetailsParams.UIEditInputEditText = edititemname;
            this.EditItemDetailsParams.UITxtItemQuantityEditText = edititemqty;
            this.EditItemDetailsParams.UITxtCreatedForEditText = edititemcreatedfor;
            this.EditItemDetailsParams.UITxtDescriptionEditText = edititemdesc;
           
        }



 

        /// <summary>
        /// DeleteItem
        /// </summary>
        public void DeleteItem()
        {
            #region Variable Declarations
            HtmlInputButton uIDeleteItemButton = this.UIStageBitzInternetExpWindow.UIStageBitzDocument7.UIDeleteItemButton;
            HtmlInputButton uIConfirmButton = this.UIStageBitzInternetExpWindow.UIStageBitzDocument7.UIConfirmButton;
            #endregion

            // Click 'Delete Item' button
            Mouse.Click(uIDeleteItemButton, new Point(34, 13));

            // Click 'Confirm' button
            Mouse.Click(uIConfirmButton, new Point(14, 5));
}



        /// <summary>
        /// InvokeItemDetailsPage
        /// </summary>
        public void InvokeItemDetailsPage()
        {
            #region Variable Declarations
            HtmlHeaderCell uINameCell = this.UIABCnetworksInventoryWindow.UIABCnetworksInventoryDocument2.UICtl00_ctl00_MainContTable.UINameCell;
            HtmlCell uIXenonCell = this.UIABCnetworksInventoryWindow.UIABCnetworksInventoryDocument2.UICtl00_ctl00_MainContTable1.UIXenonCell;
            HtmlHyperlink uIXenonHyperlink = this.UIABCnetworksInventoryWindow.UIABCnetworksInventoryDocument2.UIXenonHyperlink;
            #endregion

            // Set flag to allow play back to continue if non-essential actions fail. (For example, if a mouse hover action fails.)
            Playback.PlaybackSettings.ContinueOnError = true;

            // Mouse hover 'Name' cell at (1, 1)
            Mouse.Hover(uINameCell, new Point(1, 1));

            // Mouse hover 'Xenon' cell at (1, 1)
            Mouse.Hover(uIXenonCell, new Point(1, 1));

            // Reset flag to ensure that play back stops if there is an error.
            Playback.PlaybackSettings.ContinueOnError = false;

            uIXenonHyperlink.SearchProperties.Add("InnerText", "Xenon", PropertyExpressionOperator.EqualTo);
            // Click 'Xenon' link
            Mouse.Click(uIXenonHyperlink, new Point(14, 12));
        }

        public void SetNPBName(string npbname)
        {
            this.CreateNonProjectBookingParams.UITxtNewBookingNameEditText = npbname;
        }
    }
}
