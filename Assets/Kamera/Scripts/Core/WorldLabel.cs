using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kamera
{
    /// <summary>
    /// Etykieta "przyklejona" do obiektu w świecie, rysowana na Canvasie HUD.
    /// Pozycję na ekranie liczy Camera.WorldToScreenPoint (rzutowanie punktu 3D na ekran).
    /// </summary>
    [DefaultExecutionOrder(10000)] // po ruchu kamer w LateUpdate (Cinemachine Brain, ChaseCamera…)
    public class WorldLabel : MonoBehaviour
    {
        [TextArea(1, 4)] public string header = "";
        public Vector3 worldOffset = new Vector3(0, 1f, 0);
        public Color color = Color.white;
        public int fontSize = 15;
        [Tooltip("Kamera używana do rzutowania (puste = Camera.main)")]
        public Camera targetCamera;
        [Tooltip("Pokaż współrzędne ekranowe obiektu (WorldToScreenPoint)")]
        public bool showScreenPosition = false;

        /// <summary>Dodatkowy tekst ustawiany przez inne skrypty.</summary>
        [NonSerialized] public string extra = "";

        Text m_Text;

        void Start()
        {
            if (DemoHud.Instance != null)
            {
                m_Text = DemoHud.Instance.CreateLabelText(name);
                if (m_Text != null)
                {
                    m_Text.color = color;
                    m_Text.fontSize = fontSize;
                }
            }
        }

        void OnDisable()
        {
            if (m_Text != null) m_Text.enabled = false;
        }

        void OnDestroy()
        {
            if (m_Text != null) Destroy(m_Text.gameObject);
        }

        void LateUpdate()
        {
            if (m_Text == null)
                return;
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null || !cam.isActiveAndEnabled)
            {
                m_Text.enabled = false;
                return;
            }

            Vector3 sp = cam.WorldToScreenPoint(transform.position + worldOffset);
            // punkt za kamerą (z < 0) lub poza prostokątem widoku kamery – nie rysujemy
            Rect px = cam.pixelRect;
            if (sp.z <= 0.05f || !px.Contains(new Vector2(sp.x, sp.y)))
            {
                m_Text.enabled = false;
                return;
            }
            m_Text.enabled = true;
            var rt = (RectTransform)m_Text.transform;
            rt.position = new Vector3(sp.x, sp.y, 0f);

            string s = string.IsNullOrEmpty(header) ? "" : "<b>" + header + "</b>";
            if (showScreenPosition)
                s += (s.Length > 0 ? "\n" : "") + $"ekran: ({sp.x:0}, {sp.y:0}) px, z = {sp.z:0.0} m";
            if (!string.IsNullOrEmpty(extra))
                s += (s.Length > 0 ? "\n" : "") + extra;
            m_Text.text = s;
        }
    }
}
