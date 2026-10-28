using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 10: podstawy Cinemachine – CinemachineCamera + Follow (pozycja) + Rotation Composer (celowanie).
    /// Spacja – kolejny zestaw: sztywno / tłumienie (Damping) / martwa strefa (Dead Zone) / wyprzedzanie (Lookahead).
    /// LPM – Binding Mode: World Space ↔ Lock To Target With World Up (kamera za plecami celu).
    /// </summary>
    public class CMFollowPresets : DemoActionBase
    {
        public CinemachineFollow follow;
        public CinemachineRotationComposer composer;

        int m_Preset;
        static readonly string[] k_Names =
        {
            "1. sztywno – Damping = 0",
            "2. tłumienie – Position Damping = 1, Composer Damping = 0.5",
            "3. martwa strefa (Dead Zone 30% × 30%) + Hard Limits 80%",
            "4. wyprzedzanie ruchu – Lookahead 0.6 s",
        };

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Preset = (m_Preset + 1) % k_Names.Length;
            Apply();
            DemoHud.Log(k_Names[m_Preset]);
        }

        public override void OnSecondaryAction()
        {
            follow.TrackerSettings.BindingMode = follow.TrackerSettings.BindingMode == BindingMode.WorldSpace
                ? BindingMode.LockToTargetWithWorldUp
                : BindingMode.WorldSpace;
            DemoHud.Log("Binding Mode = " + follow.TrackerSettings.BindingMode);
        }

        void Apply()
        {
            var c = composer.Composition;
            c.ScreenPosition = Vector2.zero;
            c.DeadZone.Enabled = false;
            c.HardLimits.Enabled = false;
            composer.Lookahead.Enabled = false;
            switch (m_Preset)
            {
                case 0:
                    follow.TrackerSettings.PositionDamping = Vector3.zero;
                    composer.Damping = Vector2.zero;
                    break;
                case 1:
                    follow.TrackerSettings.PositionDamping = Vector3.one;
                    composer.Damping = new Vector2(0.5f, 0.5f);
                    break;
                case 2:
                    follow.TrackerSettings.PositionDamping = Vector3.one;
                    composer.Damping = new Vector2(0.5f, 0.5f);
                    c.DeadZone.Enabled = true;
                    c.DeadZone.Size = new Vector2(0.3f, 0.3f);
                    c.HardLimits.Enabled = true;
                    c.HardLimits.Size = new Vector2(0.8f, 0.8f);
                    break;
                default:
                    follow.TrackerSettings.PositionDamping = Vector3.one;
                    composer.Damping = new Vector2(0.5f, 0.5f);
                    composer.Lookahead.Enabled = true;
                    composer.Lookahead.Time = 0.6f;
                    composer.Lookahead.Smoothing = 5f;
                    composer.Lookahead.IgnoreY = true;
                    break;
            }
            composer.Composition = c;
        }

        void Update()
        {
            var t = follow.TrackerSettings;
            var c = composer.Composition;
            DemoHud.SetStatus("follow",
                $"<b>Zestaw:</b> {k_Names[m_Preset]}\n" +
                $"Follow: offset {follow.FollowOffset}, Binding Mode = {t.BindingMode}, Position Damping = {t.PositionDamping}\n" +
                $"Rotation Composer: Damping = {composer.Damping}, Dead Zone = {(c.DeadZone.Enabled ? c.DeadZone.Size.ToString() : "wył.")}, " +
                $"Lookahead = {(composer.Lookahead.Enabled ? composer.Lookahead.Time + " s" : "wył.")}");
        }
    }
}
