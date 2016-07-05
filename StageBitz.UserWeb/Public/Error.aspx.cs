using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StageBitz.UserWeb.Common.Helpers;
using StageBitz.Common.Exceptions;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;

namespace StageBitz.UserWeb.Public
{
    /// <summary>
    /// Landing page for errors.
    /// </summary>
    public partial class Error : PageBase
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StageBitzException ex = StageBitzException.GetLastException();

                if (ex != null)
                {
                    plcGeneric.Visible = false;

                    if (ex is ConcurrencyException)
                    {
                        //Find the project id
                        int relatedId = 0;
                        switch (ex.Origin)
                        {
                            case ExceptionOrigin.ItemBriefDetails:
                                relatedId = DataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ex.RelatedId).FirstOrDefault().ProjectId;
                                break;

                            case ExceptionOrigin.ItemBriefList:
                            case ExceptionOrigin.ProjectDetails:
                            case ExceptionOrigin.ItemBriefTasks:
                            case ExceptionOrigin.UserDetails:
                                relatedId = ex.RelatedId;
                                break;
                            default:
                                break;
                        }

                        //Generate the link
                        switch (ex.Origin)
                        {
                            case ExceptionOrigin.UserDetails:
                                lnkPage.InnerText = "Go Back";
                                lnkPage.HRef = "~/Personal/UserDetails.aspx";
                                break;
                            default:
                                lnkPage.InnerText = "View Updates";
                                lnkPage.HRef = string.Format("~/Project/ProjectNotifications.aspx?projectid={0}", relatedId);
                                break;
                        }

                        plcConcurrency.Visible = true;

                    }
                    else
                    {
                        switch (ex.Origin)
                        {
                            case ExceptionOrigin.ProjectClose:
                                plcClosedProject.Visible = true;
                                break;
                            case ExceptionOrigin.ItemDelete:
                                DeletedItemDatails itemDeleteData = this.GetBL<InventoryBL>().GetDeleteItemData(ex.RelatedId);
                                //string ItemDeleteMessage = string.Concat("This Item was deleted by ",itemDeleteData.ItemDeletedUser," on ", itemDeleteData.ItemDeletedDate,". Please contact ","<a href='mailto:", itemDeleteData.ItemDeletedUserEmail, "'>", itemDeleteData.ItemDeletedUser, "</a>");
                                ltrItemDeletedUser.Text = itemDeleteData.ItemDeletedUser;
                                ltrDeletedDate.Text = Support.FormatDate(itemDeleteData.ItemDeletedDate.Date);
                                lnkItemDeletedUserEmail.HRef = "mailto:" + itemDeleteData.ItemDeletedUserEmail;
                                lnkItemDeletedUserEmail.InnerText = itemDeleteData.ItemDeletedUser;
                                string[] msgArray = ex.Message.Split('=');
                                if (msgArray.Length > 1)
                                    lnkInventoryPage.HRef = string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", msgArray[1]);
                                plcItemDeleted.Visible = true;
                                break;
                            case ExceptionOrigin.ItemNotVisibile:
                                plcItemNotVisibile.Visible = true;
                                Data.Item item = this.GetBL<InventoryBL>().GetItem(ex.RelatedId);
                                if (item != null)
                                {
                                    Data.User locationManager = this.GetBL<InventoryBL>().GetContactBookingManager(item.CompanyId.Value, item.LocationId);
                                    lnkInventoryAdminUserProfile.Text = string.Format("{0} {1}", locationManager.FirstName, locationManager.LastName);

                                    if (StageBitz.Common.Utils.CanAccessInventory(item.CompanyId.Value, this.UserID))
                                    {
                                        lnkInventoryPageItemNotVisible.HRef = string.Format("~/Inventory/CompanyInventory.aspx?CompanyId={0}", item.CompanyId.Value);
                                        lnkInventoryAdminUserProfile.NavigateUrl = string.Format("~/Personal/UserDetails.aspx?userId={0}", locationManager.UserId);
                                    }
                                    else
                                    {
                                        lnkGotoDashboard.Visible = true;
                                        lnkInventoryPageItemNotVisible.Visible = false;
                                        lnkInventoryAdminUserProfile.NavigateUrl = string.Format("mailto:{0}", locationManager.Email1);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}