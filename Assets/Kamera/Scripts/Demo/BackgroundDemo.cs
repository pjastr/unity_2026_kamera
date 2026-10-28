using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 07: tło kamery (URP: Environment → Background Type) i Render Texture.
    /// Spacja – kolejny typ tła: Skybox → Solid Color → Solid Color (inny kolor) → Uninitialized.
    /// LPM – kamera monitoringu widzi / nie widzi warstwy "TylkoMonitor".
    /// Kamera monitoringu renderuje do tekstury (Camera.targetTexture), którą wyświetla ekran w scenie.
    /// </summary>
    public class BackgroundDemo : DemoActionBase
    {
        public Camera mainCamera;
        public Camera monitorCamera;
        public Renderer monitorScreen;
        public Vector2Int textureSize = new Vector2Int(640, 360);
        public string monitorOnlyLayer = "TylkoMonitor";
        [Tooltip("Zakres obrotu kamery monitoringu wokół osi Y [°]")]
        public float panAngle = 35f;
        public float panSpeed = 0.4f;

        RenderTexture m_Rt;
        int m_Mode;
        Quaternion m_MonitorBaseRot;

        static readonly string[] k_Modes =
        {
            "Skybox (CameraClearFlags.Skybox)",
            "Solid Color – granat (CameraClearFlags.SolidColor)",
            "Solid Color – zielone tło (green screen)",
            "Uninitialized (CameraClearFlags.Nothing) – zawartość bufora niezdefiniowana",
        };

        void Start()
        {
            m_Rt = new RenderTexture(textureSize.x, textureSize.y, 24) { name = "RT_Monitoring" };
            monitorCamera.targetTexture = m_Rt;
            monitorCamera.aspect = (float)textureSize.x / textureSize.y;
            var mat = monitorScreen.material; // kopia materiału tylko dla tego ekranu
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", m_Rt);
            mat.mainTexture = m_Rt;
            m_MonitorBaseRot = monitorCamera.transform.rotation;
            Apply();
        }

        void OnDestroy()
        {
            if (monitorCamera != null) monitorCamera.targetTexture = null;
            if (m_Rt != null)
            {
                m_Rt.Release();
                Destroy(m_Rt);
            }
        }

        public override void OnMainAction()
        {
            m_Mode = (m_Mode + 1) % k_Modes.Length;
            Apply();
            DemoHud.Log("tło: " + k_Modes[m_Mode]);
        }

        public override void OnSecondaryAction()
        {
            int bit = 1 << LayerMask.NameToLayer(monitorOnlyLayer);
            monitorCamera.cullingMask ^= bit;
            DemoHud.Log("kamera monitoringu – warstwa TylkoMonitor: " + ((monitorCamera.cullingMask & bit) != 0 ? "widoczna" : "ukryta"));
        }

        void Apply()
        {
            switch (m_Mode)
            {
                case 0:
                    mainCamera.clearFlags = CameraClearFlags.Skybox;
                    break;
                case 1:
                    mainCamera.clearFlags = CameraClearFlags.SolidColor;
                    mainCamera.backgroundColor = new Color(0.08f, 0.1f, 0.18f);
                    break;
                case 2:
                    mainCamera.clearFlags = CameraClearFlags.SolidColor;
                    mainCamera.backgroundColor = new Color(0f, 0.8f, 0.2f);
                    break;
                default:
                    mainCamera.clearFlags = CameraClearFlags.Nothing;
                    break;
            }
        }

        void Update()
        {
            float a = Mathf.Sin(Time.time * panSpeed) * panAngle;
            monitorCamera.transform.rotation = Quaternion.Euler(0f, a, 0f) * m_MonitorBaseRot;
            DemoHud.SetStatus("bg",
                $"<b>Background Type</b>: {k_Modes[m_Mode]}\n" +
                $"Render Texture: {m_Rt.width} × {m_Rt.height}, głębia {m_Rt.depth} bit\n" +
                $"kamera monitoringu: FOV {monitorCamera.fieldOfView:0}°, obrót {a:0}°");
        }
    }
}
