using CryptoExchange.Net.Objects;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Interfaces.Clients.GammaApi
{
    /// <summary>
    /// Polymarket Gamma exchange data endpoints. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    public interface IPolymarketRestClientGammaApiExchangeData
    {
        /// <summary>
        /// Get list of all sports teams
        /// <para><a href="https://docs.polymarket.com/api-reference/pricing/get-market-price" /></para>
        /// </summary>
        /// <param name="league">Filter by league name</param>
        /// <param name="name">Filter by name</param>
        /// <param name="abbreviation">Filter by abbreviation</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="offset">Result offset</param>
        /// <param name="orderBy">Order by fields</param>
        /// <param name="ascending">Ascending order</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketSportsTeam[]>> GetSportTeamsAsync(
            IEnumerable<string>? league = null,
            IEnumerable<string>? name = null,
            IEnumerable<string>? abbreviation = null,
            int? limit = null,
            int? offset = null,
            IEnumerable<string>? orderBy = null,
            bool? ascending = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get sports meta data
        /// <para><a href="https://docs.polymarket.com/api-reference/sports/get-sports-metadata-information" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketSport[]>> GetSportsAsync(CancellationToken ct = default);

        /// <summary>
        /// Get valid sport market types
        /// <para><a href="https://docs.polymarket.com/api-reference/sports/get-valid-sports-market-types" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<string[]>> GetSportMarketTypesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get tags
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/list-tags" /></para>
        /// </summary>
        /// <param name="includeTemplate">Include template</param>
        /// <param name="isCarousel">Filter by carousel</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="offset">Result offset</param>
        /// <param name="orderBy">Order by fields</param>
        /// <param name="ascending">Ascending order</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketTag[]>> GetTagsAsync(
            bool? includeTemplate = null,
            bool? isCarousel = null,
            int? limit = null,
            int? offset = null,
            IEnumerable<string>? orderBy = null,
            bool? ascending = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get tag by id
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-tag-by-id" /></para>
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="includeTemplate">Include template</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketTag>> GetTagByIdAsync(string id, bool? includeTemplate = null, CancellationToken ct = default);

        /// <summary>
        /// Get tag by slug
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-tag-by-id" /></para>
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="includeTemplate">Include template</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketTag>> GetTagBySlugAsync(string slug, bool? includeTemplate = null, CancellationToken ct = default);

        /// <summary>
        /// Get related tags for a tag
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-related-tags-relationships-by-tag-id" /></para>
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="omitEmpty">Omit empty</param>
        /// <param name="status">Filter by status</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketRelatedTag[]>> GetRelatedTagsByIdAsync(string id, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default);

        /// <summary>
        /// Get related tags for a tag
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-related-tags-relationships-by-tag-slug" /></para>
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="omitEmpty">Omit empty</param>
        /// <param name="status">Filter by status</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketRelatedTag[]>> GetRelatedTagsBySlugAsync(string slug, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default);

        /// <summary>
        /// Get tags related to a tag by id
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-related-tags-relationships-by-tag-id" /></para>
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="omitEmpty">Omit empty</param>
        /// <param name="status">Filter by status</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketTag[]>> GetTagsRelatedToTagByIdAsync(string id, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default);

        /// <summary>
        /// Get tags related to a tag by slug
        /// <para><a href="https://docs.polymarket.com/api-reference/tags/get-related-tags-relationships-by-tag-slug" /></para>
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="omitEmpty">Omit empty</param>
        /// <param name="status">Filter by status</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketTag[]>> GetTagsRelatedToTagBySlugAsync(string slug, bool? omitEmpty = null, TagStatus? status = null, CancellationToken ct = default);

        /// <summary>
        /// Get events
        /// <para><a href="https://docs.polymarket.com/api-reference/events/list-events" /></para>
        /// </summary>
        /// <param name="ids">Filter by ids</param>
        /// <param name="tagId">Filter by tag id</param>
        /// <param name="excludeTagIds">Filter by excluded tag ids</param>
        /// <param name="slugs">Filter by slugs</param>
        /// <param name="tagSlug">Filter by tag slug</param>
        /// <param name="relatedTags">Filter by related tags</param>
        /// <param name="active">Whether to return active</param>
        /// <param name="archived">Whether to return archived</param>
        /// <param name="featured">Whether to return featured</param>
        /// <param name="cyom">Whether to return cyom</param>
        /// <param name="includeChat">Whether to include chat</param>
        /// <param name="includeTemplate">Whether to include template</param>
        /// <param name="recurrence">Whether to return recurrence</param>
        /// <param name="closed">Whether to return closed</param>
        /// <param name="liquidityMin">Filter by min liquidity</param>
        /// <param name="liquidityMax">Filter by max liquidity</param>
        /// <param name="volumeMin">Filter by min volume</param>
        /// <param name="volumeMax">Filter by max volume</param>
        /// <param name="startTimeMin">Filter start time by min value</param>
        /// <param name="startTimeMax">Filter start time by max value</param>
        /// <param name="endTimeMin">Filter end time by min value</param>
        /// <param name="endTimeMax">Filter end time by max value</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="offset">Result offset</param>
        /// <param name="orderBy">Order by fields</param>
        /// <param name="ascending">Ascending order</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketEvent[]>> GetEventsAsync(
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
            CancellationToken ct = default);
    }
}
