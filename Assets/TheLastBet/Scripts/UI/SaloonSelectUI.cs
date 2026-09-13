using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastBet.Saloon
{
    public sealed class SaloonSelectUI : MonoBehaviour
    {
        [SerializeField] private Button classicButton;
        [SerializeField] private Button westernButton;

        public event Action<int> SaloonChosen;

        void OnEnable()
        {
            if (classicButton == null)
            {
                Transform found = transform.Find("ClassicCard");
                if (found != null)
                    classicButton = found.GetComponent<Button>();
            }

            if (westernButton == null)
            {
                Transform found = transform.Find("WesternCard");
                if (found != null)
                    westernButton = found.GetComponent<Button>();
            }

            Bind(classicButton, westernButton);
        }

        public void Bind(Button classic, Button western)
        {
            classicButton = classic;
            westernButton = western;
            if (classicButton != null)
            {
                classicButton.onClick.RemoveAllListeners();
                classicButton.onClick.AddListener(() => SaloonChosen?.Invoke(0));
            }

            if (westernButton != null)
            {
                westernButton.onClick.RemoveAllListeners();
                westernButton.onClick.AddListener(() => SaloonChosen?.Invoke(1));
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
