using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

namespace Kamera.EditorTools
{
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 10 – CINEMACHINE: PODSTAWY (FOLLOW + ROTATION COMPOSER)
        // =====================================================================
        static void S10_CmBasics()
        {
            Begin("10 · Cinemachine – podstawy: Follow i Rotation Composer",
                "<b>Cinemachine</b> nie zastępuje kamery Unity. Na Main Camera jest <b>CinemachineBrain</b>, który co klatkę " +
                "kopiuje do niej pozycję, obrót i obiektyw z aktywnej kamery wirtualnej <b>CinemachineCamera</b>.\n\n" +
                "CinemachineCamera „CM_Kamera” ma cel (Tracking Target = czerwona postać) i dwa komponenty proceduralne:\n" +
                "• <b>Position Control: Follow</b> – kamera trzyma stały offset (0, 5, −10) od celu,\n" +
                "• <b>Rotation Control: Rotation Composer</b> – obraca kamerę tak, by cel był w zadanym miejscu kadru.\n\n" +
                "Spacja – zestawy parametrów:\n" +
                "1. Damping = 0 – sztywno, każdy ruch celu od razu w kamerze,\n" +
                "2. Damping – kamera „nadąża” z opóźnieniem (czas w sekundach),\n" +
                "3. <b>Dead Zone</b> – obszar kadru, w którym ruch celu nie obraca kamery; <b>Hard Limits</b> – granica, " +
                "której cel nie może przekroczyć,\n" +
                "4. <b>Lookahead</b> – kamera wyprzedza ruch celu.\n" +
                "LPM – Binding Mode: World Space (offset w osiach świata) ↔ Lock To Target With World Up (offset za plecami celu).",
                "<b>Spacja</b> – zestaw Damping / Dead Zone / Lookahead   <b>LPM</b> – Binding Mode", "", cinemachine: true);
            Floor();
            var target = Character("Cel", Vector3.zero, M_Red);
            Mover(target, AutoMover.PathShape.Lissajous, Vector3.zero, new Vector2(12, 9), 0.35f, 0.6f);
            Label(target, "Tracking Target", 1.4f);
            PillarRing("Slupy", Vector3.zero, 22f, 16, 4f, M_Teal);
            Scatter("Klocki", 16, 20f, 21);

            var cam = CmCamera("CM_Kamera", new Vector3(0, 5, -10), Vector3.up);
            cam.Follow = target.transform;
            var follow = cam.gameObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0, 5, -10);
            follow.TrackerSettings = TrackerSettings.Default;
            var composer = AimComposer(cam, new Vector3(0, 0.5f, 0), new Vector2(0.5f, 0.5f));

