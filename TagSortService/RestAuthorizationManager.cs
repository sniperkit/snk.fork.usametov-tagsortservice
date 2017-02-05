using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Bookmarks.Common;

namespace TagSortService
{
    public class RestAuthorizationManager : ServiceAuthorizationManager
    {
        IBookmarksContext context;
        IBookmarksContext Context
        {
            get
            {
                if (context == null)
                    context = new Bookmarks.Mongo.Data.BookmarksContext
                                                        (Utils.GetConnectionString());

                return context;
            }
        }
        /// <summary>
        /// auth
        /// </summary>
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            //Extract the Authorization header, and parse out the credentials converting the Base64 string:
            var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];

            if (WebOperationContext.Current.IncomingRequest.Method == "OPTIONS")
                return true;

            if ((authHeader != null) && (authHeader != string.Empty))
            {
                var svcCredentials = System.Text.ASCIIEncoding.ASCII
                        .GetString(Convert.FromBase64String(authHeader)).Split(':');

                var sha = SHA256Managed.Create();
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(svcCredentials[1]));
                var hash = Convert.ToBase64String(bytes);

                var user = new User { Name = svcCredentials[0], PasswordHash = hash};
                //call user validation routine
                return CredsAreValid(user);                    
            }
            else
            {
                //No authorization header was provided, so challenge the client to provide before proceeding:
                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"TagSortService\"");

                //Throw an exception with the associated HTTP status code equivalent to HTTP status 401
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
        }

        private bool CredsAreValid(User user)
        {            
            if(Context.GetUserByUsernameAndPasswdHash
                (user.Name, user.PasswordHash) == null)
                return false;

            return true;
        }
    }
}