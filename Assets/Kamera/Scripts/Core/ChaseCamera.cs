using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Prosta kamera "pościgowa" bez Cinemachine: stały offset od celu (w układzie świata
    /// albo celu) i patrzenie na cel. Używana w scenach z wieloma kamerami (podział ekranu, minimapa).
    /// </summary>
    public class ChaseCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 6, -8);
        [Tooltip("Offset w układzie lokalnym celu (za plecami) zamiast w układzie świata")]
        public bool offsetInTargetSpace = false;
        [Tooltip("Patrz na cel (wyłącz np. dla minimapy, która ma stały obrót)")]
        public bool lookAtTarget = true;
        public float lookHeight = 1f;
        [Tooltip("Czas wygładzania [s] – 0 = sztywno")]
        public float smoothTime = 0.15f;

        Vector3 m_Velocity;

        void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 desired = offsetInTargetSpace
                ? target.position + Quaternion.Euler(0f, target.eulerAngles.y, 0f) * offset
                : target.position + offset;
            transform.position = smoothTime > 0f
                ? Vector3.SmoothDamp(transform.position, desired, ref m_Velocity, smoothTime)
                : desired;
            if (lookAtTarget)
                transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
