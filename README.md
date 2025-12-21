# 요추천자(Lumbar Puncture) 햅틱 시뮬레이터

요추 천자 시술을 햅틱 피드백을 통해 가상으로 연습할 수 있는 의료 시뮬레이션 프로젝트입니다. Haply Robotics의 Inverse3 햅틱 디바이스를 사용하여 실제 조직과의 상호작용을 실시간으로 시뮬레이션하고, 정확한 촉각 피드백을 제공합니다.

---

## 📋 목차

- [프로젝트 소개](#프로젝트-소개)
- [주요 기능](#주요-기능)
- [기술 스택](#기술-스택)
- [시스템 요구사항](#시스템-요구사항)
- [설치 및 설정](#설치-및-설정)
- [사용 방법](#사용-방법)
- [프로젝트 구조](#프로젝트-구조)
- [주요 컴포넌트 설명](#주요-컴포넌트-설명)
- [키보드 단축키](#키보드-단축키)
- [데이터 파일](#데이터-파일)
- [문제 해결](#문제-해결)
- [참고 자료](#참고-자료)
- [라이선스](#라이선스)

---

## 프로젝트 소개

이 프로젝트는 의료진이 요추 천자 시술을 안전하고 반복적으로 연습할 수 있도록 개발된 햅틱 기반 시뮬레이터입니다. 실제 의료 환경을 모방한 병원 복도 씬과 함께, 다음과 같은 핵심 기능을 제공합니다:

- **실시간 햅틱 피드백**: Inverse3 디바이스를 통한 정밀한 촉각 반응
- **물리 기반 시뮬레이션**: 강성(stiffness)과 감쇠(damping) 파라미터를 통한 현실적인 조직 반응
- **인대 파열 시뮬레이션**: Ligament of Recordati(LoR) 파열 감지 및 CSF 드립 효과
- **각도 측정 시스템**: 바늘 삽입 각도 모니터링 및 피드백
- **위험 경고 시스템**: 뼈 접촉 등 위험 상황 감지 및 알림

---

## 주요 기능

### 🎮 햅틱 피드백 시스템

- **HapticForceManager**: 여러 햅틱 이펙터의 힘을 통합 관리하고 안전 상한선 적용
- **IHapticEffector 인터페이스**: 확장 가능한 햅틱 힘 계산 시스템
- **멀티스레드 안전**: 햅틱 스레드와 Unity 메인 스레드 간 안전한 데이터 동기화

### 🧬 조직 상호작용

- **PhysicsHapticEffector**: Unity Physics 엔진과 햅틱 디바이스 간 동기화
- **HapticMaterial**: 조직별 맞춤형 햅틱 속성 (강성, 감쇠)

### 💧 인대 파열 및 CSF 드립

- **PlaneLoREffector**: Ligament of Recordati 평면 인대 시뮬레이션
- **RuptureToDrip**: 인대 파열 후 CSF(뇌척수액) 드립 효과 트리거
- **DripController**: 파티클 시스템을 사용한 현실적인 드립 애니메이션

### 📐 각도 측정 및 안내

- **AngleDisplay**: 바늘 삽입 각도 실시간 표시 (영유아 기준 50-60도)
- **LPSafeZoneBands**: L3-L4, L4-L5 interspace 시각적 안내 밴드

### ⚠️ 안전 시스템

- **HazardWarningTrigger**: 위험 물체(뼈 등) 접촉 감지
- **WarningUI**: 실시간 경고 메시지 표시
- **최대 힘 제한**: 사용자 안전을 위한 힘 상한선 (기본 20N)

### 📊 데이터 분석

- **SensorDataLoader**: CSV 형식의 센서 데이터 로드 및 분석
- **실제 측정 데이터**: 돼지 피부 계측 데이터 포함 (Assets/Data/SensorData/)

### 🎨 시각화 모드

- **ModeManager**: Guide 모드(아웃라인 표시)와 Focus 모드(아웃라인 숨김) 전환

---

## 기술 스택

### 핵심 플랫폼
- **Unity**: 2022.3.7f1
- **렌더 파이프라인**: Universal Render Pipeline (URP) 14.0.8
- **언어**: C#

### 햅틱 SDK
- **Haply Inverse**: 3.2.0
- **Haply Hardware API**: 1.1.6-preview
- **레지스트리**: https://unitypackages.haply.co

### XR 지원
- **Meta XR SDK**: 72.0.0 (VR 헤드셋 지원 가능)

### 주요 Unity 패키지
- **TextMesh Pro**: 3.0.6
- **XR Management**: 4.5.3
- **OpenXR**: 1.8.2
- **Timeline**: 1.7.5

---

## 시스템 요구사항

### 하드웨어

#### 필수
- **햅틱 디바이스**: Haply Inverse3
- **운영체제**: Windows 10 이상 (권장: Windows 11)
- **프로세서**: Intel Core i5 이상 또는 동급 AMD 프로세서
- **메모리**: 8GB RAM 이상 (권장: 16GB)
- **그래픽**: DirectX 11 호환 GPU

#### 선택사항
- **VR 헤드셋**: Meta Quest 시리즈 (XR 기능 사용 시)

### 소프트웨어

#### 필수
- **Unity Hub**: 최신 버전
- **Unity Editor**: 2022.3.7f1 이상
- **.NET Framework**: 4.8 이상

#### 개발 도구 (선택사항)
- **Visual Studio 2022** 또는 **Rider**: C# 스크립트 편집용
- **Git**: 버전 관리

---

## 설치 및 설정

### 1. 프로젝트 클론

```bash
git clone <repository-url>
cd LumbarPuncture
```

### 2. Unity에서 프로젝트 열기

1. **Unity Hub 실행**
2. **"Open"** 또는 **"Add project from disk"** 선택
3. 프로젝트 폴더(`LumbarPuncture`) 선택
4. Unity 2022.3.7f1 버전으로 프로젝트 열기

### 3. 패키지 자동 설치

Unity가 프로젝트를 열면 Package Manager가 자동으로 필요한 패키지를 다운로드합니다:
- Haply 패키지는 Haply Robotics의 스코프 레지스트리에서 자동 설치됩니다
- URP, TextMesh Pro 등 다른 패키지도 자동으로 설치됩니다

**참고**: 인터넷 연결이 필요하며, 첫 실행 시 다소 시간이 걸릴 수 있습니다.

### 4. 햅틱 디바이스 설정

1. **Inverse3 디바이스 연결**
   - USB 케이블로 컴퓨터에 연결
   - 디바이스 드라이버가 설치되어 있는지 확인

2. **디바이스 인식 확인**
   - Unity Editor에서 Play 모드 실행
   - Console 창에서 디바이스 연결 메시지 확인

### 5. 씬 선택

프로젝트에는 두 가지 주요 씬이 포함되어 있습니다:

- **SampleScene.unity**: 기본 테스트 씬
- **Hospital Corridor/Hospital Corridor.unity**: 병원 환경 씬 (권장)

---

## 사용 방법

### 기본 사용법

1. **씬 열기**
   - `Assets/Scenes/Hospital Corridor/Hospital Corridor.unity` 열기

2. **햅틱 디바이스 확인**
   - 씬에서 `Inverse3Controller` 컴포넌트가 있는 GameObject 확인
   - 디바이스가 정상적으로 연결되었는지 확인

3. **Play 모드 실행**
   - Unity Editor에서 **Play 버튼** 클릭
   - 햅틱 디바이스를 통해 조직과의 상호작용을 느낄 수 있습니다

### 파라미터 조정

#### TissueLayer (조직층)
- **Stiffness** (0-800): 조직의 강성, 높을수록 더 단단함
- **Damping** (0-3): 감쇠 계수, 움직임의 부드러움 조절

#### PlaneLoREffector (인대)
- **Stiffness**: 인대의 강성 (기본: 600)
- **Rupture Force**: 파열에 필요한 힘 (기본: 8N)
- **Post Rupture Stiffness**: 파열 후 강성 (기본: 10)

#### HapticForceManager
- **Max Force Magnitude**: 전체 힘 상한선 (기본: 20N)
- **Force Enabled**: 햅틱 피드백 활성화/비활성화

---

## 프로젝트 구조

```
LumbarPuncture/
├── Assets/
│   ├── CalculateAngle.cs          # 각도 계산 및 표시
│   ├── DripController.cs          # CSF 드립 컨트롤러
│   ├── GameManager.cs             # 게임 상태 관리
│   ├── HapticForceManager.cs      # 햅틱 힘 통합 관리
│   ├── HapticMaterial.cs          # 조직별 햅틱 속성
│   ├── HazardWarningTrigger.cs    # 위험 감지 트리거
│   ├── IHapticEffector.cs         # 햅틱 이펙터 인터페이스
│   ├── LPSafeZoneBands.cs         # 안전 구역 밴드
│   ├── ModeManager.cs             # 시각화 모드 관리
│   ├── PhysicsHapticEffector.cs   # 물리 기반 햅틱 이펙터
│   ├── PlaneLoREffector.cs        # 인대 평면 이펙터
│   ├── RuptureToDrip.cs           # 파열→드립 연결
│   ├── SensorDataLoader.cs        # 센서 데이터 로더
│   ├── SphereForceDrop.cs         # 구 형태 힘 드롭
│   ├── SphereForceEffector.cs     # 구 형태 힘 이펙터
│   ├── WarningUI.cs               # 경고 UI 시스템
│   │
│   ├── Data/
│   │   └── SensorData/            # 센서 측정 데이터
│   │       ├── 70_10.csv
│   │       ├── 80_10.csv
│   │       ├── 80.csv
│   │       ├── 90_10.csv
│   │       ├── 90.csv
│   │       └── ANALYSIS_REPORT.md
│   │
│   ├── Haply/                     # Haply 햅틱 디바이스 프리셋
│   │   └── Presets/
│   │
│   ├── QuickOutline/              # 아웃라인 효과 라이브러리
│   │   ├── Scripts/
│   │   └── Resources/
│   │
│   ├── Samples/                   # Haply 샘플 및 튜토리얼
│   │   └── Haply Inverse for Unity/
│   │
│   ├── Scenes/                    # Unity 씬 파일들
│   │   ├── SampleScene.unity
│   │   └── Hospital Corridor/     # 병원 복도 환경
│   │       └── Hospital Corridor.unity
│   │
│   ├── Settings/                  # URP 렌더링 설정
│   │
│   └── Resources/                 # 런타임 리소스
│       └── (Meta XR 설정 등)
│
├── Packages/                      # Unity 패키지 매니페스트
│   └── manifest.json
│
├── ProjectSettings/               # Unity 프로젝트 설정
│   ├── ProjectVersion.txt
│   └── ...
│
└── README.md                      # 이 파일
```

---

## 주요 컴포넌트 설명

### HapticForceManager

햅틱 힘을 통합 관리하는 핵심 컴포넌트입니다.

**주요 기능:**
- 씬 내 모든 `IHapticEffector` 구현체를 자동 등록
- 여러 이펙터의 힘을 합산
- 안전을 위한 최대 힘 제한 (`maxForceMagnitude`)
- 디바이스 상태 변경 시 자동 힘 계산

**사용 예시:**
```csharp
// Inspector에서 설정
- Inverse3: Inverse3Controller 참조
- Max Force Magnitude: 20 (기본값)
- Force Enabled: true
```

### PhysicsHapticEffector

Unity Physics 엔진과 햅틱 디바이스를 동기화하는 컴포넌트입니다.

**주요 기능:**
- ConfigurableJoint을 사용한 물리 기반 동기화
- 충돌 감지를 통한 힘 계산
- `HapticMaterial`을 통한 조직별 속성 적용
- 멀티스레드 안전한 데이터 캐싱

**파라미터:**
- `defaultStiffness`: 기본 강성 (400)
- `defaultDamping`: 기본 감쇠 (1)
- `collisionDetection`: 충돌 감지 활성화 여부
- `forceEnabled`: 힘 피드백 활성화 여부

### PlaneLoREffector

Ligament of Recordati(인대)를 평면으로 시뮬레이션하는 컴포넌트입니다.

**주요 기능:**
- 평면 충돌 감지 및 힘 계산
- 파열 임계값(`ruptureForce`) 초과 시 파열 상태 전환
- 파열 후 강성 감소 (`postRuptureStiffness`)
- `IHapticEffector` 인터페이스 구현

**사용 방법:**
1. GameObject에 `PlaneLoREffector` 컴포넌트 추가
2. Transform의 `up` 방향이 인대의 법선 방향
3. `RuptureToDrip`과 연결하여 CSF 드립 효과 트리거

### CubeTissueLayer

큐브 형태의 조직과 햅틱 커서 간 상호작용을 처리합니다.

**주요 기능:**
- 큐브 내부 침투 감지
- 침투 깊이에 따른 힘 계산
- 상대 속도 기반 감쇠 적용
- 스레드 안전한 변환 행렬 캐싱

**파라미터:**
- `stiffness`: 조직 강성 (500)
- `damping`: 감쇠 계수 (1)
- `cursorRadius`: 커서 반경 (0.002m)

### SensorDataLoader

CSV 형식의 센서 데이터를 로드하고 분석하는 유틸리티 클래스입니다.

**주요 기능:**
- CSV 파일에서 힘 데이터 로드
- 시간 기반 선형 보간
- 통계 정보 계산 (평균, 최소, 최대 힘)

**CSV 형식:**
```
timestamp, elapsed_time, sample, X, Y, Z
```

**사용 예시:**
```csharp
var dataSet = SensorDataLoader.LoadCSV("Data/SensorData/80.csv");
SensorDataLoader.PrintStatistics(dataSet);
Vector3 force = SensorDataLoader.GetInterpolatedForce(dataSet, 1.5f);
```

### ModeManager

시각화 모드를 전환하는 컴포넌트입니다.

**모드:**
- **Normal (Guide)**: 아웃라인 표시, 가이드 모드
- **OutlineOff (Focus)**: 아웃라인 숨김, 집중 모드

**단축키:**
- `1`: Focus 모드 (아웃라인 숨김)
- `2`: Guide 모드 (아웃라인 표시)

### GameManager

게임 상태와 입력을 관리하는 컴포넌트입니다.

**주요 기능:**
- 물리 주파수 조정 (240Hz 기본)
- 햅틱 피드백 토글
- 충돌 감지 토글
- 주파수 비교 UI 표시

---

## 키보드 단축키

| 키 | 기능 |
|---|---|
| `1` | Focus 모드 (아웃라인 숨김) |
| `2` | Guide 모드 (아웃라인 표시) |

---

## 데이터 파일

### 센서 데이터

`Assets/Data/SensorData/` 폴더에는 실제 돼지 피부 계측 데이터가 포함되어 있습니다:

- **70_10.csv**: 70도 각도, 10mm/s 속도
- **80_10.csv**: 80도 각도, 10mm/s 속도
- **80.csv**: 80도 각도
- **90_10.csv**: 90도 각도, 10mm/s 속도
- **90.csv**: 90도 각도

각 파일은 다음 형식을 따릅니다:
```
timestamp, elapsed_time, sample, X, Y, Z
```

### 데이터 분석

`ANALYSIS_REPORT.md` 파일에 데이터 분석 결과가 포함되어 있을 수 있습니다.

---

## 문제 해결

### 햅틱 디바이스가 인식되지 않음

1. **USB 연결 확인**
   - 디바이스가 올바르게 연결되었는지 확인
   - 다른 USB 포트 시도

2. **드라이버 확인**
   - Haply 디바이스 드라이버가 설치되어 있는지 확인
   - [Haply 공식 문서](https://docs.haply.co/) 참조

3. **Unity Console 확인**
   - 디바이스 연결 오류 메시지 확인
   - `Inverse3Controller` 컴포넌트가 씬에 있는지 확인

### 힘이 적용되지 않음

1. **Force Enabled 확인**
   - `HapticForceManager`의 `Force Enabled` 체크박스 확인
   - `PhysicsHapticEffector`의 `forceEnabled` 확인

2. **충돌 감지 확인**
   - `collisionDetection`이 활성화되어 있는지 확인
   - Collider가 올바르게 설정되어 있는지 확인

3. **최대 힘 제한 확인**
   - `HapticForceManager`의 `Max Force Magnitude` 값 확인
   - 계산된 힘이 상한선을 초과하지 않는지 확인

### 성능 문제

1. **물리 주파수 조정**
   - `GameManager`의 `physicsFrequency` 값 조정
   - 기본값 240Hz에서 시작하여 필요에 따라 조정

2. **디버그 로그 비활성화**
   - `PhysicsHapticEffector`의 디버그 로그 주석 처리

3. **씬 최적화**
   - 불필요한 GameObject 비활성화
   - 복잡한 셰이더 대신 간단한 머티리얼 사용

### 빌드 오류

1. **패키지 버전 확인**
   - `Packages/manifest.json`의 패키지 버전 확인
   - Unity 버전과 호환되는지 확인

2. **메타 파일 확인**
   - `.meta` 파일이 누락되지 않았는지 확인
   - Unity가 자동으로 재생성하도록 기다림

---

## 버전 히스토리

### 현재 버전
- Unity 2022.3.7f1
- Haply Inverse 3.2.0
- URP 14.0.8

### 주요 업데이트
- 햅틱 힘 통합 관리 시스템 추가 (`HapticForceManager`)
- 인대 파열 시뮬레이션 추가 (`PlaneLoREffector`)
- CSF 드립 효과 추가 (`DripController`)
- 각도 측정 시스템 추가 (`AngleDisplay`)
- 안전 구역 표시 추가 (`LPSafeZoneBands`)
- 센서 데이터 로더 추가 (`SensorDataLoader`)
