﻿using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Utilities
{
    public class VersionedFiles : IVersionedFiles
    {
        public VersionedFiles(IConfiguration configuration, IAssetLocationAndVersionService assetLocationAndVersionService)
        {
            var brandingAssetsCdn = configuration.GetValue<string>("BrandingAssetsCdn");
            var brandingAssetsFolder = $"{brandingAssetsCdn}/gds_service_toolkit";

            VersionedPathForMainMinCss = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/all.min.css");
            VersionedPathForGovukMinCss = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/govuk.min.css");
            VersionedPathForAllIe8Css = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/all-ie8.css");
            VersionedPathForSiteCss = assetLocationAndVersionService?.GetLocalAssetFileAndVersion("css/site.css");

            VersionedPathForJQueryBundleMinJs = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/js/jquerybundle.min.js");
            VersionedPathForAllMinJs = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/js/all.min.js");
            VersionedPathForSiteJs = assetLocationAndVersionService?.GetLocalAssetFileAndVersion("js/site.js");
        }

        public string VersionedPathForMainMinCss { get; }

        public string VersionedPathForGovukMinCss { get; }

        public string VersionedPathForAllIe8Css { get; }

        public string VersionedPathForSiteCss { get; }

        public string VersionedPathForJQueryBundleMinJs { get; }

        public string VersionedPathForAllMinJs { get; }

        public string VersionedPathForSiteJs { get; }
    }
}
