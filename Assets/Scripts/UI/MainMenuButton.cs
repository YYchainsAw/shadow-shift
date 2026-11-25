using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShadowShift
{
    public sealed class MainMenuButton : Button, IPointerMoveHandler
    {
        private bool pointerInside;
        private bool pointerFocus;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            base.OnPointerEnter(eventData);
            FocusWithPointer(eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            FocusWithPointer(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                FocusWithPointer(eventData);

            base.OnPointerDown(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            base.OnPointerExit(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            pointerFocus = eventData is PointerEventData;
            base.OnSelect(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            pointerFocus = false;
            base.OnSubmit(eventData);
        }

        protected override void OnDisable()
        {
            pointerInside = false;
            pointerFocus = false;
            base.OnDisable();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            // Keep logical selection for keyboard navigation while hiding a pointer focus after exit.
            if (state == SelectionState.Highlighted ||
                (pointerFocus && !pointerInside && (state == SelectionState.Selected || state == SelectionState.Pressed)))
                state = SelectionState.Normal;
            else if (pointerFocus && state == SelectionState.Selected)
                state = SelectionState.Highlighted;

            // Clear the old highlight before the next one starts fading in.
            base.DoStateTransition(state, instant || state == SelectionState.Normal);
        }

        private void FocusWithPointer(PointerEventData eventData)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null || !IsActive() || !IsInteractable())
                return;

            if (eventSystem.currentSelectedGameObject != gameObject)
            {
                eventSystem.SetSelectedGameObject(gameObject, eventData);
            }
            else if (!pointerFocus)
            {
                pointerFocus = true;
                DoStateTransition(currentSelectionState, false);
            }
        }
    }
}
