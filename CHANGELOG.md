# Changelog

Package versions follow this library's semantic versioning and do not correspond directly to AsyncAPI specification versions. For example, package 2.0.0 introduced the AsyncAPI 3.0 object model, while package 3.0.0 later moved V3 document output to AsyncAPI 3.1.0.

## [3.0.1](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v3.0.0...v3.0.1) (2026-08-02)

### Bug Fixes

* corrected security-scheme validation so `name` is required for `HttpApiKey` rather than `ApiKey` schemes ([#36](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/36)) ([0496eb5](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/0496eb5549abf3095e960ed870fa25eb71f463c6))

## [3.0.0](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v2.1.2...v3.0.0) (2026-06-10)

Package 3.0.0 builds on the shared AsyncAPI 2.6/3.0 object model introduced in package 2.0.0. This release moves V3 output to AsyncAPI 3.1.0 and introduces breaking binding and Avro API changes.

### AsyncAPI 3.1

* V3 documents now serialize and normalize to `asyncapi: 3.1.0`; the existing `AsyncApiVersion.AsyncApi3_0` selector continues to represent the V3 specification family ([1985b38](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/1985b385949ef2b4b7195f72f7534415f210d8b7))
* referenced schemas, servers, channels, messages, parameters, traits, security schemes, and binding components are now deserialized using the containing document's AsyncAPI version instead of always using V2 rules ([eeb9d8f](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/eeb9d8f8aedd811b2fc4c32c692868ff47a00824))
* root servers are registered under `#/servers/{name}` so channel server references and root-server aliases resolve consistently through `AsyncApiWorkspace` ([eeb9d8f](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/eeb9d8f8aedd811b2fc4c32c692868ff47a00824))
* V2 channel server entries now accept both bare server names and canonical `#/servers/...` references without double-prefixing the path ([eeb9d8f](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/eeb9d8f8aedd811b2fc4c32c692868ff47a00824))

### Binding Serialization

* binding collections now dispatch to the requested `SerializeV2` or `SerializeV3` implementation instead of silently using the V2 representation for both versions ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* V3 channels now serialize external documentation and bindings through their V3 serializers ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* added parent serialization context to `AsyncApiWorkspace`, allowing bindings to inspect their containing channel, operation, or message ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* HTTP operation bindings now infer V2 `type: request` or `type: response` from `AsyncApiOperation.Action`, omit `type` in V3, and use the matching V2 or V3 query-schema serializer ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* added `HttpMessageBinding.StatusCode`, emitted numerically for V3 and omitted when targeting V2 ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* HTTP bindings now default to binding version `0.2.0` for AsyncAPI V2 and `0.3.0` for V3 ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))
* AMQP, Kafka, MQTT, Pulsar, SNS, SQS, and WebSockets bindings now expose explicit V2 and V3 serialization paths while retaining their existing common representation ([#27](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/27)) ([7cea38b](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7cea38beda2ff5340020ca3c9262268c723a065f))

### Avro Schemas

* added `AvroNamedType` for Avro name references, kept distinct from AsyncAPI `$ref` references ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))
* added namespace-aware resolution of previously declared records, enums, fixed schemas, and named duration schemas ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))
* added support for self-recursive Avro records and named references within record fields, arrays, maps, and unions ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))
* expanded Avro maps to accept any `AsyncApiAvroSchema` as their value schema, including named and complex types ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))
* added recursive, cycle-safe Avro traversal and warnings for named types that cannot be resolved ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))
* added an explicit conversion from primitive `AsyncApiAvroSchema` values back to `AvroPrimitiveType` ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))

### Validation

