using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Core.Entities.Strategy;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Services.Strategy.Evaluation;

/// <summary>
/// description(설명) : Rule Evaluation Engine (전략 규칙 평가 엔진)
/// Details(상세설명) : Evaluates strategy rule expressions against market data
/// Applied technology patterns(적용기술패턴) : Interpreter Pattern, Expression Evaluation
/// </summary>
public class RuleEvaluationEngine
{
    private readonly ILogger _logger;
    private readonly TechnicalIndicatorCalculator _indicatorCalculator;

    public RuleEvaluationEngine(ILogger logger)
    {
        _logger = logger;
        _indicatorCalculator = new TechnicalIndicatorCalculator();
    }

    /// <summary>
    /// Evaluate a strategy rule against historical price data
    /// </summary>
    public bool EvaluateRule(
        StrategyRule rule,
        List<HistoricalPriceData> historicalData,
        HistoricalPriceData currentPrice)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rule.Condition))
            {
                _logger.LogWarning("Rule {RuleName} has empty condition", rule.Name);
                return false;
            }

            // Create evaluation context
            var context = new EvaluationContext
            {
                HistoricalData = historicalData,
                CurrentPrice = currentPrice,
                IndicatorCalculator = _indicatorCalculator
            };

            // Parse and evaluate condition expression
            var result = ParseAndEvaluateExpression(rule.Condition, context);

            _logger.LogTrace("Rule {RuleName} evaluated to {Result} for {StockCode} at {Date}",
                rule.Name, result, currentPrice.StockCode, currentPrice.Date);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating rule {RuleName}: {Condition}",
                rule.Name, rule.Condition);
            return false;
        }
    }

    /// <summary>
    /// Parse and evaluate rule expression
    /// Supports simple expressions like:
    /// - price > sma(20)
    /// - rsi(14) < 30
    /// - close > open * 1.02
    /// - volume > avgVolume(20) * 1.5
    /// </summary>
    private bool ParseAndEvaluateExpression(string expression, EvaluationContext context)
    {
        // Normalize expression
        expression = expression.Trim().ToLowerInvariant();

        // Handle logical operators (AND, OR)
        if (expression.Contains(" and "))
        {
            var parts = expression.Split(" and ", StringSplitOptions.RemoveEmptyEntries);
            return parts.All(part => ParseAndEvaluateExpression(part.Trim(), context));
        }

        if (expression.Contains(" or "))
        {
            var parts = expression.Split(" or ", StringSplitOptions.RemoveEmptyEntries);
            return parts.Any(part => ParseAndEvaluateExpression(part.Trim(), context));
        }

        // Handle comparison operators
        if (expression.Contains(">="))
        {
            var parts = expression.Split(">=");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left >= right;
        }

        if (expression.Contains("<="))
        {
            var parts = expression.Split("<=");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left <= right;
        }

        if (expression.Contains(">"))
        {
            var parts = expression.Split(">");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left > right;
        }

        if (expression.Contains("<"))
        {
            var parts = expression.Split("<");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left < right;
        }

        if (expression.Contains("=="))
        {
            var parts = expression.Split("==");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return Math.Abs(left - right) < 0.0001m;
        }

        // If no operator found, try to evaluate as boolean
        return EvaluateValue(expression, context) > 0;
    }

    /// <summary>
    /// Evaluate a value expression (price, indicator, constant)
    /// </summary>
    private decimal EvaluateValue(string expression, EvaluationContext context)
    {
        expression = expression.Trim();

        // Handle mathematical operations
        if (expression.Contains(" * "))
        {
            var parts = expression.Split(" * ");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left * right;
        }

        if (expression.Contains(" / "))
        {
            var parts = expression.Split(" / ");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return right != 0 ? left / right : 0;
        }

        if (expression.Contains(" + "))
        {
            var parts = expression.Split(" + ");
            var left = EvaluateValue(parts[0].Trim(), context);
            var right = EvaluateValue(parts[1].Trim(), context);
            return left + right;
        }

        if (expression.Contains(" - "))
        {
            var parts = expression.Split(" - ");
            if (parts.Length == 2 && !string.IsNullOrEmpty(parts[0])) // Not a negative number
            {
                var left = EvaluateValue(parts[0].Trim(), context);
                var right = EvaluateValue(parts[1].Trim(), context);
                return left - right;
            }
        }

        // Try to parse as constant number
        if (decimal.TryParse(expression, out var constant))
        {
            return constant;
        }

        // Handle price fields
        switch (expression)
        {
            case "price":
            case "close":
                return context.CurrentPrice.Close;
            case "open":
                return context.CurrentPrice.Open;
            case "high":
                return context.CurrentPrice.High;
            case "low":
                return context.CurrentPrice.Low;
            case "volume":
                return context.CurrentPrice.Volume;
        }

        // Handle technical indicators
        if (expression.StartsWith("sma(") && expression.EndsWith(")"))
        {
            var periodStr = expression.Substring(4, expression.Length - 5);
            if (int.TryParse(periodStr, out var period))
            {
                return context.IndicatorCalculator.CalculateSMA(context.HistoricalData, period);
            }
        }

        if (expression.StartsWith("ema(") && expression.EndsWith(")"))
        {
            var periodStr = expression.Substring(4, expression.Length - 5);
            if (int.TryParse(periodStr, out var period))
            {
                return context.IndicatorCalculator.CalculateEMA(context.HistoricalData, period);
            }
        }

        if (expression.StartsWith("rsi(") && expression.EndsWith(")"))
        {
            var periodStr = expression.Substring(4, expression.Length - 5);
            if (int.TryParse(periodStr, out var period))
            {
                return context.IndicatorCalculator.CalculateRSI(context.HistoricalData, period);
            }
        }

        if (expression.StartsWith("macd"))
        {
            // MACD can return value or signal
            if (expression == "macd")
            {
                var (macd, signal, histogram) = context.IndicatorCalculator.CalculateMACD(context.HistoricalData);
                return macd;
            }
            else if (expression == "macdsignal")
            {
                var (macd, signal, histogram) = context.IndicatorCalculator.CalculateMACD(context.HistoricalData);
                return signal;
            }
            else if (expression == "macdhistogram")
            {
                var (macd, signal, histogram) = context.IndicatorCalculator.CalculateMACD(context.HistoricalData);
                return histogram;
            }
        }

        if (expression.StartsWith("bb"))
        {
            // Bollinger Bands
            if (expression == "bbupper" || expression == "bbupper(20)")
            {
                var (upper, middle, lower) = context.IndicatorCalculator.CalculateBollingerBands(context.HistoricalData, 20, 2);
                return upper;
            }
            else if (expression == "bbmiddle" || expression == "bbmiddle(20)")
            {
                var (upper, middle, lower) = context.IndicatorCalculator.CalculateBollingerBands(context.HistoricalData, 20, 2);
                return middle;
            }
            else if (expression == "bblower" || expression == "bblower(20)")
            {
                var (upper, middle, lower) = context.IndicatorCalculator.CalculateBollingerBands(context.HistoricalData, 20, 2);
                return lower;
            }
        }

        if (expression.StartsWith("avgvolume(") && expression.EndsWith(")"))
        {
            var periodStr = expression.Substring(10, expression.Length - 11);
            if (int.TryParse(periodStr, out var period))
            {
                return context.IndicatorCalculator.CalculateAverageVolume(context.HistoricalData, period);
            }
        }

        _logger.LogWarning("Unknown expression: {Expression}", expression);
        return 0;
    }
}

/// <summary>
/// Evaluation context containing price data and calculator
/// </summary>
public class EvaluationContext
{
    public List<HistoricalPriceData> HistoricalData { get; set; } = new();
    public HistoricalPriceData CurrentPrice { get; set; } = null!;
    public TechnicalIndicatorCalculator IndicatorCalculator { get; set; } = null!;
}
