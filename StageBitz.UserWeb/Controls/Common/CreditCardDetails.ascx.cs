using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for CreditCard details.
    /// </summary>
    public partial class CreditCardDetails : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets the name of the credit card holder.
        /// </summary>
        /// <value>
        /// The name of the credit card holder.
        /// </value>
        public string CreditCardHolderName
        {
            get
            {
                return txtCardHolderName.Text.Trim();
            }
        }

        /// <summary>
        /// Gets the credit card number.
        /// </summary>
        /// <value>
        /// The credit card number.
        /// </value>
        public string CreditCardNumber
        {
            get
            {
                return txtCardNumber.Text.Replace(" ", "").Trim();
            }
        }

        /// <summary>
        /// Gets the CVV number.
        /// </summary>
        /// <value>
        /// The CVV number.
        /// </value>
        public string CVVNumber
        {
            get
            {
                return txtCVV.Text.Trim();
            }
        }

        /// <summary>
        /// Gets the selected month.
        /// </summary>
        /// <value>
        /// The selected month.
        /// </value>
        public int SelectedMonth
        {
            get
            {
                return ddMonth.SelectedIndex + 1;
            }
        }

        /// <summary>
        /// Gets the selected year.
        /// </summary>
        /// <value>
        /// The selected year.
        /// </value>
        public int SelectedYear
        {
            get
            {
                return int.Parse(ddYear.SelectedItem.Value);
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadExpireDateDropdowns();
                ClearValues();
            }
        }

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Sets the validation group.
        /// </summary>
        /// <param name="ValidationGroup">The validation group.</param>
        public void SetValidationGroup(string ValidationGroup)
        {
            reqCardHolderName.ValidationGroup = ValidationGroup;
            reqCardNumber.ValidationGroup = ValidationGroup;
            reqCVV.ValidationGroup = ValidationGroup;
            ccValidator.ValidationGroup = ValidationGroup;
            regexCVV.ValidationGroup = ValidationGroup;

            txtCardHolderName.ValidationGroup = ValidationGroup;
            txtCardNumber.ValidationGroup = ValidationGroup;
            txtCVV.ValidationGroup = ValidationGroup;
        }

        /// <summary>
        /// Shows the credit card editable labels.
        /// </summary>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        public void ShowCreditCardEditableLabels(bool isVisible)
        {
            divCCDetailsEditableLabels.Visible = isVisible;
            divCCDetailsEditable.Visible = isVisible;
        }

        /// <summary>
        /// Sets the notification.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetNotification(string text)
        {
            divNotification.InnerText = text;
            divNotification.Visible = true;
        }

        /// <summary>
        /// Sets the notification dates.
        /// </summary>
        public void SetNotificationDates()
        {
            divNotificationDates.InnerText = "Invalid Expiry Date";
            divNotificationDates.Style["display"] = "Inline-block";
        }

        /// <summary>
        /// Adds the styles to editable labels.
        /// </summary>
        public void AddStylesToEditableLabels()
        {
            divCCDetailsEditableLabels.Style.Add("margin-left", "0");
        }

        /// <summary>
        /// Clears the values of inputs.
        /// </summary>
        public void ClearValues()
        {
            txtCardHolderName.Text = string.Empty;
            txtCardNumber.Text = string.Empty;
            txtCVV.Text = string.Empty;
            ddYear.SelectedIndex = 0;
            ddMonth.SelectedIndex = Today.Month - 1;
            divNotificationDates.InnerHtml = string.Empty;
            divNotificationDates.Style["display"] = "None";
            divNotification.InnerHtml = string.Empty;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the expire date dropdowns.
        /// </summary>
        private void LoadExpireDateDropdowns()
        {
            DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(null);
            List<int> yearsList = new List<int>();
            yearsList.Add(Today.Year);
            for (int i = 1; i < 13; i++)
            {
                ddMonth.Items.Add(new ListItem(info.GetMonthName(i), i.ToString()));

                if (i < 5)
                {
                    yearsList.Add(Today.AddYears(i).Year);
                }
            }
            ddYear.DataSource = yearsList;
            ddYear.DataBind();
        }

        #endregion Private Methods
    }
}