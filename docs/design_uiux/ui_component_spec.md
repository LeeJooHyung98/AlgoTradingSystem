# UI 컴포넌트 명세서
**프로젝트:** 주식 자동매매 시스템  
**버전:** 1.0  
**최종 수정:** 2025-10-25

---

## 1. 버튼 컴포넌트

### Primary Button
**용도:** 주요 액션 (주문 실행, 전략 시작)

**비주얼 스펙:**
- 배경색: `#2196F3` (Primary Blue)
- 텍스트 색상: `#FFFFFF`
- 높이: `40px`
- 패딩: `12px 24px`
- Border Radius: `8px`
- 폰트: `Pretendard Medium 14px`
- 그림자: `0 2px 8px rgba(33, 150, 243, 0.3)`

**상태별 스타일:**
```
- Normal: 배경 #2196F3
- Hover: 배경 #1976D2 + 그림자 증가
- Active: 배경 #1565C0
- Disabled: 배경 #E0E0E0, 텍스트 #9E9E9E
- Loading: 배경 #2196F3 + Spinner 표시
```

**인터랙션:**
- Hover 시 0.2초 transition
- 클릭 시 scale(0.98) 애니메이션
- Ripple 효과 (Material Design)

**코드 요청 예시:**
```
"위 스펙대로 WPF용 PrimaryButton 컴포넌트를 만들어줘.
Material Design In XAML 사용하고, 모든 상태 스타일 포함해줘."
```

---

### Secondary Button
**용도:** 부가 액션 (취소, 닫기)

**비주얼 스펙:**
- 배경색: `Transparent`
- 테두리: `1px solid #2196F3`
- 텍스트 색상: `#2196F3`
- 높이: `40px`
- 패딩: `12px 24px`
- Border Radius: `8px`

**상태별 스타일:**
```
- Normal: 투명 배경, #2196F3 테두리
- Hover: 배경 rgba(33, 150, 243, 0.08)
- Active: 배경 rgba(33, 150, 243, 0.16)
- Disabled: 테두리 #E0E0E0, 텍스트 #9E9E9E
```

---

### Icon Button
**용도:** 툴바, 액션 아이콘 (새로고침, 설정)

**비주얼 스펙:**
- 크기: `40px × 40px` (원형)
- 배경색: `Transparent`
- 아이콘 크기: `20px`
- 아이콘 색상: `#757575`

**상태별 스타일:**
```
- Hover: 배경 rgba(0, 0, 0, 0.04)
- Active: 배경 rgba(0, 0, 0, 0.08)
```

---

## 2. 입력 필드 컴포넌트

### Text Input
**비주얼 스펙:**
- 높이: `48px`
- 배경: `#FFFFFF`
- 테두리: `1px solid #E0E0E0`
- Border Radius: `8px`
- 패딩: `12px 16px`
- 폰트: `Pretendard Regular 14px`

**라벨:**
- 위치: 입력 필드 위 `8px` 간격
- 폰트: `Pretendard Medium 12px`
- 색상: `#616161`

**상태별 스타일:**
```
- Normal: 테두리 #E0E0E0
- Focus: 테두리 #2196F3 (2px), 그림자 0 0 0 3px rgba(33, 150, 243, 0.1)
- Error: 테두리 #F44336, 하단에 에러 메시지 표시
- Disabled: 배경 #F5F5F5, 테두리 #E0E0E0
```

**에러 메시지:**
- 위치: 입력 필드 하단 `4px` 간격
- 폰트: `Pretendard Regular 12px`
- 색상: `#F44336`

**Placeholder:**
- 색상: `#BDBDBD`
- 폰트: `Pretendard Regular 14px`

---

### Number Input (금액, 수량)
**추가 기능:**
- 천 단위 콤마 자동 삽입
- 증가/감소 버튼 (Stepper)
- 최소/최대값 제한

**비주얼:**
- 기본 Text Input과 동일
- 우측에 증가/감소 버튼 (`20px × 20px`)
- 텍스트 정렬: `right` (숫자는 우측 정렬)

---

### Select / Dropdown
**비주얼 스펙:**
- 높이: `48px`
- 배경: `#FFFFFF`
- 테두리: `1px solid #E0E0E0`
- Border Radius: `8px`
- 우측 아이콘: 드롭다운 화살표 (`20px`)

