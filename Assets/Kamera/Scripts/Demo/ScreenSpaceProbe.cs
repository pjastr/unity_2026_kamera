using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 08: przeliczenia między przestrzeniami ekranu, widoku (viewport) i świata.
    ///  • ScreenToViewportPoint – piksele → zakres 0..1,
    ///  • ScreenPointToRay + Physics.Raycast – co jest pod kursorem (LPM stawia znacznik),
    ///  • WorldToScreenPoint – gdzie na ekranie jest punkt 3D (etykiety obiektów),
    ///  • ViewportToWorldPoint – Spacja stawia kule w narożnikach widoku w odległości 8 m.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScreenSpaceProbe : DemoActionBase
    {
        public GameObject markerPrefab;
        public Material cornerMaterial;
        public float cornerDistance = 8f;

        Camera m_Cam;
        Vector3 m_LastHit;
        bool m_HasHit;
        string m_HitName = "-";
        int m_Markers;

        void Awake() => m_Cam = GetComponent<Camera>();

        public override void OnMainAction()
        {
            // prawie narożniki (0.1 / 0.9) – kule i etykiety są w całości widoczne
            Vector2[] corners = { new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.15f), new Vector2(0.9f, 0.85f), new Vector2(0.1f, 0.85f), new Vector2(0.5f, 0.5f) };
            foreach (var c in corners)
            {
                Vector3 w = m_Cam.ViewportToWorldPoint(new Vector3(c.x, c.y, cornerDistance));
                var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                s.name = $"Viewport({c.x:0.#};{c.y:0.#})";
                s.transform.position = w;
                s.transform.localScale = Vector3.one * 0.4f;
                Destroy(s.GetComponent<Collider>());
                if (cornerMaterial != null) s.GetComponent<Renderer>().sharedMaterial = cornerMaterial;
                var l = s.AddComponent<WorldLabel>();
                l.header = $"viewport ({c.x:0.#}; {c.y:0.#}; {cornerDistance})";
                l.worldOffset = new Vector3(0, 0.4f, 0);
                Destroy(s, 12f);
            }
            DemoHud.Log($"ViewportToWorldPoint: 5 kul w odległości {cornerDistance} m (znikną po 12 s)");
        }

        public override void OnSecondaryAction()
        {
            if (!m_HasHit || markerPrefab == null)
                return;
            var m = Instantiate(markerPrefab, m_LastHit, Quaternion.identity);
            m.name = "Znacznik_" + (++m_Markers);
            m.SetActive(true);
            var l = m.GetComponent<WorldLabel>();
            if (l != null) l.header = m.name;
            DemoHud.Log($"Raycast: trafiono {m_HitName} w ({m_LastHit.x:0.00}; {m_LastHit.y:0.00}; {m_LastHit.z:0.00})");
        }

        void Update()
        {
            Vector2 px = DemoInput.PointerPosition;
            Vector3 vp = m_Cam.ScreenToViewportPoint(px);
            Ray ray = m_Cam.ScreenPointToRay(px);
            m_HasHit = Physics.Raycast(ray, out RaycastHit hit, 500f);
            if (m_HasHit)
            {
                m_LastHit = hit.point;
                m_HitName = hit.collider.name;
            }
            DemoHud.SetStatus("probe",
                "<b>Kursor</b>\n" +
                $"ekran (piksele): ({px.x:0}; {px.y:0})   rozdzielczość {m_Cam.pixelWidth} × {m_Cam.pixelHeight}\n" +
                $"viewport (0..1): ({vp.x:0.000}; {vp.y:0.000})\n" +
                $"promień: początek ({ray.origin.x:0.0}; {ray.origin.y:0.0}; {ray.origin.z:0.0}), kierunek ({ray.direction.x:0.00}; {ray.direction.y:0.00}; {ray.direction.z:0.00})\n" +
                (m_HasHit
                    ? $"trafienie: {m_HitName} w ({hit.point.x:0.00}; {hit.point.y:0.00}; {hit.point.z:0.00}), odległość {hit.distance:0.0} m"
                    : "trafienie: brak"));
        }
    }
}
