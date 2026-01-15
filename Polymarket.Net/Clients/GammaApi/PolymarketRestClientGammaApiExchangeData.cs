using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Enums;
using Polymarket.Net.Interfaces.Clients.GammaApi;
using Polymarket.Net.Objects.Internal;
using Polymarket.Net.Objects.Models;

namespace Polymarket.Net.Clients.GammaApi
{
    /// <inheritdoc />
    internal class PolymarketRestClientGammaApiExchangeData : IPolymarketRestClientGammaApiExchangeData
    {
        private readonly PolymarketRestClientGammaApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal PolymarketRestClientGammaApiExchangeData(ILogger logger, PolymarketRestClientGammaApi baseClient)
        {
            _baseClient = baseClient;
        
        }

        #region Get Sport Teams

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketSportsTeam[]>> GetSportTeamsAsync(
            IEnumerable<string>? league = null,
            IEnumerable<string>? name = null,
            IEnumerable<string>? abbreviation = null,
            int? limit = null,
            int? offset = null,
            IEnumerable<string>? orderBy = null,
            bool? ascending = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("order", orderBy);
            parameters.AddOptional("league", league);
            parameters.AddOptional("name", name);
            parameters.AddOptional("abbreviation", abbreviation);
            parameters.AddOptionalBoolString("ascending", ascending);
            parameters.Add("limit", limit ?? 20);
            parameters.Add("offset", offset ?? 0);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "teams", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketSportsTeam[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Sports

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketSport[]>> GetSportsAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, "sports", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketSport[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Sport Market Types

        /// <inheritdoc />
        public async Task<WebCallResult<string[]>> GetSportMarketTypesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, "sports/market-types", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            var result = await _baseClient.SendAsync<PolymarketSportMarketTypes>(request, parameters, ct).ConfigureAwait(false);
            return result.As<string[]>(result.Data?.MarketTypes);
        }

        #endregion

        #region Get Tags

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketTag[]>> GetTagsAsync(
            bool? includeTemplate = null,
            bool? isCarousel = null,
            int? limit = null,
            int? offset = null,
            IEnumerable<string>? orderBy = null,
            bool? ascending = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("order", orderBy);
            parameters.AddOptionalBoolString("includeTemplate", includeTemplate);
            parameters.AddOptionalBoolString("isCarousel", isCarousel);
            parameters.AddOptionalBoolString("ascending", ascending);
            parameters.Add("limit", limit ?? 20);
            parameters.Add("offset", offset ?? 0);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "tags", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketTag[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Tag By Id

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketTag>> GetTagByIdAsync(string id, bool? includeTemplate = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", includeTemplate);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "tags/" + id, PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketTag>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Tag By Slug

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketTag>> GetTagBySlugAsync(string slug, bool? includeTemplate = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", includeTemplate);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "tags/slug/" + slug, PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketTag>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Related Tags By Id

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketRelatedTag[]>> GetRelatedTagsByIdAsync(string id, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", omitEmpty);
            parameters.AddOptionalEnum("status", status);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"tags/{id}/related-tags", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketRelatedTag[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Related Tags By Slug

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketRelatedTag[]>> GetRelatedTagsBySlugAsync(string slug, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", omitEmpty);
            parameters.AddOptionalEnum("status", status);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"tags/slug/{slug}/related-tags", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketRelatedTag[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Related Tags By Slug

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketTag[]>> GetTagsRelatedToTagByIdAsync(string id, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", omitEmpty);
            parameters.AddOptionalEnum("status", status);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"tags/{id}/related-tags/tags", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketTag[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Related Tags By Slug

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketTag[]>> GetTagsRelatedToTagBySlugAsync(string slug, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("includeTemplate", omitEmpty);
            parameters.AddOptionalEnum("status", status);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"tags/slug/{slug}/related-tags/tags", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketTag[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Events

        /// <inheritdoc />
        public async Task<WebCallResult<PolymarketEvent[]>> GetEventsAsync(
            long[]? ids = null,
            long? tagId = null,
            long[]? excludeTagIds = null,
            string[]? slugs = null,
            string? tagSlug = null,
            bool? relatedTags = null,
            bool? active = null,
            bool? archived = null,
            bool? featured = null,
            bool? cyom = null,
            bool? includeChat = null,
            bool? includeTemplate = null,
            bool? recurrence = null,
            bool? closed = null,
            decimal? liquidityMin = null,
            decimal? liquidityMax = null,
            decimal? volumeMin = null,
            decimal? volumeMax = null,
            DateTime? startTimeMin = null,
            DateTime? startTimeMax = null,
            DateTime? endTimeMin = null,
            DateTime? endTimeMax = null,
            int? limit = null,
            int? offset = null,
            IEnumerable<string>? orderBy = null,
            bool? ascending = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("id", ids);
            parameters.AddOptional("tag_id", tagId);
            parameters.AddOptional("exclude_tag_id", excludeTagIds);
            parameters.AddOptional("slug", slugs);
            parameters.AddOptional("tag_slug", tagSlug);
            parameters.AddOptionalBoolString("related_tags", relatedTags);
            parameters.AddOptionalBoolString("active", active);
            parameters.AddOptionalBoolString("archived", archived);
            parameters.AddOptionalBoolString("featured", featured);
            parameters.AddOptionalBoolString("cyom", cyom);
            parameters.AddOptionalBoolString("include_chat", includeChat);
            parameters.AddOptionalBoolString("include_template", includeTemplate);
            parameters.AddOptionalBoolString("recurrence", recurrence);
            parameters.AddOptionalBoolString("closed", closed);
            parameters.AddOptionalString("liquidity_min", liquidityMin);
            parameters.AddOptionalString("liquidity_max", liquidityMax);
            parameters.AddOptionalString("volume_min", volumeMin);
            parameters.AddOptionalString("volume_max", volumeMax);
            parameters.AddOptionalString("start_data_min", startTimeMin);
            parameters.AddOptionalString("start_data_max", startTimeMax);
            parameters.AddOptionalString("end_data_min", endTimeMin);
            parameters.AddOptionalString("end_data_max", endTimeMax);

            parameters.AddOptionalCommaSeparated("order", orderBy);
            parameters.AddOptionalBoolString("ascending", ascending);
            parameters.Add("limit", limit ?? 20);
            parameters.Add("offset", offset ?? 0);
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"events", PolymarketExchange.RateLimiter.Polymarket, 1, false);
            return await _baseClient.SendAsync<PolymarketEvent[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
