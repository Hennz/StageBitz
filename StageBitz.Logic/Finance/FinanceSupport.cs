using StageBitz.Common;
using StageBitz.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Finance
{
    /// <summary>
    /// Helper class for finance support.
    /// </summary>
    public static class FinanceSupport
    {
        /// <summary>
        /// Initializes the payment settings.
        /// </summary>
        public static void InitializePaymentSettings()
        {
            FatZebra.Gateway.Username = Utils.GetSystemValue("PaymentGatewayUsername");
            FatZebra.Gateway.Token = Utils.GetSystemValue("PaymentGatewayToken");
            FatZebra.Gateway.SandboxMode = Convert.ToBoolean(Utils.GetSystemValue("PaymentGatewaySandboxMode"));
            FatZebra.Gateway.TestMode = Convert.ToBoolean(Utils.GetSystemValue("PaymentGatewayTestMode"));
        }

        /// <summary>
        /// Gets the credit card token.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public static CreditCardToken GetCreditCardToken(string relatedTable, int relatedId)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.CreditCardTokens.Where(c => c.RelatedTableName == relatedTable && c.RelatedId == relatedId && c.IsActive == true).FirstOrDefault();
            }
        }

        #region Invoice/Receipt transaction creation

        /// <summary>
        /// Creates the invoice.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="invoiceType">Type of the invoice.</param>
        /// <param name="description">The description.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static Invoice CreateInvoice(int relatedId, string relatedTableName, string invoiceType, string description, decimal amount, DateTime fromDate, DateTime toDate, StageBitzDB dataContext)
        {
            //Create Invoice Transaction and Transaction Lines.
            Transaction transaction = CreateInvoiceTransactions(relatedId, relatedTableName, amount, description, dataContext);

            //Create Invoice.
            Invoice invoice = new Invoice();
            invoice.InvoiceTypeCodeID = Utils.GetCodeByValue("InvoiceType", invoiceType).CodeId;
            invoice.Amount = amount;
            invoice.RelatedTableName = relatedTableName;
            invoice.RelatedID = relatedId;
            invoice.InvoiceStatusCodeId = Utils.GetCodeByValue("InvoiceStatus", "UNPROCESSED").CodeId;
            invoice.InvoiceDate = Utils.Now;
            invoice.InvoiceDueDate = Utils.Now;
            invoice.FromDate = fromDate;
            invoice.ToDate = toDate;
            invoice.CreatedBy = invoice.LastUpdatedBy = 0;
            invoice.CreatedDate = invoice.LastUpdateDate = Utils.Now;
            transaction.Invoices.Add(invoice);
            return invoice;
        }

        /// <summary>
        /// Creates the receipt.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="description">The description.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="receiptForTransactionID">The receipt for transaction identifier.</param>
        /// <param name="dataContext">The data context.</param>
        public static void CreateReceipt(int relatedId, string relatedTableName, string description, decimal amount, int receiptForTransactionID, StageBitzDB dataContext)
        {
            Transaction transaction = CreateReceiptTransactions(relatedId, relatedTableName, amount, description, dataContext);
            Receipt receipt = new Receipt();
            receipt.ReceiptDate = Utils.Now;
            // ReceiptForTransactionID is the TransactionId of the Invoice Transaction.
            receipt.ReceiptForTransactionID = receiptForTransactionID;
            receipt.CreatedBy = receipt.LastUpdatedBy = 0;
            receipt.CreatedDate = receipt.LastUpdateDate = Utils.Now;
            transaction.Receipts.Add(receipt);
        }

        /// <summary>
        /// Creates the invoice transactions.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="description">The description.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static Transaction CreateInvoiceTransactions(int relatedId, string relatedTableName, decimal amount, string description, StageBitzDB dataContext)
        {
            Transaction transaction = new Data.Transaction();
            transaction.TransactionTypeCodeID = Utils.GetCodeByValue("TransactionType", "INV").CodeId;
            transaction.TransactionDate = Utils.Now;
            transaction.RelatedID = relatedId;
            transaction.RelatedTable = relatedTableName;
            transaction.Description = description;
            transaction.CreatedBy = transaction.LastUpdatedBy = 0;
            transaction.CreatedDate = transaction.LastUpdateDate = Utils.Now;

            TransactionLine transactionLineDebit = new Data.TransactionLine();
            transactionLineDebit.Amount = amount;
            transactionLineDebit.MasterChartId = GetMasterChartAccountId("1-AR");
            transactionLineDebit.CreatedBy = transactionLineDebit.LastUpdatedBy = 0;
            transactionLineDebit.CreatedDate = transactionLineDebit.LastUpdateDate = Utils.Now;
            transaction.TransactionLines.Add(transactionLineDebit);

            TransactionLine transactionLineCredit = new Data.TransactionLine();
            transactionLineCredit.Amount = amount * -1;
            transactionLineCredit.MasterChartId = GetMasterChartAccountId("1-INC");
            transactionLineCredit.CreatedBy = transactionLineCredit.LastUpdatedBy = 0;
            transactionLineCredit.CreatedDate = transactionLineCredit.LastUpdateDate = Utils.Now;
            transaction.TransactionLines.Add(transactionLineCredit);
            dataContext.Transactions.AddObject(transaction);
            return transaction;
        }

        /// <summary>
        /// Creates the receipt transactions.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="description">The description.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static Transaction CreateReceiptTransactions(int relatedId, string relatedTableName, decimal amount, string description, StageBitzDB dataContext)
        {
            Transaction transaction = new Data.Transaction();
            transaction.TransactionTypeCodeID = Utils.GetCodeByValue("TransactionType", "RECEIPT").CodeId;
            transaction.TransactionDate = Utils.Now;
            transaction.RelatedID = relatedId;
            transaction.RelatedTable = relatedTableName;
            transaction.Description = description;
            transaction.CreatedBy = transaction.LastUpdatedBy = 0;
            transaction.CreatedDate = transaction.LastUpdateDate = Utils.Now;

            TransactionLine transactionLineDebit = new Data.TransactionLine();
            transactionLineDebit.Amount = amount;
            transactionLineDebit.MasterChartId = GetMasterChartAccountId("1-BA");
            transactionLineDebit.CreatedBy = transactionLineDebit.LastUpdatedBy = 0;
            transactionLineDebit.CreatedDate = transactionLineDebit.LastUpdateDate = Utils.Now;
            transaction.TransactionLines.Add(transactionLineDebit);

            TransactionLine transactionLineCredit = new Data.TransactionLine();
            transactionLineCredit.Amount = amount * -1;
            transactionLineCredit.MasterChartId = GetMasterChartAccountId("1-AR");
            transactionLineCredit.CreatedBy = transactionLineCredit.LastUpdatedBy = 0;
            transactionLineCredit.CreatedDate = transactionLineCredit.LastUpdateDate = Utils.Now;
            transaction.TransactionLines.Add(transactionLineCredit);
            dataContext.Transactions.AddObject(transaction);

            return transaction;
        }

        /// <summary>
        /// Gets the master chart account identifier.
        /// </summary>
        /// <param name="accountCode">The account code.</param>
        /// <returns></returns>
        private static int GetMasterChartAccountId(string accountCode)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                return dataContext.MasterCharts.Where(mc => mc.AccountCode == accountCode).SingleOrDefault().MasterChartID;
            }
        }

        #endregion Invoice/Receipt transaction creation

        /// <summary>
        /// Gets the un processed invoices by related table.
        /// </summary>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="invoiceType">Type of the invoice.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static List<Invoice> GetUnProcessedInvoicesByRelatedTable(string relatedTableName, int relatedId, string invoiceType, StageBitzDB dataContext)
        {
            int processedInvoiceTypeCodeID = Utils.GetCodeByValue("InvoiceStatus", "PROCESSED").CodeId;
            int invoiceTypeCodeID = Utils.GetCodeByValue("InvoiceType", invoiceType).CodeId;

            //Get UNPROCESSED/FAILED and not reversed Invoices.
            return dataContext.Invoices.Where(i => i.RelatedTableName == relatedTableName &&
                                                    i.InvoiceTypeCodeID == invoiceTypeCodeID
                                                    && i.IsVoid == false && (relatedId == 0 || i.RelatedID == relatedId)
                                                    && i.InvoiceStatusCodeId != processedInvoiceTypeCodeID).ToList<Invoice>();
        }

        /// <summary>
        /// Gets the payment failed invoices.
        /// </summary>
        /// <param name="invoiceType">Type of the invoice.</param>
        /// <param name="companyStatusCodeId">The company status code identifier.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTableName">Name of the related table.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static List<Invoice> GetPaymentFailedInvoices(string invoiceType, int companyStatusCodeId, int relatedId, string relatedTableName, StageBitzDB dataContext)
        {
            // Only considers Failed Invoices. relatedId = 0 means, it is a payment retry on GracePeriod.

            int processedInvoiceTypeCodeID = Utils.GetCodeByValue("InvoiceStatus", "PROCESSED").CodeId;
            int invoiceTypeCodeID = Utils.GetCodeByValue("InvoiceType", invoiceType).CodeId;

            return (from i in dataContext.Invoices
                    join c in dataContext.Companies on i.RelatedID equals c.CompanyId
                    where i.RelatedTableName == relatedTableName && (i.RelatedID == relatedId || relatedId == 0) && (relatedId != 0 || c.CompanyStatusCodeId == companyStatusCodeId)
                    && i.IsVoid == false
                    && i.InvoiceStatusCodeId != processedInvoiceTypeCodeID
                    select i).ToList<Invoice>();
        }

        /// <summary>
        /// Creates the payment log.
        /// </summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public static PaymentLog CreatePaymentLog(int invoiceId, StageBitzDB dataContext)
        {
            // Create Payment log record.
            PaymentLog paymentLog = new PaymentLog();
            paymentLog.RelatedTableName = "Invoice";
            paymentLog.RelatedId = invoiceId;
            paymentLog.CreatedByUserId = paymentLog.CreatedByUserId = 0;
            paymentLog.CreatedDate = paymentLog.LastUpdatedDate = Utils.Now;
            dataContext.PaymentLogs.AddObject(paymentLog);
            return paymentLog;
        }
    }
}