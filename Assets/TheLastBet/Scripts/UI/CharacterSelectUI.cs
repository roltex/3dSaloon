using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastBet.Saloon
{
    public sealed class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private Button maleButton;
        [SerializeField] private Button femaleButton;
        [SerializeField] private Button banditButton;

        public event Action<PlayableCharacter> CharacterChosen;

        void OnEnable()
        {
            if (maleButton == null)
            {
                Transform found = transform.Find("MaleCard");
                if (found != null)
                    maleButton = found.GetComponent<Button>();
            }

            if (femaleButton == null)
            {
                Transform found = transform.Find("FemaleCard");
                if (found != null)
                    femaleButton = found.GetComponent<Button>();
            }

            if (banditButton == null)
            {
                Transform found = transform.Find("BanditCard");
                if (found != null)
                    banditButton = found.GetComponent<Button>();
            }

            Bind(maleButton, femaleButton, banditButton);
        }

        public void Bind(Button male, Button female, Button bandit)
        {
            maleButton = male;
            femaleButton = female;
            banditButton = bandit;
            BindOne(maleButton, PlayableCharacter.Male);
            BindOne(femaleButton, PlayableCharacter.Female);
            BindOne(banditButton, PlayableCharacter.Bandit);
        }

        void BindOne(Button button, PlayableCharacter id)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => CharacterChosen?.Invoke(id));
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
