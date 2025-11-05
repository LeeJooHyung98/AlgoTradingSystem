namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 세금 납부 의무에 따른 거주자 유형 분류
/// Details : 증권 거래 소득에 대한 세금 납부 의무를 구분하기 위해 Resident(거주자)와 NonResident(비거주자)로 분류합니다. Account Context에서 계좌 개설 시 세금 거주자 유형을 설정하며, 거주자는 한국 세법에 따라 양도소득세와 배당소득세가 적용되고, 비거주자는 원천징수세율이 다르게 적용됩니다. 거래 체결 시 세금 계산 로직에서 이 값을 참조하여 정확한 세후 수익을 산출합니다.
/// Applied technology patterns : Domain-Driven Design (Ubiquitous Language), Policy Pattern
/// </summary>
public enum TaxResidentType
{
    /// <summary>
    /// 거주자
    /// </summary>
    Resident = 1,

    /// <summary>
    /// 비거주자
    /// </summary>
    NonResident = 2
}
