namespace AlgoTrading.Core.ValueObjects.Market;

/// <summary>
/// Market Type Value Object
/// Represents a Korean stock market type
/// </summary>
public sealed class MarketType : ValueObject
{
    /// <summary>
    /// Market code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Market name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Market description
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Is derivative market
    /// </summary>
    public bool IsDerivative { get; }

    private MarketType(string code, string name, string description, bool isDerivative = false)
    {
        Code = code;
        Name = name;
        Description = description;
        IsDerivative = isDerivative;
    }

    /// <summary>
    /// KOSPI - Korea Stock Exchange
    /// </summary>
    public static MarketType KOSPI => new(
        "KOSPI",
        "코스피",
        "한국거래소 유가증권시장"
    );

    /// <summary>
    /// KOSDAQ - Korea Securities Dealers Automated Quotations
    /// </summary>
    public static MarketType KOSDAQ => new(
        "KOSDAQ",
        "코스닥",
        "한국거래소 코스닥시장"
    );

    /// <summary>
    /// KONEX - Korea New Exchange
    /// </summary>
    public static MarketType KONEX => new(
        "KONEX",
        "코넥스",
        "한국거래소 중소기업전용시장"
    );

    /// <summary>
    /// FUTURES - Futures Market
    /// </summary>
    public static MarketType FUTURES => new(
        "FUTURES",
        "선물",
        "파생상품시장 선물",
        isDerivative: true
    );

    /// <summary>
    /// OPTIONS - Options Market
    /// </summary>
    public static MarketType OPTIONS => new(
        "OPTIONS",
        "옵션",
        "파생상품시장 옵션",
        isDerivative: true
    );

    /// <summary>
    /// ETF - Exchange Traded Fund
    /// </summary>
    public static MarketType ETF => new(
        "ETF",
        "ETF",
        "상장지수펀드"
    );

    /// <summary>
    /// ETN - Exchange Traded Note
    /// </summary>
    public static MarketType ETN => new(
        "ETN",
        "ETN",
        "상장지수증권"
    );

    /// <summary>
    /// ELW - Equity Linked Warrant
    /// </summary>
    public static MarketType ELW => new(
        "ELW",
        "ELW",
        "주식워런트증권",
        isDerivative: true
    );

    /// <summary>
    /// All predefined market types
    /// </summary>
    public static IReadOnlyList<MarketType> All => new[]
    {
        KOSPI,
        KOSDAQ,
        KONEX,
        FUTURES,
        OPTIONS,
        ETF,
        ETN,
        ELW
    };

    /// <summary>
    /// Get market types for spot trading
    /// </summary>
    public static IReadOnlyList<MarketType> SpotMarkets => new[]
    {
        KOSPI,
        KOSDAQ,
        KONEX,
        ETF,
        ETN
    };

    /// <summary>
    /// Get market types for derivative trading
    /// </summary>
    public static IReadOnlyList<MarketType> DerivativeMarkets => new[]
    {
        FUTURES,
        OPTIONS,
        ELW
    };

    /// <summary>
    /// Create from code
    /// </summary>
    public static MarketType FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Market code cannot be empty", nameof(code));

        var market = All.FirstOrDefault(m => m.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (market == null)
            throw new ArgumentException($"Unknown market code: {code}", nameof(code));

        return market;
    }

    /// <summary>
    /// Try to create from code
    /// </summary>
    public static bool TryFromCode(string code, out MarketType? marketType)
    {
        try
        {
            marketType = FromCode(code);
            return true;
        }
        catch
        {
            marketType = null;
            return false;
        }
    }

    /// <summary>
    /// Check if this is a spot market
    /// </summary>
    public bool IsSpotMarket()
    {
        return !IsDerivative;
    }

    /// <summary>
    /// Check if trading is allowed for this market type
    /// </summary>
    public bool IsTradingAllowed()
    {
        // All current markets allow trading
        return true;
    }

    /// <summary>
    /// Get trading hours for this market
    /// </summary>
    public (TimeSpan Open, TimeSpan Close) GetTradingHours()
    {
        return Code switch
        {
            "KOSPI" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            "KOSDAQ" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            "KONEX" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            "ETF" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            "ETN" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            "FUTURES" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 45, 0)),
            "OPTIONS" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 45, 0)),
            "ELW" => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0)),
            _ => (new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 0))
        };
    }

    /// <summary>
    /// Get pre-market trading hours if available
    /// </summary>
    public (TimeSpan Open, TimeSpan Close)? GetPreMarketHours()
    {
        return Code switch
        {
            "KOSPI" => (new TimeSpan(8, 30, 0), new TimeSpan(9, 0, 0)),
            "KOSDAQ" => (new TimeSpan(8, 30, 0), new TimeSpan(9, 0, 0)),
            _ => null
        };
    }

    /// <summary>
    /// Get after-hours trading hours if available
    /// </summary>
    public (TimeSpan Open, TimeSpan Close)? GetAfterHours()
    {
        return Code switch
        {
            "KOSPI" => (new TimeSpan(15, 40, 0), new TimeSpan(16, 0, 0)),
            "KOSDAQ" => (new TimeSpan(15, 40, 0), new TimeSpan(16, 0, 0)),
            _ => null
        };
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{Code} ({Name})";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
