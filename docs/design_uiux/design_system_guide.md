# 디자인 시스템 가이드
**프로젝트:** 주식 자동매매 시스템  
**디자인 언어:** Modern Finance UI  
**버전:** 1.0

---

## 목차
1. [디자인 원칙](#디자인-원칙)
2. [컬러 시스템](#컬러-시스템)
3. [타이포그래피](#타이포그래피)
4. [스페이싱 시스템](#스페이싱-시스템)
5. [그림자와 깊이](#그림자와-깊이)
6. [그리드 시스템](#그리드-시스템)
7. [아이콘 시스템](#아이콘-시스템)
8. [애니메이션](#애니메이션)

---

## 디자인 원칙

### 1. 명확성 (Clarity)
금융 데이터는 명확하게 표시되어야 합니다.
- 중요 정보는 시각적 위계를 통해 강조
- 숫자는 읽기 쉽게 포맷팅
- 에러나 경고는 즉시 인지 가능하게

### 2. 효율성 (Efficiency)
트레이더는 빠른 의사결정이 필요합니다.
- 최소 클릭으로 원하는 작업 완료
- 키보드 단축키 지원
- 자주 사용하는 기능은 쉽게 접근

### 3. 신뢰성 (Trust)
금융 시스템은 신뢰감을 줘야 합니다.
- 전문적이고 안정적인 비주얼
- 일관된 디자인 언어
- 명확한 피드백과 상태 표시

### 4. 유연성 (Flexibility)
다양한 사용자 니즈를 지원합니다.
- 커스터마이징 가능한 대시보드
- 라이트/다크 모드 지원
- 다양한 해상도 지원

---

## 컬러 시스템

### Primary Colors (주 색상)

#### Blue - 신뢰와 전문성
```
Primary 50:  #E3F2FD  // 가장 연한 배경
Primary 100: #BBDEFB  // 연한 배경
Primary 200: #90CAF9  // Hover 배경
Primary 300: #64B5F6  // 보조 요소
Primary 400: #42A5F5  // 인터랙티브 요소
Primary 500: #2196F3  // ★ 메인 색상
Primary 600: #1E88E5  // Hover 상태
Primary 700: #1976D2  // Active 상태
Primary 800: #1565C0  // 강조
Primary 900: #0D47A1  // 최고 강조
```

**사용 예:**
- Primary 500: 버튼, 링크, 선택된 항목
- Primary 100: 선택된 배경, Hover 배경
- Primary 700: 버튼 Pressed 상태

---

### Semantic Colors (의미 색상)

#### Success - 수익/성공
```
Success Light:  #E8F5E9  // 배경
Success Main:   #4CAF50  // ★ 메인
Success Dark:   #2E7D32  // 텍스트
Success Darker: #1B5E20  // 강조
```

**사용 예:**
- 수익 금액 표시
- 매수 주문 버튼
- 성공 알림

#### Error - 손실/에러
```
Error Light:  #FFEBEE  // 배경
Error Main:   #F44336  // ★ 메인
Error Dark:   #C62828  // 텍스트
Error Darker: #B71C1C  // 강조
```

**사용 예:**
- 손실 금액 표시
- 매도 주문 버튼
- 에러 메시지

#### Warning - 주의/경고
```
Warning Light:  #FFF3E0  // 배경
Warning Main:   #FF9800  // ★ 메인
Warning Dark:   #E65100  // 텍스트
Warning Darker: #BF360C  // 강조
```

**사용 예:**
- 경고 메시지
- 리스크 초과 알림
- 대기 중인 주문

#### Info - 정보
```
Info Light:  #E1F5FE  // 배경
Info Main:   #03A9F4  // ★ 메인
Info Dark:   #0277BD  // 텍스트
Info Darker: #01579B  // 강조
```

**사용 예:**
- 정보 메시지
- 툴팁
- 가이드

---

### Neutral Colors (중립 색상)

#### Gray Scale
```
White:    #FFFFFF  // 배경
Gray 50:  #FAFAFA  // 밝은 배경
Gray 100: #F5F5F5  // 카드 배경, Hover
Gray 200: #EEEEEE  // 구분선
Gray 300: #E0E0E0  // Border
Gray 400: #BDBDBD  // Placeholder
Gray 500: #9E9E9E  // Disabled
Gray 600: #757575  // Secondary Text
Gray 700: #616161  // Body Text
Gray 800: #424242  // Heading
Gray 900: #212121  // Primary Text
Black:    #000000  // 순수 검정 (거의 사용 안 함)
```

**텍스트 용도:**
- Gray 900: 제목, 중요 텍스트
- Gray 700: 본문 텍스트
- Gray 600: 보조 텍스트
- Gray 500: Disabled 텍스트

**배경 용도:**
- White: 메인 배경
- Gray 50: 섹션 배경
- Gray 100: 카드 배경

---

### Dark Mode Colors

#### Background
```
Background Primary:   #121212  // 메인 배경
Background Secondary: #1E1E1E  // 카드/패널
Background Tertiary:  #2C2C2C  // Elevated 요소
```

#### Text
```
Text Primary:   rgba(255, 255, 255, 0.87)  // 87% 불투명도
Text Secondary: rgba(255, 255, 255, 0.60)  // 60% 불투명도
Text Disabled:  rgba(255, 255, 255, 0.38)  // 38% 불투명도
```

#### Dividers
```
Divider: rgba(255, 255, 255, 0.12)  // 12% 불투명도
```

---

## 타이포그래피

### Font Family

**Primary Font: Pretendard**
```css
font-family: 'Pretendard', -apple-system, BlinkMacSystemFont, 
             'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 
             'Helvetica Neue', sans-serif;
```

**Numeric Font: JetBrains Mono** (숫자, 금액 표시)
```css
font-family: 'JetBrains Mono', 'Consolas', 'Monaco', monospace;
```

---

### Font Sizes & Line Heights

#### Display (대제목)
```
Display Large:  48px / 56px  (크기 / 줄간격)
Display Medium: 40px / 48px
Display Small:  36px / 44px

Font Weight: Semibold (600)
Letter Spacing: -0.02em
Usage: 랜딩 페이지, 주요 섹션 제목
```

#### Heading (제목)
```
H1: 32px / 40px  - Semibold (600)
H2: 24px / 32px  - Semibold (600)
H3: 20px / 28px  - Semibold (600)
H4: 18px / 26px  - Medium (500)
H5: 16px / 24px  - Medium (500)
H6: 14px / 22px  - Medium (500)

Letter Spacing: -0.01em
Usage: 페이지 제목, 섹션 헤더
```

#### Body (본문)
```
Body Large:  16px / 24px  - Regular (400)
Body Medium: 14px / 22px  - Regular (400)
Body Small:  12px / 18px  - Regular (400)

Letter Spacing: normal
Usage: 일반 텍스트, 설명
```

#### Label (레이블)
```
Label Large:  14px / 20px  - Medium (500)
Label Medium: 12px / 18px  - Medium (500)
Label Small:  11px / 16px  - Medium (500)

Letter Spacing: 0.01em
Usage: 폼 라벨, 버튼 텍스트
```

#### Caption (캡션)
```
Caption: 12px / 16px  - Regular (400)
Usage: 주석, 힌트, 타임스탬프
```

---

### Numeric Display (숫자 표시)

#### Price (주가)
```
Large:  24px / 32px  - Bold (700)
Medium: 18px / 24px  - Bold (700)
Small:  14px / 20px  - Semibold (600)

Font Family: JetBrains Mono
Letter Spacing: 0.02em
```

#### Amount (금액)
```
Large:  20px / 28px  - Bold (700)
Medium: 16px / 24px  - Semibold (600)
Small:  14px / 20px  - Medium (500)

Font Family: JetBrains Mono
Format: 천 단위 콤마 (123,456,789)
```

---

## 스페이싱 시스템

**8px 기준 시스템**

```
0:   0px    - 여백 없음
1:   4px    - XS (최소 간격)
2:   8px    - SM (작은 간격) ★ 기본 단위
3:   12px   - 컴포넌트 내부 간격
4:   16px   - MD (중간 간격) ★ 자주 사용
5:   20px   - 
6:   24px   - LG (큰 간격) ★ 자주 사용
8:   32px   - XL (매우 큰 간격)
10:  40px   - 섹션 간격
12:  48px   - XXL (섹션 간격)
16:  64px   - 메이저 섹션 간격
20:  80px   - 
24:  96px   - 페이지 상하단 여백
```

### 적용 예시

**컴포넌트 내부:**
```
버튼 패딩: 12px 24px (3 × 6)
입력 필드 패딩: 12px 16px (3 × 4)
카드 패딩: 24px (6)
```

**요소 간 간격:**
```
같은 그룹 요소: 8px (2)
다른 그룹 요소: 16px (4)
섹션 간: 32px (8) ~ 48px (12)
```

**페이지 여백:**
```
모바일: 16px (4)
태블릿: 24px (6)
데스크톱: 32px (8) ~ 48px (12)
```

---

## 그림자와 깊이

### Elevation Levels

```css
/* Level 0 - No Shadow */
box-shadow: none;

/* Level 1 - Subtle (카드, 입력 필드) */
box-shadow: 0 1px 3px rgba(0, 0, 0, 0.12),
            0 1px 2px rgba(0, 0, 0, 0.06);

/* Level 2 - Low (호버 카드) */
box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08),
            0 2px 4px rgba(0, 0, 0, 0.04);

/* Level 3 - Medium (드롭다운, 팝오버) */
box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12),
            0 2px 6px rgba(0, 0, 0, 0.08);

/* Level 4 - High (모달, 대화상자) */
box-shadow: 0 8px 32px rgba(0, 0, 0, 0.16),
            0 4px 12px rgba(0, 0, 0, 0.12);

/* Level 5 - Very High (전면 모달) */
box-shadow: 0 16px 48px rgba(0, 0, 0, 0.20),
            0 8px 16px rgba(0, 0, 0, 0.16);
```

### Colored Shadows (강조용)

```css
/* Primary Button Shadow */
box-shadow: 0 2px 8px rgba(33, 150, 243, 0.3);

/* Success Shadow */
box-shadow: 0 2px 8px rgba(76, 175, 80, 0.3);

/* Error Shadow */
box-shadow: 0 2px 8px rgba(244, 67, 54, 0.3);
```

---

## 그리드 시스템

### Breakpoints

```
XS (Mobile):     0px ~ 599px
SM (Tablet):   600px ~ 959px
MD (Desktop):  960px ~ 1279px
LG (Large):   1280px ~ 1919px
XL (X-Large): 1920px+
```

### Container Max Width

```
XS: 100% - 32px (좌우 16px 여백)
SM: 100% - 48px (좌우 24px 여백)
MD: 960px
LG: 1280px
XL: 1440px
```

### Grid Columns

```
XS: 4 columns  (모바일)
SM: 8 columns  (태블릿)
MD: 12 columns (데스크톱) ★ 기본
LG: 12 columns
XL: 12 columns
```

### Gutter (열 간격)

```
XS: 16px
SM: 24px
MD: 24px
LG: 32px
XL: 32px
```

---

## 아이콘 시스템

### Icon Library
**Material Icons** (https://fonts.google.com/icons)

### Icon Sizes

```
XS: 16px  - 인라인 아이콘
SM: 20px  - 버튼 아이콘
MD: 24px  - ★ 기본 크기
LG: 32px  - 큰 아이콘
XL: 48px  - 특별한 경우
```

### Icon Colors

```
Primary:   #2196F3  - 주요 액션
Success:   #4CAF50  - 긍정적 액션
Error:     #F44336  - 부정적 액션
Warning:   #FF9800  - 경고
Disabled:  #9E9E9E  - 비활성
Default:   #616161  - 기본
```

### Icon Usage

**Navigation Icons**
```
home, dashboard, trending_up, account_balance, 
settings, notifications, person, logout
```

**Action Icons**
```
add, edit, delete, save, cancel, search, 
filter_list, refresh, more_vert
```

**Status Icons**
```
check_circle (성공), error (에러), 
warning (경고), info (정보)
```

**Finance Icons**
```
trending_up (상승), trending_down (하락),
attach_money (금액), account_balance_wallet (지갑)
```

---

## 애니메이션

### Timing Functions

```css
/* Standard - 일반적인 전환 */
transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
duration: 0.2s ~ 0.3s

/* Deceleration - 요소가 들어올 때 */
transition-timing-function: cubic-bezier(0.0, 0, 0.2, 1);
duration: 0.2s ~ 0.3s

/* Acceleration - 요소가 나갈 때 */
transition-timing-function: cubic-bezier(0.4, 0, 1, 1);
duration: 0.15s ~ 0.2s

/* Sharp - 빠른 변화 */
transition-timing-function: cubic-bezier(0.4, 0, 0.6, 1);
duration: 0.15s
```

### Duration Guidelines

```
매우 빠름:  100ms  - 색상 변화
빠름:      150ms  - 간단한 전환
보통:      200ms  - 호버 효과
느림:      300ms  - 복잡한 전환
매우 느림:  500ms  - 페이지 전환
```

### Common Animations

#### Fade In
```css
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

animation: fadeIn 0.3s ease;
```

#### Slide In (from Right)
```css
@keyframes slideInRight {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

animation: slideInRight 0.3s cubic-bezier(0.4, 0, 0.2, 1);
```

#### Scale Up (Modal)
```css
@keyframes scaleUp {
  from {
    transform: scale(0.9);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

animation: scaleUp 0.3s cubic-bezier(0.4, 0, 0.2, 1);
```

#### Ripple Effect (버튼 클릭)
Material Design Ripple 효과 사용

---

## 반응형 디자인 규칙

### Mobile First Approach

```css
/* 기본 스타일 (모바일) */
.container {
  padding: 16px;
}

/* 태블릿 이상 */
@media (min-width: 600px) {
  .container {
    padding: 24px;
  }
}

/* 데스크톱 이상 */
@media (min-width: 960px) {
  .container {
    padding: 32px;
  }
}
```

### 반응형 폰트 크기

```
H1: 24px (mobile) → 32px (desktop)
H2: 20px (mobile) → 24px (desktop)
Body: 14px (mobile) → 16px (desktop)
```

### 터치 타겟 크기

```
최소 크기: 44px × 44px (iOS 가이드라인)
권장 크기: 48px × 48px (Material Design)
간격: 최소 8px
```

---

## 접근성 (Accessibility)

### 색상 대비

```
Normal Text (14px+):   최소 4.5:1
Large Text (18px+):    최소 3:1
그래픽/아이콘:          최소 3:1
```

### 키보드 네비게이션

```
Tab Order: 논리적 순서
Focus Indicator: 명확한 아웃라인 (2px, Primary 색상)
Skip Links: 메인 콘텐츠로 바로 이동
```

### ARIA 라벨

```html
<button aria-label="주문 실행">
<input aria-describedby="email-error">
<div role="alert" aria-live="polite">
```

---

## Claude Code 사용 예시

### 1. 전체 디자인 시스템 적용

```bash
claude-code

입력: "/docs/design-system.md 파일을 참고해서
ColorTokens.cs 파일을 생성해줘. 
모든 Primary, Semantic, Neutral 색상을 포함하고,
다크모드 색상도 별도 클래스로 만들어줘."
```

### 2. 타이포그래피 시스템 생성

```bash
입력: "design-system.md의 타이포그래피 섹션을 참고해서
Typography.cs 파일을 만들어줘.
모든 폰트 크기, Weight, Line Height를 상수로 정의해줘."
```

### 3. 컴포넌트에 디자인 시스템 적용

```bash
입력: "design-system.md의 색상과 스페이싱을 사용해서
PrimaryButton 컴포넌트를 만들어줘.
모든 상태(Normal, Hover, Active)를 구현하고,
애니메이션도 포함해줘."
```

---

## 체크리스트

새로운 컴포넌트 개발 시:
- [ ] 디자인 시스템 색상 사용
- [ ] 8px 기반 스페이싱 적용
- [ ] 타이포그래피 가이드 준수
- [ ] 적절한 그림자 레벨 사용
- [ ] 반응형 디자인 구현
- [ ] 다크모드 지원
- [ ] 애니메이션 적용 (0.2s ~ 0.3s)
- [ ] 접근성 기준 충족
- [ ] 색상 대비 4.5:1 이상

---

**Version History:**
- v1.0 (2025-10-25): 초기 작성
- Next: 아이콘 세트 추가, 일러스트레이션 가이드