using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Inventory;
using StageBitz.UserWeb.Common.Helpers;

namespace StageBitz.UserWeb.Controls.Inventory
{
    /// <summary>
    /// User control for Business card.
    /// </summary>
    public partial class ItemVisibilityToolTip : UserControlBase
    {
        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public int ItemId
        {
            get
            {
                if (ViewState["ItemId"] == null)
                {
                    return 0;
                }

                return (int)ViewState["ItemId"];
            }
            set
            {
                ViewState["ItemId"] = value;
            }
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            if (this.ItemId > 0)
            {
                Code above_sharedInventory = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_SHAREDINVENTORY");
                Code above_IO = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IO");
                Code above_IS = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IS");
                Code above_IA = Utils.GetCodeByValue("InventoryVisibilityLevel", "ABOVE_IA");

                Data.Item item = GetBL<InventoryBL>().GetItem(this.ItemId);
                if (item != null)
                {
                    string template = "<div>This item is visible to:</div><div><ul>{0}{1}{2}{3}</ul></div>";
                    string text = string.Empty;
                    if (item.VisibilityLevelCodeId == above_IA.CodeId)
                    {
                        text = string.Format(template, "<li>Administrators only</li>", string.Empty, string.Empty, string.Empty);
                    }
                    else if (item.VisibilityLevelCodeId == above_IS.CodeId)
                    {
                        text = string.Format(template, string.Empty, "<li>Inventory Staff</li>", string.Empty, string.Empty);
                    }
                    else if (item.VisibilityLevelCodeId == above_IO.CodeId)
                    {
                        text = string.Format(template, string.Empty, "<li>Inventory Staff</li>", "<li>Inventory Observers</li>", string.Empty);
                    }
                    else if (item.VisibilityLevelCodeId == above_sharedInventory.CodeId)
                    {
                        text = string.Format(template, string.Empty, "<li>Inventory Staff</li>", "<li>Inventory Observers</li>", "<li>Visitors from Shared Inventories</li>");
                    }

                    ltrlVisibility.Text = text;
                }

                upnlVisibility.Update();
            }
        }
    }
}