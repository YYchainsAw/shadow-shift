using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowShift
{
    internal sealed class SpotlightInput
    {
        private readonly SpotlightRail spotlight;
        private Mouse draggingMouse;
        private bool waitForMouseRelease;

        public SpotlightInput(SpotlightRail spotlight)
        {
            this.spotlight = spotlight;
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
                    spotlight.BeginControl(pointer);
                else
                    spotlight.MovePointer(pointer);
                draggingMouse = mouse;
            }
            else
                draggingMouse = null;

            if (mouse.rightButton.isPressed)
                spotlight.AimPointer(pointer);
        }

        public void Reset(Mouse mouse)
        {
            draggingMouse = null;
            waitForMouseRelease = mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed);
        }
    }
}
