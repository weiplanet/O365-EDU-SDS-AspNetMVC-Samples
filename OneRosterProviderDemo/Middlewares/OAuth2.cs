/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using OneRosterProviderDemo.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace OneRosterProviderDemo.Middlewares
{
    public class OAuth2
    {
        private const string OAUTH_CONSUMER_KEY = "contoso";
        private const string OAUTH_CONSUMER_SECRET = "contoso-secret";

        private readonly RequestDelegate _next;

        public OAuth2(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApiContext db)
        {
            if (!context.Request.Path.StartsWithSegments("/ims/oneroster"))
            {
                Trace.TraceInformation($"Non-OneRoster route; bypassing oauth");
                await _next(context);
                return;
            }
            Trace.TraceInformation($"Checking oauth for path {context.Request.Path}");
            int validationResult = Verify(context, db);
            if(validationResult == 0)
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = validationResult;
                await context.Response.WriteAsync("");
            }
        }

        #region Parameter Parsing

        private KeyValuePair<string, string> ParseHeaderFragment(string pair)
        {
            var pairArray = pair.Split(" ");
            return new KeyValuePair<string, string>(
                Uri.EscapeDataString(pairArray[0]),
                Uri.EscapeDataString(pairArray[1])
            );
        }

        private List<KeyValuePair<string, string>> getHeaderParams(HttpRequest request)
        {
            var authHeaders = request.Headers.GetCommaSeparatedValues("Authorization");

            var pairs = new List<KeyValuePair<string, string>>();

            foreach (var authHeader in authHeaders)
            {
                var kvp = ParseHeaderFragment(authHeader);
                if (kvp.Key != "realm")
                {
                    pairs.Add(new KeyValuePair<string, string>(kvp.Key, Uri.UnescapeDataString(kvp.Value)));
                }
            }

            return pairs;
        }

        private List<KeyValuePair<string, string>> getQueryParams(HttpRequest request)
        {
            var pairs = new List<KeyValuePair<string, string>>();

            var queryValues = request.Query.ToList();
            foreach (var pair in queryValues)
            {
                var values = pair.Value;
                foreach (var value in pair.Value)
                {
                    pairs.Add(new KeyValuePair<string, string>(
                        pair.Key,
                        Uri.EscapeDataString(value ?? "")
                    ));
                }
            }

            return pairs;
        }

        #endregion

        /// <summary>
        /// Returns 0 if oauth signature is valid
        /// Returns 400 if unsupported parameter, unsupported signature method, missing parameter, dupe parameter
        /// Returns 401 if invalid key, timestamp, token, signature, or nonce
        /// </summary>
        /// <param name="context"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private int Verify(HttpContext context, ApiContext db)
        {
            var request = context.Request;
            // Setup the variables necessary to recreate the OAuth 1.0 signature 
            string httpMethod = request.Method.ToUpper();

            var url = SignatureBaseStringUri(request);

            // Collect header and querystring params
            // OneRoster endpoints don't support urlencoded body
            var headerParams = getHeaderParams(request);
            var queryParams = getQueryParams(request);
            var combinedParams = headerParams.Concat(queryParams).ToList();

            // Generate and accept or reject the signature
            try
            {
                var token = combinedParams.First(kvp => kvp.Key == "Bearer").Value;

                if(!VerifyBearerToken(token, db))
                {
                    return 401;
                }

                return 0;
            }
            catch(InvalidOperationException)
            {
                return 400;
            }
        }

        public static string GenerateBearerToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private bool VerifyBearerToken(string token, ApiContext db)
        {
            var existingToken = db.OauthTokens.SingleOrDefault(n => n.Value == token);
            return existingToken != null && existingToken.CanBeUsed();
        }

        // https://tools.ietf.org/html/rfc5849#section-3.4.1.2
        private string SignatureBaseStringUri(HttpRequest request)
        {
            var protocolString = request.IsHttps ? "https://" : "http://";
            var domainString = request.Host.Host.ToLower();
            var path = request.Path.Value;
            string portString = "";

            if (request.Host.Port != null)
            {
                switch (request.IsHttps)
                {
                    case true:
                        if (request.Host.Port != 443)
                        {
                            portString = $":{request.Host.Port}";
                        }
                        break;
                    case false:
                        if (request.Host.Port != 80)
                        {
                            portString = $":{request.Host.Port}";
                        }
                        break;
                }
            }

            return Uri.EscapeDataString($"{protocolString}{domainString}{portString}{path}");
        }
    }
}
