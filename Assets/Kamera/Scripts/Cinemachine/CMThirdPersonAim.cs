using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 13: kamera zza ramienia – CinemachineThirdPersonFollow.
    /// PPM + mysz: obrót postaci (yaw) i pochylenie celownika (pitch) – kamera kopiuje obrót celu (Follow).
    /// Spacja – zmiana ramienia (Camera Side 0 ↔ 1). Kolumny i ściany są przeszkodami: kamera omija je
    /// dzięki wbudowanej opcji Avoid Obstacles.
    /// </summary>
    public class CMThirdPersonAim : DemoActionBase
    {
        public Transform body;
        public Transform aimTarget;
        public CinemachineThirdPersonFollow thirdPerson;
        public float sensitivity = 0.15f;
        public Vector2 pitchRange = new Vector2(-40f, 60f);

        float m_Yaw;
        float m_Pitch;
        float m_TargetSide = 1f;

        void Start()
        {
            m_Yaw = body.eulerAngles.y;
            m_TargetSide = thirdPerson.CameraSide;
        }

        public override void OnMainAction()
        {
            m_TargetSide = m_TargetSide > 0.5f ? 0f : 1f;
            DemoHud.Log("Camera Side → " + m_TargetSide + (m_TargetSide > 0.5f ? " (prawe ramię)" : " (lewe ramię)"));
        }

        void Update()
        {
            Vector2 look = DemoInput.LookWhileRightButton;
            m_Yaw += look.x * sensitivity;
            m_Pitch = Mathf.Clamp(m_Pitch - look.y * sensitivity, pitchRange.x, pitchRange.y);
            body.rotation = Quaternion.Euler(0f, m_Yaw, 0f);
            aimTarget.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);

            thirdPerson.CameraSide = Mathf.MoveTowards(thirdPerson.CameraSide, m_TargetSide, 2f * Time.deltaTime);

            var obstacle = thirdPerson.CurrentObstacle;
            DemoHud.SetStatus("tpp",
                $"<b>Third Person Follow</b>\nShoulder Offset = {thirdPerson.ShoulderOffset}, Vertical Arm Length = {thirdPerson.VerticalArmLength}\n" +
                $"Camera Side = {thirdPerson.CameraSide:0.00}, Camera Distance = {thirdPerson.CameraDistance}\n" +
                $"Avoid Obstacles: {(thirdPerson.AvoidObstacles.Enabled ? "tak" : "nie")}, przeszkoda: {(obstacle != null ? obstacle.name : "brak")}\n" +
                $"yaw = {m_Yaw:0}°, pitch = {m_Pitch:0}°");
        }
    }
}
