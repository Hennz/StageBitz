using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.ItemTypes;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Web.Http;

namespace StageBitz.UserWeb.Services
{
    /// <summary>
    /// Item type field service controller.
    /// </summary>
    [Authorize]
    public class ItemTypeServiceController : ApiController
    {
        /// <summary>
        /// Gets the item brief details.
        /// </summary>
        /// <param name="itemBriefRequestDetails">The item brief request details.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemBriefDetails GetItemBriefDetails(ItemBriefRequestDetails itemBriefRequestDetails)
        {
            ItemBriefDetails itemBriefDetails = new Data.DataTypes.ItemBriefDetails();

            using (StageBitzDB dataContext = new StageBitzDB())
            {
                try
                {
                    ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                    ItemTypesBL itemTypesBL = new ItemTypesBL(dataContext);

                    Data.ItemBrief itemBrief = itemBriefBL.GetItemBrief(itemBriefRequestDetails.ItemBriefId);

                    int projectId = 0;
                    if (itemBrief != null)
                        projectId = itemBrief.ProjectId;

                    itemBriefDetails.CountryId = itemBriefBL.GetCountryIdByItemBriefId(itemBrief.ItemBriefId);

                    itemBriefDetails.ItemBriefInfo = itemBriefBL.GetItemBriefInfoByItemBrief(itemBrief);
                    itemBriefDetails.IsReadOnly = Support.IsReadOnlyRightsForProject(projectId, itemBriefRequestDetails.UserId);
                    itemBriefDetails.CanSeeBudgetSummary = Support.CanSeeBudgetSummary(itemBriefRequestDetails.UserId, projectId);
                    int itemTypeId = itemBriefRequestDetails.ItemTypeId.HasValue ?
                        itemBriefRequestDetails.ItemTypeId.Value : itemBriefBL.GetItemBriefType(itemBriefRequestDetails.ItemBriefId).ItemTypeId;
                    itemBriefDetails.DisplayMarkUp = itemTypesBL.GetItemTypeHTML(itemTypeId);
                    itemBriefDetails.ItemBriefValues = itemTypesBL.GetItemBriefFieldValues(itemBriefRequestDetails.ItemBriefId).Select(ibfv =>
                                                            new ValuesInfo
                                                            {
                                                                Id = ibfv.ItemBriefValueId,
                                                                FieldId = ibfv.FieldId,
                                                                FieldOptionId = ibfv.FieldOptionId,
                                                                Value = ibfv.Value
                                                            }).ToList();

                    int itemId = 0;
                    int userId = 0;

                    ItemBooking itemBooking = itemBriefBL.GetInUseItemBooking(itemBriefRequestDetails.ItemBriefId);
                    if (itemBooking != null)
                        itemId = itemBooking.ItemId;

                    userId = itemBriefRequestDetails.UserId;
                    ItemDetails itemDetails = itemTypesBL.GetItemDetails(itemId, itemBriefRequestDetails.ItemBriefId, userId, null, null, null);
                    if (itemDetails != null)
                        itemBriefDetails.ItemDetails = itemDetails;
                }
                catch (Exception ex)
                {
                    AgentErrorLog.HandleException(ex);
                }
                return itemBriefDetails;
            }
        }

        /// <summary>
        /// Saves the item brief details.
        /// </summary>
        /// <param name="itemBriefDetails">The item brief details.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemBriefResulstObject SaveItemBriefDetails(ItemBriefDetails itemBriefDetails)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                return itemBriefBL.SaveItemBrief(itemBriefDetails);
            }
        }

        /// <summary>
        /// Gets the item details.
        /// </summary>
        /// <param name="itemRequestDetails">The item request details.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemDetails GetItemDetails(ItemRequestDetails itemRequestDetails)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ItemTypesBL itemTypesBL = new ItemTypesBL(dataContext);
                return itemTypesBL.GetItemDetails(itemRequestDetails.ItemId, itemRequestDetails.ItemBriefId, itemRequestDetails.UserId, itemRequestDetails.ItemTypeId, Utils.GetDatetime(itemRequestDetails.FromDate, false), Utils.GetDatetime(itemRequestDetails.ToDate, false));
            }
        }

        /// <summary>
        /// Saves the item details.
        /// </summary>
        /// <param name="itemDetails">The item details.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemResultObject SaveItemDetails(ItemDetails itemDetails)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ItemTypesBL itemTypesBL = new ItemTypesBL(dataContext);
                ItemResultObject returnObj = itemTypesBL.SaveItemDetails(itemDetails, false, true);
                return returnObj;
            }
        }

        /// <summary>
        /// Saves the and complete item details.
        /// </summary>
        /// <param name="itemDetails">The item details.</param>
        /// <returns></returns>
        [HttpPost]
        public ItemResultObject SaveAndCompleteItemDetails(ItemDetails itemDetails)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                ItemBriefBL itemBriefBL = new ItemBriefBL(dataContext);
                return itemBriefBL.CompleteItemBrief(itemDetails);
            }
        }
    }
}