using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User class for listview display settings.
    /// </summary>
    public partial class ListViewDisplaySettings : UserControlBase
    {
        /// <summary>
        /// Occurs when [display mode changed].
        /// </summary>
        public event EventHandler DisplayModeChanged;

        #region Enums

        /// <summary>
        /// Enum for view setting module.
        /// </summary>
        public enum ViewSettingModule
        {
            UserDashboardProjectList,
            CompanyDashBoardProjectList,
            ProjectItemBriefList,
            MyContacts
        }

        /// <summary>
        /// Enum for viewsetting value
        /// </summary>
        public enum ViewSettingValue
        {
            ThumbnailView, //Default
            ListView
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public ViewSettingModule Module
        {
            get
            {
                if (ViewState["Module"] == null)
                {
                    ViewState["Module"] = default(ViewSettingModule);
                }

                return (ViewSettingModule)ViewState["Module"];
            }
            set
            {
                ViewState["Module"] = value;
            }
        }

        /// <summary>
        /// Gets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public ViewSettingValue DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(ViewSettingValue);
                }

                return (ViewSettingValue)ViewState["DisplayMode"];
            }
            private set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the imgbtnViewMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs"/> instance containing the event data.</param>
        protected void imgbtnViewMode_Click(object sender, ImageClickEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                if (sender == imgbtnThumbView)
                {
                    UpdateUIForMode(ViewSettingValue.ThumbnailView);
                }
                else if (sender == imgbtnListView)
                {
                    UpdateUIForMode(ViewSettingValue.ListView);
                }

                SaveCurrentSetting();

                if (DisplayModeChanged != null)
                {
                    DisplayModeChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Loads the control.
        /// </summary>
        public void LoadControl()
        {
            int currentModuleCodeId = GetModuleCodeId(Module);

            ViewSetting setting = (from vs in DataContext.ViewSettings
                                   where vs.UserId == UserID && vs.ViewSettingModuleCodeId == currentModuleCodeId
                                   select vs).FirstOrDefault();

            if (setting == null)
            {
                UpdateUIForMode(default(ViewSettingValue));
            }
            else
            {
                Code settingMode = Support.GetCodeByCodeId(setting.ViewSettingValueCodeId);

                if (settingMode.Value == "THUMBNAILVIEW")
                {
                    UpdateUIForMode(ViewSettingValue.ThumbnailView);
                }
                else if (settingMode.Value == "LISTVIEW")
                {
                    UpdateUIForMode(ViewSettingValue.ListView);
                }
            }

            if (DisplayModeChanged != null)
            {
                DisplayModeChanged(this, EventArgs.Empty);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Updates the UI for mode.
        /// </summary>
        /// <param name="viewMode">The view mode.</param>
        private void UpdateUIForMode(ViewSettingValue viewMode)
        {
            DisplayMode = viewMode;

            if (viewMode == ViewSettingValue.ThumbnailView)
            {
                imgbtnThumbView.CssClass = "viewModeIcon viewModeIconSelected";
                imgbtnListView.CssClass = "viewModeIcon";

                imgbtnThumbView.Enabled = false;
                imgbtnListView.Enabled = true;
            }
            if (viewMode == ViewSettingValue.ListView)
            {
                imgbtnThumbView.CssClass = "viewModeIcon";
                imgbtnListView.CssClass = "viewModeIcon viewModeIconSelected";

                imgbtnThumbView.Enabled = true;
                imgbtnListView.Enabled = false;
            }
        }

        /// <summary>
        /// Saves the currently selected display mode to the database.
        /// </summary>
        private void SaveCurrentSetting()
        {
            int currentModuleCodeId = GetModuleCodeId(Module);

            ViewSetting setting = (from vs in DataContext.ViewSettings
                                   where vs.UserId == UserID && vs.ViewSettingModuleCodeId == currentModuleCodeId
                                   select vs).FirstOrDefault();

            //If there is no existing setting, create a new one
            if (setting == null)
            {
                setting = new ViewSetting();
                setting.UserId = UserID;
                setting.ViewSettingModuleCodeId = currentModuleCodeId;
                setting.CreatedByUserId = UserID;
                setting.CreatedDate = Now;

                DataContext.ViewSettings.AddObject(setting);
            }

            setting.ViewSettingValueCodeId = GetViewModeCodeId(DisplayMode);
            setting.LastUpdatedByUserId = UserID;
            setting.LastUpdatedDate = Now;

            DataContext.SaveChanges();
        }

        /// <summary>
        /// Gets the module code identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        private int GetModuleCodeId(ViewSettingModule module)
        {
            switch (module)
            {
                case ViewSettingModule.UserDashboardProjectList:
                    return Support.GetCodeIdByCodeValue("ViewSettingModuleCode", "USERPROJLIST");

                case ViewSettingModule.CompanyDashBoardProjectList:
                    return Support.GetCodeIdByCodeValue("ViewSettingModuleCode", "COMPANYPROJLIST");

                case ViewSettingModule.ProjectItemBriefList:
                    return Support.GetCodeIdByCodeValue("ViewSettingModuleCode", "PROJITEMBRIEFLIST");

                case ViewSettingModule.MyContacts:
                    return Support.GetCodeIdByCodeValue("ViewSettingModuleCode", "MYCONTACTSLIST");

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the view mode code identifier.
        /// </summary>
        /// <param name="viewMode">The view mode.</param>
        /// <returns></returns>
        private int GetViewModeCodeId(ViewSettingValue viewMode)
        {
            switch (viewMode)
            {
                case ViewSettingValue.ThumbnailView:
                    return Support.GetCodeIdByCodeValue("ViewSettingValueCode", "THUMBNAILVIEW");

                case ViewSettingValue.ListView:
                    return Support.GetCodeIdByCodeValue("ViewSettingValueCode", "LISTVIEW");

                default:
                    return 0;
            }
        }

        #endregion Private Methods
    }
}