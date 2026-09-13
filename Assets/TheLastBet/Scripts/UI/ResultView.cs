using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastBet.Saloon
{
    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private Button playAgain;

        public event Action PlayAgainClicked;

        void Awake()
        {
            if (title == null)
            {
                Transform found = transform.Find("Title");
                if (found != null)
                    title = found.GetComponent<Text>();
            }

            if (subtitle == null)
            {
                Transform found = transform.Find("Subtitle");
                if (found != null)
                    subtitle = found.GetComponent<Text>();
            }

            if (playAgain == null)
            {
                Transform found = transform.Find("PlayAgain");
                if (found != null)
                    playAgain = found.GetComponent<Button>();
            }

            Bind(title, subtitle, playAgain);
        }

        public void Bind(Text titleLabel, Text subtitleLabel, Button again)
        {
            title = titleLabel;
            subtitle = subtitleLabel;
            playAgain = again;
            if (playAgain != null)
            {
                playAgain.onClick.RemoveAllListeners();
                playAgain.onClick.AddListener(() => PlayAgainClicked?.Invoke());
            }
        }

        public void Show(bool won)
        {
            Show(won, null, null);
        }

        public void Show(bool won, string titleText, string subtitleText)
        {
            gameObject.SetActive(true);
            if (title != null)
                title.text = string.IsNullOrEmpty(titleText) ? (won ? "YOU STAND" : "DEAD MAN") : titleText;
            if (subtitle != null)
                subtitle.text = string.IsNullOrEmpty(subtitleText)
                    ? (won ? "THE LAST BET IS YOURS" : "THE SALOON KEEPS YOUR NAME")
                    : subtitleText;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
