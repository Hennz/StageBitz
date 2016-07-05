using StageBitz.Common;
using StageBitz.Logic.Business.Finance;
using StageBitz.Logic.Finance.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Finance
{
    /// <summary>
    /// User control for transaction search.
    /// </summary>
    public partial class TransactionSearch : UserControlBase
    {
        #region Enums & classes

        /// <summary>
        /// Enum for display module.
        /// </summary>
        public enum Module
        {
            Company,
            Project
        }

        /// <summary>
        /// Internal class fro search.
        /// </summary>
        [Serializable]
        private class SearchCriteria
        {
            /// <summary>
            /// Gets or sets from date.
            /// </summary>
            /// <value>
            /// From date.
            /// </value>
            public DateTime? FromDate { get; set; }

            /// <summary>
            /// Gets or sets to date.
            /// </summary>
            /// <value>
            /// To date.
            /// </value>
            public DateTime? ToDate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [show unpaid invoice tx only].
            /// </summary>
            /// <value>
            /// <c>true</c> if [show unpaid invoice tx only]; otherwise, <c>false</c>.
            /// </value>
            public bool ShowUnpaidInvoiceTxOnly { get; set; }

            /// <summary>
            /// Gets a value indicating whether this instance is blank.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is blank; otherwise, <c>false</c>.
            /// </value>
            public bool IsBlank
            {
                get
                {
                    return (FromDate == null && ToDate == null && ShowUnpaidInvoiceTxOnly == false);
                }
            }
        }

        #endregion Enums & classes

        #region Properties

        /// <summary>
        /// Gets the search fields.
        /// </summary>
        /// <value>
        /// The search fields.
        /// </value>
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

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId
        {
            get
            {
                if (ViewState["RelatedId"] == null)
                {
                    return 0;
                }

                return (int)ViewState["RelatedId"];
            }
            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                #region Update Search Criteria

                SearchCriteria criteria = SearchFields;
                criteria.FromDate = dtpkFrom.SelectedDate.HasValue ? dtpkFrom.SelectedDate.Value.Date : (DateTime?)null;
                criteria.ToDate = dtpkTo.SelectedDate.HasValue ? dtpkTo.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;
                criteria.ShowUnpaidInvoiceTxOnly = chkShowUnpaid.Checked;

                #endregion Update Search Criteria

                gvTransactions.CurrentPageIndex = 0;
                gvTransactions.Rebind();
                upnlSearchResults.Update();
            }
        }

        #region Grid Events

        /// <summary>
        /// Handles the NeedDataSource event of the gvTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        protected void gvTransactions_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            SearchCriteria criteria = SearchFields;

            int processedInvoiceTypeCodeId = Utils.GetCodeIdByCodeValue("InvoiceStatus", "PROCESSED");

            var transactions = this.GetBL<FinanceBL>().GetTransactionHistory(criteria.FromDate, criteria.ToDate, criteria.ShowUnpaidInvoiceTxOnly, RelatedId, processedInvoiceTypeCodeId);
            gvTransactions.Visible = true;
            gvTransactions.DataSource = transactions;

            upnlSearchCriteria.Update();
        }

        /// <summary>
        /// Handles the SortCommand event of the gvTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridSortCommandEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the ItemDataBound event of the gvTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Telerik.Web.UI.GridItemEventArgs"/> instance containing the event data.</param>
        protected void gvTransactions_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic transaction = (dynamic)dataItem.DataItem;

                var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
                dataItem["Amount"].Text = Support.FormatCurrency(transaction.Amount, globalizationSection.Culture);
                dataItem["InvoiceDate"].Text = Support.FormatDate(transaction.InvoiceDate);
                dataItem["FromDate"].Text = Support.FormatDate(transaction.FromDate);
                dataItem["ToDate"].Text = Support.FormatDate(transaction.ToDate);

                if (transaction.ReceiptID != 0)
                {
                    dataItem["ReceiptDate"].Text = Support.FormatDate(transaction.ReceiptDate);
                }

                Image imgInfo = (Image)e.Item.FindControl("imgInfo");
                Image imgError = (Image)e.Item.FindControl("imgError");

                string paymentLogRefNoString = (string.IsNullOrEmpty(transaction.PaymentLogReferenceNumber) ? string.Empty : string.Format(", Ref. No: {0}", transaction.PaymentLogReferenceNumber));

                if (transaction.InvoiceID != 0)
                {
                    if (transaction.ReceiptID == 0) //Failed payment
                    {
                        imgError.Visible = true;
                        imgInfo.Visible = false;
                        imgError.ToolTip = string.Format("Unpaid Invoice (Invoice No: {0:000000}{1}) - {2}", transaction.InvoiceID, paymentLogRefNoString, ProjectFinanceHandler.GetCompanyPaymentFailureDetails(this.RelatedId));
                    }
                    else
                    {
                        imgError.Visible = false;
                        imgInfo.Visible = true;
                        imgInfo.ToolTip = string.Format("Invoice No: {0:000000}, Receipt No: {1:000000}{2}", transaction.InvoiceID, transaction.ReceiptID, paymentLogRefNoString);
                    }
                }
                else
                {
                    imgError.Visible = false;
                    imgInfo.Visible = true;
                    imgInfo.ToolTip = "Invoice request transaction. An invoice will have been posted to you. Receipt dates for posted invoices do not appear here.";
                }
            }
        }

        #endregion Grid Events

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            gvTransactions.MasterTableView.SortExpressions.Clear();

            //divSearchCriteria.Attributes["class"] = "boxBorder";
            //divSearchCriteria.Style["padding"] = "10px";

            gvTransactions.Rebind();
        }

        #endregion Public Methods
    }
}