using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 01: przełączanie rzutowania perspektywicznego i ortogonalnego (Camera.orthographic).
    /// Przy przełączeniu dobieramy Size tak, aby obiekt w odległości focusDistance miał podobną wielkość:
    /// Size = d · tan(FOV / 2). W panelu stanu – macierz projekcji kamery na żywo.
    /// LPM – rzut ukośny (kabinetowy: głębokość skrócona o połowę, kąt 45°) zbudowany ręcznie z macierzy (Camera.projectionMatrix).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ProjectionToggle : DemoActionBase
    {
        [Tooltip("Odległość, w której obiekty mają mieć ten sam rozmiar w obu rzutach")]
        public float focusDistance = 14f;

        Camera m_Cam;
        bool m_Oblique;
        bool m_WasOrthographic;

        void Awake() => m_Cam = GetComponent<Camera>();

        public override void OnMainAction()
        {
            m_Cam.ResetProjectionMatrix();
            m_Oblique = false;
            m_Cam.orthographic = !m_Cam.orthographic;
            if (m_Cam.orthographic)
                m_Cam.orthographicSize = focusDistance * Mathf.Tan(0.5f * m_Cam.fieldOfView * Mathf.Deg2Rad);
            DemoHud.Log(m_Cam.orthographic
                ? $"orthographic = true, Size = {m_Cam.orthographicSize:0.00}"
                : $"orthographic = false, FOV = {m_Cam.fieldOfView:0}°");
        }

        public override void OnSecondaryAction()
        {
            // Rzut ukośny: do macierzy ortogonalnej dokładamy ścinanie (shear) zależne od głębokości.
            m_Oblique = !m_Oblique;
            if (!m_Oblique)
            {
                m_Cam.orthographic = m_WasOrthographic;
                m_Cam.ResetProjectionMatrix();
                DemoHud.Log("ResetProjectionMatrix()");
                return;
            }
            m_WasOrthographic = m_Cam.orthographic;
            m_Cam.orthographic = true;
            m_Cam.orthographicSize = focusDistance * Mathf.Tan(0.5f * m_Cam.fieldOfView * Mathf.Deg2Rad);
            m_Cam.ResetProjectionMatrix();
            Matrix4x4 p = m_Cam.projectionMatrix;
            Matrix4x4 shear = Matrix4x4.identity;
            const float k = 0.5f;               // rzut kabinetowy: głębokość skrócona o połowę (k = 1 – kawalerski)
            const float angle = 45f * Mathf.Deg2Rad;
            // przesunięcie x' = x + k·cos(α)·(z + d), y' = y + k·sin(α)·(z + d) – w przestrzeni kamery z < 0,
            // więc płaszczyzna w odległości d = focusDistance zostaje na miejscu, a reszta "przesuwa się" ukośnie
            shear[0, 2] = k * Mathf.Cos(angle);
            shear[1, 2] = k * Mathf.Sin(angle);
            shear[0, 3] = k * Mathf.Cos(angle) * focusDistance;
            shear[1, 3] = k * Mathf.Sin(angle) * focusDistance;
            m_Cam.projectionMatrix = p * shear;
            DemoHud.Log("projectionMatrix = ortho · shear (rzut ukośny kabinetowy)");
        }

        void Update()
        {
            Matrix4x4 m = m_Cam.projectionMatrix;
            string mode = m_Oblique ? "ukośne (własna macierz)" : (m_Cam.orthographic ? "ortogonalne" : "perspektywiczne");
            string par = m_Cam.orthographic
                ? $"Size = {m_Cam.orthographicSize:0.00} (połowa wysokości widoku w jednostkach)"
                : $"FOV (pion) = {m_Cam.fieldOfView:0.0}°";
            DemoHud.SetStatus("proj",
                $"<b>Rzutowanie:</b> {mode}\n{par}\naspect = {m_Cam.aspect:0.000}   near = {m_Cam.nearClipPlane}   far = {m_Cam.farClipPlane}\n" +
                "<b>Camera.projectionMatrix</b>\n" +
                Row(m, 0) + "\n" + Row(m, 1) + "\n" + Row(m, 2) + "\n" + Row(m, 3));
        }

        static string Row(Matrix4x4 m, int r) =>
            $"[ {m[r, 0],7:0.000}  {m[r, 1],7:0.000}  {m[r, 2],7:0.000}  {m[r, 3],7:0.000} ]";
    }
}
