using System;
using System.Net;
using System.Web.Http;
using InteractionPlanApi.Auth;
using InteractionPlanApi.Request;
using InteractionPlanApi.Request.NCCO;
using Microsoft.Web.Http;
using NewVoiceMedia.AspNet.WebApi.Models;
using NSwag.Annotations;

namespace InteractionPlanApi.Controllers
{
    [RoutePrefix("nexmoanswerattach")]
    [KongAuthentication(InteractionPlanApiScope)]
    [AllowAnonymous]
    [ApiVersion("1")]
    public class NexmoAnswerAttachController : ApiController
    {
        internal const string InteractionPlanApiScope = "newvoicemedia.com/api/interactionplan";
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ApiError))]
        [Route("")]
        public IHttpActionResult NexmoAnswerAttach(string uuid)
        {
            var nccos = new NCCO[]
            {
                new Conversation
                {
                    Name = "conf_" + uuid
                }
            };
            return Ok(nccos);
        }
    }
}
