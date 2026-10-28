using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Kamera.EditorTools
{
    public static partial class SceneBuilder
    {
        // =====================================================================
        // 01 – PERSPEKTYWA / ORTOGRAFIA
        // =====================================================================
        static void S01_Projection()
        {
            var cam = Begin("01 · Rzutowanie perspektywiczne i ortogonalne",
                "Kamera rzutuje scenę 3D na płaski ekran.\n\n" +
                "<b>Perspektywa</b> (Projection = Perspective): obiekty dalsze są mniejsze, równoległe tory i rzędy słupów " +
                "zbiegają się w punkcie zbiegu. Parametr: <b>Field of View</b> – kąt widzenia w pionie.\n\n" +
                "<b>Ortografia</b> (Projection = Orthographic): rzut równoległy – sześciany w odległości 10, 20 i 40 m " +
                "mają ten sam rozmiar na ekranie, tory pozostają równoległe. Parametr: <b>Size</b> – połowa wysokości " +
                "widoku w jednostkach świata. Ruch kamery do przodu (kółko) w ortografii nie zmienia wielkości obiektów!\n\n" +
                "W panelu stanu widać <b>macierz projekcji</b> (Camera.projectionMatrix). W perspektywie wiersz 4 to (0, 0, −1, 0) – " +
                "stąd dzielenie przez głębokość (w). W ortografii wiersz 4 to (0, 0, 0, 1).\n\n" +
                "LPM – rzut ukośny (kabinetowy – głębokość skrócona o połowę): macierz ortogonalna pomnożona przez macierz ścinania. " +
                "Unity nie ma takiego trybu w Inspektorze, ale można ustawić własną macierz w kodzie.",
                "<b>Spacja</b> – perspektywa ↔ ortografia   <b>LPM</b> – rzut ukośny (własna macierz)", "", freeCamera: true);
            Floor();
            Cam(new Vector3(0, 3.2f, -8f), new Vector3(0, 1f, 20f));
            cam.gameObject.AddComponent<ProjectionToggle>().focusDistance = 18f;

            var g = Group("Korytarz");
            for (int i = 0; i < 12; i++)
            {
                float z = i * 6f;
                Box($"Slup_L_{i + 1}", new Vector3(-5, 2, z), new Vector3(0.8f, 4, 0.8f), i % 2 == 0 ? M_Blue : M_Gray, g);
                Box($"Slup_P_{i + 1}", new Vector3(5, 2, z), new Vector3(0.8f, 4, 0.8f), i % 2 == 0 ? M_Blue : M_Gray, g);
                Box($"Podklad_{i + 1}", new Vector3(0, 0.05f, z), new Vector3(3.4f, 0.1f, 0.4f), M_Wood(), g);
            }
            Box("Szyna_L", new Vector3(-1.2f, 0.15f, 33), new Vector3(0.15f, 0.2f, 72), M_Dark, g);
            Box("Szyna_P", new Vector3(1.2f, 0.15f, 33), new Vector3(0.15f, 0.2f, 72), M_Dark, g);

            float[] zs = { 10f, 20f, 40f };
            for (int i = 0; i < zs.Length; i++)
            {
                var c = Box($"Szescian_z{zs[i]}", new Vector3(i == 1 ? -2.6f : 2.6f, 1f, zs[i]), Vector3.one * 2f, Palette[i], g);
                Label(c, $"sześcian 2 m, z = {zs[i]} m", 1.6f);
            }
        }

        static Material M_Wood() => Mat("Drewno", new Color(0.6f, 0.42f, 0.25f));

        // =====================================================================
        // 02 – FOV / DOLLY ZOOM
        // =====================================================================
        static void S02_Fov()
        {
            var cam = Begin("02 · Field of View i efekt dolly zoom",
                "<b>Field of View (FOV)</b> – kąt widzenia kamery. W Unity domyślnie mierzony w pionie (FOV Axis = Vertical); " +
                "kąt poziomy wynika z proporcji ekranu (aspect).\n" +
                "Widoczna wysokość w odległości d: h = 2 · d · tan(FOV / 2).\n\n" +
                "<b>Kółko myszy</b> – zmiana FOV (zoom optyczny: kamera stoi w miejscu, zmienia się tylko kąt).\n\n" +
                "<b>Spacja – dolly zoom</b> (efekt „Vertigo”): kamera odjeżdża od postaci, a FOV maleje tak, aby postać " +
                "zajmowała tę samą część kadru: FOV = 2 · atan(H / (2 · d)). Postać ma stały rozmiar, " +
                "a tło „rośnie” – zmienia się perspektywa.\n\n" +
                "<b>LPM – Physical Camera</b>: zamiast FOV ustawiamy ogniskową (Focal Length, mm) i rozmiar matrycy (Sensor Size, " +
                "tu 36 × 24 mm). Zależność: f = (wysokość matrycy / 2) / tan(FOV / 2).",
                "<b>Spacja</b> – dolly zoom start/stop   <b>LPM</b> – Physical Camera on/off",
                "<b>kółko</b> – zmiana FOV (zoom)");
            Floor();
            // start w odległości minDistance = 5 m z FOV pasującym do dolly zoom: 2·atan(4 / (2·5)) ≈ 43.6°
            Cam(new Vector3(0, 2f, -5f), new Vector3(0, 2f, 0));
            cam.fieldOfView = 43.6f;

            var subject = Character("Postac", Vector3.zero, M_Red);
            Label(subject, "postać (cel)", 1.4f);
            var dz = cam.gameObject.AddComponent<DollyZoom>();
            dz.subject = subject.transform;
            dz.frameHeight = 4f;
            dz.minDistance = 5f;
            dz.maxDistance = 32f;
            dz.travelTime = 4f;

            var g = Group("Tlo");
            for (int i = 0; i < 8; i++)
            {
                float x = -14f + i * 4f;
                Box($"Budynek_{i + 1}", new Vector3(x, 3 + (i % 3), 18f), new Vector3(3f, 6 + 2 * (i % 3), 3f), Palette[i % 7], g);
                Box($"Slup_{i + 1}", new Vector3(x + 2f, 1.5f, 8f + (i % 2) * 3f), new Vector3(0.6f, 3, 0.6f), M_Gray, g);
            }
            Box("Mur", new Vector3(0, 5, 34f), new Vector3(60, 10, 1), M_Wall, g);
        }

        // =====================================================================
        // 03 – CLIPPING PLANES / FRUSTUM
        // =====================================================================
        static void S03_Clipping()
        {
            var cam = Begin("03 · Płaszczyzny obcinania i ostrosłup widzenia",
                "Kamera rysuje tylko to, co leży w <b>ostrosłupie widzenia</b> (frustum) – między płaszczyzną bliską " +
                "(<b>Near</b>) i daleką (<b>Far</b>), w granicach kąta widzenia.\n\n" +
                "Spacja przełącza zestawy Near/Far – obserwuj, jak znikają sześciany położone bliżej niż Near " +
                "albo dalej niż Far. Etykieta mówi, czy obiekt leży w ostrosłupie (test GeometryUtility.TestPlanesAABB – " +
                "tak samo silnik odrzuca obiekty niewidoczne: <b>frustum culling</b>).\n\n" +
                "Bufor głębokości (z-buffer) ma skończoną precyzję, a rozkład tej precyzji zależy od Near i Far – zbyt małe Near " +
                "i bardzo duże Far pogarszają rozróżnianie głębokości. Po prawej: dwie ściany w tej samej płaszczyźnie – " +
                "<b>z-fighting</b> (migotanie), którego nie usunie żadne ustawienie Near/Far.\n\n" +
                "LPM – rysowanie krawędzi ostrosłupa (Debug.DrawLine, widoczne w oknie Scene lub z włączonymi Gizmos).",
                "<b>Spacja</b> – kolejny zestaw Near/Far   <b>LPM</b> – rysuj ostrosłup", "", freeCamera: true);
            Floor();
            Cam(new Vector3(0, 2.2f, -4f), new Vector3(0, 1.2f, 10f));
            var clip = cam.gameObject.AddComponent<ClipPlanesDemo>();

            var g = Group("Szesciany");
            float[] dist = { 2f, 4f, 8f, 15f, 22f, 30f, 45f, 70f };
            var tracked = new List<Renderer>();
            for (int i = 0; i < dist.Length; i++)
            {
                float z = -4f + dist[i];
                float x = (i % 2 == 0 ? -1.4f : 1.4f) * (1f + i * 0.35f);
                var c = Box($"Szescian_{dist[i]}m", new Vector3(x, 0.75f, z), Vector3.one * 1.5f, Palette[i % 7], g);
                Label(c, $"{dist[i]} m od kamery", 1.3f);
                tracked.Add(c.GetComponent<Renderer>());
            }
            clip.tracked = tracked.ToArray();

            var zf = Group("Z_fighting");
            Box("Sciana_A", new Vector3(9f, 2f, 12f), new Vector3(4f, 3f, 0.2f), M_Red, zf);
            var b = Box("Sciana_B", new Vector3(10.2f, 2.4f, 12f), new Vector3(4f, 3f, 0.2f), M_Green, zf);
            Label(b, "z-fighting: dwie ściany w tej samej płaszczyźnie", 2f);
        }

        // =====================================================================
        // 04 – VIEWPORT RECT / PODZIAŁ EKRANU
        // =====================================================================
        static void S04_Viewport()
        {
            var cam = Begin("04 · Viewport Rect – kilka kamer na jednym ekranie",
                "<b>Viewport Rect</b> (X, Y, W, H) określa, jaką część ekranu zajmuje obraz kamery. Wartości są " +
                "znormalizowane: (0, 0) to lewy dolny róg, (1, 1) – prawy górny.\n\n" +
                "Spacja przełącza układy:\n" +
                "1. jedna kamera na cały ekran (0, 0, 1, 1),\n" +
                "2. podział pionowy dla dwóch graczy: (0, 0, 0.5, 1) i (0.5, 0, 0.5, 1),\n" +
                "3. podział poziomy: (0, 0.5, 1, 0.5) i (0, 0, 1, 0.5),\n" +
                "4. cztery widoki jak w programie do modelowania 3D: góra, przód, bok (rzuty ortogonalne) i perspektywa.\n\n" +
                "Każda kamera to osobny obiekt z komponentem Camera (w URP: Render Type = Base). Kolejność rysowania " +
                "nakładających się kamer ustala <b>Priority</b> (Camera.depth). Tylko jedna kamera ma AudioListener.",
                "<b>Spacja</b> – kolejny układ ekranu", "");
            Floor();

            var a = Character("Gracz_A", new Vector3(-6, 0, 0), M_Red);
            Mover(a, AutoMover.PathShape.Figure8, new Vector3(-4, 0, 0), new Vector2(9, 7), 0.45f, 0.3f);
            Label(a, "gracz A", 1.4f);
            var bch = Character("Gracz_B", new Vector3(6, 0, 0), M_Blue);
            Mover(bch, AutoMover.PathShape.Circle, new Vector3(5, 0, 2), new Vector2(10, 8), -0.35f, 0.2f, 1.5f);
            Label(bch, "gracz B", 1.4f);
            PillarRing("Slupy", Vector3.zero, 18f, 12, 4f, M_Teal);
            Scatter("Przeszkody", 14, 16f, 4);

            cam.name = "Kamera_Gracz_A";
            var chaseA = cam.gameObject.AddComponent<ChaseCamera>();
            chaseA.target = a.transform;
            chaseA.offset = new Vector3(0, 4, -8);
            chaseA.offsetInTargetSpace = true;
            Cam(new Vector3(-6, 4, -8), new Vector3(-6, 1, 0));

            var camB = NewCamera("Kamera_Gracz_B", new Vector3(6, 4, -8), new Vector3(6, 1, 0));
            var chaseB = camB.gameObject.AddComponent<ChaseCamera>();
            chaseB.target = bch.transform;
            chaseB.offset = new Vector3(0, 4, -8);
            chaseB.offsetInTargetSpace = true;

            var top = NewCamera("Kamera_Gora", new Vector3(0, 40, 0), new Vector3(0, 0, 0.01f));
            top.orthographic = true;
            top.orthographicSize = 20f;
            var front = NewCamera("Kamera_Przod", new Vector3(0, 3, -40), new Vector3(0, 3, 0));
            front.orthographic = true;
            front.orthographicSize = 10f;
            var side = NewCamera("Kamera_Bok", new Vector3(40, 3, 0), new Vector3(0, 3, 0));
            side.orthographic = true;
            side.orthographicSize = 10f;

            var vl = Manager<ViewportLayouts>();
            vl.playerA = cam;
            vl.playerB = camB;
            vl.top = top;
            vl.front = front;
            vl.side = side;
        }

        static Camera NewCamera(string name, Vector3 pos, Vector3 lookAt)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.LookAt(lookAt);
            var c = go.AddComponent<Camera>();
            c.nearClipPlane = 0.1f;
            c.farClipPlane = 500f;
            c.fieldOfView = 60f;
            return c;
        }

        // =====================================================================
        // 05 – CULLING MASK / MINIMAPA
        // =====================================================================
        static void S05_CullingMask()
        {
            var cam = Begin("05 · Culling Mask, warstwy i minimapa",
                "Każdy obiekt należy do jednej <b>warstwy</b> (Layer). <b>Culling Mask</b> kamery to lista warstw, które " +
                "kamera rysuje.\n\n" +
                "W rogu ekranu działa <b>minimapa</b> – druga kamera: ortogonalna, patrzy z góry, ma mały " +
                "Viewport Rect i wyższy priorytet (rysuje się na wierzchu). Nad postaciami unoszą się kolorowe ikony " +
                "na warstwie <b>„Minimapa”</b>: widzi je tylko kamera minimapy, bo kamera główna ma tę warstwę " +
                "wyłączoną w Culling Mask.\n\n" +
                "Spacja – włącz/wyłącz warstwę „Minimapa” w kamerze głównej (ikony pojawią się w głównym widoku).\n" +
                "LPM – minimapa perspektywiczna ↔ ortogonalna.\n\n" +
                "Inne zastosowania: kamera rysująca tylko interfejs, broń FPP (scena 06), obiekty widoczne tylko w lustrze " +
                "lub na monitorze (scena 07), pomijanie niepotrzebnych obiektów dla wydajności.",
                "<b>Spacja</b> – warstwa Minimapa w kamerze głównej   <b>LPM</b> – minimapa ortho/perspektywa", "");
            Floor();
            int lm = Layer(LayerMinimap);

            var player = Character("Gracz", new Vector3(0, 0, -6), M_Red);
            Mover(player, AutoMover.PathShape.Figure8, Vector3.zero, new Vector2(14, 10), 0.3f, 0.3f);
            Icon(player.transform, M_Red, lm, 2.2f);
            var npcMats = new[] { M_Green, M_Blue, M_Yellow, M_Purple };
            for (int i = 0; i < 4; i++)
            {
                var n = Character($"NPC_{i + 1}", new Vector3(8, 0, 8), npcMats[i]);
                Mover(n, AutoMover.PathShape.Circle, new Vector3(i < 2 ? -12 : 12, 0, i % 2 == 0 ? -10 : 10), new Vector2(5 + i, 5 + i), 0.4f + 0.1f * i, 0f, i);
                Icon(n.transform, npcMats[i], lm, 1.6f);
            }
            PillarRing("Slupy", Vector3.zero, 26f, 16, 5f, M_Teal);
            Scatter("Budynki", 18, 24f, 7);

            cam.name = "Main Camera";
            var chase = cam.gameObject.AddComponent<ChaseCamera>();
            chase.target = player.transform;
            chase.offset = new Vector3(0, 5, -9);
            chase.offsetInTargetSpace = true;
            Cam(new Vector3(0, 5, -15), new Vector3(0, 1, -6));
            cam.cullingMask = ~(1 << lm);
            cam.depth = 0;

            var mini = NewCamera("Kamera_Minimapa", new Vector3(0, 40, -6), new Vector3(0, 0, -5.99f));
            mini.orthographic = true;
            mini.orthographicSize = 22f;
            mini.rect = new Rect(0.73f, 0.09f, 0.25f, 0.4f);
            mini.depth = 1;
            mini.clearFlags = CameraClearFlags.SolidColor;
            mini.backgroundColor = new Color(0.1f, 0.12f, 0.16f);
            mini.cullingMask = (1 << 0) | (1 << lm);
            var mchase = mini.gameObject.AddComponent<ChaseCamera>();
            mchase.target = player.transform;
            mchase.offset = new Vector3(0, 40, 0);
            mchase.lookAtTarget = false;
            mchase.smoothTime = 0f;
            mini.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var demo = Manager<CullingMaskDemo>();
            demo.mainCamera = cam;
            demo.minimapCamera = mini;
            demo.layerName = LayerMinimap;
        }

        static void Icon(Transform parent, Material mat, int layer, float size)
        {
            var icon = Prim(PrimitiveType.Cylinder, parent.name + "_Ikona", parent.position + Vector3.up * 5f, new Vector3(size, 0.05f, size),
                UnlitMat("Ikona_" + mat.name, mat.color), parent);
            NoCollider(icon);
            SetLayer(icon, layer);
            icon.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // =====================================================================
        // 06 – CAMERA STACK (URP)
        // =====================================================================
        static void S06_CameraStack()
        {
            var cam = Begin("06 · Stos kamer URP: Base + Overlay",
                "W URP kamera ma <b>Render Type</b>: <b>Base</b> (rysuje obraz od zera) albo <b>Overlay</b> (dorysowuje się " +
                "na obrazie kamery bazowej). Kamery Overlay dodaje się do listy <b>Stack</b> kamery bazowej.\n\n" +
                "Kamera idzie w stronę ściany i cofa się. Broń z pierwszej osoby jest na warstwie <b>„Bron”</b>.\n" +
                "<b>Tryb A – jedna kamera</b>: broń rysuje kamera bazowa; gdy kamera jest blisko ściany, lufa „wchodzi” w ścianę.\n" +
                "<b>Tryb B – stos kamer</b>: kamera Base pomija warstwę „Bron” (Culling Mask), a broń rysuje kamera Overlay " +
                "po wyczyszczeniu bufora głębokości – broń jest zawsze na wierzchu i może mieć własny FOV i Near.\n\n" +
                "Spacja – tryb A ↔ B. LPM – FOV kamery Overlay 40° ↔ 60°.\n\n" +
                "Inne zastosowania stosu: interfejs 3D nad sceną, podgląd postaci w ekwipunku, efekty nakładane na obraz.",
                "<b>Spacja</b> – jedna kamera ↔ Base + Overlay   <b>LPM</b> – FOV kamery Overlay", "");
            Floor();
            int lw = Layer(LayerWeapon);

            cam.name = "Kamera_Base";
            cam.fieldOfView = 65f;
            cam.transform.position = new Vector3(0, 1.7f, 0);
            cam.transform.rotation = Quaternion.identity;
            var walker = cam.gameObject.AddComponent<PingPongMover>();
            walker.pointA = new Vector3(0, 1.7f, 0);
            walker.pointB = new Vector3(0, 1.7f, 9.15f);
            walker.travelTime = 3f;
            walker.pauseTime = 2f;

            var g = Group("Korytarz");
            Box("Sciana_czolowa", new Vector3(0, 2.5f, 10.25f), new Vector3(8, 5, 0.5f), M_Wall, g);
            Box("Sciana_L", new Vector3(-4, 2.5f, 5), new Vector3(0.5f, 5, 11), M_Wall, g);
            Box("Sciana_P", new Vector3(4, 2.5f, 5), new Vector3(0.5f, 5, 11), M_Wall, g);
            for (int i = 0; i < 4; i++)
                Box($"Skrzynia_{i + 1}", new Vector3(i % 2 == 0 ? -2.8f : 2.8f, 0.5f, 2 + i * 2f), Vector3.one, Palette[i], g);
            var plaque = Box("Tablica", new Vector3(0, 2.2f, 9.95f), new Vector3(3, 1.2f, 0.1f), M_Yellow, g);
            Label(plaque, "ściana", 0.9f);

            // broń – dziecko kamery bazowej
            var weapon = new GameObject("Bron_FPP");
            weapon.transform.SetParent(cam.transform, false);
            weapon.transform.localPosition = new Vector3(0.32f, -0.28f, 0.45f);
            var body = Box("Korpus", Vector3.zero, new Vector3(0.12f, 0.16f, 0.5f), M_Dark, weapon.transform);
            body.transform.localPosition = new Vector3(0, 0, 0.1f);
            var barrel = Prim(PrimitiveType.Cylinder, "Lufa", Vector3.zero, new Vector3(0.06f, 0.35f, 0.06f), M_Gray, weapon.transform, null);
            barrel.transform.localPosition = new Vector3(0, 0.03f, 0.6f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var grip = Box("Chwyt", Vector3.zero, new Vector3(0.1f, 0.22f, 0.12f), M_Wood(), weapon.transform);
            grip.transform.localPosition = new Vector3(0, -0.15f, -0.05f);
            grip.transform.localRotation = Quaternion.Euler(-15, 0, 0);
            NoCollider(weapon);
            SetLayer(weapon, lw);

            // kamera Overlay – dziecko kamery bazowej, w tym samym miejscu
            var ovGo = new GameObject("Kamera_Overlay");
            ovGo.transform.SetParent(cam.transform, false);
            var ov = ovGo.AddComponent<Camera>();
            ov.fieldOfView = 60f;
            ov.nearClipPlane = 0.01f;
            ov.farClipPlane = 10f;
            ov.cullingMask = 1 << lw;
            var ovData = ov.GetUniversalAdditionalCameraData();
            ovData.renderType = CameraRenderType.Overlay;

            var demo = Manager<CameraStackDemo>();
            demo.baseCamera = cam;
            demo.overlayCamera = ov;
            demo.weaponLayer = LayerWeapon;
        }

        // =====================================================================
        // 07 – TŁO I RENDER TEXTURE
        // =====================================================================
        static void S07_Background()
        {
            var cam = Begin("07 · Tło kamery i Render Texture",
                "<b>Background Type</b> (w URP: Environment) – czym kamera wypełnia piksele, na których nie ma obiektów:\n" +
                "• <b>Skybox</b> – niebo z materiału skybox (Lighting → Environment),\n" +
                "• <b>Solid Color</b> – jednolity kolor (Background),\n" +
                "• <b>Uninitialized</b> – nic nie jest czyszczone, zawartość bufora jest nieokreślona (smugi) – " +
                "tylko gdy obiekty zakrywają cały ekran.\n" +
                "Spacja – kolejny typ tła (rusz kamerą, aby zobaczyć efekt Uninitialized).\n\n" +
                "<b>Render Texture</b>: kamera monitoringu nie rysuje na ekran, tylko do tekstury (Camera.targetTexture). " +
                "Tę teksturę wyświetla ekran telewizora w scenie – tak robi się monitoring, lustra, portale, podgląd postaci.\n\n" +
                "Półprzezroczysty „duch” jest na warstwie <b>„TylkoMonitor”</b>: kamera główna go nie rysuje (Culling Mask), " +
                "widać go tylko na monitorze. LPM – ukryj/pokaż ducha także na monitorze.",
                "<b>Spacja</b> – typ tła   <b>LPM</b> – warstwa TylkoMonitor w kamerze monitoringu", "", freeCamera: true);
            Floor();
            Cam(new Vector3(-2, 3.5f, -14f), new Vector3(2, 2f, 4f));
            int lmon = Layer(LayerMonitor);
            cam.cullingMask = ~(1 << lmon);

            Monument("Pomnik", new Vector3(-4, 0, 6));
            Scatter("Klocki", 10, 14f, 11);
            var walker = Character("Spacerowicz", new Vector3(0, 0, 2), M_Purple);
            Mover(walker, AutoMover.PathShape.Circle, new Vector3(-3, 0, 4), new Vector2(5, 4), 0.5f);

            var ghost = Prim(PrimitiveType.Capsule, "Duch", new Vector3(-1.5f, 1.2f, 3f), new Vector3(1, 1.2f, 1),
                TransparentMat("Duch", new Color(0.6f, 0.9f, 1f, 0.45f)));
            NoCollider(ghost);
            SetLayer(ghost, lmon);
            var gm = ghost.AddComponent<AutoMover>();
            gm.shape = AutoMover.PathShape.Line;
            gm.center = new Vector3(-1.5f, 1.2f, 3f);
            gm.size = new Vector2(3f, 0f);
            gm.speed = 0.6f;

            // telewizor
            var tv = Group("Telewizor");
            // ekran zwrócony w stronę kamery startowej (obrót 25° wokół osi Y)
            Box("Obudowa", new Vector3(7, 3, 6), new Vector3(5.2f, 3.2f, 0.4f), M_Dark, tv, Quaternion.Euler(0, 25, 0));
            Box("Noga", new Vector3(7, 0.75f, 6), new Vector3(0.4f, 1.5f, 0.4f), M_Dark, tv);
            var screen = Prim(PrimitiveType.Quad, "Ekran", new Vector3(7, 3, 6), new Vector3(4.8f, 2.7f, 1), UnlitMat("Ekran_monitora", Color.white), tv,
                Quaternion.Euler(0, 25, 0));
            // Quad jest widoczny od strony lokalnej osi -Z – przesuwamy go przed obudowę
            screen.transform.position -= screen.transform.forward * 0.21f;
            NoCollider(screen);
            Label(screen, "monitor (Render Texture)", 1.8f);

            var mon = NewCamera("Kamera_Monitoringu", new Vector3(-12, 7, 14), new Vector3(-2, 0.5f, 3));
            mon.fieldOfView = 55f;
            var monVis = Box("Kamera_Monitoringu_Obudowa", new Vector3(-12, 7, 14), new Vector3(0.6f, 0.4f, 0.9f), M_Gray, null, mon.transform.rotation);
            monVis.transform.SetParent(mon.transform, true);
            NoCollider(monVis);

            var demo = Manager<BackgroundDemo>();
            demo.mainCamera = cam;
            demo.monitorCamera = mon;
            demo.monitorScreen = screen.GetComponent<Renderer>();
            demo.monitorOnlyLayer = LayerMonitor;
        }

        // =====================================================================
        // 08 – EKRAN ↔ ŚWIAT
        // =====================================================================
        static void S08_ScreenWorld()
        {
            var cam = Begin("08 · Ekran, viewport i świat – przeliczenia współrzędnych",
                "Kamera zamienia współrzędne między przestrzeniami:\n" +
                "• <b>ekran</b> (Screen) – piksele, (0, 0) w lewym dolnym rogu,\n" +
                "• <b>viewport</b> – znormalizowane (0..1, 0..1), niezależne od rozdzielczości,\n" +
                "• <b>świat</b> (World) – metry w scenie.\n\n" +
                "Panel stanu pokazuje pozycję kursora w obu układach oraz promień <b>ScreenPointToRay</b> i wynik " +
                "<b>Physics.Raycast</b> – tak wybiera się obiekty myszką.\n" +
                "LPM – postaw znacznik w punkcie trafienia.\n" +
                "Spacja – <b>ViewportToWorldPoint</b>: kule w narożnikach i środku widoku, 8 m od kamery. Obróć kamerę – kule " +
                "zostają w świecie.\n" +
                "Etykiety obiektów pokazują wynik <b>WorldToScreenPoint</b> – współrzędne ekranowe i głębokość z (w metrach od kamery).",
                "<b>LPM</b> – znacznik (Raycast)   <b>Spacja</b> – kule w narożnikach widoku (ViewportToWorldPoint)", "", freeCamera: true);
            Floor();
            Cam(new Vector3(0, 6, -12f), new Vector3(0, 0.5f, 3f));

            var g = Group("Obiekty");
            Vector3[] pos = { new Vector3(-5, 1, 2), new Vector3(0, 1, 6), new Vector3(5, 1, 1), new Vector3(-2, 1, 12), new Vector3(7, 1, 10) };
            for (int i = 0; i < pos.Length; i++)
            {
                var o = i % 2 == 0 ? Box($"Obiekt_{i + 1}", pos[i], Vector3.one * 2, Palette[i], g) : Ball($"Obiekt_{i + 1}", pos[i], 2f, Palette[i], g);
                var l = Label(o, o.name, 1.4f);
                l.showScreenPosition = true;
            }

            var marker = Ball("Znacznik_wzor", Vector3.zero, 0.35f, M_Red);
            NoCollider(marker);
            Label(marker, "znacznik", 0.5f).showScreenPosition = true;
            marker.SetActive(false);

            var probe = cam.gameObject.AddComponent<ScreenSpaceProbe>();
            probe.markerPrefab = marker;
            probe.cornerMaterial = M_Yellow;
            probe.cornerDistance = 8f;
        }

        // =====================================================================
        // 09 – KAMERA ZE SKRYPTU
        // =====================================================================
        static void S09_ScriptedFollow()
        {
            var cam = Begin("09 · Kamera podążająca – własny skrypt",
                "Zanim użyjemy Cinemachine – kamera napisana ręcznie. Spacja przełącza metody:\n" +
                "1. <b>dziecko celu</b> (transform.parent) – sztywno; każdy podskok i skręt celu przenosi się na kamerę,\n" +
                "2. <b>Lerp w Update</b> ze stałym współczynnikiem – wygładza, ale wynik zależy od liczby klatek na sekundę,\n" +
                "3. <b>SmoothDamp w LateUpdate</b> – płynnie i niezależnie od FPS; LateUpdate wykonuje się po ruchu celu " +
                "w Update, więc kamera nie „drży”,\n" +
                "4. <b>stały punkt + LookAt</b> – kamera monitoringu, tylko się obraca.\n\n" +
                "LPM – ograniczenie do 20 FPS. Porównaj metodę 2 (kamera bardziej zostaje w tyle) i 3 (bez zmian).\n" +
                "Poprawny Lerp niezależny od FPS: t = 1 − e^(−k · Δt).\n\n" +
                "Następne sceny (10–19) robią to samo i dużo więcej za pomocą pakietu <b>Cinemachine</b>.",
                "<b>Spacja</b> – metoda podążania   <b>LPM</b> – limit 20 FPS", "");
            Floor();

            var target = Character("Cel", new Vector3(0, 0, 0), M_Red);
            Mover(target, AutoMover.PathShape.Figure8, Vector3.zero, new Vector2(12, 9), 0.4f, 0.6f, 0f, 0.25f);
            Label(target, "cel", 1.4f);
            PillarRing("Slupy", Vector3.zero, 20f, 14, 4f, M_Teal);
            Scatter("Klocki", 14, 18f, 9);

            Cam(new Vector3(0, 3, -7), Vector3.up);
            var f = cam.gameObject.AddComponent<ScriptedFollowCamera>();
            f.target = target.transform;
            f.offset = new Vector3(0, 3f, -7f);
            f.lerpFactor = 0.05f;
            f.smoothTime = 0.35f;
            f.fixedPoint = new Vector3(0, 9, -22);
        }
    }
}
