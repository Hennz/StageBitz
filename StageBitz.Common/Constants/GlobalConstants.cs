namespace StageBitz.Common.Constants
{
    /// <summary>
    /// Class contains global level constatns.
    /// </summary>
    public class GlobalConstants
    {
        /// <summary>
        /// Keep code headers.
        /// </summary>
        public class CodeHeaderValues
        {
        }

        /// <summary>
        /// Keep Code values.
        /// </summary>
        public class CodeValues
        {
        }

        /// <summary>
        /// Keep related table text.
        /// </summary>
        public class RelatedTables
        {
            /// <summary>
            /// Only booking related constants.
            /// </summary>
            public class Bookings
            {
                public const string NonProject = "NonProject";
                public const string ItemBrief = "ItemBrief";
                public const string Project = "Project";
            }

            public class ExportFiles
            {
                public const string Company = "Company";
                public const string Project = "Project";
            }

            public class UserRoleTypes
            {
                public const string Companies = "Companies";
                public const string Projects = "Projects";
            }
        }

        /// <summary>
        /// Keep file extention types.
        /// </summary>
        public class FileExtensions
        {
            public const string ExcelFile = "xls";
        }

        /// <summary>
        /// Keep file content types.
        /// </summary>
        public class FileContentTypes
        {
            public const string ZipFile = "application/x-zip-compressed";

        }

        /// <summary>
        /// The parameter delimiter
        /// </summary>
        public const char parameterDelimiter = '|';
    }
}