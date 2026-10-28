using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kamera.EditorTools
{
    /// <summary>
    /// Generator scen demonstracyjnych + polecenia w górnym menu "Kamera".
    /// Sceny są tworzone kodem, więc zawsze można je odtworzyć (Kamera > Generuj wszystkie sceny).
    /// </summary>
    public static partial class SceneBuilder
    {
        public const string Root = "Assets/Kamera";
        public const string ScenesDir = Root + "/Scenes";
        const string MatDir = Root + "/Materials";
        const string TexDir = Root + "/Textures";
        const string DataDir = Root + "/Data";
        const string ActionsPath = "Assets/InputSystem_Actions.inputactions";
        const string NoiseDir = "Packages/com.unity.cinemachine/Presets/Noise";
        public const string BuildPath = "Builds/Windows/KameraDemo.exe";

        public struct SceneDef
        {
            public string file;
            public Action build;
            public SceneDef(string file, Action build) { this.file = file; this.build = build; }
        }

        /// <summary>Lista scen w kolejności numeracji (część I – kamera Unity, część II – Cinemachine).</summary>
        public static readonly SceneDef[] Scenes =
        {
            new SceneDef("01_Perspektywa_Ortografia", S01_Projection),
            new SceneDef("02_FOV_DollyZoom", S02_Fov),
            new SceneDef("03_Clipping_Frustum", S03_Clipping),
            new SceneDef("04_Viewport_PodzialEkranu", S04_Viewport),
            new SceneDef("05_CullingMask_Minimapa", S05_CullingMask),
            new SceneDef("06_CameraStack_Overlay", S06_CameraStack),
            new SceneDef("07_Tlo_RenderTexture", S07_Background),
            new SceneDef("08_Ekran_Swiat_Raycast", S08_ScreenWorld),
            new SceneDef("09_Kamera_ze_skryptu", S09_ScriptedFollow),
            new SceneDef("10_CM_Podstawy_Follow", S10_CmBasics),
            new SceneDef("11_CM_Priorytety_Blendy", S11_CmPriority),
            new SceneDef("12_CM_Orbita_FreeLook", S12_CmOrbital),
            new SceneDef("13_CM_TrzeciaOsoba", S13_CmThirdPerson),
            new SceneDef("14_CM_PierwszaOsoba", S14_CmFirstPerson),
            new SceneDef("15_CM_Z_gory_TargetGroup", S15_CmGroup),
            new SceneDef("16_CM_2D_Confiner", S16_Cm2D),
            new SceneDef("17_CM_Dolly_Spline", S17_CmDolly),
            new SceneDef("18_CM_Szum_Impuls", S18_CmNoise),
            new SceneDef("19_CM_ClearShot", S19_CmClearShot),
        };

        public static string ScenePath(string file) => $"{ScenesDir}/{file}.unity";

        // =====================================================================
        //  MENU
        // =====================================================================

        [MenuItem("Kamera/Generuj wszystkie sceny", priority = 0)]
        public static void GenerateAllMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            GenerateAll();
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorUtility.DisplayDialog("Kamera",
                $"Wygenerowano {Scenes.Length} scen w {ScenesDir} i dodano je do Build Settings.\n\n" +
                "Naciśnij Play. Klawisze 1/2 przełączają sceny.", "OK");
        }

        [MenuItem("Kamera/Panel scen (otwieranie i generowanie)", priority = 1)]
        public static void OpenBrowser() => SceneBrowserWindow.ShowWindow();

        [MenuItem("Kamera/Otwórz pierwszą scenę i uruchom Play", priority = 2)]
        public static void OpenFirstAndPlay()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!File.Exists(ScenePath(Scenes[0].file)))
                GenerateAll();
            EditorSceneManager.OpenScene(ScenePath(Scenes[0].file));
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Kamera/Zbuduj wersję Windows (x64)", priority = 20)]
        public static void BuildWindowsMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var report = BuildWindows();
            bool ok = report != null && report.summary.result == BuildResult.Succeeded;
            if (ok)
            {
                EditorUtility.DisplayDialog("Kamera", $"Build zakończony powodzeniem:\n{Path.GetFullPath(BuildPath)}", "OK");
                EditorUtility.RevealInFinder(BuildPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Kamera", "Build nie powiódł się – szczegóły w oknie Console.", "OK");
            }
        }

        [MenuItem("Kamera/Napraw ustawienia projektu (Input System, warstwy, Build Settings)", priority = 21)]
        public static void FixProjectSettingsMenu()
        {
            FixProjectSettings();
            UpdateBuildSettings();
            EditorUtility.DisplayDialog("Kamera",
                "Ustawiono:\n• Active Input Handling = Input System Package (New)\n" +
                "• Project-wide Actions = Assets/InputSystem_Actions.inputactions\n" +
                "• warstwy 8–10: Minimapa, Bron, TylkoMonitor\n• sceny 01–19 w Build Settings", "OK");
        }

        [MenuItem("Kamera/Otwórz instrukcję (README.md)", priority = 40)]
        public static void OpenReadme()
        {
            string p = Path.GetFullPath("README.md");
            if (File.Exists(p))
                EditorUtility.OpenWithDefaultApp(p);
            else
                Debug.LogWarning("Nie znaleziono README.md w katalogu projektu.");
        }

        // =====================================================================
        //  BATCH MODE (wywołania z linii poleceń: -executeMethod ...)
        // =====================================================================

        /// <summary>Unity.exe -batchmode -projectPath ... -executeMethod Kamera.EditorTools.SceneBuilder.BatchGenerate</summary>
        public static void BatchGenerate()
        {
            int code = 0;
            try
            {
                GenerateAll();
                Debug.Log("[Kamera] BatchGenerate: OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        /// <summary>Generuje sceny i buduje wersję Windows x64.</summary>
        public static void BatchGenerateAndBuildWindows()
        {
            int code = 0;
            try
            {
                GenerateAll();
                var report = BuildWindows();
                if (report == null || report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[Kamera] Build Windows: NIEPOWODZENIE " + (report != null ? report.summary.result.ToString() : "brak raportu"));
                    code = 2;
                }
                else
                {
                    Debug.Log($"[Kamera] Build Windows: OK – {report.summary.totalSize / (1024 * 1024)} MB, błędów: {report.summary.totalErrors}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        // =====================================================================
        //  GENEROWANIE
        // =====================================================================

        public static void GenerateAll()
        {
            FixProjectSettings();
            EnsureFolders();
            try
            {
                for (int i = 0; i < Scenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Kamera", "Generowanie " + Scenes[i].file, (float)i / Scenes.Length);
                    GenerateScene(Scenes[i]);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateScene(SceneDef def)
        {
            EnsureFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s_Hud = null;
            s_Camera = null;
            def.build();
            string path = ScenePath(def.file);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("[Kamera] Zapisano scenę: " + path);
        }

        public static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var d in Scenes)
            {
                string p = ScenePath(d.file);
                if (File.Exists(p))
                    list.Add(new EditorBuildSettingsScene(p, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }

        public static BuildReport BuildWindows()
        {
            UpdateBuildSettings();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                GenerateAll();
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath));
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            return BuildPipeline.BuildPlayer(opts);
        }

        public static void FixProjectSettings()
        {
            // 1) Active Input Handling = Input System Package (New)
            var psAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
            if (psAsset != null)
            {
                var so = new SerializedObject(psAsset);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue == 0)
                {
                    prop.intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.LogWarning("[Kamera] Ustawiono Active Input Handling = Input System Package (New). Uruchom ponownie edytor.");
                }
            }

            // 2) Project-wide actions = domyślny plik InputSystem_Actions
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            if (actions != null && InputSystem.actions != actions)
            {
                InputSystem.actions = actions;
                Debug.Log("[Kamera] Ustawiono Project-wide Actions: " + ActionsPath);
            }
            else if (actions == null)
            {
                Debug.LogWarning("[Kamera] Nie znaleziono " + ActionsPath + " – sterowanie nie będzie działać.");
            }

            // 3) Warstwy używane w scenach (Culling Mask)
            EnsureLayer(LayerMinimap, 8);
            EnsureLayer(LayerWeapon, 9);
            EnsureLayer(LayerMonitor, 10);
        }

        public const string LayerMinimap = "Minimapa";
        public const string LayerWeapon = "Bron";
        public const string LayerMonitor = "TylkoMonitor";

        static void EnsureLayer(string name, int index)
        {
            if (LayerMask.NameToLayer(name) >= 0)
                return;
            var tm = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").FirstOrDefault();
            if (tm == null) return;
            var so = new SerializedObject(tm);
            var layers = so.FindProperty("layers");
            if (layers == null || index >= layers.arraySize) return;
            var el = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(el.stringValue))
            {
                Debug.LogWarning($"[Kamera] Warstwa {index} jest zajęta przez \"{el.stringValue}\" – nie można dodać \"{name}\".");
                return;
            }
            el.stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[Kamera] Dodano warstwę {index}: {name}");
        }

        static int Layer(string name)
        {
            int l = LayerMask.NameToLayer(name);
            return l < 0 ? 0 : l;
        }

        static void EnsureFolders()
        {
            foreach (var f in new[] { Root, ScenesDir, MatDir, TexDir, DataDir })
            {
                if (AssetDatabase.IsValidFolder(f))
                    continue;
                string parent = Path.GetDirectoryName(f).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, Path.GetFileName(f));
            }
        }

        // =====================================================================
        //  MATERIAŁY I TEKSTURY
        // =====================================================================

        static Shader LitShader
        {
            get
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                if (s == null) s = Shader.Find("Standard");
                return s;
            }
        }

        static Material Mat(string name, Color c)
        {
            string path = $"{MatDir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(LitShader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = c;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material UnlitMat(string name, Color c)
        {
            string path = $"{MatDir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/Unlit");
                if (sh == null) sh = Shader.Find("Unlit/Color");
                m = new Material(sh);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material TransparentMat(string name, Color c)
        {
            var m = Mat(name, c);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetShaderPassEnabled("ShadowCaster", false);
            m.SetShaderPassEnabled("DepthOnly", false);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material s_FloorMat;

        static Material FloorMat()
        {
            if (s_FloorMat != null)
                return s_FloorMat;
            string texPath = $"{TexDir}/Szachownica.png";
            if (!File.Exists(texPath))
            {
                const int size = 256;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Color a = new Color(0.58f, 0.6f, 0.63f);
                Color b = new Color(0.46f, 0.48f, 0.51f);
                Color line = new Color(0.36f, 0.38f, 0.41f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool cell = ((x / (size / 2)) + (y / (size / 2))) % 2 == 0;
                    bool border = x % (size / 2) < 2 || y % (size / 2) < 2;
                    tex.SetPixel(x, y, border ? line : (cell ? a : b));
                }
                tex.Apply();
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath);
                var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.anisoLevel = 8;
                imp.filterMode = FilterMode.Trilinear;
                imp.SaveAndReimport();
            }
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            var m = Mat("Podloga", Color.white);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
            m.mainTexture = t;
            m.mainTextureScale = new Vector2(60, 60); // podłoga 120 m → kratka 1 m
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(m);
            s_FloorMat = m;
            return m;
        }

        static NoiseSettings LoadNoise(string file)
        {
            var n = AssetDatabase.LoadAssetAtPath<NoiseSettings>($"{NoiseDir}/{file}.asset");
            if (n == null)
                Debug.LogWarning($"[Kamera] Nie znaleziono profilu szumu {NoiseDir}/{file}.asset");
            return n;
        }

        static readonly Color ColTitle = new Color(1f, 0.85f, 0.4f);

        static Material M_Red => Mat("Czerwony", new Color(0.86f, 0.27f, 0.23f));
        static Material M_Orange => Mat("Pomaranczowy", new Color(0.95f, 0.55f, 0.15f));
        static Material M_Yellow => Mat("Zolty", new Color(0.95f, 0.82f, 0.25f));
        static Material M_Green => Mat("Zielony", new Color(0.3f, 0.75f, 0.35f));
        static Material M_Teal => Mat("Turkusowy", new Color(0.2f, 0.7f, 0.75f));
        static Material M_Blue => Mat("Niebieski", new Color(0.25f, 0.45f, 0.9f));
        static Material M_Purple => Mat("Fioletowy", new Color(0.6f, 0.35f, 0.85f));
        static Material M_Gray => Mat("Szary", new Color(0.55f, 0.57f, 0.6f));
        static Material M_Dark => Mat("Ciemny", new Color(0.22f, 0.23f, 0.26f));
        static Material M_White => Mat("Bialy", new Color(0.92f, 0.92f, 0.92f));
        static Material M_Wall => Mat("Sciana", new Color(0.72f, 0.66f, 0.58f));
        static Material M_Stone => Mat("Kamien", new Color(0.62f, 0.62f, 0.58f));

        static Material[] Palette => new[] { M_Red, M_Orange, M_Yellow, M_Green, M_Teal, M_Blue, M_Purple, M_White };

        // =====================================================================
        //  SCENA, KAMERA, HUD
        // =====================================================================

        static DemoHud s_Hud;
        static Camera s_Camera;

        /// <summary>
        /// Wspólny początek sceny: światło, kamera główna, HUD.
        /// freeCamera – swobodna kamera (PPM + mysz, WASD); cinemachine – CinemachineBrain na kamerze głównej.
        /// </summary>
        static Camera Begin(string title, string description, string sceneHint, string controls,
            bool freeCamera = false, bool cinemachine = false)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.46f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.21f, 0.2f);
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.97f, 0.92f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            s_Camera = camGo.AddComponent<Camera>();
            s_Camera.nearClipPlane = 0.1f;
            s_Camera.farClipPlane = 500f;
            s_Camera.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();
            if (freeCamera)
                camGo.AddComponent<FreeCamera>();
            if (cinemachine)
            {
                var brain = camGo.AddComponent<CinemachineBrain>();
                brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.5f);
                brain.ShowDebugText = false;
                camGo.AddComponent<CMStatus>();
            }

            if (freeCamera)
                controls = "<b>PPM + mysz</b> – obrót kamery   <b>WASD</b> – ruch kamery   <b>Shift</b> – szybciej   <b>kółko</b> – przód/tył" +
                           (string.IsNullOrEmpty(controls) ? "" : "   " + controls);
            s_Hud = CreateHud(title, description, sceneHint, controls);
            return s_Camera;
        }

        static void Cam(Vector3 pos, Vector3 lookAt)
        {
            s_Camera.transform.position = pos;
            s_Camera.transform.LookAt(lookAt);
        }

        static DemoHud CreateHud(string title, string description, string hint, string controls)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("UI_Opis", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var labels = NewRect("Etykiety", canvasGo.transform);
            labels.anchorMin = Vector2.zero;
            labels.anchorMax = Vector2.zero;
            labels.pivot = Vector2.zero;
            labels.anchoredPosition = Vector2.zero;
            labels.sizeDelta = Vector2.zero;

            var panel = NewPanel("PanelOpisu", canvasGo.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -12), 560f,
                new Color(0.04f, 0.06f, 0.09f, 0.82f));
            var titleText = NewText("Tytul", panel, font, 24, FontStyle.Bold, ColTitle, TextAnchor.UpperLeft);
            titleText.text = title;
            var descText = NewText("Opis", panel, font, 16, FontStyle.Normal, new Color(0.93f, 0.94f, 0.96f), TextAnchor.UpperLeft);
            descText.text = description;
            descText.lineSpacing = 1.05f;

            var hintPanel = NewRect("PasekPodpowiedzi", canvasGo.transform);
            hintPanel.anchorMin = new Vector2(0, 0);
            hintPanel.anchorMax = new Vector2(1, 0);
            hintPanel.pivot = new Vector2(0.5f, 0);
            hintPanel.anchoredPosition = Vector2.zero;
            hintPanel.sizeDelta = new Vector2(0, 60);
            hintPanel.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.78f);
            var hlg = hintPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            var hfit = hintPanel.gameObject.AddComponent<ContentSizeFitter>();
            hfit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var hintText = NewText("Podpowiedz", hintPanel, font, 16, FontStyle.Normal, new Color(0.9f, 0.92f, 0.95f), TextAnchor.MiddleCenter);

            var statusPanel = NewPanel("PanelStanu", canvasGo.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -12), 600f,
                new Color(0.04f, 0.06f, 0.09f, 0.78f));
            var statusText = NewText("Stan", statusPanel, font, 15, FontStyle.Normal, new Color(0.93f, 0.94f, 0.96f), TextAnchor.UpperLeft);
            statusPanel.gameObject.SetActive(false);

            var mgr = new GameObject("DemoManager");
            var hud = mgr.AddComponent<DemoHud>();
            hud.title = title;
            hud.description = description;
            hud.sceneHint = hint;
            hud.controlsHint = controls;
            hud.descriptionPanel = panel.gameObject;
            hud.titleText = titleText;
            hud.descriptionText = descText;
            hud.hintText = hintText;
            hud.statusPanel = statusPanel.gameObject;
            hud.statusText = statusText;
            hud.labelsRoot = labels;
            hud.font = font;
            hud.Refresh();
            return hud;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform NewPanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, float width, Color bg)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 100);
            rt.gameObject.AddComponent<Image>().color = bg;
            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 10, 12);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fit = rt.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        static Text NewText(string name, Transform parent, Font font, int size, FontStyle style, Color color, TextAnchor align)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static T Manager<T>() where T : Component => s_Hud.gameObject.AddComponent<T>();

        // =====================================================================
        //  OBIEKTY SCENY
        // =====================================================================

        static Transform Group(string name) => new GameObject(name).transform;

        static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.rotation = rot ?? Quaternion.identity;
            if (parent != null)
            {
                // rozmiar w świecie niezależny od skali rodzica
                Vector3 ps = parent.lossyScale;
                scale = new Vector3(scale.x / ps.x, scale.y / ps.y, scale.z / ps.z);
            }
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, Transform parent = null, Quaternion? rot = null)
            => Prim(PrimitiveType.Cube, name, pos, size, mat, parent, rot);

        static GameObject Ball(string name, Vector3 pos, float diameter, Material mat, Transform parent = null)
            => Prim(PrimitiveType.Sphere, name, pos, Vector3.one * diameter, mat, parent);

        static void NoCollider(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                UnityEngine.Object.DestroyImmediate(c);
        }

        static void SetLayer(GameObject go, int layer)
        {
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;
        }

        static WorldLabel Label(GameObject go, string header, float offsetY = 1f, int fontSize = 15)
        {
            var l = go.AddComponent<WorldLabel>();
            l.header = header;
            l.worldOffset = new Vector3(0, offsetY, 0);
            l.fontSize = fontSize;
            return l;
        }

        static GameObject SectionLabel(string text, Vector3 pos, Transform parent = null)
        {
            var go = new GameObject("Napis_" + text.Split('\n')[0]);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            var l = Label(go, text, 0f, 20);
            l.color = ColTitle;
            return go;
        }

        static GameObject Floor(float size = 120f)
        {
            return Box("Podloga", new Vector3(0, -0.5f, 0), new Vector3(size, 1, size), FloorMat());
        }

        /// <summary>Postać: kapsuła 2 m z "nosem" wskazującym przód. Opcjonalnie CharacterController zamiast collidera.</summary>
        static GameObject Character(string name, Vector3 feet, Material mat, bool controller = false, Transform parent = null)
        {
            var root = Prim(PrimitiveType.Capsule, name, feet + Vector3.up, Vector3.one, mat, parent);
            var nose = Box(name + "_Przod", feet + new Vector3(0, 1.45f, 0.45f), new Vector3(0.5f, 0.18f, 0.3f), M_Dark, root.transform);
            NoCollider(nose);
            if (controller)
            {
                UnityEngine.Object.DestroyImmediate(root.GetComponent<Collider>());
                var cc = root.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = Vector3.zero;
                cc.slopeLimit = 45f;
                cc.stepOffset = 0.3f;
            }
            return root;
        }

        static AutoMover Mover(GameObject go, AutoMover.PathShape shape, Vector3 center, Vector2 size, float speed, float variation = 0f, float phase = 0f, float bob = 0f)
        {
            var m = go.AddComponent<AutoMover>();
            m.shape = shape;
            // center podajemy na poziomie podłogi – wysokość obiektu (np. środek kapsuły y = 1) zostaje zachowana
            m.center = center + Vector3.up * go.transform.position.y;
            m.size = size;
            m.speed = speed;
            m.speedVariation = variation;
            m.phase = phase;
            m.bobHeight = bob;
            m.transform.position = m.Evaluate(phase);
            return m;
        }

        /// <summary>Słupy na okręgu – punkty odniesienia, przy których dobrze widać ruch kamery.</summary>
        static void PillarRing(string name, Vector3 center, float radius, int count, float height, Material mat)
        {
            var g = Group(name);
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                Box($"Slup_{i + 1}", center + new Vector3(Mathf.Cos(a) * radius, height / 2f, Mathf.Sin(a) * radius),
                    new Vector3(0.8f, height, 0.8f), i % 2 == 0 ? mat : M_Gray, g);
            }
        }

        /// <summary>Pomnik – charakterystyczny obiekt, na który patrzą kamery.</summary>
        static GameObject Monument(string name, Vector3 pos)
        {
            var root = new GameObject(name);
            root.transform.position = pos;
            Box("Cokol", pos + new Vector3(0, 0.5f, 0), new Vector3(3, 1, 3), M_Stone, root.transform);
            Prim(PrimitiveType.Cylinder, "Kolumna", pos + new Vector3(0, 2.5f, 0), new Vector3(1.2f, 1.5f, 1.2f), M_White, root.transform);
            Ball("Kula", pos + new Vector3(0, 4.6f, 0), 1.6f, M_Orange, root.transform);
            return root;
        }

        static void Scatter(string name, int count, float radius, int seed, Transform parent = null, float minRadius = 6f)
        {
            var g = parent != null ? parent : Group(name);
            var rnd = new System.Random(seed);
            var pal = Palette;
            for (int i = 0; i < count; i++)
            {
                float a = (float)rnd.NextDouble() * Mathf.PI * 2f;
                float r = minRadius + (float)rnd.NextDouble() * (radius - minRadius);
                float h = 0.6f + (float)rnd.NextDouble() * 2.4f;
                float w = 0.6f + (float)rnd.NextDouble() * 1.6f;
                Box($"Klocek_{i + 1}", new Vector3(Mathf.Cos(a) * r, h / 2f, Mathf.Sin(a) * r), new Vector3(w, h, w),
                    pal[i % pal.Length], g, Quaternion.Euler(0, (float)rnd.NextDouble() * 90f, 0));
            }
        }

        // ------------------------------ Cinemachine ------------------------------

        static CinemachineCamera CmCamera(string name, Vector3 pos, Vector3 lookAt, int priority = 10, float fov = 50f, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            if ((lookAt - pos).sqrMagnitude > 1e-4f)
                go.transform.rotation = Quaternion.LookRotation(lookAt - pos);
            var cam = go.AddComponent<CinemachineCamera>();
            cam.Priority = priority;
            var lens = LensSettings.Default;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.1f;
            lens.FarClipPlane = 500f;
            cam.Lens = lens;
            return cam;
        }

        static CinemachineRotationComposer AimComposer(CinemachineCamera cam, Vector3 targetOffset, Vector2 damping)
        {
            var c = cam.gameObject.AddComponent<CinemachineRotationComposer>();
            c.TargetOffset = targetOffset;
            c.Damping = damping;
            c.Composition = ScreenComposerSettings.Default;
            c.CenterOnActivate = true;
            return c;
        }

        /// <summary>Linia (np. tor, granica) – LineRenderer z materiałem Unlit.</summary>
        static LineRenderer Line(string name, Vector3[] points, Color color, float width, bool loop, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, true);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = loop;
            lr.positionCount = points.Length;
            lr.SetPositions(points);
            lr.widthMultiplier = width;
            lr.sharedMaterial = UnlitMat("Linia_" + ColorUtility.ToHtmlStringRGB(color), color);
            lr.shadowCastingMode = ShadowCastingMode.Off;
            return lr;
        }
    }
}
