using System;
using System.Diagnostics;
using System.Net;
using System.Web.Http;
using InteractionPlanApi.Auth;
using InteractionPlanApi.Request;
using InteractionPlanApi.Request.NCCO;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using NewVoiceMedia.AspNet.WebApi.Models;
using NewVoiceMedia.CallCentre.ToggleService;
using NSwag.Annotations;

namespace InteractionPlanApi.Controllers
{
    [RoutePrefix("nexmogetnccos")]
    [KongAuthentication(InteractionPlanApiScope)]
    [AllowAnonymous]
    [ApiVersion("1")]
    public class NexmoGetNccosController : ApiController
    {
        private readonly NccoQueue _nccoQueue;

        public NexmoGetNccosController(NccoQueue nccoQueue)
        {
            _nccoQueue = nccoQueue;
        }

        internal const string InteractionPlanApiScope = "newvoicemedia.com/api/interactionplan";
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ApiError))]
        [Route("")]
        public IHttpActionResult NexmoGetNccos(string uuid)
        {
            Debug.WriteLine($"NexmoGetNccos {uuid}");
            var nccos = _nccoQueue.GetQueue(uuid);
            nccos.Add(new Notify { Payload = new Payload { Data = "finishedExecuting" }});
            nccos.Add(new Conversation { Name = "conf_" + uuid });
            _nccoQueue.SetCurrentlyExecuting(uuid, true);
            _nccoQueue.Clear(uuid);

            Debug.Write(JsonConvert.SerializeObject(nccos));

            return Ok(nccos);
        }
    }
}
