using System;
using System.Linq;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.Common;
using StageBitz.Logic.Finance.Project;
using Telerik.Web.UI;
using StageBitz.Logic.Business.Finance;
using StageBitz.Data;
using System.Text;
using System.Globalization;
using StageBitz.Logic.Business.Company;


namespace StageBitz.AdminWeb.Controls.Common
{
    public partial class TransactionSearch : UserControlBase
    {
        public int CompanyId
        {
            get
            {
                if (ViewState["CompanyId"] == null)
                {
                    int CompanyId = 0;

                    if (Request["CompanyId"] != null)
                    {
                        int.TryParse(Request["CompanyId"], out CompanyId);
                    }

                    ViewState["CompanyId"] = CompanyId;
                }

                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        [Serializable]
        class SearchCriteria
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string CompanyName { get; set; }
            public int CompanyId { get; set; }
            public bool ShowUnpaidInvoiceTxOnly { get; set; }
            public bool IsBlank
            {
                get
                {
                    return (CompanyId == 0 && FromDate == null && ToDate == null && string.IsNullOrEmpty(CompanyName) && ShowUnpaidInvoiceTxOnly == false);
                }
            }
        }

        #region Properties

        private SearchCriteria SearchFields
        {
            get
            {
                if (ViewState["SearchFields"] == null)
                {
                    ViewState["SearchFields"] = new SearchCriteria();
                }

                return (SearchCriteria)ViewState["SearchFields"];
            }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                gvTransactions.DataSource = null;
                gvTransactions.Visible = false;
                litCompany.Visible = (CompanyId == 0);
                txtCompany.Visible = (CompanyId == 0);
                if (CompanyId > 0)
                {
                    foreach (GridColumn col in gvTransactions.MasterTableView.RenderColumns)
                    {
                        if (col.UniqueName == "CompanyName")
                        {
                            col.Visible = false;
                            break;
                        }
                    }
                }
                else
                {
                    upnlDiscountCode.Visible = false;
                }

                LoadDiscountCodes();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Update Search Criteria

            SearchCriteria criteria = SearchFields;

            criteria.FromDate = dtpkFrom.SelectedDate.HasValue ? dtpkFrom.SelectedDate.Value.Date : (DateTime?)null;
            criteria.ToDate = dtpkTo.SelectedDate.HasValue ? dtpkTo.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;
            criteria.CompanyName = txtCompany.Text.Trim();
            criteria.CompanyId = CompanyId;
            criteria.ShowUnpaidInvoiceTxOnly = chkShowUnpaid.Checked;
            #endregion

            gvTransactions.Rebind();
            upnlSearchResults.Update();
        }

        #region Grid Events

        protected void gvTransactions_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            SearchCriteria criteria = SearchFields;

            if (criteria.IsBlank)
            {
                gvTransactions.DataSource = null;
                gvTransactions.Visible = false;
                divNoSearchCriteria.Visible = true;
                divCriteriaValidation.InnerText = "Please provide a criteria to search";
            }
            else
            {
                int processedInvoiceTypeCodeId = Utils.GetCodeIdByCodeValue("InvoiceStatus", "PROCESSED");

                divCriteriaValidation.InnerText = "";
                var transactions = from inv in DataContext.Invoices
                                   //join tr in DataContext.Transactions on inv.TransactionID equals tr.TransactionID
                                   from paylog in DataContext.PaymentLogs.Where(pl => pl.RelatedTableName == "Invoice" && pl.RelatedId == inv.InvoiceID && pl.IsPaymentSuccess == true).DefaultIfEmpty()
                                   join r in DataContext.Receipts on inv.TransactionID equals r.ReceiptForTransactionID into receiptJoin
                                   from receipt in receiptJoin.DefaultIfEmpty()
                                   join c in DataContext.Companies on inv.RelatedID equals c.CompanyId
                                   where inv.RelatedTableName == "Company"
                                   && ((criteria.ShowUnpaidInvoiceTxOnly && inv.InvoiceStatusCodeId != processedInvoiceTypeCodeId) || criteria.ShowUnpaidInvoiceTxOnly == false)
                                   && (c.CompanyId == criteria.CompanyId || (CompanyId == 0 && (string.IsNullOrEmpty(criteria.CompanyName) || c.CompanyName.Contains(criteria.CompanyName))))
                                       //Invoices and corresponding Receipts might not have the same date (due to payment failures). Therefore with the Dat Range criteria, we search for both Invoice date and Receipt date separately.
                                   && (((criteria.FromDate == null || inv.InvoiceDate >= criteria.FromDate) && (criteria.ToDate == null || inv.InvoiceDate <= criteria.ToDate))
                                      || (receipt != null && ((criteria.FromDate == null || receipt.ReceiptDate >= criteria.FromDate) && (criteria.ToDate == null || receipt.ReceiptDate <= criteria.ToDate))))
                                   orderby c.CompanyName, inv.InvoiceDate descending
                                   select new
                                   {                                       
                                       c.CompanyId,
                                       c.CompanyName,
                                       CountryName = (c.Country == null ? string.Empty : c.Country.CountryName),
                                       Invoice = inv,
                                       ReceiptID = (receipt == null ? 0 : receipt.ReceiptID),
                                       ReceiptDate = (receipt == null ? (DateTime?)null : receipt.ReceiptDate),
                                       PaymentLogReferenceNumber = (paylog == null ? DataContext.PaymentLogs.Where(pl => pl.RelatedTableName == "Invoice" && pl.RelatedId == inv.InvoiceID && pl.IsPaymentSuccess == false).OrderByDescending(pl => pl.PaymentLogId).FirstOrDefault().ReferenceNumber : paylog.ReferenceNumber)
                                   };

                gvTransactions.Visible = true;
                divNoSearchCriteria.Visible = false;
                gvTransactions.DataSource = transactions;
            }
        }

