﻿using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ApplicationController : Controller
    {
        private const string MainRenderViewName = "Application/RenderView";

        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly ILogger<ApplicationController> logger;
        private readonly IApplicationService applicationService;
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;
        private readonly IBaseUrlService baseUrlService;

        public ApplicationController(
            IMapper<ApplicationModel, PageViewModel> mapper,
            ILogger<ApplicationController> logger,
            IApplicationService applicationService,
            IVersionedFiles versionedFiles,
            IConfiguration configuration,
            IBaseUrlService baseUrlService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.applicationService = applicationService;
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
            this.baseUrlService = baseUrlService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            try
            {
                if (requestViewModel != null)
                {
                    logger.LogInformation($"{nameof(Action)}: Getting child response for: {requestViewModel.Path}");

                    var application = await applicationService.GetApplicationAsync(requestViewModel.Path).ConfigureAwait(false);

                    if (application?.Path == null)
                    {
                        var errorString = $"The path {requestViewModel.Path} is not registered";

                        logger.LogWarning($"{nameof(Action)}: {errorString}");

                        var redirectTo = new Uri($"/alert/{(int)HttpStatusCode.NotFound}", UriKind.Relative);

                        throw new RedirectException(new Uri($"/{requestViewModel.Path}/{requestViewModel.Data}", UriKind.Relative), redirectTo, false);
                    }
                    else if (!string.IsNullOrWhiteSpace(application.Path.ExternalURL))
                    {
                        logger.LogInformation($"{nameof(Action)}: Redirecting to external for: {requestViewModel.Path}");

                        return Redirect(application.Path.ExternalURL);
                    }
                    else
                    {
                        mapper.Map(application, viewModel);

                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                        await applicationService.GetMarkupAsync(application, requestViewModel.Data + Request.QueryString, viewModel).ConfigureAwait(false);

                        logger.LogInformation(
                            $"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                    }
                }
            }
            catch (RedirectException ex)
            {
                string redirectTo = ex.Location?.OriginalString;

                logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.ToString()} to: {redirectTo}");

                Response.Redirect(redirectTo, ex.IsPermenant);
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Action(ActionPostRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            try
            {
                if (requestViewModel != null)
                {
                    logger.LogInformation($"{nameof(Action)}: Posting child request for: {requestViewModel.Path}");

                    var application = await applicationService.GetApplicationAsync(requestViewModel.Path).ConfigureAwait(false);

                    if (application?.Path == null)
                    {
                        var errorString = $"The path {requestViewModel.Path} is not registered";

                        logger.LogWarning($"{nameof(Action)}: {errorString}");

                        var redirectTo = new Uri($"/alert/{(int)HttpStatusCode.NotFound}", UriKind.Relative);

                        throw new RedirectException(new Uri($"/{requestViewModel.Path}/{requestViewModel.Data}", UriKind.Relative), redirectTo, false);
                    }
                    else
                    {
                        mapper.Map(application, viewModel);

                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                        var formParameters = (from a in requestViewModel.FormCollection
                                              select new KeyValuePair<string, string>(a.Key, a.Value)).ToArray();

                        await applicationService.PostMarkupAsync(application, requestViewModel.Path, requestViewModel.Data, formParameters, viewModel).ConfigureAwait(false);

                        logger.LogInformation($"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                    }
                }
            }
            catch (RedirectException ex)
            {
                string redirectTo = ex.Location?.OriginalString;

                logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.ToString()} to: {redirectTo}");

                Response.Redirect(redirectTo, ex.IsPermenant);
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        private static PageViewModelResponse Map(PageViewModel source)
        {
            var result = new PageViewModelResponse
            {
                BrandingAssetsCdn = source.BrandingAssetsCdn,
                LayoutName = source.LayoutName,
                PageTitle = source.PageTitle,
                Path = source.Path,
                PhaseBannerHtml = source.PhaseBannerHtml,

                VersionedPathForMainMinCss = source.VersionedPathForMainMinCss,
                VersionedPathForGovukMinCss = source.VersionedPathForGovukMinCss,
                VersionedPathForAllIe8Css = source.VersionedPathForAllIe8Css,
                VersionedPathForJQueryBundleMinJs = source.VersionedPathForJQueryBundleMinJs,
                VersionedPathForAllMinJs = source.VersionedPathForAllMinJs,
                VersionedPathForDfcDigitalMinJs = source.VersionedPathForDfcDigitalMinJs,

                ContentBody = GetContent(source, PageRegion.Body),
                ContentBodyFooter = GetContent(source, PageRegion.BodyFooter),
                ContentBodyTop = GetContent(source, PageRegion.BodyTop),
                ContentHeroBanner = GetContent(source, PageRegion.HeroBanner),
                ContentBreadcrumb = GetContent(source, PageRegion.Breadcrumb),
                ContentHead = GetContent(source, PageRegion.Head),
                ContentSidebarLeft = GetContent(source, PageRegion.SidebarLeft),
                ContentSidebarRight = GetContent(source, PageRegion.SidebarRight),
            };

            return result;
        }

        private static HtmlString GetContent(PageViewModel pageViewModel, PageRegion pageRegionType)
        {
            var result = string.Empty;
            var pageRegionContentModel = pageViewModel.PageRegionContentModels.FirstOrDefault(x => x.PageRegionType == pageRegionType);
            if (pageRegionContentModel != null && pageRegionContentModel.Content != null)
            {
                result = pageRegionContentModel.Content.Value;
            }

            return new HtmlString(result);
        }
    }
}