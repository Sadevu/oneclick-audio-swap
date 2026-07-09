# AudioQuickSwitch

## 프로젝트 목적

Windows에서 출력 장치를 변경할 때 기본 출력 장치(Default Device)만 변경되고 기본 통신 장치(Default Communications Device)는 변경되지 않아 Discord 등의 프로그램을 별도로 설정해야 하는 불편함이 있다.

또한 Steam Game Recording을 사용 중인데 출력 장치를 변경하면 녹화 오디오가 정상적으로 따라오지 않는 문제가 있어, 가능한 한 항상 Windows의 기본 장치와 기본 통신 장치를 동일하게 유지하려고 한다.

이 프로그램의 목적은 **출력 장치를 쉽고 빠르게 변경하면서 동시에 기본 장치와 기본 통신 장치를 모두 변경하는 것**이다.

---

# 개발 환경

- Language : C#
- Framework : .NET 8
- UI : Windows Forms
- Platform : Windows 11
- Target : x64

---

# 프로젝트 목표

가볍고 빠른 개인용 Windows Tray Utility

별도의 설정창 없이 필요한 기능만 제공한다.

프로그램은 항상 백그라운드에서 실행되며 작업표시줄 알림 영역(Tray)에 상주한다.

---

# MVP 기능

## 1. Tray Icon

프로그램 실행 시

- 작업표시줄 Tray에 아이콘 표시
- 메인 윈도우는 존재하지 않음

---

## 2. 좌클릭 Popup

Tray 아이콘 좌클릭 시

Windows 볼륨 선택 UI처럼 작은 Popup을 표시한다.

Popup에는

- 현재 출력 장치 목록
- 현재 기본 장치 표시
- 현재 기본 통신 장치 표시

가 보이면 좋다.

가능하면 현재 기본 장치는 체크 또는 강조 표시한다.

---

## 3. 장치 선택

Popup에서 장치를 클릭하면

해당 장치를

- Default Console
- Default Multimedia
- Default Communications

세 가지 모두 동일한 장치로 변경한다.

즉,

Windows의

- 기본 장치
- 기본 통신 장치

가 동시에 변경되어야 한다.

---

## 4. Popup 자동 닫기

장치 변경 후

Popup은 자동으로 닫힌다.

---

## 5. 우클릭 메뉴

Tray 우클릭 시

메뉴는 최소한으로 구성한다.

예시

- 종료

향후 설정 기능이 추가될 수 있다.

---

# 구현 제외

다음 기능은 현재 MVP에서 구현하지 않는다.

- Discord 출력 변경
- Discord 입력 변경
- Steam 설정 변경
- 프로그램별 오디오 출력 변경
- Hotkey
- 프로필 저장
- 시작프로그램 등록
- 다크모드 설정
- 자동 업데이트
- 다국어

---

# UI 방향

최대한 Windows 기본 UI 느낌을 유지한다.

과도한 디자인은 불필요하다.

목표는

"Windows에 원래 있는 기능처럼 보이는 것"

이다.

---

# 기술 구현

## 출력 장치 조회

Windows Core Audio API 사용

출력(Render) 장치 목록 조회

---

## 기본 장치 변경

선택한 Endpoint를

- eConsole
- eMultimedia
- eCommunications

모두 동일하게 설정한다.

Windows에서 사용하는

Default Device와

Default Communications Device를

동시에 변경하는 것이 핵심이다.

---

# 프로젝트 구조

AudioQuickSwitch

```
AudioQuickSwitch
│
├── Program.cs
├── TrayApplicationContext.cs
├── PopupForm.cs
├── Services
│   ├── AudioDeviceService.cs
│   └── DefaultDeviceService.cs
│
├── Native
│   ├── PolicyConfig.cs
│   ├── CoreAudio.cs
│   └── ComInterfaces.cs
│
└── Resources
    └── tray.ico
```

---

# 구현 원칙

- 불필요한 라이브러리 사용 최소화
- 유지보수하기 쉬운 구조
- UI와 오디오 제어 로직 분리
- COM 관련 코드는 Native 폴더에만 위치
- Service는 UI를 모르도록 구현
- UI는 Service만 호출

---

# 개발 순서

## Step 1

Tray Icon 생성

---

## Step 2

출력 장치 목록 조회

---

## Step 3

Popup UI 구현

---

## Step 4

장치 선택 이벤트 연결

---

## Step 5

기본 장치 + 기본 통신 장치 변경

---

## Step 6

현재 기본 장치 표시

---

# 참고 사항

Windows 작업표시줄의 출력 장치 변경 기능은 **기본 장치(Default Device)** 만 변경하며 **기본 통신 장치(Default Communications Device)** 는 변경하지 않는다.

이 프로그램은 이 동작을 보완하기 위한 유틸리티이다.

---

# 중요 요구사항

코드 품질을 우선한다.

- 불필요한 기능 추가 금지
- MVP 범위를 벗어나는 구현 금지
- 추후 확장이 쉽도록 구조 설계
- 한 번에 모든 기능을 구현하지 말고 단계별로 구현

먼저 프로젝트 구조를 생성하고 Tray Icon이 정상 동작하는 상태까지 구현한 후 다음 단계로 진행한다.