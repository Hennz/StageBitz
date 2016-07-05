using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Logic.Business.Notification;
using StageBitz.Logic.Business.Utility;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Web.UI;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// ItemChanged delegate.
    /// </summary>
    public delegate void ItemChanged();

    /// <summary>
    /// User control for attach hyperlinks. (all attachement tabs).
    /// </summary>
    public partial class AttachHyperlink : UserControlBase
    {
        #region events

        /// <summary>
        /// Occurs when item changed.
        /// </summary>
        public event ItemChanged ItemChanged;

        #endregion events

        #region Enums

        /// <summary>
        ///
        /// </summary>
        public enum LinkDisplayMode
        {
            Add,
            Edit,
            Delete
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the document media identifier.
        /// </summary>
        /// <value>
        /// The document media identifier.
        /// </value>
        public int DocumentMediaId
        {
            get
            {
                if (ViewState["DocumentMediaId"] == null)
                {
                    ViewState["DocumentMediaId"] = 0;
                }

                return (int)ViewState["DocumentMediaId"];
            }
            set
            {
                ViewState["DocumentMediaId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId
        {
            get
            {
                if (ViewState["ProjectId"] == null)
                {
                    ViewState["ProjectId"] = 0;
                }

                return (int)ViewState["ProjectId"];
            }
            set
            {
                ViewState["ProjectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId
        {
            get
            {
                if (ViewState["RelatedId"] == null)
                {
                    return 0;
                }
                else
                {
                    return (int)ViewState["RelatedId"];
                }
            }
            set
            {
                ViewState["RelatedId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the related table.
        /// </summary>
        /// <value>
        /// The name of the related table.
        /// </value>
        public string RelatedTableName
        {
            get
            {
                if (ViewState["RelatedTableName"] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)ViewState["RelatedTableName"];
                }
            }
            set
            {
                ViewState["RelatedTableName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["IsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsReadOnly"];
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public LinkDisplayMode DisplayMode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(LinkDisplayMode);
                }

                return (LinkDisplayMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveConfirm_Click(object sender, EventArgs e)
        {
            //logic to remove link
            UtilityBL utilityBL = new UtilityBL(DataContext);
            DocumentMedia media = utilityBL.RemoveHyperlinkDocumentMedia(DocumentMediaId);
            if (media != null && (RelatedTableName == "Project" || RelatedTableName == "ItemBrief"))
            {
                this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Delete);
            }

            UpdateParent();
            popupAttachLinks.HidePopup();
            popupLinkRemoveConfirmation.HidePopup();
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnOK_Click(object sender, EventArgs e)//If you are generting notifications, you should genetate them based on RelatedTableName
        {
            if (Page.IsValid)
            {
                string name = txtName.Text;
                UriBuilder uriBuilder = new UriBuilder(txtHyperlink.Text);
                string url = new UriBuilder(txtHyperlink.Text).Uri.AbsoluteUri;
                //if the uploaded url is an url to a file we change the url format to UNC
                if (uriBuilder.Uri.Scheme.Equals("file"))
                {
                    url = url.Insert(5, "///");
                }
                UtilityBL utilityBL = new UtilityBL(DataContext);

                if (DisplayMode == LinkDisplayMode.Add)
                {
                    int createdBy = UserID;
                    DocumentMedia media = utilityBL.InsertHyperlinkToDocumentMedia(name, RelatedTableName, RelatedId, url, createdBy, Utils.Today);
                    if (media != null && (RelatedTableName == "Project" || RelatedTableName == "ItemBrief"))
                    {
                        this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Add);
                    }
                    UpdateParent();
                }
                else
                {
                    int lastUpdatedBy = UserID;
                    DateTime lastUpdatedDate = DateTime.Now;
                    DocumentMedia media = utilityBL.GetDocumentMedia(DocumentMediaId);
                    if (media == null)
                    {
                        linkAlreadyDeleted.ShowPopup();
                    }
                    else
                    {
                        if ((media.Name != name || media.Description != url) && (RelatedTableName == "Project" || RelatedTableName == "ItemBrief"))
                        {
                            this.GetBL<NotificationBL>().GenerateNotificationsForMedia(media, UserID, ProjectId, NotificationBL.OperationMode.Edit, name, url);
                        }
                        utilityBL.UpdateHyperlinkDocumentMedia(media, name, url, lastUpdatedBy, lastUpdatedDate);
                        UpdateParent();
                    }
                }

                popupAttachLinks.HidePopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the imgbtnRemoveLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs"/> instance containing the event data.</param>
        protected void imgbtnRemoveLink_Click(object sender, ImageClickEventArgs e)
        {
            UtilityBL utilityBL = new UtilityBL(DataContext);
            bool isDocumentMediaExist = utilityBL.IsDocumentMediaExist(DocumentMediaId);
            if (!isDocumentMediaExist)
            {
                linkAlreadyDeleted.ShowPopup();
            }
            else
            {
                popupLinkRemoveConfirmation.ShowPopup();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConifrmAlreadyRemoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConifrmAlreadyRemoved_Click(object sender, EventArgs e)
        {
            if (ItemChanged != null)
            {
                ItemChanged();
            }
            popupAttachLinks.HidePopup();
            linkAlreadyDeleted.HidePopup();
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Updates the parent.
        /// </summary>
        private void UpdateParent()
        {
            if (ItemChanged != null)
            {
                ItemChanged();
            }
        }

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        public void InitializeUI()
        {
            imgbtnRemoveLink.ImageUrl = string.Format("~/Common/Images/Remove.png?v={0}", ApplicationVersionString);
        }

        /// <summary>
        /// Shows the popup.
        /// </summary>
        public void ShowPopup()
        {
            LoadUI();
            popupAttachLinks.ShowPopup();
        }

        /// <summary>
        /// Loads the UI.
        /// </summary>
        private void LoadUI()
        {
            InitializeUI();
            if (this.DisplayMode == LinkDisplayMode.Add)
            {
                popupAttachLinks.Title = "Add new Hyperlink";
                btnOK.Text = "Ok";
                imgbtnRemoveLink.Visible = false;
                txtHyperlink.Text = "";
                txtName.Text = "";
                cancelButton.Visible = true;
            }
            else
            {
                cancelButton.Visible = false;
                btnOK.Text = "Done";
                popupAttachLinks.Title = "Edit/ Delete Hyperlink";
                imgbtnRemoveLink.Visible = true;
                UtilityBL utilityBL = new UtilityBL(DataContext);
                DocumentMedia media = utilityBL.GetDocumentMedia(DocumentMediaId);
                if (media != null)
                {
                    txtHyperlink.Text = media.Description;
                    txtName.Text = media.Name;
                }
                else
                {
                    linkAlreadyDeleted.ShowPopup();
                }
            }
        }

        #endregion Private Methods
    }
}