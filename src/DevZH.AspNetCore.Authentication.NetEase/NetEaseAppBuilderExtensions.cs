﻿using System;
using DevZH.AspNetCore.Authentication.NetEase;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace DevZH.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add NetEase authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class NetEaseAppBuilderExtensions
    {
        /// <summary>
		/// Adds the <see cref="NetEaseMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables NetEase authentication capabilities.
		/// </summary>
		/// <param name="app">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
		/// <param name="options">A <see cref="NetEaseOptions"/> that specifies options for the middleware.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseNetEaseAuthentication(this IApplicationBuilder app, NetEaseOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<NetEaseMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Adds the <see cref="NetEaseMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables NetEase authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseNetEaseAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            return app.UseMiddleware<NetEaseMiddleware>();
        }
    }
}
