using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 04: Viewport Rect – kilka kamer na jednym ekranie.
    /// Spacja – kolejny układ: pełny ekran, podział pionowy (2 graczy), podział poziomy,
    /// cztery widoki jak w programie do modelowania (góra / przód / bok – rzuty ortogonalne + perspektywa).
    /// </summary>
    public class ViewportLayouts : DemoActionBase
    {
        public Camera playerA;
        public Camera playerB;
        public Camera top;
        public Camera front;
        public Camera side;

        int m_Layout;
        static readonly string[] k_Names =
        {
            "1 kamera – pełny ekran",
            "podział pionowy (gracz A | gracz B)",
            "podział poziomy (gracz A nad graczem B)",
            "4 widoki: góra, przód, bok (ortogonalne) + perspektywa",
        };

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Layout = (m_Layout + 1) % k_Names.Length;
            Apply();
            DemoHud.Log("układ: " + k_Names[m_Layout]);
        }

        void Apply()
        {
            Set(playerA, false, default);
            Set(playerB, false, default);
            Set(top, false, default);
            Set(front, false, default);
            Set(side, false, default);
            switch (m_Layout)
            {
                case 0:
                    Set(playerA, true, new Rect(0, 0, 1, 1));
                    break;
                case 1:
                    Set(playerA, true, new Rect(0, 0, 0.5f, 1));
                    Set(playerB, true, new Rect(0.5f, 0, 0.5f, 1));
                    break;
                case 2:
                    Set(playerA, true, new Rect(0, 0.5f, 1, 0.5f));
                    Set(playerB, true, new Rect(0, 0, 1, 0.5f));
                    break;
                default:
                    Set(top, true, new Rect(0, 0.5f, 0.5f, 0.5f));
                    Set(playerA, true, new Rect(0.5f, 0.5f, 0.5f, 0.5f));
                    Set(front, true, new Rect(0, 0, 0.5f, 0.5f));
                    Set(side, true, new Rect(0.5f, 0, 0.5f, 0.5f));
                    break;
            }
        }

        static void Set(Camera c, bool on, Rect r)
        {
            if (c == null) return;
            c.enabled = on;
            if (on) c.rect = r;
        }

        void Update()
        {
            string s = $"<b>Viewport Rect</b> – układ {m_Layout + 1}/{k_Names.Length}: {k_Names[m_Layout]}";
            foreach (var c in new[] { playerA, playerB, top, front, side })
            {
                if (c == null || !c.enabled) continue;
                Rect r = c.rect;
                s += $"\n{c.name}: X={r.x:0.##} Y={r.y:0.##} W={r.width:0.##} H={r.height:0.##}" +
                     (c.orthographic ? "  (ortho)" : "");
            }
            DemoHud.SetStatus("vp", s);
        }
    }
}
