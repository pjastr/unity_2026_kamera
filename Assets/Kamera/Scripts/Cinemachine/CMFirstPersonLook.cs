using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 14: kamera z pierwszej osoby – Hard Lock To Target (pozycja głowy) + Pan Tilt (obrót z osi wejścia).
    /// PPM + mysz zmieniają osie Pan (yaw) i Tilt (pitch); ciało postaci obraca się razem z osią Pan,
    /// więc WASD porusza w kierunku patrzenia.
    /// Spacja – FOV obiektywu 60° ↔ 90° (Lens.FieldOfView). LPM – kołysanie kamery przy chodzeniu (szum Perlina).
    /// </summary>
    public class CMFirstPersonLook : DemoActionBase
    {
        public CinemachineCamera cmCamera;
        public CinemachinePanTilt panTilt;
        public CinemachineBasicMultiChannelPerlin bob;
        public Transform body;
        public PlayerMover mover;
        public float sensitivity = 0.15f;

        bool m_Bob = true;

        public override void OnMainAction()
        {
            cmCamera.Lens.FieldOfView = cmCamera.Lens.FieldOfView > 75f ? 60f : 90f;
            DemoHud.Log($"Lens.FieldOfView = {cmCamera.Lens.FieldOfView:0}°");
        }

        public override void OnSecondaryAction()
        {
            m_Bob = !m_Bob;
            DemoHud.Log("kołysanie przy chodzeniu: " + (m_Bob ? "tak" : "nie"));
        }

        void Update()
        {
            Vector2 look = DemoInput.LookWhileRightButton;
            if (look.sqrMagnitude > 0f)
            {
                panTilt.PanAxis.Value = panTilt.PanAxis.ClampValue(panTilt.PanAxis.Value + look.x * sensitivity);
                panTilt.TiltAxis.Value = panTilt.TiltAxis.ClampValue(panTilt.TiltAxis.Value - look.y * sensitivity);
            }
            body.rotation = Quaternion.Euler(0f, panTilt.PanAxis.Value, 0f);

            if (bob != null)
            {
                float speed = mover != null ? new Vector2(mover.Velocity.x, mover.Velocity.z).magnitude : 0f;
                float target = m_Bob ? Mathf.Clamp01(speed / 6f) * 1.2f : 0f;
                bob.AmplitudeGain = Mathf.MoveTowards(bob.AmplitudeGain, target, 3f * Time.deltaTime);
                bob.FrequencyGain = 1f + Mathf.Clamp01(speed / 6f);
            }

            DemoHud.SetStatus("fpp",
                $"<b>Pan Tilt</b>: Pan = {panTilt.PanAxis.Value:0.0}°, Tilt = {panTilt.TiltAxis.Value:0.0}° (zakres {panTilt.TiltAxis.Range.x}..{panTilt.TiltAxis.Range.y})\n" +
                $"Lens.FieldOfView = {cmCamera.Lens.FieldOfView:0}°, Near = {cmCamera.Lens.NearClipPlane}\n" +
                (bob != null ? $"szum (Amplitude Gain) = {bob.AmplitudeGain:0.00}" : ""));
        }
    }
}
