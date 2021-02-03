//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Update
{
    sealed class ReleasesClient
    {
        // This is a public access token, meaning it can only access information anyone can already access in a browser.
        // But using this token instead of no authorization increases the API rate limit greatly.
        private const string Token = "0bac4fa86ff998683273de92714fb82be45ffe6b";

        private readonly GitHubClient _client;

        public ReleasesClient()
        {
            var product = new ProductHeaderValue("ModMyFactory2", VersionStatistics.AppVersion.ToString());
            _client = new GitHubClient(product);

            var auth = new Credentials(Token, AuthenticationType.Oauth);
            _client.Credentials = auth;
        }

        private bool CheckRateLimit()
        {
            var info = _client.GetLastApiInfo();
            if (info is null) return true;

            var limit = info.RateLimit;
            return (limit.Remaining > 0) || (limit.Reset < DateTimeOffset.UtcNow);
        }

        public async Task<(bool, IReadOnlyList<Release>?)> TryRequestReleasesAsync()
        {
            if (!CheckRateLimit()) return (false, null);

            try
            {
                var releases = await _client.Repository.Release.GetAll("Artentus", "ModMyFactory2");
                return (true, releases);
            }
            catch (ApiException ex)
            {
                Log.Warning(ex, "Error requesting update information");
                return (false, null);
            }
        }
    }
}
