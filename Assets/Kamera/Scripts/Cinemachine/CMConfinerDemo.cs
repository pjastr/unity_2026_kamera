using Unity.Cinemachine;
using UnityEngine;

namespace Kamera
{
    /// <summary>
    /// Scena 16: kamera 2D (rzut ortogonalny) – Position Composer + Confiner 2D.
    /// Gracz porusza się w płaszczyźnie XY (WASD). Confiner 2D nie pozwala kamerze pokazać obszaru
    /// poza kształtem PolygonCollider2D (granice poziomu).
    /// Spacja – Confiner włączony / wyłączony. LPM – Lens.OrthographicSize 5 ↔ 8.
    /// </summary>
    public class CMConfinerDemo : DemoActionBase
    {
        public CinemachineCamera cmCamera;
        public CinemachineConfiner2D confiner;
        public CinemachinePositionComposer composer;
        public Transform player;
        public float speed = 6f;
        public Rect playerArea = new Rect(-20f, -4f, 40f, 16f);

        public override void OnMainAction()
        {
            confiner.enabled = !confiner.enabled;
            DemoHud.Log("Confiner 2D: " + (confiner.enabled ? "włączony" : "wyłączony"));
        }

        public override void OnSecondaryAction()
        {
            cmCamera.Lens.OrthographicSize = cmCamera.Lens.OrthographicSize > 6.5f ? 5f : 8f;
            confiner.InvalidateLensCache();
            DemoHud.Log($"Lens.OrthographicSize = {cmCamera.Lens.OrthographicSize}");
        }

        void Update()
        {
            Vector2 m = DemoInput.ReadVector2(DemoInput.Move);
            float s = speed * (DemoInput.IsPressed(DemoInput.Sprint) ? 1.8f : 1f);
            Vector3 p = player.position + new Vector3(m.x, m.y, 0f) * s * Time.deltaTime;
            p.x = Mathf.Clamp(p.x, playerArea.xMin, playerArea.xMax);
            p.y = Mathf.Clamp(p.y, playerArea.yMin, playerArea.yMax);
            player.position = p;

            var c = composer.Composition;
            DemoHud.SetStatus("2d",
                $"<b>Kamera 2D</b>: Lens.OrthographicSize = {cmCamera.Lens.OrthographicSize} (połowa wysokości kadru)\n" +
                $"Position Composer: Camera Distance = {composer.CameraDistance}, Dead Zone = {(c.DeadZone.Enabled ? c.DeadZone.Size.ToString() : "wył.")}, Damping = {composer.Damping}\n" +
                $"Confiner 2D: {(confiner.enabled ? "włączony" : "wyłączony")}, kamera przesunięta przez Confiner: {(confiner.enabled && confiner.CameraWasDisplaced(cmCamera) ? "tak" : "nie")}");
        }
    }
}