**드롭다운 메뉴:**
- 최대 높이: `320px` (스크롤)
- 항목 높이: `40px`
- 항목 Hover: 배경 `#F5F5F5`
- 항목 선택: 배경 `rgba(33, 150, 243, 0.08)`, 좌측 체크 아이콘

---

## 3. 카드 컴포넌트

### Standard Card
**용도:** 포지션 카드, 전략 카드

**비주얼 스펙:**
- 배경: `#FFFFFF`
- Border Radius: `12px`
- 그림자: `0 2px 12px rgba(0, 0, 0, 0.08)`
- 패딩: `24px`
- 최소 너비: `280px`

**구조:**
```
[카드 헤더]
- 제목: Pretendard Semibold 16px, #212121
- 서브 타이틀: Pretendard Regular 12px, #757575
- 액션 버튼: 우측 정렬

[카드 바디]
- 패딩: 16px 0

[카드 푸터]
- 테두리 상단: 1px solid #E0E0E0
- 패딩: 16px 0 0 0
```

**Hover 효과:**
- 그림자 증가: `0 4px 16px rgba(0, 0, 0, 0.12)`
- Transform: `translateY(-2px)`
- Transition: `0.3s ease`

---

### Status Card (손익 카드)
**추가 기능:**
- 상태에 따른 색상 변경
  - 수익: 좌측 Border `4px solid #4CAF50` (Green)
  - 손실: 좌측 Border `4px solid #F44336` (Red)
  - 중립: 좌측 Border `4px solid #9E9E9E` (Gray)

**숫자 표시:**
- 수익: `#4CAF50`, 앞에 `+` 표시
- 손실: `#F44336`, 앞에 `-` 표시
- 폰트: `Pretendard Bold 20px`

---

## 4. 테이블 컴포넌트

### Data Table
**비주얼 스펙:**
- 배경: `#FFFFFF`
- Border Radius: `12px`
- 테두리: `1px solid #E0E0E0`

**헤더:**
- 배경: `#FAFAFA`
- 높이: `48px`
- 텍스트: `Pretendard Semibold 12px, #616161`
- 정렬: 좌측 (텍스트), 우측 (숫자)

**행:**
- 높이: `56px`
- 구분선: `1px solid #F5F5F5`
- Hover: 배경 `#F5F5F5`
- 선택됨: 배경 `rgba(33, 150, 243, 0.08)`

**셀 패딩:**
- 좌우: `16px`

**액션 열:**
- 우측 고정
- 아이콘 버튼 (수정, 삭제)

---

## 5. 차트 컴포넌트

### Line Chart (손익 차트)
**비주얼 스펙:**
- 배경: `#FFFFFF`
- Border Radius: `12px`
- 패딩: `24px`
- 최소 높이: `320px`

**라인 스타일:**
- 수익 라인: `#4CAF50`, 굵기 `2px`
- 기준선: `#E0E0E0`, 굵기 `1px`, 점선

**그리드:**
- 색상: `#F5F5F5`
- 간격: 자동 계산

**툴팁:**
- 배경: `rgba(0, 0, 0, 0.8)`
- 텍스트: `#FFFFFF`
- Border Radius: `4px`
- 패딩: `8px 12px`
- 폰트: `Pretendard Regular 12px`

---

## 6. 모달 컴포넌트

### Standard Modal
**비주얼 스펙:**
- 배경: `#FFFFFF`
- Border Radius: `16px`
- 최대 너비: `600px`
- 그림자: `0 8px 32px rgba(0, 0, 0, 0.24)`

**오버레이:**
- 배경: `rgba(0, 0, 0, 0.5)`
- 애니메이션: Fade In

**헤더:**
- 높이: `64px`
- 패딩: `20px 24px`
- 제목: `Pretendart Semibold 18px`
- 닫기 버튼: 우측 상단

**바디:**
- 패딩: `24px`
- 최대 높이: `calc(100vh - 200px)`
- 스크롤: 필요시

**푸터:**
- 높이: `72px`
- 패딩: `16px 24px`
- 버튼: 우측 정렬, 간격 `8px`

**애니메이션:**
- 등장: Scale(0.9) → Scale(1), Opacity 0 → 1
- Duration: `0.3s`
- Easing: `cubic-bezier(0.4, 0, 0.2, 1)`

---

## 7. 알림/토스트 컴포넌트

