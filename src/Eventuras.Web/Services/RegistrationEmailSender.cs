using Eventuras.Services;
using Eventuras.ViewModels;
using Losol.Communication.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Eventuras.Domain.Registration;

namespace Eventuras.Web.Services
{
    public sealed class RegistrationEmailSender : ApplicationEmailSender
    {
        protected override string Template => "Templates/Email/Registration";
        private readonly IRegistrationService _registrationService;
        private readonly IUrlHelper _urlHelper;
        private readonly string _requestScheme;
        private readonly ILogger<RegistrationEmailSender> _logger;


        public RegistrationEmailSender(
                IEmailSender emailSender,
                IRenderService renderService,
                IUrlHelperFactory urlHelperFactory,
                IActionContextAccessor actionContextAccessor,
                IRegistrationService registrationService,
                ILogger<RegistrationEmailSender> logger)
            : base(emailSender, renderService)
        {
            _registrationService = registrationService;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext); ;
            _requestScheme = actionContextAccessor.ActionContext.HttpContext.Request.Scheme;
            _logger = logger;
        }


        public async Task SendRegistrationAsync(string emailAddress, string subject, string message, int registrationId)
        {

            var registration = await _registrationService.GetWithUserAndEventInfoAndOrders(registrationId);

            var eventRegistration = new EventRegistration
            {
                ParticipantName = registration.ParticipantName,
                EventInfo = registration.EventInfo,
                Orders = registration.Orders,
                Email = registration.User.Email,
                Verified = (registration.Status != RegistrationStatus.Draft) ? true : false,
                HasOrder = registration.Orders.Count > 0,
                Message = message
            };

            eventRegistration.VerificationUrl = _urlHelper.Page(
                    pageName: "/Events/Register/Confirm",
                    pageHandler: null,
                    values: new
                    {
                        id = registration.RegistrationId,
                        auth = registration.VerificationCode
                    },
                    protocol: _requestScheme
                );

            eventRegistration.EventUrl = _urlHelper.Page(
                    pageName: "/Events/Details",
                    pageHandler: null,
                    values: new
                    {
                        id = registration.EventInfoId
                    },
                    protocol: _requestScheme
                );

            _logger.LogInformation("RegistrationEmailSender: Sending registration email for registrationId {registrationId}", registrationId);
            await SendAsync(emailAddress, subject, eventRegistration);
        }
    }
}