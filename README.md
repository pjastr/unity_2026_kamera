# Kamera i Cinemachine w Unity – sceny demonstracyjne

Projekt Unity **6.3 LTS (6000.3.24f1)** pokazujący komponent **Camera** (rzutowanie, FOV, płaszczyzny obcinania, Viewport Rect, Culling Mask, stos kamer URP, tło, Render Texture, przeliczenia ekran ↔ świat) oraz pakiet **Cinemachine 3** (Follow, Rotation/Position Composer, priorytety i przejścia, Orbital Follow, Third Person Follow, FPP, Target Group, Confiner 2D, Spline Dolly, Noise, Impulse, Clear Shot).

Każde zagadnienie ma **osobną scenę** (`01_…` – `19_…`). W każdej scenie jest panel z opisem po polsku (lewy górny róg), panel stanu z parametrami kamery na żywo i dziennikiem zdarzeń (prawy górny róg) oraz pasek ze sterowaniem (dół ekranu).

- Render pipeline: **URP** (ustawienia z szablonu „Universal 3D”)
- Pakiety: **Cinemachine 3.1.7**, **Splines 2.8.4**, Input System, uGUI
- Wejście: **Input System** – wyłącznie akcje z domyślnego pliku `Assets/InputSystem_Actions.inputactions` (Project-wide Actions), bez własnych map i akcji
- Warstwy (Tags and Layers): 8 – `Minimapa`, 9 – `Bron`, 10 – `TylkoMonitor`

---

## 1. Szybki start

1. Otwórz projekt w Unity Hub (wersja **6000.3.24f1**) albo uruchom `Narzedzia/Otworz_projekt_w_Unity.bat`. Przy pierwszym otwarciu Unity pobierze pakiety (Cinemachine, Splines) – może to potrwać kilka minut.
2. Sceny są **generowane kodem**. Przy pierwszym otwarciu pojawi się pytanie „Wygenerować teraz 19 scen?” – wybierz **Generuj**. Można to zrobić także później: menu **Kamera → Generuj wszystkie sceny**.
3. **Kamera → Otwórz pierwszą scenę i uruchom Play** (albo otwórz dowolną scenę z `Assets/Kamera/Scenes` i naciśnij Play).
4. Klawiszami **1 / 2** przechodzisz do poprzedniej / następnej sceny (działa także w buildzie).

> Jeżeli Unity pokaże okno „URP Material upgrade” – kliknij **OK**.
> Jeżeli klawisze nie działają – **Kamera → Napraw ustawienia projektu** i uruchom ponownie edytor.

### Build Windows (opcjonalnie)
`Narzedzia/Generuj_sceny_i_zbuduj_Windows.bat` (edytor z tym projektem musi być zamknięty) – generuje sceny i buduje `Builds/Windows/KameraDemo.exe`. Log: `Logs/batch_build.log`.

---

## 2. Menu „Kamera” w edytorze

| Polecenie | Działanie |
|---|---|
| **Generuj wszystkie sceny** | Tworzy od nowa 19 scen, materiały, asset `CM_Blends` i ustawia Build Settings |
| **Panel scen** | Okno z listą scen: *Otwórz*, ▶ (otwórz i Play), ↻ (wygeneruj tylko tę scenę) |
| **Otwórz pierwszą scenę i uruchom Play** | Skrót do pokazu |
| **Zbuduj wersję Windows (x64)** | Build do `Builds/Windows/KameraDemo.exe` |
| **Napraw ustawienia projektu** | Active Input Handling = Input System, Project-wide Actions, warstwy, Build Settings |
| **Otwórz instrukcję (README.md)** | Otwiera ten plik |

**Uwaga:** ponowne generowanie nadpisuje ręczne zmiany w scenach. Eksperymentuj, zmieniając wartości w Inspectorze w trakcie Play, albo zrób kopię sceny.

---

## 3. Sterowanie (domyślne akcje Input System)

