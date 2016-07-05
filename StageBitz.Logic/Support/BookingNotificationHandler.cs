using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Inventory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StageBitz.Logic.Support
{
    /// <summary>
    /// Booking notification handler class.
    /// </summary>
    public class BookingNotificationHandler
    {
        /// <summary>
        /// Sends the notifications.
        /// </summary>
        public static void SendNotifications()
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                var bookingNotifications = from dbn in dataContext.DailyBookingNotifications
                                           join cbn in dataContext.CompanyBookingNumbers on dbn.CompanyBookingNumberId equals cbn.CompanyBookingNumberId
                                           select new
                                           {
                                               BookingNotification = dbn,
                                               RelatedTable = cbn.Booking.RelatedTable,
                                               BookingId = dbn.BookingId,
                                               CompanyId = cbn.CompanyId,
                                               BookingNumber = cbn.BookingNumber,
                                               CompanyName = cbn.Company.CompanyName,
                                               UserId = dbn.UserId,
                                               IsInventoryManager = dbn.IsInventoryManager,
                                               UserName = dbn.User.FirstName,
                                               ProjectId = dbn.Booking.RelatedId,
                                               UserEmail = dbn.User.Email1
                                           };

                foreach (var bookingNotification in bookingNotifications)
                {
                    string userWebUrl = Utils.GetSystemValue("SBUserWebURL");
                    string url = string.Empty;

                    if (bookingNotification.IsInventoryManager)
                    {
                        url = string.Format("{0}/Inventory/BookingDetails.aspx?BookingId={1}&CompanyId={2}", userWebUrl, bookingNotification.BookingId, bookingNotification.CompanyId);
                    }
                    else
                    {
                        //Now it has to decide which to open (IM or Non Project Booking User)
                        if (bookingNotification.RelatedTable == "Project")
                            url = string.Format("{0}/Project/ProjectBookingDetails.aspx?projectid={1}", userWebUrl, bookingNotification.ProjectId);
                        else
                            url = string.Format("{0}/Inventory/MyBookingDetails.aspx?BookingId={1}&CompanyId={2}", userWebUrl, bookingNotification.BookingId, bookingNotification.CompanyId);
                    }

                    EmailSender.SendBookingNotificationEmail(bookingNotification.UserEmail, bookingNotification.UserName,
                            bookingNotification.BookingNumber.ToString(CultureInfo.InvariantCulture), url);
                    dataContext.DailyBookingNotifications.DeleteObject(bookingNotification.BookingNotification);
                }

                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Sends the booking overdue emails.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        public static void SendBookingOverdueEmails(DateTime dateToConsider)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                int overdueEmailCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGOVERDUE");

                var overdueBookings = (from ibs in dataContext.ItemBookings.Where(ibs => dataContext.IsItemBookingOverdueByDate(ibs.ItemBookingId, dateToConsider))
                                       from eh in dataContext.EmailHistories.Where(eh => ibs.ItemBookingId == eh.RelatedId
                                             && eh.RelatedTable == "ItemBooking" && eh.EmailTemplateTypeCodeId == overdueEmailCodeId).DefaultIfEmpty().Take(1)
                                       //join ib in dataContext.ItemBriefs on ibs.RelatedId equals ib.ItemBriefId
                                       join b in dataContext.Bookings on ibs.BookingId equals b.BookingId
                                       from ib in dataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                       join i in dataContext.Items on ibs.ItemId equals i.ItemId
                                       //                                       join b in dataContext.Bookings on ib.ProjectId equals b.RelatedId
                                       join cbn in dataContext.CompanyBookingNumbers on b.BookingId equals cbn.BookingId
                                       //join p in dataContext.Projects on ib.ProjectId equals p.ProjectId
                                       from p in dataContext.Projects.Where(p => p.ProjectId == b.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                       from np in dataContext.NonProjectBookings.Where(npb => npb.NonProjectBookingId == b.RelatedId && b.RelatedTable == "NonProject").DefaultIfEmpty().Take(1)
                                       where eh == null && ibs.IsActive && i.CompanyId.HasValue && i.CompanyId.Value == cbn.CompanyId
                                       group new { Booking = b, CompayBookingNumber = cbn, Project = p, NonProject = np, ItemBooking = ibs } by cbn.CompanyId into grp
                                       select new
                                       {
                                           BookingInfo = grp.GroupBy(g => g.Booking.BookingId).
                                                            Select(g => new
                                                            {
                                                                Booking = g.FirstOrDefault().Booking,
                                                                CompayBookingNumber = g.FirstOrDefault().CompayBookingNumber,
                                                                BookingName = g.FirstOrDefault().Project != null ? g.FirstOrDefault().Project.ProjectName : g.FirstOrDefault().NonProject.Name,
                                                                ItemBookingIds = g.Select(gx => gx.ItemBooking.ItemBookingId),
                                                                CompanyId = grp.Key
                                                            }),
                                           CompanyId = grp.Key
                                       }).ToList();

                string userWebUrl = Utils.GetSystemValue("SBUserWebURL");

                Dictionary<User, List<dynamic>> userBookings = new Dictionary<User, List<dynamic>>();

                foreach (var companyBookings in overdueBookings)
                {
                    foreach (var bookingInfo in companyBookings.BookingInfo)
                    {
                        foreach (var itemBookingId in bookingInfo.ItemBookingIds)
                        {
                            ItemBooking itemBooking = inventoryBL.GetItemBooking(itemBookingId);
                            if (itemBooking != null)
                            {
                                User locationManager = inventoryBL.GetContactBookingManager(itemBooking.Item.CompanyId.Value, itemBooking.Item.LocationId);
                                if (locationManager != null)
                                {
                                    if (userBookings.Keys.Where(ul => ul.UserId == locationManager.UserId).FirstOrDefault() == null)
                                    {
                                        userBookings[locationManager] = new List<dynamic>();
                                    }

                                    User key = userBookings.Keys.Where(ul => ul.UserId == locationManager.UserId).FirstOrDefault();
                                    if (userBookings[key].Where(bi => bi.Booking.BookingId == bookingInfo.Booking.BookingId).FirstOrDefault() == null)
                                    {
                                        userBookings[key].Add(bookingInfo);
                                    }

                                    Data.EmailHistory emailHistory = new EmailHistory
                                    {
                                        EmailTemplateTypeCodeId = overdueEmailCodeId,
                                        RelatedId = itemBookingId,
                                        RelatedTable = "ItemBooking",
                                        CreatedDate = Utils.Now
                                    };

                                    dataContext.EmailHistories.AddObject(emailHistory);
                                }
                            }
                        }

                        dataContext.SaveChanges();
                    }
                }

                foreach (User user in userBookings.Keys)
                {
                    StringBuilder bookingLinks = new StringBuilder();
                    List<dynamic> bookingInfos = userBookings[user];
                    foreach (dynamic bookingInfo in bookingInfos)
                    {
                        if (bookingInfo != null)
                        {
                            string url = string.Format("{0}/Inventory/BookingDetails.aspx?BookingId={1}&CompanyId={2}", userWebUrl, bookingInfo.Booking.BookingId, bookingInfo.CompanyId);
                            string link = string.Format("Booking name: <a href='{0}' target='_blank'>{1}</a> - Booking ID: {2} <br />", url, bookingInfo.BookingName, bookingInfo.CompayBookingNumber.BookingNumber);
                            bookingLinks.Append(link);
                        }
                    }

                    EmailSender.SendBookingOverdueEmail(user.Email1, user.FirstName, bookingLinks.ToString());
                }
            }
        }

        /// <summary>
        /// Sends the booking delayed emails.
        /// </summary>
        /// <param name="dateToConsider">The date to consider.</param>
        public static void SendBookingDelayedEmails(DateTime dateToConsider)
        {
            using (StageBitzDB dataContext = new StageBitzDB())
            {
                InventoryBL inventoryBL = new InventoryBL(dataContext);
                int delayedEmailCodeId = Utils.GetCodeIdByCodeValue("EmailTemplateTypeCode", "BOOKINGDELAYED");
                string userWebUrl = Utils.GetSystemValue("SBUserWebURL");

                var delayedBookings = (from ibs in dataContext.ItemBookings.Where(ibs => dataContext.IsItemBookingDelayedByDate(ibs.ItemBookingId, dateToConsider))
                                       from eh in dataContext.EmailHistories.Where(eh => ibs.ItemBookingId == eh.RelatedId
                                             && eh.RelatedTable == "ItemBooking" && eh.EmailTemplateTypeCodeId == delayedEmailCodeId).DefaultIfEmpty().Take(1)
                                       join b in dataContext.Bookings on ibs.BookingId equals b.BookingId
                                       from ib in dataContext.ItemBriefs.Where(ib => ib.ItemBriefId == ibs.RelatedId && b.RelatedTable == "Project").DefaultIfEmpty().Take(1)
                                       from npb in dataContext.NonProjectBookings.Where(npb => npb.NonProjectBookingId == b.RelatedId && b.RelatedTable == "NonProject").DefaultIfEmpty().Take(1)
                                       join i in dataContext.Items on ibs.ItemId equals i.ItemId
                                       join u in dataContext.Users on ibs.CreatedBy equals u.UserId
                                       where eh == null && ibs.IsActive && i.CompanyId.HasValue
                                       select new
                                       {
                                           ItemBrief = ib,
                                           NonProjectBooking = npb,
                                           CompanyId = i.CompanyId.Value,
                                           CreatedBy = u,
                                           ItemBooking = ibs,
                                           Item = i
                                       }).ToList();

                foreach (var delayedBooking in delayedBookings)
                {
                    User locationManager = inventoryBL.GetContactBookingManager(delayedBooking.Item.CompanyId.Value, delayedBooking.Item.LocationId);
                    if (locationManager != null)
                    {
                        Data.EmailHistory emailHistory = new EmailHistory
                        {
                            EmailTemplateTypeCodeId = delayedEmailCodeId,
                            RelatedId = delayedBooking.ItemBooking.ItemBookingId,
                            RelatedTable = "ItemBooking",
                            CreatedDate = Utils.Now
                        };

                        dataContext.EmailHistories.AddObject(emailHistory);
                        dataContext.SaveChanges();
                        string inventoryManagerName = string.Concat(locationManager.FirstName, " ", locationManager.LastName);

                        if (delayedBooking.ItemBrief != null)
                        {
                            string url = string.Format("{0}/ItemBrief/ItemBriefDetails.aspx?ItemBriefId={1}&TabId=3", userWebUrl, delayedBooking.ItemBrief.ItemBriefId);
                            EmailSender.SendBookingDelayedEmail(delayedBooking.CreatedBy.Email1, delayedBooking.CreatedBy.FirstName, delayedBooking.ItemBrief.Name,
                           url, inventoryManagerName, locationManager.Email1);
                        }
                        else
                        {
                            EmailSender.SendBookingDelayedEmailForNonProject(delayedBooking.CreatedBy.Email1, delayedBooking.CreatedBy.FirstName, delayedBooking.NonProjectBooking.Name,
                                inventoryManagerName, locationManager.Email1);
                        }
                    }
                }
            }
        }
    }
}