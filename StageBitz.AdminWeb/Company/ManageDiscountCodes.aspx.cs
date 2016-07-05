using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.AdminWeb.Common.Helpers;
using Telerik.Web.UI;
using StageBitz.Data;
using StageBitz.Common.Exceptions;
using System.Globalization;
using StageBitz.AdminWeb.Controls.Common;

namespace StageBitz.AdminWeb.Company
{
    public partial class ManageDiscountCodes : PageBase
    {
        private int SelectedDiscountCodeId
        {
            get
            {
                if (ViewState["SelectedDiscountCodeId"] == null)
                    return 0;
                return (int)ViewState["SelectedDiscountCodeId"];
            }
            set
            {
                ViewState["SelectedDiscountCodeId"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dtExpireDate.MinDate = Today;
                dtExpireDate.SelectedDate = Today.AddDays(txtDuration.Value.Value * 7);
                LoadBreadCrumbs();
            }
        }

        private void LoadData()
        {
            bool isHideExpiredCodes = chkHideExpiredCode.Checked;

            //Load DiscountCodes and Usage count.
            var discountCodes = from dc in DataContext.DiscountCodes
                                orderby dc.DiscountCodeID
                                where !isHideExpiredCodes || dc.ExpireDate >= Today
                                select new { DiscountCode = dc, UsageCount = dc.DiscountCodeUsages.Count };


            gvDiscountCodes.DataSource = discountCodes;
        }

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        private void ClearUI()
        {
            imgDiscountCodeError.Visible = false;
            txtDiscountCode.Text = string.Empty;
            txtDiscountPercentage.Text = string.Empty;
            txtDuration.Text = "1";
            lblweek.InnerText = " week";
            dtExpireDate.MinDate = Today;
            dtExpireDate.SelectedDate = Today.AddDays(txtDuration.Value.Value * 7);
            txtInstanceCount.Text = "1";
            imgDurationError.Visible = false;
            imgExpireDate.Visible = false;
        }

        protected void AddDiscountCode(object sender, EventArgs e)
        {
            if (IsValid)
            {
                //Default set to hide. 
                imgDiscountCodeError.Visible = false;
                imgDurationError.Visible = false;

                if (!IsValidtoSave(txtDiscountCode.Text, 0, false))
                {
                    imgDiscountCodeError.Visible = true;
                    imgDiscountCodeError.Attributes.Add("Title", "Please choose a different Discount Code.");
                    LoadData();
                    gvDiscountCodes.DataBind();
                    return;
                }

                if (dtExpireDate.SelectedDate.HasValue && dtExpireDate.SelectedDate.Value < Today)
                {
                    imgExpireDate.Visible = true;
                    imgExpireDate.Attributes.Add("Title", "Expiry date cannot be a past date.");
                    LoadData();
                    gvDiscountCodes.DataBind();
                    return;
                }

                DiscountCode discountCode = new DiscountCode();
                discountCode.Code = txtDiscountCode.Text.Trim();
                discountCode.Discount = (decimal)txtDiscountPercentage.Value.Value;
                discountCode.InstanceCount = (int)txtInstanceCount.Value.Value;
                discountCode.Duration = (int)txtDuration.Value.Value;
                discountCode.ExpireDate = dtExpireDate.SelectedDate.Value;
                discountCode.CreatedByUserId = discountCode.LastUpdatedByUserId = UserID;
                discountCode.CreatedDate = discountCode.LastUpdatedDate = Now;
                DataContext.DiscountCodes.AddObject(discountCode);
                DataContext.SaveChanges();

                IsPageDirty = false;

                LoadData();
                gvDiscountCodes.DataBind();
                uplAddDisList.Update();
                ClearUI();
            }
        }

        protected void gvDiscountCodes_ItemDeleted(object sender, GridCommandEventArgs e)
        {
            //Get the GridDataItem of the RadGrid     
            GridDataItem item = (GridDataItem)e.Item;

            //Get the primary key value using the DataKeyValue.     
            int discountCodeID = (int)item.OwnerTableView.DataKeyValues[item.ItemIndex]["DiscountCode.DiscountCodeID"];

            DataContext.DeleteObject(DataContext.DiscountCodes.First(dc => dc.DiscountCodeID == discountCodeID));
            DataContext.SaveChanges();
        }

        private int GetDiscountUsageCount(int discountCodeId)
        {
            return (from dcu in DataContext.DiscountCodeUsages
                    where dcu.DiscountCodeId == discountCodeId
                    select dcu).Count();
        }
        protected void gvDiscountCodes_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.EditItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int discountCodeID = (int)dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["DiscountCode.DiscountCodeID"];

                DiscountCode discountCode = (from dc in DataContext.DiscountCodes
                                             where dc.DiscountCodeID == discountCodeID
                                             select dc).FirstOrDefault();

                int discountCodeUsageCount = discountCode.DiscountCodeUsages.Count;

                RadNumericTextBox txtInstanceCount = (RadNumericTextBox)dataItem.FindControl("txtInstanceCount");
                txtInstanceCount.Text = discountCode.InstanceCount.ToString();
                TextBox txtDiscountCode = (TextBox)dataItem.FindControl("txtDiscountCode");
                RadNumericTextBox txtDiscountPercentage = (RadNumericTextBox)dataItem.FindControl("txtDiscountPercentage");
                RadNumericTextBox txtDuration = (RadNumericTextBox)dataItem.FindControl("txtDuration");
                RadDatePicker dtExpireDate = (RadDatePicker)dataItem.FindControl("dtExpireDate");
                Literal litCreatedDate = (Literal)dataItem.FindControl("litCreatedDate");


                //There are two edit modes. When there is no usage and if there are usage.
                if (discountCodeUsageCount == 0)
                {
                    //means you can edit all the fields.
                    txtDiscountPercentage.Value = (double)discountCode.Discount;
                    txtDiscountCode.Text = discountCode.Code;


                    txtDuration.Value = discountCode.Duration;
                    dtExpireDate.SelectedDate = discountCode.ExpireDate;


                    dtExpireDate.SelectedDate = discountCode.ExpireDate;
                    litCreatedDate.Text = Support.FormatDate(discountCode.CreatedDate);
                    //For expired discounts the min date has to be the expired Date.
                    if (dtExpireDate.SelectedDate < Today)
                        dtExpireDate.MinDate = dtExpireDate.SelectedDate.Value;
                    else
                        dtExpireDate.MinDate = Today;
                }
                else
                {
                    //Needs to disable unnecessary validation controls since it is in edit mode.
                    RegularExpressionValidator regExMinLengthEdit = (RegularExpressionValidator)dataItem.FindControl("regExMinLengthEdit");
                    regExMinLengthEdit.Enabled = false;

                    RequiredFieldValidator rqdCodeEditMode = (RequiredFieldValidator)dataItem.FindControl("rqdCodeEditMode");
                    rqdCodeEditMode.Enabled = false;

                    RequiredFieldValidator rqdDiscountEdit = (RequiredFieldValidator)dataItem.FindControl("rqdDiscountEdit");
                    rqdDiscountEdit.Enabled = false;

                    RequiredFieldValidator rqdDurationEdit = (RequiredFieldValidator)dataItem.FindControl("rqdDurationEdit");
                    rqdDurationEdit.Enabled = false;

                    RequiredFieldValidator rqdExpireEdit = (RequiredFieldValidator)dataItem.FindControl("rqdExpireEdit");
                    rqdExpireEdit.Enabled = false;

                    RequiredFieldValidator rqdInstanceCountEdit = (RequiredFieldValidator)dataItem.FindControl("rqdInstanceCountEdit");
                    rqdInstanceCountEdit.Enabled = false;

                    //means you can edit only Instance count.
                    txtDiscountCode.Visible = false;
                    txtDiscountPercentage.Visible = false;
                    txtDiscountPercentage.Visible = false;
                    txtDuration.Visible = false;
                    dtExpireDate.Visible = false;

                    //Only shows these fields in read only mode.
                    Label litDiscountCode = (Label)dataItem.FindControl("litDiscountCode");
                    Literal litExpireDate = (Literal)dataItem.FindControl("litExpireDate");
                    Literal litDuration = (Literal)dataItem.FindControl("litDuration");
                    Literal litDiscountPercentage = (Literal)dataItem.FindControl("litDiscountPercentage");

                    litDiscountCode.Text = Support.TruncateString(discountCode.Code, 15);
                    if (discountCode.Code.Length >= 15)
                    {
                        litDiscountCode.ToolTip = discountCode.Code;
                    }

                    litDiscountCode.Visible = true;
                    litExpireDate.Text = Support.FormatDate(discountCode.ExpireDate);
                    litExpireDate.Visible = true;
                    litCreatedDate.Text = Support.FormatDate(discountCode.CreatedDate);
                    litDuration.Text = discountCode.Duration.ToString() + (discountCode.Duration == 1 ? " week" : " weeks");
                    litDuration.Visible = true;
                    litDiscountPercentage.Text = (discountCode.Discount / 100).ToString("#0.##%", CultureInfo.InvariantCulture);
                    litDiscountPercentage.Visible = true;
                }
            }
            else if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                DiscountCode discountCode = ((dynamic)dataItem.DataItem).DiscountCode;

