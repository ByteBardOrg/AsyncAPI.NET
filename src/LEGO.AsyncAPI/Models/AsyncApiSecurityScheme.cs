// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    public class AsyncApiSecurityScheme : IAsyncApiSerializable, IAsyncApiExtensible
    {
        public static AsyncApiSecurityScheme UserPassword(string description = null) => new()
        {
            Type = SecuritySchemeType.UserPassword,
            Description = description,
        };

        public static AsyncApiSecurityScheme ApiKey(ParameterLocation @in, string description = null) => new()
        {
            Type = SecuritySchemeType.ApiKey,
            Description = description,
            In = @in,
        };

        public static AsyncApiSecurityScheme X509(string description = null) => new()
        {
            Type = SecuritySchemeType.X509,
            Description = description,
        };

        public static AsyncApiSecurityScheme SymmetricEncryption(string description = null) => new()
        {
            Type = SecuritySchemeType.SymmetricEncryption,
            Description = description,
        };

        public static AsyncApiSecurityScheme AsymmetricEncryption(string description = null) => new()
        {
            Type = SecuritySchemeType.AsymmetricEncryption,
            Description = description,
        };

        public static AsyncApiSecurityScheme HttpApiKey(ParameterLocation @in, string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.");
            }

            return new AsyncApiSecurityScheme
            {
                Type = SecuritySchemeType.HttpApiKey,
                Description = description,
                Name = name,
                In = @in,
            };
        }

        public static AsyncApiSecurityScheme Http(string scheme, string bearerFormat = null, string description = null)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException($"'{nameof(scheme)}' cannot be null or whitespace.");
            }

            return new AsyncApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Description = description,
                Scheme = scheme,
                BearerFormat = bearerFormat,
            };
        }

        public static AsyncApiSecurityScheme OAuth2(AsyncApiOAuthFlows flows, string[] scopes = null, string description = null)
        {
            if (flows is null)
            {
                throw new ArgumentException($"'{nameof(flows)}' cannot be null.");
            }

            return new AsyncApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = description,
                Flows = flows,
                Scopes = scopes?.ToHashSet() ?? new HashSet<string>(),
            };
        }

        public static AsyncApiSecurityScheme OpenIdConnect(Uri openIdConnectUrl, string description = null)
        {
            if (openIdConnectUrl is null)
            {
                throw new ArgumentException($"'{nameof(openIdConnectUrl)}' cannot be null.");
            }

            if (!openIdConnectUrl.IsAbsoluteUri)
            {
                throw new ArgumentException($"'{nameof(openIdConnectUrl)}' must be an absolute URI.");
            }

            return new AsyncApiSecurityScheme
            {
                Type = SecuritySchemeType.OpenIdConnect,
                Description = description,
                OpenIdConnectUrl = openIdConnectUrl,
            };
        }

        public static AsyncApiSecurityScheme Plain(string description = null) => new()
        {
            Type = SecuritySchemeType.Plain,
            Description = description,
        };

        public static AsyncApiSecurityScheme ScramSha256(string description = null) => new()
        {
            Type = SecuritySchemeType.ScramSha256,
            Description = description,
        };

        public static AsyncApiSecurityScheme ScramSha512(string description = null) => new()
        {
            Type = SecuritySchemeType.ScramSha512,
            Description = description,
        };

        public static AsyncApiSecurityScheme Gssapi(string description = null) => new()
        {
            Type = SecuritySchemeType.Gssapi,
            Description = description,
        };

        /// <summary>
        /// REQUIRED. The type of the security scheme. Valid values are "userPassword", "apiKey", "X509", "symmetricEncryption", "asymmetricEncryption", "httpApiKey", "http", "oauth2", "openIdConnect", "plain", "scramSha256", "scramSha512", and "gssapi".
        /// </summary>
        public virtual SecuritySchemeType Type { get; set; }

        /// <summary>
        /// A short description for security scheme. CommonMark syntax MAY be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// REQUIRED. The name of the header, query or cookie parameter to be used.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// REQUIRED. The location of the API key. Valid values are "user" and "password" for apiKey and "query", "header" or "cookie" for httpApiKey.
        /// </summary>
        public virtual ParameterLocation? In { get; set; }

        /// <summary>
        /// REQUIRED. The name of the HTTP Authorization scheme to be used
        /// in the Authorization header as defined in RFC7235.
        /// </summary>
        public virtual string Scheme { get; set; }

        /// <summary>
        /// A hint to the client to identify how the bearer token is formatted.
        /// Bearer tokens are usually generated by an authorization server,
        /// so this information is primarily for documentation purposes.
        /// </summary>
        public virtual string BearerFormat { get; set; }

        /// <summary>
        /// REQUIRED. An object containing configuration information for the flow types supported.
        /// </summary>
        public virtual AsyncApiOAuthFlows Flows { get; set; }

        /// <summary>
        /// REQUIRED. OpenId Connect URL to discover OAuth2 configuration values.
        /// </summary>
        public virtual Uri OpenIdConnectUrl { get; set; }

        /// <summary>
        /// List of the needed scope names. An empty array means no scopes are needed.
        /// </summary>
        public virtual ISet<string> Scopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Specification Extensions.
        /// </summary>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        /// <summary>
        /// Serialize to AsyncApi V2 document without using reference.
        /// </summary>
        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();

            // type
            writer.WriteRequiredProperty(AsyncApiConstants.Type, this.Type.GetDisplayName());

            // description
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            switch (this.Type)
            {
                case SecuritySchemeType.UserPassword:
                    break;
                case SecuritySchemeType.ApiKey:
                    writer.WriteOptionalProperty(AsyncApiConstants.In, this.In.GetDisplayName());
                    break;
                case SecuritySchemeType.X509:
                    break;
                case SecuritySchemeType.SymmetricEncryption:
                    break;
                case SecuritySchemeType.AsymmetricEncryption:
                    break;
                case SecuritySchemeType.HttpApiKey:
                    writer.WriteOptionalProperty(AsyncApiConstants.Name, this.Name);
                    writer.WriteOptionalProperty(AsyncApiConstants.In, this.In.GetDisplayName());
                    break;
                case SecuritySchemeType.Http:
                    writer.WriteOptionalProperty(AsyncApiConstants.Scheme, this.Scheme);
                    writer.WriteOptionalProperty(AsyncApiConstants.BearerFormat, this.BearerFormat);
                    break;
                case SecuritySchemeType.OAuth2:
                    writer.WriteOptionalObject(AsyncApiConstants.Flows, this.Flows, (w, o) => o.SerializeV2(w));
                    break;
                case SecuritySchemeType.OpenIdConnect:
                    writer.WriteOptionalProperty(AsyncApiConstants.OpenIdConnectUrl, this.OpenIdConnectUrl?.ToString());
                    break;
                case SecuritySchemeType.Plain:
                    break;
                case SecuritySchemeType.ScramSha256:
                    break;
                case SecuritySchemeType.ScramSha512:
                    break;
                case SecuritySchemeType.Gssapi:
                    break;
                default:
                    break;
            }

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();

            // type
            writer.WriteRequiredProperty(AsyncApiConstants.Type, this.Type.GetDisplayName());

            // description
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            switch (this.Type)
            {
                case SecuritySchemeType.UserPassword:
                    break;
                case SecuritySchemeType.ApiKey:
                    writer.WriteOptionalProperty(AsyncApiConstants.In, this.In.GetDisplayName());
                    break;
                case SecuritySchemeType.X509:
                    break;
                case SecuritySchemeType.SymmetricEncryption:
                    break;
                case SecuritySchemeType.AsymmetricEncryption:
                    break;
                case SecuritySchemeType.HttpApiKey:
                    writer.WriteOptionalProperty(AsyncApiConstants.Name, this.Name);
                    writer.WriteOptionalProperty(AsyncApiConstants.In, this.In.GetDisplayName());
                    break;
                case SecuritySchemeType.Http:
                    writer.WriteOptionalProperty(AsyncApiConstants.Scheme, this.Scheme);
                    writer.WriteOptionalProperty(AsyncApiConstants.BearerFormat, this.BearerFormat);
                    break;
                case SecuritySchemeType.OAuth2:
                    writer.WriteRequiredObject(AsyncApiConstants.Flows, this.Flows, (w, o) => o.SerializeV2(w));
                    writer.WriteOptionalCollection(AsyncApiConstants.Scopes, this.Scopes, (writer, s) => writer.WriteValue(s));
                    break;
                case SecuritySchemeType.OpenIdConnect:
                    writer.WriteOptionalProperty(AsyncApiConstants.OpenIdConnectUrl, this.OpenIdConnectUrl?.ToString());
                    writer.WriteOptionalCollection(AsyncApiConstants.Scopes, this.Scopes, (writer, s) => writer.WriteValue(s));
                    break;
                case SecuritySchemeType.Plain:
                    break;
                case SecuritySchemeType.ScramSha256:
                    break;
                case SecuritySchemeType.ScramSha512:
                    break;
                case SecuritySchemeType.Gssapi:
                    break;
                default:
                    break;
            }

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}