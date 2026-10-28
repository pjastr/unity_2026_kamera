using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Prosta postać na CharacterController: WASD – ruch, Shift – bieg.
    /// Kierunek ruchu liczony względem kamery (gra 3-osobowa) albo względem samej postaci
    /// (FPP / kamera zza ramienia). Grawitację liczymy sami.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMover : MonoBehaviour
    {
        public enum Relative { Camera, Self }

        public Relative relativeTo = Relative.Camera;
        public float speed = 4.5f;
        public float sprintSpeed = 8f;
        [Tooltip("Obracaj postać w kierunku ruchu (tylko Relative = Camera)")]
        public bool rotateTowardsMove = true;
        [Tooltip("Kamera odniesienia (puste = Camera.main)")]
        public Transform cameraTransform;

        CharacterController m_Controller;
        float m_VerticalSpeed;

        public Vector3 Velocity { get; private set; }

        void Awake() => m_Controller = GetComponent<CharacterController>();

        void Update()
        {
            Vector2 input = DemoInput.ReadVector2(DemoInput.Move);
            Vector3 fwd, right;
            if (relativeTo == Relative.Camera)
            {
                Transform cam = cameraTransform != null ? cameraTransform : (Camera.main != null ? Camera.main.transform : transform);
                fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
                if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.ProjectOnPlane(cam.up, Vector3.up);
                fwd.Normalize();
                right = Vector3.Cross(Vector3.up, fwd);
            }
            else
            {
                fwd = transform.forward;
                right = transform.right;
            }

            Vector3 move = fwd * input.y + right * input.x;
            if (move.sqrMagnitude > 1f) move.Normalize();
            float s = DemoInput.IsPressed(DemoInput.Sprint) ? sprintSpeed : speed;

            if (relativeTo == Relative.Camera && rotateTowardsMove && move.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 1f - Mathf.Exp(-12f * Time.deltaTime));

            if (m_Controller.isGrounded && m_VerticalSpeed < 0f)
                m_VerticalSpeed = -2f;
            m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

            Velocity = move * s + Vector3.up * m_VerticalSpeed;
            m_Controller.Move(Velocity * Time.deltaTime);
        }
    }
}
