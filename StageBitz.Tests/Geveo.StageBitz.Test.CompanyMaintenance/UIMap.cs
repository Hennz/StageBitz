namespace Geveo.StageBitz.Test.CompanyMaintenance
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
    using System.Configuration;
    using System.Text;
    using System.IO;


    public partial class UIMap
    {
        public void SetCreateCompanyDetails(string company, string add1, string add2, string city, string state, string postal, string country, string phone, string web)
        {
            this.CreateCompanyParams.tbxCompanyNameText = company;
            this.CreateCompanyParams.tbxAddress1Text = add1;
            this.CreateCompanyParams.tbxAddress2Text = add2;
            this.CreateCompanyParams.tbxCityText = city;
            this.CreateCompanyParams.tbxStateText = state;
            this.CreateCompanyParams.tbxPostalCodeText = postal;
            this.CreateCompanyParams.cmbCountrySelectedItem = country;
            this.CreateCompanyParams.tbxPhone1Text = phone;
            this.CreateCompanyParams.tbxPhone2Text = web;
        }

        public void SetEditCompanyDetails(string company)
        {
            this.EditCompanyDetailsParams.tbxCompanyNameText = company;
        }

        public void SetInviteeLoginDetails(string username2, string pwd2)
        {
           this.LoginAsInviteeParams.tbxUserName1Text = username2;
           this.LoginAsInviteeParams.tbxPasswordPassword = pwd2;
        }
    }
}
