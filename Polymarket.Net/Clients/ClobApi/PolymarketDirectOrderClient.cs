using Polymarket.Net.Converters;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

            var body = signedOrder.GetSubmitBody(_owner, timeInForce, postOnly, deferExecution);
            var headers = _authProvider.CreateL2Headers(HttpMethod.Post, OrderPath, body);

            using var request = new HttpRequestMessage(HttpMethod.Post, _orderUri)
            {
                Version = _httpClient.DefaultRequestVersion,
                VersionPolicy = _httpClient.DefaultVersionPolicy,
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            try
            {
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
                        "ServerError",
                        orderResult.Error,
                        rawBody,
                        orderResult);
                }

                return PolymarketDirectOrderResult.Succeeded(
                    response.StatusCode,
                    response.Version,
                    responseHeaders,
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
                orderResult = JsonSerializer.Deserialize(rawBody, PolymarketSourceGenerationContext.Default.PolymarketOrderResult);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string ResolveErrorMessage(string rawBody, string? fallback)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return fallback ?? "unknown";

            try
            {
                using var document = JsonDocument.Parse(rawBody);
                if (document.RootElement.TryGetProperty("error", out var errorProperty))
                    return errorProperty.GetString() ?? fallback ?? "unknown";
                if (document.RootElement.TryGetProperty("errorMsg", out var errorMsgProperty))
                    return errorMsgProperty.GetString() ?? fallback ?? "unknown";
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
            string? errorType,
            string? errorMessage,
            string rawBody,
            PolymarketOrderResult? data)
        {
            Success = success;
            ResponseStatusCode = responseStatusCode;
            ResponseVersion = responseVersion;
            ResponseHeaders = responseHeaders;
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
            string rawBody,
            PolymarketOrderResult data)
            => new(true, responseStatusCode, responseVersion, responseHeaders, null, null, rawBody, data);

        internal static PolymarketDirectOrderResult Failed(
            HttpStatusCode? responseStatusCode,
            Version? responseVersion,
            IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
            string errorType,
            string errorMessage,
            string rawBody,
            PolymarketOrderResult? data = null)
            => new(false, responseStatusCode, responseVersion, responseHeaders, errorType, errorMessage, rawBody, data);
    }
}
