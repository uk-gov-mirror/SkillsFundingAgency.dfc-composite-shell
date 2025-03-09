using DFC.Composite.Shell.Models.AjaxApiModels;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;

using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AjaxRequest
{
    public class AjaxRequestService : IAjaxRequestService
    {
        private readonly ILogger<AjaxRequestService> logger;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly HttpClient httpClient;

        public AjaxRequestService(ILogger<AjaxRequestService> logger, IAppRegistryDataService appRegistryDataService, HttpClient httpClient)
        {
            this.logger = logger;
            this.appRegistryDataService = appRegistryDataService;
            this.httpClient = httpClient;
        }

        public async Task<ResponseModel> GetResponseAsync(RequestModel requestModel, AjaxRequestModel ajaxRequest)
        {
            _ = requestModel ?? throw new ArgumentNullException(nameof(requestModel));
            _ = ajaxRequest ?? throw new ArgumentNullException(nameof(ajaxRequest));

            logger.LogInformation("Retrieving response for ajax request with Path:{Path}, Method:{Method} and AppData:{AppData} ", requestModel.Path, requestModel.Method, requestModel.AppData);

            var appData = string.IsNullOrWhiteSpace(requestModel.AppData) ? string.Empty : "/" + Uri.EscapeDataString(requestModel.AppData);
            var url = ajaxRequest.AjaxEndpoint.Replace("/{0}", $"{appData}", StringComparison.OrdinalIgnoreCase);
            var responseModel = new ResponseModel
            {
                Status = HttpStatusCode.OK,
                StatusMessage = "Unhealthy",
                IsHealthy = ajaxRequest.IsHealthy,
                OfflineHtml = ajaxRequest.OfflineHtml,
            };

            logger.LogInformation($"Ajax request using: {url}");

            if (ajaxRequest.IsHealthy)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

                    var response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));

                    responseModel.Status = response.StatusCode;
                    responseModel.StatusMessage = response.ReasonPhrase;

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation($"Ajax request successful: {url}");
                        responseModel.Payload = await response.Content.ReadAsStringAsync();
                        responseModel.OfflineHtml = null;
                    }
                    else
                    {
                        logger.LogError($"Ajax request error: {responseModel.Status}: {responseModel.StatusMessage}, using: {url}");
                    }
                }
                catch (TaskCanceledException ex)
                {
                    //logger.LogError(ex, $"TaskCancelled: {url} - {ex.Message}");

                    logger.LogError(ex, "TaskCancelled. Url:{Url} - Message:{Message}", url, ex.Message);

                    logger.LogInformation("TaskCancelled. Url:{Url} - Message:{Message}", url, ex.Message);

                    responseModel.IsHealthy = false;

                    if (ajaxRequest.HealthCheckRequired)
                    {
                        logger.LogInformation("Attempting to set IsHealthy property to false for Ajax request.");
                        await appRegistryDataService.SetAjaxRequestHealthState(requestModel.Path, ajaxRequest.Name, false);
                        logger.LogInformation("IsHealthy property is set to false for Ajax request.");
                    }
                }
            }

            logger.LogInformation("Returning response with Status Code:{StatusCode} and IsHeathy:{IsHealthy} ", responseModel.Status, responseModel.IsHealthy);

            return responseModel;
        }
    }
}
