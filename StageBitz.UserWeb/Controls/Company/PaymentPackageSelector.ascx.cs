using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes.Finance;
using StageBitz.Logic.Business.Finance;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// Delegate for inform parent to update.
    /// </summary>
    /// <param name="projectPaymentPackageDetailId">The project payment package detail identifier.</param>
    /// <param name="inventoryPaymentPackageDetailId">The inventory payment package detail identifier.</param>
    /// <param name="paymentDurationTypeCodeId">The payment duration type code identifier.</param>
    /// <param name="hasAllpackagesSelected">if set to <c>true</c> [has allpackages selected].</param>
    public delegate void InformParentToUpdate(int projectPaymentPackageDetailId, int inventoryPaymentPackageDetailId, int paymentDurationTypeCodeId, bool hasAllpackagesSelected);

    /// <summary>
    ///
    /// </summary>
    public partial class PaymentPackageSelector : UserControlBase
    {
        /// <summary>
        /// The inform parent to update
        /// </summary>
        public InformParentToUpdate InformParentToUpdate;

        #region Properties

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        private int CompanyId
        {
            get
            {
                if (ViewState["companyid"] == null)
                {
                    int companyId = 0;

                    if (Request["companyid"] != null)
                    {
                        int.TryParse(Request["companyid"], out companyId);
                    }

                    ViewState["companyid"] = companyId;
                }

                return (int)ViewState["companyid"];
            }
        }

        /// <summary>
        /// Gets or sets the payment duration type code id.
        /// </summary>
        /// <value>
        /// The payment duration type code id.
        /// </value>
        public int PaymentDurationTypeCodeId
        {
            get
            {
                if (ViewState["paymentDurationTypeCodeId"] == null)
                {
                    ViewState["paymentDurationTypeCodeId"] = Utils.GetCodeByValue("PaymentPackageDuration", "MONTHLY").CodeId;
                }

                return (int)ViewState["paymentDurationTypeCodeId"];
            }
            set
            {
                ViewState["paymentDurationTypeCodeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project payment package type id.
        /// </summary>
        /// <value>
        /// The project payment package type id.
        /// </value>
        public int ProjectPaymentPackageTypeId
        {
            get
            {
                if (ViewState["ProjectPaymentPackageTypeId"] == null)
                {
                    ViewState["ProjectPaymentPackageTypeId"] = 0;
                }

                return (int)ViewState["ProjectPaymentPackageTypeId"];
            }
            set
            {
                ViewState["ProjectPaymentPackageTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the inventory payment package type id.
        /// </summary>
        /// <value>
        /// The inventory payment package type id.
        /// </value>
        public int InventoryPaymentPackageTypeId
        {
            get
            {
                if (ViewState["InventoryPaymentPackageTypeId"] == null)
                {
                    ViewState["InventoryPaymentPackageTypeId"] = 0;
                }

                return (int)ViewState["InventoryPaymentPackageTypeId"];
            }
            set
            {
                ViewState["InventoryPaymentPackageTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current project payment package type id.
        /// </summary>
        /// <value>
        /// The current project payment package type id.
        /// </value>
        public int CurrentProjectPaymentPackageTypeId
        {
            get
            {
                if (ViewState["CurrentProjectPaymentPackageTypeId"] == null)
                {
                    ViewState["CurrentProjectPaymentPackageTypeId"] = 0;
                }

                return (int)ViewState["CurrentProjectPaymentPackageTypeId"];
            }
            set
            {
                ViewState["CurrentProjectPaymentPackageTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current inventory payment package type identifier.
        /// </summary>
        /// <value>
        /// The current inventory payment package type identifier.
        /// </value>
        public int CurrentInventoryPaymentPackageTypeId
        {
            get
            {
                if (ViewState["CurrentInventoryPaymentPackageTypeId"] == null)
                {
                    ViewState["CurrentInventoryPaymentPackageTypeId"] = 0;
                }

                return (int)ViewState["CurrentInventoryPaymentPackageTypeId"];
            }
            set
            {
                ViewState["CurrentInventoryPaymentPackageTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] != null)
                {
                    return (bool)ViewState["IsReadOnly"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            //Load the selected package
            CompanyPaymentPackage companyPaymentPackage = this.GetBL<FinanceBL>().GetCurrentPaymentPackageFortheCompanyIncludingFreeTrial(CompanyId);
            if (companyPaymentPackage != null)
            {
                CurrentInventoryPaymentPackageTypeId = companyPaymentPackage.InventoryPaymentPackageTypeId;
                CurrentProjectPaymentPackageTypeId = companyPaymentPackage.ProjectPaymentPackageTypeId;
                ProjectPaymentPackageTypeId = companyPaymentPackage.ProjectPaymentPackageTypeId;
                InventoryPaymentPackageTypeId = companyPaymentPackage.InventoryPaymentPackageTypeId;
                PaymentDurationTypeCodeId = companyPaymentPackage.PaymentDurationCodeId;

                lblDuration.Text = Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description.ToLower(CultureInfo.InvariantCulture);
            }

            divUpgradeNotice.Visible = companyPaymentPackage != null;

            SetReadOnlyRights();
            ConfigureDuration();
            LoadAllProjectPackages();
            LoadAllInventoryPackages();
            InformParent();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sets the read only rights.
        /// </summary>
        private void SetReadOnlyRights()
        {
            lnkMonthlyPaymentPackage.Enabled = !IsReadOnly;
            lnkYearlyPaymentPackage.Enabled = !IsReadOnly;
        }

        /// <summary>
        /// Loads the duration of the data for selected.
        /// </summary>
        private void LoadDataForSelectedDuration()
        {
            SetReadOnlyRights();
            ConfigureDuration();
            LoadAllProjectPackages();
            LoadAllInventoryPackages();
            InformParent();
        }

        /// <summary>
        /// Loads all project packages.
        /// </summary>
        private void LoadAllProjectPackages()
        {
            List<ProjectPaymentPackageDetails> projectPackageDetails = Utils.GetSystemProjectPackageDetails();
            lvProjectList.DataSource = projectPackageDetails;
            lvProjectList.DataBind();
        }

        /// <summary>
        /// Loads all inventory packages.
        /// </summary>
        private void LoadAllInventoryPackages()
        {
            List<InventoryPaymentPackageDetails> inventoryPackageDetails = Utils.GetSystemInventoryPackageDetails();
            lvInventoryList.DataSource = inventoryPackageDetails;
            lvInventoryList.DataBind();
        }

        /// <summary>
        /// Informs the parent.
        /// </summary>
        private void InformParent()
        {
            if (InformParentToUpdate != null)
            {
                InformParentToUpdate(ProjectPaymentPackageTypeId, InventoryPaymentPackageTypeId, PaymentDurationTypeCodeId, (InventoryPaymentPackageTypeId != 0 && ProjectPaymentPackageTypeId != 0));
            }
        }

        /// <summary>
        /// Configures the duration.
        /// </summary>
        private void ConfigureDuration()
        {
            //Apply the selected css
            if (PaymentDurationTypeCodeId == Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId)
            {
                if (!IsReadOnly)
                {
                    tdYearlySelector.Attributes.Add("class", "paymentPackageDurationSelected");
                    tdMonthlySelector.Attributes.Add("class", "paymentPackageDurationSelector");
                }
                else
                {
                    tdYearlySelector.Attributes.Add("class", "paymentPackageDurationSelected ReadOnly");
                    tdMonthlySelector.Attributes.Add("class", "paymentPackageDurationSelector ReadOnly");
                }
            }
            else
            {
                if (!IsReadOnly)
                {
                    tdMonthlySelector.Attributes.Add("class", "paymentPackageDurationSelected");
                    tdYearlySelector.Attributes.Add("class", "paymentPackageDurationSelector");
                }
                else
                {
                    tdMonthlySelector.Attributes.Add("class", "paymentPackageDurationSelected ReadOnly");
                    tdYearlySelector.Attributes.Add("class", "paymentPackageDurationSelector ReadOnly");
                }
            }
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvProjectList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvProjectList_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                LinkButton lnkProjectPaymentPackage = e.Item.FindControl("lnkProjectPaymentPackage") as LinkButton;
                if (lnkProjectPaymentPackage != null)
                {
                    lnkProjectPaymentPackage.Enabled = !IsReadOnly;
                }

                int anualPaymentPackageDurationCodeId = Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId;
                ProjectPaymentPackageDetails projectPackageDetail = (ProjectPaymentPackageDetails)e.Item.DataItem;

                Literal litPackageName = (Literal)e.Item.FindControl("litPackageName");
                litPackageName.Text = projectPackageDetail.PackageName;

                Literal litPackageCharges = (Literal)e.Item.FindControl("litPackageCharges");
                StringBuilder textCharhgesBuilder = new StringBuilder();
                //eg: $1 per month
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                string cultureName = globalizationSection.Culture;

                decimal? packageAmount = this.GetBL<FinanceBL>().CalculatethePackageAmountByPeriod(Utils.GetCodeByValue("PaymentPackageType", "PROJECT").CodeId, projectPackageDetail.PackageTypeId, PaymentDurationTypeCodeId);
                if (packageAmount.HasValue && packageAmount > 0)
                    textCharhgesBuilder.AppendFormat("<p><b>{0}</b> <I>per {1}</I></p>", String.Format("{0:C0}", packageAmount), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                else
                    textCharhgesBuilder.AppendFormat("<p><b>FREE</b></p>");

                litPackageCharges.Text = textCharhgesBuilder.ToString();

                if (projectPackageDetail.Amount > 0)
                {
                    Literal litPackageLimits = (Literal)e.Item.FindControl("litPackageLimits");
                    StringBuilder textLimitBuilder = new StringBuilder();
                    textLimitBuilder.AppendFormat("<p>{0} Active {1} </p><p> Up to {2} Active {3} </p> ", projectPackageDetail.ProjectCount.ToString(), projectPackageDetail.ProjectCount == 1 ? "Project" : "Projects", projectPackageDetail.HeadCount, projectPackageDetail.HeadCount == 1 ? "User" : "Users");
                    litPackageLimits.Text = textLimitBuilder.ToString();
                }

                Literal litPackageDisplayText = (Literal)e.Item.FindControl("litPackageDisplayText");
                litPackageDisplayText.Text = projectPackageDetail.PackageDisplayText;
                Literal litTitleDescription = (Literal)e.Item.FindControl("litTitleDescription");
                litTitleDescription.Text = projectPackageDetail.PackageTitleDiscription;
                //High light the already configured package
                if (CurrentProjectPaymentPackageTypeId == projectPackageDetail.PackageTypeId)
                {
                    Panel tblPackage = (Panel)e.Item.FindControl("tblPackage");
                    tblPackage.CssClass = "PaymentPackageSaved";
                }
                else if (ProjectPaymentPackageTypeId == projectPackageDetail.PackageTypeId)
                {
                    Panel tblPackage = (Panel)e.Item.FindControl("tblPackage");
                    tblPackage.CssClass = "PaymentPackageSelected";
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvInventoryList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvInventoryList_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                LinkButton lnkInventoryPaymentPackage = e.Item.FindControl("lnkInventoryPaymentPackage") as LinkButton;
                if (lnkInventoryPaymentPackage != null)
                {
                    lnkInventoryPaymentPackage.Enabled = !IsReadOnly;
                }

                InventoryPaymentPackageDetails inventoryPaymentPackageDetails = (InventoryPaymentPackageDetails)e.Item.DataItem;

                Literal litPackageName = (Literal)e.Item.FindControl("litPackageName");
                litPackageName.Text = inventoryPaymentPackageDetails.PackageName;

                Literal litPackageCharges = (Literal)e.Item.FindControl("litPackageCharges");
                StringBuilder textCharhgesBuilder = new StringBuilder();
                //eg: $1 per month
                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                string cultureName = globalizationSection.Culture;

                decimal? packageAmount = this.GetBL<FinanceBL>().CalculatethePackageAmountByPeriod(Utils.GetCodeByValue("PaymentPackageType", "INVENTORY").CodeId, inventoryPaymentPackageDetails.PackageTypeId, PaymentDurationTypeCodeId);

                if (packageAmount > 0)
                    textCharhgesBuilder.AppendFormat("<p><b>{0}</b> <I>per {1}</I></p>", String.Format("{0:C0}", packageAmount), Utils.GetCodeByCodeId(PaymentDurationTypeCodeId).Description);
                else
                    textCharhgesBuilder.AppendFormat("<b>FREE</b>");

                litPackageCharges.Text = textCharhgesBuilder.ToString();

                Literal litPackageLimits = (Literal)e.Item.FindControl("litPackageLimits");
                StringBuilder textLimitBuilder = new StringBuilder();

                if (inventoryPaymentPackageDetails.ItemCount != null)
                {
                    double itemcount = 0;
                    double.TryParse(inventoryPaymentPackageDetails.ItemCount.ToString(), out itemcount);
                    textLimitBuilder.AppendFormat("<p>Up to {0} Items{1}</p>", String.Format("{0:#,0}", inventoryPaymentPackageDetails.ItemCount), inventoryPaymentPackageDetails.Amount == 0 ? " for FREE!" : "");
                    litPackageLimits.Text = textLimitBuilder.ToString();
                }
                else
                {
                    litPackageLimits.Text = "<p>Unlimited Items</p>";
                }

                Literal litPackageDisplayText = (Literal)e.Item.FindControl("litPackageDisplayText");
                litPackageDisplayText.Text = inventoryPaymentPackageDetails.PackageDisplayText;

                //High light the already configured package
                if (CurrentInventoryPaymentPackageTypeId == inventoryPaymentPackageDetails.PackageTypeId)
                {
                    Panel tblInvPackage = (Panel)e.Item.FindControl("tblInvPackage");
                    tblInvPackage.CssClass = "PaymentPackageSaved";
                }
                else if (InventoryPaymentPackageTypeId == inventoryPaymentPackageDetails.PackageTypeId)
                {
                    Panel tblInvPackage = (Panel)e.Item.FindControl("tblInvPackage");
                    tblInvPackage.CssClass = "PaymentPackageSelected";
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvProjectList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvProjectList_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (String.Equals(e.CommandName, "PackageSelected"))
            {
                int projectPaymentPackageDetailId = 0;
                int.TryParse(e.CommandArgument.ToString(), out projectPaymentPackageDetailId);
                ProjectPaymentPackageTypeId = projectPaymentPackageDetailId;
                //System.Web.UI.HtmlControls.HtmlTable tblPackage = (System.Web.UI.HtmlControls.HtmlTable)e.Item.FindControl("tblPackage");
                Panel tblPackage = (Panel)e.Item.FindControl("tblPackage");
                //Removed PaymentPackageSelected css class from existing selected ones
                foreach (ListViewItem item in lvProjectList.Items)
                {
                    Panel tblexPackages = (Panel)item.FindControl("tblPackage");
                    //tblexPackages.CssClass.Remove(0);
                    if (tblexPackages.CssClass.Contains("PaymentPackageSelected"))
                    {
                        tblexPackages.CssClass = "PaymentPackage";
                    }
                }

                //Apply the new css
                if (CurrentProjectPaymentPackageTypeId == projectPaymentPackageDetailId)
                {
                    tblPackage.CssClass = "PaymentPackageSaved";
                }
                else
                {
                    tblPackage.CssClass = "PaymentPackageSelected";
                }

                InformParent();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvInventoryList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvInventoryList_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (String.Equals(e.CommandName, "PackageSelected"))
            {
                int inventoryPaymentPackageDetailId = 0;
                int.TryParse(e.CommandArgument.ToString(), out inventoryPaymentPackageDetailId);
                InventoryPaymentPackageTypeId = inventoryPaymentPackageDetailId;

                Panel tblInvPackage = (Panel)e.Item.FindControl("tblInvPackage");

                //Removed PaymentPackageSelected css class from existing selected ones
                foreach (ListViewItem item in lvInventoryList.Items)
                {
                    Panel tblexPackages = (Panel)item.FindControl("tblInvPackage");
                    //tblexPackages.CssClass.Remove(0);
                    if (tblexPackages.CssClass.Contains("PaymentPackageSelected"))
                    {
                        tblexPackages.CssClass = "PaymentPackage";
                    }
                }

                //Apply the new css
                if (CurrentInventoryPaymentPackageTypeId == inventoryPaymentPackageDetailId)
                {
                    tblInvPackage.CssClass = "PaymentPackageSaved";
                }
                else
                {
                    tblInvPackage.CssClass = "PaymentPackageSelected";
                }

                InformParent();
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkMonthlyPaymentPackage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkMonthlyPaymentPackage_Click(object sender, EventArgs e)
        {
            //Update the Global variable
            PaymentDurationTypeCodeId = Utils.GetCodeByValue("PaymentPackageDuration", "MONTHLY").CodeId;
            LoadDataForSelectedDuration();
        }

        /// <summary>
        /// Handles the Click event of the lnkYearlyPaymentPackage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkYearlyPaymentPackage_Click(object sender, EventArgs e)
        {
            //Update the Global variable
            PaymentDurationTypeCodeId = Utils.GetCodeByValue("PaymentPackageDuration", "ANUAL").CodeId;
            LoadDataForSelectedDuration();
        }

        #endregion Event Handlers
    }
}