                int discountCodeUsageCount = ((dynamic)dataItem.DataItem).UsageCount;

                dataItem["DeleteColumn"].Controls[0].Visible = discountCodeUsageCount == 0;

                if (discountCodeUsageCount > 0)
                {
                    LinkButton lnkDiscountCode = (LinkButton)dataItem.FindControl("lnkDiscountCode");
                    lnkDiscountCode.Text = Support.TruncateString(discountCode.Code, 15);
                    if (discountCode.Code.Length >= 15)
                    {
                        lnkDiscountCode.ToolTip = discountCode.Code;
                    }

                    lnkDiscountCode.Visible = true;
                }
                else
                {
                    Label litDiscountCode = (Label)dataItem.FindControl("litDiscountCode");
                    litDiscountCode.Text = Support.TruncateString(discountCode.Code, 15);
                    if (discountCode.Code.Length >= 15)
                    {
                        litDiscountCode.ToolTip = discountCode.Code;
                    }

                    litDiscountCode.Visible = true;
                }

                dataItem["Duration"].Text = discountCode.Duration.ToString() + (discountCode.Duration == 1 ? " week" : " weeks");
                dataItem["InstanceCount"].Text = discountCode.InstanceCount.ToString() + (discountCodeUsageCount > 0 ? (" (" + discountCodeUsageCount + " used)") : string.Empty);
                dataItem["Discount"].Text = (discountCode.Discount / 100).ToString("#0.##%", CultureInfo.InvariantCulture);
                dataItem["ExpireDate"].Text = Support.FormatDate(discountCode.ExpireDate);
                dataItem["CreatedDate"].Text = Support.FormatDate(discountCode.CreatedDate);

            }
        }

        protected void gvDiscountCodeUsage_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                int discountCodeUsageId = (int)dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["DiscountCodeUsageId"];

                DiscountCodeUsage discountUsage = (from dcu in DataContext.DiscountCodeUsages
                                                   where dcu.DiscountCodeUsageId == discountCodeUsageId
                                                   select dcu).FirstOrDefault();

                dataItem["StartDate"].Text = Support.FormatDate(discountUsage.CreatedDate);
                dataItem["EndDate"].Text = Support.FormatDate(discountUsage.EndDate);

                HyperLink lnkCompanyName = (HyperLink)dataItem.FindControl("lnkCompanyName");
                if (lnkCompanyName != null)
                {
                    lnkCompanyName.Text = Support.TruncateString(discountUsage.Company.CompanyName, 40);
                    lnkCompanyName.ToolTip = discountUsage.Company.CompanyName;
                    lnkCompanyName.NavigateUrl = ResolveUrl("~/Company/CompanyDetails.aspx") +
                            "?CompanyID=" + discountUsage.Company.CompanyId.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        protected void gvDiscountCodes_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                switch (e.CommandName)
                {
                    case "ViewDiscountUsage":
                        GridDataItem dataItem = (GridDataItem)e.Item;
                        SelectedDiscountCodeId = (int)dataItem.GetDataKeyValue("DiscountCode.DiscountCodeID");

                        DiscountCode discountCode = (from dc in DataContext.DiscountCodes
                                                     where dc.DiscountCodeID == SelectedDiscountCodeId
                                                     select dc).FirstOrDefault();
                        popupDiscountUsage.Title = "Discount Code Usage - " + discountCode.Code;
                        LoadDiscountUsage();
                        gvDiscountUsage.DataBind();
                        popupDiscountUsage.ShowPopup();
                        break;
                }
            }
        }

        /// <summary>
        /// Determines whether the discount code is a duplicated one or an existing one.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="discountCodeID">The discount code ID.</param>
        /// <param name="isAddNew">if set to <c>true</c> [is add new].</param>
        /// <returns>
        /// 	<c>true</c> if [is valid to save] [the specified code]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidtoSave(string code, int discountCodeID, bool isAddNew)
        {
            var existingcodeList = (from dc in DataContext.DiscountCodes
                                    where dc.Code.Equals(code.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                    select dc).ToList<DiscountCode>();
            bool status = false;

            if (isAddNew)
            {
                status = existingcodeList.Count() == 0 ? true : false;
            }
            else if ((existingcodeList.Count() == 1 && existingcodeList[0].DiscountCodeID == discountCodeID) || existingcodeList.Count() == 0)
            {
                status = true;
            }
            return status;
        }



        protected void gvDiscountCodes_OnCancelCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            IsPageDirty = false;
        }

        protected void gvDiscountCodes_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (Page.IsValid)
            {
                ClearUI();
                uplAddDisCode.Update();
                //Get the GridEditableItem of the RadGrid    
                GridEditableItem editedItem = e.Item as GridEditableItem;

                TextBox txtDiscountCode = (TextBox)editedItem.FindControl("txtDiscountCode");
                RadNumericTextBox txtDuration = (RadNumericTextBox)editedItem.FindControl("txtDuration");
                RadNumericTextBox txtInstanceCount = (RadNumericTextBox)editedItem.FindControl("txtInstanceCount");
                RadNumericTextBox txtDiscountPercentage = (RadNumericTextBox)editedItem.FindControl("txtDiscountPercentage");
                RadDatePicker dtExpireDate = (RadDatePicker)editedItem.FindControl("dtExpireDate");

                int discountCodeID = (int)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["DiscountCode.DiscountCodeID"];
                int discountCodeUsageCount = GetDiscountUsageCount(discountCodeID);


                DateTime originalLastUpdatedDate = (DateTime)editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["DiscountCode.LastUpdatedDate"];

                DiscountCode discountCode = (from dc in DataContext.DiscountCodes
                                             where dc.DiscountCodeID == discountCodeID
                                             select dc).FirstOrDefault();

                if (dtExpireDate.SelectedDate.HasValue && dtExpireDate.SelectedDate.Value < Today)
                {
                    Image imgExpireDate = (Image)editedItem.FindControl("imgExpireDate");
                    imgExpireDate.Visible = true;
                    imgExpireDate.ToolTip = string.Format("Expiry date cannot be a past date.");
                    e.Canceled = true;
                    return;
                }

                if (discountCode == null)
                {
                    StageBitzException.ThrowException(new ConcurrencyException(ExceptionOrigin.ManageDiscounts, discountCodeID));
                }

                if (discountCodeUsageCount > 0)
                {
                    if (discountCodeUsageCount > txtInstanceCount.Value.Value)
                    {
                        Image imgInstanceCountError = (Image)editedItem.FindControl("imgInstanceCountError");
                        imgInstanceCountError.Visible = true;
                        imgInstanceCountError.ToolTip = string.Format("Instance count cannot be reduced below the used count {0}.", discountCodeUsageCount);
                        e.Canceled = true;
                        return;
                    }

                    discountCode.InstanceCount = (int)txtInstanceCount.Value.Value;
                }
                else
                {
                    if (!IsValidtoSave(txtDiscountCode.Text, discountCodeID, false))
                    {

                        Image imgDiscountCodeError = (Image)editedItem.FindControl("imgDiscountCodeError");
                        imgDiscountCodeError.Visible = true;
                        imgDiscountCodeError.ToolTip = "Please choose a different Discount Code.";
                        e.Canceled = true;
                        return;
                    }

                    discountCode.Code = txtDiscountCode.Text.Trim();
                    discountCode.Discount = (decimal)txtDiscountPercentage.Value.Value;
                    discountCode.InstanceCount = (int)txtInstanceCount.Value.Value;
                    discountCode.Duration = (int)txtDuration.Value.Value;
                    discountCode.ExpireDate = dtExpireDate.SelectedDate.Value;
                }

                discountCode.LastUpdatedByUserId = UserID;
                discountCode.LastUpdatedDate = Now;
                DataContext.SaveChanges();
                IsPageDirty = false;//Clear the page dirty
                gvDiscountCodes.EditIndexes.Clear();
                gvDiscountCodes.MasterTableView.IsItemInserted = false;
                gvDiscountCodes.Rebind();

            }

        }

        protected void gvDiscountCodes_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadData();
        }

        protected void gvDiscountUsage_NeedDataSource(object source, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            LoadDiscountUsage();
        }

        private void LoadDiscountUsage()
        {
            var discountUsage = from dcu in DataContext.DiscountCodeUsages
                                join c in DataContext.Companies on dcu.CompanyId equals c.CompanyId
                                where dcu.DiscountCodeId == SelectedDiscountCodeId
                                select new { dcu.DiscountCodeUsageId, c.CompanyName, dcu.StartDate, dcu.EndDate };

            gvDiscountUsage.DataSource = discountUsage;

        }

        protected void chkHideExpiredCode_CheckedChanged(object sender, EventArgs e)
        {
            LoadData();
            gvDiscountCodes.DataBind();
        }
    }
}