            var demo = Manager<CMFollowPresets>();
            demo.follow = follow;
            demo.composer = composer;
        }

        // =====================================================================
        // 11 – PRIORYTETY I PRZEJŚCIA (BLENDS)
        // =====================================================================
        static void S11_CmPriority()
        {
            var main = Begin("11 · Cinemachine – priorytety i przejścia (blends)",
                "W scenie są cztery kamery CinemachineCamera ustawione wokół pomnika. Brain wybiera kamerę o najwyższym " +
                "<b>Priority</b>; przy równym priorytecie wygrywa ta włączona najpóźniej.\n\n" +
                "Spacja – kolejna kamera: skrypt podnosi jej Priority do 20, pozostałym ustawia 10.\n\n" +
                "Zmiana aktywnej kamery to <b>blend</b> – płynne przejście pozycji, obrotu i obiektywu. " +
                "<b>Default Blend</b> na Brainie określa kształt krzywej i czas.\n" +
                "LPM – styl: Ease In Out, Linear, Hard In, Cut (cięcie, 0 s).\n\n" +
                "<b>Custom Blends</b> (asset CinemachineBlenderSettings w Assets/Kamera/Data) pozwala ustawić inne przejście dla " +
                "konkretnej pary kamer – tu z „CM_Gora” do „CM_Zblizenie” zawsze jest cięcie.\n\n" +
                "W panelu stanu: priorytety, kamera LIVE i postęp przejścia.",
                "<b>Spacja</b> – następna kamera (Priority)   <b>LPM</b> – styl Default Blend", "", cinemachine: true);
            Floor();
            var mon = Monument("Pomnik", Vector3.zero);
            var orb = Ball("Satelita", new Vector3(6, 1.5f, 0), 1f, M_Purple);
            Mover(orb, AutoMover.PathShape.Circle, Vector3.zero, new Vector2(6, 6), 0.8f);
            PillarRing("Slupy", Vector3.zero, 14f, 12, 3f, M_Teal);
            Scatter("Klocki", 10, 20f, 5);

            Vector3 look = new Vector3(0, 2.5f, 0);
            var cams = new[]
            {
                CmCamera("CM_Przod", new Vector3(0, 3, -13), look, 10, 50f),
                CmCamera("CM_Bok", new Vector3(14, 5, 2), look, 10, 50f),
                CmCamera("CM_Gora", new Vector3(0.5f, 24, -3), look, 10, 50f),
                CmCamera("CM_Zblizenie", new Vector3(2.2f, 3.4f, -4f), look + Vector3.up * 1.5f, 10, 30f),
            };
            foreach (var c in cams)
            {
                c.LookAt = mon.transform;
                AimComposer(c, new Vector3(0, 2.5f, 0), Vector2.zero);
                Label(c.gameObject, c.name, 0.6f, 13);
            }
            cams[3].GetComponent<CinemachineRotationComposer>().TargetOffset = new Vector3(0, 4.4f, 0);

            var brain = main.GetComponent<CinemachineBrain>();
            brain.CustomBlends = BlendAsset();

            var demo = Manager<CMPrioritySwitcher>();
            demo.brain = brain;
            demo.cameras = cams;
        }

        static CinemachineBlenderSettings BlendAsset()
        {
            string path = $"{DataDir}/CM_Blends.asset";
            var a = AssetDatabase.LoadAssetAtPath<CinemachineBlenderSettings>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<CinemachineBlenderSettings>();
                AssetDatabase.CreateAsset(a, path);
            }
            a.CustomBlends = new[]
            {
                new CinemachineBlenderSettings.CustomBlend
                {
                    From = "CM_Gora", To = "CM_Zblizenie",
                    Blend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f)
                }
            };
            EditorUtility.SetDirty(a);
            return a;
        }

        // =====================================================================
        // 12 – ORBITAL FOLLOW (FREELOOK)
        // =====================================================================
        static void S12_CmOrbital()
        {
            Begin("12 · Cinemachine – kamera orbitalna (Orbital Follow)",
                "Kamera krąży wokół postaci po powierzchni sfery – to dawny <b>FreeLook</b>. " +
                "Komponent <b>Orbital Follow</b> ma trzy osie wejścia (InputAxis):\n" +
                "• Horizontal Axis – obrót wokół postaci (−180..180°, zawijany),\n" +
                "• Vertical Axis – wysokość na orbicie (−10..45°),\n" +
                "• Radial Axis – mnożnik odległości (0.5..2).\n" +
                "Tu osie zmienia skrypt na podstawie domyślnych akcji Input System (zwykle robi to " +
                "komponent Cinemachine Input Axis Controller). Po 2 s bez ruchu myszy działa <b>Recentering</b> – " +
                "kamera wraca za plecy postaci.\n\n" +
                "Spacja – <b>Orbit Style</b>: Sphere (stały promień) ↔ Three Ring (trzy okręgi: górny, środkowy i dolny o różnych " +
                "promieniach – patrząc z dołu kamera jest bliżej).\n\n" +
                "WASD – ruch postaci względem kierunku kamery (Shift – bieg).",
                "<b>Spacja</b> – Orbit Style Sphere ↔ Three Ring",
                "<b>WASD</b> – ruch postaci   <b>PPM + mysz</b> – orbita   <b>kółko</b> – odległość", cinemachine: true);
            Floor();
            var player = Character("Gracz", new Vector3(0, 0, 0), M_Red, controller: true);
            player.tag = "Player";
            player.AddComponent<PlayerMover>().relativeTo = PlayerMover.Relative.Camera;
            PillarRing("Slupy", Vector3.zero, 20f, 14, 5f, M_Teal);
            Scatter("Klocki", 18, 18f, 33);

            var cam = CmCamera("CM_Orbita", new Vector3(0, 3, -7), Vector3.up);
            cam.Follow = player.transform;
            var orbital = cam.gameObject.AddComponent<CinemachineOrbitalFollow>();
            orbital.TargetOffset = new Vector3(0, 0.5f, 0);
            var ts = TrackerSettings.Default;
            ts.PositionDamping = new Vector3(0.3f, 0.3f, 0.3f);
            orbital.TrackerSettings = ts;
            orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbital.Radius = 7f;
            orbital.Orbits = new Cinemachine3OrbitRig.Settings
            {
                Top = new Cinemachine3OrbitRig.Orbit { Height = 5.5f, Radius = 2.5f },
                Center = new Cinemachine3OrbitRig.Orbit { Height = 1.5f, Radius = 7f },
                Bottom = new Cinemachine3OrbitRig.Orbit { Height = -0.6f, Radius = 3.5f },
                SplineCurvature = 0.5f,
            };
            orbital.HorizontalAxis = new InputAxis
            {
                Value = 0, Center = 0, Range = new Vector2(-180, 180), Wrap = true,
                Recentering = new InputAxis.RecenteringSettings { Enabled = true, Wait = 2f, Time = 1.5f }
            };
            orbital.VerticalAxis = new InputAxis
            {
                Value = 17.5f, Center = 17.5f, Range = new Vector2(-10, 45), Wrap = false,
                Recentering = new InputAxis.RecenteringSettings { Enabled = true, Wait = 2f, Time = 1.5f }
            };
            orbital.RadialAxis = new InputAxis
            {
                Value = 1, Center = 1, Range = new Vector2(0.5f, 2f), Wrap = false,
                Recentering = new InputAxis.RecenteringSettings { Enabled = false, Wait = 2f, Time = 1.5f }
            };
            AimComposer(cam, new Vector3(0, 0.5f, 0), new Vector2(0.2f, 0.2f));

            Manager<CMOrbitInput>().orbital = orbital;
        }

        // =====================================================================
        // 13 – THIRD PERSON FOLLOW
        // =====================================================================
        static void S13_CmThirdPerson()
        {
            Begin("13 · Cinemachine – kamera zza ramienia (Third Person Follow)",
                "Typowa kamera gier akcji TPP. <b>Third Person Follow</b> buduje „ramię” kamery: od celu (Follow) " +
                "przez bark (<b>Shoulder Offset</b>), w górę (<b>Vertical Arm Length</b>) i do tyłu (<b>Camera Distance</b>). " +
                "Kamera kopiuje obrót celu, więc nie potrzebuje komponentu Rotation Control.\n\n" +
                "Celem jest obiekt „Celownik” – dziecko postaci na wysokości głowy. Skrypt obraca postać w poziomie (yaw), " +
                "a celownik w pionie (pitch) – kamera podąża za oboma obrotami.\n\n" +
                "Spacja – <b>Camera Side</b>: prawe ramię (1) ↔ lewe ramię (0), z płynnym przejściem.\n\n" +
                "<b>Avoid Obstacles</b>: gdy między postacią a kamerą jest kolumna lub ściana (warstwa Default), kamera " +
                "przysuwa się do postaci. Postać ma tag Player i jest ignorowana.\n\n" +
                "WASD – chód (A/D – krok w bok), PPM + mysz – obrót i celowanie.",
                "<b>Spacja</b> – zmiana ramienia (Camera Side)",
                "<b>WASD</b> – ruch   <b>Shift</b> – bieg   <b>PPM + mysz</b> – obrót i celowanie", cinemachine: true);
            Floor();
            var player = Character("Gracz", new Vector3(0, 0, -4), M_Red, controller: true);
            player.tag = "Player";
            var mover = player.AddComponent<PlayerMover>();
            mover.relativeTo = PlayerMover.Relative.Self;
            mover.speed = 4f;
            var aim = new GameObject("Celownik");
            aim.transform.SetParent(player.transform, false);
            aim.transform.localPosition = new Vector3(0, 0.6f, 0);

            var g = Group("Przeszkody");
            for (int x = -3; x <= 3; x++)
            for (int z = -1; z <= 4; z++)
            {
                if ((x + z) % 2 != 0) continue;
                Box($"Kolumna_{x}_{z}", new Vector3(x * 4f, 2f, z * 4f + 2f), new Vector3(1f, 4f, 1f), (x + z) % 4 == 0 ? M_Stone : M_Teal, g);
            }
            Box("Sciana_1", new Vector3(-8, 1.5f, -8), new Vector3(10, 3, 0.6f), M_Wall, g);
            Box("Sciana_2", new Vector3(10, 1.5f, 0), new Vector3(0.6f, 3, 12), M_Wall, g);
            Scatter("Klocki", 10, 28f, 41);

            var cam = CmCamera("CM_TPP", new Vector3(0.6f, 2f, -8f), new Vector3(0, 1.6f, 0), 10, 55f);
            cam.Follow = aim.transform;
            var tpf = cam.gameObject.AddComponent<CinemachineThirdPersonFollow>();
            tpf.Damping = new Vector3(0.1f, 0.3f, 0.3f);
            tpf.ShoulderOffset = new Vector3(0.6f, 0.1f, 0f);
            tpf.VerticalArmLength = 0.3f;
            tpf.CameraSide = 1f;
            tpf.CameraDistance = 3.5f;
            var o = tpf.AvoidObstacles;
            o.Enabled = true;
            o.CollisionFilter = 1; // warstwa Default
            o.IgnoreTag = "Player";
            o.CameraRadius = 0.25f;
            o.DampingIntoCollision = 0f;
            o.DampingFromCollision = 0.4f;
            tpf.AvoidObstacles = o;

            var demo = Manager<CMThirdPersonAim>();
            demo.body = player.transform;
            demo.aimTarget = aim.transform;
            demo.thirdPerson = tpf;
        }

        // =====================================================================
        // 14 – FIRST PERSON (HARD LOCK + PAN TILT)
        // =====================================================================
        static void S14_CmFirstPerson()
        {
            Begin("14 · Cinemachine – kamera z pierwszej osoby (FPP)",
                "Kamera w miejscu oczu postaci:\n" +
                "• <b>Position Control: Hard Lock To Target</b> – pozycja kamery = pozycja obiektu „Glowa” (dziecko postaci),\n" +
                "• <b>Rotation Control: Pan Tilt</b> – obrót z dwóch osi wejścia: Pan (w poziomie, −180..180°) i Tilt " +
                "(w pionie, −70..70°).\n" +
                "Skrypt zmienia osie Pan/Tilt myszą i obraca ciało postaci zgodnie z osią Pan, więc W idzie tam, gdzie patrzysz.\n\n" +
                "<b>Noise: Basic Multi Channel Perlin</b> z profilem „Handheld_normal_mild” – kamera lekko kołysze się, " +
                "gdy postać idzie (Amplitude Gain zależy od prędkości). LPM – kołysanie on/off.\n\n" +
                "Spacja – <b>Lens.FieldOfView</b> 60° ↔ 90° (szeroki kąt typowy dla FPS; Brain płynnie przenosi obiektyw na kamerę Unity).\n\n" +
                "Ciało postaci ma tryb cieni „Shadows Only” – nie zasłania widoku, ale rzuca cień.",
                "<b>Spacja</b> – FOV 60° ↔ 90°   <b>LPM</b> – kołysanie kamery",
                "<b>WASD</b> – ruch   <b>Shift</b> – bieg   <b>PPM + mysz</b> – rozglądanie", cinemachine: true);
            Floor();
            var player = Character("Gracz", new Vector3(0, 0, -6), M_Red, controller: true);
            player.tag = "Player";
            foreach (var r in player.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            var mover = player.AddComponent<PlayerMover>();
            mover.relativeTo = PlayerMover.Relative.Self;
            mover.speed = 3.5f;
            mover.sprintSpeed = 7f;
            var head = new GameObject("Glowa");
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0, 0.7f, 0);

            var g = Group("Pokoj");
            Box("Sciana_N", new Vector3(0, 2, 12), new Vector3(24, 4, 0.5f), M_Wall, g);
            Box("Sciana_S", new Vector3(0, 2, -12), new Vector3(24, 4, 0.5f), M_Wall, g);
            Box("Sciana_W", new Vector3(-12, 2, 0), new Vector3(0.5f, 4, 24), M_Wall, g);
            Box("Sciana_E", new Vector3(12, 2, 0), new Vector3(0.5f, 4, 24), M_Wall, g);
            for (int i = 0; i < 6; i++)
            {
                var b = Box($"Skrzynia_{i + 1}", new Vector3(-8 + i * 3.2f, 0.6f + 0.4f * (i % 3), 4 + (i % 2) * 3), Vector3.one * (1.2f + 0.8f * (i % 3)), Palette[i], g);
                Label(b, $"skrzynia {i + 1}", 1.4f);
            }
            Monument("Pomnik", new Vector3(0, 0, 0));

            var cam = CmCamera("CM_FPP", head.transform.position, head.transform.position + Vector3.forward, 10, 60f);
            var lens = cam.Lens;
            lens.NearClipPlane = 0.05f;
            cam.Lens = lens;
            cam.Follow = head.transform;
            var hl = cam.gameObject.AddComponent<CinemachineHardLockToTarget>();
            hl.Damping = 0f;
            var pt = cam.gameObject.AddComponent<CinemachinePanTilt>();
            pt.ReferenceFrame = CinemachinePanTilt.ReferenceFrames.World;
            pt.PanAxis = new InputAxis { Value = 0, Center = 0, Range = new Vector2(-180, 180), Wrap = true };
            pt.TiltAxis = new InputAxis { Value = 0, Center = 0, Range = new Vector2(-70, 70), Wrap = false };
            var noise = cam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            noise.NoiseProfile = LoadNoise("Handheld_normal_mild");
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 1f;

            var demo = Manager<CMFirstPersonLook>();
            demo.cmCamera = cam;
            demo.panTilt = pt;
            demo.bob = noise;
            demo.body = player.transform;
            demo.mover = mover;
        }

        // =====================================================================
        // 15 – WIDOK Z GÓRY: TARGET GROUP + GROUP FRAMING
        // =====================================================================
        static void S15_CmGroup()
        {
            Begin("15 · Cinemachine – widok z góry na grupę (Target Group)",
                "Kamera w stylu gier strategicznych / imprezowych: ma w kadrze wszystkie jednostki naraz.\n\n" +
                "• <b>Cinemachine Target Group</b> – obiekt, którego pozycja i rozmiar wynikają z listy celów " +
                "(Targets: obiekt, waga, promień),\n" +
                "• <b>Position Control: Position Composer</b> – kamera przesuwa się równolegle do płaszczyzny ekranu " +
                "(nie obraca się), stała odległość Camera Distance,\n" +
                "• rozszerzenie <b>Group Framing</b> – dobiera odległość (dolly) i/lub FOV (zoom), aby grupa zajmowała " +
                "Framing Size = 70% kadru.\n\n" +
                "Jednostki rozjeżdżają się i zbliżają – obserwuj, jak kamera oddala się i przybliża.\n" +
                "Spacja – cel: cała grupa ↔ jedna jednostka.\n" +
                "LPM – Size Adjustment: Zoom Only (tylko FOV) / Dolly Only (tylko odległość) / Dolly Then Zoom.",
                "<b>Spacja</b> – grupa ↔ jedna jednostka   <b>LPM</b> – Size Adjustment", "", cinemachine: true);
            Floor();
            PillarRing("Slupy", Vector3.zero, 34f, 20, 3f, M_Teal);
            Scatter("Klocki", 24, 32f, 51);

            var group = new GameObject("Grupa_celow").AddComponent<CinemachineTargetGroup>();
            group.PositionMode = CinemachineTargetGroup.PositionModes.GroupCenter;
            group.RotationMode = CinemachineTargetGroup.RotationModes.Manual;
            var mats = new[] { M_Red, M_Green, M_Blue, M_Yellow, M_Purple };
            Transform first = null;
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                var u = Character($"Jednostka_{i + 1}", Vector3.zero, mats[i]);
                Vector3 c = new Vector3(Mathf.Cos(a) * 7f, 0, Mathf.Sin(a) * 7f);
                if (i == 4)
                    Mover(u, AutoMover.PathShape.Line, Vector3.zero, new Vector2(24, 0), 0.25f, 0f, 0f);
                else
                    Mover(u, AutoMover.PathShape.Circle, c, new Vector2(3 + 2 * i, 3 + 2 * i), 0.35f + 0.08f * i, 0.3f, i);
                Label(u, u.name, 1.4f, 13);
                group.AddMember(u.transform, 1f, 1.2f);
                if (first == null) first = u.transform;
            }

            var cam = CmCamera("CM_Gora", new Vector3(0, 22, -14), Vector3.zero, 10, 40f);
            cam.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            cam.Follow = group.transform;
            var pc = cam.gameObject.AddComponent<CinemachinePositionComposer>();
            pc.CameraDistance = 26f;
            pc.Damping = new Vector3(1f, 1f, 1f);
            pc.Composition = ScreenComposerSettings.Default;
            var fr = cam.gameObject.AddComponent<CinemachineGroupFraming>();
            fr.FramingMode = CinemachineGroupFraming.FramingModes.HorizontalAndVertical;
            fr.FramingSize = 0.7f;
            fr.Damping = 1.5f;
            fr.SizeAdjustment = CinemachineGroupFraming.SizeAdjustmentModes.DollyThenZoom;
            fr.FovRange = new Vector2(20f, 60f);
            fr.DollyRange = new Vector2(-20f, 30f);

            var demo = Manager<CMGroupDemo>();
            demo.cmCamera = cam;
            demo.group = group;
            demo.framing = fr;
            demo.singleUnit = first;
        }

        // =====================================================================
        // 16 – KAMERA 2D + CONFINER 2D
        // =====================================================================
        static void S16_Cm2D()
        {
            var main = Begin("16 · Cinemachine – kamera 2D i Confiner 2D",
                "Gra 2D: kamera Unity ma <b>Projection = Orthographic</b>, a CinemachineCamera dziedziczy ten tryb; " +
                "wielkość kadru ustala <b>Lens.OrthographicSize</b> (połowa wysokości widoku w jednostkach).\n\n" +
                "• <b>Position Composer</b> – kamera przesuwa się w płaszczyźnie XY za graczem; <b>Dead Zone</b> " +
                "(środkowy prostokąt kadru) – mały ruch gracza nie przesuwa kamery, <b>Damping</b> – wygładzenie.\n" +
                "• <b>Confiner 2D</b> – kamera nie pokaże obszaru poza kształtem <b>PolygonCollider2D</b> " +
                "(niebieska linia – granice poziomu, z „wieżą” na górze). Kształt jest przeliczany (bake) " +
                "z uwzględnieniem rozmiaru kadru.\n\n" +
                "WASD – ruch czerwonego gracza (W/S – w górę i w dół). Podejdź do krawędzi poziomu.\n" +
                "Spacja – Confiner włączony / wyłączony (bez niego widać pustkę za granicą).\n" +
                "LPM – Orthographic Size 5 ↔ 8 (Confiner musi przeliczyć kształt: InvalidateLensCache).",
                "<b>Spacja</b> – Confiner on/off   <b>LPM</b> – Orthographic Size 5 ↔ 8",
                "<b>WASD</b> – ruch gracza   <b>Shift</b> – szybciej", cinemachine: true);
            main.orthographic = true;
            main.orthographicSize = 5f;
            main.clearFlags = CameraClearFlags.SolidColor;
            main.backgroundColor = new Color(0.55f, 0.75f, 0.95f);

            var lvl = Group("Poziom");
            Box("Ziemia", new Vector3(0, -6f, 0), new Vector3(52, 1, 4), M_Green, lvl);
            float[,] plats = { { -18, -2, 6 }, { -9, 1, 5 }, { 0, -1, 6 }, { 8, 3, 5 }, { 3, 7, 4 }, { 9, 11, 4 }, { -3, 13, 4 }, { 18, 0, 6 } };
            for (int i = 0; i < plats.GetLength(0); i++)
                Box($"Platforma_{i + 1}", new Vector3(plats[i, 0], plats[i, 1], 0), new Vector3(plats[i, 2], 0.6f, 2), M_Wood(), lvl);
            for (int i = 0; i < 14; i++)
                Box($"Tlo_{i + 1}", new Vector3(-26 + i * 4f, -3 + (i % 4) * 2.5f, 6f), new Vector3(3f, 6 + (i % 3) * 3f, 1f), M_Gray, lvl);

            var player = Box("Gracz", new Vector3(0, -5f, 0), Vector3.one, M_Red);
            Label(player, "gracz", 0.9f);

            var bounds = new GameObject("Granice_poziomu");
            var poly = bounds.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            var pts = new[]
            {
                // dolna część 18.5 × 50, "wieża" 30 × 10 – kadr przy Orthographic Size 8 (ok. 28.4 × 16) wciąż się mieści
                new Vector2(-25, -6.5f), new Vector2(25, -6.5f), new Vector2(25, 12), new Vector2(18, 12),
                new Vector2(18, 22), new Vector2(-12, 22), new Vector2(-12, 12), new Vector2(-25, 12),
            };
            poly.pathCount = 1;
            poly.SetPath(0, pts);
            var linePts = new Vector3[pts.Length];
            for (int i = 0; i < pts.Length; i++) linePts[i] = new Vector3(pts[i].x, pts[i].y, 1.5f);
            Line("Granice_linia", linePts, new Color(0.15f, 0.35f, 0.95f), 0.15f, true, bounds.transform);

            var cam = CmCamera("CM_2D", new Vector3(0, -2, -10), new Vector3(0, -2, 0), 10, 50f);
            cam.transform.rotation = Quaternion.identity;
            var lens = cam.Lens;
            lens.OrthographicSize = 5f;
            lens.NearClipPlane = 0.1f;
            lens.FarClipPlane = 100f;
            cam.Lens = lens;
            cam.Follow = player.transform;
            var pc = cam.gameObject.AddComponent<CinemachinePositionComposer>();
            pc.CameraDistance = 10f;
            pc.Damping = new Vector3(0.6f, 0.6f, 0f);
            var comp = ScreenComposerSettings.Default;
            comp.DeadZone.Enabled = true;
            comp.DeadZone.Size = new Vector2(0.15f, 0.2f);
            pc.Composition = comp;
            var conf = cam.gameObject.AddComponent<CinemachineConfiner2D>();
            conf.BoundingShape2D = poly;
            conf.Damping = 0.3f;
            conf.SlowingDistance = 1f;

            var demo = Manager<CMConfinerDemo>();
            demo.cmCamera = cam;
            demo.confiner = conf;
            demo.composer = pc;
            demo.player = player.transform;
            demo.playerArea = new Rect(-23f, -5f, 46f, 25f);
        }

        // =====================================================================
        // 17 – DOLLY / SPLINE
        // =====================================================================
        static void S17_CmDolly()
        {
            Begin("17 · Cinemachine – kamera na torze (Spline Dolly)",
                "Kamera filmowa „na wózku”: jej pozycja jest ograniczona do krzywej <b>Spline</b> (pakiet Splines).\n\n" +
                "• Wewnętrzny tor: wagonik z komponentem <b>Cinemachine Spline Cart</b> jedzie ze stałą prędkością 4 m/s.\n" +
                "• Zewnętrzny, falujący tor: <b>CinemachineCamera</b> z <b>Position Control: Spline Dolly</b> " +
                "i <b>Rotation Composer</b>.\n\n" +
                "Spacja – <b>Automatic Dolly</b> kamery:\n" +
                "• <b>Fixed Speed</b> – kamera sama jedzie po torze (4 m/s) i patrzy na pomnik – ujęcie typu „przelot”,\n" +
                "• <b>Nearest Point To Target</b> – kamera ustawia się w punkcie toru najbliższym wagonikowi i śledzi go.\n\n" +
                "LPM – <b>Camera Rotation</b>: Default (obrót z Rotation Composer) ↔ Spline (kamera patrzy wzdłuż toru).\n" +
                "Pozycja na torze: Camera Position w jednostkach Distance (metry od początku krzywej).",
                "<b>Spacja</b> – Fixed Speed ↔ Nearest Point To Target   <b>LPM</b> – Camera Rotation", "", cinemachine: true);
            Floor();
            var monument = Monument("Pomnik", Vector3.zero);
            Scatter("Klocki", 14, 34f, 61, null, 23f); // poza torami (promienie 9 i 18 m)

            var inner = MakeSpline("Tor_wagonika", 9f, 8, a => 0.3f);
            var outer = MakeSpline("Tor_kamery", 18f, 10, a => 3.5f + 2.5f * Mathf.Sin(2f * a));
            SplineLine(inner, new Color(0.25f, 0.2f, 0.15f), 0.35f);
            SplineLine(outer, new Color(1f, 0.85f, 0.3f), 0.12f);

            var cart = Box("Wagonik", new Vector3(9, 0.7f, 0), new Vector3(1.4f, 0.8f, 2.2f), M_Red);
            var cartNose = Box("Wagonik_przod", new Vector3(9, 1.2f, 1f), new Vector3(1f, 0.3f, 0.3f), M_Dark, cart.transform);
            NoCollider(cartNose);
            Label(cart, "wagonik (Spline Cart)", 1f);
            var sc = cart.AddComponent<CinemachineSplineCart>();
            sc.Spline = inner;
            sc.UpdateMethod = CinemachineSplineCart.UpdateMethods.Update;
            sc.SplineSettings.Units = PathIndexUnit.Distance;
            sc.SplineSettings.Position = 0f;
            sc.AutomaticDolly.Enabled = true;
            sc.AutomaticDolly.Method = new SplineAutoDolly.FixedSpeed { Speed = 4f };

            var cam = CmCamera("CM_Dolly", new Vector3(18, 4, 0), Vector3.zero, 10, 45f);
            var dolly = cam.gameObject.AddComponent<CinemachineSplineDolly>();
            dolly.Spline = outer;
            dolly.SplineSettings.Units = PathIndexUnit.Distance;
            dolly.CameraPosition = 0f;
            dolly.CameraRotation = CinemachineSplineDolly.RotationMode.Default;
            dolly.Damping = new CinemachineSplineDolly.DampingSettings { Enabled = true, Position = new Vector3(0.5f, 0.5f, 0.5f), Angular = 0.5f };
            dolly.AutomaticDolly.Enabled = true;
            dolly.AutomaticDolly.Method = new SplineAutoDolly.FixedSpeed { Speed = 4f };
            cam.LookAt = monument.transform;
            var dollyComposer = AimComposer(cam, new Vector3(0, 1.5f, 0), new Vector2(0.3f, 0.3f));

            var demo = Manager<CMDollyDemo>();
            demo.composer = dollyComposer;
            demo.cmCamera = cam;
            demo.dolly = dolly;
            demo.monument = monument.transform;
            demo.cart = cart.transform;
            demo.tourSpeed = 4f;
        }

        /// <summary>Zamknięta krzywa Spline: count węzłów na okręgu o promieniu radius, wysokość y(a).</summary>
        static SplineContainer MakeSpline(string name, float radius, int count, System.Func<float, float> height)
        {
            var go = new GameObject(name);
            var container = go.AddComponent<SplineContainer>();
            if (container.Spline == null)
                container.Spline = new Spline();
            var spline = container.Spline;
            spline.Clear();
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                spline.Add(new float3(Mathf.Cos(a) * radius, height(a), Mathf.Sin(a) * radius), TangentMode.AutoSmooth);
            }
            spline.Closed = true;
            return container;
        }

        static void SplineLine(SplineContainer c, Color color, float width)
        {
            const int n = 128;
            var pts = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float3 p = c.EvaluatePosition((float)i / n);
                pts[i] = new Vector3(p.x, p.y, p.z);
            }
            Line(c.name + "_linia", pts, color, width, true, c.transform);
        }

        // =====================================================================
        // 18 – SZUM I IMPULS
        // =====================================================================
        static void S18_CmNoise()
        {
            Begin("18 · Cinemachine – szum (Noise) i wstrząsy (Impulse)",
                "Dwa sposoby na drgania kamery:\n\n" +
                "<b>1. Noise – Basic Multi Channel Perlin</b>: ciągły szum (kamera z ręki, wiatr, silnik). " +
                "Profil <b>Noise Settings</b> opisuje częstotliwości i amplitudy drgań pozycji i obrotu; " +
                "Amplitude Gain i Frequency Gain skalują profil.\n" +
                "Spacja – profil: brak → Handheld_normal_mild → Handheld_normal_strong → 6D Shake " +
                "(gotowe profile z pakietu Cinemachine).\n\n" +
                "<b>2. Impulse</b>: jednorazowy wstrząs (wybuch, uderzenie, lądowanie). " +
                "<b>Cinemachine Impulse Source</b> na beczce wysyła sygnał, a rozszerzenie <b>Cinemachine Impulse Listener</b> " +
                "na kamerze go odbiera. Impulse Shape = Explosion, Impulse Type = Dissipating – siła maleje z odległością od źródła.\n" +
                "LPM – wybuch beczki (GenerateImpulseWithForce).",
                "<b>Spacja</b> – profil szumu   <b>LPM</b> – wybuch (Impulse)", "", cinemachine: true);
            Floor();
            Scatter("Klocki", 16, 20f, 71);
            PillarRing("Slupy", new Vector3(0, 0, 6), 12f, 10, 3f, M_Teal);
            var barrel = Prim(PrimitiveType.Cylinder, "Beczka", new Vector3(0, 0.7f, 6), new Vector3(1.2f, 0.7f, 1.2f), M_Red);
            Label(barrel, "beczka (Impulse Source)", 1.2f);
            var flash = Ball("Blysk", barrel.transform.position + Vector3.up * 0.5f, 1f, UnlitMat("Blysk", new Color(1f, 0.7f, 0.2f)), barrel.transform);
            NoCollider(flash);
            var fr = flash.GetComponent<Renderer>();
            fr.enabled = false;
            fr.shadowCastingMode = ShadowCastingMode.Off;

            var src = barrel.AddComponent<CinemachineImpulseSource>();
            src.ImpulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = 1,
                ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Dissipating,
                ImpulseDuration = 0.8f,
                DissipationDistance = 60f,
                DissipationRate = 0.25f,
            };
            src.DefaultVelocity = new Vector3(0f, -1f, 0f);

            var cam = CmCamera("CM_Kamera", new Vector3(0, 3, -6), new Vector3(0, 1, 6), 10, 50f);
            cam.LookAt = barrel.transform;
            AimComposer(cam, new Vector3(0, 0.5f, 0), Vector2.zero);
            var perlin = cam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            perlin.NoiseProfile = null;
            perlin.AmplitudeGain = 1f;
            perlin.FrequencyGain = 1f;
            var listener = cam.gameObject.AddComponent<CinemachineImpulseListener>();
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = 1f;
            listener.Use2DDistance = false;
            listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.Additive;
            listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction { AmplitudeGain = 1f, FrequencyGain = 1f, Duration = 1f };

            var demo = Manager<CMNoiseImpulseDemo>();
            demo.perlin = perlin;
            demo.profiles = new[] { LoadNoise("Handheld_normal_mild"), LoadNoise("Handheld_normal_strong"), LoadNoise("6D Shake") };
            demo.profileNames = new[] { "Handheld_normal_mild", "Handheld_normal_strong", "6D Shake" };
            demo.impulseSource = src;
            demo.flash = fr;
            demo.impulseForce = 0.8f;
        }

        // =====================================================================
        // 19 – CLEAR SHOT
        // =====================================================================
        static void S19_CmClearShot()
        {
            Begin("19 · Cinemachine – Clear Shot: automatyczny wybór ujęcia",
                "Postać chodzi wokół muru w kształcie krzyża. Trzy stałe kamery stoją w narożnikach – z każdej postać co jakiś czas " +
                "znika za murem.\n\n" +
                "<b>Cinemachine Clear Shot</b> to „menedżer” kamer: jego dzieci to zwykłe CinemachineCamera. Każda ma rozszerzenie " +
                "<b>Deoccluder</b> z włączonym <b>Shot Quality Evaluation</b> (ocena ujęcia: czy cel jest zasłonięty, czy jest w dobrej " +
                "odległości). Clear Shot włącza dziecko z najlepszą oceną – gdy mur zasłoni postać, przełącza na inną kamerę.\n\n" +
                "Spacja – Clear Shot ↔ tylko kamera stała nr 1 (bez automatycznego wyboru).\n" +
                "LPM – <b>Activate After</b> 0 s ↔ 1 s: jak długo nowe ujęcie musi być lepsze, zanim nastąpi przełączenie " +
                "(<b>Min Duration</b> = 1 s – minimalny czas trwania ujęcia).\n\n" +
                "Ten sam Deoccluder z włączonym Avoid Obstacles potrafi też przesuwać kamerę, aby ominąć przeszkodę.",
                "<b>Spacja</b> – Clear Shot ↔ jedna kamera   <b>LPM</b> – Activate After 0 ↔ 1 s", "", cinemachine: true);
            Floor();
            var walls = Group("Mury");
            Box("Mur_X", new Vector3(0, 2.5f, 0), new Vector3(16, 5, 1), M_Wall, walls);
            Box("Mur_Z", new Vector3(0, 2.5f, 0), new Vector3(1, 5, 16), M_Wall, walls);
            Box("Mur_SW", new Vector3(-10, 2f, -6), new Vector3(1, 4, 5), M_Stone, walls);
            Box("Mur_NE", new Vector3(10, 2f, 6), new Vector3(1, 4, 5), M_Stone, walls);
            Scatter("Klocki", 8, 30f, 81);

            var target = Character("Postac", new Vector3(-11, 0, -11), M_Red);
            target.tag = "Player";
            Mover(target, AutoMover.PathShape.Square, Vector3.zero, new Vector2(11.5f, 11.5f), 0.35f);
            Label(target, "postać", 1.4f);

            var csGo = new GameObject("CM_ClearShot");
            var cs = csGo.AddComponent<CinemachineClearShot>();
            cs.Priority = 20;
            cs.ActivateAfter = 0f;
            cs.MinDuration = 1f;
            cs.RandomizeChoice = false;
            cs.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.6f);

            Vector3[] positions = { new Vector3(-17, 4, -17), new Vector3(17, 5, -17), new Vector3(17, 4, 17) };
            var children = new List<CinemachineCamera>();
            for (int i = 0; i < positions.Length; i++)
            {
                var c = CmCamera($"CM_Stala_{i + 1}", positions[i], Vector3.up, 10, 45f, csGo.transform);
                c.LookAt = target.transform;
                AimComposer(c, new Vector3(0, 0.5f, 0), new Vector2(0.3f, 0.3f));
                var d = c.gameObject.AddComponent<CinemachineDeoccluder>();
                d.CollideAgainst = 1;
                d.IgnoreTag = "Player";
                d.TransparentLayers = 0;
                d.MinimumDistanceFromTarget = 0.3f;
                d.AvoidObstacles = new CinemachineDeoccluder.ObstacleAvoidance
                {
                    Enabled = false,
                    CameraRadius = 0.2f,
                    Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward,
                    MaximumEffort = 4,
                    Damping = 0.4f,
                    DampingWhenOccluded = 0.2f,
                };
                d.ShotQualityEvaluation = new CinemachineDeoccluder.QualityEvaluation
                {
                    Enabled = true, NearLimit = 5f, FarLimit = 45f, OptimalDistance = 18f, MaxQualityBoost = 0.2f,
                };
                var post = Prim(PrimitiveType.Cylinder, $"Statyw_{i + 1}", new Vector3(positions[i].x, positions[i].y / 2f - 0.3f, positions[i].z),
                    new Vector3(0.2f, positions[i].y / 2f - 0.3f, 0.2f), M_Dark);
                NoCollider(post);
                Label(post, $"kamera {i + 1}", positions[i].y / 2f + 0.8f, 13);
                children.Add(c);
            }

            var fixedCam = CmCamera("CM_Stala_1_bez_ClearShot", positions[0], Vector3.up, 10, 45f);
            fixedCam.LookAt = target.transform;
            AimComposer(fixedCam, new Vector3(0, 0.5f, 0), new Vector2(0.3f, 0.3f));

            var demo = Manager<CMClearShotDemo>();
            demo.clearShot = cs;
            demo.fixedCamera = fixedCam;
            demo.children = children.ToArray();
        }
    }
}
