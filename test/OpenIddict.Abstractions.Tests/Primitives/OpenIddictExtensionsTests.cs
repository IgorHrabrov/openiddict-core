/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace OpenIddict.Abstractions.Tests.Primitives
{
    public class OpenIdConnectExtensionsTests
    {
        [Fact]
        public void GetAcrValues_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.GetAcrValues());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData("mod-pr", new[] { "mod-pr" })]
        [InlineData("mod-pr ", new[] { "mod-pr" })]
        [InlineData(" mod-pr ", new[] { "mod-pr" })]
        [InlineData("mod-pr mod-mf", new[] { "mod-pr", "mod-mf" })]
        [InlineData("mod-pr     mod-mf", new[] { "mod-pr", "mod-mf" })]
        [InlineData("mod-pr mod-mf ", new[] { "mod-pr", "mod-mf" })]
        [InlineData(" mod-pr mod-mf", new[] { "mod-pr", "mod-mf" })]
        [InlineData("mod-pr mod-pr mod-mf", new[] { "mod-pr", "mod-mf" })]
        [InlineData("mod-pr MOD-PR mod-mf", new[] { "mod-pr", "MOD-PR", "mod-mf" })]
        public void GetAcrValues_ReturnsExpectedAcrValues(string value, string[] values)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                AcrValues = value
            };

            // Act and assert
            Assert.Equal(values, request.GetAcrValues());
        }

        [Fact]
        public void GetResponseTypes_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest)null;

            // Act
            var exception = Assert.Throws<ArgumentNullException>(() => request.GetResponseTypes());

            // Assert
            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData("code", new[] { "code" })]
        [InlineData("code ", new[] { "code" })]
        [InlineData(" code ", new[] { "code" })]
        [InlineData("code id_token", new[] { "code", "id_token" })]
        [InlineData("code     id_token", new[] { "code", "id_token" })]
        [InlineData("code id_token ", new[] { "code", "id_token" })]
        [InlineData(" code id_token", new[] { "code", "id_token" })]
        [InlineData("code code id_token", new[] { "code", "id_token" })]
        [InlineData("code CODE id_token", new[] { "code", "CODE", "id_token" })]
        public void GetResponseTypes_ReturnsExpectedResponseTypes(string value, string[] values)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = value
            };

            // Act and assert
            Assert.Equal(values, request.GetResponseTypes());
        }

        [Fact]
        public void GetScopes_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.GetScopes());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData("openid", new[] { "openid" })]
        [InlineData("openid ", new[] { "openid" })]
        [InlineData(" openid ", new[] { "openid" })]
        [InlineData("openid profile", new[] { "openid", "profile" })]
        [InlineData("openid     profile", new[] { "openid", "profile" })]
        [InlineData("openid profile ", new[] { "openid", "profile" })]
        [InlineData(" openid profile", new[] { "openid", "profile" })]
        [InlineData("openid openid profile", new[] { "openid", "profile" })]
        [InlineData("openid OPENID profile", new[] { "openid", "OPENID", "profile" })]
        public void GetScopes_ReturnsExpectedScopes(string scope, string[] scopes)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                Scope = scope
            };

            // Act and assert
            Assert.Equal(scopes, request.GetScopes());
        }

        [Fact]
        public void HasAcrValue_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.HasAcrValue("mod-mf"));

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasAcrValue_ThrowsAnExceptionForNullOrEmptyAcrValue(string value)
        {
            // Arrange
            var request = new OpenIddictRequest();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => request.HasAcrValue(value));

            Assert.Equal("value", exception.ParamName);
            Assert.StartsWith("The value cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("mod-mf", true)]
        [InlineData("mod-mf mod-pr", true)]
        [InlineData(" mod-mf mod-pr", true)]
        [InlineData("mod-pr mod-mf", true)]
        [InlineData("mod-pr mod-mf ", true)]
        [InlineData("mod-pr mod-mf mod-cstm", true)]
        [InlineData("mod-pr mod-mf mod-cstm ", true)]
        [InlineData("mod-pr    mod-mf   mod-cstm ", true)]
        [InlineData("mod-pr", false)]
        [InlineData("mod-pr mod-cstm", false)]
        [InlineData("MOD-MF", false)]
        [InlineData("MOD-MF MOD-PR", false)]
        [InlineData(" MOD-MF MOD-PR", false)]
        [InlineData("MOD-PR MOD-MF", false)]
        [InlineData("MOD-PR MOD-MF ", false)]
        [InlineData("MOD-PR MOD-MF MOD-CSTM", false)]
        [InlineData("MOD-PR MOD-MF MOD-CSTM ", false)]
        [InlineData("MOD-PR    MOD-MF   MOD-CSTM ", false)]
        [InlineData("MOD-PR", false)]
        [InlineData("MOD-PR MOD-CSTM", false)]
        public void HasAcrValue_ReturnsExpectedResult(string value, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                AcrValues = value
            };

            // Act and assert
            Assert.Equal(result, request.HasAcrValue("mod-mf"));
        }

        [Fact]
        public void HasPrompt_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                request.HasPrompt(OpenIddictConstants.Prompts.Consent);
            });

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasPrompt_ThrowsAnExceptionForNullOrEmptyPrompt(string prompt)
        {
            // Arrange
            var request = new OpenIddictRequest();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => request.HasPrompt(prompt));

            Assert.Equal("prompt", exception.ParamName);
            Assert.StartsWith("The prompt cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("consent", true)]
        [InlineData("consent login", true)]
        [InlineData(" consent login", true)]
        [InlineData("login consent", true)]
        [InlineData("login consent ", true)]
        [InlineData("login consent select_account", true)]
        [InlineData("login consent select_account ", true)]
        [InlineData("login    consent   select_account ", true)]
        [InlineData("login", false)]
        [InlineData("login select_account", false)]
        [InlineData("CONSENT", false)]
        [InlineData("CONSENT LOGIN", false)]
        [InlineData(" CONSENT LOGIN", false)]
        [InlineData("LOGIN CONSENT", false)]
        [InlineData("LOGIN CONSENT ", false)]
        [InlineData("LOGIN CONSENT SELECT_ACCOUNT", false)]
        [InlineData("LOGIN CONSENT SELECT_ACCOUNT ", false)]
        [InlineData("LOGIN    CONSENT   SELECT_ACCOUNT ", false)]
        [InlineData("LOGIN", false)]
        [InlineData("LOGIN SELECT_ACCOUNT", false)]
        public void HasPrompt_ReturnsExpectedResult(string prompt, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                Prompt = prompt
            };

            // Act and assert
            Assert.Equal(result, request.HasPrompt(OpenIddictConstants.Prompts.Consent));
        }

        [Fact]
        public void HasResponseType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                request.HasResponseType(OpenIddictConstants.ResponseTypes.Code);
            });

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasResponseType_ThrowsAnExceptionForNullOrEmptyResponseType(string type)
        {
            // Arrange
            var request = new OpenIddictRequest();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => request.HasResponseType(type));

            Assert.Equal("type", exception.ParamName);
            Assert.StartsWith("The response type cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("code", true)]
        [InlineData("code id_token", true)]
        [InlineData(" code id_token", true)]
        [InlineData("id_token code", true)]
        [InlineData("id_token code ", true)]
        [InlineData("id_token code token", true)]
        [InlineData("id_token code token ", true)]
        [InlineData("id_token    code   token ", true)]
        [InlineData("id_token", false)]
        [InlineData("id_token token", false)]
        [InlineData("CODE", false)]
        [InlineData("CODE ID_TOKEN", false)]
        [InlineData(" CODE ID_TOKEN", false)]
        [InlineData("ID_TOKEN CODE", false)]
        [InlineData("ID_TOKEN CODE ", false)]
        [InlineData("ID_TOKEN CODE TOKEN", false)]
        [InlineData("ID_TOKEN CODE TOKEN ", false)]
        [InlineData("ID_TOKEN    CODE   TOKEN ", false)]
        [InlineData("ID_TOKEN", false)]
        [InlineData("ID_TOKEN TOKEN", false)]
        public void HasResponseType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.HasResponseType(OpenIddictConstants.ResponseTypes.Code));
        }

        [Fact]
        public void HasScope_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                request.HasScope(OpenIddictConstants.Scopes.OpenId);
            });

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasScope_ThrowsAnExceptionForNullOrEmptyScope(string scope)
        {
            // Arrange
            var request = new OpenIddictRequest();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => request.HasScope(scope));

            Assert.Equal("scope", exception.ParamName);
            Assert.StartsWith("The scope cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("openid", true)]
        [InlineData("openid ", true)]
        [InlineData(" openid ", true)]
        [InlineData("openid profile", true)]
        [InlineData("openid     profile", true)]
        [InlineData("openid profile ", true)]
        [InlineData(" openid profile", true)]
        [InlineData("profile", false)]
        [InlineData("profile email", false)]
        [InlineData("OPENID", false)]
        [InlineData("OPENID ", false)]
        [InlineData(" OPENID ", false)]
        [InlineData("OPENID PROFILE", false)]
        [InlineData("OPENID     PROFILE", false)]
        [InlineData("OPENID PROFILE ", false)]
        [InlineData(" OPENID PROFILE", false)]
        [InlineData("PROFILE", false)]
        [InlineData("PROFILE EMAIL", false)]
        public void HasScope_ReturnsExpectedResult(string scope, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                Scope = scope
            };

            // Act and assert
            Assert.Equal(result, request.HasScope(OpenIddictConstants.Scopes.OpenId));
        }

        [Fact]
        public void IsNoneFlow_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsNoneFlow());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("none", true)]
        [InlineData("none ", true)]
        [InlineData(" none", true)]
        [InlineData("none id_token", false)]
        [InlineData(" none id_token", false)]
        [InlineData("none id_token ", false)]
        [InlineData(" none id_token ", false)]
        [InlineData("NONE", false)]
        [InlineData("NONE ", false)]
        [InlineData(" NONE", false)]
        [InlineData("NONE ID_TOKEN", false)]
        [InlineData(" NONE ID_TOKEN", false)]
        [InlineData("NONE ID_TOKEN ", false)]
        [InlineData(" NONE ID_TOKEN ", false)]
        public void IsNoneFlow_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsNoneFlow());
        }

        [Fact]
        public void IsAuthorizationCodeFlow_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsAuthorizationCodeFlow());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("code", true)]
        [InlineData("code ", true)]
        [InlineData(" code", true)]
        [InlineData("code id_token", false)]
        [InlineData(" code id_token", false)]
        [InlineData("code id_token ", false)]
        [InlineData(" code id_token ", false)]
        [InlineData("CODE", false)]
        [InlineData("CODE ", false)]
        [InlineData(" CODE", false)]
        [InlineData("CODE ID_TOKEN", false)]
        [InlineData(" CODE ID_TOKEN", false)]
        [InlineData("CODE ID_TOKEN ", false)]
        [InlineData(" CODE ID_TOKEN ", false)]
        public void IsAuthorizationCodeFlow_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsAuthorizationCodeFlow());
        }

        [Fact]
        public void IsImplicitFlow_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsImplicitFlow());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("id_token", true)]
        [InlineData("id_token ", true)]
        [InlineData(" id_token", true)]
        [InlineData("id_token token", true)]
        [InlineData(" id_token token", true)]
        [InlineData("id_token token ", true)]
        [InlineData(" id_token token ", true)]
        [InlineData("token", true)]
        [InlineData("token ", true)]
        [InlineData(" token", true)]
        [InlineData("code id_token", false)]
        [InlineData("code id_token token", false)]
        [InlineData("code token", false)]
        [InlineData("ID_TOKEN", false)]
        [InlineData("ID_TOKEN ", false)]
        [InlineData(" ID_TOKEN", false)]
        [InlineData("ID_TOKEN TOKEN", false)]
        [InlineData(" ID_TOKEN TOKEN", false)]
        [InlineData("ID_TOKEN TOKEN ", false)]
        [InlineData(" ID_TOKEN TOKEN ", false)]
        [InlineData("TOKEN", false)]
        [InlineData("TOKEN ", false)]
        [InlineData(" TOKEN", false)]
        [InlineData("CODE ID_TOKEN", false)]
        [InlineData("CODE ID_TOKEN TOKEN", false)]
        [InlineData("CODE TOKEN", false)]
        public void IsImplicitFlow_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsImplicitFlow());
        }

        [Fact]
        public void IsHybridFlow_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsHybridFlow());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("code id_token", true)]
        [InlineData("code id_token ", true)]
        [InlineData(" code id_token", true)]
        [InlineData("code id_token token", true)]
        [InlineData(" code id_token token", true)]
        [InlineData("code id_token token ", true)]
        [InlineData(" code id_token token ", true)]
        [InlineData(" code  id_token  token ", true)]
        [InlineData("code token", true)]
        [InlineData("code token ", true)]
        [InlineData(" code token", true)]
        [InlineData("id_token", false)]
        [InlineData("id_token token", false)]
        [InlineData("token", false)]
        [InlineData("CODE ID_TOKEN", false)]
        [InlineData("CODE ID_TOKEN ", false)]
        [InlineData(" CODE ID_TOKEN", false)]
        [InlineData("CODE ID_TOKEN TOKEN", false)]
        [InlineData(" CODE ID_TOKEN TOKEN", false)]
        [InlineData("CODE ID_TOKEN TOKEN ", false)]
        [InlineData(" CODE ID_TOKEN TOKEN ", false)]
        [InlineData(" CODE  ID_TOKEN  TOKEN ", false)]
        [InlineData("CODE TOKEN", false)]
        [InlineData("CODE TOKEN ", false)]
        [InlineData(" CODE TOKEN", false)]
        [InlineData("ID_TOKEN", false)]
        [InlineData("ID_TOKEN TOKEN", false)]
        [InlineData("TOKEN", false)]
        public void IsHybridFlow_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsHybridFlow());
        }

        [Fact]
        public void IsFragmentResponseMode_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsFragmentResponseMode());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData("unknown", null, false)]
        [InlineData("query", null, false)]
        [InlineData("form_post", null, false)]
        [InlineData("fragment", null, true)]
        [InlineData("fragment ", null, false)]
        [InlineData(" fragment", null, false)]
        [InlineData(" fragment ", null, false)]
        [InlineData(null, "code", false)]
        [InlineData(null, "code id_token", true)]
        [InlineData(null, "code id_token token", true)]
        [InlineData(null, "code token", true)]
        [InlineData(null, "id_token", true)]
        [InlineData(null, "id_token token", true)]
        [InlineData(null, "token", true)]
        [InlineData("QUERY", null, false)]
        [InlineData("FRAGMENT", null, false)]
        [InlineData("FORM_POST", null, false)]
        [InlineData(null, "CODE", false)]
        [InlineData(null, "CODE ID_TOKEN", false)]
        [InlineData(null, "CODE ID_TOKEN TOKEN", false)]
        [InlineData(null, "CODE TOKEN", false)]
        [InlineData(null, "ID_TOKEN", false)]
        [InlineData(null, "ID_TOKEN TOKEN", false)]
        [InlineData(null, "TOKEN", false)]
        public void IsFragmentResponseMode_ReturnsExpectedResult(string mode, string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseMode = mode,
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsFragmentResponseMode());
        }

        [Fact]
        public void IsQueryResponseMode_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsQueryResponseMode());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData("unknown", null, false)]
        [InlineData("query", null, true)]
        [InlineData("query ", null, false)]
        [InlineData(" query", null, false)]
        [InlineData(" query ", null, false)]
        [InlineData("fragment", null, false)]
        [InlineData("form_post", null, false)]
        [InlineData(null, "none", true)]
        [InlineData(null, "code", true)]
        [InlineData(null, "code id_token token", false)]
        [InlineData(null, "code token", false)]
        [InlineData(null, "id_token", false)]
        [InlineData(null, "id_token token", false)]
        [InlineData(null, "token", false)]
        [InlineData("QUERY", null, false)]
        [InlineData("FRAGMENT", null, false)]
        [InlineData("FORM_POST", null, false)]
        [InlineData(null, "CODE", false)]
        [InlineData(null, "CODE ID_TOKEN", false)]
        [InlineData(null, "CODE ID_TOKEN TOKEN", false)]
        [InlineData(null, "CODE TOKEN", false)]
        [InlineData(null, "ID_TOKEN", false)]
        [InlineData(null, "ID_TOKEN TOKEN", false)]
        [InlineData(null, "TOKEN", false)]
        public void IsQueryResponseMode_ReturnsExpectedResult(string mode, string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseMode = mode,
                ResponseType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsQueryResponseMode());
        }

        [Fact]
        public void IsFormPostResponseMode_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsFormPostResponseMode());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("query", false)]
        [InlineData("fragment", false)]
        [InlineData("form_post", true)]
        [InlineData("form_post ", false)]
        [InlineData(" form_post", false)]
        [InlineData(" form_post ", false)]
        [InlineData("QUERY", false)]
        [InlineData("FRAGMENT", false)]
        [InlineData("FORM_POST", false)]
        public void IsFormPostResponseMode_ReturnsExpectedResult(string mode, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                ResponseMode = mode
            };

            // Act and assert
            Assert.Equal(result, request.IsFormPostResponseMode());
        }

        [Fact]
        public void IsAuthorizationCodeGrantType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsAuthorizationCodeGrantType());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("authorization_code", true)]
        [InlineData("authorization_code ", false)]
        [InlineData(" authorization_code", false)]
        [InlineData(" authorization_code ", false)]
        [InlineData("client_credentials", false)]
        [InlineData("password", false)]
        [InlineData("refresh_token", false)]
        [InlineData("AUTHORIZATION_CODE", false)]
        [InlineData("CLIENT_CREDENTIALS", false)]
        [InlineData("PASSWORD", false)]
        [InlineData("REFRESH_TOKEN", false)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code", false)]
        public void IsAuthorizationCodeGrantType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                GrantType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsAuthorizationCodeGrantType());
        }

        [Fact]
        public void IsClientCredentialsGrantType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsClientCredentialsGrantType());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("authorization_code", false)]
        [InlineData("client_credentials", true)]
        [InlineData("client_credentials ", false)]
        [InlineData(" client_credentials", false)]
        [InlineData(" client_credentials ", false)]
        [InlineData("password", false)]
        [InlineData("refresh_token", false)]
        [InlineData("AUTHORIZATION_CODE", false)]
        [InlineData("CLIENT_CREDENTIALS", false)]
        [InlineData("PASSWORD", false)]
        [InlineData("REFRESH_TOKEN", false)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code", false)]
        public void IsClientCredentialsGrantType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                GrantType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsClientCredentialsGrantType());
        }

        [Fact]
        public void IsDeviceCodeGrantType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest)null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsDeviceCodeGrantType());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("authorization_code", false)]
        [InlineData("client_credentials", false)]
        [InlineData("password", false)]
        [InlineData("refresh_token", false)]
        [InlineData("AUTHORIZATION_CODE", false)]
        [InlineData("CLIENT_CREDENTIALS", false)]
        [InlineData("PASSWORD", false)]
        [InlineData("REFRESH_TOKEN", false)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code", true)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code ", false)]
        [InlineData(" urn:ietf:params:oauth:grant-type:device_code", false)]
        [InlineData(" urn:ietf:params:oauth:grant-type:device_code ", false)]
        public void IsDeviceCodeGrantType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                GrantType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsDeviceCodeGrantType());
        }

        [Fact]
        public void IsPasswordGrantType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsPasswordGrantType());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("authorization_code", false)]
        [InlineData("client_credentials", false)]
        [InlineData("password", true)]
        [InlineData("password ", false)]
        [InlineData(" password", false)]
        [InlineData(" password ", false)]
        [InlineData("refresh_token", false)]
        [InlineData("AUTHORIZATION_CODE", false)]
        [InlineData("CLIENT_CREDENTIALS", false)]
        [InlineData("PASSWORD", false)]
        [InlineData("REFRESH_TOKEN", false)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code", false)]
        public void IsPasswordGrantType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                GrantType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsPasswordGrantType());
        }

        [Fact]
        public void IsRefreshTokenGrantType_ThrowsAnExceptionForNullRequest()
        {
            // Arrange
            var request = (OpenIddictRequest) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.IsRefreshTokenGrantType());

            Assert.Equal("request", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData("authorization_code", false)]
        [InlineData("client_credentials", false)]
        [InlineData("password", false)]
        [InlineData("refresh_token", true)]
        [InlineData("refresh_token ", false)]
        [InlineData(" refresh_token", false)]
        [InlineData(" refresh_token ", false)]
        [InlineData("AUTHORIZATION_CODE", false)]
        [InlineData("CLIENT_CREDENTIALS", false)]
        [InlineData("PASSWORD", false)]
        [InlineData("REFRESH_TOKEN", false)]
        [InlineData("urn:ietf:params:oauth:grant-type:device_code", false)]
        public void IsRefreshTokenGrantType_ReturnsExpectedResult(string type, bool result)
        {
            // Arrange
            var request = new OpenIddictRequest
            {
                GrantType = type
            };

            // Act and assert
            Assert.Equal(result, request.IsRefreshTokenGrantType());
        }

        [Fact]
        public void GetDestinations_ThrowsAnExceptionForNullClaim()
        {
            // Arrange
            var claim = (Claim) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => claim.GetDestinations());

            Assert.Equal("claim", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData("", new string[0])]
        [InlineData("[]", new string[0])]
        [InlineData(@"[""id_token""]", new[] { "id_token" })]
        [InlineData(@"[""access_token"",""id_token""]", new[] { "access_token", "id_token" })]
        [InlineData(@"[""access_token"",""access_token"",""id_token""]", new[] { "access_token", "id_token" })]
        [InlineData(@"[""access_token"",""ACCESS_TOKEN"",""id_token""]", new[] { "access_token", "id_token" })]
        public void GetDestinations_ReturnsExpectedDestinations(string destination, string[] destinations)
        {
            // Arrange
            var claim = new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");
            claim.Properties[OpenIddictConstants.Properties.Destinations] = destination;

            // Act and assert
            Assert.Equal(destinations, claim.GetDestinations());
        }

        [Fact]
        public void SetDestinations_ThrowsAnExceptionForNullClaim()
        {
            // Arrange
            var claim = (Claim) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => claim.SetDestinations());

            Assert.Equal("claim", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new string[0] })]
        public void SetDestinations_RemovesPropertyForEmptyArray(string[] destinations)
        {
            // Arrange
            var claim = new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");

            // Act
            claim.SetDestinations(destinations);

            // Assert
            Assert.Equal(0, claim.Properties.Count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SetDestinations_ThrowsAnExceptionForNullOrEmptyDestinations(string destination)
        {
            // Arrange
            var claim = new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => claim.SetDestinations(destination));

            Assert.Equal("destinations", exception.ParamName);
            Assert.StartsWith("Destinations cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(new[] { "access_token" }, @"[""access_token""]")]
        [InlineData(new[] { "access_token", "id_token" }, @"[""access_token"",""id_token""]")]
        [InlineData(new[] { "access_token", "access_token", "id_token" }, @"[""access_token"",""id_token""]")]
        [InlineData(new[] { "access_token", "ACCESS_TOKEN", "id_token" }, @"[""access_token"",""id_token""]")]
        public void SetDestinations_SetsAppropriateDestinations(string[] destinations, string destination)
        {
            // Arrange
            var claim = new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");

            // Act
            claim.SetDestinations(destinations);

            // Assert
            Assert.Equal(destination, claim.Properties[OpenIddictConstants.Properties.Destinations]);
        }

        [Fact]
        public void ClaimsIdentity_Clone_ReturnsDifferentInstanceWithFilteredClaims()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.ClientId, "B56BF6CE-8D8C-4290-A0E7-A4F8EE0A9FC4"));

            // Act
            var clone = identity.Clone(claim => claim.Type == OpenIddictConstants.Claims.Name);
            clone.AddClaim(new Claim("clone_claim", "value"));

            // Assert
            Assert.NotSame(identity, clone);
            Assert.Null(identity.FindFirst("clone_claim"));
            Assert.NotNull(clone.FindFirst(OpenIddictConstants.Claims.Name));
            Assert.Null(clone.FindFirst(OpenIddictConstants.Claims.ClientId));
        }

        [Fact]
        public void ClaimsIdentity_Clone_ExcludesUnwantedClaims()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, "D8F1A010-BD46-4F8F-AD4E-05582307F8F4"));

            // Act
            var clone = identity.Clone(claim => claim.Type == OpenIddictConstants.Claims.Name);

            // Assert
            Assert.Single(clone.Claims);
            Assert.Null(clone.FindFirst(OpenIddictConstants.Claims.Subject));
            Assert.Equal("Bob le Bricoleur", clone.FindFirst(OpenIddictConstants.Claims.Name).Value);
        }

        [Fact]
        public void ClaimsIdentity_Clone_ExcludesUnwantedClaimsFromActor()
        {
            // Arrange
            var identity = new ClaimsIdentity
            {
                Actor = new ClaimsIdentity()
            };
            identity.Actor.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));
            identity.Actor.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, "D8F1A010-BD46-4F8F-AD4E-05582307F8F4"));

            // Act
            var clone = identity.Clone(claim => claim.Type == OpenIddictConstants.Claims.Name);

            // Assert
            Assert.Single(clone.Actor.Claims);
            Assert.Null(clone.Actor.FindFirst(OpenIddictConstants.Claims.Subject));
            Assert.Equal("Bob le Bricoleur", clone.Actor.FindFirst(OpenIddictConstants.Claims.Name).Value);
        }

        [Fact]
        public void ClaimsPrincipal_Clone_ExcludesUnwantedClaimsFromIdentities()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, "D8F1A010-BD46-4F8F-AD4E-05582307F8F4"));

            var principal = new ClaimsPrincipal(identity);

            // Act
            var clone = principal.Clone(claim => claim.Type == OpenIddictConstants.Claims.Name);

            // Assert
            Assert.Single(clone.Claims);
            Assert.Null(clone.FindFirst(OpenIddictConstants.Claims.Subject));
            Assert.Equal("Bob le Bricoleur", clone.FindFirst(OpenIddictConstants.Claims.Name).Value);
        }

        [Fact]
        public void AddClaim_ThrowsAnExceptionForNullIdentity()
        {
            // Arrange
            var identity = (ClaimsIdentity) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                identity.AddClaim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");
            });

            Assert.Equal("identity", exception.ParamName);
        }

        [Fact]
        public void AddClaim_SetsAppropriateClaim()
        {
            // Arrange
            var identity = new ClaimsIdentity();

            // Act
            identity.AddClaim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");

            // Assert
            Assert.Equal("Bob le Bricoleur", identity.FindFirst(OpenIddictConstants.Claims.Name).Value);
        }

        [Theory]
        [InlineData(new[] { "access_token" }, @"[""access_token""]")]
        [InlineData(new[] { "access_token", "id_token" }, @"[""access_token"",""id_token""]")]
        [InlineData(new[] { "access_token", "access_token", "id_token" }, @"[""access_token"",""id_token""]")]
        [InlineData(new[] { "access_token", "ACCESS_TOKEN", "id_token" }, @"[""access_token"",""id_token""]")]
        public void AddClaim_SetsAppropriateDestinations(string[] destinations, string destination)
        {
            // Arrange
            var identity = new ClaimsIdentity();

            // Act
            identity.AddClaim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur", destinations);

            var claim = identity.FindFirst(OpenIddictConstants.Claims.Name);

            // Assert
            Assert.Equal("Bob le Bricoleur", claim.Value);
            Assert.Equal(destination, claim.Properties[OpenIddictConstants.Properties.Destinations]);
        }

        [Fact]
        public void GetClaim_ThrowsAnExceptionForNullIdentity()
        {
            // Arrange
            var identity = (ClaimsIdentity) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                identity.GetClaim(OpenIddictConstants.Claims.Name);
            });

            Assert.Equal("identity", exception.ParamName);
        }

        [Fact]
        public void GetClaim_ReturnsNullForMissingClaims()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal();

            // Act and assert
            Assert.Null(identity.GetClaim(OpenIddictConstants.Claims.Name));
            Assert.Null(principal.GetClaim(OpenIddictConstants.Claims.Name));
        }

        [Fact]
        public void GetClaim_ReturnsAppropriateResult()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            identity.AddClaim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur");

            // Act and assert
            Assert.Equal("Bob le Bricoleur", identity.GetClaim(OpenIddictConstants.Claims.Name));
            Assert.Equal("Bob le Bricoleur", principal.GetClaim(OpenIddictConstants.Claims.Name));
        }

        [Fact]
        public void ClaimsIdentity_Clone_ThrowsAnExceptionForNullIdentity()
        {
            // Arrange
            var identity = (ClaimsIdentity) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => identity.Clone(c => true));

            Assert.Equal("identity", exception.ParamName);
        }

        [Fact]
        public void ClaimsIdentity_Clone_ReturnsIdenticalIdentity()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim("type", "value");

            // Act
            var copy = identity.Clone(c => true);

            // Assert
            Assert.Equal("value", copy.GetClaim("type"));
            Assert.Equal(identity.Claims.Count(), copy.Claims.Count());
        }

        [Fact]
        public void ClaimsPrincipal_Clone_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.Clone(c => true));

            Assert.Equal("principal", exception.ParamName);
        }

        [Fact]
        public void ClaimsPrincipal_Clone_ReturnsIdenticalPrincipal()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));

            var principal = new ClaimsPrincipal(identity);

            // Act
            var copy = principal.Clone(c => true);

            // Assert
            Assert.Equal("Bob le Bricoleur", copy.GetClaim(OpenIddictConstants.Claims.Name));
            Assert.Equal(principal.Claims.Count(), copy.Claims.Count());
        }

        [Fact]
        public void ClaimsIdentity_Clone_ReturnsDifferentIdentityInstance()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim("type", "value");

            // Act
            var copy = identity.Clone(c => true);
            copy.AddClaim("clone_type", "value");

            // Assert
            Assert.NotSame(identity, copy);
            Assert.Null(identity.FindFirst("clone_type"));
        }

        [Fact]
        public void ClaimsPrincipal_Clone_ReturnsDifferentPrincipalInstance()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, "Bob le Bricoleur"));

            var principal = new ClaimsPrincipal(identity);

            // Act
            var copy = principal.Clone(c => true);
            copy.SetClaim("clone_claim", "value");

            // Assert
            Assert.NotSame(principal, copy);
            Assert.Null(principal.FindFirst("clone_claim"));
        }

        [Fact]
        public void GetClaim_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetClaim("type"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Fact]
        public void GetClaim_ReturnsNullForMissingClaim()
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            Assert.Null(principal.GetClaim("type"));
        }

        [Fact]
        public void GetClaim_IsCaseInsensitive()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            principal.SetClaim("type", "value");

            // Act and assert
            Assert.Equal("value", principal.GetClaim("TYPE"));
        }

        [Fact]
        public void GetAudiences_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetAudiences());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void GetAudiences_ReturnsExpectedAudiences(string[] audience, string[] audiences)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Audience, audience.ToImmutableArray());

            // Act and assert
            Assert.Equal(audiences, principal.GetAudiences());
        }

        [Fact]
        public void GetPresenters_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetPresenters());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void GetPresenters_ReturnsExpectedPresenters(string[] presenter, string[] presenters)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Presenters, presenter.ToImmutableArray());

            // Act and assert
            Assert.Equal(presenters, principal.GetPresenters());
        }

        [Fact]
        public void GetResources_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetResources());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void GetResources_ReturnsExpectedResources(string[] resource, string[] resources)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Resources, resource.ToImmutableArray());

            // Act and assert
            Assert.Equal(resources, principal.GetResources());
        }

        [Fact]
        public void GetScopes_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetScopes());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "openid" }, new[] { "openid" })]
        [InlineData(new[] { "openid", "profile" }, new[] { "openid", "profile" })]
        [InlineData(new[] { "openid", "openid", "profile" }, new[] { "openid", "profile" })]
        [InlineData(new[] { "openid", "OPENID", "profile" }, new[] { "openid", "OPENID", "profile" })]
        public void ClaimsPrincipal_GetScopes_ReturnsExpectedScopes(string[] scope, string[] scopes)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Scopes, scope.ToImmutableArray());

            // Act and assert
            Assert.Equal(scopes, principal.GetScopes());
        }

        [Fact]
        public void GetAccessTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetAccessTokenLifetime());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void GetAccessTokenLifetime_ReturnsExpectedResult(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.AccessTokenLifetime, lifetime);

            // Act and assert
            Assert.Equal(ParseLifeTime(lifetime), principal.GetAccessTokenLifetime());
        }

        [Fact]
        public void GetAuthorizationCodeLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetAuthorizationCodeLifetime());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void GetAuthorizationCodeLifetime_ReturnsExpectedResult(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.AuthorizationCodeLifetime, lifetime);

            // Act and assert
            Assert.Equal(ParseLifeTime(lifetime), principal.GetAuthorizationCodeLifetime());
        }

        [Fact]
        public void GetIdentityTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetIdentityTokenLifetime());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void GetIdentityTokenLifetime_ReturnsExpectedResult(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.IdentityTokenLifetime, lifetime);

            // Act and assert
            Assert.Equal(ParseLifeTime(lifetime), principal.GetIdentityTokenLifetime());
        }

        [Fact]
        public void GetRefreshTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetRefreshTokenLifetime());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void GetRefreshTokenLifetime_ReturnsExpectedResult(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.RefreshTokenLifetime, lifetime);

            // Act and assert
            Assert.Equal(ParseLifeTime(lifetime), principal.GetRefreshTokenLifetime());
        }

        [Fact]
        public void GetInternalTokenId_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetInternalTokenId());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("identifier")]
        public void GetInternalTokenId_ReturnsExpectedResult(string identifier)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenId, identifier);

            // Act and assert
            Assert.Equal(identifier, principal.GetInternalTokenId());
        }

        [Fact]
        public void GetTokenUsage_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.GetTokenUsage());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("access_token")]
        public void GetTokenUsage_ReturnsExpectedResult(string usage)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenUsage, usage);

            // Act and assert
            Assert.Equal(usage, principal.GetTokenUsage());
        }

        [Fact]
        public void HasAudience_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.HasAudience("Fabrikam"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasAudience_ThrowsAnExceptionForNullOrEmptyAudience(string audience)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.HasAudience(audience));

            Assert.Equal("audience", exception.ParamName);
            Assert.StartsWith("The audience cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "fabrikam" }, true)]
        public void HasAudience_ReturnsExpectedResult(string[] audience, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Audience, audience.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasAudience());
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "contoso" }, false)]
        [InlineData(new[] { "contoso", "fabrikam" }, true)]
        [InlineData(new[] { "fabrikam" }, true)]
        [InlineData(new[] { "fabrikam", "contoso" }, true)]
        [InlineData(new[] { "CONTOSO" }, false)]
        [InlineData(new[] { "CONTOSO", "FABRIKAM" }, false)]
        [InlineData(new[] { "FABRIKAM" }, false)]
        [InlineData(new[] { "FABRIKAM", "CONTOSO" }, false)]
        public void HasAudience_ReturnsAppropriateResult(string[] audience, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Audience, audience.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasAudience("fabrikam"));
        }

        [Fact]
        public void HasPresenter_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.HasPresenter("Fabrikam"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasPresenter_ThrowsAnExceptionForNullOrEmptyPresenter(string presenter)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.HasPresenter(presenter));

            Assert.Equal("presenter", exception.ParamName);
            Assert.StartsWith("The presenter cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "fabrikam" }, true)]
        public void HasPresenter_ReturnsExpectedResult(string[] presenter, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Presenters, presenter.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasPresenter());
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "contoso" }, false)]
        [InlineData(new []{ "contoso", "fabrikam" }, true)]
        [InlineData(new []{ "fabrikam" }, true)]
        [InlineData(new []{ "fabrikam", "contoso" }, true)]
        [InlineData(new []{ "CONTOSO" }, false)]
        [InlineData(new []{ "CONTOSO", "FABRIKAM" }, false)]
        [InlineData(new []{ "FABRIKAM" }, false)]
        [InlineData(new []{ "FABRIKAM", "CONTOSO" }, false)]
        public void HasPresenter_ReturnsAppropriateResult(string[] presenter, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Presenters, presenter.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasPresenter("fabrikam"));
        }

        [Fact]
        public void HasResource_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.HasResource("Fabrikam"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasResource_ThrowsAnExceptionForNullOrEmptyResource(string resource)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.HasResource(resource));

            Assert.Equal("resource", exception.ParamName);
            Assert.StartsWith("The resource cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "fabrikam" }, true)]
        public void HasResource_ReturnsExpectedResult(string[] resource, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Resources, resource.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasResource());
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new []{ "contoso" }, false)]
        [InlineData(new []{ "contoso", "fabrikam" }, true)]
        [InlineData(new []{ "fabrikam" }, true)]
        [InlineData(new []{ "fabrikam", "contoso" }, true)]
        [InlineData(new []{ "CONTOSO" }, false)]
        [InlineData(new []{ "CONTOSO", "FABRIKAM" }, false)]
        [InlineData(new []{ "FABRIKAM" }, false)]
        [InlineData(new[] { "FABRIKAM" , "CONTOSO" }, false)]
        public void HasResource_ReturnsAppropriateResult(string[] resource, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Resources, resource.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasResource("fabrikam"));
        }

        [Fact]
        public void HasScope_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.HasScope(OpenIddictConstants.Scopes.OpenId));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ClaimsPrincipal_HasScope_ThrowsAnExceptionForNullOrEmptyScope(string scope)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.HasScope(scope));

            Assert.Equal("scope", exception.ParamName);
            Assert.StartsWith("The scope cannot be null or empty.", exception.Message);
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "openid" }, true)]
        public void ClaimsPrincipal_HasScope_ReturnsExpectedResult(string[] scope, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Scopes, scope.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasScope());
        }

        [Theory]
        [InlineData(new string[0], false)]
        [InlineData(new[] { "profile" }, false)]
        [InlineData(new []{ "profile", "openid" }, true)]
        [InlineData(new []{ "openid" }, true)]
        [InlineData(new []{ "openid" , "profile" }, true)]
        [InlineData(new []{ "PROFILE" }, false)]
        [InlineData(new []{ "PROFILE", "OPENID" }, false)]
        [InlineData(new []{ "OPENID" }, false)]
        [InlineData(new []{ "OPENID", "PROFILE" }, false)]
        public void HasScope_ReturnsAppropriateResult(string[] scope, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaims(OpenIddictConstants.Claims.Private.Scopes, scope.ToImmutableArray());

            // Act and assert
            Assert.Equal(result, principal.HasScope(OpenIddictConstants.Scopes.OpenId));
        }

        [Fact]
        public void IsAccessToken_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.IsAccessToken());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData(OpenIddictConstants.TokenUsages.AccessToken, true)]
        [InlineData(OpenIddictConstants.TokenUsages.AuthorizationCode, false)]
        [InlineData(OpenIddictConstants.TokenUsages.IdToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.RefreshToken, false)]
        public void IsAccessToken_ReturnsExpectedResult(string usage, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenUsage, usage);

            // Act and assert
            Assert.Equal(result, principal.IsAccessToken());
        }

        [Fact]
        public void IsAuthorizationCode_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.IsAuthorizationCode());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData(OpenIddictConstants.TokenUsages.AccessToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.AuthorizationCode, true)]
        [InlineData(OpenIddictConstants.TokenUsages.IdToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.RefreshToken, false)]
        public void IsAuthorizationCode_ReturnsExpectedResult(string usage, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenUsage, usage);

            // Act and assert
            Assert.Equal(result, principal.IsAuthorizationCode());
        }

        [Fact]
        public void IsIdentityToken_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.IsIdentityToken());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData(OpenIddictConstants.TokenUsages.AccessToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.AuthorizationCode, false)]
        [InlineData(OpenIddictConstants.TokenUsages.IdToken, true)]
        [InlineData(OpenIddictConstants.TokenUsages.RefreshToken, false)]
        public void IsIdentityToken_ReturnsExpectedResult(string usage, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenUsage, usage);

            // Act and assert
            Assert.Equal(result, principal.IsIdentityToken());
        }

        [Fact]
        public void IsRefreshToken_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.IsRefreshToken());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        [InlineData(OpenIddictConstants.TokenUsages.AccessToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.AuthorizationCode, false)]
        [InlineData(OpenIddictConstants.TokenUsages.IdToken, false)]
        [InlineData(OpenIddictConstants.TokenUsages.RefreshToken, true)]
        public void IsRefreshToken_ReturnsExpectedResult(string usage, bool result)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim(OpenIddictConstants.Claims.Private.TokenUsage, usage);

            // Act and assert
            Assert.Equal(result, principal.IsRefreshToken());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AddClaim_ThrowsAnExceptionForNullOrEmptyType(string type)
        {
            // Arrange
            var identity = new ClaimsIdentity();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => identity.AddClaim(type, "value"));

            Assert.Equal("type", exception.ParamName);
            Assert.StartsWith("The claim type cannot be null or empty.", exception.Message);
        }

        [Fact]
        public void AddClaim_AddsExpectedClaim()
        {
            // Arrange
            var identity = new ClaimsIdentity();

            // Act
            identity.AddClaim("type", "value");

            // Assert
            Assert.Equal("value", identity.GetClaim("type"));
        }

        [Fact]
        public void RemoveClaims_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.RemoveClaims("type"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RemoveClaims_ThrowsAnExceptionForNullOrEmptyClaimType(string type)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.RemoveClaims(type));

            Assert.Equal("type", exception.ParamName);
            Assert.StartsWith("The claim type cannot be null or empty.", exception.Message);
        }

        [Fact]
        public void RemoveClaims_RemoveClaims()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            principal.SetClaim("type", "value");

            // Act
            principal.RemoveClaims("type");

            // Assert
            Assert.Null(principal.GetClaim("type"));
        }

        [Fact]
        public void SetClaim_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetClaim("type", "value"));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SetClaim_ThrowsAnExceptionForNullOrEmptyProperty(string type)
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => principal.SetClaim(type, "value"));

            Assert.Equal("type", exception.ParamName);
            Assert.StartsWith("The claim type cannot be null or empty.", exception.Message);
        }

        [Fact]
        public void SetClaim_AddsExpectedClaim()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetClaim("type", "value");

            // Assert
            Assert.Equal("value", principal.GetClaim("type"));
        }

        [Fact]
        public void SetClaim_IsCaseInsensitive()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetClaim("TYPE", "value");

            // Assert
            Assert.Equal("value", principal.GetClaim("type"));
        }

        [Fact]
        public void SetClaim_RemovesEmptyClaim()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetClaim("type", string.Empty);

            // Assert
            Assert.Null(principal.GetClaim("type"));
        }

        [Fact]
        public void SetAudiences_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetAudiences());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void SetAudiences_AddsAudiences(string[] audiences, string[] audience)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetAudiences(audiences);

            // Assert
            Assert.Equal(audience, principal.GetClaims(OpenIddictConstants.Claims.Audience));
        }

        [Fact]
        public void SetPresenters_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetPresenters());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void SetPresenters_AddsPresenters(string[] presenters, string[] presenter)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetPresenters(presenters);

            // Assert
            Assert.Equal(presenter, principal.GetClaims(OpenIddictConstants.Claims.Private.Presenters));
        }

        [Fact]
        public void SetResources_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetResources());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "fabrikam" }, new[] { "fabrikam" })]
        [InlineData(new[] { "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "fabrikam", "contoso" }, new[] { "fabrikam", "contoso" })]
        [InlineData(new[] { "fabrikam", "FABRIKAM", "contoso" }, new[] { "fabrikam", "FABRIKAM", "contoso" })]
        public void SetResources_AddsResources(string[] resources, string[] resource)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetResources(resources);

            // Assert
            Assert.Equal(resource, principal.GetClaims(OpenIddictConstants.Claims.Private.Resources));
        }

        [Fact]
        public void SetScopes_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetScopes());

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "openid" }, new[] { "openid" })]
        [InlineData(new[] { "openid", "profile" }, new[] { "openid", "profile" })]
        [InlineData(new[] { "openid", "openid", "profile" }, new[] { "openid", "profile" })]
        [InlineData(new[] { "openid", "OPENID", "profile" }, new[] { "openid", "OPENID", "profile" })]
        public void SetScopes_AddsScopes(string[] scopes, string[] scope)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetScopes(scopes);

            // Assert
            Assert.Equal(scope, principal.GetClaims(OpenIddictConstants.Claims.Private.Scopes));
        }

        [Fact]
        public void SetAccessTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetAccessTokenLifetime(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void SetAccessTokenLifetime_AddsLifetime(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetAccessTokenLifetime(ParseLifeTime(lifetime));

            // Assert
            Assert.Equal(lifetime, principal.GetClaim(OpenIddictConstants.Claims.Private.AccessTokenLifetime));
        }

        [Fact]
        public void SetAuthorizationCodeLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetAuthorizationCodeLifetime(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void SetAuthorizationCodeLifetime_AddsLifetime(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetAuthorizationCodeLifetime(ParseLifeTime(lifetime));

            // Assert
            Assert.Equal(lifetime, principal.GetClaim(OpenIddictConstants.Claims.Private.AuthorizationCodeLifetime));
        }

        [Fact]
        public void SetIdentityTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetIdentityTokenLifetime(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void SetIdentityTokenLifetime_AddsLifetime(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetIdentityTokenLifetime(ParseLifeTime(lifetime));

            // Assert
            Assert.Equal(lifetime, principal.GetClaim(OpenIddictConstants.Claims.Private.IdentityTokenLifetime));
        }

        [Fact]
        public void SetRefreshTokenLifetime_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetRefreshTokenLifetime(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("62")]
        public void SetRefreshTokenLifetime_AddsLifetime(string lifetime)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetRefreshTokenLifetime(ParseLifeTime(lifetime));

            // Assert
            Assert.Equal(lifetime, principal.GetClaim(OpenIddictConstants.Claims.Private.RefreshTokenLifetime));
        }

        [Fact]
        public void SetInternalTokenId_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetInternalTokenId(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("identifier")]
        public void SetInternalTokenId_AddsScopes(string identifier)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetInternalTokenId(identifier);

            // Assert
            Assert.Equal(identifier, principal.GetClaim(OpenIddictConstants.Claims.Private.TokenId));
        }

        [Fact]
        public void SetTokenUsage_ThrowsAnExceptionForNullPrincipal()
        {
            // Arrange
            var principal = (ClaimsPrincipal) null;

            // Act and assert
            var exception = Assert.Throws<ArgumentNullException>(() => principal.SetTokenUsage(null));

            Assert.Equal("principal", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("usage")]
        public void SetTokenUsage_AddsScopes(string usage)
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            principal.SetTokenUsage(usage);

            // Assert
            Assert.Equal(usage, principal.GetClaim(OpenIddictConstants.Claims.Private.TokenUsage));
        }

        private TimeSpan? ParseLifeTime(string lifetime)
        {
            var lifeT = lifetime != null
                ? (TimeSpan?)TimeSpan.FromSeconds(double.Parse(lifetime, NumberStyles.Number, CultureInfo.InvariantCulture))
                : null;

            return lifeT;
        }
    }
}
