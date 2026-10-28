using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Ruch "tam i z powrotem" między dwiema pozycjami z zatrzymaniem na końcach
    /// (np. kamera podchodząca do ściany w scenie Camera Stack).
    /// </summary>
    public class PingPongMover : MonoBehaviour
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public float travelTime = 3f;
        public float pauseTime = 1.5f;

        float m_T;

        void Update()
        {
            float cycle = 2f * (travelTime + pauseTime);
            m_T = (m_T + Time.deltaTime) % cycle;
            float k;
            if (m_T < travelTime) k = m_T / travelTime;
            else if (m_T < travelTime + pauseTime) k = 1f;
            else if (m_T < 2f * travelTime + pauseTime) k = 1f - (m_T - travelTime - pauseTime) / travelTime;
            else k = 0f;
            transform.position = Vector3.Lerp(pointA, pointB, Mathf.SmoothStep(0f, 1f, k));
        }
    }
}
