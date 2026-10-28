using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 12: kamera orbitalna (dawny FreeLook) – CinemachineOrbitalFollow + Rotation Composer.
    /// Osie wejścia (InputAxis) sterujemy sami z domyślnych akcji: PPM + mysz – obrót (Horizontal/Vertical Axis),
    /// kółko – przybliżenie (Radial Axis). Po 2 s bez ruchu myszy działa automatyczne centrowanie (Recentering).
    /// Spacja – Orbit Style: Sphere ↔ Three Ring.
    /// </summary>
    public class CMOrbitInput : DemoActionBase
    {
        public CinemachineOrbitalFollow orbital;
        public float sensitivity = 0.2f;
        public float zoomStep = 0.15f;

        public override void OnMainAction()
        {
            orbital.OrbitStyle = orbital.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere
                ? CinemachineOrbitalFollow.OrbitStyles.ThreeRing
                : CinemachineOrbitalFollow.OrbitStyles.Sphere;
            DemoHud.Log("Orbit Style = " + orbital.OrbitStyle);
        }

        void Update()
        {
            Vector2 look = DemoInput.LookWhileRightButton;
            if (look.sqrMagnitude > 0f)
            {
                orbital.HorizontalAxis.Value = orbital.HorizontalAxis.ClampValue(orbital.HorizontalAxis.Value + look.x * sensitivity);
                orbital.VerticalAxis.Value = orbital.VerticalAxis.ClampValue(orbital.VerticalAxis.Value - look.y * sensitivity);
            }
            float s = DemoInput.ScrollStep;
            if (s != 0f)
                orbital.RadialAxis.Value = orbital.RadialAxis.ClampValue(orbital.RadialAxis.Value - s * zoomStep);

            var o = orbital.Orbits;
            DemoHud.SetStatus("orbit",
                $"<b>Orbital Follow</b> – Orbit Style = {orbital.OrbitStyle}\n" +
                $"Horizontal Axis = {orbital.HorizontalAxis.Value:0.0}° (zakres {orbital.HorizontalAxis.Range.x}..{orbital.HorizontalAxis.Range.y}, Wrap)\n" +
                $"Vertical Axis = {orbital.VerticalAxis.Value:0.00} (zakres {orbital.VerticalAxis.Range.x}..{orbital.VerticalAxis.Range.y})\n" +
                $"Radial Axis = {orbital.RadialAxis.Value:0.00} (mnożnik odległości)\n" +
                (orbital.OrbitStyle == CinemachineOrbitalFollow.OrbitStyles.Sphere
                    ? $"Radius = {orbital.Radius}"
                    : $"Three Ring: Top h={o.Top.Height} r={o.Top.Radius}, Center h={o.Center.Height} r={o.Center.Radius}, Bottom h={o.Bottom.Height} r={o.Bottom.Radius}"));
        }
    }
}
