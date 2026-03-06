# VR Ehliyet Sınav Simülasyonu

Unity VR tabanlı ehliyet sınavı simülasyon oyunu.

---

## Gereksinimler

- Unity **2022.3 LTS** veya üstü
- **Universal Render Pipeline (URP)** veya HDRP
- XR Interaction Toolkit (`com.unity.xr.interaction.toolkit`)
- XR Plugin Management (`com.unity.xr.management`)
- OpenXR Plugin (`com.unity.xr.openxr`)

---

## Kurulum

### 1. XR Paketleri

```
Window → Package Manager → + Add package by name:
  com.unity.xr.management
  com.unity.xr.openxr
  com.unity.xr.interaction.toolkit
```

### 2. OpenXR Ayarları

```
Edit → Project Settings → XR Plug-in Management → OpenXR ✓
  → Interaction Profiles → Oculus Touch / Valve Index ekle
```

### 3. Input System

```
Edit → Project Settings → Player → Active Input Handling → "Both"
```

### 4. Physics

```
Edit → Project Settings → Physics
  → Default Solver Iterations: 10
  → Default Solver Velocity Iterations: 5
  → Fixed Timestep: 0.01 (100 Hz)
```

### 5. Quality / FPS

```
Edit → Project Settings → Quality
  → VSync Count: Don't Sync
Application.targetFrameRate = 90;  // GameManager.Awake() içinde
```

---

## Klasör Yapısı

```
Assets/Scripts/
├── Core/              GameManager, AudioManager, EventBus, SceneLoader, GameSettings
├── Vehicle/           VehicleController, GearSystem, BrakeSystem, SteeringSystem,
│                      HandbrakeSystem, SignalSystem, DesktopInputHandler
├── VR/                VRInputManager, GazeTracker, VRSteeringWheel, VRGearLever
├── UI/                HUDManager, MainMenuUI, FeedbackOverlay, ResultsScreenUI
├── AI/                AdaptiveDifficultySystem, IdealRouteCalculator, ErrorScoringSystem, TrafficAI
├── Analytics/         AnalyticsTracker, ReactionTimeMeasurer
├── Data/              PlayerProgress, SaveSystem, ModuleConfig (ScriptableObject)
├── Levels/            ModuleBase, Module1-5, ExamSimulation
├── Camera/            CameraController
└── Collision/         CollisionDetector, TriggerZone
```

---

## Sahne Entegrasyonu

### A. Araç Prefabı

1. Boş GameObject → `PlayerVehicle`
2. Ekle: `Rigidbody`, `VehicleController`, `GearSystem`, `BrakeSystem`, `SteeringSystem`, `HandbrakeSystem`, `SignalSystem`, `CollisionDetector`
3. Alt objeler: 4× `WheelCollider` + 4× wheel mesh
4. `VehicleController` Inspector'da wheel collider ve mesh referanslarını ata
5. Tag: `Player`

### B. VR Rig

1. `XR Origin (XR Rig)` sahneye ekle
2. Camera Offset altına `VRInputManager` ekle
3. `VRSteeringWheel` → direksiyon objesine ekle, XR Grab Interactable ile
4. `VRGearLever` → vites objesine ekle
5. `GazeTracker` → XR Camera'ya ekle

### C. Manager Objeleri

Boş GameObject `[Managers]` oluştur, DontDestroyOnLoad:

| Component | Açıklama |
|-----------|----------|
| `GameManager` | Oyun durumu |
| `AudioManager` | Ses yönetimi |
| `SceneLoader` | Sahne geçişi |
| `SaveSystem` | JSON kayıt |
| `AnalyticsTracker` | Metrik takibi |

### D. UI

- **World Space Canvas** (VR) → HUDManager, FeedbackOverlay
- **Screen Space Canvas** → MainMenuUI, ResultsScreenUI

### E. Modüller

Her sahneye ilgili modül scriptini ekle:

| Sahne | Script | Ek Gereksinim |
|-------|--------|----------------|
| Module1 | `Module1_CockpitSafety` | GazeTracker, GazeZone collider'ları |
| Module2 | `Module2_CityTraffic` | TrafficAI, TrafficLightZone |
| Module3 | `Module3_Parking` | ParkingZone collider'ları, IdealRouteCalculator |
| Module4 | `Module4_ChallengingConditions` | ParticleSystem (rain), PedestrianHazard |
| Module5 | `Module5_FreeDrive` | Açık harita |
| Exam | `ExamSimulation` | Tüm modüller sıralı |

### F. ScriptableObject Config'ler

```
Assets → Create → VRCarSim → Module Config   (her modül için 1 adet)
Assets → Create → VRCarSim → Game Settings    (1 adet global)
```

---

## Tag'ler

```
Player, Vehicle, TrafficAI, Pedestrian, TrafficCone, Curb, Barrier
```

**Edit → Project Settings → Tags and Layers** üzerinden ekle.

---

## JSON Kayıt

Otomatik olarak `Application.persistentDataPath/player_progress.json` dosyasına yazılır.

---

## Keyboard Kontrolleri (VR olmadan test)

| Tuş | İşlev |
|-----|-------|
| W/S | Gaz / Fren |
| A/D | Direksiyon |
| Space | Fren |
| E | Vites yükselt |
| Q | Vites düşür |
| H | El freni |
| Z/X | Sol/Sağ sinyal |
