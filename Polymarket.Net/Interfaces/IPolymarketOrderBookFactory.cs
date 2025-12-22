using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using Polymarket.Net.Objects.Options;

namespace Polymarket.Net.Interfaces
{
    /// <summary>
    /// Polymarket local order book factory
    /// </summary>
    public interface IPolymarketOrderBookFactory
    {
        
        /// <summary>
        /// Clob order book factory methods
        /// </summary>
        IOrderBookFactory<PolymarketOrderBookOptions> Clob { get; }


        /// <summary>
        /// Create a SymbolOrderBook for the symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="options">Book options</param>
        /// <returns></returns>
        ISymbolOrderBook Create(SharedSymbol symbol, Action<PolymarketOrderBookOptions>? options = null);

        
        /// <summary>
        /// Create a new Clob local order book instance
        /// </summary>
        ISymbolOrderBook CreateClob(string symbol, Action<PolymarketOrderBookOptions>? options = null);

    }
}