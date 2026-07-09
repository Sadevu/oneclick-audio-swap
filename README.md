# AudioQuickSwitch

## English

Small Windows tray utility for switching the default output device and default communications device together.

### Goal

Windows can change the default output device without changing the default communications device. AudioQuickSwitch keeps both targets aligned from a small tray utility.

### Development

This is a Windows Forms app targeting `.NET 8` and `x64`.

Run locally on Windows:

```powershell
dotnet run --project .\AudioQuickSwitch\AudioQuickSwitch.csproj
```

## 한국어

AudioQuickSwitch는 Windows의 기본 출력 장치와 기본 통신 장치를 함께 바꾸기 위한 작은 트레이 유틸리티입니다.

### 목표

Windows에서는 출력 장치를 바꿔도 기본 통신 장치가 그대로 남을 수 있습니다. AudioQuickSwitch는 트레이에서 빠르게 장치를 선택해 두 기본 장치를 같은 출력 장치로 맞추는 것을 목표로 합니다.

### 개발

이 프로젝트는 `.NET 8`, `x64` 대상의 Windows Forms 앱입니다.

Windows 로컬 실행:

```powershell
dotnet run --project .\AudioQuickSwitch\AudioQuickSwitch.csproj
```
