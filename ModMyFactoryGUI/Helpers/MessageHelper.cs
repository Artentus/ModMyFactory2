//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.WebApi;
using System.Net;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class MessageHelper
    {
        public static async Task ShowMessageForApiException(ApiException exception)
        {
            if (exception is ConnectFailureException)
            {
                // Connection error
                await Messages.ConnectionError.Show(exception);
            }
            else if (exception is TimeoutException)
            {
                // Timeout
                await Messages.TimeoutError.Show(exception);
            }
            else
            {
                // Server error
                await Messages.ServerError.Show(exception);
            }
        }

        public static async Task ShowMessageForWebException(WebException exception)
        {
            switch (exception.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                    // Connection error
                    await Messages.ConnectionError.Show(exception);
                    break;
                case WebExceptionStatus.Timeout:
                    // Timeout
                    await Messages.TimeoutError.Show(exception);
                    break;
                default:
                    // Server error
                    await Messages.ServerError.Show(exception);
                    break;
            }
        }
    }
}
