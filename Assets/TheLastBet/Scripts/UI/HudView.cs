using UnityEngine;
using UnityEngine.UI;

namespace TheLastBet.Saloon
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Text healthLabel;
        [SerializeField] private Text ammoLabel;
        [SerializeField] private Text remainingLabel;
        [SerializeField] private Text crosshair;

        void Awake()
        {
            if (healthLabel == null)
                healthLabel = FindLabel("Health");
            if (ammoLabel == null)
                ammoLabel = FindLabel("Ammo");
            if (remainingLabel == null)
                remainingLabel = FindLabel("Remaining");
            if (crosshair == null)
                crosshair = FindLabel("Crosshair");
        }

        Text FindLabel(string childName)
        {
            Transform found = transform.Find(childName);
            return found != null ? found.GetComponent<Text>() : null;
        }

        public void Bind(Text health, Text ammo, Text remaining, Text sight)
        {
            healthLabel = health;
            ammoLabel = ammo;
            remainingLabel = remaining;
            crosshair = sight;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void Refresh(float health, float maxHealth, int ammo, int clip, int remaining)
        {
            SetCombatVisible(true);
            if (healthLabel != null)
                healthLabel.text = $"HEALTH  {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}";
            if (ammoLabel != null)
                ammoLabel.text = ammo <= 0 ? "RELOADING" : $"AMMO  {ammo} / {clip}";
            if (remainingLabel != null)
            {
                remainingLabel.fontSize = 20;
                remainingLabel.text = remaining > 0 ? $"GUNSLINGERS  {remaining}" : "LAST ONE STANDING";
            }
        }

        public void ShowRoamPrompt(string prompt)
        {
            SetCombatVisible(true);
            if (remainingLabel != null)
            {
                remainingLabel.fontSize = 26;
                remainingLabel.text = prompt;
            }
        }

        public void ShowRoam(float health, float maxHealth, int ammo, int clip, string prompt)
        {
            SetCombatVisible(true);
            if (healthLabel != null)
                healthLabel.text = $"HEALTH  {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}";
            if (ammoLabel != null)
                ammoLabel.text = ammo <= 0 ? "RELOADING" : $"AMMO  {ammo} / {clip}";
            if (remainingLabel != null)
            {
                remainingLabel.fontSize = 26;
                remainingLabel.text = prompt;
            }
        }

        public void ShowDuelBanner(string text, int size = 86)
        {
            SetCombatVisible(false);
            if (remainingLabel != null)
            {
                remainingLabel.fontSize = size;
                remainingLabel.text = text;
            }
        }

        void SetCombatVisible(bool visible)
        {
            if (healthLabel != null)
                healthLabel.enabled = visible;
            if (ammoLabel != null)
                ammoLabel.enabled = visible;
        }
    }
}
