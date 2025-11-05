namespace AlgoTrading.Core.ValueObjects;

/// <summary>
/// description(설명) : Money Value Object representing monetary amount (화폐 금액을 나타내는 Money 값 객체)
/// Details(상세설명) : Immutable value object for money calculations with currency support (통화 지원을 포함한 불변 화폐 계산 값 객체)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Immutability Pattern (값 객체 패턴, 불변성 패턴)
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    /// <summary>
    /// 금액
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// 통화 코드
    /// </summary>
    public string Currency { get; }

    public Money(decimal amount, string currency = "KRW")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero => new(0);
    public static Money ZeroOf(string currency) => new(0, currency);

    /// <summary>
    /// 금액이 0인지 확인
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// 금액이 양수인지 확인
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// 금액이 음수인지 확인
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// description(설명) : Addition operator for Money (Money 덧셈 연산자)
    /// Details(상세설명) : Adds two Money values with same currency (동일 통화의 두 Money 값을 더함)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>Sum of two Money values (두 Money 값의 합)</returns>
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot add money in different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// description(설명) : Subtraction operator for Money (Money 뺄셈 연산자)
    /// Details(상세설명) : Subtracts two Money values with same currency (동일 통화의 두 Money 값을 뺌)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>Difference of two Money values (두 Money 값의 차)</returns>
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot subtract money in different currencies: {left.Currency} and {right.Currency}");

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// description(설명) : Multiplication operator for Money (Money 곱셈 연산자)
    /// Details(상세설명) : Multiplies Money value by scalar (Money 값에 스칼라를 곱함)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>Money multiplied by scalar (스칼라를 곱한 Money 값)</returns>
    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    /// <summary>
    /// description(설명) : Division operator for Money (Money 나눗셈 연산자)
    /// Details(상세설명) : Divides Money value by scalar (Money 값을 스칼라로 나눔)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>Money divided by scalar (스칼라로 나눈 Money 값)</returns>
    public static Money operator /(Money money, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero");

        return new Money(money.Amount / divisor, money.Currency);
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money in different currencies: {left.Currency} and {right.Currency}");

        return left.Amount > right.Amount;
    }

    /// <summary>
    /// Less than operator
    /// </summary>
    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money in different currencies: {left.Currency} and {right.Currency}");

        return left.Amount < right.Amount;
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static bool operator >=(Money left, Money right)
    {
        return left > right || left == right;
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static bool operator <=(Money left, Money right)
    {
        return left < right || left == right;
    }

    /// <summary>
    /// Compare to another Money instance
    /// </summary>
    public int CompareTo(Money? other)
    {
        if (other is null)
            return 1;

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money in different currencies: {Currency} and {other.Currency}");

        return Amount.CompareTo(other.Amount);
    }

    /// <summary>
    /// Get the minimum of two Money values
    /// </summary>
    public static Money Min(Money a, Money b)
    {
        return a <= b ? a : b;
    }

    /// <summary>
    /// Get the minimum of multiple Money values
    /// </summary>
    public static Money Min(params Money[] values)
    {
        if (values.Length == 0)
            throw new ArgumentException("At least one value is required", nameof(values));

        var min = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min)
                min = values[i];
        }
        return min;
    }

    /// <summary>
    /// Get the maximum of two Money values
    /// </summary>
    public static Money Max(Money a, Money b)
    {
        return a >= b ? a : b;
    }

    /// <summary>
    /// description(설명) : Get absolute value of Money (Money의 절대값 조회)
    /// Details(상세설명) : Returns Money with absolute amount value (금액의 절대값을 가진 Money를 반환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Money with absolute amount (절대값 금액의 Money)</returns>
    public Money Abs()
    {
        return new Money(Math.Abs(Amount), Currency);
    }

    /// <summary>
    /// description(설명) : Round Money to specified decimal places (지정된 소수점 자리로 Money 반올림)
    /// Details(상세설명) : Returns rounded Money value preserving currency (통화를 보존하며 반올림된 Money 값을 반환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Rounded Money value (반올림된 Money 값)</returns>
    public Money Round(int decimals = 2)
    {
        return new Money(Math.Round(Amount, decimals), Currency);
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }

    /// <summary>
    /// Formatted string with custom format
    /// </summary>
    public string ToString(string format)
    {
        return $"{Amount.ToString(format)} {Currency}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