### Toast Notification
**비주얼 스펙:**
- 너비: `360px`
- Border Radius: `8px`
- 그림자: `0 4px 16px rgba(0, 0, 0, 0.12)`
- 패딩: `16px`

**타입별 색상:**
```
- Success: 배경 #E8F5E9, 좌측 Border 4px #4CAF50, 아이콘 #4CAF50
- Error: 배경 #FFEBEE, 좌측 Border 4px #F44336, 아이콘 #F44336
- Warning: 배경 #FFF3E0, 좌측 Border 4px #FF9800, 아이콘 #FF9800
- Info: 배경 #E3F2FD, 좌측 Border 4px #2196F3, 아이콘 #2196F3
```

**위치:**
- 우측 상단: 화면 모서리에서 `24px` 간격
- 여러 개 표시시 세로로 쌓임, 간격 `12px`

**자동 사라짐:**
- Success/Info: 3초 후
- Warning: 5초 후
- Error: 수동 닫기만

**애니메이션:**
- 등장: 우측에서 슬라이드 인
- 사라짐: 우측으로 슬라이드 아웃 + Fade

---

## 8. 네비게이션 컴포넌트

### Sidebar Navigation
**비주얼 스펙:**
- 너비: `280px` (확장), `72px` (축소)
- 배경: `#1E1E1E` (다크) 또는 `#FFFFFF` (라이트)
- 높이: `100vh`

**메뉴 항목:**
- 높이: `48px`
- 패딩: `12px 16px`
- Border Radius: `8px` (내부에 4px 여백)
- 아이콘 크기: `24px`
- 텍스트: `Pretendard Medium 14px`

**상태별 스타일 (다크 모드):**
```
- Normal: 투명 배경, 텍스트 #B0B0B0
- Hover: 배경 rgba(255, 255, 255, 0.08), 텍스트 #FFFFFF
- Active: 배경 #2196F3, 텍스트 #FFFFFF
```

**축소 상태:**
- 아이콘만 표시 (중앙 정렬)
- 툴팁으로 메뉴명 표시

---

## 9. 배지/라벨 컴포넌트

### Status Badge
**비주얼 스펙:**
- 높이: `24px`
- 패딩: `4px 12px`
- Border Radius: `12px` (Pill 형태)
- 폰트: `Pretendard Medium 12px`

**상태별 색상:**
```
- Active (활성): 배경 #E8F5E9, 텍스트 #2E7D32
- Inactive (비활성): 배경 #EEEEEE, 텍스트 #616161
- Warning (주의): 배경 #FFF3E0, 텍스트 #E65100
- Error (오류): 배경 #FFEBEE, 텍스트 #C62828
```

---

## 10. 로딩 인디케이터

### Spinner
**비주얼 스펙:**
- 크기: `40px` (기본), `24px` (small), `64px` (large)
- 색상: `#2196F3`
- 굵기: `4px`
- 애니메이션: 회전 (1.2초 무한 반복)

### Progress Bar
**비주얼 스펙:**
- 높이: `4px`
- 배경: `#E0E0E0`
- 진행 바: `#2196F3`
- Border Radius: `2px`

**애니메이션:**
- Indeterminate: 좌우로 이동
- Determinate: 0% → 100% 채워짐

---

## 사용 방법

### Claude Code에 요청하는 방법

**1단계: 이 MD 파일을 프로젝트에 저장**
```
/docs/ui-component-spec.md
```

**2단계: Claude Code 실행 후 요청**
```bash
claude-code

입력: "프로젝트의 /docs/ui-component-spec.md 파일을 참고해서
WPF로 Primary Button 컴포넌트를 만들어줘.
파일명: Components/PrimaryButton.xaml"
```

**3단계: 다른 컴포넌트도 동일하게 요청**
```
"같은 디자인 시스템으로 Secondary Button도 만들어줘"
"같은 스타일로 Text Input 컴포넌트 만들어줘"
```

---

## 체크리스트

컴포넌트 구현 시 확인사항:
- [ ] 모든 상태 (Normal, Hover, Active, Disabled) 구현
- [ ] 애니메이션 Transition 적용
- [ ] 접근성 (Keyboard Navigation, ARIA)
- [ ] 반응형 (다양한 해상도)
- [ ] 다크모드 지원
- [ ] 재사용 가능한 구조
- [ ] Props/Parameters로 커스터마이징 가능

---

**버전 히스토리:**
- v1.0 (2025-10-25): 초기 작성