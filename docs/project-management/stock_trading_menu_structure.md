# 주식 자동매매 시스템 - 전체 메뉴 구조

## 목차
- [시스템 개요](#시스템-개요)
- [1. 시장 데이터 관리](#1-시장-데이터-관리)
- [2. 전략 개발 및 관리](#2-전략-개발-및-관리)
- [3. 주문 실행 관리](#3-주문-실행-관리)
- [4. 포트폴리오 관리](#4-포트폴리오-관리)
- [5. 백테스팅 및 최적화](#5-백테스팅-및-최적화)
- [6. 모니터링 및 알림](#6-모니터링-및-알림)
- [7. 시스템 관리](#7-시스템-관리)
- [8. 뉴스 및 정보](#8-뉴스-및-정보)

---

## 시스템 개요

본 시스템은 DDD(Domain-Driven Design) 설계 원칙을 기반으로 하는 엔터프라이즈급 주식 자동매매 플랫폼입니다. 각 기능 모듈은 명확한 경계(Bounded Context)로 분리되어 있으며, 4단계(Phase 1-4)에 걸쳐 순차적으로 개발됩니다.

**핵심 기술 스택:**
- Backend: .NET 8, C#
- Architecture: Clean Architecture, CQRS, Event Sourcing
- Database: PostgreSQL 17, Redis, TimescaleDB
- Real-time: SignalR
- Message Bus: RabbitMQ
- API: 키움 OpenAPI

---

## 1. 시장 데이터 관리
**Market Data Management Context**

실시간 시장 데이터 수신, 과거 데이터 수집 및 관리를 담당하는 핵심 모듈

### 1.1 실시간 시세 수신 `[Phase 1]`
**기능 ID:** MD-001

**상세 기능:**
- 현재가/호가/체결 실시간 수신
- 관심종목 실시간 감시
- 시장지수 모니터링 (KOSPI, KOSDAQ)
- 실시간 데이터 캐싱 (Redis)

**기술 스택:** 키움 OpenAPI, SignalR, Redis Cache

**API 통합:**
- `OnReceiveRealData` 이벤트 핸들링
- 실시간 체결가 (`주식체결`)
- 실시간 호가 (`주식호가`)

---

### 1.2 과거 데이터 수집 `[Phase 1]`
**기능 ID:** MD-002

**상세 기능:**
- 일/주/월봉 데이터 저장
- 분봉 데이터 수집 (1/5/15/30/60분)
- 재무제표 데이터 수집
- 기업 정보 및 배당 정보

**기술 스택:** 키움 TR 조회, InfluxDB, EF Core

**데이터 소스:**
- `OPT10081` - 주식일봉차트조회
- `OPT10080` - 주식분봉차트조회
- `OPT10086` - 일별주가

---

### 1.3 종목 검색/필터링 `[Phase 2]`
**기능 ID:** MD-003

**상세 기능:**
- 조건식 검색 (기술적 조건)
- 기술적 지표 필터 (RSI, MACD, 볼린저밴드)
- 재무 지표 필터 (PER, PBR, ROE)
- 섹터/테마 필터링

**기술 스택:** LINQ, Expression Trees

---

### 1.4 데이터 저장/관리 `[Phase 1]`
**기능 ID:** MD-004

**상세 기능:**
- Time-Series DB 저장 (InfluxDB)
- 데이터 정합성 검증
- 데이터 백업
- 데이터 압축 및 아카이빙

**기술 스택:** InfluxDB, SQL Server, Azure Blob Storage

---

## 2. 전략 개발 및 관리
**Strategy Development & Management Context**

알고리즘 트레이딩 전략의 생성, 관리, 최적화를 담당

### 2.1 전략 생성
**기능 ID:** ST-001

#### 2.1.1 코드 기반 전략 작성 `[Phase 1]`
- C# 코드로 전략 로직 작성
- Strategy 인터페이스 구현
- 컴파일 및 동적 로딩

**예시 구조:**
```csharp
public interface ITradingStrategy
{
    string StrategyName { get; }
    SignalResult GenerateSignal(MarketData data);
    void OnTick(MarketData data);
}
```

#### 2.1.2 빌더 UI로 전략 설계 `[Phase 2]`
- 드래그 앤 드롭 전략 빌더
- 조건 블록 조합
- 노코드 전략 생성

#### 2.1.3 템플릿 기반 생성 `[Phase 2]`
- 사전 정의된 전략 템플릿
  - 이동평균 크로스오버
  - RSI 역전 전략
  - 볼린저밴드 돌파 전략
  - 페어 트레이딩
- 파라미터만 수정하여 사용

---

### 2.2 전략 설정 `[Phase 2]`
**기능 ID:** ST-002

**상세 기능:**
- 매매 조건 설정
  - 진입 조건
  - 청산 조건
  - 필터 조건
- 리스크 관리 설정
  - 손절선 (Stop Loss)
  - 익절선 (Take Profit)
  - 트레일링 스탑
- 포지션 관리 설정
  - 포지션 크기
  - 최대 보유 종목 수
  - 분할 매수/매도

---

### 2.3 전략 최적화 `[Phase 3]`
**기능 ID:** ST-003

#### 2.3.1 파라미터 그리드 서치
- 모든 파라미터 조합 테스트
- 최적 파라미터 조합 발견
- 과최적화 방지

#### 2.3.2 유전자 알고리즘
- 진화 알고리즘 기반 최적화
- 다중 목표 최적화
- ML.NET 통합

#### 2.3.3 Walk-Forward 분석
- 순차적 최적화 및 검증
- 과적합 방지
- 실전 투입 전 검증

**기술 스택:** ML.NET, GPU Acceleration

---

### 2.4 신호 생성 `[Phase 2]`
**기능 ID:** ST-004

**상세 기능:**
- 실시간 조건 모니터링
- 매수/매도 신호 생성
- 멀티 타임프레임 분석
- 신호 우선순위 관리

**기술 스택:** CEP (Complex Event Processing), Event Sourcing

---

## 3. 주문 실행 관리
**Order Execution Management Context**

주문의 생성, 전송, 체결, 수정, 취소를 관리

### 3.1 주문 생성/전송 `[Phase 1]`
**기능 ID:** OE-001

**상세 기능:**
- 시장가/지정가 주문
- 조건부 주문
- 분할 주문 실행 (TWAP, VWAP)
- 주문 전 검증
  - 잔고 확인
  - 리스크 한도 체크
  - 주문 가능 여부 확인

**기술 스택:** 키움 SendOrder API, Saga Pattern

**API 통합:**
- `SendOrder` - 주문 전송
- `SendOrderFO` - 선물옵션 주문

---

### 3.2 주문 체결 처리 `[Phase 1]`
**기능 ID:** OE-002

**상세 기능:**
- 체결 통보 수신
- 부분 체결 처리
- 포지션 자동 업데이트
- 체결 이벤트 발행

**기술 스택:** Event Sourcing, CQRS

**이벤트:**
- `OrderFilledEvent`
- `PartialFillEvent`
- `OrderCompletedEvent`

---

### 3.3 주문 수정/취소 `[Phase 1]`
**기능 ID:** OE-003

**상세 기능:**
- 가격/수량 정정
- 일괄 취소 (모든 미체결 주문)
- 타임아웃 자동 취소
- 취소 실패 재시도

**기술 스택:** State Machine, Saga Pattern

---

### 3.4 주문 이력 관리 `[Phase 2]`
**기능 ID:** OE-004

**상세 기능:**
- 주문 이력 저장
- 체결 내역 관리
- 감사 추적 (Audit Trail)
- 거래 리포트 생성

**기술 스택:** EF Core, Audit Log

---

## 4. 포트폴리오 관리
**Portfolio Management Context**

계좌, 포지션, 자산 배분, 손익 분석 관리

### 4.1 포지션 관리 `[Phase 1]`
**기능 ID:** PM-001

**상세 기능:**
- 실시간 포지션 추적
- 평균 단가 계산
- 미실현 손익 계산
- 실현 손익 집계
- 포지션 히스토리

**기술 스택:** In-Memory Cache, CQRS

**계산 로직:**
```
평균 단가 = (기존 수량 × 기존 평균 단가 + 추가 수량 × 체결가) / 총 수량
미실현 손익 = (현재가 - 평균 단가) × 보유 수량
```

---

### 4.2 계좌 관리 `[Phase 1]`
**기능 ID:** PM-002

**상세 기능:**
- 계좌 잔고 조회
- 예수금/증거금 관리
- 주문 가능 금액 계산
- 다중 계좌 지원
- 계좌 간 자금 이체

**기술 스택:** 키움 계좌 API, Redis

**API 통합:**
- `GetLoginInfo` - 계좌 정보 조회
- `opw00018` - 계좌평가잔고내역

---

### 4.3 자산 배분 `[Phase 4]`
**기능 ID:** PM-003

**상세 기능:**
- 목표 비중 설정
- 자동 리밸런싱
- 섹터/업종 분산
- 상관관계 기반 배분

**기술 스택:** Optimization Algorithms

**배분 전략:**
- 균등 배분 (Equal Weight)
- 시가총액 가중
- 리스크 패리티
- 켈리 기준 (Kelly Criterion)

---

### 4.4 손익 분석 `[Phase 2]`
**기능 ID:** PM-004

**상세 기능:**
- 일별/월별 손익 집계
- 종목별 수익률 분석
- 누적 손익 차트
- 손익 비율 (P&L Ratio)
- 승률 계산

**기술 스택:** Chart.js, SignalR

**지표:**
- 총 수익률
- 연환산 수익률
- 승률 (Win Rate)
- 평균 이익 / 평균 손실
- Profit Factor

---

### 4.5 리스크 관리 `[Phase 2]`
**기능 ID:** PM-005

**상세 기능:**
- 손절/익절 설정
- 최대 손실 한도 (Max Drawdown Limit)
- VaR (Value at Risk) 계산
- 포지션 사이징
- 킬 스위치 (긴급 종료)

**기술 스택:** Risk Calculation Engine

**리스크 지표:**
- VaR (95%, 99%)
- CVaR (Conditional VaR)
- Max Drawdown
- Sharpe Ratio
- Sortino Ratio

---

## 5. 백테스팅 및 최적화
**Backtesting & Optimization Context**

과거 데이터를 사용한 전략 검증 및 성과 분석

### 5.1 백테스팅 실행 `[Phase 2]`
**기능 ID:** BT-001

**상세 기능:**
- 과거 데이터 시뮬레이션
- 슬리피지/수수료 적용
- 멀티 전략 동시 백테스트
- 병렬 처리

**기술 스택:** Backtesting Engine, Parallel Processing

**시뮬레이션 옵션:**
- 초기 자금 설정
- 거래 비용 (수수료, 세금)
- 슬리피지 모델
- 체결 가정 (Market-On-Close, Limit)

---

### 5.2 성과 분석 `[Phase 2]`
**기능 ID:** BT-002

**상세 기능:**
- 수익률/MDD/샤프비율 계산
- 승률/손익비 분석
- 거래 횟수 및 보유 기간
- Equity Curve 생성

**기술 스택:** Statistical Analysis

**성과 지표:**
- **수익률:** 총 수익률, CAGR
- **리스크 조정 수익률:** Sharpe, Sortino, Calmar Ratio
- **손실 지표:** Max DD, Average DD, Recovery Factor
- **거래 지표:** 총 거래 횟수, 승률, Profit Factor
- **통계:** 표준편차, 왜도, 첨도

---

### 5.3 전략 비교 `[Phase 3]`
**기능 ID:** BT-003

**상세 기능:**
- 여러 전략 성과 비교
- 기간별 성과 분석
- 최적 전략 선택
- 벤치마크 대비 성과

**비교 기준:**
- 리스크 조정 수익률
- 최대 손실폭
- 안정성 지표
- 실전 적용 가능성

---

### 5.4 리포트 생성 `[Phase 3]`
**기능 ID:** BT-004

**상세 기능:**
- PDF/HTML 리포트 생성
- 차트 및 테이블 자동 생성
- 상세 거래 내역 포함
- 이메일 자동 발송

**기술 스택:** Report Generator, PDF Library

**리포트 구성:**
1. Executive Summary
2. Performance Metrics
3. Equity Curve
4. Drawdown Chart
5. Monthly Returns Table
6. Trade List
7. Risk Analysis

---

## 6. 모니터링 및 알림
**Monitoring & Notification Context**

시스템 상태 모니터링, 대시보드, 알림 관리

### 6.1 실시간 대시보드 `[Phase 2]`
**기능 ID:** MN-001

**상세 기능:**
- 포트폴리오 현황 표시
- 실시간 손익 차트
- 커스텀 위젯 (드래그 앤 드롭)
- 다중 레이아웃 지원

**기술 스택:** Blazor, SignalR, Chart.js

**위젯 종류:**
- 계좌 요약
- 포지션 리스트
- 최근 체결 내역
- 실시간 손익 그래프
- 시장 지수
- 뉴스 피드
- 알림 센터

---

### 6.2 알림 설정 `[Phase 4]`
**기능 ID:** MN-002

**상세 기능:**
- 가격 알림 (특정 가격 도달)
- 체결 알림
- 손익 알림 (목표 달성/손실 한도)
- 시스템 알림 (오류, 연결 끊김)

**알림 채널:**
- 이메일
- SMS
- 푸시 알림 (모바일)
- Telegram Bot
- Slack Webhook

**기술 스택:** RabbitMQ, Push Notification Service

---

### 6.3 로그 관리 `[Phase 2]`
**기능 ID:** MN-003

**상세 기능:**
- 거래 로그 기록
- 에러 로그 추적
- 감사 로그 (Audit Log)
- 로그 검색 및 필터링

**로그 레벨:**
- Trace
- Debug
- Information
- Warning
- Error
- Critical

**기술 스택:** Serilog, Elasticsearch (ELK Stack)

---

### 6.4 시스템 모니터링 `[Phase 1]`
**기능 ID:** MN-004

**상세 기능:**
- API 연결 상태 모니터링
- 성능 지표 (CPU, 메모리, 네트워크)
- 헬스 체크
- 장애 감지 및 알림

**모니터링 항목:**
- 키움 API 연결 상태
- 데이터베이스 연결
- Message Bus 상태
- 응답 시간 (Latency)
- 처리량 (Throughput)

**기술 스택:** Health Check API, Prometheus, Grafana

---

### 6.5 성과 리포트 `[Phase 3]`
**기능 ID:** MN-005

**상세 기능:**
- 일일 리포트 자동 생성
- 주간/월간 리포트
- 이메일 자동 발송
- 리포트 히스토리 보관

**리포트 내용:**
- 당일 거래 요약
- 손익 현황
- 주요 이벤트
- 리스크 지표
- 다음 날 전망

**기술 스택:** Scheduled Jobs (Hangfire), Email Service

---

## 7. 시스템 관리
**System Management Context**

사용자 관리, 시스템 설정, 백업/복구

### 7.1 사용자 인증/권한 `[Phase 1]`
**기능 ID:** SM-001

**상세 기능:**
- 로그인/로그아웃
- 2단계 인증 (2FA)
- 역할 기반 접근제어 (RBAC)
- 세션 관리

**역할 정의:**
- **Admin:** 전체 시스템 관리
- **Trader:** 거래 실행 및 전략 관리
- **Viewer:** 조회만 가능
- **Developer:** 전략 개발

**기술 스택:** ASP.NET Core Identity, JWT, OAuth 2.0

---

### 7.2 시스템 설정 `[Phase 2]`
**기능 ID:** SM-002

**상세 기능:**
- 거래 시간 설정
- API 키 관리
- 시스템 파라미터 관리
- 환경 설정 (개발/스테이징/프로덕션)

**설정 항목:**
- 거래 가능 시간
- 주문 제한 (일일 최대 주문 수)
- 리스크 한도
- 알림 설정
- 로그 레벨

**기술 스택:** Configuration API, Azure Key Vault

---

### 7.3 데이터 백업/복구 `[Phase 4]`
**기능 ID:** SM-003

**상세 기능:**
- 자동 백업 (Daily, Weekly)
- 증분 백업
- 복구 테스트
- 재해 복구 계획 (DR)

**백업 대상:**
- 거래 데이터
- 전략 코드
- 시스템 설정
- 사용자 데이터

**기술 스택:** Azure Backup, DR Plan

---

### 7.4 API 연결 관리 `[Phase 1]`
**기능 ID:** SM-004

**상세 기능:**
- 연결 상태 모니터링
- 자동 재연결
- API 호출 제한 (Rate Limiting)
- Circuit Breaker 패턴

**기술 스택:** Polly (Resilience Library), Circuit Breaker

---

## 8. 뉴스 및 정보
**News & Information Context** `[Phase 4]`

뉴스 수집, 감성 분석, 전략 연동

### 8.1 뉴스 수집 `[Phase 4]`
**기능 ID:** NI-001

**상세 기능:**
- 실시간 뉴스 크롤링
- 공시 정보 수집 (DART)
- 키워드 필터링
- 뉴스 분류 (긍정/부정/중립)

**뉴스 소스:**
- 네이버 증권
- 한국거래소 공시
- DART (전자공시시스템)
- 경제 뉴스 포털

**기술 스택:** Web Scraping, NLP

---

### 8.2 감성 분석 `[Phase 4]`
**기능 ID:** NI-002

**상세 기능:**
- 뉴스 긍정/부정 분석
- 중요도 평가
- 영향도 분석 (주가 변동 예측)
- 트렌드 분석

**분석 방법:**
- 자연어 처리 (NLP)
- 감성 사전 기반
- 머신러닝 모델

**기술 스택:** ML.NET, Sentiment Analysis

---

### 8.3 알림 연동 `[Phase 4]`
**기능 ID:** NI-003

**상세 기능:**
- 중요 뉴스 알림
- 전략 트리거 연동
- 대시보드 실시간 표시
- 뉴스 기반 자동 거래

**기술 스택:** Event-Driven Architecture

---

## 문서 버전

- **버전:** 1.0
- **최종 수정일:** 2025-10-15
- **작성자:** Development Team
- **검토자:** Architecture Team

---

## 참고 자료

- [키움 OpenAPI 가이드](https://www.kiwoom.com)
- [Clean Architecture 패턴](https://blog.cleancoder.com)
- [DDD 설계 원칙](https://martinfowler.com/tags/domain%20driven%20design.html)
- [CQRS 패턴](https://martinfowler.com/bliki/CQRS.html)