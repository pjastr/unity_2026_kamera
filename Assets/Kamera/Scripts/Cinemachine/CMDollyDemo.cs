using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 17: kamera na torze (dolly) – Spline Dolly + Splines.
    /// Wagonik jeździ po wewnętrznym torze (CinemachineSplineCart). Kamera jedzie po zewnętrznym torze.
    /// Spacja – tryb automatycznego dolly kamery:
    ///  • Fixed Speed – przejazd ze stałą prędkością, kamera patrzy na pomnik w środku,
    ///  • Nearest Point To Target – kamera ustawia się w punkcie toru najbliższym wagonikowi i patrzy na niego.
    /// LPM – Camera Rotation: Default (obrót z Rotation Composer) ↔ Spline (obrót z kierunku toru, Composer wyłączony).
    /// </summary>
    public class CMDollyDemo : DemoActionBase
    {
        public CinemachineCamera cmCamera;
        public CinemachineSplineDolly dolly;
        public CinemachineRotationComposer composer;
        public Transform monument;
        public Transform cart;
        public float tourSpeed = 4f;

        bool m_Tracking;

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Tracking = !m_Tracking;
            Apply();
        }

        public override void OnSecondaryAction()
        {
            dolly.CameraRotation = dolly.CameraRotation == CinemachineSplineDolly.RotationMode.Default
                ? CinemachineSplineDolly.RotationMode.Spline
                : CinemachineSplineDolly.RotationMode.Default;
            // Rotation Composer (etap Aim) nadpisałby obrót z toru – w trybie Spline go wyłączamy
            if (composer != null)
                composer.enabled = dolly.CameraRotation == CinemachineSplineDolly.RotationMode.Default;
            DemoHud.Log("Camera Rotation = " + dolly.CameraRotation);
        }

        void Apply()
        {
            dolly.AutomaticDolly.Enabled = true;
            if (m_Tracking)
            {
                dolly.AutomaticDolly.Method = new SplineAutoDolly.NearestPointToTarget();
                cmCamera.Follow = cart;
                cmCamera.LookAt = cart;
            }
            else
            {
                dolly.AutomaticDolly.Method = new SplineAutoDolly.FixedSpeed { Speed = tourSpeed };
                cmCamera.Follow = null;
                cmCamera.LookAt = monument;
            }
            dolly.AutomaticDolly.Method.Reset();
            DemoHud.Log("Automatic Dolly = " + (m_Tracking ? "Nearest Point To Target (śledzi wagonik)" : $"Fixed Speed ({tourSpeed} m/s)"));
        }

        void Update()
        {
            DemoHud.SetStatus("dolly",
                $"<b>Spline Dolly</b>: tryb = {(m_Tracking ? "Nearest Point To Target" : "Fixed Speed")}\n" +
                $"Camera Position = {dolly.CameraPosition:0.00} ({dolly.PositionUnits}), Camera Rotation = {dolly.CameraRotation}\n" +
                $"Look At = {(cmCamera.LookAt != null ? cmCamera.LookAt.name : "-")}");
        }
    }
}
