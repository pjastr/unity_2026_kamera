using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 18: drgania kamery.
    ///  • Szum ciągły – Basic Multi Channel Perlin z gotowymi profilami (Noise Settings) z pakietu Cinemachine.
    ///    Spacja – kolejny profil (brak / Handheld mild / Handheld strong / 6D Shake).
    ///  • Impuls – Cinemachine Impulse Source + Impulse Listener na kamerze: LPM – "wybuch" beczki.
    ///    Siła wstrząsu maleje z odległością od źródła (Impulse Type = Dissipating).
    /// </summary>
    public class CMNoiseImpulseDemo : DemoActionBase
    {
        public CinemachineBasicMultiChannelPerlin perlin;
        public NoiseSettings[] profiles;
        public string[] profileNames;
        public CinemachineImpulseSource impulseSource;
        public Renderer flash;
        public float impulseForce = 1.5f;

        int m_Index = -1;
        float m_Flash;
        Vector3 m_FlashScale = Vector3.one;

        void Start()
        {
            if (flash != null) m_FlashScale = flash.transform.localScale;
            SetProfile(-1);
        }

        public override void OnMainAction()
        {
            int next = m_Index + 1;
            if (next >= profiles.Length) next = -1;
            SetProfile(next);
        }

        void SetProfile(int i)
        {
            m_Index = i;
            if (i < 0 || profiles[i] == null)
            {
                perlin.NoiseProfile = null;
                DemoHud.Log("Noise Profile = brak");
            }
            else
            {
                perlin.NoiseProfile = profiles[i];
                perlin.AmplitudeGain = 1f;
                perlin.FrequencyGain = 1f;
                DemoHud.Log("Noise Profile = " + profileNames[i]);
            }
        }

        public override void OnSecondaryAction()
        {
            impulseSource.GenerateImpulseWithForce(impulseForce);
            m_Flash = 0.25f;
            DemoHud.Log($"GenerateImpulseWithForce({impulseForce}) – wybuch!");
        }

        void Update()
        {
            if (flash != null)
            {
                m_Flash -= Time.deltaTime;
                flash.enabled = m_Flash > 0f;
                if (flash.enabled)
                    flash.transform.localScale = m_FlashScale * (1f + (0.25f - m_Flash) * 16f);
            }
            var def = impulseSource.ImpulseDefinition;
            DemoHud.SetStatus("noise",
                $"<b>Noise</b> (Basic Multi Channel Perlin): {(m_Index < 0 ? "brak" : profileNames[m_Index])}, Amplitude Gain = {perlin.AmplitudeGain}, Frequency Gain = {perlin.FrequencyGain}\n" +
                $"<b>Impulse</b>: Shape = {def.ImpulseShape}, Type = {def.ImpulseType}, czas = {def.ImpulseDuration} s, zasięg tłumienia = {def.DissipationDistance} m");
        }
    }
}
