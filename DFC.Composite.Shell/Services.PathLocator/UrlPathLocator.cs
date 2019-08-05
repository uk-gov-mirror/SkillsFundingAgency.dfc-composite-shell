﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace DFC.Composite.Shell.Services.PathLocator
{
    public class UrlPathLocator : IPathLocator
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UrlPathLocator> logger;

        public UrlPathLocator(IHttpContextAccessor httpContextAccessor, ILogger<UrlPathLocator> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public string GetPath()
        {
            var result = _httpContextAccessor.HttpContext.Request.Path.Value.Trim();
            if (result.StartsWith("/"))
            {
                result = result.Substring(1);
            }
            var forwardSlashPosition = result.IndexOf("/");
            if (forwardSlashPosition != -1)
            {
                result = result.Substring(0, forwardSlashPosition);
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.ToLower();
            }
            _logger.LogDebug($"PathLocator. Request.Path is {_httpContextAccessor.HttpContext.Request.Path.Value} and located path is {result}");
            return result;
        }
    }
}