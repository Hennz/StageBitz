using StageBitz.Common;
using StageBitz.Data;
using StageBitz.Data.DataTypes;
using StageBitz.Logic.Business.Personal;
using StageBitz.Service.Helpers;
using System;
using System.Linq;
using System.Web.Http;

namespace StageBitz.Service.Controllers
{
    /// <summary>
    /// Web api controller for mobile security.
    /// </summary>
    public class SecurityController : ApiController
    {
        /// <summary>
        /// Authenticates the user.
        /// POST api/security/AuthenticateUser
        /// </summary>
        /// <param name="userAuthenticationDetailsObj">The user authentication details object.</param>
        /// <returns></returns>
        [HttpPost]
        public MobileInitialData AuthenticateUser(InitialRequestDetails userAuthenticationDetailsObj)
        {
            string status = string.Empty;
            string message = string.Empty;

            MobileInitialData mobileInitialData = null;
            try
            {
                using (StageBitzDB dataContext = new StageBitzDB())
                {
                    bool isValidVersion = Helper.IsValidAppVersion(userAuthenticationDetailsObj.Version, out status);
                    if (isValidVersion)
                    {
                        string passwordHash = Utils.HashPassword(userAuthenticationDetailsObj.Pwd);
                        PersonalBL personalBL = new PersonalBL(dataContext);
                        StageBitz.Data.User user = personalBL.AuthenticateUser(userAuthenticationDetailsObj.Email, passwordHash);

                        if (user == null)
                        {
                            int pendingEmailTypeCodeId = Utils.GetCodeByValue("EmailChangeRequestStatus", "PENDING").CodeId;
                            EmailChangeRequest emailChangeRequest = dataContext.EmailChangeRequests.Where(ec => ec.Email == userAuthenticationDetailsObj.Email && ec.StatusCode == pendingEmailTypeCodeId).FirstOrDefault();
                            if (emailChangeRequest != null)
                            {
                                //Check the password by getting the current active userID.
                                int userId = emailChangeRequest.UserId;
                                //If the PassWord is matched, we know that the user is valid where as he did not follow the link.
                                if (dataContext.Users.Where(u => u.UserId == userId && u.Password == passwordHash).FirstOrDefault() != null)
                                {
                                    // He has changed his Primary Email Address. However he has not activate it yet
                                    status = "NOTOK";
                                    message = "Email updated please confirm.";
                                    goto FinalStatement;
                                }
                            }

                            //Invalid LogIn
                            status = "NOTOK";
                            message = "Invalid Email address or Password.";
                        }
                        else
                        {
                            if (user.IsActive == true)
                            {
                                //Build the token
                                //Return Initializtion data
                                status = "OK";
                                byte[] content = Utils.EncryptStringAES(user.UserId.ToString());
                                mobileInitialData = Helper.GetAllInitializeDataForUser(user.UserId);
                                mobileInitialData.UserToken = Utils.EncryptStringAES(user.UserId.ToString());
                            }
                            else
                            {
                                //User is not activated yet
                                status = "NOTOK";
                                message = "Please activate your account.";
                            }
                        }
                    }
                    else
                    {
                        message = "Please update App.";
                    }
                }
            }
            catch (Exception ex)
            {
                AgentErrorLog.HandleException(ex);
                status = "ERROR";
                message = "Oops! Unkown error. Sorry...";
            }
        FinalStatement:
            if (mobileInitialData == null)
                mobileInitialData = new MobileInitialData();
            mobileInitialData.Status = status;
            mobileInitialData.Message = message;
            return mobileInitialData;
        }
    }
}