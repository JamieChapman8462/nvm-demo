using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using InteractionPlanApi.Auth;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using NewVoiceMedia.AspNet.WebApi.Models;
using NewVoiceMedia.CallCentre.Core.Events;
using NewVoiceMedia.CallCentre.Model;
using NewVoiceMedia.CallCentre.Model.Entities;
using NewVoiceMedia.CallCentre.ToggleService;
using NewVoiceMedia.Services.CallCentre;
using NSwag.Annotations;
using Argument = NewVoiceMedia.Services.EventWebService.Argument;

namespace InteractionPlanApi.Controllers
{
    [RoutePrefix("nexmoevent")]
    [KongAuthentication(InteractionPlanApiScope)]
    [AllowAnonymous]
    [ApiVersion("1")]
    public class NexmoEventController : ApiController
    {
        private readonly IEventServiceHandler _eventServiceHandler;
        private readonly ICallChannelService _callChannelService;
        private readonly IActiveCallRepository _activeCallRepository;

        public NexmoEventController(IEventServiceHandler eventServiceHandler,
            ICallChannelService callChannelService,
            IActiveCallRepository activeCallRepository)
        {
            _eventServiceHandler = eventServiceHandler;
            _callChannelService = callChannelService;
            _activeCallRepository = activeCallRepository;
        }

        internal const string InteractionPlanApiScope = "newvoicemedia.com/api/interactionplan";
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(ApiError))]
        [Route("")]
        public async Task<IHttpActionResult> NexmoEvent(EventRequest request)
        {
            Debug.WriteLine($"Nexmo event: {request.Status} {request.From} {request.To}");
            switch (request.Status)
            {
                case "started":
                    break;

                case "completed":
                    var channel = _callChannelService.GetByConnectionGuid(Guid.Parse(request.Uuid));
                    var activeCall = _activeCallRepository.GetByCallGuid(channel.CallGuid);
                    foreach (var channelToStop in _callChannelService.GetByCallGuid(activeCall.Guid).Where(a => a.ConnectionGuid != channel.ConnectionGuid))
                    {
                        await KillChannel(channelToStop);
                    }

                    var arguments = new List<Argument>
                    {
                        new Argument { Name = "ConnectionGuid", Value = request.Uuid },
                        new Argument { Name = "CallId", Value = "0" },
                        new Argument { Name = "Reason", Value = ConnectionEndedReason.Normal.ToString() },
                        new Argument { Name = "UtcTimestamp", Value = TimestampDateTimeConverter.FromDateTime(DateTime.UtcNow).ToTimestamp() }
                    };
                    _eventServiceHandler.Notify(NewVoiceMedia.Services.EventWebService.EventType.ConnectionDisconnected, arguments.ToArray());

                    break;
            }

            return Ok();
        }

        private async Task KillChannel(ICallChannel channelToStop)
        {
            var hangUp = new HangUp();
            var body = JsonConvert.SerializeObject(hangUp);
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                Content = new StringContent(body),
                RequestUri = new System.Uri($"https://api.nexmo.com/v1/calls/{channelToStop.ConnectionGuid}")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Jwt.BearerToken);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await new HttpClient().SendAsync(message);
            var data = await response.Content.ReadAsStringAsync();

        }
    }

    public class EventRequest
    {
        [JsonProperty("from")]
        public string From {get; set;}

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("conversation_uuid")]
        public string ConversationUuid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
