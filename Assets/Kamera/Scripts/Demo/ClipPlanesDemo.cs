using System.Collections.Generic;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 03: płaszczyzny obcinania (Near / Far) i ostrosłup widzenia (frustum).
    /// Spacja – kolejny zestaw Near/Far. W panelu stanu – liczba obiektów w ostrosłupie
    /// (GeometryUtility.CalculateFrustumPlanes + TestPlanesAABB – tak działa frustum culling).
    /// LPM – rysowanie ostrosłupa kamery liniami (Debug.DrawLine, widoczne w oknie Scene).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ClipPlanesDemo : DemoActionBase
    {
        [System.Serializable]
        public struct Preset
        {
            public string name;
            public float near;
            public float far;
        }

        public Preset[] presets =
        {
            new Preset { name = "domyślne", near = 0.3f, far = 1000f },
            new Preset { name = "duże Near – bliskie obiekty znikają", near = 6f, far = 1000f },
            new Preset { name = "małe Far – dalekie obiekty znikają", near = 0.3f, far = 25f },
            new Preset { name = "wąski zakres 6 – 25 m", near = 6f, far = 25f },
        };

        [Tooltip("Obiekty, dla których sprawdzamy przynależność do ostrosłupa")]
        public Renderer[] tracked;

        Camera m_Cam;
        int m_Index;
        readonly Plane[] m_Planes = new Plane[6];
        readonly List<string> m_Inside = new List<string>();

        void Awake() => m_Cam = GetComponent<Camera>();

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Index = (m_Index + 1) % presets.Length;
            Apply();
        }

        void Apply()
        {
            var p = presets[m_Index];
            m_Cam.nearClipPlane = p.near;
            m_Cam.farClipPlane = p.far;
            DemoHud.Log($"Near = {p.near}, Far = {p.far} ({p.name})");
        }

        void Update()
        {
            GeometryUtility.CalculateFrustumPlanes(m_Cam, m_Planes);
            int inside = 0;
            if (tracked != null)
            {
                foreach (var r in tracked)
                {
                    if (r == null) continue;
                    bool vis = GeometryUtility.TestPlanesAABB(m_Planes, r.bounds);
                    if (vis) inside++;
                    var lbl = r.GetComponent<WorldLabel>();
                    if (lbl != null)
                        lbl.extra = vis ? "<color=#7dff8a>w ostrosłupie</color>" : "<color=#ff8a80>poza</color>";
                }
            }
            var pr = presets[m_Index];
            float ratio = m_Cam.farClipPlane / m_Cam.nearClipPlane;
            DemoHud.SetStatus("clip",
                $"<b>Clipping Planes</b> – zestaw {m_Index + 1}/{presets.Length}: {pr.name}\n" +
                $"Near = {m_Cam.nearClipPlane} m   Far = {m_Cam.farClipPlane} m   (Far / Near = {ratio:0})\n" +
                $"obiekty w ostrosłupie: {inside} / {(tracked != null ? tracked.Length : 0)}");

            if (m_DrawFrustum)
                DrawFrustum();
        }

        bool m_DrawFrustum;

        public override void OnSecondaryAction()
        {
            m_DrawFrustum = !m_DrawFrustum;
            DemoHud.Log(m_DrawFrustum ? "rysowanie ostrosłupa (okno Scene)" : "bez rysowania ostrosłupa");
        }

        void DrawFrustum()
        {
            var nearC = new Vector3[4];
            var farC = new Vector3[4];
            m_Cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), m_Cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearC);
            m_Cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), Mathf.Min(m_Cam.farClipPlane, 60f), Camera.MonoOrStereoscopicEye.Mono, farC);
            for (int i = 0; i < 4; i++)
            {
                Vector3 a = transform.TransformPoint(nearC[i]);
                Vector3 b = transform.TransformPoint(farC[i]);
                Vector3 a2 = transform.TransformPoint(nearC[(i + 1) % 4]);
                Vector3 b2 = transform.TransformPoint(farC[(i + 1) % 4]);
                Debug.DrawLine(a, b, Color.yellow);
                Debug.DrawLine(a, a2, Color.green);
                Debug.DrawLine(b, b2, Color.red);
            }
        }
    }
}
