using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kamera
{
    /// <summary>
    /// Dostęp do akcji z domyślnego, projektowego pliku akcji Input System
    /// (Assets/InputSystem_Actions.inputactions, ustawionego jako "Project-wide Actions").
    /// Nie tworzymy własnych akcji – korzystamy wyłącznie z map "Player" i "UI".
    ///
    /// Player/Move      – WASD / strzałki
    /// Player/Look      – ruch myszy
    /// Player/Attack    – lewy przycisk myszy (LPM) / Enter
    /// Player/Interact  – E (przytrzymanie – interakcja "Hold")
    /// Player/Crouch    – C
    /// Player/Jump      – Spacja
    /// Player/Previous  – 1
    /// Player/Next      – 2
    /// Player/Sprint    – lewy Shift
    /// UI/Point         – pozycja kursora
    /// UI/RightClick    – prawy przycisk myszy (PPM)
    /// UI/ScrollWheel   – kółko myszy
    /// UI/Cancel        – Escape
    /// </summary>
    public static class DemoInput
    {
        static InputActionAsset s_Asset;
        static readonly Dictionary<string, InputAction> s_Cache = new Dictionary<string, InputAction>();
        static bool s_Warned;

        static bool Ensure()
        {
            if (s_Asset != null)
                return true;

            s_Cache.Clear();
            s_Asset = InputSystem.actions;
            if (s_Asset == null)
            {
                if (!s_Warned)
                {
                    s_Warned = true;
                    Debug.LogError("[Kamera] Brak 'Project-wide Actions'. Ustaw Assets/InputSystem_Actions.inputactions " +
                                   "w Edit > Project Settings > Input System Package (albo użyj menu Kamera > Napraw ustawienia projektu).");
                }
                return false;
            }

            if (!s_Asset.enabled)
                s_Asset.Enable();
            return true;
        }

        static InputAction Get(string path)
        {
            if (!Ensure())
                return null;
            if (s_Cache.TryGetValue(path, out var a) && a != null)
                return a;
            a = s_Asset.FindAction(path, false);
            if (a == null)
                Debug.LogWarning("[Kamera] Nie znaleziono akcji: " + path);
            else if (!a.enabled)
                a.Enable();
            s_Cache[path] = a;
            return a;
        }

        public static InputAction Move => Get("Player/Move");
        public static InputAction Look => Get("Player/Look");
        public static InputAction Attack => Get("Player/Attack");
        public static InputAction Interact => Get("Player/Interact");
        public static InputAction Crouch => Get("Player/Crouch");
        public static InputAction Jump => Get("Player/Jump");
        public static InputAction Previous => Get("Player/Previous");
        public static InputAction Next => Get("Player/Next");
        public static InputAction Sprint => Get("Player/Sprint");
        public static InputAction Point => Get("UI/Point");
        public static InputAction RightClick => Get("UI/RightClick");
        public static InputAction ScrollWheel => Get("UI/ScrollWheel");
        public static InputAction Cancel => Get("UI/Cancel");

        public static bool PressedThisFrame(InputAction a) => a != null && a.WasPressedThisFrame();
        public static bool PerformedThisFrame(InputAction a) => a != null && a.WasPerformedThisFrame();
        public static bool IsPressed(InputAction a) => a != null && a.IsPressed();

        public static Vector2 ReadVector2(InputAction a) => a != null ? a.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Ruch myszy tylko przy wciśniętym PPM (obrót kamery / postaci).</summary>
        public static Vector2 LookWhileRightButton =>
            IsPressed(RightClick) ? ReadVector2(Look) : Vector2.zero;

        /// <summary>Kółko myszy: +1 (do przodu), -1 (do tyłu), 0 (brak).</summary>
        public static float ScrollStep
        {
            get
            {
                float y = ReadVector2(ScrollWheel).y;
                return Mathf.Abs(y) > 0.01f ? Mathf.Sign(y) : 0f;
            }
        }

        public static Vector2 PointerPosition
        {
            get
            {
                var p = Point;
                if (p != null)
                    return p.ReadValue<Vector2>();
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            }
        }
    }
}
