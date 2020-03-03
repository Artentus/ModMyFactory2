//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactory.WebApi
{
    public static class Authentication
    {
        private const string BaseUrl = "https://auth.factorio.com";
        private const string LogInUrl = BaseUrl + "/api-login";

        private static void DestroyByteArray(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = 0;
        }

        /// <summary>
        /// Logs into the API to receive a token. The token can be used to authenticate to other API endpoints.
        /// </summary>
        /// <param name="username">The username to log in.</param>
        /// <param name="password">The password to log in.</param>
        public async static Task<(string username, string token)> LogInAsync(string username, SecureString password)
        {
            string contentString = $"api_version=2&require_game_ownership=true&username={username}&password=";
            var contentBytes = Encoding.UTF8.GetBytes(contentString);
            var pwBytes = password.ToBytes();

            var content = new byte[contentBytes.Length + pwBytes.Length];
            contentBytes.CopyTo(content, 0);
            pwBytes.CopyTo(content, contentBytes.Length);

            try
            {
                string document = await WebHelper.RequestDocumentAsync(LogInUrl, content);
                dynamic response = JsonConvert.DeserializeObject(document);
                return (response.username, response.token);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
            finally
            {
                DestroyByteArray(content);
            }
        }

        /// <summary>
        /// Logs into the API to receive a token. The token can be used to authenticate to other API endpoints.
        /// </summary>
        /// <param name="username">The username to log in.</param>
        /// <param name="password">The password to log in.</param>
        public async static Task<(string username, string token)> LogInAsync(string username, string password)
        {
            string contentString = $"api_version=2&require_game_ownership=true&username={username}&password={password}";
            var content = Encoding.UTF8.GetBytes(contentString);

            try
            {
                string document = await WebHelper.RequestDocumentAsync(LogInUrl, content);
                dynamic response = JsonConvert.DeserializeObject(document);
                return (response.username, response.token);
            }
            catch (WebException ex)
            {
                throw ApiException.FromWebException(ex);
            }
            finally
            {
                DestroyByteArray(content);
            }
        }
    }
}
