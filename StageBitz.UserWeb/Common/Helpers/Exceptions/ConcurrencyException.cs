using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StageBitz.UserWeb.Common.Helpers.Exceptions
{
    /// <summary>
    /// Represents a DB Concurrency vialation in StageBitz system.
    /// </summary>
    [Serializable]
    public class ConcurrencyException : StageBitzException
    {
        public ConcurrencyException(ExceptionOrigin origin, int relatedId)
            : base(origin, relatedId, string.Format("DB concurrency vialoation occured in {0} with RelatedId {1}.", origin, relatedId))
        {
        }
    }
}