* added `ValidationRule.ApplicableVersions`, `AsyncApiVersionRuleAttribute`, and version-filtered rule lookup so V2 and V3 rules run only against their intended document family ([#25](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/25)) ([5e4ed51](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/5e4ed51045b97f595d2e4d6287406b227763d43d))
* stopped applying V3-only operation action, channel-reference, and message-subset rules to operations upgraded from V2 documents ([#25](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/25)) ([5e4ed51](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/5e4ed51045b97f595d2e4d6287406b227763d43d))
* added V2-specific validation requiring a non-empty Channels Object while allowing V3 documents to omit channels ([#25](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/25)) ([5e4ed51](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/5e4ed51045b97f595d2e4d6287406b227763d43d))
* fragment validation warnings are now reported through `AsyncApiDiagnostic.Warnings` instead of being promoted to errors ([#32](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/32)) ([310868a](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/310868a6e6cb9d0a4feba3ba0b473c031e1d494e))

### Framework Support

* added `net9.0` and `net10.0` package targets while retaining `netstandard2.0`, `netstandard2.1`, and `net8.0` ([#33](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/33)) ([723a99d](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/723a99dc487fe39708803c394facd948899a8f68))

### Bug Fixes

* corrected variable handling, nesting, and reference-name behavior during document deserialization ([#29](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/29)) ([eeb9d8f](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/eeb9d8f8aedd811b2fc4c32c692868ff47a00824))

### BREAKING CHANGES

* custom `AsyncApiBinding` implementations must now override both `SerializeV2` and `SerializeV3`; `SerializeProperties` alone is no longer sufficient.
* `HttpOperationBinding.Type` was removed. Set the containing `AsyncApiOperation.Action`; V2 output derives `request` or `response` from that action.
* **avro:** `AvroMap.Values` now uses `AsyncApiAvroSchema` instead of `AvroPrimitiveType`.

## [2.1.2](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v2.1.1...v2.1.2) (2025-12-21)

### Reference Handling

* added base-URI-aware loading for relative external references in readers and stream loaders ([#21](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/21)) ([07d912d](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/07d912dbfe3a5a2500f066477387b278e4cca824))
* added `AsyncApiReaderSettings.BaseUri` for resolving relative references ([#21](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/21)) ([07d912d](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/07d912dbfe3a5a2500f066477387b278e4cca824))
* made `AsyncApiWalker` safely traverse operations whose optional reply address or channel reference is absent ([#23](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/23)) ([7085d23](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/7085d23f7441fe87645562192b0efe3c8f85c9f1))

### BREAKING CHANGES

* custom `IStreamLoader` implementations must accept both the base URI and requested URI in `Load(Uri baseUri, Uri uri)` and `LoadAsync(Uri baseUri, Uri uri)`.

## [2.1.1](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v2.1.0...v2.1.1) (2025-08-28)

### Serialization and Validation

* corrected discriminator serialization to use the expected constant name ([b9657e0](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/b9657e0e8c0ad1e82d54c40cec1f34d50402e5e1))
* converted out-of-range integer constraints in JSON Schema into reader diagnostics instead of unhandled parsing failures, and corrected non-negative numeric validation ([cf9f212](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/cf9f212e9e34c21eabfb2e5ada3949255950c205))

## [2.1.0](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v2.0.1...v2.1.0) (2025-08-08)

### AsyncAPI 3 Improvements

* added pluggable schema parsers for custom multi-format schema payloads ([#13](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/13)) ([d852d02](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/d852d025a6fae626a1e2e008b77a2a479eaae7df))
* fixed parameter reference resolution while upgrading V2 documents into the shared V3-shaped model ([#15](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/15)) ([ccd6ff3](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/ccd6ff3a1e5359512ab12b9a8d1375049839571b))
* corrected required AMQP binding properties in the shared V2/V3 binding model ([#17](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/17)) ([34e2733](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/34e273346b3e09c1fdacbdbc77254740e1b14e29))

### BREAKING CHANGES

* `AMQPOperationBinding.Expiration`, `Priority`, `DeliveryMode`, `Mandatory`, `Timestamp`, and `Ack` are now non-nullable and serialize as required properties; `UserId` is also emitted as required.
* `AsyncApiSchemaDeserializer` was renamed to `AsyncApiJsonSchemaDeserializer` as part of the pluggable schema-parser API.

## [2.0.1](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v2.0.0...v2.0.1) (2025-05-31)

### Bug Fixes

* re-add the .NET Standard target to ensure source generator compatibility ([b28d6ff](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/b28d6ff2a3c7b9df13d2ed9bab88e38593280ce6))

## [2.0.0](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v1.0.0...v2.0.0) (2025-05-25)

Package 2.0.0 replaced the V2-shaped public API with a shared AsyncAPI 3.0-shaped object model. The same model can read AsyncAPI 2.x or 3.x documents and serialize back to either AsyncAPI 2.6 or 3.0. This package major version is independent of the AsyncAPI specification version.

### AsyncAPI 3.0 Object Model

* added native AsyncAPI 3.0 readers, writers, validation, and `AsyncApiVersion.AsyncApi3_0` selection ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* added root `AsyncApiDocument.Operations`; operations now use `AsyncApiOperation.Action` and an `AsyncApiChannelReference` instead of living under each channel as `Publish` and `Subscribe` ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* separated channel identity from its address through `AsyncApiChannel.Address` and added channel-level message maps, titles, summaries, tags, and external documentation ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* changed operations to reference a subset of their channel's messages through `IList<AsyncApiMessageReference>` and added operation titles and request/reply support ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* added `AsyncApiOperationReply`, `AsyncApiOperationReplyAddress`, their typed references, and reusable reply and reply-address components ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* moved root tags and external documentation to `AsyncApiInfo`, matching their AsyncAPI 3 placement ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))

### Messages and Schemas

* added `AsyncApiMultiFormatSchema` and `IAsyncApiSchema` so message payloads, headers, and reusable schemas can represent JSON Schema, Avro, and custom schema formats through one model ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* moved message `schemaFormat` into the payload's multi-format schema wrapper and changed message headers to use the same abstraction ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* changed component schemas from `AsyncApiJsonSchema` to `AsyncApiMultiFormatSchema` and added `AsyncApiMultiFormatSchemaReference` ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* added implicit conversion from `AsyncApiJsonSchema` to `AsyncApiMultiFormatSchema` to simplify JSON Schema construction ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))

### Servers, Components, and References

* replaced `AsyncApiServer.Url` with `Host` and `PathName`, and added server title, summary, and external documentation fields ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* expanded components with reusable operations, replies, reply addresses, external documentation, tags, server variables, and all four binding component categories ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* added typed references for operations, replies, reply addresses, tags, external documentation, and multi-format schemas ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* added value equality to `AsyncApiReference` and expanded workspace registration for V3 components, root channels, and channel messages ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))

### Security, Parameters, and Traits

* replaced `AsyncApiSecurityRequirement` with security scheme objects and references carrying required scopes; added V3 security scheme factories and separated required scopes from OAuth flow `AvailableScopes` ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* replaced parameter `Schema` with the V3 `Enum`, `Default`, and `Examples` fields, with V2 schema reconstruction during downgrade serialization ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* updated operation and message traits for V3 titles, security, and multi-format headers ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))

### V2 Compatibility

* V2 channel keys are retained as `AsyncApiChannel.Address`, while normalized channel identifiers are used by the shared V3-shaped model ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* V2 `subscribe` and `publish` operations are promoted to root operations with `Send` and `Receive` actions and channel references ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* V2 inline messages and `message.oneOf` entries are promoted into channel message maps and operation message references ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* V2 `operationId` and `messageId` values become operation and message dictionary keys in the shared model ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* V2 server URLs are split into V3 host and pathname fields when read and recombined when serialized back to V2 ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* V2 serialization reconstructs channel `subscribe` and `publish`, emits one message directly or multiple messages through `oneOf`, and flattens multi-format schemas back into V2 payload and component schema objects ([#8](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/8)) ([2b98f81](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/2b98f81c4adbd61f8b981bc2247e7f008f598310))
* operations with no explicit message references now infer their V2 message or `oneOf` entries from the referenced channel's message map ([#10](https://github.com/ByteBardOrg/AsyncAPI.NET/issues/10)) ([c201b13](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/c201b13a70f37d853aa063f74ff8a89faf2bf27f))

### BREAKING CHANGES

* The public object model now follows AsyncAPI 3.0. Existing applications must migrate channel operations, message schemas, server URLs, security requirements, parameters, traits, components, and references to the new shapes described above.
* `AsyncApiChannel.Publish` and `Subscribe` were removed; place operations in `AsyncApiDocument.Operations` and set their `Action` and `Channel`.
* `AsyncApiChannel.Servers` now contains `AsyncApiServerReference` values instead of server-name strings.
* `AsyncApiOperation.Message` was replaced by `Messages`, containing `AsyncApiMessageReference` values into the referenced channel's message map.
* `AsyncApiOperation.OperationId`, `AsyncApiOperationTrait.OperationId`, `AsyncApiMessage.MessageId`, and `AsyncApiMessageTrait.MessageId` were removed; use operation, channel-message, or component dictionary keys as identity.
* `AsyncApiMessage.Payload`, `AsyncApiMessage.Headers`, and `AsyncApiComponents.Schemas` now use `AsyncApiMultiFormatSchema`.
* `IAsyncApiMessagePayload` was replaced by `IAsyncApiSchema`; schema helper extensions now operate on the new interface.
* `AsyncApiServer.Url` was replaced by `Host` and `PathName`.
* `AsyncApiSecurityRequirement` was removed; use security scheme references with `Scopes`.
* `AsyncApiParameter.Schema` was replaced by `Enum`, `Default`, and `Examples`.
* `AsyncApiOAuthFlow.Scopes` was renamed to `AvailableScopes` to distinguish offered scopes from scopes required by a security-scheme reference.
* `AsyncApiReaderSettings.BaseUrl` was removed.
* `AsyncApiWriterSettings.InlineLocalReferences` is now controlled through `ReferenceInline` rather than being set directly.
* custom `IAsyncApiSerializable` implementations must implement `SerializeV3`.

## [1.0.1](https://github.com/ByteBardOrg/AsyncAPI.NET/compare/v1.0.0...v1.0.1) (2025-04-24)

### Bug Fixes

* missing `messageId` serialization ([0a1d70f](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/0a1d70f21802f0059b3f9a7ba4e215e5d27b450e))

## 1.0.0 (2025-03-28)

### Features

* AsyncAPI v2.6 full support ([f0ef397](https://github.com/ByteBardOrg/AsyncAPI.NET/commit/f0ef397944bfbc124c16b7769c2512fcf17d5fb9))
