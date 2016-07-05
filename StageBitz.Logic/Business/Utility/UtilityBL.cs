using StageBitz.Data;
using StageBitz.Data.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StageBitz.Logic.Business.Utility
{
    /// <summary>
    /// Utility business layer
    /// </summary>
    public class UtilityBL : BaseBL
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public UtilityBL(StageBitzDB dataContext)
            : base(dataContext)
        {
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="relatedTable">The related table.</param>
        /// <returns></returns>
        public DocumentMedia GetDefaultImage(int relatedId, string relatedTable)
        {
            return (from m in DataContext.DocumentMedias
                    where m.RelatedTableName == relatedTable && m.RelatedId == relatedId && m.SortOrder == 1
                    select m).FirstOrDefault();
        }

        /// <summary>
        /// Gets the thumbnail image.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <returns></returns>
        public byte[] GetThumbnailImage(string relatedTable, int relatedId)
        {
            return (from d in DataContext.DocumentMedias
                    where d.RelatedTableName == relatedTable && d.RelatedId == relatedId
                    select d.Thumbnail).FirstOrDefault();
        }

        /// <summary>
        /// Inserts the hyperlink to document media.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="RelatedTableName">Name of the related table.</param>
        /// <param name="RelatedId">The related identifier.</param>
        /// <param name="url">The URL.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns></returns>
        public DocumentMedia InsertHyperlinkToDocumentMedia(string name, string RelatedTableName, int RelatedId, string url, int createdBy, DateTime createdDate)
        {
            DocumentMedia media = new DocumentMedia();
            media.Name = string.IsNullOrEmpty(name) ? null : name;
            media.RelatedTableName = RelatedTableName;
            media.RelatedId = RelatedId;
            media.FileExtension = "Hyperlink";
            media.CreatedBy = media.LastUpdatedBy = createdBy;
            media.CreatedDate = media.LastUpdatedDate = createdDate;
            media.Description = url;
            DataContext.DocumentMedias.AddObject(media);
            DataContext.SaveChanges();
            return media;
        }

        /// <summary>
        /// Gets the document medias.
        /// </summary>
        /// <param name="relatedTable">The related table.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="showImagesOnly">if set to <c>true</c> [show images only].</param>
        /// <param name="documentMediaIdsToExclude">The document media ids to exclude.</param>
        /// <returns></returns>
        public List<DocumentMediaInfoList> GetDocumentMedias(string relatedTable, int relatedId, bool showImagesOnly, List<int> documentMediaIdsToExclude)
        {
            if (documentMediaIdsToExclude == null)
                documentMediaIdsToExclude = new List<int>();

            string hyperlinkText = "Hyperlink";
            var medias = (from m in DataContext.DocumentMedias
                          where m.RelatedTableName == relatedTable && m.RelatedId == relatedId && m.FileExtension != hyperlinkText.ToUpper()
                             && (showImagesOnly == false || m.IsImageFile == true)
                             && !documentMediaIdsToExclude.Contains(m.DocumentMediaId)
                          select new DocumentMediaInfoList
                          {
                              DocumentMediaId = m.DocumentMediaId,
                              Name = m.Name,
                              IsImageFile = m.IsImageFile,
                              SortOrder = m.SortOrder
                          });

            return medias.ToList<DocumentMediaInfoList>();
        }

        /// <summary>
        /// Gets the document media.
        /// </summary>
        /// <param name="DocumentMediaId">The document media identifier.</param>
        /// <returns></returns>
        public DocumentMedia GetDocumentMedia(int DocumentMediaId)
        {
            return (from m in DataContext.DocumentMedias
                    where m.DocumentMediaId == DocumentMediaId
                    select m).FirstOrDefault();
        }

        /// <summary>
        /// Updates the hyperlink document media.
        /// </summary>
        /// <param name="media">The media.</param>
        /// <param name="name">The name.</param>
        /// <param name="url">The URL.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <param name="lastUpdatedDate">The last updated date.</param>
        public void UpdateHyperlinkDocumentMedia(DocumentMedia media, string name, string url, int lastUpdatedBy, DateTime lastUpdatedDate)
        {
            if (media != null)
            {
                media.Name = name;
                media.Description = url;
                media.LastUpdatedBy = lastUpdatedBy;
                media.LastUpdatedDate = lastUpdatedDate;
                DataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Removes the hyperlink document media.
        /// </summary>
        /// <param name="documentMediaId">The document media identifier.</param>
        /// <returns></returns>
        public DocumentMedia RemoveHyperlinkDocumentMedia(int documentMediaId)
        {
            var media = (from m in DataContext.DocumentMedias where m.DocumentMediaId == documentMediaId select m).FirstOrDefault();
            if (media != null)
            {
                DataContext.DocumentMedias.DeleteObject(media);
                DataContext.SaveChanges();
            }
            return media;
        }

        /// <summary>
        /// Determines whether document media is exists.
        /// </summary>
        /// <param name="documentMediaId">The document media identifier.</param>
        /// <returns></returns>
        public bool IsDocumentMediaExist(int documentMediaId)
        {
            var media = (from m in DataContext.DocumentMedias where m.DocumentMediaId == documentMediaId select m).FirstOrDefault();
            return media != null ? true : false;
        }

        /// <summary>
        /// Gets the invitation.
        /// </summary>
        /// <param name="invitation">The invitation.</param>
        /// <returns></returns>
        public Data.Invitation GetInvitation(int invitationId)
        {
            return DataContext.Invitations.Where(i => i.InvitationId == invitationId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the email change requests by username and email type code identifier.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="pendingEmailTypeCodeId">The pending email type code identifier.</param>
        /// <returns></returns>
        public Data.EmailChangeRequest GetEmailChangeRequestsByUsernameAndEmailTypeCodeId(string username, int pendingEmailTypeCodeId)
        {
            return DataContext.EmailChangeRequests.Where(ec => ec.Email == username && ec.StatusCode == pendingEmailTypeCodeId).FirstOrDefault();
        }
    }
}