namespace AlgoTrading.Core.ValueObjects;

/// <summary>
/// description(설명) : Base class for strongly-typed identifiers (강타입 식별자 기본 클래스)
/// Details(상세설명) : Prevents primitive obsession by wrapping primitive types in domain-specific ID classes (원시 타입을 도메인별 ID 클래스로 래핑하여 원시 타입 집착 방지)
/// Applied technology patterns(적용기술패턴) : Strongly Typed ID Pattern, Value Object Pattern, Domain-Driven Design (강타입 ID 패턴, 값 객체 패턴, 도메인 주도 설계)
/// </summary>
/// <typeparam name="T">The underlying type (e.g., Guid, int, long) (기본 타입 예: Guid, int, long)</typeparam>
public abstract class StronglyTypedId<T> : ValueObject where T : notnull
{
    /// <summary>
    /// ID 값
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// description(설명) : Constructor for strongly-typed ID (강타입 ID 생성자)
    /// Details(상세설명) : Initializes ID with validation to prevent null values (null 값을 방지하기 위한 유효성 검증과 함께 ID를 초기화)
    /// Applied technology patterns(적용기술패턴) : Strongly Typed ID Pattern (강타입 ID 패턴)
    /// </summary>
    protected StronglyTypedId(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "ID value cannot be null");

        Value = value;
    }

    /// <summary>
    /// description(설명) : Get equality components for value comparison (값 비교를 위한 동등성 구성 요소 조회)
    /// Details(상세설명) : Returns the ID value for equality comparison in value object pattern (값 객체 패턴의 동등성 비교를 위해 ID 값을 반환)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Enumerable containing the ID value (ID 값을 포함하는 열거형)</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// description(설명) : String representation of ID (ID의 문자열 표현)
    /// Details(상세설명) : Converts ID value to string format for display (표시를 위해 ID 값을 문자열 형식으로 변환)
    /// Applied technology patterns(적용기술패턴) : Strongly Typed ID Pattern (강타입 ID 패턴)
    /// </summary>
    /// <returns>String representation of ID value (ID 값의 문자열 표현)</returns>
    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// description(설명) : Implicit conversion to underlying type (기본 타입으로의 암시적 변환)
    /// Details(상세설명) : Allows seamless conversion from strongly-typed ID to its underlying value (강타입 ID를 기본 값으로 원활하게 변환)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading (연산자 오버로딩)
    /// </summary>
    /// <returns>Underlying ID value (기본 ID 값)</returns>
    public static implicit operator T(StronglyTypedId<T> id)
    {
        return id.Value;
    }
}

/// <summary>
/// description(설명) : Base class for Guid-based identifiers (Guid 기반 식별자 기본 클래스)
/// Details(상세설명) : Most common ID type in the system, provides factory methods for creating and parsing GUIDs (시스템에서 가장 일반적인 ID 타입, GUID 생성 및 파싱을 위한 팩토리 메서드 제공)
/// Applied technology patterns(적용기술패턴) : Strongly Typed ID Pattern, Factory Pattern (강타입 ID 패턴, 팩토리 패턴)
/// </summary>
public abstract class GuidId : StronglyTypedId<Guid>
{
    /// <summary>
    /// description(설명) : Constructor for Guid-based ID (Guid 기반 ID 생성자)
    /// Details(상세설명) : Initializes GUID ID with validation to prevent empty GUIDs (빈 GUID를 방지하기 위한 유효성 검증과 함께 GUID ID를 초기화)
    /// Applied technology patterns(적용기술패턴) : Strongly Typed ID Pattern (강타입 ID 패턴)
    /// </summary>
    protected GuidId(Guid value) : base(value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(value));
    }

    /// <summary>
    /// description(설명) : Create a new unique identifier (새로운 고유 식별자 생성)
    /// Details(상세설명) : Factory method to generate a new GUID-based ID using Activator (Activator를 사용하여 새로운 GUID 기반 ID를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New GUID-based ID instance (새로운 GUID 기반 ID 인스턴스)</returns>
    public static T New<T>() where T : GuidId
    {
        return (T)Activator.CreateInstance(typeof(T), Guid.NewGuid())!;
    }

    /// <summary>
    /// description(설명) : Parse a string to create an ID (문자열을 파싱하여 ID 생성)
    /// Details(상세설명) : Converts string representation of GUID to strongly-typed ID instance (GUID의 문자열 표현을 강타입 ID 인스턴스로 변환)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>Parsed GUID-based ID instance (파싱된 GUID 기반 ID 인스턴스)</returns>
    public static T Parse<T>(string value) where T : GuidId
    {
        if (!Guid.TryParse(value, out var guid))
            throw new FormatException($"Invalid GUID format: {value}");

        return (T)Activator.CreateInstance(typeof(T), guid)!;
    }
}
