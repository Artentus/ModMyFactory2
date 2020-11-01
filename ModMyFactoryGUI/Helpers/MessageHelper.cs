//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class MessageHelper
    {
        public static Task ShowMessageForApiException(ApiException exception)
        {
            if (exception is ConnectFailureException)
            {
                // Connection error
                return Messages.ConnectionError.Show(exception);
            }
            else if (exception is TimeoutException)
            {
                // Timeout
                return Messages.TimeoutError.Show(exception);
            }
            else
            {
                // Server error
                return Messages.ServerError.Show(exception);
            }
        }

        public static Task ShowMessageForWebException(WebException exception)
        {
            switch (exception.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.SecureChannelFailure:
                    // Connection error
                    return Messages.ConnectionError.Show(exception);
                case WebExceptionStatus.Timeout:
                    // Timeout
                    return Messages.TimeoutError.Show(exception);
                default:
                    // Server error
                    return Messages.ServerError.Show(exception);
            }
        }

        public static Task ShowMessageForHttpException(HttpRequestException exception)
        {
            if (exception.InnerException is WebException inner)
            {
                // Handle inner exception instead
                return ShowMessageForWebException(inner);
            }
            else
            {
                // Server error
                return Messages.ServerError.Show(exception);
            }
        }
    }
}
