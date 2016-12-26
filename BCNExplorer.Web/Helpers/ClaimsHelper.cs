using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace BCNExplorer.Web.Helpers
{
    public static class ClaimsHelper
    {
        public static string Get(IIdentity identity, string type)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var claims = claimsIdentity?.Claims;

            var claimsList = claims as IList<Claim> ?? claims.ToList();

            return claimsList.Where(p => p.Type == type.ToString()).Select(p => p.Value).FirstOrDefault();
        }
    }
}