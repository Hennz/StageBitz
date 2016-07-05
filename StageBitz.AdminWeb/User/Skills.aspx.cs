using System;
using System.Collections.Generic;
using System.Linq;
using StageBitz.AdminWeb.Common.Helpers;
using Telerik.Web.UI;
using StageBitz.AdminWeb.Controls.Common;
using System.Web.UI.WebControls;
using StageBitz.Data;

namespace StageBitz.AdminWeb.User
{
    public partial class Skills : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBreadCrumbs();
            }
        }

        #region Events

        #region Default Skills Grid Events

        protected void gvDefaultSkills_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic skill = (dynamic)dataItem.DataItem;

                if (e.Item.ItemType != GridItemType.EditItem)
                {
                    dataItem["Name"].Text = Support.TruncateString(skill.Name, 30);
                    if (skill.Name.Length > 30)
                    {
                        dataItem["Name"].ToolTip = skill.Name;
                    }

                    #region Delete buttons

                    if (skill.Usage > 0)
                    {
                        dataItem["DeleteColumn"].Controls[0].Visible = false;
                    }

                    #endregion
                }
            }
        }

        protected void gvDefaultSkills_UpdateCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
        {
            if (Page.IsValid)
            {
                GridEditableItem dataItem = e.Item as GridEditableItem;
                TextBox txtName = (TextBox)dataItem.FindControl("txtName");
                string editedSkillName = txtName.Text.Trim();

                int skillId = (int)dataItem.GetDataKeyValue("SkillId");

                //look for duplicate skill names
                Skill duplicateSkill = DataContext.Skills.Where(sk => sk.Name.ToLower() == editedSkillName.ToLower()).FirstOrDefault();
                if (duplicateSkill != null && duplicateSkill.SkillId != skillId)
                {
                    popupDefaultSkillMessage.ShowPopup();
                    e.Canceled = true;
                    return;
                }
                else //only update if the new value is different
                {
                    Skill skill = DataContext.Skills.Where(sk => sk.SkillId == skillId).FirstOrDefault();
                    skill.Name = editedSkillName;
                    skill.LastUpdatedByUserId = UserID;
                    skill.LastUpdatedDate = Now;

                    DataContext.SaveChanges();
                }

                gvDefaultSkills.EditIndexes.Clear();
                gvDefaultSkills.Rebind();
            }
        }

        protected void gvDefaultSkills_DeleteCommand(object sender, GridCommandEventArgs e)
        {
            GridDataItem dataItem = (GridDataItem)e.Item;

            int skillId = (int)dataItem.GetDataKeyValue("SkillId");
            Skill skill = DataContext.Skills.Where(sk => sk.SkillId == skillId).FirstOrDefault();

            if (skill.UserSkills.Count == 0)
            {
                DataContext.Skills.DeleteObject(skill);
                DataContext.SaveChanges();
            }

            gvDefaultSkills.EditIndexes.Clear();
            gvDefaultSkills.Rebind();
        }

        protected void gvDefaultSkills_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvDefaultSkills.MasterTableView.SortExpressions.Clear();
                gvDefaultSkills.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvDefaultSkills.Rebind();
            }
        }

        protected void gvDefaultSkills_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            gvDefaultSkills.DataSource = (from sk in DataContext.Skills
                                          select new
                                          {
                                              sk.SkillId,
                                              sk.Name,
                                              Usage = sk.UserSkills.Count
                                          }).OrderBy(sk => sk.Name);
        }

        #endregion

        #region Custom Skills Grid Events

        protected void gvCustomSkills_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;
                dynamic skill = (dynamic)dataItem.DataItem;

                dataItem["Name"].Text = Support.TruncateString(skill.Name, 30);
                if (skill.Name.Length > 30)
                {
                    dataItem["Name"].ToolTip = skill.Name;
                }
            }
        }

        protected void gvCustomSkills_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (e.SortExpression.Length > 0)
            {
                GridSortExpression sortExpr = new GridSortExpression();
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = e.NewSortOrder;

                gvCustomSkills.MasterTableView.SortExpressions.Clear();
                gvCustomSkills.MasterTableView.SortExpressions.AddSortExpression(sortExpr);

                e.Canceled = true;
                gvCustomSkills.Rebind();
            }
        }

        protected void gvCustomSkills_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem dataItem = e.Item as GridDataItem;
                int userSkillId = (int)dataItem.GetDataKeyValue("UserSkillId");

                if (e.CommandName == "Keep")
                {
                    string customSkillName = DataContext.UserSkills.Where(usk => usk.UserSkillId == userSkillId).Select(usk => usk.CustomSkillName).FirstOrDefault();

                    if (customSkillName != null)
                    {
                        KeepCustomSkill(customSkillName);
                        LoadData();
                    }
                }
                else if (e.CommandName == "Ignore")
                {
                    string customSkillName = DataContext.UserSkills.Where(usk => usk.UserSkillId == userSkillId).Select(usk => usk.CustomSkillName).FirstOrDefault().ToLower();

                    if (customSkillName != string.Empty)
                    {
                        var matchingUserSkills = DataContext.UserSkills.Where(usk => usk.CustomSkillName == customSkillName);

                        foreach (UserSkill usk in matchingUserSkills)
                        {
                            usk.IgnoreFromAdmin = true;
                            usk.LastUpdatedByUserId = UserID;
                            usk.LastUpdatedDate = Now;
                        }
                        DataContext.SaveChanges();
                    }

                    LoadCustomSkills();
                }
            }
        }

        protected void gvCustomSkills_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            var allCustomSkills = DataContext.UserSkills.Where(usk => usk.SkillId == null);
            //var groupedSkills = allCustomSkills.GroupBy(usk => usk.CustomSkillName.ToLower()).Select(usk => new {Name = usk.Key, Usage = usk.Count()});

            var groupedSkills = (from usk in DataContext.UserSkills
                                 where usk.SkillId == null
                                 group usk by usk.CustomSkillName into grp
                                 select new
                                 {
                                     Name = grp.Key,
                                     Usage = grp.Count(),
                                     IgnoreCount = grp.Count(usk => usk.IgnoreFromAdmin),
                                     UserSkillId = grp.FirstOrDefault().UserSkillId
                                 }).Where(sk => sk.Usage > sk.IgnoreCount);

            gvCustomSkills.DataSource = groupedSkills;
        }

        #endregion

        protected void btnAddSkill_Click(object sender, EventArgs e)
        {
            string editedSkillName = txtNewSkill.Text.Trim();

            //look for duplicate skill names
            Skill duplicateSkill = DataContext.Skills.Where(sk => sk.Name.ToLower() == editedSkillName.ToLower()).FirstOrDefault();
            if (duplicateSkill != null)
            {
                popupDefaultSkillMessage.ShowPopup();
                return;
            }
            else
            {
                Skill skill = new Skill();
                DataContext.Skills.AddObject(skill);
                skill.Name = editedSkillName;
                skill.LastUpdatedByUserId = skill.CreatedByUserId = UserID;
                skill.LastUpdatedDate = skill.CreatedDate = Now;

                DataContext.SaveChanges();

                gvDefaultSkills.EditIndexes.Clear();
                gvDefaultSkills.Rebind();

                txtNewSkill.Text = string.Empty;
            }
        }

        #endregion

        #region Support Methods

        private void LoadData()
        {
            LoadDefaultSkills();
            LoadCustomSkills();
        }

        private void LoadDefaultSkills()
        {
            gvDefaultSkills.Rebind();
            upnlDefaultSkills.Update();
        }

        private void LoadCustomSkills()
        {
            gvCustomSkills.Rebind();
            upnlCustomSkills.Update();
        }

        private void KeepCustomSkill(string customSkillName)
        {
            //Look for a default skill with a simillar name. If not create new default skill with the specified custom skill name.
            //Modify all UserSkill entries using the specified custom skill name to use the default skill.

            Skill defaultSkill = DataContext.Skills.Where(sk => sk.Name.ToLower() == customSkillName.ToLower()).FirstOrDefault();

            if (defaultSkill == null)
            {
                defaultSkill = new Skill();
                defaultSkill.Name = customSkillName;
                defaultSkill.CreatedByUserId = defaultSkill.LastUpdatedByUserId = UserID;
                defaultSkill.CreatedDate = defaultSkill.LastUpdatedDate = Now;
                DataContext.Skills.AddObject(defaultSkill);
            }

            //Modify all UserSkill entries using the specified custom skill name to use the default skill.
            var userSkills = DataContext.UserSkills.Where(usk => usk.SkillId == null && usk.CustomSkillName.ToLower() == customSkillName.ToLower());

            if (userSkills.Count() > 0)
            {
                foreach (UserSkill uSkill in userSkills)
                {
                    uSkill.Skill = defaultSkill;
                    uSkill.CustomSkillName = null;
                    uSkill.LastUpdatedByUserId = UserID;
                    uSkill.LastUpdatedDate = Now;
                }
            }

            DataContext.SaveChanges();
        }

        private void LoadBreadCrumbs()
        {
            BreadCrumbs breadCrumbs = GetBreadCrumbsControl();
            breadCrumbs.AddLink("Users", "~/User/Users.aspx");
            breadCrumbs.AddLink(DisplayTitle, null);
            breadCrumbs.LoadControl();
        }

        #endregion
    }
}