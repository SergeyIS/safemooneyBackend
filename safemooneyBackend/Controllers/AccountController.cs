﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using safemooneyBackend.Security.Filters;
using DataAccessLibrary;
using SharedResourcesLibrary;
using System.Web.Http.Results;
using System.Net;
using System.Net.Http;
using safemooneyBackend.Models;
using safemooneyBackend.Security.Util;

namespace safemooneyBackend.Controllers
{
    public class AccountController : ApiController
    {
        private IDataAccess db = new DataBuilder();

        /// <summary>
         /// This method provide access to resources  for user
         /// </summary>
         /// <param name="user">
         /// {
         ///  "username": "value",
         ///  "password": "value"
         /// }
         /// </param>
         /// <returns></returns>
        [HttpPost]
        [Route("api/account/login")]
        public HttpResponseMessage LogIn(UserRequestModel user)
        {
            if (user == null || user.Username == null || user.Password == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var localUser = db.FindUserByLogin(user.Username);
            //todo: password decryption

            if (localUser == null || !localUser.Password.Equals(user.Password))
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            
            TokenGenerator tgen = new TokenGenerator(user.Username, user.Password);
            string token = tgen.GenerateKey();

            //save changes to db
            bool resultOfOperation = db.SetTokenForUser(localUser.Id, token);

            if (!resultOfOperation)
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
                     
            TokenResponseModel response = new TokenResponseModel();
            response.UserId = localUser.Id;
            response.Username = localUser.Username;
            response.FirstName = localUser.FirstName;
            response.LastName = localUser.LastName;
            response.Access_Token = token;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [AuthFilter]
        [HttpGet]
        [Route("api/{userId}/account/logout")]
        public HttpResponseMessage LogOut(int userId = -1)
        {
            bool resultOfOperation = db.ResetTokenForUser(userId);

            if (!resultOfOperation)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            return Request.CreateResponse(HttpStatusCode.OK);

        }

        /// <summary>
        /// This method register user in the system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/account/signup")]
        public HttpResponseMessage SignUp(UserRequestModel user)
        {
            if (user == null || user.Username == null || user.Password == null || 
                user.Username == "" || user.Password == "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            try
            {
                if (db.CheckForUser(user.Username))
                    return Request.CreateResponse(HttpStatusCode.Forbidden);

                db.AddUserSafely(user.Username, user.Password, user.FirstName, user.LastName);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [AuthFilter]
        [HttpPost]
        [Route("api/{userId}/account/change")]
        public HttpResponseMessage Change([FromBody]UserRequestModel user, int userId)
        {
            if(user == null || user.FirstName == null || user.LastName == null ||
                user.Username == null || user.Password == null || user.UserId != userId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            User localUser = db.FindUserById(userId);

            if(localUser == null)
                return Request.CreateResponse(HttpStatusCode.InternalServerError);

            localUser.Username = user.Username;
            localUser.Password = user.Password;
            localUser.FirstName = user.FirstName;
            localUser.LastName = user.LastName;

            bool resultOfOperation = db.ChangeUserInfo(localUser);

            if (!resultOfOperation)
                return Request.CreateResponse(HttpStatusCode.InternalServerError);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}