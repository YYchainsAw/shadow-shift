using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class LightBattery : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float chargeDuration = 1.25f;
        [SerializeField] private SpriteRenderer batteryVisual;
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite chargingSprite;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private SpriteRenderer exitVisual;
        [SerializeField] private Sprite lockedExitSprite;
        [SerializeField] private Sprite poweredExitSprite;
        [SerializeField] private GameObject exitMarker;

        private float chargeTime;

        public bool IsCharged => chargeTime >= chargeDuration;
        internal float ChargeProgress => Mathf.Clamp01(chargeTime / chargeDuration);

        internal void ChargeFrom(SpotlightRail light, float deltaTime)
        {
            if (IsCharged || deltaTime <= 0f || !light.Contains(transform.position))
                return;

            bool startedCharging = chargeTime == 0f;
            chargeTime = Mathf.Min(chargeTime + deltaTime, chargeDuration);
            if (IsCharged)
            {
                SetBatterySprite(fullSprite);
                SetExitPower(true);
            }
            else if (startedCharging)
                SetBatterySprite(chargingSprite);
        }

        internal void ResetCharge()
        {
            chargeTime = 0f;
            SetBatterySprite(emptySprite);
            SetExitPower(false);
        }

        private void SetBatterySprite(Sprite sprite)
        {
            if (batteryVisual != null)
                batteryVisual.sprite = sprite;
        }

        private void SetExitPower(bool powered)
        {
            if (exitVisual != null)
                exitVisual.sprite = powered ? poweredExitSprite : lockedExitSprite;
            if (exitMarker != null)
                exitMarker.SetActive(powered);
        }
    }
}
