using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using InteractionPlanApi.Auth;
using InteractionPlanApi.Request;
using InteractionPlanApi.Request.NCCO;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using NewVoiceMedia.AspNet.WebApi.Models;
using NewVoiceMedia.CallCentre.Model;
using NewVoiceMedia.Services.CallCentre;
using NSwag.Annotations;

namespace InteractionPlanApi.Controllers
{
    [RoutePrefix("nexmocallback")]
    [AllowAnonymous]
    [ApiVersion("1")]
    public class NexmoCallbackController : ApiController
    {
        private readonly NccoQueue _nccoQueue;
        private readonly ICallChannelService _callChannelService;
        private readonly IAgentRepository _agentRepository;
        private readonly IActiveCallRepository _activeCallRepository;
        internal const string InteractionPlanApiScope = "newvoicemedia.com/api/interactionplan";

        public NexmoCallbackController(NccoQueue nccoQueue,
             ICallChannelService callChannelService,
             IAgentRepository agentRepository,
             IActiveCallRepository activeCallRepository)
        {
            _nccoQueue = nccoQueue;
            _callChannelService = callChannelService;
            _agentRepository = agentRepository;
            _activeCallRepository = activeCallRepository;
        }

        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ApiError))]
        [Route("")]
        public async Task<IHttpActionResult> NexmoCallback(ICollection<NotificationRequest> requests)
        {
            foreach (var request in requests)
            {
                await ProcessRequest(request);
            }

            return Ok();
        }

        private async Task ProcessRequest(NotificationRequest request)
        {
            Debug.WriteLine($"NexmoCallback {request.ApiProviderNotification}");

            switch (request.ApiProviderNotification)
            {
                case NewVoiceMedia.CallCentre.Model.InteractionPlan.Api.ApiProviderNotification.PlayMessage:
                    if (!string.IsNullOrEmpty(request.Parameters["PathToMedia"]))
                    {
                        await PostNcco(
                            false,
                            request.ExternalId,
                            new NCCO[]
                            {
                                new AudioStream
                                {
                                    StreamUrl = new []{$"https://www.cmcardle75.co.uk:65431{request.Parameters["PathToMedia"]}"}
                                }
                            });
                    }
                    else
                    {
                        await PostNcco(
                            false,
                            request.ExternalId,
                            new NCCO[]
                            {
                                new Talk
                                {
                                    Text = request.Parameters["Text"]
                                }
                            });
                    }
                    break;

                case NewVoiceMedia.CallCentre.Model.InteractionPlan.Api.ApiProviderNotification.AssignToAgent:
                    var postCall = new PostCall
                    {
                        To = new[] { new PostCallNumber { Number = request.Parameters["AgentPhone"] } },
                        From = new PostCallNumber { Number = "442039051225" },
                        AnswerCallUrl = new[] { $"https://www.cmcardle75.co.uk:65432/nexmoanswerattach?uuid={request.ExternalId}" }
                    };
                    var body = JsonConvert.SerializeObject(postCall);
                    var message = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = new StringContent(body),
                        RequestUri = new System.Uri($"https://api.nexmo.com/v1/calls")
                    };
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Jwt.BearerToken);
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await new HttpClient().SendAsync(message);
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CreateLegResult>(data);
                    var agent = _agentRepository.GetAgent("Master", request.Parameters["AgentId"], AgentResponsibilities.None);
                    var activeCall = _activeCallRepository.GetByExternalId(1, 3, request.ExternalId, ExternalIdType.Api);

                    var channel = _callChannelService.GetByCallGuidAndAgentUid(activeCall.Guid, agent.AgentUid, true);
                    channel.ConnectionGuid = Guid.Parse(result.Uuid);
                    _callChannelService.SaveOrUpdate(channel);
                    break;
            }
        }

        private async Task<string> PostNcco(bool immediate, string externalId, ICollection<NCCO> nccos)
        {
            if (immediate)
            {
                _nccoQueue.Clear(externalId);
            }

            foreach (var ncco in nccos)
            {
                _nccoQueue.AddToQueue(externalId, ncco);
            }

            if (immediate || !_nccoQueue.IsCurrentlyExecuting(externalId))
            {
                var putNcco = new PutNcco
                {
                    Destination = new Destination
                    {
                        Url = new List<string> { $"https://www.cmcardle75.co.uk:65432/nexmogetnccos?uuid={externalId}" }
                    }
                };

                var body = JsonConvert.SerializeObject(putNcco);
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    Content = new StringContent(body),
                    RequestUri = new System.Uri($"https://api.nexmo.com/v1/calls/{externalId}")
                };
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Jwt.BearerToken);
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await new HttpClient().SendAsync(message);
                var data = await response.Content.ReadAsStringAsync();
                return data;
            }

            return string.Empty;
        }
    }

    public class PostCall
    {
        [JsonProperty("to")]
        public ICollection<PostCallNumber> To { get; set; }

        [JsonProperty("from")]
        public PostCallNumber From { get; set; }

        [JsonProperty("answer_url")]
        public ICollection<string> AnswerCallUrl { get; set; }

        [JsonProperty("answer-method")]
        public string AnswerMethod => "GET";
    }

    public class PostCallNumber
    {
        [JsonProperty("type")]
        public string Type => "phone";

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public class PutNcco
    {
        [JsonProperty("action")]
        public string Action => "transfer";

        [JsonProperty("destination")]
        public Destination Destination { get; set; }
    }

    public class HangUp
    {
        [JsonProperty("action")]
        public string Action => "hangup";
    }

    public class Destination
    {
        [JsonProperty("type")]
        public string Type => "ncco";

        [JsonProperty("url")]
        public List<string> Url { get; set; } = new List<string>();

    }

    public class CreateLegResult
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("conversation_uuid")]
        public string ConversationUuid { get; set; }
    }
}
