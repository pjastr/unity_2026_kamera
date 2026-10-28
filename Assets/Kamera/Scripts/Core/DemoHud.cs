using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kamera
{
    /// <summary>
    /// Wspólny "menedżer" każdej sceny demonstracyjnej:
    ///  • panel z tytułem i opisem (po polsku) – przełączany klawiszem C,
    ///  • panel stanu (prawy górny róg): parametry kamery na żywo + dziennik zdarzeń,
    ///  • pasek podpowiedzi sterowania (dół ekranu),
    ///  • skróty: Spacja – akcja sceny, LPM – akcja dodatkowa, przytrzymaj E – reset,
    ///    1/2 – poprzednia/następna scena, Esc – wyjście (w buildzie).
    /// Obiekty UI tworzy generator scen (menu Kamera) – są widoczne w hierarchii.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DemoHud : MonoBehaviour
    {
        public static DemoHud Instance { get; private set; }

        [Header("Treść")]
        public string title = "Tytuł sceny";
        [TextArea(5, 30)] public string description = "Opis";
        [Tooltip("Co robi Spacja / LPM w tej scenie")]
        [TextArea(1, 4)] public string sceneHint = "<b>Spacja</b> – akcja sceny";
        [Tooltip("Sterowanie w tej scenie (kamera, postać)")]
        [TextArea(1, 4)] public string controlsHint = "";

        [Header("Referencje UI (ustawiane przez generator)")]
        public GameObject descriptionPanel;
        public Text titleText;
        public Text descriptionText;
        public Text hintText;
        public GameObject statusPanel;
        public Text statusText;
        public RectTransform labelsRoot;
        public Font font;

        [Header("Dziennik")]
        public int maxLogLines = 6;

        readonly List<string> m_Log = new List<string>();
        readonly List<string> m_StatusKeys = new List<string>();
        readonly Dictionary<string, string> m_Status = new Dictionary<string, string>();
        readonly StringBuilder m_Sb = new StringBuilder();
        bool m_Dirty = true;

        void Awake()
        {
            Instance = this;
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Refresh();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Refresh()
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            if (hintText != null)
            {
                int idx = SceneManager.GetActiveScene().buildIndex;
                int count = SceneManager.sceneCountInBuildSettings;
                string sceneInfo = idx >= 0 && count > 0 ? $"Scena {idx + 1}/{count}" : "Scena";
                string controls = string.IsNullOrEmpty(controlsHint) ? "" : controlsHint + "   ";
                hintText.text =
                    $"<b>{sceneInfo}</b>   {sceneHint}\n" + controls +
                    "<b>C</b> – opis   <b>przytrzymaj E</b> – reset   <b>1/2</b> – poprzednia/następna scena   <b>Esc</b> – wyjście";
            }
        }

        void Update()
        {
            if (DemoInput.PressedThisFrame(DemoInput.Jump) && !SuppressMainAction)
                DemoActionBase.InvokeMain();

            if (DemoInput.PressedThisFrame(DemoInput.Attack))
                DemoActionBase.InvokeSecondary();

            if (DemoInput.PerformedThisFrame(DemoInput.Interact))
                ReloadScene();

            if (DemoInput.PressedThisFrame(DemoInput.Crouch) && descriptionPanel != null)
                descriptionPanel.SetActive(!descriptionPanel.activeSelf);

            if (DemoInput.PressedThisFrame(DemoInput.Next))
                LoadRelative(+1);
            if (DemoInput.PressedThisFrame(DemoInput.Previous))
                LoadRelative(-1);

            if (DemoInput.PressedThisFrame(DemoInput.Cancel))
            {
#if !UNITY_EDITOR
                Application.Quit();
#endif
            }
        }

        void LateUpdate()
        {
            if (!m_Dirty || statusText == null)
                return;
            m_Dirty = false;
            m_Sb.Clear();
            foreach (var k in m_StatusKeys)
            {
                if (m_Sb.Length > 0) m_Sb.Append('\n');
                m_Sb.Append(m_Status[k]);
            }
            if (m_Log.Count > 0)
            {
                if (m_Sb.Length > 0) m_Sb.Append("\n\n");
                m_Sb.Append("<b>Dziennik</b>");
                foreach (var line in m_Log)
                    m_Sb.Append('\n').Append(line);
            }
            statusText.text = m_Sb.ToString();
            if (statusPanel != null)
                statusPanel.SetActive(m_Sb.Length > 0);
        }

        /// <summary>Scena może wyłączyć Spację jako akcję sceny (np. gdy Spacja to skok postaci).</summary>
        public static bool SuppressMainAction { get; set; }

        public static void ReloadScene()
        {
            var s = SceneManager.GetActiveScene();
            if (s.buildIndex >= 0)
                SceneManager.LoadScene(s.buildIndex);
            else
                SceneManager.LoadScene(s.name);
        }

        public static void LoadRelative(int delta)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (count <= 0)
            {
                Debug.LogWarning("[Kamera] Brak scen w Build Settings – użyj menu Kamera > Generuj wszystkie sceny.");
                return;
            }
            int idx = SceneManager.GetActiveScene().buildIndex;
            if (idx < 0) idx = 0;
            int next = ((idx + delta) % count + count) % count;
            SceneManager.LoadScene(next);
        }

        // ---------------- panel stanu ----------------

        /// <summary>Ustawia (lub zastępuje) blok tekstu w panelu stanu. Klucz porządkuje bloki.</summary>
        public static void SetStatus(string key, string text)
        {
            if (Instance == null)
                return;
            var h = Instance;
            if (!h.m_Status.ContainsKey(key))
                h.m_StatusKeys.Add(key);
            if (h.m_Status.TryGetValue(key, out var old) && old == text)
                return;
            h.m_Status[key] = text;
            h.m_Dirty = true;
        }

        public static void Log(string line)
        {
            if (Instance == null)
            {
                Debug.Log("[Kamera] " + line);
                return;
            }
            var h = Instance;
            h.m_Log.Add($"<color=#9fd3ff>{Time.time:0.0}s</color>  {line}");
            while (h.m_Log.Count > h.maxLogLines)
                h.m_Log.RemoveAt(0);
            h.m_Dirty = true;
        }

        // ---------------- etykiety ----------------

        public Text CreateLabelText(string name)
        {
            if (labelsRoot == null)
                return null;
            var go = new GameObject("Label_" + name, typeof(RectTransform));
            go.transform.SetParent(labelsRoot, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(260, 120);
            rt.pivot = new Vector2(0.5f, 0f);
            var t = go.AddComponent<Text>();
            t.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 15;
            t.alignment = TextAnchor.LowerCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.color = Color.white;
            t.raycastTarget = false;
            var sh = go.AddComponent<Outline>();
            sh.effectColor = new Color(0, 0, 0, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }
    }
}
