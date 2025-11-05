namespace AlgoTrading.Core.ValueObjects.Trading;

/// <summary>
/// Quantity Value Object
/// Represents a quantity of shares/units with validation
/// </summary>
public sealed class Quantity : ValueObject, IComparable<Quantity>
{
    /// <summary>
    /// Quantity value
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Check if quantity is zero
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Check if quantity is positive
    /// </summary>
    public bool IsPositive => Value > 0;

    private Quantity(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Create a quantity
    /// </summary>
    public static Quantity Create(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(value));

        return new Quantity(value);
    }

    /// <summary>
    /// Create a quantity from decimal (rounds down)
    /// </summary>
    public static Quantity FromDecimal(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(value));

        return new Quantity((int)Math.Floor(value));
    }

    /// <summary>
    /// Zero quantity
    /// </summary>
    public static Quantity Zero => new(0);

    /// <summary>
    /// One quantity
    /// </summary>
    public static Quantity One => new(1);

    /// <summary>
    /// Addition operator
    /// </summary>
    public static Quantity operator +(Quantity left, Quantity right)
    {
        return new Quantity(left.Value + right.Value);
    }

    /// <summary>
    /// Subtraction operator
    /// </summary>
    public static Quantity operator -(Quantity left, Quantity right)
    {
        var result = left.Value - right.Value;

        if (result < 0)
            throw new InvalidOperationException("Quantity cannot be negative");

        return new Quantity(result);
    }

    /// <summary>
    /// Multiplication by scalar
    /// </summary>
    public static Quantity operator *(Quantity quantity, int multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Quantity(quantity.Value * multiplier);
    }

    /// <summary>
    /// Division by scalar (rounds down)
    /// </summary>
    public static Quantity operator /(Quantity quantity, int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide quantity by zero");

        if (divisor < 0)
            throw new ArgumentException("Divisor cannot be negative", nameof(divisor));

        return new Quantity(quantity.Value / divisor);
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static bool operator >(Quantity left, Quantity right)
    {
        return left.Value > right.Value;
    }

    /// <summary>
    /// Less than operator
    /// </summary>
    public static bool operator <(Quantity left, Quantity right)
    {
        return left.Value < right.Value;
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static bool operator >=(Quantity left, Quantity right)
    {
        return left.Value >= right.Value;
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static bool operator <=(Quantity left, Quantity right)
    {
        return left.Value <= right.Value;
    }

    /// <summary>
    /// Compare to another Quantity
    /// </summary>
    public int CompareTo(Quantity? other)
    {
        if (other is null)
            return 1;

        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Calculate total value at a given price
    /// </summary>
    public Money CalculateValue(Price price)
    {
        return new Money(Value * price.Value, price.Currency);
    }

    /// <summary>
    /// Calculate percentage of another quantity
    /// </summary>
    public decimal CalculatePercentageOf(Quantity total)
    {
        if (total.Value == 0)
            return 0;

        return ((decimal)Value / total.Value) * 100;
    }

    /// <summary>
    /// Split quantity into N parts
    /// </summary>
    public List<Quantity> Split(int parts)
    {
        if (parts <= 0)
            throw new ArgumentException("Parts must be positive", nameof(parts));

        if (parts > Value)
            throw new ArgumentException("Cannot split into more parts than the quantity", nameof(parts));

        var quantities = new List<Quantity>();
        var baseQuantity = Value / parts;
        var remainder = Value % parts;

        for (int i = 0; i < parts; i++)
        {
            var quantity = baseQuantity + (i < remainder ? 1 : 0);
            quantities.Add(new Quantity(quantity));
        }

        return quantities;
    }

    /// <summary>
    /// Get the minimum of two quantities
    /// </summary>
    public static Quantity Min(Quantity quantity1, Quantity quantity2)
    {
        return quantity1.Value <= quantity2.Value ? quantity1 : quantity2;
    }

    /// <summary>
    /// Get the maximum of two quantities
    /// </summary>
    public static Quantity Max(Quantity quantity1, Quantity quantity2)
    {
        return quantity1.Value >= quantity2.Value ? quantity1 : quantity2;
    }

    /// <summary>
    /// Check if quantity can be filled by available quantity
    /// </summary>
    public bool CanBeFilled(Quantity available)
    {
        return Value <= available.Value;
    }

    /// <summary>
    /// Get remaining quantity after partial fill
    /// </summary>
    public Quantity GetRemaining(Quantity filled)
    {
        if (filled.Value > Value)
            throw new ArgumentException("Filled quantity cannot exceed original quantity", nameof(filled));

        return new Quantity(Value - filled.Value);
    }

    /// <summary>
    /// Check if this is a partial quantity of the total
    /// </summary>
    public bool IsPartialOf(Quantity total)
    {
        return Value > 0 && Value < total.Value;
    }

    /// <summary>
    /// Round to lot size
    /// </summary>
    public Quantity RoundToLot(int lotSize)
    {
        if (lotSize <= 0)
            throw new ArgumentException("Lot size must be positive", nameof(lotSize));

        var rounded = (Value / lotSize) * lotSize;
        return new Quantity(rounded);
    }

    /// <summary>
    /// Round up to lot size
    /// </summary>
    public Quantity RoundUpToLot(int lotSize)
    {
        if (lotSize <= 0)
            throw new ArgumentException("Lot size must be positive", nameof(lotSize));

        var rounded = ((Value + lotSize - 1) / lotSize) * lotSize;
        return new Quantity(rounded);
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{Value:N0} shares";
    }

    /// <summary>
    /// Implicit conversion to int
    /// </summary>
    public static implicit operator int(Quantity quantity)
    {
        return quantity.Value;
    }

    /// <summary>
    /// Implicit conversion from int
    /// </summary>
    public static implicit operator Quantity(int value)
    {
        return Create(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
