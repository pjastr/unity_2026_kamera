using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Kamera
{
    /// <summary>
    /// Scena 06: stos kamer URP (Camera Stack): kamera bazowa (Base) + nakładkowa (Overlay).
    /// Broń z pierwszej osoby leży na warstwie "Bron".
    ///  • tryb A – jedna kamera: broń rysuje kamera bazowa, więc przy ścianie "wchodzi" w ścianę,
    ///  • tryb B – stos: kamera bazowa pomija warstwę "Bron", a rysuje ją kamera Overlay
    ///    po wyczyszczeniu bufora głębokości – broń jest zawsze na wierzchu, może mieć własny FOV.
    /// Spacja – przełącz tryb. LPM – FOV kamery Overlay 40° ↔ 60°.
    /// </summary>
    public class CameraStackDemo : DemoActionBase
    {
        public Camera baseCamera;
        public Camera overlayCamera;
        public string weaponLayer = "Bron";

        bool m_Stack = false;

        int Bit => 1 << LayerMask.NameToLayer(weaponLayer);

        void Start()
        {
            var od = overlayCamera.GetUniversalAdditionalCameraData();
            od.renderType = CameraRenderType.Overlay;
            var bd = baseCamera.GetUniversalAdditionalCameraData();
            var stack = bd.cameraStack;
            if (stack != null && !stack.Contains(overlayCamera))
                stack.Add(overlayCamera);
            Apply();
        }

        public override void OnMainAction()
        {
            m_Stack = !m_Stack;
            Apply();
            DemoHud.Log(m_Stack ? "tryb B: Base + Overlay (Camera Stack)" : "tryb A: tylko kamera Base");
        }

        public override void OnSecondaryAction()
        {
            overlayCamera.fieldOfView = overlayCamera.fieldOfView > 50f ? 40f : 60f;
            DemoHud.Log($"Overlay FOV = {overlayCamera.fieldOfView:0}°");
        }

        void Apply()
        {
            if (m_Stack)
            {
                baseCamera.cullingMask &= ~Bit;
                overlayCamera.cullingMask = Bit;
                overlayCamera.enabled = true;
            }
            else
            {
                baseCamera.cullingMask |= Bit;
                overlayCamera.enabled = false;
            }
        }

        void Update()
        {
            var bd = baseCamera.GetUniversalAdditionalCameraData();
            var od = overlayCamera.GetUniversalAdditionalCameraData();
            DemoHud.SetStatus("stack",
                "<b>Camera Stack (URP)</b>\n" +
                $"tryb: {(m_Stack ? "B – Base + Overlay" : "A – jedna kamera")}\n" +
                $"{baseCamera.name}: Render Type = {bd.renderType}, FOV = {baseCamera.fieldOfView:0}°, warstwa Bron: {((baseCamera.cullingMask & Bit) != 0 ? "tak" : "nie")}\n" +
                $"{overlayCamera.name}: Render Type = {od.renderType}, FOV = {overlayCamera.fieldOfView:0}°, aktywna: {(overlayCamera.enabled ? "tak" : "nie")}");
        }
    }
}
