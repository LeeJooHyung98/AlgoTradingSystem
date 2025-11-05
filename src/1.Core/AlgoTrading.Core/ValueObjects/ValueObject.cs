namespace AlgoTrading.Core.ValueObjects;

/// <summary>
/// description(설명) : Base class for Value Objects (값 객체 기본 클래스)
/// Details(상세설명) : Abstract base class for immutable value objects that are identified by their value rather than identity (ID가 아닌 값으로 식별되는 불변 값 객체를 위한 추상 기본 클래스)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Immutability Pattern, Domain-Driven Design (값 객체 패턴, 불변성 패턴, 도메인 주도 설계)
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// description(설명) : Get the atomic values that define this value object (값 객체를 정의하는 원자적 값들을 조회)
    /// Details(상세설명) : Returns enumerable of component values used for equality comparison (동등성 비교에 사용되는 구성 요소 값들의 열거형을 반환)
    /// Applied technology patterns(적용기술패턴) : Template Method Pattern (템플릿 메서드 패턴)
    /// </summary>
    /// <returns>Enumerable of component values (구성 요소 값들의 열거형)</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// description(설명) : Value equality comparison (값 동등성 비교)
    /// Details(상세설명) : Compares two value objects by their component values rather than reference (참조가 아닌 구성 요소 값들로 두 값 객체를 비교)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>True if objects are equal by value (값으로 객체가 동등하면 true)</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// description(설명) : Generate hash code based on component values (구성 요소 값들을 기반으로 해시 코드 생성)
    /// Details(상세설명) : Creates consistent hash code from all equality components for use in collections (컬렉션 사용을 위해 모든 동등성 구성 요소로부터 일관된 해시 코드 생성)
    /// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴)
    /// </summary>
    /// <returns>Hash code of the value object (값 객체의 해시 코드)</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// description(설명) : Equality operator overload (동등성 연산자 오버로드)
    /// Details(상세설명) : Implements == operator for value-based equality comparison (값 기반 동등성 비교를 위한 == 연산자 구현)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>True if both value objects are equal (두 값 객체가 동등하면 true)</returns>
    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// description(설명) : Inequality operator overload (불일치 연산자 오버로드)
    /// Details(상세설명) : Implements != operator for value-based inequality comparison (값 기반 불일치 비교를 위한 != 연산자 구현)
    /// Applied technology patterns(적용기술패턴) : Operator Overloading, Value Object Pattern (연산자 오버로딩, 값 객체 패턴)
    /// </summary>
    /// <returns>True if value objects are not equal (값 객체가 동등하지 않으면 true)</returns>
    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// description(설명) : Create a shallow copy of this value object (값 객체의 얕은 복사본 생성)
    /// Details(상세설명) : Creates a memberwise clone for derived value object types (파생 값 객체 타입을 위한 멤버별 복제 생성)
    /// Applied technology patterns(적용기술패턴) : Prototype Pattern (프로토타입 패턴)
    /// </summary>
    /// <returns>Cloned value object (복제된 값 객체)</returns>
    protected T Copy<T>() where T : ValueObject
    {
        return (T)MemberwiseClone();
    }
}