        protected void gvTransactions_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvTransactions.MasterTableView.SortExpressions.Clear();
                gvTransactions.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvTransactions.Rebind();
            }
        }

        protected void gvTransactions_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic transaction = (dynamic)dataItem.DataItem;

                HyperLink lnkCompanyName = (HyperLink)e.Item.FindControl("lnkCompanyName");
                lnkCompanyName.Text = Support.TruncateString(transaction.CompanyName, 30);
                if (transaction.CompanyName.Length > 30)
                {
                    lnkCompanyName.ToolTip = transaction.CompanyName;
                }
                lnkCompanyName.NavigateUrl = string.Format("~/Company/CompanyDetails.aspx?CompanyID={0}", transaction.CompanyId);

                dataItem["Country"].Text = Support.TruncateString(transaction.CountryName, 12);
                if (transaction.CountryName.Length > 12)
                {
                    dataItem["Country"].ToolTip = transaction.CountryName;
                }

                dataItem["Amount"].Text = Support.FormatCurrency(transaction.Invoice.Amount);
                dataItem["InvoiceDate"].Text = Support.FormatDate(transaction.Invoice.InvoiceDate);
                dataItem["FromDate"].Text = Support.FormatDate(transaction.Invoice.FromDate);
                dataItem["ToDate"].Text = Support.FormatDate(transaction.Invoice.ToDate);

                if (transaction.ReceiptID != 0)
                {
                    //dataItem["ReceiptNo"].Text = transaction.ReceiptID.ToString();
                    dataItem["ReceiptDate"].Text = Support.FormatDate(transaction.ReceiptDate);
                }

                Image imgInfo = (Image)e.Item.FindControl("imgInfo");
                Image imgError = (Image)e.Item.FindControl("imgError");

                string paymentLogRefNoString = (string.IsNullOrEmpty(transaction.PaymentLogReferenceNumber) ? string.Empty : string.Format(", Ref. No: {0}", transaction.PaymentLogReferenceNumber));

                if (transaction.ReceiptID == 0) //Failed payment
                {
                    imgError.Visible = true;
                    imgInfo.Visible = false;

                    imgError.ToolTip = string.Format("Unpaid Invoice (Invoice No: {0:000000}{1}) - {2}", transaction.Invoice.InvoiceID, paymentLogRefNoString, ProjectFinanceHandler.GetCompanyPaymentFailureDetails(transaction.CompanyId));
                }
                else
                {
                    imgError.Visible = false;
                    imgInfo.Visible = true;

                    imgInfo.ToolTip = string.Format("Invoice No: {0:000000}, Receipt No: {1:000000}{2}", transaction.Invoice.InvoiceID, transaction.ReceiptID, paymentLogRefNoString);
                }




            }
        }

        #endregion

        protected void btnOk_Click(object sender, EventArgs e)
        {
            if (txtDiscountCode.Text.Trim().Length == 0)
            {
                spanErrorMsg.InnerText = "Discount Code cannot be empty.";
                spanErrorMsg.Visible = true;
                return;
            }

            //Validate the Discount Code.
            DiscountCode discountCode = GetBL<FinanceBL>().GetDiscountCode(txtDiscountCode.Text.Trim());
            if (discountCode == null)
            {
                //Invalid Discount Code.
                spanErrorMsg.InnerText = "Invalid Discount Code.";
                spanErrorMsg.Visible = true;
                return;
            }

            if (discountCode.ExpireDate.Date < Today)
            {
                //If it is expired
                spanErrorMsg.InnerText = "Discount Code has expired.";
                spanErrorMsg.Visible = true;
                return;
            }

            //If the usage limit has exceeded.
            var dicountCodeUsageList = GetBL<FinanceBL>().GetDiscountCodeUsages(discountCode.DiscountCodeID, true);
            if (dicountCodeUsageList.Count() == discountCode.InstanceCount)
            {
                spanErrorMsg.InnerText = "This Discount code has reached its maximum instance count.";
                spanErrorMsg.Visible = true;
                return;
            }

            //if the new discount code is already being used
            if (dicountCodeUsageList.Where(dcu => dcu.CompanyId == CompanyId).FirstOrDefault() != null)
            {
                spanErrorMsg.InnerText = "Discount Code has already been used by this company.";
                spanErrorMsg.Visible = true;
                return;
            }

            spanErrorMsg.Visible = false;
            DiscountCodeUsage newDiscountCodeUsage = new DiscountCodeUsage();
            newDiscountCodeUsage.DiscountCodeId = discountCode.DiscountCodeID;
            newDiscountCodeUsage.CreatedDate = newDiscountCodeUsage.LastUpdatedDate = Now;
            newDiscountCodeUsage.StartDate = Today;
            newDiscountCodeUsage.EndDate = Today.AddDays(discountCode.Duration * 7);
            newDiscountCodeUsage.CreatedByUserId = newDiscountCodeUsage.LastUpdatedByUserId = UserID;
            newDiscountCodeUsage.CompanyId = CompanyId;
            newDiscountCodeUsage.IsAdminApplied = true;
            newDiscountCodeUsage.IsActive = true;
            GetBL<FinanceBL>().AddDiscountCodeUsageBySBAdmin(newDiscountCodeUsage, UserID);

            popupManageDiscount.HidePopup();
            LoadDiscountCodes();
        }

        protected void lnkSetUpDiscountCode_Click(object sender, EventArgs e)
        {
            popupManageDiscount.ShowPopup();
            spanErrorMsg.Visible = false;
            txtDiscountCode.Text = string.Empty;
        }

        protected void btnConfirmDeleteDiscount_Click(object sender, EventArgs e)
        {
            Data.DiscountCodeUsage discountUsage = GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId);
            if (discountUsage != null)
            {
                //Expire it by today
                discountUsage.EndDate = Today;
                discountUsage.LastUpdatedByUserId = UserID;
                discountUsage.LastUpdatedDate = Now;
                discountUsage.IsActive = false;
            }

            ProjectUsageHandler.UpdatePaymentSummaryForFreeTrialCompanyBySBAdmin(CompanyId, null, null, UserID, DataContext);

            GetBL<FinanceBL>().SaveChanges();
            LoadDiscountCodes();
            popupRemoveDiscountCode.HidePopup();
        }

        protected void lnkRemoveDiscountCode_Click(object sender, EventArgs e)
        {
            popupRemoveDiscountCode.ShowPopup();
        }
        #endregion

        #region Private Methods
        private DiscountCodeUsage GetLatestDiscountCode()
        {
            return this.GetBL<FinanceBL>().GetLatestDiscountCodeUsage(CompanyId);
        }

        private void LoadDiscountCodes()
        {
            DiscountCodeUsage dicountCodeUsage = GetLatestDiscountCode();

            if (dicountCodeUsage == null)
            {
                lnkSetUpDiscountCode.Visible = true;
                lnkRemoveDiscountCode.Visible = false;
                divDiscountSet.Visible = false;
            }
            else
            {
                litDiscountApplied.Text = "\"" + dicountCodeUsage.DiscountCode.Code + "\"" + " applied";
                lnkSetUpDiscountCode.Visible = false;
                lnkRemoveDiscountCode.Visible = true;
                divDiscountSet.Visible = true;
                //show the tooltip
                StringBuilder msg = new StringBuilder();
                msg.Append((dicountCodeUsage.DiscountCode.Discount / 100).ToString("#0.##%", CultureInfo.InvariantCulture));
                msg.Append(" ");
                msg.Append("Daily Discount from ");
                msg.Append(Support.FormatDate(dicountCodeUsage.CreatedDate));
                msg.Append(" to ");
                msg.Append(Support.FormatDate(dicountCodeUsage.EndDate.Date));
                imgDiscountCode.Attributes.Add("Title", msg.ToString());
            }

            upnlDiscountCode.Update();
        }
        #endregion

    }
}