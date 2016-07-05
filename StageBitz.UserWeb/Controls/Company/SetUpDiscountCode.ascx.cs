using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// Delegate for inform parent to update discount usage.
    /// </summary>
    public delegate void InformParentToUpdateDiscountUsage();

    /// <summary>
    /// User control for setup discount codes.
    /// </summary>
    public partial class SetUpDiscountCode : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Enum for view mode.
        /// </summary>
        public enum ViewMode
        {
            PricingPlan,
            CreateNewCompany
        }

        #endregion Enums

        #region Events

        /// <summary>
        /// The inform parent to update discount usage
        /// </summary>
        public InformParentToUpdateDiscountUsage InformParentToUpdateDiscountUsage;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public ViewMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(ViewMode);
                }

                return (ViewMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public DiscountCode DiscountCode
        {
            get
            {
                return (DiscountCode)ViewState["DiscountCode"];
            }
            set
            {
                ViewState["DiscountCode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the discount code usage.
        /// </summary>
        /// <value>
        /// The discount code usage.
        /// </value>
        public DiscountCodeUsage DiscountCodeUsage
        {
            get
            {
                return (DiscountCodeUsage)ViewState["DiscountCodeUsage"];
            }
            set
            {
                ViewState["DiscountCodeUsage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        public int CompanyId
        {
            get
            {
                if (ViewState["companyid"] == null)
                {
                    ViewState["companyid"] = 0;
                }

                return (int)ViewState["companyid"];
            }
            set
            {
                ViewState["companyid"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Shows the set up discount code.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ShowSetUpDiscountCode(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                popupManageDiscount.ShowPopup();
                spanErrorMsg.Visible = false;
                txtDiscountCode.Text = string.Empty;
                txtDiscountCode.Focus();
                spanDiscountavailable.Visible = true;
                spanDiscountavailable.Visible = (this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId) != null);
            }
        }

        /// <summary>
        /// Sets up discount.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SetUpDiscount(object sender, EventArgs e)
        {
            SetupDiscount();
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the discount codes.
        /// </summary>
        public void LoadDiscountCodes()
        {
            #region DiscountCode Build

            //Build the DiscountCode from Database or Memory
            //First check from the Memory
            if (DiscountCodeUsage != null)
            {
                // Get the applied Discount
                litDiscountApplied.Text = "\"" + DiscountCode.Code + "\"";
                lnkSetUpDiscountCode.Text = "Replace";
                divDiscountSet.Visible = true;
                //show the tooltip
                StringBuilder msg = new StringBuilder();
                msg.Append((DiscountCode.Discount / 100).ToString("#0.##%", CultureInfo.InvariantCulture));
                msg.Append(" ");
                msg.Append("Discount from ");
                msg.Append(Support.FormatDate(Utils.Today));
                msg.Append(" to ");
                //msg.Append(Support.FormatDate(Utils.Today.AddDays(DiscountCode.Duration * 7)));
                msg.Append(Support.FormatDate(DiscountCodeUsage.EndDate));
                imgDiscountCode.Attributes.Add("Title", msg.ToString());
                return;
            }
            else
            {
                //Else get it from the Discount Usage.
                DiscountCodeUsage = this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId);

                //Still the usage can be NULL, because the company might not have a Discount code at all.
                if (DiscountCodeUsage != null)
                    DiscountCode = DiscountCodeUsage.DiscountCode;
            }

            #endregion DiscountCode Build

            //Does not have a Discount for the company
            if (DiscountCodeUsage == null)
            {
                lnkSetUpDiscountCode.Text = "Apply Code";
                divDiscountSet.Visible = false;
            }
            else
            {
                //If it has a Discount, That can be either from Memory or DB.
                litDiscountApplied.Text = "\"" + DiscountCode.Code + "\"";
                lnkSetUpDiscountCode.Text = "Replace";
                divDiscountSet.Visible = true;
                //show the tooltip
                StringBuilder msg = new StringBuilder();
                msg.Append((DiscountCode.Discount / 100).ToString("#0.##%", CultureInfo.InvariantCulture));
                msg.Append(" ");
                msg.Append("Discount from ");
                msg.Append(Support.FormatDate(DiscountCodeUsage.CreatedDate));
                msg.Append(" to ");
                msg.Append(Support.FormatDate(DiscountCodeUsage.EndDate.Date));
                imgDiscountCode.Attributes.Add("Title", msg.ToString());
            }
        }

        /// <summary>
        /// Sets the read only.
        /// </summary>
        public void SetReadOnly()
        {
            divDiscountSet.Visible = false;
            lnkSetUpDiscountCode.Visible = false;
        }

        /// <summary>
        /// Sets the disable.
        /// </summary>
        public void SetDisable()
        {
            lnkSetUpDiscountCode.Enabled = false;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Setups the discount.
        /// </summary>
        private void SetupDiscount()
        {
            if (!PageBase.StopProcessing)
            {
                string errorMsg;
                bool canUseDiscount = GetBL<FinanceBL>().IsDiscountCodeValidToUse(txtDiscountCode.Text.Trim(), CompanyId, out errorMsg);
                if (!canUseDiscount)
                {
                    spanErrorMsg.InnerText = errorMsg;
                    spanErrorMsg.Visible = true;
                    return;
                }

                DiscountCode discountCode = DataContext.DiscountCodes.Where(dc => dc.Code == txtDiscountCode.Text.Trim()).FirstOrDefault();
                DiscountCode = discountCode;

                spanErrorMsg.Visible = false;
                DiscountCodeUsage = this.GetBL<FinanceBL>().SaveDiscountCodeUsageToCompany(discountCode, UserID, CompanyId);

                popupManageDiscount.HidePopup();
                LoadDiscountCodes();

                //Trigger an event to parent
                if (InformParentToUpdateDiscountUsage != null)
                {
                    InformParentToUpdateDiscountUsage();
                }
            }
        }

        #endregion Private Methods
    }
}