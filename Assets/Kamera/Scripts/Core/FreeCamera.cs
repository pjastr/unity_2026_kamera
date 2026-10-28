using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Swobodna kamera sterowana akcjami z domyślnego Input Action:
    /// PPM + mysz – obrót, WASD – ruch, Shift – szybciej, kółko – przód/tył.
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        public float moveSpeed = 8f;
        public float sprintMultiplier = 3f;
        public float lookSensitivity = 0.15f;
        public float scrollStep = 1.5f;

        float m_Yaw;
        float m_Pitch;

        void Start()
        {
            var e = transform.eulerAngles;
            m_Yaw = e.y;
            m_Pitch = e.x > 180f ? e.x - 360f : e.x;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;

            Vector2 look = DemoInput.LookWhileRightButton;
            if (look.sqrMagnitude > 0f)
            {
                m_Yaw += look.x * lookSensitivity;
                m_Pitch = Mathf.Clamp(m_Pitch - look.y * lookSensitivity, -89f, 89f);
                transform.rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);
            }

            Vector2 move = DemoInput.ReadVector2(DemoInput.Move);
            float speed = moveSpeed * (DemoInput.IsPressed(DemoInput.Sprint) ? sprintMultiplier : 1f);
            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-4f) fwd = transform.up;
            fwd.Normalize();
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();
            transform.position += (fwd * move.y + right * move.x) * speed * dt;

            float s = DemoInput.ScrollStep;
            if (s != 0f)
                transform.position += transform.forward * s * scrollStep *
                                      (DemoInput.IsPressed(DemoInput.Sprint) ? sprintMultiplier : 1f);
        }
    }
}
