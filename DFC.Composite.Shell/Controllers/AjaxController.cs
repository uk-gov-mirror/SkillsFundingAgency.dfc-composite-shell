using DFC.Composite.Shell.Models.AjaxApiModels;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.AppRegistry;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AjaxController : ControllerBase
    {
        private readonly IAjaxRequestService ajaxRequestService;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly ILogger<AjaxController> logger;

        public AjaxController(IAjaxRequestService ajaxRequestService, IAppRegistryDataService appRegistryDataService, ILogger<AjaxController> logger)
        {
            this.ajaxRequestService = ajaxRequestService;
            this.appRegistryDataService = appRegistryDataService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("Action")]
        public async Task<IActionResult> Action([FromQuery] RequestModel? requestModel)
        {
            logger.LogInformation("Ajax Action method has been called");
            if (string.IsNullOrWhiteSpace(requestModel?.Path) || string.IsNullOrWhiteSpace(requestModel?.Method))
            {
                return BadRequest();
            }

            logger.LogInformation("Ajax Action method has been called with request data. Path:{Path}, Method:{Method} and AppData:{AppData} ", requestModel.Path, requestModel.Method, requestModel.AppData);

            logger.LogInformation("Attempting to retrieve App registration model for Path:{Path}", requestModel.Path);
            var appRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(requestModel.Path);

            var ajaxRequest = appRegistrationModel?.AjaxRequests.FirstOrDefault(f => string.Compare(f.Name, requestModel.Method, StringComparison.OrdinalIgnoreCase) == 0);
            if (string.IsNullOrWhiteSpace(ajaxRequest?.AjaxEndpoint))
            {
                logger.LogWarning("Ajax endpoint not found. AjaxEndpoint:{AjaxEndpoint}", ajaxRequest?.AjaxEndpoint);
                return NotFound();
            }

            logger.LogInformation("App registration model retrieved with IsHealthy property as {IsHealthy}", ajaxRequest?.IsHealthy);

            logger.LogInformation("Attempting to get response for ajax request with Path:{Path}, Method:{Method} and AppData:{AppData} ", requestModel.Path, requestModel.Method, requestModel.AppData);
            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            var statusCode = (int)result.Status;
            logger.LogInformation("Retrieved response with status code:{StatusCode}", statusCode);

            return new ObjectResult(result) { StatusCode = (int)result.Status };
        }
    }
}