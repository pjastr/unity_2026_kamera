using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 15: widok z góry na grupę jednostek – Position Composer + Target Group + Group Framing.
    /// Spacja – cel kamery: cała grupa (CinemachineTargetGroup) ↔ jedna jednostka.
    /// LPM – sposób dopasowania kadru do grupy (Size Adjustment): Zoom Only / Dolly Only / Dolly Then Zoom.
    /// </summary>
    public class CMGroupDemo : DemoActionBase
    {
        public CinemachineCamera cmCamera;
        public CinemachineTargetGroup group;
        public CinemachineGroupFraming framing;
        public Transform singleUnit;

        bool m_Group = true;

        public override void OnMainAction()
        {
            m_Group = !m_Group;
            cmCamera.Follow = m_Group ? group.transform : singleUnit;
            DemoHud.Log("Tracking Target = " + (m_Group ? "Target Group (" + group.Targets.Count + " celów)" : singleUnit.name));
        }

        public override void OnSecondaryAction()
        {
            framing.SizeAdjustment = (CinemachineGroupFraming.SizeAdjustmentModes)(((int)framing.SizeAdjustment + 1) % 3);
            DemoHud.Log("Size Adjustment = " + framing.SizeAdjustment);
        }

        void Update()
        {
            var b = group.BoundingBox;
            DemoHud.SetStatus("group",
                $"<b>Target Group</b>: {group.Targets.Count} celów, środek ({group.transform.position.x:0.0}; {group.transform.position.z:0.0}), rozmiar {b.size.x:0.0} × {b.size.z:0.0} m\n" +
                $"Tracking Target = {(m_Group ? "grupa" : singleUnit.name)}\n" +
                $"Group Framing: Framing Size = {framing.FramingSize:0.00}, Size Adjustment = {framing.SizeAdjustment}");
        }
    }
}
