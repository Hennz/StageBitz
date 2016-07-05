using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;
using StageBitz.Common.Enum;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Finance;
using StageBitz.Reports.AdminWeb.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.Company
{
    public partial class PricingPlanHistory : PageBase
    {
        #region Properties
        private enum ViewMode
        {
            All,
            Invoice,
            CreditCard,
            Latest
        }

        private ViewMode DisplayMode
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

        #endregion

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();
            }
        }

        protected void ddlDisplayPricingPlans_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ddlDisplayMode = ddlDisplayPricingPlans.SelectedValue;

            switch (ddlDisplayPricingPlans.SelectedValue)
            {
                case "Invoice":
                    DisplayMode = ViewMode.Invoice;
                    break;
                case "CC":
                    DisplayMode = ViewMode.CreditCard;
                    break;
                case "All":
                    DisplayMode = ViewMode.All;
                    break;
                case "Latest":
                    DisplayMode = ViewMode.Latest;
                    break;
            }
            LoadData();
            gvPricingPlans.Rebind();
        }

        protected void gvPricingPlans_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvPricingPlans.MasterTableView.SortExpressions.Clear();
                gvPricingPlans.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvPricingPlans.Rebind();
            }
        }

        protected void gvPricingPlans_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                PricingPlanHistoryData pricingPlanHistoryData = (PricingPlanHistoryData)dataItem.DataItem;

                HyperLink lnkCompany = (HyperLink)e.Item.FindControl("lnkCompany");
                lnkCompany.Text = Support.TruncateString(pricingPlanHistoryData.CompanyName, 25);
                if (pricingPlanHistoryData.CompanyName.Length > 25)
                {
                    lnkCompany.ToolTip = pricingPlanHistoryData.CompanyName;
                }
                lnkCompany.NavigateUrl = string.Format("~/Company/CompanyDetails.aspx?CompanyID={0}", pricingPlanHistoryData.CompanyId);

                HyperLink lnkAdmin = (HyperLink)e.Item.FindControl("lnkAdmin");
                lnkAdmin.Text = Support.TruncateString(pricingPlanHistoryData.CompanyAdminName, 17);
                if (pricingPlanHistoryData.CompanyAdminName.Length > 17)
                {
                    lnkAdmin.ToolTip = pricingPlanHistoryData.CompanyAdminName;
                }
                lnkAdmin.NavigateUrl = string.Format("~/User/UserDetails.aspx?ViewUserID={0}", pricingPlanHistoryData.CompanyAdminId);

                if (pricingPlanHistoryData.PromotionalCode != string.Empty)
                {
                    dataItem["PromotionalCode"].Text = "Yes";
                    dataItem["PromotionalCode"].ToolTip = pricingPlanHistoryData.PromotionalCode;
                }
                dataItem["TotalCost"].Text = Support.FormatCurrency(pricingPlanHistoryData.TotalCost);
                dataItem["StartDate"].Text = Support.FormatDate(pricingPlanHistoryData.StartDate);
                
            }
        }

        protected void gvPricingPlans_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadData();
        }

        protected void btnSendToExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }


        #endregion
        
        #region PrivateMethods
        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Companies", "~/Company/Companies.aspx");
            breadCrumbs.AddLink("Pricing Plan History", null);
            breadCrumbs.LoadControl();
        }

        private void LoadData()
        {
            gvPricingPlans.DataSource = GetPricingPlanHistoryData();
        }

        private List<PricingPlanHistoryData> GetPricingPlanHistoryData()
        {
            List<PricingPlanHistoryData> pricingPlanHistory = null;
            if (DisplayMode == ViewMode.All)
            {
                pricingPlanHistory = this.GetBL<FinanceBL>().GetPricingPlanHistoryData();
            }
            else if (DisplayMode == ViewMode.Invoice)
            {
                pricingPlanHistory = this.GetBL<FinanceBL>().GetPricingPlanHistoryData(Utils.GetCodeIdByCodeValue("PaymentMethod", "INVOICE"));
            }
            else if (DisplayMode == ViewMode.CreditCard)
            {
                pricingPlanHistory = this.GetBL<FinanceBL>().GetPricingPlanHistoryData(Utils.GetCodeIdByCodeValue("PaymentMethod", "CREDITCARD"));
            }
            else
            {
                pricingPlanHistory = this.GetBL<FinanceBL>().GetPricingPlanHistoryData(0, true);
            }
            return pricingPlanHistory;
        }

        private void ExportToExcel()
        {
            string sortExpression = gvPricingPlans.MasterTableView.SortExpressions.GetSortString();
            DataView dvPricingPlanHistory = GetPricingPlanHistoryDataTable().DefaultView;
            //set sorting
            if (!string.IsNullOrEmpty(sortExpression))
            {
                dvPricingPlanHistory.Sort = sortExpression;
            }
            
            string fileNameExtension;
            string encoding;
            string mimeType;
            string fileName = "PrcingPlanHistory";

            byte[] reportBytes = AdminWebReportHandler.GeneratePrcingPlanHistoryReport(dvPricingPlanHistory, ReportTypes.Excel,
                    out fileNameExtension, out encoding, out mimeType);
            Utils.ExportReport(reportBytes, mimeType, fileNameExtension, fileName);
        }

        private DataTable GetPricingPlanHistoryDataTable()
        {
            List<PricingPlanHistoryData> pricingPlanHistory = GetPricingPlanHistoryData();
            using (DataTable dtPricingPlanHistory = new DataTable("tblItems"))
            {
                dtPricingPlanHistory.Columns.Add("CompanyName");
                dtPricingPlanHistory.Columns.Add("CompanyAdminName");
                dtPricingPlanHistory.Columns.Add("ProjectLevel");
                dtPricingPlanHistory.Columns.Add("InventoryLevel");
                dtPricingPlanHistory.Columns.Add("PromotionalCode");
                dtPricingPlanHistory.Columns.Add("Educational");
                dtPricingPlanHistory.Columns.Add("Period");
                dtPricingPlanHistory.Columns.Add("StartDate", typeof(DateTime));
                dtPricingPlanHistory.Columns.Add("TotalCost", typeof(decimal));
                dtPricingPlanHistory.Columns.Add("PaymentMethod");

                foreach (PricingPlanHistoryData pricingPlan in pricingPlanHistory)
                {
                    DataRow dtRow = dtPricingPlanHistory.NewRow();
                    dtRow["CompanyName"] = pricingPlan.CompanyName;
                    dtRow["CompanyAdminName"] = pricingPlan.CompanyAdminName;
                    dtRow["ProjectLevel"] = pricingPlan.ProjectLevel;
                    dtRow["InventoryLevel"] = pricingPlan.InventoryLevel;
                    dtRow["PromotionalCode"] = pricingPlan.PromotionalCode;
                    dtRow["Educational"] = pricingPlan.Educational;
                    dtRow["Period"] = pricingPlan.Period;
                    dtRow["StartDate"] = pricingPlan.StartDate;
                    dtRow["TotalCost"] = pricingPlan.TotalCost;
                    dtRow["PaymentMethod"] = pricingPlan.PaymentMethod;
                    dtPricingPlanHistory.Rows.Add(dtRow);
                }
                return dtPricingPlanHistory;
            }
        }

        #endregion
    }
}