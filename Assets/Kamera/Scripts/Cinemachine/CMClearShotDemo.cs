using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 19: automatyczny wybór ujęcia – Cinemachine Clear Shot.
    /// Trzy stałe kamery (dzieci Clear Shot) mają Deoccluder, który ocenia jakość ujęcia (czy cel jest zasłonięty
    /// i jak daleko jest). Clear Shot włącza kamerę z najlepszym ujęciem.
    /// Spacja – Clear Shot ↔ tylko kamera nr 1 (bez wyboru – cel często znika za ścianami).
    /// LPM – Activate After 0 s ↔ 1 s (opóźnienie przełączenia, żeby nie "skakać" zbyt często).
    /// </summary>
    public class CMClearShotDemo : DemoActionBase
    {
        public CinemachineClearShot clearShot;
        public CinemachineCamera fixedCamera;
        public CinemachineCamera[] children;

        bool m_ClearShot = true;

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_ClearShot = !m_ClearShot;
            Apply();
        }

        public override void OnSecondaryAction()
        {
            clearShot.ActivateAfter = clearShot.ActivateAfter > 0.5f ? 0f : 1f;
            DemoHud.Log($"Activate After = {clearShot.ActivateAfter} s");
        }

        void Apply()
        {
            clearShot.Priority = m_ClearShot ? 20 : 5;
            fixedCamera.Priority = 10;
            DemoHud.Log(m_ClearShot ? "Clear Shot – automatyczny wybór ujęcia" : "tylko kamera stała nr 1");
        }

        void Update()
        {
            string s = $"<b>Clear Shot</b>: {(m_ClearShot ? "aktywny" : "wyłączony")}, Activate After = {clearShot.ActivateAfter} s, Min Duration = {clearShot.MinDuration} s";
            foreach (var c in children)
            {
                var d = c.GetComponent<CinemachineDeoccluder>();
                bool obscured = d != null && d.IsTargetObscured(c);
                s += $"\n{c.name}: cel {(obscured ? "<color=#ff8a80>zasłonięty</color>" : "<color=#7dff8a>widoczny</color>")}" +
                     (c.IsLive ? "  LIVE" : "");
            }
            DemoHud.SetStatus("clear", s);
        }
    }
}
