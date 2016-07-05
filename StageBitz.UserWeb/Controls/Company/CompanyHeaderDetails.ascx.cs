using StageBitz.Logic.Business.Company;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Company
{
    /// <summary>
    /// User control for company header details.
    /// </summary>
    public partial class CompanyHeaderDetails : UserControlBase
    {
        #region Properties

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
                if (ViewState["companyId"] != null)
                {
                    return (int)ViewState["companyId"];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["companyId"] = value;
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
                if (ViewState["IsReadOnly"] != null)
                {
                    return (bool)ViewState["IsReadOnly"];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["IsReadOnly"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the FileUploaded event of the fileUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fileUpload_FileUploaded(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadProfileThumbnail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveImageConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveImageConfirm_Click(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int profileImageId = 0;
                if (CompanyId > 0)
                {
                    profileImageId = this.GetBL<CompanyBL>().GetCompanyProfileImageId(CompanyId);
                }
                else
                {
                    profileImageId = fileUpload.DocumentMediaId;
                }

                DataContext.DeleteDocumentMedia(profileImageId);
                popupImageRemoveConfirmation.HidePopup();
                fileUpload.DocumentMediaId = 0;
                LoadProfileThumbnail();
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Setup image upload control
                fileUpload.RelatedTableName = "Company";
                fileUpload.RelatedId = CompanyId;
                fileUpload.UploadMediaType = UserWeb.Controls.Common.FileUpload.MediaType.Images;
                fileUpload.LoadUI();
                LoadProfileThumbnail();
                if (CompanyId > 0)
                {
                    tdHelpTip.Visible = true;
                    ltrlMembershipPeriod.Text = string.Format("Member of StageBitz™ since {0}", this.GetBL<CompanyBL>().GetCompany(CompanyId).CreatedDate.Value.Year);
                }
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Intializes the header controls.
        /// </summary>
        public void IntializeHeaderControls()
        {
            fileUpload.IsReadOnly = IsReadOnly;
            upnlFileUpload.Visible = !IsReadOnly;
        }

        /// <summary>
        /// Gets the media.
        /// </summary>
        /// <returns></returns>
        public Data.DocumentMedia GetMedia()
        {
            return (from m in DataContext.DocumentMedias
                    where m.DocumentMediaId == fileUpload.DocumentMediaId
                    select m).FirstOrDefault();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the profile thumbnail.
        /// </summary>
        private void LoadProfileThumbnail()
        {
            int profileImageId = 0;
            if (CompanyId > 0)
            {
                profileImageId = this.GetBL<CompanyBL>().GetCompanyProfileImageId(CompanyId);
            }
            else
            {
                profileImageId = fileUpload.DocumentMediaId;
            }

            bool hasProfileImage = (profileImageId != 0);
            divRemoveImage.Visible = hasProfileImage;
            popupImageRemoveConfirmation.Visible = hasProfileImage;

            if (hasProfileImage)
            {
                companyImage.DocumentMediaId = profileImageId;
                companyImage.Visible = true;
                imgCompanies.Visible = false;
            }
            else
            {
                imgCompanies.Visible = true;
                companyImage.Visible = false;
            }

            upnlThumbnail.Update();
            upnlFileUpload.Update();
        }

        #endregion Private Methods
    }
}