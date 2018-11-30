using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CirrusPlay.PortalLibrary.Extensions;

namespace CirrusPlay.PortalLibrary.Examples {

    /// <summary>
    /// A class that attempts to coerce input into something which makes sense for this demo.
    /// </summary>
    public class Omniput : MonoBehaviour
    {
        private static readonly InputStateManager manager = new InputStateManager();
        private static readonly InputState activate;
        private static readonly InputState run;
        private static Vector3 moveVector;
        private static Vector3 lookVector;

        static Omniput()
        {
            activate = manager.Add("Activate", () => Input.GetButton("Fire1") || Input.GetAxis("Fire1") > 0.1f);
            run = manager.Add("Run", () => Input.GetButton("Fire3") || Input.GetAxis("Fire3") > 0.1f);
        }

        private void Update()
        {
            moveVector = Clamp(
                new Vector3(Flt(Input.GetKey(KeyCode.D)) - Flt(Input.GetKey(KeyCode.A)) + XInput.LeftStick.X(), 0, Flt(Input.GetKey(KeyCode.W)) - Flt(Input.GetKey(KeyCode.S)) + XInput.LeftStick.Y())
                );

            lookVector = new Vector3(-Mouse.Y(), Mouse.X());

            manager.Update();
        }

        public static Vector3 LookVector()
        {
            return lookVector;
        }
        public static Vector3 MoveVector()
        {
            return moveVector;
        }
        public static bool PressingRun() { return run.IsActive(); }
        public static bool PressActivate() { return activate.Activating(); }
        public static bool PressExit() { return Input.GetKeyDown(KeyCode.Escape); }

        private static Vector3 Clamp(Vector3 input) { return input.magnitude > 1.0f ? input.normalized : input; }
        private static float Flt(bool value) { return value ? 1.0f : 0.0f; }

        private static class Mouse
        {
            public static float X() { return Input.GetAxisRaw("Mouse X"); }
            public static float Y() { return Input.GetAxisRaw("Mouse Y"); }
        }

        private static class XInput
        {
            public static class LeftStick
            {
                public static float X() { return Input.GetAxis("Horizontal"); }
                public static float Y() { return Input.GetAxis("Vertical"); }
            }

            public static class RightStick
            {
                public static float X() { return Input.GetAxis("Mouse X"); }
                public static float Y() { return Input.GetAxis("Mouse Y"); }
            }

            public static float LeftTrigger() { return Input.GetAxisRaw("Fire1"); }
            public static float RightTrigger() { return Input.GetAxisRaw("Fire3"); }

        }

        delegate bool InputStatusPoller();
        private class InputState
        {
            public readonly string name;
            private InputStatusPoller poller;
            private bool state;
            private bool lastState;

            public InputState(string name, InputStatusPoller poller) { this.name = name;  this.poller = poller; }
            public bool IsActive() { return state; }
            public bool Activating() { return state != lastState && state; }
            public bool Activated() { return state != lastState && !state; }
            public void Update() {
                lastState = state;
                state = poller();
            }
        }
        private class InputStateManager
        {
            private readonly List<InputState> states = new List<InputState>();

            public InputState Add(string name, InputStatusPoller poller)
            {
                var result = new InputState(name, poller);
                states.Add(result);
                return result;
            }

            public void Update()
            {
                states.Each(state => state.Update());
           }
        }
    }
}