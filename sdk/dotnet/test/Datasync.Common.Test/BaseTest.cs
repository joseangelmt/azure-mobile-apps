﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.Service;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Datasync.Common.Test
{
    /// <summary>
    /// A basic template for a test suite
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class BaseTest
    {
        private readonly Lazy<TestServer> _server = new(() => MovieApiServer.CreateTestServer());
        private readonly ITestOutputHelper logger;

        protected BaseTest(ITestOutputHelper helper)
        {
            logger = helper;
        }

        protected BaseTest()
        {
        }

        /// <summary>
        /// An authentication token that is expired.
        /// </summary>
        protected static readonly AuthenticationToken ExpiredAuthenticationToken = new()
        {
            DisplayName = "John Smith",
            ExpiresOn = DateTimeOffset.Now.AddMinutes(-5),
            Token = "YmFzaWMgdG9rZW4gZm9yIHRlc3Rpbmc=",
            UserId = "the_doctor"
        };

        /// <summary>
        /// A completely valid authentication token.
        /// </summary>
        protected readonly AuthenticationToken ValidAuthenticationToken = new()
        {
            DisplayName = "John Smith",
            ExpiresOn = DateTimeOffset.Now.AddMinutes(5),
            Token = "YmFzaWMgdG9rZW4gZm9yIHRlc3Rpbmc=",
            UserId = "the_doctor"
        };

        /// <summary>
        /// Default endpoint.
        /// </summary>
        protected Uri Endpoint { get; } = new Uri("https://localhost");

        /// <summary>
        /// The mock handler that allows us to set responses and see requests.
        /// </summary>
        protected MockDelegatingHandler MockHandler { get; } = new();

        /// <summary>
        /// The count of the movies in the data set.
        /// </summary>
        protected static int MovieCount { get; } = TestData.Movies.Count;

        /// <summary>
        /// Creates a <see cref="HttpClient"/> to access the movie server
        /// </summary>
        protected HttpClient MovieHttpClient { get => MovieServer.CreateClient(); }

        /// <summary>
        /// Creates a reference to the movie server, when needed.
        /// </summary>
        protected TestServer MovieServer { get => _server.Value; }

        /// <summary>
        /// The start time for the test.
        /// </summary>
        protected DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Creates a paging response.
        /// </summary>
        /// <param name="count">The count of elements to return</param>
        /// <param name="totalCount">The total count</param>
        /// <param name="nextLink">The next link</param>
        /// <returns></returns>
        protected Page<IdEntity> CreatePageOfItems(int count, long? totalCount = null, Uri nextLink = null)
        {
            List<IdEntity> items = new();

            for (int i = 0; i < count; i++)
            {
                items.Add(new IdEntity { Id = Guid.NewGuid().ToString("N") });
            }
            var page = new Page<IdEntity> { Items = items, Count = totalCount, NextLink = nextLink };
            MockHandler.AddResponse(HttpStatusCode.OK, page);
            return page;
        }

        /// <summary>
        /// Get a datasync client that is completely mocked and doesn't require a service
        /// </summary>
        /// <param name="authProvider">... with the provided authentication provider</param>
        /// <returns>A datasync client</returns>
        protected DatasyncClient GetMockClient(AuthenticationProvider authProvider = null)
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { MockHandler }
            };
            return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, authProvider, options);
        }

        /// <summary>
        /// Get a datasync client that is connected to the integration movie service.
        /// </summary>
        /// <param name="authProvider">... with the provided authentication provider</param>
        /// <returns>A datasync client</returns>
        protected DatasyncClient GetMovieClient(AuthenticationProvider authProvider = null)
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { MovieServer.CreateHandler() }
            };
            return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, authProvider, options);
        }

        /// <summary>
        /// Gets the random ID for a movie.
        /// </summary>
        /// <returns></returns>
        protected static string GetRandomId() => TestData.Movies.GetRandomId();

        /// <summary>
        /// We use "Black Panther" as a sample movie throughout the tests.
        /// </summary>
        /// <typeparam name="T">The concrete type of create.</typeparam>
        /// <returns>The sample movie in the type provided</returns>
        protected static T GetSampleMovie<T>() where T : IMovie, new()
        {
            return new T()
            {
                BestPictureWinner = true,
                Duration = 134,
                Rating = "PG-13",
                ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
                Title = "Black Panther",
                Year = 2018
            };
        }

        /// <summary>
        /// Wait until a condition is met - useful for testing async processes.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="ms"></param>
        /// <param name="maxloops"></param>
        /// <returns></returns>
        protected static async Task<bool> WaitUntil(Func<bool> func, int ms = 10, int maxloops = 500)
        {
            int waitCtr = 0;
            do
            {
                waitCtr++;
                await Task.Delay(ms).ConfigureAwait(false);
            } while (!func.Invoke() && waitCtr < maxloops);
            return waitCtr < maxloops;
        }

        /// <summary>
        /// Log the provided response.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="response">The response</param>
        /// <returns></returns>
        protected async Task AssertResponseWithLoggingAsync(HttpStatusCode expectedStatusCode, HttpResponseMessage response)
        {
            if (response.RequestMessage != null)
            {
                logger?.WriteLine($"Request: {response.RequestMessage.RequestUri}");
            }
            if (response.StatusCode != expectedStatusCode)
            {
                logger?.WriteLine($"Response (expected: {expectedStatusCode}): {response.StatusCode} {response.ReasonPhrase}");
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                logger?.WriteLine($"Content: {content}");
            }
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
