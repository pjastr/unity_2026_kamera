using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Automatyczny ruch obiektu po prostej trajektorii (cel dla kamer w scenach demonstracyjnych).
    /// Prędkość może się okresowo zmieniać, żeby było widać, jak kamera reaguje na przyspieszenia.
    /// </summary>
    public class AutoMover : MonoBehaviour
    {
        public enum PathShape { Circle, Figure8, Line, Lissajous, Square }

        public PathShape shape = PathShape.Circle;
        public Vector3 center = Vector3.zero;
        [Tooltip("Promień / połowa wymiaru trajektorii w osiach X i Z")]
        public Vector2 size = new Vector2(8f, 8f);
        [Tooltip("Prędkość kątowa parametru trajektorii [rad/s]")]
        public float speed = 0.5f;
        [Tooltip("0 = stała prędkość, 0.6 = wyraźne przyspieszanie i hamowanie")]
        [Range(0f, 0.9f)] public float speedVariation = 0f;
        public float phase = 0f;
        public bool faceDirection = true;
        [Tooltip("Wysokość podskoków (0 = brak)")]
        public float bobHeight = 0f;

        float m_T;

        void Start()
        {
            m_T = phase;
            transform.position = Evaluate(m_T);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float k = 1f + speedVariation * Mathf.Sin(Time.time * 0.7f);
            m_T += speed * k * dt;
            Vector3 p = Evaluate(m_T);
            Vector3 d = p - transform.position;
            transform.position = p;
            d.y = 0f;
            if (faceDirection && d.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 1f - Mathf.Exp(-10f * dt));
        }

        public Vector3 Evaluate(float t)
        {
            float x, z;
            switch (shape)
            {
                case PathShape.Figure8:
                    x = Mathf.Sin(t) * size.x;
                    z = Mathf.Sin(t) * Mathf.Cos(t) * size.y * 2f;
                    break;
                case PathShape.Line:
                    x = Mathf.Sin(t) * size.x;
                    z = 0f;
                    break;
                case PathShape.Lissajous:
                    x = Mathf.Sin(t) * size.x;
                    z = Mathf.Sin(2f * t + 0.5f) * size.y;
                    break;
                case PathShape.Square:
                {
                    // obwód kwadratu, parametr t w [0, 2π) → 4 boki
                    float u = Mathf.Repeat(t / (2f * Mathf.PI), 1f) * 4f;
                    int side = Mathf.FloorToInt(u);
                    float f = u - side;
                    Vector2 a, b;
                    switch (side)
                    {
                        case 0: a = new Vector2(-1, -1); b = new Vector2(1, -1); break;
                        case 1: a = new Vector2(1, -1); b = new Vector2(1, 1); break;
                        case 2: a = new Vector2(1, 1); b = new Vector2(-1, 1); break;
                        default: a = new Vector2(-1, 1); b = new Vector2(-1, -1); break;
                    }
                    Vector2 q = Vector2.Lerp(a, b, f);
                    x = q.x * size.x;
                    z = q.y * size.y;
                    break;
                }
                default:
                    x = Mathf.Cos(t) * size.x;
                    z = Mathf.Sin(t) * size.y;
                    break;
            }
            float y = bobHeight > 0f ? Mathf.Abs(Mathf.Sin(t * 6f)) * bobHeight : 0f;
            return center + new Vector3(x, y, z);
        }
    }
}
