using System;
using System.Linq;
using StageBitz.AdminWeb.Common.Helpers;
using System.Configuration;
using StageBitz.Data;
using StageBitz.AdminWeb.Controls.Common;
using StageBitz.Common;
using System.Diagnostics;
using System.Web.UI;
using System.Net;

namespace StageBitz.AdminWeb.System
{
    public partial class SystemSettings : PageBase
    {
        #region Properties

        public string SystemDateConfig
        {
            get
            {
                return ConfigurationManager.AppSettings["Debug.SystemDate"];
            }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ShowHideFeatures();

                LoadAgentLastRunDates();

                LoadBreadCrumbs();
            }
        }

        protected void CheckForUpdate(object sender, EventArgs e)
        {
            LoadAgentLastRunDates();
        }

        private void LoadAgentExecutionStatus()
        {
            SystemValue systemValue = DataContext.SystemValues.Where(sv=>sv.Name == "IsAgentRunning").FirstOrDefault();
            if (systemValue != null)
            {
                litStatus.Visible = string.Equals(systemValue.Value, "true");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SaveSystemDate();

            LoadAgentLastRunDates();
        }

        protected void btnResetProcesses_Click(object sender, EventArgs e)
        {
            ResetAgentLastRunDates();
        }

        protected void btnClearFinanceData_Click(object sender, EventArgs e)
        {
            ltrlNotification.Text = "Cleanup Successful.";

            try
            {
                DataContext.ClearFinancialData("ClearFinancialData", Now);
            }
            catch (Exception ex)
            {
                ltrlNotification.Text = "Error: " + ((ex.InnerException == null) ? ex.Message : ex.InnerException.Message);
            }

            popupNotification.ShowPopup();
            upnlNotification.Update();
        }

        protected void btnClearRefDataCache_Click(object sender, EventArgs e)
        {
            ltrlNotification.Text = "Cache Cleared.";

            try
            {
                ClearSystemCache();
            }
            catch (Exception ex)
            {
                ltrlNotification.Text = "Error: " + ((ex.InnerException == null) ? ex.Message : ex.InnerException.Message);
            }

            popupNotification.ShowPopup();
            upnlNotification.Update();
        }

        protected void RefreshClick(object sender, EventArgs e)
        {
            LoadAgentLastRunDates();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Show/hide UI features depending on the configuration.
        /// </summary>
        private void ShowHideFeatures()
        {
            if (Utils.IsDebugMode && SystemDateConfig == "systemvalue")
            {
                plcNoSettings.Visible = false;
                plcSystemDate.Visible = true;

                LoadSystemDate();
            }
            else
            {
                plcNoSettings.Visible = true;
                plcSystemDate.Visible = false;
            }

            btnResetProcesses.Visible = Utils.IsDebugMode;
            trClearFinanceData.Visible = Utils.IsDebugMode;
        }

        /// <summary>
        /// Clears ref-data cache for both Admin portal and user web.
        /// </summary>
        private void ClearSystemCache()
        {
            //Clear cache for the current application
            SystemCache.ClearCache();

            //Clear cache for user web
            string url = string.Format("{0}/Public/System.aspx?clearcache=1", Utils.GetSystemValue("SBUserWebURL"));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.GetResponse();
        }

        private void LoadSystemDate()
        {
            string systemDateString = DataContext.SystemValues.Where(sv => sv.Name == "SystemDate").FirstOrDefault().Value;

            DateTime configDate;
            bool success = DateTime.TryParse(systemDateString, out configDate);
            if (success)
            {
                dtpkSystemDate.SelectedDate = configDate;
            }
            else
            {
                dtpkSystemDate.SelectedDate = null;
            }
        }

        protected void SaveSystemDate()
        {
            string oldSystemDateString = DataContext.SystemValues.Where(sv => sv.Name == "SystemDate").FirstOrDefault().Value;

            DateTime newDate = dtpkSystemDate.SelectedDate == null ? DateTime.Now : dtpkSystemDate.SelectedDate.Value;

            DateTime oldConfigDate = newDate;
            DateTime.TryParse(oldSystemDateString, out oldConfigDate);

            string newSystemDateString = dtpkSystemDate.SelectedDate == null ? string.Empty : dtpkSystemDate.SelectedDate.Value.ToString("dd-MMM-yyyy HH:mm:ss");

            SystemValue sysValue = DataContext.SystemValues.Where(sv => sv.Name == "SystemDate").FirstOrDefault();
            sysValue.Value = newSystemDateString;

            DataContext.SaveChanges();

            ClearSystemCache();

            ShowNotification("saveNotice");

            if (newDate < oldConfigDate)
            {
                //If the date has moved into past, reset the agent last run date as well.
                ResetAgentLastRunDates();
            }
        }

        private void LoadAgentLastRunDates()
        {
            int dailyAgentCodeId = Utils.GetCodeIdByCodeValue("SystemTaskType", "DAILY");
            int monthlyAgentCodeId = Utils.GetCodeIdByCodeValue("SystemTaskType", "MONTHLY");

            DateTime? dailyLastRunDate = DataContext.SystemTasks.Where(st => st.TaskTypeCodeId == dailyAgentCodeId).FirstOrDefault().LastRunDate;
            DateTime? monthlyLastRunDate = DataContext.SystemTasks.Where(st => st.TaskTypeCodeId == monthlyAgentCodeId).FirstOrDefault().LastRunDate;

            ltrlDaily.Text = (dailyLastRunDate == null) ? string.Empty : Support.FormatDatetime(dailyLastRunDate.Value, true);
            ltrlMonthly.Text = (monthlyLastRunDate == null) ? string.Empty : Support.FormatDatetime(monthlyLastRunDate.Value, true);
            LoadAgentExecutionStatus();
            upnlLastRunDates.Update();
        }

        private void ResetAgentLastRunDates()
        {
            DateTime today = Today;

            int dailyAgentCodeId = Utils.GetCodeIdByCodeValue("SystemTaskType", "DAILY");
            int monthlyAgentCodeId = Utils.GetCodeIdByCodeValue("SystemTaskType", "MONTHLY");

            SystemTask dailyTask = DataContext.SystemTasks.Where(st => st.TaskTypeCodeId == dailyAgentCodeId).FirstOrDefault();
            SystemTask monthlyTask = DataContext.SystemTasks.Where(st => st.TaskTypeCodeId == monthlyAgentCodeId).FirstOrDefault();

            dailyTask.LastRunDate = today.AddDays(-1).AddHours(23);

            int monthlyTaskDayToRun = int.Parse(Utils.GetSystemValue("MonthlyFinanceProcessDay"));

            if (today.Day <= monthlyTaskDayToRun)
            {
                monthlyTask.LastRunDate = new DateTime(today.Year, Today.Month, monthlyTaskDayToRun).AddMonths(-1).AddHours(23);
            }
            else
            {
                monthlyTask.LastRunDate = new DateTime(today.Year, Today.Month, monthlyTaskDayToRun).AddHours(23);
            }

            DataContext.SaveChanges();

            LoadAgentLastRunDates();
        }

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadcrumbs = GetBreadCrumbsControl();
            breadcrumbs.AddLink(DisplayTitle, null);

            breadcrumbs.LoadControl();
        }

        #endregion
    }
}