using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using System.Web.Http.Cors;
using System.Security.Claims;
using System.Configuration;

namespace BackEndApiService.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    public class ValuesController : ApiController
    {
        private static string trustedCallerClientId = ConfigurationManager.AppSettings["ida:trustedCallerClientId"];

        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(int id)
        {
            //Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            //if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
            //{
            //    throw new HttpResponseException(new HttpResponseMessage
            //    {
            //        StatusCode = HttpStatusCode.Unauthorized,
            //        ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found"
            //    });
            //}

            Claim subject = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"); //For App
            var appid = subject.Subject.FindFirst("appid").Value;

            //if (appid == trustedCallerClientId)
            //{
            //    return $"value {id}";
            //}



            return $"value {id}";
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
