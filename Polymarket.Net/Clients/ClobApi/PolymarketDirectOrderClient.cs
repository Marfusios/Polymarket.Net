using Polymarket.Net.Enums;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Clients.ClobApi
{
    /// <summary>
    /// Minimal /order submitter for already-signed orders.
    /// </summary>
    public sealed class PolymarketDirectOrderClient
    {
        private const string OrderPath = "/order";

        private readonly HttpClient _httpClient;
        private readonly Uri _orderUri;
        private readonly PolymarketAuthenticationProvider _authProvider;
        private readonly string _owner;

        /// <summary>
        /// Create a direct order submitter using the supplied HTTP client.
        /// </summary>
        public PolymarketDirectOrderClient(
            HttpClient httpClient,
            PolymarketEnvironment environment,
            PolymarketCredentials credentials)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));
            if (string.IsNullOrWhiteSpace(credentials.L2ApiKey))
                throw new ArgumentException("Layer 2 credentials required", nameof(credentials));

            _orderUri = new Uri(environment.ClobRestClientAddress.TrimEnd('/') + OrderPath, UriKind.Absolute);
            _authProvider = new PolymarketAuthenticationProvider(credentials);
            _owner = credentials.L2ApiKey!;
        }

        /// <summary>
        /// Pre-serialize the /order request body for a signed order.
        /// </summary>
        public void PrepareSignedOrder(
            PreSignedOrder signedOrder,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null)
        {
            if (signedOrder == null)
                throw new ArgumentNullException(nameof(signedOrder));

            signedOrder.PrepareSubmitBody(_owner, timeInForce, postOnly, deferExecution);
        }

        /// <summary>
        /// Submit an already-signed order using a pre-serialized request body plus fresh L2 headers.
        /// </summary>
        public async Task<PolymarketDirectOrderResult> PlaceSignedOrderAsync(
            PreSignedOrder signedOrder,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null,
            CancellationToken ct = default)
        {
            if (signedOrder == null)
                throw new ArgumentNullException(nameof(signedOrder));

            // The HMAC signs the string form; the wire content reuses the UTF-8 bytes
            // cached at PrepareSubmitBody time, skipping a per-submit re-encode.
            var body = signedOrder.GetSubmitBody(_owner, timeInForce, postOnly, deferExecution);
            var bodyUtf8 = signedOrder.GetSubmitBodyUtf8(_owner, timeInForce, postOnly, deferExecution);
            var headers = _authProvider.CreateL2Headers(HttpMethod.Post, OrderPath, body);

            var content = new ByteArrayContent(bodyUtf8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
            using var request = new HttpRequestMessage(HttpMethod.Post, _orderUri)
            {
                Version = _httpClient.DefaultRequestVersion,
                VersionPolicy = _httpClient.DefaultVersionPolicy,
                Content = content
            };
            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            var httpSendStartedTimestamp = 0L;
            try
            {
                httpSendStartedTimestamp = Stopwatch.GetTimestamp();
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                var rawBody = response.Content == null
                    ? string.Empty
                    : await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var responseHeaders = CollectHeaders(response);

                if (!TryDeserializeOrderResult(rawBody, out var orderResult, out var deserializeError))
                {
                    return PolymarketDirectOrderResult.Failed(
                        response.StatusCode,
                        response.Version,
                        responseHeaders,
                        httpSendStartedTimestamp,
                        "DeserializeError",
                        ResolveErrorMessage(rawBody, deserializeError),
                        rawBody);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return PolymarketDirectOrderResult.Failed(
                        response.StatusCode,
                        response.Version,
                        responseHeaders,
                        httpSendStartedTimestamp,
                        "ServerError",
                        ResolveErrorMessage(rawBody, orderResult?.Error),
                        rawBody,
                        orderResult);
                }

                if (orderResult == null)
                {
                    return PolymarketDirectOrderResult.Failed(
                        response.StatusCode,
                        response.Version,
                        responseHeaders,
                        httpSendStartedTimestamp,
                        "DeserializeError",
                        "empty order response",
                        rawBody);
                }

                if (!string.IsNullOrEmpty(orderResult.Error))
                {
                    return PolymarketDirectOrderResult.Failed(
                        response.StatusCode,
                        response.Version,
                        responseHeaders,
                        httpSendStartedTimestamp,
                        "ServerError",
                        orderResult.Error,
                        rawBody,
                        orderResult);
                }

                return PolymarketDirectOrderResult.Succeeded(
                    response.StatusCode,
                    response.Version,
                    responseHeaders,
                    httpSendStartedTimestamp,
                    rawBody,
                    orderResult);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return PolymarketDirectOrderResult.Failed(
                    null,
                    null,
                    Array.Empty<KeyValuePair<string, IEnumerable<string>>>(),
                    httpSendStartedTimestamp,
                    ex.GetType().Name,
                    ex.Message,
                    rawBody: string.Empty);
            }
        }

        private static IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> CollectHeaders(HttpResponseMessage response)
        {
            var headers = new List<KeyValuePair<string, IEnumerable<string>>>();
            headers.AddRange(response.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value.ToArray())));
            if (response.Content != null)
                headers.AddRange(response.Content.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value.ToArray())));
            return headers;
        }

        private static bool TryDeserializeOrderResult(string rawBody, out PolymarketOrderResult? orderResult, out string? error)
        {
            orderResult = null;
            error = null;
            if (string.IsNullOrWhiteSpace(rawBody))
            {
                error = "empty response";
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(rawBody);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    error = "order response is not a JSON object";
                    return false;
                }

                orderResult = new PolymarketOrderResult
                {
                    Success = GetBooleanProperty(document.RootElement, "success"),
                    Error = GetStringProperty(document.RootElement, "errorMsg") ?? GetStringProperty(document.RootElement, "error"),
                    OrderId = GetStringProperty(document.RootElement, "orderID")
                        ?? GetStringProperty(document.RootElement, "orderId")
                        ?? GetStringProperty(document.RootElement, "id")
                        ?? string.Empty,
                    TakingQuantity = GetDecimalProperty(document.RootElement, "takingAmount"),
                    MakingQuantity = GetDecimalProperty(document.RootElement, "makingAmount"),
                    Status = GetOrderStatusProperty(document.RootElement, "status"),
                    TradeHashes = GetStringArrayProperty(document.RootElement, "transactionsHashes")
                };
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool GetBooleanProperty(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property))
                return false;

            return property.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(property.GetString(), out var value) && value,
                _ => false
            };
        }

        private static string? GetStringProperty(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
                return null;

            if (property.ValueKind == JsonValueKind.String)
                return property.GetString();

            return property.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Object or JsonValueKind.Array
                ? property.GetRawText()
                : null;
        }

        private static decimal? GetDecimalProperty(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
                return null;

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
                return number;

            if (property.ValueKind == JsonValueKind.String
                && decimal.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return parsed;

            return null;
        }

        private static OrderStatus? GetOrderStatusProperty(JsonElement root, string propertyName)
        {
            var raw = GetStringProperty(root, propertyName);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            return raw.Trim() switch
            {
                var value when value.Equals("LIVE", StringComparison.OrdinalIgnoreCase) => OrderStatus.Live,
                var value when value.Equals("CANCELED", StringComparison.OrdinalIgnoreCase) => OrderStatus.Canceled,
                var value when value.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) => OrderStatus.Canceled,
                var value when value.Equals("MATCHED", StringComparison.OrdinalIgnoreCase) => OrderStatus.Matched,
                var value when value.Equals("DELAYED", StringComparison.OrdinalIgnoreCase) => OrderStatus.Delayed,
                _ => null
            };
        }

        private static string[] GetStringArrayProperty(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
                return Array.Empty<string>();

            if (property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();
                return string.IsNullOrEmpty(value) ? Array.Empty<string>() : new[] { value };
            }

            if (property.ValueKind != JsonValueKind.Array)
                return Array.Empty<string>();

            var values = new List<string>();
            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var value = item.GetString();
                    if (!string.IsNullOrEmpty(value))
                        values.Add(value);
                }
            }

            return values.Count == 0 ? Array.Empty<string>() : values.ToArray();
        }

        private static string ResolveErrorMessage(string rawBody, string? fallback)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return fallback ?? "unknown";

            try
            {
                using var document = JsonDocument.Parse(rawBody);
                if (document.RootElement.TryGetProperty("error", out var errorProperty))
                {
                    var error = errorProperty.ValueKind == JsonValueKind.String
                        ? errorProperty.GetString()
                        : errorProperty.GetRawText();
                    if (!string.IsNullOrWhiteSpace(error))
                        return error;
                }

                if (document.RootElement.TryGetProperty("errorMsg", out var errorMsgProperty))
                {
                    var errorMsg = errorMsgProperty.ValueKind == JsonValueKind.String
                        ? errorMsgProperty.GetString()
                        : errorMsgProperty.GetRawText();
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        return errorMsg;
                }
            }
            catch
            {
            }

            var trimmed = rawBody.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? fallback ?? "unknown" : trimmed;
        }
    }

    /// <summary>
    /// Response metadata from the direct signed-order submitter.
    /// </summary>
    public sealed class PolymarketDirectOrderResult
    {
        private PolymarketDirectOrderResult(
            bool success,
            HttpStatusCode? responseStatusCode,
            Version? responseVersion,
            IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
            long httpSendStartedTimestamp,
            string? errorType,
            string? errorMessage,
            string rawBody,
            PolymarketOrderResult? data)
        {
            Success = success;
            ResponseStatusCode = responseStatusCode;
            ResponseVersion = responseVersion;
            ResponseHeaders = responseHeaders;
            HttpSendStartedTimestamp = httpSendStartedTimestamp;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
            RawBody = rawBody;
            Data = data;
        }

        /// <summary>True when HTTP and CLOB order processing both succeeded.</summary>
        public bool Success { get; }

        /// <summary>Parsed order result, when the response body could be parsed.</summary>
        public PolymarketOrderResult? Data { get; }

        /// <summary>HTTP status code returned by the CLOB endpoint.</summary>
        public HttpStatusCode? ResponseStatusCode { get; }

        /// <summary>Negotiated HTTP version reported by HttpClient.</summary>
        public Version? ResponseVersion { get; }

        /// <summary>Response headers from the CLOB endpoint.</summary>
        public IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders { get; }

        /// <summary>
        /// Monotonic <see cref="Stopwatch.GetTimestamp"/> value captured immediately before
        /// entering <see cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/>.
        /// </summary>
        public long HttpSendStartedTimestamp { get; }

        /// <summary>Short error category for logging.</summary>
        public string? ErrorType { get; }

        /// <summary>Human-readable error message.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Raw response body.</summary>
        public string RawBody { get; }

        internal static PolymarketDirectOrderResult Succeeded(
            HttpStatusCode responseStatusCode,
            Version responseVersion,
            IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
            long httpSendStartedTimestamp,
            string rawBody,
            PolymarketOrderResult data)
            => new(true, responseStatusCode, responseVersion, responseHeaders, httpSendStartedTimestamp, null, null, rawBody, data);

        internal static PolymarketDirectOrderResult Failed(
            HttpStatusCode? responseStatusCode,
            Version? responseVersion,
            IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
            long httpSendStartedTimestamp,
            string errorType,
            string errorMessage,
            string rawBody,
            PolymarketOrderResult? data = null)
            => new(false, responseStatusCode, responseVersion, responseHeaders, httpSendStartedTimestamp, errorType, errorMessage, rawBody, data);
    }
}