| Klawisz / mysz | Akcja z `InputSystem_Actions` | Działanie |
|---|---|---|
| **Spacja** | Player/Jump | akcja główna sceny (przełączenie trybu kamery) |
| **LPM** | Player/Attack | akcja dodatkowa sceny |
| **PPM + ruch myszy** | UI/RightClick + Player/Look | obrót kamery / orbita / rozglądanie |
| **W A S D** | Player/Move | ruch swobodnej kamery albo postaci |
| **Shift** | Player/Sprint | szybciej |
| **Kółko myszy** | UI/ScrollWheel | przód/tył, zoom (FOV), odległość orbity |
| **C** | Player/Crouch | pokaż / ukryj panel opisu |
| **przytrzymaj E** | Player/Interact (Hold) | reset sceny |
| **1 / 2** | Player/Previous / Player/Next | poprzednia / następna scena |
| **Esc** | UI/Cancel | wyjście (tylko w buildzie) |

---

## 4. Opis scen

### Część I – komponent Camera (URP)

| Scena | Co pokazuje | Spacja | LPM |
|---|---|---|---|
| **01_Perspektywa_Ortografia** | Perspective vs Orthographic, FOV vs Size, macierz projekcji na żywo | perspektywa ↔ ortografia | rzut ukośny (własna `projectionMatrix`) |
| **02_FOV_DollyZoom** | Field of View, FOV poziomy/pionowy, efekt dolly zoom, Physical Camera (ogniskowa, matryca) | dolly zoom | Physical Camera on/off |
| **03_Clipping_Frustum** | Near/Far Clip Plane, ostrosłup widzenia, frustum culling (`GeometryUtility`), z-fighting | kolejny zestaw Near/Far | rysowanie ostrosłupa |
| **04_Viewport_PodzialEkranu** | Viewport Rect: pełny ekran, podział pionowy/poziomy, 4 widoki (góra/przód/bok/perspektywa) | kolejny układ | – |
| **05_CullingMask_Minimapa** | warstwy, Culling Mask, minimapa jako druga kamera (Viewport Rect + priorytet) | warstwa „Minimapa” w kamerze głównej | minimapa ortho/perspektywa |
| **06_CameraStack_Overlay** | URP Camera Stack: Base + Overlay, broń FPP nie wchodzi w ścianę | jedna kamera ↔ stos | FOV kamery Overlay |
| **07_Tlo_RenderTexture** | Background Type (Skybox, Solid Color, Uninitialized), Render Texture (monitoring), obiekt widoczny tylko na monitorze | typ tła | warstwa „TylkoMonitor” |
| **08_Ekran_Swiat_Raycast** | ScreenToViewportPoint, ScreenPointToRay + Raycast, WorldToScreenPoint, ViewportToWorldPoint | kule w narożnikach widoku | znacznik w punkcie trafienia |
| **09_Kamera_ze_skryptu** | kamera podążająca bez Cinemachine: dziecko celu, Lerp w Update, SmoothDamp w LateUpdate, LookAt | metoda | limit 20 FPS |

### Część II – Cinemachine 3

| Scena | Co pokazuje | Spacja | LPM |
|---|---|---|---|
| **10_CM_Podstawy_Follow** | CinemachineBrain + CinemachineCamera, Follow + Rotation Composer, Damping, Dead Zone, Hard Limits, Lookahead | zestaw parametrów | Binding Mode |
| **11_CM_Priorytety_Blendy** | Priority, Default Blend (Ease In Out, Linear, Hard In, Cut), Custom Blends (asset) | następna kamera | styl przejścia |
| **12_CM_Orbita_FreeLook** | Orbital Follow (Sphere / Three Ring), osie wejścia, Recentering | Orbit Style | – |
| **13_CM_TrzeciaOsoba** | Third Person Follow, Shoulder Offset, Camera Side, Avoid Obstacles | zmiana ramienia | – |
| **14_CM_PierwszaOsoba** | Hard Lock To Target + Pan Tilt, Lens FOV, szum przy chodzeniu | FOV 60° ↔ 90° | kołysanie on/off |
| **15_CM_Z_gory_TargetGroup** | Target Group + Position Composer + Group Framing | grupa ↔ jedna jednostka | Size Adjustment |
| **16_CM_2D_Confiner** | kamera ortogonalna 2D, Position Composer z Dead Zone, Confiner 2D (PolygonCollider2D) | Confiner on/off | Orthographic Size 5 ↔ 8 |
| **17_CM_Dolly_Spline** | Spline Dolly (Fixed Speed / Nearest Point To Target), Spline Cart | tryb Automatic Dolly | Camera Rotation |
| **18_CM_Szum_Impuls** | Basic Multi Channel Perlin (profile szumu), Impulse Source + Impulse Listener | profil szumu | wybuch (impuls) |
| **19_CM_ClearShot** | Clear Shot + Deoccluder (ocena ujęcia), Activate After, Min Duration | Clear Shot ↔ jedna kamera | Activate After |

