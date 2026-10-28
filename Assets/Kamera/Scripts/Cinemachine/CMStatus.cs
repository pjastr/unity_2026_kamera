using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Pokazuje w panelu stanu, która kamera Cinemachine jest aktywna (live) i czy trwa przejście (blend).
    /// Dodawana do Main Camera razem z CinemachineBrain.
    /// </summary>
    [RequireComponent(typeof(CinemachineBrain))]
    public class CMStatus : MonoBehaviour
    {
        CinemachineBrain m_Brain;
        string m_LastName;

        void Awake() => m_Brain = GetComponent<CinemachineBrain>();

        void LateUpdate()
        {
            var cam = m_Brain.ActiveVirtualCamera;
            string name = cam != null ? cam.Name : "(brak)";
            if (name != m_LastName)
            {
                if (m_LastName != null)
                    DemoHud.Log("aktywna kamera: " + name);
                m_LastName = name;
            }
            string blend = m_Brain.IsBlending && m_Brain.ActiveBlend != null
                ? m_Brain.ActiveBlend.Description
                : "brak";
            var c = m_Brain.OutputCamera;
            string lens = c == null ? "" : c.orthographic
                ? $"Size = {c.orthographicSize:0.00}"
                : $"FOV = {c.fieldOfView:0.0}°";
            DemoHud.SetStatus("~cm",
                $"<b>CinemachineBrain</b>: aktywna kamera = <b>{name}</b>\nprzejście (blend): {blend}\nobiektyw Unity Camera: {lens}");
        }
    }
}
