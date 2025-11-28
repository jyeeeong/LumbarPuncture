# Lumbar Puncture 시뮬레이터

요추 천자(Lumbar Puncture) 시술을 햅틱 피드백을 통해 가상으로 연습할 수 있는 의료 시뮬레이션 프로젝트입니다.

## 📋 프로젝트 소개

이 프로젝트는 의료진이 요추 천자 시술을 안전하고 반복적으로 연습할 수 있도록 개발된 햅틱 기반 시뮬레이터입니다. Haply Robotics의 Inverse3 햅틱 디바이스를 사용하여 실제 조직과의 상호작용을 시뮬레이션하고, 실시간 힘 피드백을 제공합니다.

## ✨ 주요 기능

- **햅틱 피드백**: Inverse3 디바이스를 통한 실시간 촉각 피드백
- **조직 상호작용**: 조직층과의 충돌 감지 및 힘 계산
- **물리 기반 시뮬레이션**: 강성(stiffness)과 감쇠(damping) 파라미터를 통한 현실적인 조직 반응
- **병원 환경**: 실제 병원 복도 씬을 포함한 몰입형 환경

## 🛠️ 기술 스택

- **Unity**: 2022.3.7f1
- **렌더 파이프라인**: Universal Render Pipeline (URP) 14.0.8
- **햅틱 SDK**: 
  - Haply Inverse 3.2.0
  - Haply Hardware API 1.1.6-preview

## 📦 요구사항

### 하드웨어
- Haply Inverse3 햅틱 디바이스
- Windows 10 이상 (권장)

### 소프트웨어
- Unity Hub
- Unity Editor 2022.3.7f1 이상
- Visual Studio 또는 Rider (스크립트 편집용)

## 🚀 설치 방법

1. **프로젝트 클론**
   ```bash
   git clone <repository-url>
   cd LumbarPuncture
   ```

2. **Unity에서 프로젝트 열기**
   - Unity Hub를 실행합니다
   - "Open" 또는 "Add project from disk"를 선택합니다
   - 프로젝트 폴더를 선택합니다
   - Unity 2022.3.7f1 버전을 사용하여 프로젝트를 엽니다

3. **패키지 자동 설치**
   - Unity가 프로젝트를 열면 Package Manager가 자동으로 필요한 패키지를 다운로드합니다
   - Haply 패키지는 Haply Robotics의 스코프 레지스트리에서 자동으로 설치됩니다

4. **햅틱 디바이스 연결**
   - Inverse3 디바이스를 컴퓨터에 연결합니다
   - 디바이스 드라이버가 설치되어 있는지 확인합니다

## 🎮 사용 방법

1. **씬 열기**
   - `Assets/Scenes/SampleScene.unity` 또는 `Assets/Scenes/Hospital Corridor/Hospital Corridor.unity`를 엽니다

2. **햅틱 디바이스 설정**
   - 씬에서 `Inverse3Controller` 컴포넌트가 있는 GameObject를 찾습니다
   - `TissueLayer` 컴포넌트가 있는 GameObject에 Inverse3Controller를 할당합니다

3. **파라미터 조정**
   - `TissueLayer` 컴포넌트의 Inspector에서 다음 파라미터를 조정할 수 있습니다:
     - **Stiffness** (0-800): 조직의 강성, 높을수록 더 단단함
     - **Damping** (0-3): 감쇠 계수, 움직임의 부드러움 조절

4. **플레이 모드 실행**
   - Unity Editor에서 Play 버튼을 클릭합니다
   - 햅틱 디바이스를 통해 조직과의 상호작용을 느낄 수 있습니다

## 📁 프로젝트 구조

```
LumbarPuncture/
├── Assets/
│   ├── Haply/                    # Haply 햅틱 디바이스 관련 프리셋
│   ├── Scenes/                   # Unity 씬 파일들
│   │   ├── SampleScene.unity
│   │   └── Hospital Corridor/    # 병원 복도 환경
│   ├── Settings/                 # URP 렌더링 설정
│   ├── TissueLayer.cs            # 주요 조직 상호작용 스크립트
│   └── Samples/                  # Haply 샘플 및 튜토리얼
├── Packages/                     # Unity 패키지 매니페스트
├── ProjectSettings/              # Unity 프로젝트 설정
└── README.md                     # 이 파일
```

## 🔧 주요 스크립트

### TissueLayer.cs
조직층과 햅틱 커서 간의 상호작용을 처리하는 핵심 스크립트입니다.

**주요 기능:**
- 햅틱 커서와 조직(볼) 간의 충돌 감지
- 침투 깊이에 따른 힘 계산
- 상대 속도 기반 감쇠 적용
- 스레드 안전한 데이터 캐싱

**파라미터:**
- `stiffness`: 조직의 강성 (기본값: 500)
- `damping`: 감쇠 계수 (기본값: 1)

## 📚 참고 자료

- [Haply Robotics 공식 문서](https://docs.haply.co/)
- [Unity URP 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Haply Unity 패키지 레지스트리](https://unitypackages.haply.co)

## 📄 라이선스

이 프로젝트는 Haply Robotics Inc.의 저작권을 포함하고 있습니다.

```
Copyright 2024 Haply Robotics Inc. All rights reserved.
```

## 🤝 기여

이슈 리포트나 개선 제안은 언제든지 환영합니다.

## 📧 문의

프로젝트에 대한 문의사항이 있으시면 이슈를 생성해주세요.

---

**주의**: 이 시뮬레이터는 교육 및 훈련 목적으로 제작되었으며, 실제 의료 시술을 대체할 수 없습니다.

