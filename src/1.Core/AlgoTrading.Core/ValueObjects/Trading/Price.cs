namespace AlgoTrading.Core.ValueObjects.Trading;

/// <summary>
/// Price Value Object
/// Represents a stock price with validation
/// </summary>
public sealed class Price : ValueObject, IComparable<Price>
{
    /// <summary>
    /// Price value
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Currency code (default: KRW)
    /// </summary>
    public string Currency { get; }

    private Price(decimal value, string currency = "KRW")
    {
        Value = value;
        Currency = currency;
    }

    /// <summary>
    /// Create a price
    /// </summary>
    public static Price Create(decimal value, string currency = "KRW")
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative", nameof(value));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        return new Price(value, currency);
    }

    /// <summary>
    /// Create a price from integer (won)
    /// </summary>
    public static Price FromWon(int won)
    {
        if (won < 0)
            throw new ArgumentException("Price cannot be negative", nameof(won));

        return new Price(won, "KRW");
    }

    /// <summary>
    /// Zero price
    /// </summary>
    public static Price Zero(string currency = "KRW") => new(0, currency);

    /// <summary>
    /// Check if price is zero
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Check if price is positive
    /// </summary>
    public bool IsPositive => Value > 0;

    /// <summary>
    /// Addition operator
    /// </summary>
    public static Price operator +(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        return new Price(left.Value + right.Value, left.Currency);
    }

    /// <summary>
    /// Subtraction operator
    /// </summary>
    public static Price operator -(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        var result = left.Value - right.Value;

        if (result < 0)
            throw new InvalidOperationException("Price cannot be negative");

        return new Price(result, left.Currency);
    }

    /// <summary>
    /// Multiplication by scalar
    /// </summary>
    public static Price operator *(Price price, decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Price(price.Value * multiplier, price.Currency);
    }

    /// <summary>
    /// Division by scalar
    /// </summary>
    public static Price operator /(Price price, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide price by zero");

        if (divisor < 0)
            throw new ArgumentException("Divisor cannot be negative", nameof(divisor));

        return new Price(price.Value / divisor, price.Currency);
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static bool operator >(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        return left.Value > right.Value;
    }

    /// <summary>
    /// Less than operator
    /// </summary>
    public static bool operator <(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        return left.Value < right.Value;
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static bool operator >=(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        return left.Value >= right.Value;
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static bool operator <=(Price left, Price right)
    {
        EnsureSameCurrency(left, right);
        return left.Value <= right.Value;
    }

    /// <summary>
    /// Compare to another Price
    /// </summary>
    public int CompareTo(Price? other)
    {
        if (other is null)
            return 1;

        EnsureSameCurrency(this, other);
        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Calculate percentage change from another price
    /// </summary>
    public decimal CalculatePercentageChange(Price fromPrice)
    {
        EnsureSameCurrency(this, fromPrice);

        if (fromPrice.Value == 0)
            return 0;

        return ((Value - fromPrice.Value) / fromPrice.Value) * 100;
    }

    /// <summary>
    /// Apply percentage change
    /// </summary>
    public Price ApplyPercentageChange(decimal percentageChange)
    {
        var newValue = Value * (1 + percentageChange / 100);

        if (newValue < 0)
            throw new InvalidOperationException("Resulting price cannot be negative");

        return new Price(newValue, Currency);
    }

    /// <summary>
    /// Round to tick size
    /// </summary>
    public Price RoundToTick(decimal tickSize)
    {
        if (tickSize <= 0)
            throw new ArgumentException("Tick size must be positive", nameof(tickSize));

        var rounded = Math.Round(Value / tickSize) * tickSize;
        return new Price(rounded, Currency);
    }

    /// <summary>
    /// Get the absolute difference between two prices
    /// </summary>
    public static Price Difference(Price price1, Price price2)
    {
        EnsureSameCurrency(price1, price2);
        var diff = Math.Abs(price1.Value - price2.Value);
        return new Price(diff, price1.Currency);
    }

    /// <summary>
    /// Get the minimum of two prices
    /// </summary>
    public static Price Min(Price price1, Price price2)
    {
        EnsureSameCurrency(price1, price2);
        return price1.Value <= price2.Value ? price1 : price2;
    }

    /// <summary>
    /// Get the maximum of two prices
    /// </summary>
    public static Price Max(Price price1, Price price2)
    {
        EnsureSameCurrency(price1, price2);
        return price1.Value >= price2.Value ? price1 : price2;
    }

    /// <summary>
    /// Convert to Money
    /// </summary>
    public Money ToMoney()
    {
        return new Money(Value, Currency);
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return Currency == "KRW"
            ? $"₩{Value:N0}"
            : $"{Value:N2} {Currency}";
    }

    /// <summary>
    /// String representation with custom decimal places
    /// </summary>
    public string ToString(int decimalPlaces)
    {
        var format = $"N{decimalPlaces}";
        return Currency == "KRW"
            ? $"₩{Value.ToString(format)}"
            : $"{Value.ToString(format)} {Currency}";
    }

    /// <summary>
    /// Implicit conversion to decimal
    /// </summary>
    public static implicit operator decimal(Price price)
    {
        return price.Value;
    }

    private static void EnsureSameCurrency(Price left, Price right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot operate on prices with different currencies: {left.Currency} and {right.Currency}");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }
}
