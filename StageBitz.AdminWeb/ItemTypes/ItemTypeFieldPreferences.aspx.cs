using StageBitz.AdminWeb.Common.Helpers;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.ItemTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.AdminWeb.ItemTypes
{


    public partial class ItemTypeFieldPreferences : PageBase
    {

        List<ItemTypeData> ItemTypeDataList;

        private class NewTemplateColumn : GridTemplateColumn
        {
            public void InstantiateIn(Control container)
            { }
        }

        private class TemplateColumn : ITemplate
        {
            public int ColumnID
            {
                get;


                set;
            }

            public void InstantiateIn(Control container)
            {
                CheckBox chk = new CheckBox();
                chk.ID = this.ColumnID.ToString();
                container.Controls.Add(chk);
            }
        }

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();

            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                base.LoadBL();
                GenrateItemTypeColumns();
                LoadData();
            }
        }

        protected void gvItemTypeFieldPreference_ItemDataBound(object sender, GridItemEventArgs e)
        {
            var dataItem = e.Item.DataItem;

            if (e.Item is GridDataItem)
            {
                foreach (ItemTypeData itemTypeData in ItemTypeDataList)
                {
                    var rowArray = ((global::System.Data.DataRowView)(dataItem)).Row.ItemArray;
                    ItemFieldTypeData itemFieldTypeData = (ItemFieldTypeData)(rowArray.Where(i => (i is ItemFieldTypeData) && ((ItemFieldTypeData)i).ItemTypeId == itemTypeData.Id).FirstOrDefault());
                    CheckBox chk = e.Item.FindControl(itemTypeData.Id.ToString()) as CheckBox;
                    if (chk != null)
                    {
                        chk.Checked = itemFieldTypeData.IsSelected;
                        chk.Enabled = !itemFieldTypeData.IsSelected;
                        chk.Attributes.Add("data-fieldid", itemFieldTypeData.FieldId.ToString(CultureInfo.InvariantCulture));
                        chk.Attributes.Add("data-itemtypeid", itemFieldTypeData.ItemTypeId.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            if (e.Item is GridGroupHeaderItem)
            {
                GridGroupHeaderItem item = (GridGroupHeaderItem)e.Item;
                DataRowView groupDataRow = (DataRowView)e.Item.DataItem;
                item.DataCell.Text = groupDataRow["GroupName"].ToString();
            }


        }

        protected void gvItemTypeFieldPreference_ItemCreated(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridGroupHeaderItem)
            {
                (e.Item as GridGroupHeaderItem).Cells[0].Controls.Clear();
            }
        }

        #endregion



        private void GenrateItemTypeColumns()
        {
            ItemTypeDataList = this.GetBL<ItemTypesBL>().GetAllItemTypeData().ToList();
            NewTemplateColumn[] templateColumns = new NewTemplateColumn[ItemTypeDataList.Count];
            TemplateColumn[] cols = new TemplateColumn[ItemTypeDataList.Count];
            int i = 0;
            foreach (ItemTypeData itemTypeData in ItemTypeDataList)
            {
                templateColumns[i] = new NewTemplateColumn();
                cols[i] = new TemplateColumn();
                cols[i].ColumnID = itemTypeData.Id;
                templateColumns[i].HeaderText = itemTypeData.Name;
                templateColumns[i].UniqueName = itemTypeData.Id.ToString();
                templateColumns[i].HeaderStyle.Width = Unit.Pixel(60);
                templateColumns[i].ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                templateColumns[i].ItemTemplate = cols[i];
                gvItemTypeFieldPreference.MasterTableView.Columns.Add(templateColumns[i]);
                i++;
            }
        }

        private void LoadData()
        {
            DataTable ItemTypeTable = this.GetBL<ItemTypesBL>().GetItemTypeFieldsPreferences();
            gvItemTypeFieldPreference.DataSource = ItemTypeTable;
        }

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static void SaveItemFieldPreferences(List<ItemTypeFieldsValues> objectList)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                List<ItemTypeField> allItemTypeFields = dataContext.ItemTypeFields.ToList();

                List<FieldData> allFields = (from f in dataContext.Fields
                                             where f.IsActive == true && f.FieldGroup.IsActive == true
                                             orderby f.FieldGroup.SortOrder
                                             select new FieldData
                                             {
                                                 Field = f,
                                                 FieldId = f.FieldId,
                                                 FieldHtml = f.FieldType.FieldTypeHtml,
                                                 FieldHtmlControl = f.FieldType.HtmlControl,
                                                 FieldInnerHtml = f.FieldType.InnerHtmlTemplate,
                                                 Group = f.FieldGroup,
                                                 GroupId = f.GroupId,
                                                 FieldOptions = f.FieldOptions.OrderBy(fo => fo.SortOrder).Select(fo => new FieldOptionValue { OptionId = fo.FieldOptionId, OptionValue = fo.OptionText }),
                                                 FieldAttributes = f.FieldAttributes.Select(fa => new FieldAttributeValue { TagName = fa.Attribute.TagName, AttributeValue = fa.FieldAttributeValue }),//(from fa in dataContext.FieldAttributes  where fa.FieldId==f.FieldId select new FieldAttributeValue { AttributeId = fa.AttributeId, AttributeValue = fa.FieldAttributeValue }).ToList(),
                                             }).ToList();

                List<Data.FieldGroup> fieldGroups = allFields.Select(fg => fg.Group).OrderBy(fg => fg.SortOrder).Distinct().ToList();
                var itemTypeHtmls = (from it in dataContext.ItemTypes
                                     from ith in dataContext.ItemTypeHtmls.Where(ith => ith.ItemTypeId == it.ItemTypeId).DefaultIfEmpty()
                                     select new { it.ItemTypeId, ith }).ToList();
                List<int> itemTypeIds = itemTypeHtmls.Select(ith => ith.ItemTypeId).ToList();

                //these are hard coded countries for the Sizing field.
                List<String> countries = new List<string>();
                countries.Add("Australia");
                countries.Add("United States");
                countries.Add("United Kingdom");
                List<Country> countryList = dataContext.Countries.Where(c => countries.Contains(c.CountryName)).ToList();

                List<FieldData> groupFields;
                StringBuilder groupHtml, leftColumnHtml, rightColumnHtml, fieldHtml, fieldOptions, masterHtml;
                int[] itemTypeFields;
                const string FIELD_VISIBLE = "Visible";
                const string NAME = "Name";
                const string COLUMN1 = "@Column1";
                const string COLUMN2 = "@Column2";
                const string FIELD = "Field";
                const string CLASSINVISIBLE = "remove_dynamic_field";
                const string FIELDOPTIONS = "@Options";
                const string FIELDOPTIONValue = "@Option";
                const string FIELDOPTIONID = "@optionvalue";
                const string TAGWATERMARK = "@watermark";
                const string TAGCLASS = "@class";
                const string TAGHEIGHT = "@height";
                const string TAGWIDTH = "@width";
                const string TAGMAXLENGTH = "@maxlength";
                const string TAGFieldId = "@fieldId";
                const string TAGCountryId = "@countryId";
                bool groupHasFields = false;
                int i = 0;
                foreach (int itemTypeId in itemTypeIds)
                {
                    masterHtml = new StringBuilder(Utils.GetSystemValue("ItemFieldsMasterHtml"));
                    leftColumnHtml = new StringBuilder();
                    rightColumnHtml = new StringBuilder();
                    itemTypeFields = objectList.Where(itfv => itfv.ItemTypeId == itemTypeId).Select(itfv => itfv.Fields).FirstOrDefault();
                    if (itemTypeFields != null)
                    {
                        foreach (Data.FieldGroup fieldGroup in fieldGroups)
                        {
                            groupFields = allFields.Where(f => f.GroupId == fieldGroup.FieldGroupId).ToList();

                            groupHtml = new StringBuilder(fieldGroup.FieldGroupHtml);
                            groupHtml.Replace(fieldGroup.TagName + ":" + NAME, fieldGroup.DisplayName);
                            foreach (FieldData fieldData in groupFields)
                            {
                                if (itemTypeFields != null && Array.IndexOf(itemTypeFields, fieldData.FieldId) > -1)
                                {
                                    groupHasFields = true;
                                    groupHtml.Replace(fieldData.Field.TagName + ":" + FIELD_VISIBLE, string.Empty);
                                    groupHtml.Replace(fieldData.Field.TagName + ":" + NAME, fieldData.Field.DisplayName);
                                    fieldHtml = new StringBuilder(fieldData.FieldHtml);
                                    fieldHtml.Replace(TAGFieldId, fieldData.FieldId.ToString());
                                    foreach (FieldAttributeValue attribute in fieldData.FieldAttributes)
                                    {
                                        fieldHtml.Replace(attribute.TagName, attribute.AttributeValue);
                                    }
                                    fieldHtml.Replace(TAGWATERMARK, string.Empty);
                                    fieldHtml.Replace(TAGCLASS, string.Empty);
                                    fieldHtml.Replace(TAGHEIGHT, string.Empty);
                                    fieldHtml.Replace(TAGWIDTH, string.Empty);
                                    fieldHtml.Replace(TAGMAXLENGTH, string.Empty);

                                    if (fieldData.FieldOptions.Count() > 0)
                                    {
                                        fieldOptions = new StringBuilder();
                                        foreach (FieldOptionValue option in fieldData.FieldOptions)
                                        {
                                            StringBuilder fieldOption = new StringBuilder(fieldData.FieldInnerHtml);
                                            fieldOption.Replace(FIELDOPTIONID, option.OptionId.ToString());
                                            fieldOption.Replace(FIELDOPTIONValue, option.OptionValue);
                                            if (fieldData.FieldHtmlControl.Equals("CountryDropDown"))
                                            { 
                                                fieldOption.Replace(TAGCountryId, GetSizingCountryValue(countryList,option.OptionValue));
                                            }
                                            fieldOptions.Append(fieldOption);
                                        }
                                        fieldHtml.Replace(FIELDOPTIONS, fieldOptions.ToString());
                                    }

                                    //we need to insert record if and only if the record doesn't exist.
                                    if (allItemTypeFields.Where(itf => itf.FieldId == fieldData.FieldId && itf.ItemTypeId == itemTypeId).FirstOrDefault() == null)
                                    {
                                        Data.ItemTypeField itemTypeField = new ItemTypeField();
                                        itemTypeField.FieldId = fieldData.FieldId;
                                        itemTypeField.ItemTypeId = itemTypeId;
                                        dataContext.ItemTypeFields.AddObject(itemTypeField);
                                    }
                                    groupHtml.Replace(fieldData.Field.TagName + ":" + FIELD, fieldHtml.ToString());
                                }
                                else
                                {
                                    //we need to delete the record if only it already exists in the table.
                                    Data.ItemTypeField itemTypeField = allItemTypeFields.Where(itf => itf.FieldId == fieldData.FieldId && itf.ItemTypeId == itemTypeId).FirstOrDefault();
                                    if (itemTypeField != null)
                                    {
                                        dataContext.ItemTypeFields.DeleteObject(itemTypeField);
                                    }
                                    //hide the field from the group
                                    groupHtml.Replace(fieldData.Field.TagName + ":" + FIELD_VISIBLE, CLASSINVISIBLE);
                                }
                            }
                            //if the group has at least one field that group attached to the group.
                            if (groupHasFields)
                            {
                                if (i++ % 2 == 0)
                                {
                                    leftColumnHtml.Append(groupHtml);
                                }
                                else
                                {
                                    rightColumnHtml.Append(groupHtml);
                                }
                            }
                            groupHasFields = false;
                        }
                    }
                    masterHtml.Replace(COLUMN1, leftColumnHtml.ToString());
                    masterHtml.Replace(COLUMN2, rightColumnHtml.ToString());
                    Data.ItemTypeHtml itemTypeHtml = itemTypeHtmls.Where(ith => ith.ItemTypeId == itemTypeId).Select(ith => ith.ith).FirstOrDefault();
                    if (itemTypeHtml != null)
                    {
                        itemTypeHtml.Html = masterHtml.ToString();
                    }
                    else
                    {
                        itemTypeHtml = new ItemTypeHtml();
                        itemTypeHtml.ItemTypeId = itemTypeId;
                        itemTypeHtml.Html = masterHtml.ToString();
                        dataContext.ItemTypeHtmls.AddObject(itemTypeHtml);
                    }
                    itemTypeFields = null;
                    i = 0;
                }
                dataContext.SaveChanges();
            }
        }

        public static string GetSizingCountryValue(List<Country> countryList, string countryName)
        {
            string countryDropDownValue = "0";
            switch (countryName)
            {
                case "AUS":
                    countryDropDownValue = countryList.Where(c => c.CountryName.Equals("Australia")).FirstOrDefault().CountryId.ToString();
                    break;
                case "US":
                    countryDropDownValue = countryList.Where(c => c.CountryName.Equals("United States")).FirstOrDefault().CountryId.ToString();
                    break;
                case "UK":
                    countryDropDownValue = countryList.Where(c => c.CountryName.Equals("United Kingdom")).FirstOrDefault().CountryId.ToString();
                    break;
            }
            return countryDropDownValue;
        }
    }
}