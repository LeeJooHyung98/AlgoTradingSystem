namespace AlgoTrading.Core.ValueObjects.Common;

/// <summary>
/// description(설명) : Percentage Value Object representing percentage values (백분율 값을 나타내는 백분율 값 객체)
/// Details(상세설명) : Immutable value object for percentage calculations with validation (검증을 포함한 백분율 계산을 위한 불변 값 객체)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Immutability Pattern, Operator Overloading (값 객체 패턴, 불변성 패턴, 연산자 오버로딩)
/// </summary>
public sealed class Percentage : ValueObject, IComparable<Percentage>
{
    /// <summary>
    /// 백분율 값 (0-100)
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// 소수 표현 (0-1)
    /// </summary>
    public decimal DecimalValue => Value / 100;

    private Percentage(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// description(설명) : Create percentage from percentage value (백분율 값으로부터 백분율 생성)
    /// Details(상세설명) : Factory method to create percentage from 0-100 range value (0-100 범위 값으로부터 백분율을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New Percentage instance (새로운 백분율 인스턴스)</returns>
    public static Percentage FromPercent(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percent));

        return new Percentage(percent);
    }

    /// <summary>
    /// description(설명) : Create percentage from decimal value (소수 값으로부터 백분율 생성)
    /// Details(상세설명) : Factory method to create percentage from 0-1 decimal range (0-1 소수 범위로부터 백분율을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New Percentage instance (새로운 백분율 인스턴스)</returns>
    public static Percentage FromDecimal(decimal decimalValue)
    {
        if (decimalValue < 0 || decimalValue > 1)
            throw new ArgumentException("Decimal value must be between 0 and 1", nameof(decimalValue));

        return new Percentage(decimalValue * 100);
    }

    /// <summary>
    /// description(설명) : Create percentage from fraction (분수로부터 백분율 생성)
    /// Details(상세설명) : Factory method to calculate percentage from numerator and denominator (분자와 분모로부터 백분율을 계산하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New Percentage instance (새로운 백분율 인스턴스)</returns>
    public static Percentage FromFraction(decimal numerator, decimal denominator)
    {
        if (denominator == 0)
            throw new ArgumentException("Denominator cannot be zero", nameof(denominator));

        var percent = (numerator / denominator) * 100;
        return new Percentage(percent);
    }

    /// <summary>
    /// 0%
    /// </summary>
    public static Percentage Zero => new(0);

    /// <summary>
    /// 100%
    /// </summary>
    public static Percentage OneHundred => new(100);

    /// <summary>
    /// 50%
    /// </summary>
    public static Percentage Fifty => new(50);

    /// <summary>
    /// 백분율이 0인지 확인
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// 백분율이 100인지 확인
    /// </summary>
    public bool IsOneHundred => Value == 100;

    /// <summary>
    /// description(설명) : Addition operator for Percentage (백분율 덧셈 연산자)
    /// Details(상세설명) : Adds two percentages with capping at 100% (100%로 제한하며 두 백분율 더하기)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Sum of percentages capped at 100% (100%로 제한된 백분율의 합)</returns>
    public static Percentage operator +(Percentage left, Percentage right)
    {
        var result = left.Value + right.Value;
        return new Percentage(Math.Min(result, 100));
    }

    /// <summary>
    /// description(설명) : Subtraction operator for Percentage (백분율 뺄셈 연산자)
    /// Details(상세설명) : Subtracts percentages with flooring at 0% (0%로 제한하며 백분율 빼기)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Difference of percentages floored at 0% (0%로 제한된 백분율의 차)</returns>
    public static Percentage operator -(Percentage left, Percentage right)
    {
        var result = left.Value - right.Value;
        return new Percentage(Math.Max(result, 0));
    }

    /// <summary>
    /// description(설명) : Multiplication operator for Percentage (백분율 곱셈 연산자)
    /// Details(상세설명) : Multiplies percentage by scalar with clamping to 0-100 range (0-100 범위로 제한하며 백분율에 스칼라 곱하기)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Multiplied percentage clamped to 0-100 (0-100으로 제한된 곱셈 백분율)</returns>
    public static Percentage operator *(Percentage percentage, decimal multiplier)
    {
        var result = percentage.Value * multiplier;
        return new Percentage(Math.Clamp(result, 0, 100));
    }

    /// <summary>
    /// description(설명) : Division operator for Percentage (백분율 나눗셈 연산자)
    /// Details(상세설명) : Divides percentage by scalar with clamping to 0-100 range (0-100 범위로 제한하며 백분율을 스칼라로 나누기)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Divided percentage clamped to 0-100 (0-100으로 제한된 나눗셈 백분율)</returns>
    public static Percentage operator /(Percentage percentage, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide percentage by zero");

        var result = percentage.Value / divisor;
        return new Percentage(Math.Clamp(result, 0, 100));
    }

    /// <summary>
    /// Greater than operator (크다 연산자)
    /// </summary>
    public static bool operator >(Percentage left, Percentage right)
    {
        return left.Value > right.Value;
    }

    /// <summary>
    /// Less than operator (작다 연산자)
    /// </summary>
    public static bool operator <(Percentage left, Percentage right)
    {
        return left.Value < right.Value;
    }

    /// <summary>
    /// Greater than or equal operator (크거나 같다 연산자)
    /// </summary>
    public static bool operator >=(Percentage left, Percentage right)
    {
        return left.Value >= right.Value;
    }

    /// <summary>
    /// Less than or equal operator (작거나 같다 연산자)
    /// </summary>
    public static bool operator <=(Percentage left, Percentage right)
    {
        return left.Value <= right.Value;
    }

    /// <summary>
    /// description(설명) : Compare to another Percentage (다른 백분율과 비교)
    /// Details(상세설명) : Implements IComparable for sorting and comparison operations (정렬 및 비교 연산을 위한 IComparable 구현)
    /// Applied technology patterns(적용기술패턴) : Comparable Pattern (비교 가능 패턴)
    /// </summary>
    /// <returns>Comparison result (-1, 0, 1) (비교 결과 -1, 0, 1)</returns>
    public int CompareTo(Percentage? other)
    {
        if (other is null)
            return 1;

        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// description(설명) : Apply percentage to a value (값에 백분율 적용)
    /// Details(상세설명) : Calculates the percentage of a given value (주어진 값의 백분율을 계산)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Calculated percentage of value (계산된 값의 백분율)</returns>
    public decimal ApplyTo(decimal value)
    {
        return value * DecimalValue;
    }

    /// <summary>
    /// description(설명) : Get the inverse percentage (역 백분율 조회)
    /// Details(상세설명) : Returns complement percentage (100 - this) (보수 백분율 반환 100 - this)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Inverse percentage (역 백분율)</returns>
    public Percentage Inverse()
    {
        return new Percentage(100 - Value);
    }

    /// <summary>
    /// description(설명) : String representation (문자열 표현)
    /// Details(상세설명) : Converts percentage to formatted string with % symbol (% 기호를 포함한 형식화된 문자열로 백분율 변환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Formatted percentage string (형식화된 백분율 문자열)</returns>
    public override string ToString()
    {
        return $"{Value:F2}%";
    }

    /// <summary>
    /// description(설명) : String representation with custom decimal places (사용자 정의 소수점 자리를 가진 문자열 표현)
    /// Details(상세설명) : Formats percentage with specified decimal precision (지정된 소수점 정밀도로 백분율 형식화)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Formatted percentage string (형식화된 백분율 문자열)</returns>
    public string ToString(int decimalPlaces)
    {
        return $"{Math.Round(Value, decimalPlaces)}%";
    }

    /// <summary>
    /// description(설명) : Implicit conversion to decimal (소수로의 암시적 변환)
    /// Details(상세설명) : Converts percentage to decimal value (0-1 range) (백분율을 소수 값으로 변환 0-1 범위)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Decimal value representation (소수 값 표현)</returns>
    public static implicit operator decimal(Percentage percentage)
    {
        return percentage.DecimalValue;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
