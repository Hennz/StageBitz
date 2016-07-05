using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageBitz.Data.DataTypes
{
    public class FieldData
    {
        public int FieldId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string FieldName { get; set; }
        public int ProjectUsingField { get; set; }
        public int ItemTypeUsingField { get; set; }
        public string FieldHtml { get; set; }
        public string FieldHtmlControl { get; set; }
        public string FieldInnerHtml { get; set; }
        public FieldGroup Group { get; set; }
        public Field Field { get; set; }
        public IEnumerable<FieldOptionValue> FieldOptions { get; set; }
        public IEnumerable<FieldAttributeValue> FieldAttributes { get; set; }
    }

    public class FieldAttributeValue
    {
        public string TagName;
        public string AttributeValue;
    }

    public class FieldOptionValue
    {
        public int OptionId;
        public string OptionValue;
    }
}
