//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi
{
    public static class Authentication
    {
        private const string BaseUrl = "https://auth.factorio.com";
        private const string LogInUrl = BaseUrl + "/api-login";

        /// <summary>
        /// Logs into the API to receive a token. The token can be used to authenticate to other API endpoints.
        /// </summary>
        /// <param name="username">The username to log in.</param>
        /// <param name="password">The password to log in.</param>
        public async static Task<(string username, string token)> LogInAsync(string username, string password)
        {
            string username_enc = WebUtility.UrlEncode(username);
            string password_enc = WebUtility.UrlEncode(password);
            string contentString = $"api_version=2&require_game_ownership=true&username={username_enc}&password={password_enc}";
            var content = Encoding.UTF8.GetBytes(contentString);

            try
            {
                string document = await WebHelper.RequestDocumentAsync(LogInUrl, content);
                dynamic response = JsonConvert.DeserializeObject(document)!;
                return (response.username, response.token);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
        }
    }
}
