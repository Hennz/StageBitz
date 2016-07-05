using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace StageBitz.UserWeb.Controls.Personal
{
    /// <summary>
    /// User control for user skils
    /// </summary>
    public partial class UserSkills : UserControlBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the view user identifier.
        /// </summary>
        /// <value>
        /// The view user identifier.
        /// </value>
        public int ViewUserId
        {
            get
            {
                if (ViewState["ViewUserId"] == null)
                {
                    ViewState["ViewUserId"] = UserID;
                }

                return (int)ViewState["ViewUserId"];
            }
            set
            {
                ViewState["ViewUserId"] = value;
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
        /// Gets or sets the user skills temporary table.
        /// </summary>
        /// <value>
        /// The user skills temporary table.
        /// </value>
        private DataTable UserSkillsTempTable
        {
            get
            {
                if (ViewState["UserSkillsTempTable"] == null)
                {
                    ViewState["UserSkillsTempTable"] = InitializeUserSkillsTempTable();
                }

                return (DataTable)ViewState["UserSkillsTempTable"];
            }
            set
            {
                ViewState["UserSkillsTempTable"] = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the ItemsRequested event of the cboSkills control.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="e">The <see cref="RadComboBoxItemsRequestedEventArgs"/> instance containing the event data.</param>
        protected void cboSkills_ItemsRequested(object o, RadComboBoxItemsRequestedEventArgs e)
        {
            string keyword = e.Text.Trim().ToLower();

            if (keyword == string.Empty)
                return;

            string matchPattern = string.Format(@"\b{0}", keyword); //Search begining of words.

            //Get all built-in skills
            Skill[] allSkills = DataContext.Skills.OrderBy(sk => sk.Name).ToArray();

            try
            {
                //Take top 10 matches from regular expression
                Regex regex = new Regex(matchPattern);
                Skill[] filteredSkills = allSkills.Where(sk => regex.IsMatch(sk.Name.ToLower())).Take(10).ToArray();

                int resultCount = filteredSkills.Length;

                for (int i = 0; i < resultCount; i++)
                {
                    Match keywordMatch = Regex.Match(filteredSkills[i].Name, matchPattern, RegexOptions.IgnoreCase);
                    StringBuilder formattedItemText = new StringBuilder(filteredSkills[i].Name);

                    //Highlight matching word portion
                    if (keywordMatch != null && keywordMatch.Length > 0)
                    {
                        formattedItemText.Insert(keywordMatch.Index, "<b>");
                        formattedItemText.Insert(3 + keywordMatch.Index + keyword.Length, "</b>");
                    }

                    //Add the matched items to the suggestion list
                    using (RadComboBoxItem item = new RadComboBoxItem())
                    {
                        Literal ltrl = new Literal();
                        item.Controls.Add(ltrl);
                        ltrl.Text = Support.TruncateString(formattedItemText.ToString(), 35);

                        item.Text = filteredSkills[i].Name;
                        item.Value = filteredSkills[i].SkillId.ToString();

                        cboSkills.Items.Add(item);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboSkills control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadComboBoxSelectedIndexChangedEventArgs"/> instance containing the event data.</param>
        protected void cboSkills_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            //verify whether the postback has occured from a skill addition or not.
            if (!PageBase.StopProcessing)
            {
                if (hdnSkillAdded.Value != string.Empty)
                {
                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "RestAddEventFireStatus", "addEventFired = false;", true);
                    hdnSkillAdded.Value = string.Empty;

                    AddSkill(e.Text);

                    cboSkills.Text = string.Empty;
                    cboSkills.Focus();
                }
            }
        }

        #region List View Events

        /// <summary>
        /// Handles the ItemDataBound event of the lvSkillsEditable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvSkillsEditable_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            Label lblSkillName = (Label)e.Item.FindControl("lblSkillName");
            DataRowView drvSkill = ((DataRowView)e.Item.DataItem);

            string skillName = drvSkill["SkillName"].ToString();
            lblSkillName.Text = Support.TruncateString(skillName, 30);
            if (skillName.Length > 30)
            {
                lblSkillName.ToolTip = skillName;
            }
        }

        /// <summary>
        /// Handles the ItemDeleting event of the lvSkillsEditable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewDeleteEventArgs"/> instance containing the event data.</param>
        protected void lvSkillsEditable_ItemDeleting(object sender, ListViewDeleteEventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                int uniqueId = (int)e.Keys["UniqueId"];

                DataRow dr = UserSkillsTempTable.Select("UniqueId=" + uniqueId)[0];
                dr.Delete();

                PageBase.IsPageDirty = true;

                DisplaySkills();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvSkillsReadOnly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvSkillsReadOnly_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            Label lblSkillName = (Label)e.Item.FindControl("lblSkillName");
            dynamic skill = ((dynamic)e.Item.DataItem);

            lblSkillName.Text = Support.TruncateString(skill.SkillName, 55);
            if (skill.SkillName.Length > 55)
            {
                lblSkillName.ToolTip = skill.SkillName;
            }
        }

        #endregion List View Events

        #endregion Event Handlers

        #region Public Methods

        public void LoadData()
        {
            upnlUserSkillsAdd.Visible = !IsReadOnly;

            DisplaySkills();
        }

        /// <summary>
        /// Saves the temporary changes made to the datatable into the database.
        /// </summary>
        public void SaveChanges()
        {
            #region Deleted records

            DataTable dtDeleted = UserSkillsTempTable.GetChanges(DataRowState.Deleted);

            if (dtDeleted != null)
            {
                foreach (DataRow drDeleted in dtDeleted.Rows)
                {
                    if (drDeleted["UserSkillId", DataRowVersion.Original] != DBNull.Value)
                    {
                        int deletedUserSkillId = (int)drDeleted["UserSkillId", DataRowVersion.Original];
                        UserSkill deleteSkill = DataContext.UserSkills.Where(sk => sk.UserSkillId == deletedUserSkillId).FirstOrDefault();
                        DataContext.UserSkills.DeleteObject(deleteSkill);
                    }
                }
            }

            #endregion Deleted records

            #region Added records

            DataTable dtAdded = UserSkillsTempTable.GetChanges(DataRowState.Added);

            if (dtAdded != null)
            {
                foreach (DataRow drNew in dtAdded.Rows)
                {
                    UserSkill newSkill = new UserSkill();
                    newSkill.UserId = ViewUserId;
                    newSkill.CreatedByUserId = newSkill.LastUpdatedByUserId = UserID;
                    newSkill.CreatedDate = newSkill.LastUpdatedDate = Now;

                    if (drNew["SkillId"] == DBNull.Value)
                    {
                        newSkill.CustomSkillName = drNew["SkillName"].ToString();
                    }
                    else
                    {
                        newSkill.SkillId = (int)drNew["SkillId"];
                    }

                    DataContext.UserSkills.AddObject(newSkill);
                }
            }

            #endregion Added records

            DataContext.SaveChanges();

            //Empty the temporary changes and reload data.
            UserSkillsTempTable = null;
            DisplaySkills();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Adds the specified skill to the user's skills, if it does not already exist.
        /// </summary>
        private void AddSkill(string newSkill)
        {
            newSkill = newSkill.Trim();

            //Check whether the new skill text is already in the user's skill list
            DataRow[] matchingRows = UserSkillsTempTable.Select(string.Format("SkillName='{0}'", newSkill));

            if (matchingRows.Length == 0 && newSkill != string.Empty)
            {
                DataRow drNew = UserSkillsTempTable.NewRow();
                UserSkillsTempTable.Rows.Add(drNew);

                //Search for the skill name among built-in skills.
                //If a built-in skill is found, add it. Otherwise add a custom skill name.
                var builtInSkill = DataContext.Skills.Where(sk => sk.Name.ToLower() == newSkill.ToLower()).FirstOrDefault();
                drNew["SkillName"] = newSkill;
                if (builtInSkill != null)
                {
                    drNew["SkillId"] = builtInSkill.SkillId;
                }

                PageBase.IsPageDirty = true;

                DisplaySkills();
            }
        }

        /// <summary>
        /// Initializes the user skills temporary table.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private DataTable InitializeUserSkillsTempTable()
        {
            DataTable dt = new DataTable("UserSkills");
            dt.Columns.Add("UserSkillId", typeof(int));
            dt.Columns.Add("SkillId", typeof(int));
            dt.Columns.Add("SkillName", typeof(string));

            DataColumn colUnique = new DataColumn("UniqueId", typeof(int));
            colUnique.AutoIncrement = true;
            dt.Columns.Add(colUnique);

            dt.CaseSensitive = false;

            var skills = from usk in DataContext.UserSkills
                         from sk in DataContext.Skills.Where(skill => skill.SkillId == usk.SkillId).Take(1).DefaultIfEmpty()
                         where usk.UserId == ViewUserId
                         select new { usk.UserSkillId, SkillName = (sk == null) ? usk.CustomSkillName : sk.Name };

            foreach (var skill in skills)
            {
                dt.Rows.Add(skill.UserSkillId, 0, skill.SkillName);
            }

            dt.AcceptChanges();

            return dt;
        }

        /// <summary>
        /// Gets the user skills.
        /// </summary>
        /// <returns></returns>
        private List<dynamic> GetUserSkills()
        {
            var skills = (from usk in DataContext.UserSkills
                          from sk in DataContext.Skills.Where(skill => skill.SkillId == usk.SkillId).Take(1).DefaultIfEmpty()
                          where usk.UserId == ViewUserId
                          select new { usk.UserSkillId, SkillName = (sk == null) ? usk.CustomSkillName : sk.Name }).OrderBy(sk => sk.SkillName);

            return skills.ToList<dynamic>();
        }

        /// <summary>
        /// Displays the skills.
        /// </summary>
        private void DisplaySkills()
        {
            if (IsReadOnly)
            {
                lvSkillsEditable.Visible = false;
                lvSkillsReadOnly.DataSource = GetUserSkills();
                lvSkillsReadOnly.DataBind();
            }
            else
            {
                UserSkillsTempTable.DefaultView.Sort = "SkillName";

                lvSkillsReadOnly.Visible = false;
                lvSkillsEditable.DataSource = UserSkillsTempTable;
                lvSkillsEditable.DataBind();
            }

            upnlUserSkillList.Update();
        }

        #endregion Private Methods
    }
}