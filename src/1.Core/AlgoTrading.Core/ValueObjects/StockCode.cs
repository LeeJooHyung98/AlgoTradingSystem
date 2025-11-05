using System.Text.RegularExpressions;

namespace AlgoTrading.Core.ValueObjects;

/// <summary>
/// description(설명) : Stock Code Value Object representing stock ticker symbols (주식 티커 심볼을 나타내는 종목 코드 값 객체)
/// Details(상세설명) : Immutable value object for Korean stock codes with 6-digit validation (6자리 검증을 포함한 한국 주식 코드 불변 값 객체)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Immutability Pattern, Domain-Driven Design (값 객체 패턴, 불변성 패턴, 도메인 주도 설계)
/// </summary>
public sealed class StockCode : ValueObject
{
    private static readonly Regex KoreanStockCodeRegex = new(@"^\d{6}$", RegexOptions.Compiled);

    /// <summary>
    /// 종목 코드 값
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// description(설명) : Constructor for StockCode (종목 코드 생성자)
    /// Details(상세설명) : Creates stock code with validation for 6-digit Korean stock format (6자리 한국 주식 형식에 대한 검증과 함께 종목 코드 생성)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    public StockCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Stock code cannot be empty", nameof(value));

        // Validate Korean stock code format (6 digits)
        if (!KoreanStockCodeRegex.IsMatch(value))
            throw new ArgumentException($"Invalid Korean stock code format: {value}. Expected 6 digits.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// description(설명) : Create a StockCode from string (문자열로부터 종목 코드 생성)
    /// Details(상세설명) : Factory method to create stock code with validation (검증을 포함한 종목 코드 생성 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New StockCode instance (새로운 종목 코드 인스턴스)</returns>
    public static StockCode From(string value)
    {
        return new StockCode(value);
    }

    /// <summary>
    /// description(설명) : Try to create a StockCode from string (문자열로부터 종목 코드 생성 시도)
    /// Details(상세설명) : Safe factory method that returns false if validation fails (검증 실패 시 false를 반환하는 안전한 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Try-Parse Pattern (팩토리 패턴, Try-Parse 패턴)
    /// </summary>
    /// <returns>True if creation succeeded (생성 성공 시 true)</returns>
    public static bool TryCreate(string value, out StockCode? stockCode)
    {
        try
        {
            stockCode = new StockCode(value);
            return true;
        }
        catch
        {
            stockCode = null;
            return false;
        }
    }

    /// <summary>
    /// description(설명) : Check if a string is a valid stock code (문자열이 유효한 종목 코드인지 확인)
    /// Details(상세설명) : Validates string against Korean stock code format rules (한국 주식 코드 형식 규칙에 대해 문자열 검증)
    /// Applied technology patterns(적용기술패턴) : Validation Pattern (검증 패턴)
    /// </summary>
    /// <returns>True if valid stock code format (유효한 종목 코드 형식이면 true)</returns>
    public static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && KoreanStockCodeRegex.IsMatch(value);
    }

    /// <summary>
    /// description(설명) : String representation of stock code (종목 코드의 문자열 표현)
    /// Details(상세설명) : Returns stock code value as string (종목 코드 값을 문자열로 반환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Stock code value (종목 코드 값)</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// description(설명) : Implicit conversion to string (문자열로의 암시적 변환)
    /// Details(상세설명) : Allows seamless conversion from StockCode to string (StockCode에서 문자열로 원활한 변환 허용)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Stock code string value (종목 코드 문자열 값)</returns>
    public static implicit operator string(StockCode stockCode)
    {
        return stockCode.Value;
    }

    /// <summary>
    /// description(설명) : Get equality components for value comparison (값 비교를 위한 동등성 구성 요소 조회)
    /// Details(상세설명) : Returns stock code value for equality comparison (동등성 비교를 위해 종목 코드 값 반환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Enumerable containing stock code value (종목 코드 값을 포함하는 열거형)</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
