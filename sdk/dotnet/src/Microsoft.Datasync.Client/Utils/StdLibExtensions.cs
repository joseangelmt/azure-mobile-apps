﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Extensions to the standard library
    /// </summary>
    internal static class StdLibExtensions
    {
        /// <summary>
        /// Normalize an endpoint by removing any query and fragment, then ensuring that the
        /// path has a trailing slash.
        /// </summary>
        /// <param name="endpoint">The endpoint to normalize.</param>
        /// <returns>The normalized endpoint.</returns>
        internal static Uri NormalizeEndpoint(this Uri endpoint)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));

            var builder = new UriBuilder(endpoint) { Query = string.Empty, Fragment = string.Empty };
            builder.Path = builder.Path.TrimEnd('/') + "/";
            return builder.Uri;
        }

        /// <summary>
        /// Sets the query parameters of a Uri.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <param name="queryString">the query to set</param>
        /// <returns>The updated <see cref="UriBuilder"/></returns>
        internal static UriBuilder WithQuery(this UriBuilder builder, string queryString)
        {
            builder.Query = string.IsNullOrWhiteSpace(queryString) ? string.Empty : queryString.Trim();
            return builder;
        }
    }
}
