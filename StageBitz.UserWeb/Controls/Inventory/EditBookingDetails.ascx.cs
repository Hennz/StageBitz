using Newtonsoft.Json;
using StageBitz.Common;
using StageBitz.Data.DataTypes;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for edit booking details.
    /// </summary>
    public partial class EditBookingDetails : UserControlBase
    {
        #region Private Classes

        /// <summary>
        ///
        /// </summary>
        private class NewTemplateColumn : GridTemplateColumn
        {
            public void InstantiateIn(Control container)
            { }
        }

        /// <summary>
        ///
        /// </summary>
        private class TemplateColumn : ITemplate
        {
            /// <summary>
            /// Gets or sets the column identifier.
            /// </summary>
            /// <value>
            /// The column identifier.
            /// </value>
            public string ColumnID
            {
                get;
                set;
            }

            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn(Control container)
            {
                CheckBox chk = new CheckBox();
                chk.ID = this.ColumnID.ToString();
                container.Controls.Add(chk);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class BaseHtmlInputCheckBox : System.Web.UI.HtmlControls.HtmlInputCheckBox
        {
            public string Text { get; set; }

            public override void RenderControl(HtmlTextWriter writer)
            {
                base.RenderControl(writer);
                writer.Write(this.Text);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class HeaderTemplateColumn : ITemplate
        {
            /// <summary>
            /// Gets or sets the column identifier.
            /// </summary>
            /// <value>
            /// The column identifier.
            /// </value>
            public string ColumnID
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the display text.
            /// </summary>
            /// <value>
            /// The display text.
            /// </value>
            public string DisplayText
            {
                get;
                set;
            }

            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn(Control container)
            {
                BaseHtmlInputCheckBox chk = new BaseHtmlInputCheckBox();
                chk.Text = string.Format(DisplayText.Trim().Length == 1 ? "&nbsp;&nbsp{0}" : "&nbsp;{0}", DisplayText);
                chk.Attributes.Add("onChange", "HeaderCheckClicked(this);");
                chk.Value = DisplayText;
                chk.ID = this.ColumnID.ToString();
                container.Controls.Add(chk);
                HiddenField hdnVal = new HiddenField();
                hdnVal.ID = "hdn" + this.ColumnID.ToString();
                container.Controls.Add(hdnVal);
            }
        }

        #endregion Private Classes

        #region Properties

        /// <summary>
        /// Gets or sets the item type identifier.
        /// </summary>
        /// <value>
        /// The item type identifier.
        /// </value>
        public int ItemTypeId
        {
            get
            {
                if (ViewState["ItemTypeId"] == null)
                {
                    ViewState["ItemTypeId"] = 16;
                }

                return (int)ViewState["ItemTypeId"];
            }

            set
            {
                ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the booking identifier.
        /// </summary>
        /// <value>
        /// The booking identifier.
        /// </value>
        public int BookingId
        {
            get
            {
                if (ViewState["BookingId"] == null)
                {
                    ViewState["BookingId"] = 0;
                }

                return (int)ViewState["BookingId"];
            }

            set
            {
                ViewState["BookingId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the viewing company identifier.
        /// </summary>
        /// <value>
        /// The viewing company identifier.
        /// </value>
        public int ViewingCompanyId
        {
            get
            {
                if (ViewState["ViewingCompanyId"] == null)
                {
                    ViewState["ViewingCompanyId"] = 0;
                }

                return (int)ViewState["ViewingCompanyId"];
            }

            set
            {
                ViewState["ViewingCompanyId"] = value;
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
                if (ViewState["CompanyId"] == null)
                {
                    ViewState["CompanyId"] = 0;
                }

                return (int)ViewState["CompanyId"];
            }
            set
            {
                ViewState["CompanyId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the call back URL.
        /// </summary>
        /// <value>
        /// The call back URL.
        /// </value>
        public string CallBackURL
        {
            get
            {
                if (ViewState["CallBackURL"] == null)
                {
                    ViewState["CallBackURL"] = string.Empty;
                }

                return (string)ViewState["CallBackURL"];
            }
            set
            {
                ViewState["CallBackURL"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether todate change.
        /// </summary>
        /// <value>
        /// <c>true</c> if todate change; otherwise, <c>false</c>.
        /// </value>
        public bool IsToDateChange
        {
            get
            {
                if (ViewState["IsToDateChange"] == null)
                {
                    ViewState["IsToDateChange"] = true;
                }

                return (bool)ViewState["IsToDateChange"];
            }

            set
            {
                ViewState["IsToDateChange"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this user is inventory manager.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user is inventory manager; otherwise, <c>false</c>.
        /// </value>
        public bool IsInventoryManager
        {
            get
            {
                if (ViewState["IsInventoryManager"] == null)
                {
                    ViewState["IsInventoryManager"] = true;
                }

                return (bool)ViewState["IsInventoryManager"];
            }

            set
            {
                ViewState["IsInventoryManager"] = value;
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
            if (!IsPostBack)
            {
                GenrateItemTypeColumns();
                LoadData();
                btnCancel.PostBackUrl = CallBackURL;
            }
        }

        /// <summary>
        /// Handles the PreRender event of the gvBookingDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gvBookingDetails_PreRender(object sender, EventArgs e)
        {
            gvBookingDetails.MasterTableView.GetColumn("ItemBookingId").Display = false;
            gvBookingDetails.MasterTableView.GetColumn("ItemBriefName").Visible = (CompanyId == 0);
            gvBookingDetails.MasterTableView.GetColumn("Date").HeaderText = IsToDateChange ? "Booked To" : "Booked From";
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            Data.ItemType itemType = Utils.GetItemTypeById(ItemTypeId);
            litTitle.Text = string.Format("Change '{0}' Booking Dates for selected {1} Items.", IsToDateChange ? "To" : "From", itemType != null ? itemType.Name : string.Empty);
            litDateTypeHeader.Text = litDateTypeBody.Text = IsToDateChange ? "'To'" : "'From'";
            BookingDetailsEditRequest bookingDetailsEditRequest = new BookingDetailsEditRequest()
            {
                BookingId = BookingId,
                CompanyId = ViewingCompanyId,
                ItemTypeId = ItemTypeId,
                ToDay = Utils.Today,
                IsToDateEdit = IsToDateChange,
                IsInventoryManager = IsInventoryManager,
                UserId = UserID
            };
            //dtPeriod.SelectedDate = Today;
            //Need to just initiate the grid by setting empty data set and a virtual Item count.
            gvBookingDetails.DataSource = new List<BookingDetailsToEdit>();
            gvBookingDetails.DataBind();
            gvBookingDetails.VirtualItemCount = 1000;
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "LoadBookingDetails", string.Concat("LoadBookingDetails_", this.ClientID, "(", JsonConvert.SerializeObject(bookingDetailsEditRequest), ");"), true);
        }

        /// <summary>
        /// Genrates the item type columns.
        /// </summary>
        private void GenrateItemTypeColumns()
        {
            NewTemplateColumn[] templateColumns = new NewTemplateColumn[32];
            TemplateColumn[] cols = new TemplateColumn[32];
            HeaderTemplateColumn[] hcols = new HeaderTemplateColumn[32];

            for (int i = 1; i < 32; i++)
            {
                templateColumns[i] = new NewTemplateColumn();
                cols[i] = new TemplateColumn();
                hcols[i] = new HeaderTemplateColumn();
                cols[i].ColumnID = string.Concat("chkBookingDetail", i);
                hcols[i].ColumnID = string.Concat("hchkBookingDetail", i);
                hcols[i].DisplayText = i.ToString();
                templateColumns[i].HeaderText = (i).ToString();
                templateColumns[i].UniqueName = i.ToString();
                templateColumns[i].HeaderTemplate = hcols[i];
                templateColumns[i].HeaderStyle.Width = Unit.Pixel(40);
                templateColumns[i].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                templateColumns[i].ItemTemplate = cols[i];
                gvBookingDetails.MasterTableView.Columns.Add(templateColumns[i]);
            }
        }

        #endregion Private Methods
    }
}