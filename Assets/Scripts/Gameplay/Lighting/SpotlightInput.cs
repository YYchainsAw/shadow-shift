using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowShift
{
    internal sealed class SpotlightInput
    {
        private Mouse draggingMouse;
        private bool waitForMouseRelease;

        public SpotlightInput(SpotlightRail spotlight)
        {
            ControlledSpotlight = spotlight;
        }

        public SpotlightRail ControlledSpotlight { get; private set; }

        public void Select(SpotlightRail spotlight, Mouse mouse)
        {
            ControlledSpotlight = spotlight;
            Reset(mouse);
        }

        public void Update(Mouse mouse)
        {
            if (mouse == null)
            {
                Reset(null);
                return;
            }

            if (waitForMouseRelease)
            {
                waitForMouseRelease = mouse.leftButton.isPressed || mouse.rightButton.isPressed;
                return;
            }

            Vector2 pointer = mouse.position.ReadValue();
            if (mouse.leftButton.isPressed)
            {
                if (draggingMouse != mouse || mouse.leftButton.wasPressedThisFrame)
                    ControlledSpotlight.BeginControl(pointer);
                else
                    ControlledSpotlight.MovePointer(pointer);
                draggingMouse = mouse;
            }
            else
                draggingMouse = null;

            if (mouse.rightButton.isPressed)
                ControlledSpotlight.AimPointer(pointer);
        }

        public void Reset(Mouse mouse)
        {
            draggingMouse = null;
            waitForMouseRelease = mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed);
        }
    }
}
