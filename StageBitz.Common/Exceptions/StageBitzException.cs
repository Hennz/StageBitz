using System;
using System.Runtime.Serialization;
using System.Web;

namespace StageBitz.Common.Exceptions
{
    /// <summary>
    /// Enum for exception origin.
    /// </summary>
    public enum ExceptionOrigin
    {
        ItemBriefDetails,
        ItemBriefList,
        ProjectDetails,
        ItemBriefTasks,
        UserDetails,
        ManageDiscounts,
        ProjectClose,
        ItemDelete,
        WebAnalytics,
        CompanyPaymentPackage,
        ItemNotVisibile
    }

    /// <summary>
    /// Exception class for StageBitz Exceptions.
    /// </summary>
    [Serializable]
    public class StageBitzException : Exception
    {
        /// <summary>
        /// Where this exception originated from.
        /// </summary>
        public ExceptionOrigin Origin { get; set; }

        /// <summary>
        /// Gets or sets the related identifier.
        /// </summary>
        /// <value>
        /// The related identifier.
        /// </value>
        public int RelatedId { get; set; }

        /// <summary>
        /// Returns and clears the last StageBitz exception that occured in StageBitz system.
        /// </summary>
        /// <returns></returns>
        public static StageBitzException GetLastException()
        {
            if (HttpContext.Current.Session["LastException"] == null)
            {
                return null;
            }

            StageBitzException ex = (StageBitzException)HttpContext.Current.Session["LastException"];
            HttpContext.Current.Session["LastException"] = null;

            return ex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageBitzException"/> class.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="message">The message.</param>
        public StageBitzException(ExceptionOrigin origin, int relatedId, string message)
            : base(message)
        {
            Origin = origin;
            RelatedId = relatedId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageBitzException"/> class.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="relatedId">The related identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public StageBitzException(ExceptionOrigin origin, int relatedId, string message, Exception ex)
            : base(message, ex)
        {
            Origin = origin;
            RelatedId = relatedId;
        }

        /// <summary>
        /// Throws the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void ThrowException(StageBitzException ex)
        {
            HttpContext.Current.Session["LastException"] = ex;
            throw ex;
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}