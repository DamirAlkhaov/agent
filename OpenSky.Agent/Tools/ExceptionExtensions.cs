﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Tools
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;

    using Newtonsoft.Json;

    using OpenSky.Agent.Controls;
    using OpenSky.Agent.Controls.Models;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.OpenAPIs;
    using OpenSky.Agent.Views;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Exception extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class ExceptionExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handle exception being thrown as a result of an OpenSky API call.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <param name="ex">
        /// The exception to act on.
        /// </param>
        /// <param name="window">
        /// The OpenSky window where the exception originated (to show the custom notifications inside that window).
        /// </param>
        /// <param name="command">
        /// The asynchronous command executing the API call.
        /// </param>
        /// <param name="friendlyErrorMessage">
        /// Friendly error messages describing what we were trying to do.
        /// </param>
        /// <param name="alert401">
        /// (Optional) True to alert about HTTP 401 (unauthorized) errors - letting the user know to
        /// login again.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void HandleApiCallException(this Exception ex, OpenSkyWindow window, AsynchronousCommand command, string friendlyErrorMessage, bool alert401 = true)
        {
            if (ex is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    innerException.HandleApiCallException(window, command, friendlyErrorMessage, alert401);
                }
            }
            else if (ex is ApiException apiException)
            {
                if (apiException.StatusCode == 401)
                {
                    Debug.WriteLine("Forcing token refresh due to 401 response from OpenSky API server.");
                    var result = UserSessionService.Instance.ForceTokenRefresh().Result;
                    if (result)
                    {
                        if (alert401)
                        {
                            command.ReportProgress(
                                () =>
                                {
                                    Debug.WriteLine($"{friendlyErrorMessage}: {ex.Message}");
                                    window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = friendlyErrorMessage, Exception = ex }, "Authorization", "Authorization token was expired, please try again.", ExtendedMessageBoxImage.Error, 30));
                                });
                        }
                    }
                    else
                    {
                        if (alert401)
                        {
                            command.ReportProgress(
                                () =>
                                {
                                    Debug.WriteLine($"{friendlyErrorMessage}: {ex.Message}");
                                    window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = friendlyErrorMessage, Exception = ex }, "Authorization", "Authorization token is invalid, please login with your OpenSky account again.", ExtendedMessageBoxImage.Error, 30));
                                });
                        }

                        command.ReportProgress(() => LoginNotification.Open());
                    }
                }
                else if (!string.IsNullOrEmpty(apiException.Response))
                {
                    try
                    {
                        var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(apiException.Response);
                        if (problemDetails != null)
                        {
                            foreach (var problemDetailsError in problemDetails.Errors)
                            {
                                foreach (var errorMessage in problemDetailsError.Value)
                                {
                                    command.ReportProgress(
                                        () =>
                                        {
                                            Debug.WriteLine($"{friendlyErrorMessage}: {errorMessage}");
                                            window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = errorMessage, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                                        });
                                }
                            }
                        }
                        else
                        {
                            command.ReportProgress(
                                () =>
                                {
                                    Debug.WriteLine($"{friendlyErrorMessage}: {apiException.Message}");
                                    window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = apiException.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                                });
                        }
                    }
                    catch
                    {
                        command.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine($"{friendlyErrorMessage}: {apiException.Response}");
                                window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = new string(apiException.Response.Take(500).ToArray()), Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                            });
                    }
                }
                else
                {
                    command.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine($"{friendlyErrorMessage}: {apiException.Message}");
                            window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = apiException.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                        });
                }
            }
            else if (ex is HttpRequestException httpRequestException)
            {
                if (httpRequestException.InnerException != null)
                {
                    httpRequestException.InnerException.HandleApiCallException(window, command, friendlyErrorMessage, alert401);
                }
                else
                {
                    command.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine($"{friendlyErrorMessage}: {httpRequestException.Message}");
                            window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = httpRequestException.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                        });
                }
            }
            else if (ex is WebException webException)
            {
                Debug.WriteLine(webException.Message);
                if (webException.InnerException != null)
                {
                    webException.InnerException.HandleApiCallException(window, command, friendlyErrorMessage, alert401);
                }
                else
                {
                    command.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine($"{friendlyErrorMessage}: {webException.Message}");
                            window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = webException.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));

                        });
                }
            }
            else if (ex is SocketException socketException)
            {
                command.ReportProgress(
                    () =>
                    {
                        Debug.WriteLine($"{friendlyErrorMessage}: {socketException.Message}");
                        window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = socketException.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));

                    });
            }
            else
            {
                command.ReportProgress(
                    () =>
                    {
                        Debug.WriteLine($"{friendlyErrorMessage}: {ex.Message}");
                        window.ShowNotification(new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error", friendlyErrorMessage, ExtendedMessageBoxImage.Error, 30));
                    });
            }
        }
    }
}