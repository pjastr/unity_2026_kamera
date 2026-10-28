using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 05: Culling Mask i warstwy (Layers) + minimapa jako druga kamera (Viewport Rect, wyższy priorytet).
    /// Ikony minimapy leżą na warstwie "Minimapa": widzi je tylko kamera minimapy.
    /// Spacja – dołącz / odłącz warstwę "Minimapa" w Culling Mask kamery głównej.
    /// LPM – minimapa: rzut ortogonalny ↔ perspektywiczny.
    /// </summary>
    public class CullingMaskDemo : DemoActionBase
    {
        public Camera mainCamera;
        public Camera minimapCamera;
        public string layerName = "Minimapa";

        int Bit => 1 << LayerMask.NameToLayer(layerName);

        public override void OnMainAction()
        {
            if (mainCamera == null || LayerMask.NameToLayer(layerName) < 0)
                return;
            mainCamera.cullingMask ^= Bit;
            DemoHud.Log($"Main Camera: warstwa \"{layerName}\" " + ((mainCamera.cullingMask & Bit) != 0 ? "WIDOCZNA" : "ukryta"));
        }

        public override void OnSecondaryAction()
        {
            if (minimapCamera == null) return;
            minimapCamera.orthographic = !minimapCamera.orthographic;
            DemoHud.Log("minimapa: " + (minimapCamera.orthographic ? "ortogonalna" : "perspektywiczna"));
        }

        void Update()
        {
            DemoHud.SetStatus("mask",
                "<b>Culling Mask</b>\n" +
                $"Main Camera: {Describe(mainCamera)}\n" +
                $"Minimapa: {Describe(minimapCamera)}\n" +
                (minimapCamera != null
                    ? $"Minimapa: Viewport Rect = ({minimapCamera.rect.x:0.##}; {minimapCamera.rect.y:0.##}; {minimapCamera.rect.width:0.##}; {minimapCamera.rect.height:0.##}), priorytet (depth) = {minimapCamera.depth}"
                    : ""));
        }

        static string Describe(Camera c)
        {
            if (c == null) return "-";
            if (c.cullingMask == ~0) return "Everything";
            string s = "";
            for (int i = 0; i < 32; i++)
            {
                if ((c.cullingMask & (1 << i)) == 0) continue;
                string n = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(n)) continue;
                s += (s.Length > 0 ? ", " : "") + n;
            }
            return s.Length > 0 ? s : "Nothing";
        }
    }
}
