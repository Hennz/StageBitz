namespace Geveo.StageBitz.Test.ProjectMaintanance
{
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
    
    
    public partial class UIMap
    {
        public void SetProjectDetails(string pname,string startDate,string endDate,string keyEvent,string keyDate,string lName,string location,int noOfPeople)
        {
            this.EnterProjectDetailsParams.tbxProjectNameText = pname;
            this.EnterProjectDetailsParams.tbxStartDateText= startDate;
            this.EnterProjectDetailsParams.tbxEndDateText = endDate;
            this.EnterProjectDetailsParams.tbxEventText = keyEvent;
            this.EnterProjectDetailsParams.tbxEventDateText = keyDate;
            this.EnterProjectDetailsParams.tbxLNameText = lName;
            this.EnterProjectDetailsParams.tbxLocationText = location;
            this.EnterProjectDetailsParams.tbxNoOfPeopleText = noOfPeople.ToString();
        }
       
        //public void SetCreditCardDetails(string name, string cardNo, string code)
        //{
        //    this.EnterCreditCardDetailsParams.tbxCardHolderNameText = name;
        //    this.EnterCreditCardDetailsParams.tbxCreditCardNoText = cardNo;
        //    //this.EnterCreditCardDetailsParams.tbxMonthSelectedItem = month;
        //    this.EnterCreditCardDetailsParams.tbxCVVText = code;
        //}

        public void SetCreditCardDetails(string name, string cardNo, string month, string year, string code)
        {
            this.RecordedMethod1Params.tbxCardHolderNameText = name;
            this.RecordedMethod1Params.tbxCreditCardNoText = cardNo;
            this.RecordedMethod1Params.cmbMonthSelectedItem = month;
            this.RecordedMethod1Params.cmbYearSelectedItem = year;
            this.RecordedMethod1Params.tbxCVCText = code;
        
        }



        public void SetProjectName(string pname)
        {
            this.EnterProjectNameParams.tbxProjectNameText = pname;
        }

        public void SetEditProjectDetails(string newPname, string NewLName, string NewLocation)
        {
            this.EditProjectDetailsParams.tbxEditNameText = newPname;
            this.EditProjectDetailsParams.tbxEditLNameText = NewLName;
            this.EditProjectDetailsParams.tbxEditLocationText = NewLocation;
        }

        public void SetInlineEditLocation(string inlineEditLName, string newLocation)
        {
            this.EditProjectDetailsParams.tbxEditNameText = inlineEditLName;
            this.EditProjectDetailsParams.tbxEditLNameText = newLocation;
        }

        public void SetSheduleDetails(string startDate, string endDate, string keyEvent, string keyDate)
        {
            this.EditSheduleParams.tbxStartDateText = startDate;
            this.EditSheduleParams.tbxEndDateText = endDate;
            this.EditSheduleParams.tbxEventText = keyEvent;
            this.EditSheduleParams.tbxEventDateText = keyDate;
        }

        public void SetSearchInMyContactDetails(string fname, string lname)
        {
            this.SearchInMyContactsParams.tbxFirstNameTextText = fname;
            this.SearchInMyContactsParams.tbxLastNameTextText = lname;
        }

        public void SetSearchInStageBitzDetails(string email)
        {
            this.SearchInStageBitzParams.tbxEmailText = email;

        }
    }
}
