using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class SaloonVenue : MonoBehaviour
    {
        [SerializeField] private string displayName = "Saloon";
        [SerializeField] private GameObject[] hideWhenInactive;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform selectMale;
        [SerializeField] private Transform selectFemale;
        [SerializeField] private Transform selectBandit;
        [SerializeField] private Transform selectCamera;
        [SerializeField] private Transform selectLook;
        [SerializeField] private DuelArenaAnchors duel;
        [SerializeField] private InteractableStation duelStation;

        public string DisplayName => displayName;
        public Transform PlayerSpawn => playerSpawn;
        public Transform SelectMale => selectMale;
        public Transform SelectFemale => selectFemale;
        public Transform SelectBandit => selectBandit;
        public Transform SelectCamera => selectCamera;
        public Transform SelectLook => selectLook;
        public DuelArenaAnchors Duel => duel;
        public InteractableStation DuelStation => duelStation;

        public void Configure(
            string name,
            GameObject[] extras,
            Transform spawn,
            Transform male,
            Transform female,
            Transform bandit,
            Transform cam,
            Transform look,
            DuelArenaAnchors duelAnchors,
            InteractableStation station)
        {
            displayName = name;
            hideWhenInactive = extras;
            playerSpawn = spawn;
            selectMale = male;
            selectFemale = female;
            selectBandit = bandit;
            selectCamera = cam;
            selectLook = look;
            duel = duelAnchors;
            duelStation = station;
        }

        public void BindSelectBandit(Transform bandit)
        {
            selectBandit = bandit;
        }

        public void SetVenueActive(bool active)
        {
            if (hideWhenInactive != null)
            {
                for (int i = 0; i < hideWhenInactive.Length; i++)
                {
                    if (hideWhenInactive[i] != null)
                        hideWhenInactive[i].SetActive(active);
                }
            }

            bool isWorldRoot = hideWhenInactive != null && hideWhenInactive.Length > 0;
            if (!isWorldRoot)
                gameObject.SetActive(active);
        }
    }
}
