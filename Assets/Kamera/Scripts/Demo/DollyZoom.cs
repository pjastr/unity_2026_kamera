using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 02: kąt widzenia (Field of View) i efekt "dolly zoom" (Vertigo).
    /// Kamera odjeżdża od obiektu, a FOV maleje tak, by obiekt o wysokości H zajmował stałą część kadru:
    /// FOV = 2 · atan( H / (2 · d) ).
    /// Kółko myszy – ręczna zmiana FOV (zoom), Spacja – dolly zoom, LPM – Physical Camera on/off.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DollyZoom : DemoActionBase
    {
        public Transform subject;
        [Tooltip("Wysokość kadru [m], która ma pozostać stała w odległości obiektu")]
        public float frameHeight = 4f;
        public float minDistance = 5f;
        public float maxDistance = 30f;
        [Tooltip("Czas przejazdu w jedną stronę [s]")]
        public float travelTime = 4f;
        public float heightAboveSubject = 1f;

        Camera m_Cam;
        bool m_Active;
        float m_T;
        Vector3 m_Dir;

        void Awake()
        {
            m_Cam = GetComponent<Camera>();
            m_Cam.sensorSize = new Vector2(36f, 24f); // matryca pełnoklatkowa 36 × 24 mm
            m_Cam.gateFit = Camera.GateFitMode.Vertical; // FOV pionowy wynika z wysokości matrycy
        }

        void Start()
        {
            if (subject != null)
            {
                Vector3 d = transform.position - (subject.position + Vector3.up * heightAboveSubject);
                d.y = 0f;
                m_Dir = d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.back;
            }
        }

        public override void OnMainAction()
        {
            m_Active = !m_Active;
            DemoHud.Log(m_Active ? "dolly zoom: start" : "dolly zoom: stop");
        }

        public override void OnSecondaryAction()
        {
            m_Cam.usePhysicalProperties = !m_Cam.usePhysicalProperties;
            if (m_Cam.usePhysicalProperties)
                m_Cam.focalLength = Camera.FieldOfViewToFocalLength(m_Cam.fieldOfView, m_Cam.sensorSize.y);
            DemoHud.Log("usePhysicalProperties = " + m_Cam.usePhysicalProperties);
        }

        void Update()
        {
            if (subject == null)
                return;
            Vector3 focus = subject.position + Vector3.up * heightAboveSubject;

            if (m_Active)
            {
                m_T += Time.deltaTime / Mathf.Max(0.1f, travelTime);
                float k = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(m_T, 1f));
                float d = Mathf.Lerp(minDistance, maxDistance, k);
                transform.position = focus + m_Dir * d;
                transform.rotation = Quaternion.LookRotation(focus - transform.position);
                SetFov(2f * Mathf.Atan(frameHeight / (2f * d)) * Mathf.Rad2Deg);
            }
            else
            {
                float s = DemoInput.ScrollStep;
                if (s != 0f)
                    SetFov(Mathf.Clamp(m_Cam.fieldOfView - s * 5f, 5f, 120f));
            }

            float dist = Vector3.Distance(transform.position, focus);
            float vfov = m_Cam.fieldOfView;
            float hfov = Camera.VerticalToHorizontalFieldOfView(vfov, m_Cam.aspect);
            float f = Camera.FieldOfViewToFocalLength(vfov, m_Cam.sensorSize.y);
            float visibleHeight = 2f * dist * Mathf.Tan(0.5f * vfov * Mathf.Deg2Rad);
            DemoHud.SetStatus("fov",
                $"<b>Field of View</b> (oś pionowa) = {vfov:0.0}°\n" +
                $"FOV poziomy = {hfov:0.0}°  (aspect {m_Cam.aspect:0.00})\n" +
                $"odległość od obiektu d = {dist:0.0} m\n" +
                $"widoczna wysokość w odl. d: 2·d·tan(FOV/2) = {visibleHeight:0.00} m\n" +
                $"ogniskowa (matryca 24 mm): f = {f:0.0} mm\n" +
                $"Physical Camera: {(m_Cam.usePhysicalProperties ? "tak" : "nie")}");
        }

        void SetFov(float fov)
        {
            if (m_Cam.usePhysicalProperties)
                m_Cam.focalLength = Camera.FieldOfViewToFocalLength(fov, m_Cam.sensorSize.y);
            else
                m_Cam.fieldOfView = fov;
        }
    }
}
