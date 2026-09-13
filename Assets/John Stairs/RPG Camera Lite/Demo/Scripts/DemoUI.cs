using JohnStairs.RPG.Character.Cam;
using UnityEngine;
using UnityEngine.UI;

namespace JohnStairs.PPC.UI {
    public class DemoUI : MonoBehaviour {
        public GameObject Character;
        public GameObject CharacterLite;
        public Transform StartSpawn;
        public Transform PillarsSpawn;
        public Transform HousesSpawn;
        public Transform WaterSpawn;
        public Toggle FullVersionToggle;
        public Text HeadingLabel;

        protected const string FULL_VERSION_HEADING = "RPG Camera";
        protected const string LITE_VERSION_HEADING = "RPG Camera Lite";
        protected bool _liteVersionActive;
        protected bool _teleport;
        protected Transform _teleportTarget;

        protected void Awake() {
        }

        protected void Start() {
            if (Character == null) {
                Character = new GameObject("Empty");
            }

            if (CharacterLite == null) {
                CharacterLite = new GameObject("Empty");
            }

            Component[] components = Character.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == null) {
                    FullVersionToggle.isOn = false;
                    break;
                }
            }
        }

        protected void FixedUpdate() { // required to prevent interference with Character Controller component physics
            if (_teleport) {
                _teleport = false;
                Character.transform.position = _teleportTarget.position;
                CharacterLite.transform.position = _teleportTarget.position;
            }
        }

        public void ToggleLiteVersion() {
            _liteVersionActive = !_liteVersionActive;
            if (_liteVersionActive) {
                Character.SetActive(false);
                CharacterLite.SetActive(true);
                HeadingLabel.text = LITE_VERSION_HEADING;
            } else {
                CharacterLite.SetActive(false);
                Character.SetActive(true);
                HeadingLabel.text = FULL_VERSION_HEADING;
            }
        }

        public void JumpToStart() {
            _teleport = true;
            _teleportTarget = StartSpawn;
            AlignWithTransform(StartSpawn);
            ResetAndYaw(0);
        }

        public void JumpToPillars() {
            _teleport = true;
            _teleportTarget = PillarsSpawn;
            AlignWithTransform(PillarsSpawn);
            ResetAndYaw(90.0f);
        }

        public void JumpToHouses() {
            _teleport = true;
            _teleportTarget = HousesSpawn;
            AlignWithTransform(HousesSpawn);
            ResetAndYaw(HousesSpawn.rotation.eulerAngles.y);
        }

        public void JumpToWater() {
            _teleport = true;
            _teleportTarget = WaterSpawn;
            AlignWithTransform(WaterSpawn);
            ResetAndYaw(0);
        }

        protected void AlignWithTransform(Transform targetTransform) {
            Character.transform.rotation = targetTransform.rotation;
            CharacterLite.transform.rotation = targetTransform.rotation;
        }

        protected void ResetAndYaw(float yaw) {
            Character.GetComponent<RPGCameraLite>()?.ResetView();
            Character.GetComponent<RPGCameraLite>()?.SetYaw(yaw);
            CharacterLite.GetComponent<RPGCameraLite>()?.ResetView();
            CharacterLite.GetComponent<RPGCameraLite>()?.SetYaw(yaw);
        }
    }
}
