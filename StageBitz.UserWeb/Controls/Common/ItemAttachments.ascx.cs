using StageBitz.Common;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Inventory;
using StageBitz.Logic.Business.ItemBrief;
using StageBitz.Logic.Business.Project;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace StageBitz.UserWeb.Controls.Common
{
    #region Delegates

    public delegate void InformParentToUpdateAboutAttachmentChanges();

    public delegate void InformParentToUpdateInItem();

    public delegate void UpdateAttachmentsCount(int count);

    #endregion Delegates

    /// <summary>
    /// Usercontrol for item attachments.
    /// </summary>
    public partial class ItemAttachments : UserControlBase
    {
        private int ItemBriefTypePrimaryCodeId = Support.GetCodeIdByCodeValue("ItemBriefType", "PRIMARY");

        #region Events

        /// <summary>
        /// The inform parent to update about attachment changes
        /// </summary>
        public InformParentToUpdateAboutAttachmentChanges InformParentToUpdateAboutAttachmentChanges;

        /// <summary>
        /// The inform parent to update in item
        /// </summary>
        public InformParentToUpdateInItem InformParentToUpdateInItem;

        /// <summary>
        /// The inform parent to update attachments count
        /// </summary>
        public UpdateAttachmentsCount UpdateAttachmentsCount;

        #endregion Events

        #region Enums

        /// <summary>
        /// Enum for Link Display Mode
        /// </summary>
        public enum LinkDisplayMode
        {
            Add,
            Edit,
            Delete
        }

        /// <summary>
        /// Enum for control display mode.
        /// </summary>
        public enum DisplayMode
        {
            ItemBrief,
            Project
        }

        #endregion Enums

        #region Properties

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
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether item allready exist.
        /// </summary>
        /// <value>
        /// <c>true</c> if item allready exist; otherwise, <c>false</c>.
        /// </value>
        private bool IsItemAllreadyExist
        {
            get
            {
                if (ViewState["IsItemAllreadyExist"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["IsItemAllreadyExist"];
                }
            }
            set
            {
                ViewState["IsItemAllreadyExist"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the loaded image count.
        /// </summary>
        /// <value>
        /// The loaded image count.
        /// </value>
        public int LoadedImageCount
        {
            get
            {
                if (ViewState["LoadedImageCount"] == null)
                {
                    return 0;
                }

                return (int)ViewState["LoadedImageCount"];
            }
            set
            {
                ViewState["LoadedImageCount"] = value;
            }
        }

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
                    ViewState["ItemTypeId"] = 0;
                }

                return (int)ViewState["ItemTypeId"];
            }
            set
            {
                ViewState["ItemTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public DisplayMode Mode
        {
            get
            {
                if (ViewState["DisplayMode"] == null)
                {
                    ViewState["DisplayMode"] = default(DisplayMode);
                }

                return (DisplayMode)ViewState["DisplayMode"];
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion Properties

        #region Events Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Setup File upload control
                switch (Mode)
                {
                    case DisplayMode.ItemBrief:
                        fileUpload.RelatedTableName = "ItemBrief";
                        break;

                    case DisplayMode.Project:
                        fileUpload.RelatedTableName = "Project";
                        break;
                }
                fileUpload.ProjectId = ProjectId;
                fileUpload.RelatedId = RelatedId;
                fileUpload.UploadMediaType = UserWeb.Controls.Common.FileUpload.MediaType.AllFiles;
                fileUpload.IsReadOnly = IsReadOnly;
                fileUpload.LoadUI();
                documentPreviewEditable.IsReadOnly = IsReadOnly;
                documentPreviewEditable.ProjectId = ProjectId;
                btnAddLink.Enabled = !IsReadOnly;
                LoadData();
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fileUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fileUpload_FileUploaded(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadData();

                if (InformParentToUpdateAboutAttachmentChanges != null)
                {
                    InformParentToUpdateAboutAttachmentChanges();
                }

                if (InformParentToUpdateInItem != null)
                {
                    InformParentToUpdateInItem();
                }

                upnlDocumentList.Update();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvAttachments_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic media = e.Item.DataItem as dynamic;

                if (media.FileExtension == "HYPERLINK")//If it is a hyperlink,load hyperlink specific content.
                {
                    HtmlGenericControl divDocAttachments = (HtmlGenericControl)e.Item.FindControl("divDocAttachments");
                    divDocAttachments.Visible = false;
                    HtmlGenericControl divLinks = (HtmlGenericControl)e.Item.FindControl("divLinks");
                    divLinks.Visible = true;
                    HtmlAnchor link = (HtmlAnchor)e.Item.FindControl("lblLinkURL");
                    LinkButton hyperlinkButton = (LinkButton)e.Item.FindControl("HyperLinkLinkButton");

                    hyperlinkButton.Enabled = !IsReadOnly;

                    hyperlinkButton.CommandArgument = media.DocumentMediaId.ToString();
                    string labelTxt = string.Empty;
                    if (media.Name != null && media.Name != string.Empty)
                    {
                        labelTxt = media.Name;
                        link.HRef = media.Description;
                    }
                    else
                    {
                        labelTxt = media.Description;
                        link.HRef = media.Description;
                    }

                    Label lblType = (Label)e.Item.FindControl("lblType");
                    Label lblLinkedDate = (Label)e.Item.FindControl("lblLinkedDate");
                    Label lblLinkedBy = (Label)e.Item.FindControl("lblLinkedBy");
                    HtmlGenericControl divDescription = (HtmlGenericControl)e.Item.FindControl("divDescription");
                    HtmlGenericControl divLinkBlock = (HtmlGenericControl)e.Item.FindControl("divLinkBlock");
                    HtmlGenericControl extraLinkDetails = (HtmlGenericControl)e.Item.FindControl("extraLinkDetails");

                    switch (Mode)  //Display and hide extra fields based on the mode
                    {
                        case DisplayMode.ItemBrief:
                            link.InnerText = Support.TruncateString(labelTxt, 60);
                            link.Title = labelTxt;
                            extraLinkDetails.Visible = false;
                            divDescription.Visible = true;
                            break;

                        case DisplayMode.Project:
                            extraLinkDetails.Visible = true;
                            link.InnerText = Support.TruncateString(labelTxt, 30);
                            link.Title = labelTxt;
                            lblLinkedDate.Text = string.Concat("Uploaded: ", Utils.FormatDate(media.LastUpdatedDate));
                            lblLinkedBy.Text = Support.TruncateString(string.Concat("By: ", media.LastUpdatedBy), 35);
                            divDescription.Visible = false;
                            divLinks.Style.Add("Width", "400px");
                            divLinkBlock.Style.Add("Width", "200px");
                            break;
                    }
                }
                else
                {
                    ImageDisplay thumbAttachment = (ImageDisplay)e.Item.FindControl("thumbAttachment");
                    thumbAttachment.ImageTitle = media.Name;
                    thumbAttachment.DocumentMediaId = media.DocumentMediaId;
                    thumbAttachment.IsThumbnail = true;
                    LinkButton lnkbtnAttachment = (LinkButton)e.Item.FindControl("lnkbtnAttachment");
                    lnkbtnAttachment.Enabled = false;
                    lnkbtnAttachment.CommandArgument = media.DocumentMediaId.ToString();

                    Label lblAttachmentName = (Label)e.Item.FindControl("lblAttachmentName");

                    Literal litAttachmentType = (Literal)e.Item.FindControl("litAttachmentType");
                    litAttachmentType.Text = media.FileExtension;

                    Label lblUploadedDate = (Label)e.Item.FindControl("lblUploadedDate");
                    Label lblUploadedBy = (Label)e.Item.FindControl("lblUploadedBy");
                    HtmlGenericControl divAttachmentBlock = (HtmlGenericControl)e.Item.FindControl("divAttachmentBlock");
                    HtmlGenericControl divDocAttachments = (HtmlGenericControl)e.Item.FindControl("divDocAttachments");
                    HtmlGenericControl extraDetails = (HtmlGenericControl)e.Item.FindControl("extraDetails");
                    switch (Mode)
                    {
                        case DisplayMode.ItemBrief:
                            lblAttachmentName.Text = Support.TruncateString(media.Name, 50);
                            if (!string.IsNullOrEmpty(media.Name) && media.Name.Length > 50)
                                lblAttachmentName.ToolTip = media.Name;
                            CheckBox chkInclude = (CheckBox)e.Item.FindControl("chkInclude");
                            chkInclude.Enabled = !IsReadOnly && this.GetBL<InventoryBL>().CanEditIteminItemBrief(RelatedId);
                            if (media.SourceTable == "Item")
                            {
                                chkInclude.Enabled = false;
                                chkInclude.ToolTip = "These attachments are connected to this Item in the Inventory so they cannot be changed or removed";
                            }

                            chkInclude.Visible = IsItemAllreadyExist;
                            chkInclude.Checked = media.ItemBriefItemDocumentMediaId > 0;
                            thumbAttachment.FunctionPrefix = (media.SourceTable == "Item") ? "ReadOnlyModeDP" : "EditModeModeDP";
                            extraDetails.Visible = false;
                            break;

                        case DisplayMode.Project:
                            lblAttachmentName.Text = Support.TruncateString(media.Name, 35);
                            if (!string.IsNullOrEmpty(media.Name) && media.Name.Length > 35)
                                lblAttachmentName.ToolTip = media.Name;
                            divDocAttachments.Style.Add("Width", "400px");
                            divAttachmentBlock.Style.Add("Width", "200px");
                            HtmlGenericControl divCheckInclude = (HtmlGenericControl)e.Item.FindControl("divCheckInclude");
                            divCheckInclude.Visible = false;
                            extraDetails.Visible = true;
                            lblUploadedDate.Text = string.Concat("Uploaded: ", Utils.FormatDate(media.LastUpdatedDate));
                            lblUploadedBy.Text = Support.TruncateString(string.Concat("By: ", media.LastUpdatedBy), 35);
                            thumbAttachment.FunctionPrefix = "EditModeModeDP";
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DocumentChanged event of the documentPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void documentPreview_DocumentChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadData();
                if (InformParentToUpdateAboutAttachmentChanges != null)
                {
                    InformParentToUpdateAboutAttachmentChanges();
                }
            }
        }

        /// <summary>
        /// Handles the DocumentDeleted event of the documentPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void documentPreview_DocumentDeleted(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadData();
                if (InformParentToUpdateAboutAttachmentChanges != null)
                {
                    InformParentToUpdateAboutAttachmentChanges();
                }
            }
        }

        /// <summary>
        /// Handles the OnCheckedChanged event of the chkInclude control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void chkInclude_OnCheckedChanged(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                CheckBox cb = (CheckBox)sender;
                ListViewItem lvitem = (ListViewItem)cb.NamingContainer;
                ListViewDataItem dataItem = (ListViewDataItem)lvitem;
                DataKey currentDataKey = this.lvAttachments.DataKeys[dataItem.DataItemIndex];
                int documentMediaId = (int)currentDataKey["DocumentMediaId"];
                int itemBriefItemDocumentMediaId = (int)currentDataKey["ItemBriefItemDocumentMediaId"];

                this.GetBL<ItemBriefBL>().ShareAttachmentWithItemBrief(RelatedId, documentMediaId, itemBriefItemDocumentMediaId, UserID, cb.Checked);

                if (InformParentToUpdateInItem != null)
                {
                    InformParentToUpdateInItem();
                }

                LoadData();
                cb.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddLink_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case DisplayMode.ItemBrief:
                    newHyperLink.RelatedTableName = "ItemBrief";
                    break;

                case DisplayMode.Project:
                    newHyperLink.RelatedTableName = "Project";
                    break;
            }
            newHyperLink.ProjectId = ProjectId;
            newHyperLink.IsReadOnly = IsReadOnly;
            newHyperLink.IsReadOnly = IsReadOnly;
            newHyperLink.RelatedId = RelatedId;
            newHyperLink.DisplayMode = AttachHyperlink.LinkDisplayMode.Add;
            newHyperLink.ShowPopup();
        }

        /// <summary>
        /// News the hyper link_ item changed.
        /// </summary>
        protected void newHyperLink_ItemChanged()
        {
            if (!PageBase.StopProcessing)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Handles the Click event of the HyperLinkLinkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HyperLinkLinkButton_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case DisplayMode.ItemBrief:
                    newHyperLink.RelatedTableName = "ItemBrief";
                    break;

                case DisplayMode.Project:
                    newHyperLink.RelatedTableName = "Project";
                    break;
            }

            LinkButton btn = (LinkButton)sender;
            int documentMediaId = int.Parse(btn.CommandArgument);
            newHyperLink.IsReadOnly = IsReadOnly;
            newHyperLink.RelatedId = RelatedId;
            newHyperLink.ProjectId = ProjectId;
            newHyperLink.DocumentMediaId = documentMediaId;
            newHyperLink.DisplayMode = AttachHyperlink.LinkDisplayMode.Edit;
            newHyperLink.ShowPopup();
        }

        #endregion Events Handlers

        #region Public Methods

        /// <summary>
        /// Loads the data.
        /// </summary>
        public void LoadData()
        {
            switch (Mode)
            {
                case DisplayMode.ItemBrief:
                    IsItemAllreadyExist = this.GetBL<ItemBriefBL>().IsItemAllreadyExist(RelatedId);

                    List<ItemBriefAttachment> attachments = this.GetBL<ItemBriefBL>().GetItemBriefAttachments(RelatedId);

                    if (attachments.Count() > 0)
                    {
                        divNodivItemBriefAttachments.Visible = false;
                        divNoProjectAttachments.Visible = false;
                        divAttachments.Visible = true;
                    }
                    else
                    {
                        divAttachments.Visible = false;
                        divNoProjectAttachments.Visible = false;
                        divNodivItemBriefAttachments.Visible = true;
                    }

                    if (UpdateAttachmentsCount != null)
                    {
                        UpdateAttachmentsCount(attachments.Count());
                    }

                    this.LoadedImageCount = attachments.Count(m => m.IsImageFile == true);

                    string[] itemBriefKeys = new string[] { "DocumentMediaId", "ItemBriefItemDocumentMediaId" };
                    lvAttachments.DataKeyNames = itemBriefKeys;
                    lvAttachments.GroupItemCount = 1;
                    lvAttachments.DataSource = attachments;
                    lvAttachments.DataBind();
                    upnlDocumentList.Update();
                    //Based on the ItemBrief Status (If an Item has already being created or not)
                    if (ItemTypeId > 0)
                        ltrItemTypeName.Text = Utils.GetItemTypeById(ItemTypeId).Name;
                    break;

                case DisplayMode.Project:
                    List<ProjectAttachment> projAttachments = this.GetBL<ProjectBL>().GetProjectAttachments(RelatedId);
                    if (projAttachments.Count > 0)
                    {
                        divNodivItemBriefAttachments.Visible = false;
                        divNoProjectAttachments.Visible = false;
                        divAttachments.Visible = true;
                    }
                    else
                    {
                        divAttachments.Visible = false;
                        divNoProjectAttachments.Visible = true;
                        divNodivItemBriefAttachments.Visible = false;
                    }
                    if (UpdateAttachmentsCount != null)
                    {
                        UpdateAttachmentsCount(projAttachments.Count());
                    }

                    string[] projectKeys = new string[] { "DocumentMediaId" };
                    lvAttachments.DataKeyNames = projectKeys;
                    lvAttachments.GroupItemCount = 2;
                    lvAttachments.DataSource = projAttachments;
                    lvAttachments.DataBind();
                    upnlDocumentList.Update();
                    break;
            }
        }

        #endregion Public Methods
    }
}