using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using InteractionPlanApi.Request.Base.Interaction;
using InteractionPlanApi.Request.NamedRoute;
using InteractionPlanApi.Request.NCCO;
using InteractionPlanApi.Service;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using NewVoiceMedia.AspNet.WebApi.Models;
using NewVoiceMedia.CallCentre.Model.Repositories;
using NSwag.Annotations;

namespace InteractionPlanApi.Controllers
{
    public class AnswerRequest
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("conversation_uuid")]
        public string ConversationUuid { get; set; }
    }

    [RoutePrefix("nexmoanswer")]
    [AllowAnonymous]
    [ApiVersion("1")]
    public class NexmoAnswerController : ApiController
    {
        internal const string InteractionPlanApiScope = "newvoicemedia.com/api/interactionplan";
        private readonly IInvokeRouteService _invokeRouteService;
        private readonly IAccountRepository _accountRepository;

        public NexmoAnswerController(
            IInvokeRouteService invokeRouteService,
            IAccountRepository accountRepository)
        {
            _invokeRouteService = invokeRouteService;
            _accountRepository = accountRepository;
        }


        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ApiError))]
        [Route("")]
        public async Task<IHttpActionResult> NexmoAnswer(AnswerRequest request)
        {
            var account = await _accountRepository.GetAccountAsync("Master");

            await _invokeRouteService.Dispatch(
                new Invocation
                {
                    ProviderName = "NexmoC",
                    Requests = new []
                    {
                        new Interaction
                        {
                            CallGuid = Guid.Parse(request.Uuid),
                            ApiAction = ApiIncomingInteractionAction.dispatch,
                            ExternalId = request.Uuid,
                            RoutePlanIdentifier = "nexmo",
                            LinkedData = new []
                            {
                                new LinkedDataItem
                                {
                                    LinkedDataType = LinkedDataType.Clid,
                                    Value = request.From
                                }
                            }
                        }
                    }
                },
                account);

            return Ok(new NCCO[]
            {
                new Conversation{Name = "conf_" + request.Uuid}
            });
        }
    }
}
