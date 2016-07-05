using StageBitz.Data;
using StageBitz.UserWeb.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StageBitz.UserWeb.Controls.Common
{
    /// <summary>
    /// User control for country list.
    /// </summary>
    public partial class CountryList : UserControlBase
    {
        #region Enums

        /// <summary>
        /// Validator Possition Enum
        /// </summary>
        public enum ValidatorPossition
        {
            Left = 1,
            Right = 2
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// The validator possition var.
        /// </summary>
        private ValidatorPossition validatorPossition = ValidatorPossition.Right;

        /// <summary>
        /// Gets or sets the validator possition mode.
        /// </summary>
        /// <value>
        /// The validator possition mode.
        /// </value>
        public ValidatorPossition ValidatorPossitionMode
        {
            get { return validatorPossition; }
            set { validatorPossition = value; }
        }

        /// <summary>
        /// Gets or sets the country identifier.
        /// </summary>
        /// <value>
        /// The country identifier.
        /// </value>
        public int CountryID
        {
            get
            {
                int countryID = 0;
                int.TryParse(ddCountry.SelectedValue, out countryID);
                if (countryID <= 0 && ViewState["CountryID"] != null)
                    return (int)ViewState["CountryID"];
                else
                    return countryID;
            }
            set
            {
                ViewState["CountryID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the drop down.
        /// </summary>
        /// <value>
        /// The width of the drop down.
        /// </value>
        public int DropDownWidth
        {
            get
            {
                if (ViewState["DropDownWidth"] == null)
                {
                    ViewState["DropDownWidth"] = 210;
                }
                return (int)ViewState["DropDownWidth"];
            }
            set
            {
                ViewState["DropDownWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this control is read only; otherwise, <c>false</c>.
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

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return rqdRight.ValidationGroup;
            }
            set
            {
                rqdRight.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation error message.
        /// </summary>
        /// <value>
        /// The validation error message.
        /// </value>
        public string ValidationErrorMessage
        {
            get
            {
                return rqdRight.ErrorMessage;
            }
            set
            {
                rqdRight.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation tool tip.
        /// </summary>
        /// <value>
        /// The validation tool tip.
        /// </value>
        public string ValidationToolTip
        {
            get
            {
                return rqdRight.ToolTip;
            }
            set
            {
                rqdRight.ToolTip = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCountries(false);
                ddCountry.Width = DropDownWidth;
            }

            ddCountry.Enabled = !IsReadOnly;
        }

        /// <summary>
        /// Shows all countries.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ShowAllCountries(object sender, EventArgs e)
        {
            if (!PageBase.StopProcessing)
            {
                LoadCountries(true);
            }
        }

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Selects the country identifier.
        /// </summary>
        /// <param name="countryId">The country identifier.</param>
        public void SelectCountryId(int countryId)
        {
            var country = (from c in DataContext.Countries
                           where c.IsActive == true && c.SortOrder == 1 && c.CountryId == countryId
                           orderby c.CountryName
                           select c).FirstOrDefault();

            if (country != null)
            {
                LoadCountries(false);
            }
            else
            {
                LoadCountries(true);
            }

            ddCountry.SelectedValue = countryId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Loads the countries.
        /// </summary>
        /// <param name="isShowAll">if set to <c>true</c> [is show all].</param>
        public void LoadCountries(bool isShowAll)
        {
            List<Country> countries = null;

            if (!isShowAll)
            {
                countries = (from c in DataContext.Countries
                             where c.IsActive == true && c.SortOrder == 1
                             orderby c.CountryName
                             select c).ToList<Country>();

                //Default selection
                if (CountryID == 0 || CountryID == -1)
                {
                    Country country = new Country();
                    country.CountryName = "-- Please select --";
                    country.CountryId = -1;//This is to track whether a country is not being selected
                    countries.Insert(0, country);
                }

                Country countryALL = new Country();
                countryALL.CountryName = "-- Show all countries--";
                countryALL.CountryId = 0;//This is to track whether a country is not being selected
                countries.Add(countryALL);
            }
            else
            {
                countries = (from c in DataContext.Countries
                             where c.IsActive == true
                             orderby c.CountryName
                             select c).ToList<Country>();
                //Default selection
                if (CountryID == 0 || CountryID == -1)
                {
                    Country country = new Country();
                    country.CountryName = "-- Select from all countries --";
                    country.CountryId = -1;//This is to track whether a country is not being selected
                    countries.Insert(0, country);
                }
            }

            if (CountryID > 0) //If there is a countryID, check whether the country is in list, else add it to the list.
            {
                Country country = countries.Find(delegate(Country c)
                {
                    return c.CountryId == CountryID;
                }
                );

                if (country == null)
                {
                    //Needs to get the country and add to the list
                    countries.Insert(0, (DataContext.Countries.First(c => c.CountryId == CountryID)));
                }
                else
                {
                    //Means it is in the list.
                    ddCountry.SelectedValue = CountryID.ToString();
                }
            }
            ddCountry.DataSource = countries;
            ddCountry.DataBind();
        }

        #endregion Public Methods
    }
}