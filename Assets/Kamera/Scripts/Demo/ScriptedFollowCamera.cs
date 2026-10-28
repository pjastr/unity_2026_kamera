using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 09: kamera podążająca za celem napisana ręcznie (bez Cinemachine).
    /// Spacja – kolejna metoda: dziecko celu, Lerp w Update, SmoothDamp w LateUpdate, stały punkt + LookAt.
    /// LPM – limit 20 FPS on/off: Lerp ze stałym współczynnikiem zależy od liczby klatek na sekundę,
    /// a SmoothDamp (lub Lerp z 1 - e^(-k·Δt)) – nie.
    /// </summary>
    public class ScriptedFollowCamera : DemoActionBase
    {
        public enum Method { ChildOfTarget, LerpInUpdate, SmoothDampInLateUpdate, FixedPointLookAt }

        public Transform target;
        public Method method = Method.ChildOfTarget;
        [Tooltip("Offset kamery w układzie celu (za plecami i wyżej)")]
        public Vector3 offset = new Vector3(0f, 3f, -7f);
        [Range(0.01f, 1f)] public float lerpFactor = 0.05f;
        public float smoothTime = 0.35f;
        public Vector3 fixedPoint = new Vector3(0f, 8f, -16f);

        Vector3 m_Vel;
        bool m_Limited;
        int m_OldTarget;
        int m_OldVSync;

        static readonly string[] k_Names =
        {
            "1. dziecko celu (transform.parent) – sztywno, każdy wstrząs celu widać w kamerze",
            "2. Vector3.Lerp(pos, cel, 0.05) w Update – zależy od FPS",
            "3. Vector3.SmoothDamp w LateUpdate – płynnie, niezależnie od FPS",
            "4. stały punkt + transform.LookAt(cel) – kamera \"monitoringu\"",
        };

        void Start()
        {
            m_OldTarget = Application.targetFrameRate;
            m_OldVSync = QualitySettings.vSyncCount;
            Enter();
        }

        void OnDestroy()
        {
            Application.targetFrameRate = m_OldTarget;
            QualitySettings.vSyncCount = m_OldVSync;
        }

        public override void OnMainAction()
        {
            transform.SetParent(null, true);
            method = (Method)(((int)method + 1) % 4);
            Enter();
            DemoHud.Log(k_Names[(int)method]);
        }

        public override void OnSecondaryAction()
        {
            m_Limited = !m_Limited;
            QualitySettings.vSyncCount = m_Limited ? 0 : m_OldVSync;
            Application.targetFrameRate = m_Limited ? 20 : m_OldTarget;
            DemoHud.Log(m_Limited ? "limit 20 FPS (Application.targetFrameRate = 20)" : "bez limitu FPS");
        }

        void Enter()
        {
            if (target == null) return;
            if (method == Method.ChildOfTarget)
            {
                transform.SetParent(target, false);
                transform.localPosition = offset;
                transform.localRotation = Quaternion.LookRotation(-offset + Vector3.up * 1.5f);
            }
            else if (method == Method.FixedPointLookAt)
            {
                transform.position = fixedPoint;
            }
        }

        Vector3 Desired => target.position + Quaternion.Euler(0f, target.eulerAngles.y, 0f) * offset;

        void Update()
        {
            if (target == null) return;
            if (method == Method.LerpInUpdate)
            {
                // ŹLE (celowo): stały współczynnik – przy 20 FPS kamera "zostaje" bardziej niż przy 144 FPS
                transform.position = Vector3.Lerp(transform.position, Desired, lerpFactor);
                transform.LookAt(target.position + Vector3.up * 1.5f);
            }
            DemoHud.SetStatus("follow",
                $"<b>Metoda:</b> {k_Names[(int)method]}\n" +
                $"FPS ≈ {1f / Mathf.Max(Time.smoothDeltaTime, 1e-4f):0}   odległość od celu = {Vector3.Distance(transform.position, target.position):0.00} m");
        }

        void LateUpdate()
        {
            if (target == null) return;
            if (method == Method.SmoothDampInLateUpdate)
            {
                transform.position = Vector3.SmoothDamp(transform.position, Desired, ref m_Vel, smoothTime);
                Quaternion look = Quaternion.LookRotation(target.position + Vector3.up * 1.5f - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-8f * Time.deltaTime));
            }
            else if (method == Method.FixedPointLookAt)
            {
                transform.LookAt(target.position + Vector3.up * 1.5f);
            }
        }
    }
}
