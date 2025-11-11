using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Application.Services.Strategy.Backtesting;

/// <summary>
/// description(설명) : Trade Simulator for Backtesting (백테스팅용 거래 시뮬레이터)
/// Details(상세설명) : Simulates order execution, position management, and P&L calculation for backtesting
/// Applied technology patterns(적용기술패턴) : Simulation Pattern, State Management, Financial Calculations
/// </summary>
public class TradeSimulator
{
    private readonly decimal _commissionRate;
    private readonly decimal _slippagePercent;

    // Current state
    private decimal _cash;
    private decimal _equity;
    private readonly Dictionary<string, Position> _positions;
    private readonly List<BacktestTrade> _completedTrades;
    private readonly List<EquityCurvePoint> _equityCurve;

    public TradeSimulator(decimal initialCapital, decimal commissionRate = 0.00015m, decimal slippagePercent = 0.001m)
    {
        _cash = initialCapital;
        _equity = initialCapital;
        _commissionRate = commissionRate;
        _slippagePercent = slippagePercent;

        _positions = new Dictionary<string, Position>();
        _completedTrades = new List<BacktestTrade>();
        _equityCurve = new List<EquityCurvePoint>();
    }

    /// <summary>
    /// Execute a buy order
    /// </summary>
    public bool ExecuteBuyOrder(string stockCode, decimal price, int quantity, DateTime time)
    {
        // Apply slippage (buy at slightly higher price)
        var executionPrice = price * (1 + _slippagePercent);

        // Calculate commission
        var orderValue = executionPrice * quantity;
        var commission = orderValue * _commissionRate;
        var totalCost = orderValue + commission;

        // Check if enough cash
        if (totalCost > _cash)
            return false;

        // Execute order
        _cash -= totalCost;

        // Update or create position
        if (_positions.TryGetValue(stockCode, out var existingPosition))
        {
            // Add to existing position (average price)
            var totalQuantity = existingPosition.Quantity + quantity;
            var totalValue = (existingPosition.AveragePrice * existingPosition.Quantity) + orderValue;
            existingPosition.AveragePrice = totalValue / totalQuantity;
            existingPosition.Quantity = totalQuantity;
        }
        else
        {
            _positions[stockCode] = new Position
            {
                StockCode = stockCode,
                Quantity = quantity,
                AveragePrice = executionPrice,
                EntryTime = time
            };
        }

        UpdateEquity(time);
        return true;
    }

    /// <summary>
    /// Execute a sell order
    /// </summary>
    public bool ExecuteSellOrder(string stockCode, decimal price, int quantity, DateTime time)
    {
        // Check if position exists
        if (!_positions.TryGetValue(stockCode, out var position))
            return false;

        // Check if enough quantity
        if (position.Quantity < quantity)
            return false;

        // Apply slippage (sell at slightly lower price)
        var executionPrice = price * (1 - _slippagePercent);

        // Calculate commission
        var orderValue = executionPrice * quantity;
        var commission = orderValue * _commissionRate;
        var proceeds = orderValue - commission;

        // Calculate realized P&L
        var costBasis = position.AveragePrice * quantity;
        var realizedPL = (executionPrice * quantity) - costBasis - commission;
        var realizedPLPercent = (realizedPL / costBasis) * 100;

        // Execute order
        _cash += proceeds;

        // Record trade
        _completedTrades.Add(new BacktestTrade
        {
            StockCode = stockCode,
            EntryTime = position.EntryTime,
            ExitTime = time,
            EntryPrice = position.AveragePrice,
            ExitPrice = executionPrice,
            Quantity = quantity,
            RealizedPL = realizedPL,
            RealizedPLPercent = realizedPLPercent
        });

        // Update position
        position.Quantity -= quantity;
        if (position.Quantity == 0)
        {
            _positions.Remove(stockCode);
        }

        UpdateEquity(time);
        return true;
    }

    /// <summary>
    /// Update equity based on current positions and market prices
    /// </summary>
    public void UpdateEquity(DateTime time, Dictionary<string, decimal>? currentPrices = null)
    {
        var positionValue = 0m;

        if (currentPrices != null)
        {
            foreach (var position in _positions.Values)
            {
                if (currentPrices.TryGetValue(position.StockCode, out var currentPrice))
                {
                    positionValue += currentPrice * position.Quantity;
                }
                else
                {
                    // Use average price if current price not available
                    positionValue += position.AveragePrice * position.Quantity;
                }
            }
        }
        else
        {
            // Use average prices
            foreach (var position in _positions.Values)
            {
                positionValue += position.AveragePrice * position.Quantity;
            }
        }

        var previousEquity = _equity;
        _equity = _cash + positionValue;

        // Calculate drawdown
        var peak = _equityCurve.Count > 0
            ? Math.Max(_equityCurve.Max(p => p.Equity), _equity)
            : _equity;

        var drawdown = peak - _equity;
        var drawdownPercent = peak > 0 ? (drawdown / peak) * 100 : 0;

        _equityCurve.Add(new EquityCurvePoint
        {
            Time = time,
            Equity = _equity,
            Drawdown = drawdown,
            DrawdownPercent = drawdownPercent
        });
    }

    /// <summary>
    /// Close all open positions at market prices
    /// </summary>
    public void CloseAllPositions(Dictionary<string, decimal> currentPrices, DateTime time)
    {
        var positionsToClose = _positions.Values.ToList();

        foreach (var position in positionsToClose)
        {
            if (currentPrices.TryGetValue(position.StockCode, out var price))
            {
                ExecuteSellOrder(position.StockCode, price, position.Quantity, time);
            }
            else
            {
                // Use average price if current price not available
                ExecuteSellOrder(position.StockCode, position.AveragePrice, position.Quantity, time);
            }
        }
    }

    /// <summary>
    /// Get current account state
    /// </summary>
    public AccountState GetAccountState()
    {
        return new AccountState
        {
            Cash = _cash,
            Equity = _equity,
            OpenPositions = _positions.Count,
            CompletedTrades = _completedTrades.Count
        };
    }

    /// <summary>
    /// Get all completed trades
    /// </summary>
    public IReadOnlyList<BacktestTrade> GetCompletedTrades() => _completedTrades.AsReadOnly();

    /// <summary>
    /// Get equity curve
    /// </summary>
    public IReadOnlyList<EquityCurvePoint> GetEquityCurve() => _equityCurve.AsReadOnly();

    /// <summary>
    /// Get final equity
    /// </summary>
    public decimal GetFinalEquity() => _equity;

    /// <summary>
    /// Get open positions
    /// </summary>
    public IReadOnlyDictionary<string, Position> GetOpenPositions() => _positions;
}

/// <summary>
/// Position information
/// </summary>
public class Position
{
    public string StockCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public DateTime EntryTime { get; set; }
}

/// <summary>
/// Account state snapshot
/// </summary>
public class AccountState
{
    public decimal Cash { get; set; }
    public decimal Equity { get; set; }
    public int OpenPositions { get; set; }
    public int CompletedTrades { get; set; }
}
