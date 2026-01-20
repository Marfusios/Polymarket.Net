using CryptoExchange.Net.Objects.Errors;

namespace Polymarket.Net
{
    internal static class PolymarketErrors
    {
        public static ErrorMapping Errors { get; } = new ErrorMapping(
            [
                new ErrorInfo(ErrorType.Unauthorized, false, "Invalid signature, make sure your credentials are set correctly", "invalid signature"),

                new ErrorInfo(ErrorType.UnknownSymbol, false, "Unknown market", "market not found"),

                new ErrorInfo(ErrorType.InvalidQuantity, false, "Invalid order quantity tick size", "INVALID_ORDER_MIN_TICK_SIZE"),
                new ErrorInfo(ErrorType.InvalidQuantity, false, "Order quantity too small", "INVALID_ORDER_MIN_SIZE"),

                new ErrorInfo(ErrorType.InsufficientBalance, false, "Insufficient balance/allowance", "INVALID_ORDER_NOT_ENOUGH_BALANCE"),

                new ErrorInfo(ErrorType.RejectedOrderConfiguration, false, "Post only order failed", "INVALID_POST_ONLY_ORDER"),
                new ErrorInfo(ErrorType.RejectedOrderConfiguration, false, "FillOrKill order failed", "FOK_ORDER_NOT_FILLED_ERROR"),
            ]
            );
    }
}
