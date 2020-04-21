﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Collections.Immutable;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreHandlerFilters;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OpenIddict.Server.AspNetCore
{
    public static partial class OpenIddictServerAspNetCoreHandlers
    {
        public static class Exchange
        {
            public static ImmutableArray<OpenIddictServerHandlerDescriptor> DefaultHandlers { get; } = ImmutableArray.Create(
                /*
                 * Token request extraction:
                 */
                ExtractPostRequest<ExtractTokenRequestContext>.Descriptor,
                ExtractBasicAuthenticationCredentials<ExtractTokenRequestContext>.Descriptor,

                /*
                 * Token request handling:
                 */
                EnablePassthroughMode<HandleTokenRequestContext, RequireTokenEndpointPassthroughEnabled>.Descriptor,

                /*
                 * Token response processing:
                 */
                AttachHttpResponseCode<ApplyTokenResponseContext>.Descriptor,
                AttachCacheControlHeader<ApplyTokenResponseContext>.Descriptor,
                AttachWwwAuthenticateHeader<ApplyTokenResponseContext>.Descriptor,
                ProcessJsonResponse<ApplyTokenResponseContext>.Descriptor);
        }
    }
}