---

## 5. Architektura

```
Assets/
  InputSystem_Actions.inputactions   ← domyślne akcje (Project-wide Actions)
  Settings/                          ← ustawienia URP z szablonu
  Kamera/
    Scenes/       01_… – 19_….unity (generowane)
    Materials/    materiały URP (generowane)
    Textures/     szachownica podłogi (generowana)
    Data/         CM_Blends.asset – Custom Blends dla sceny 11 (generowany)
    Scripts/Core/          DemoInput, DemoHud, DemoActionBase, FreeCamera, WorldLabel, AutoMover, PlayerMover, ChaseCamera
    Scripts/Demo/          komponenty scen 01–09 (kamera Unity)
    Scripts/Cinemachine/   komponenty scen 10–19 (Cinemachine)
    Editor/                SceneBuilder (menu Kamera, build), SceneDefinitions_01_09 / _10_19, SceneBrowserWindow, FirstRunSetup
Narzedzia/                 pliki .bat
```

- `DemoActionBase` – komponenty sceny reagują na Spację (`OnMainAction`) i LPM (`OnSecondaryAction`).
- `DemoHud` – panel opisu, panel stanu (`DemoHud.SetStatus`), dziennik (`DemoHud.Log`), skróty klawiszowe.
- Osie kamer Cinemachine (Orbital Follow, Pan Tilt) zmieniają skrypty sceny na podstawie domyślnych akcji Input System – w projektach zwykle robi to komponent **Cinemachine Input Axis Controller**.

---

## 6. Uwagi dotyczące Unity 6 / URP / Cinemachine 3

- W URP zamiast *Clear Flags* jest **Background Type** (Skybox / Solid Color / Uninitialized), a zamiast *Depth* – **Priority**; kamera ma **Render Type** Base lub Overlay i listę **Stack**.
- Cinemachine 3: `CinemachineCamera` zastąpiła `CinemachineVirtualCamera`, komponenty mają nowe nazwy (Follow, Rotation Composer, Position Composer, Orbital Follow, Pan Tilt, Third Person Follow, Spline Dolly, Deoccluder…), przestrzeń nazw to `Unity.Cinemachine`.
- Profile szumu (Noise Settings) pochodzą z pakietu: `Packages/com.unity.cinemachine/Presets/Noise`.

---

## 7. Rozwiązywanie problemów

| Problem | Rozwiązanie |
|---|---|
| Błędy kompilacji `Unity.Cinemachine` / `UnityEngine.Splines` | Window → Package Manager: sprawdź, czy zainstalowane są Cinemachine i Splines (wersje z `Packages/manifest.json`) |
| Klawisze nie działają | **Kamera → Napraw ustawienia projektu**, restart edytora |
| Brak scen / 1–2 nie przełączają | **Kamera → Generuj wszystkie sceny** |
| Różowe obiekty | projekt musi używać URP (Project Settings → Graphics → `PC_RPAsset`) |
| Ikony minimapy / broń widoczne niepoprawnie | sprawdź warstwy 8–10 w Project Settings → Tags and Layers |
