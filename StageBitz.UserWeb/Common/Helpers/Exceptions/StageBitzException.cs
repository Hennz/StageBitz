using System;
using System.Web;

namespace StageBitz.UserWeb.Common.Helpers.Exceptions
{
    public enum ExceptionOrigin
    {
        ItemBriefDetails,
        ItemBriefList,
        ProjectDetails,
        ItemBriefTasks,
        UserDetails
    }
    [Serializable]
    public class StageBitzException : Exception
    {
        /// <summary>
        /// Where this exception originated from.
        /// </summary>
        public ExceptionOrigin Origin { get; set; }
        public int RelatedId { get; set; }

        /// <summary>
        /// Returns and clears the last StageBitz exception that occured in StageBitz system.
        /// </summary>
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

        protected StageBitzException(ExceptionOrigin origin, int relatedId, string message)
            : base(message)
        {
            Origin = origin;
            RelatedId = relatedId;
        }

        public static void ThrowException(StageBitzException ex)
        {
            HttpContext.Current.Session["LastException"] = ex;
            throw ex;
        }
    }
}