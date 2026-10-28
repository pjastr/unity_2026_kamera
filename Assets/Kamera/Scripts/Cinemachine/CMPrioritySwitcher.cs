using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 11: kilka kamer CinemachineCamera, aktywna jest ta o najwyższym priorytecie.
    /// Spacja – kolejna kamera (podnosimy jej Priority). LPM – styl domyślnego przejścia (Default Blend).
    /// Przejście "Góra → Przód" jest zdefiniowane osobno w Custom Blends (zawsze cięcie).
    /// </summary>
    public class CMPrioritySwitcher : DemoActionBase
    {
        public CinemachineBrain brain;
        public CinemachineCamera[] cameras;
        public int activePriority = 20;
        public int idlePriority = 10;

        int m_Index;
        int m_Blend;
        static readonly CinemachineBlendDefinition[] k_Blends =
        {
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.5f),
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Linear, 1.5f),
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.HardIn, 1.5f),
            new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f),
        };

        void Start()
        {
            brain.DefaultBlend = k_Blends[0];
            Apply();
        }

        public override void OnMainAction()
        {
            m_Index = (m_Index + 1) % cameras.Length;
            Apply();
        }

        public override void OnSecondaryAction()
        {
            m_Blend = (m_Blend + 1) % k_Blends.Length;
            brain.DefaultBlend = k_Blends[m_Blend];
            DemoHud.Log($"Default Blend = {k_Blends[m_Blend].Style}, {k_Blends[m_Blend].Time} s");
        }

        void Apply()
        {
            for (int i = 0; i < cameras.Length; i++)
                cameras[i].Priority = i == m_Index ? activePriority : idlePriority;
        }

        void Update()
        {
            string s = $"<b>Priority</b> (Spacja – następna kamera), Default Blend: {brain.DefaultBlend.Style} {brain.DefaultBlend.Time} s";
            foreach (var c in cameras)
                s += $"\n{c.name}: Priority = {c.Priority.Value}" + (c.IsLive ? "  <color=#7dff8a>LIVE</color>" : "");
            DemoHud.SetStatus("prio", s);
        }
    }
}
