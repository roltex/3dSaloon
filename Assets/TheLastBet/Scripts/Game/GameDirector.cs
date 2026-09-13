using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace TheLastBet.Saloon
{
    public enum MatchState
    {
        ChooseSaloon,
        Select,
        Playing,
        DuelIntro,
        DuelCountdown,
        DuelDraw,
        Won,
        Lost
    }

    [DefaultExecutionOrder(-80)]
    public sealed class GameDirector : MonoBehaviour
    {
        static readonly Vector3 PlayerSpawn = new Vector3(0f, 0f, -5.2f);

        [SerializeField] private GameObject malePrefab;
        [SerializeField] private GameObject femalePrefab;
        [SerializeField] private GameObject banditPrefab;
        [SerializeField] private GameObject viewmodelPrefab;
        [SerializeField] private SaloonCameraRig cameraRig;
        [SerializeField] private SaloonSelectUI saloonSelectUi;
        [SerializeField] private CharacterSelectUI selectUi;
        [SerializeField] private HudView hud;
        [SerializeField] private ResultView result;
        [SerializeField] private SaloonVenue[] venues;
        [SerializeField] private GameObject westernSaloonPrefab;
        [SerializeField] private AudioClip fireClip;
        [SerializeField] private AudioClip reloadClip;

        readonly List<GameObject> _selectPreviews = new();
        GameObject _player;
        GameObject _opponent;
        SaloonWalkPreview _motor;
        CharacterAnimDriver _opponentAnim;
        HitscanWeapon _opponentWeapon;
        Health _playerHealth;
        HitscanWeapon _playerWeapon;
        FirstPersonViewmodel _viewmodel;
        DuelArenaAnchors _duel;
        InteractableStation _duelStation;
        SaloonVenue _venue;
        MatchState _state = MatchState.ChooseSaloon;
        PlayableCharacter _playerCharacter;
        bool _duelResolved;
        int _countdownValue;
        float _phaseUntil;
        float _opponentDrawAt;

        public MatchState State => _state;

        void Awake()
        {
            CombatAudio.AssignGenerated(ref fireClip, ref reloadClip);
            ResolveReferences();
            EnsureEventSystem();
        }

        void Start()
        {
            ClearLegacyDummies();
            EnsureVenues();
            EnterChooseSaloon();
        }

        void OnEnable()
        {
            if (saloonSelectUi != null)
                saloonSelectUi.SaloonChosen += OnSaloonChosen;
            if (selectUi != null)
                selectUi.CharacterChosen += OnCharacterChosen;
            if (result != null)
                result.PlayAgainClicked += RestartToSelect;
        }

        void OnDisable()
        {
            if (saloonSelectUi != null)
                saloonSelectUi.SaloonChosen -= OnSaloonChosen;
            if (selectUi != null)
                selectUi.CharacterChosen -= OnCharacterChosen;
            if (result != null)
                result.PlayAgainClicked -= RestartToSelect;
        }

        public void ChooseCharacter(PlayableCharacter character)
        {
            OnCharacterChosen(character);
        }

        public void GoToSaloonSelect()
        {
            EnterChooseSaloon();
        }

        public void ForceBeginDuel()
        {
            if (_state == MatchState.Playing)
                BeginDuel();
        }

        void Update()
        {
            if (_state == MatchState.Select)
            {
                TryClickWorldSelect();
                return;
            }

            if (_state == MatchState.Playing)
            {
                UpdateRoam();
                return;
            }

            if (_state == MatchState.DuelIntro || _state == MatchState.DuelCountdown || _state == MatchState.DuelDraw)
                UpdateDuel();
        }

        void ResolveReferences()
        {
            if (cameraRig == null)
                cameraRig = FindAnyObjectByType<SaloonCameraRig>();
#if UNITY_EDITOR
            if (banditPrefab == null)
                banditPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/TheLastBet/Prefabs/Characters/PF_Bandit.prefab");
            if (viewmodelPrefab == null)
                viewmodelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    FirstPersonViewmodel.ModelPath);
            if (westernSaloonPrefab == null)
                westernSaloonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    WesternSaloonBuilder.ModelPath);
            if (fireClip == null)
                fireClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/TheLastBet/Audio/Sfx_RevolverFire.wav");
            if (reloadClip == null)
                reloadClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/TheLastBet/Audio/Sfx_RevolverReload.wav");
#endif
            if (selectUi == null || hud == null || result == null || saloonSelectUi == null)
                BuildRuntimeUi();
        }

        void EnterChooseSaloon()
        {
            _state = MatchState.ChooseSaloon;
            _venue = null;
            ClearMatchActors();
            SetCursorLocked(false);
            saloonSelectUi?.SetVisible(true);
            selectUi?.SetVisible(false);
            hud?.SetVisible(false);
            result?.Hide();
            ShowVenue(0, false);
            FrameChooseSaloonCamera();
        }

        void OnSaloonChosen(int index)
        {
            if (_state != MatchState.ChooseSaloon)
                return;
            ShowVenue(index, true);
            StartMatch(PlayableCharacter.Bandit);
        }

        void ShowVenue(int index, bool bind)
        {
            EnsureVenues();
            if (venues == null || venues.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, venues.Length - 1);
            for (int i = 0; i < venues.Length; i++)
            {
                if (venues[i] != null)
                    venues[i].SetVenueActive(i == index);
            }

            _venue = venues[index];
            if (bind && _venue != null)
            {
                _duel = _venue.Duel;
                _duelStation = _venue.DuelStation;
            }

            ClassicAmbientCrowd.Sync(bind ? _venue : venues[0], banditPrefab);
        }

        void EnterSelect()
        {
            _state = MatchState.Select;
            ClearMatchActors();
            SetCursorLocked(false);
            saloonSelectUi?.SetVisible(false);
            selectUi?.SetVisible(true);
            hud?.SetVisible(false);
            result?.Hide();
            FrameSelectCamera();
            SpawnSelectPreviews();
        }

        void OnCharacterChosen(PlayableCharacter character)
        {
            if (_state != MatchState.Select)
                return;
            StartMatch(character);
        }

        void StartMatch(PlayableCharacter character)
        {
            _playerCharacter = character;
            ClearSelectPreviews();
            saloonSelectUi?.SetVisible(false);
            selectUi?.SetVisible(false);
            result?.Hide();

            _player = SpawnPlayer(character);
            if (_player == null)
            {
                Debug.LogError("[The Last Bet] Character prefab is missing. Run The Last Bet > Rebuild Characters.");
                EnterChooseSaloon();
                return;
            }

            _duel = _venue != null ? _venue.Duel : FindAnyObjectByType<DuelArenaAnchors>(FindObjectsInactive.Exclude);
            _duelStation = _venue != null ? _venue.DuelStation : FindDuelStation();
            _state = MatchState.Playing;
            hud?.SetVisible(true);
            SetCursorLocked(true);
        }

        void EnterResult(bool won, string title = null, string subtitle = null)
        {
            _duelResolved = true;
            _state = won ? MatchState.Won : MatchState.Lost;
            if (_motor != null)
            {
                _motor.ShotAttempted -= OnPlayerShotAttempted;
                _motor.MovementLocked = false;
                _motor.CombatEnabled = false;
            }

            SetPlayerMeshVisible(false);
            hud?.SetVisible(false);
            result?.Show(won, title, subtitle);
            SetCursorLocked(false);
        }

        void RestartToSelect()
        {
            EnterChooseSaloon();
        }

        GameObject SpawnPlayer(PlayableCharacter character)
        {
            GameObject prefab = PrefabFor(character);
            if (prefab == null)
                return null;

            Vector3 spawn = _venue != null && _venue.PlayerSpawn != null
                ? _venue.PlayerSpawn.position
                : PlayerSpawn;
            Quaternion rot = _venue != null && _venue.PlayerSpawn != null
                ? _venue.PlayerSpawn.rotation
                : Quaternion.identity;
            var go = Instantiate(prefab, spawn, rot);
            go.name = "Player_" + character;
            CombatLayers.SetLayerRecursively(go, CombatLayers.Player);
            if (go.CompareTag("Untagged"))
                go.tag = "Player";
            ApplyVenueCharacterScale(go);

            FitControllerToMesh(go);
            SnapToFloor(go);

            _motor = go.GetComponent<SaloonWalkPreview>();
            if (_motor == null)
                _motor = go.AddComponent<SaloonWalkPreview>();
            _motor.SetCursorManaged(false);
            _motor.CombatEnabled = true;
            _motor.MovementLocked = false;
            if (cameraRig != null && cameraRig.GameplayCamera != null)
                _motor.SetCamera(cameraRig.GameplayCamera.transform);

            _playerHealth = go.GetComponent<Health>();
            _playerWeapon = go.GetComponent<HitscanWeapon>();
            BindWeapon(_playerWeapon, go.transform, 6, 0.45f, 34f);

            if (cameraRig != null)
            {
                cameraRig.SetFollowTarget(go.transform);
                cameraRig.SetOrbit(48f, 12f);
                cameraRig.ResumeFollow();
            }

            return go;
        }

        void BindWeapon(HitscanWeapon weapon, Transform owner, int clip, float interval, float damage)
        {
            if (weapon == null)
                return;
            Transform muzzle = owner.Find("Muzzle");
            weapon.Configure(muzzle, fireClip != null ? fireClip : CombatAudio.Fire, reloadClip != null ? reloadClip : CombatAudio.Reload);
            weapon.ConfigureStats(clip, interval, damage);
        }

        void UpdateRoam()
        {
            RescueIfFallen();
            if (InteractPressed() && IsNearDuel())
            {
                BeginDuel();
                return;
            }

            float hp = _playerHealth != null ? _playerHealth.Current : 0f;
            float max = _playerHealth != null ? _playerHealth.MaxHealth : 100f;
            int ammo = _playerWeapon != null ? _playerWeapon.Ammo : 0;
            int clip = _playerWeapon != null ? _playerWeapon.ClipSize : 6;
            hud?.ShowRoam(hp, max, ammo, clip, IsNearDuel() ? "PRESS E TO DUEL" : "FIND THE DUEL RING");
        }

        void BeginDuel()
        {
            if (_player == null)
                return;

            if (_duel == null && _venue != null)
                _duel = _venue.Duel;
            if (_duelStation == null && _venue != null)
                _duelStation = _venue.DuelStation;
            _duel ??= FindAnyObjectByType<DuelArenaAnchors>(FindObjectsInactive.Exclude);
            _duelStation ??= FindDuelStation();
            if (!SpawnOpponent())
            {
                hud?.ShowRoamPrompt("NO OPPONENT READY");
                return;
            }

            PlaceDuelists();
            FrameDuelCamera();

            if (_motor != null)
            {
                _motor.MovementLocked = true;
                _motor.CombatEnabled = true;
                _motor.ShotAttempted -= OnPlayerShotAttempted;
                _motor.ShotAttempted += OnPlayerShotAttempted;
            }

            SetPlayerMeshVisible(false);
            AttachViewmodel();
            if (_viewmodel == null)
                SetPlayerMeshVisible(true);

            _duelResolved = false;
            _state = MatchState.DuelIntro;
            _phaseUntil = Time.time + 1.4f;
            hud?.ShowDuelBanner("BEGIN DUEL", 72);
        }

        bool SpawnOpponent()
        {
            if (_opponent != null)
                Destroy(_opponent);

            GameObject prefab = PrefabFor(PlayableCharacter.Bandit);
            if (prefab == null)
                return false;

            Transform mark = _duel != null && _duel.PlayerB != null ? _duel.PlayerB : null;
            Vector3 pos = mark != null ? mark.position : new Vector3(-5.2f, 0.05f, -7.15f);
            Quaternion rot = mark != null ? mark.rotation : Quaternion.identity;
            _opponent = Instantiate(prefab, pos, rot);
            _opponent.name = "DuelOpponent";
            CombatLayers.SetLayerRecursively(_opponent, CombatLayers.Enemy);
            ApplyVenueCharacterScale(_opponent);

            var ai = _opponent.GetComponent<GunslingerAI>();
            if (ai != null)
                Destroy(ai);
            var agent = _opponent.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
                Destroy(agent);

            FitControllerToMesh(_opponent);
            SnapToFloor(_opponent);

            _opponentAnim = _opponent.GetComponent<CharacterAnimDriver>();
            _opponentWeapon = _opponent.GetComponent<HitscanWeapon>();
            BindWeapon(_opponentWeapon, _opponent.transform, 6, 0.45f, 34f);
            return true;
        }

        void PlaceDuelists()
        {
            Transform playerMark = _duel != null && _duel.PlayerA != null
                ? _duel.PlayerA
                : (_duelStation != null ? _duelStation.PlayerAnchor : null);
            Transform foeMark = _duel != null && _duel.PlayerB != null
                ? _duel.PlayerB
                : (_duelStation != null ? _duelStation.OpponentAnchor : null);

            if (playerMark != null)
                Teleport(_player, playerMark.position, playerMark.rotation);
            if (_opponent != null && foeMark != null)
                Teleport(_opponent, foeMark.position, foeMark.rotation);

            if (_player != null && _opponent != null)
            {
                FaceEachOther(_player.transform, _opponent.transform);
                FaceEachOther(_opponent.transform, _player.transform);
            }
        }

        void FrameDuelCamera()
        {
            if (cameraRig == null || _player == null)
                return;
            cameraRig.SetFirstPersonDuel(_player.transform, _opponent != null ? _opponent.transform : null);
        }

        void AttachViewmodel()
        {
            DestroyViewmodel();
            Camera cam = cameraRig != null ? cameraRig.GameplayCamera : Camera.main;
            _viewmodel = FirstPersonViewmodel.Attach(cam, viewmodelPrefab);
            if (_viewmodel != null && _playerWeapon != null)
            {
                _playerWeapon.Configure(
                    _viewmodel.Muzzle,
                    fireClip != null ? fireClip : CombatAudio.Fire,
                    reloadClip != null ? reloadClip : CombatAudio.Reload);
            }
        }

        void DestroyViewmodel()
        {
            if (_viewmodel != null)
            {
                Destroy(_viewmodel.gameObject);
                _viewmodel = null;
            }

            if (_playerWeapon != null && _player != null)
                BindWeapon(_playerWeapon, _player.transform, 6, 0.45f, 34f);
        }

        void SetPlayerMeshVisible(bool visible)
        {
            if (_player == null)
                return;
            Renderer[] renderers = _player.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }

        void UpdateDuel()
        {
            if (_player != null && _opponent != null)
            {
                FaceEachOther(_player.transform, _opponent.transform);
                FaceEachOther(_opponent.transform, _player.transform);
            }

            if (_state == MatchState.DuelIntro && Time.time >= _phaseUntil)
            {
                _state = MatchState.DuelCountdown;
                _countdownValue = 5;
                _phaseUntil = Time.time + 1f;
                hud?.ShowDuelBanner("5");
                return;
            }

            if (_state == MatchState.DuelCountdown && Time.time >= _phaseUntil)
            {
                _countdownValue--;
                if (_countdownValue <= 0)
                {
                    _state = MatchState.DuelDraw;
                    _opponentDrawAt = Time.time + 0.42f + Random.Range(0f, 0.12f);
                    if (_motor != null)
                        _motor.CombatEnabled = true;
                    hud?.ShowDuelBanner("DRAW!");
                    return;
                }

                _phaseUntil = Time.time + 1f;
                hud?.ShowDuelBanner(_countdownValue.ToString());
                return;
            }

            if (_state == MatchState.DuelDraw && !_duelResolved)
            {
                if (FirePressed())
                {
                    ResolveDuel(true, "YOU STAND", "FIRST SHOT TAKES THE BET");
                    return;
                }

                if (Time.time >= _opponentDrawAt)
                    ResolveDuel(false, "DEAD MAN", "THE OTHER GUN WAS FASTER");
            }
        }

        void OnPlayerShotAttempted()
        {
            if (_duelResolved)
                return;

            _viewmodel?.PlayShoot();
            if (_state == MatchState.DuelIntro || _state == MatchState.DuelCountdown)
                ResolveDuel(false, "TOO SOON", "WAIT FOR THE DRAW");
        }

        void ResolveDuel(bool playerWon, string title, string subtitle)
        {
            if (_duelResolved)
                return;
            _duelResolved = true;

            if (playerWon)
            {
                _viewmodel?.PlayShoot();
                FireVisual(_playerWeapon, _player, _opponent);
                if (_player != null)
                    _player.GetComponent<CharacterAnimDriver>()?.TriggerShoot();
                _opponentAnim?.SetDead();
                var foeHealth = _opponent != null ? _opponent.GetComponent<Health>() : null;
                foeHealth?.ApplyDamage(999f, _player);
            }
            else if (_state == MatchState.DuelDraw)
            {
                FireVisual(_opponentWeapon, _opponent, _player);
                _opponentAnim?.TriggerShoot();
                _playerHealth?.ApplyDamage(999f, _opponent);
            }

            EnterResult(playerWon, title, subtitle);
        }

        void FireVisual(HitscanWeapon weapon, GameObject from, GameObject to)
        {
            if (weapon == null || from == null || to == null)
                return;

            Vector3 origin = from.transform.position + Vector3.up * 1.35f;
            if (from == _player && cameraRig != null && cameraRig.GameplayCamera != null)
                origin = cameraRig.GameplayCamera.transform.position;

            Vector3 dir = (to.transform.position + Vector3.up * 1.35f - origin).normalized;
            weapon.TryFire(origin, dir, from);
        }

        bool IsNearDuel()
        {
            if (_player == null)
                return false;
            if (_duelStation != null && _duelStation.CanInteract(_player.transform.position))
                return true;

            Vector3 center = _duelStation != null ? _duelStation.transform.position : new Vector3(-6.55f, 0f, -7.15f);
            if (_duel != null && _duel.PlayerA != null && _duel.PlayerB != null)
                center = (_duel.PlayerA.position + _duel.PlayerB.position) * 0.5f;
            return Vector3.Distance(_player.transform.position, center) <= 4.2f;
        }

        static InteractableStation FindDuelStation()
        {
            InteractableStation[] stations = FindObjectsByType<InteractableStation>(FindObjectsInactive.Exclude);
            for (int i = 0; i < stations.Length; i++)
            {
                if (stations[i] != null && stations[i].StationType == SaloonStationType.DuelArena)
                    return stations[i];
            }

            return null;
        }

        static void Teleport(GameObject go, Vector3 position, Quaternion rotation)
        {
            if (go == null)
                return;
            var controller = go.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;
            go.transform.SetPositionAndRotation(position, rotation);
            SnapToFloor(go);
            if (controller != null)
                controller.enabled = true;
        }

        static void FitControllerToMesh(GameObject go)
        {
            if (go == null)
                return;

            var controller = go.GetComponent<CharacterController>();
            if (controller == null)
                controller = go.AddComponent<CharacterController>();

            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.radius = 0.28f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0f;
            controller.stepOffset = 0.45f;
            controller.slopeLimit = 50f;
        }

        void ApplyVenueCharacterScale(GameObject go)
        {
            if (go == null)
                return;
            float scale = 1f;
            if (_venue != null && _venue.name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) >= 0)
                scale = WesternSaloonBuilder.CharacterScale;
            go.transform.localScale = Vector3.one * scale;
        }

        static void SnapToFloor(GameObject go)
        {
            if (go == null)
                return;

            var controller = go.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null)
                controller.enabled = false;

            Vector3 pos = go.transform.position;
            float worldHeight = controller != null
                ? controller.height * go.transform.lossyScale.y
                : 1.8f;
            float startY = pos.y + worldHeight + 0.6f;
            Vector3 origin = new Vector3(pos.x, startY, pos.z);
            int mask = ~0;
            int view = CombatLayers.Viewmodel;
            if (view >= 0)
                mask &= ~(1 << view);
            mask &= ~LayerMask.GetMask("UI", "Ignore Raycast", "TransparentFX");

            float floorY = pos.y;
            bool hitFloor = false;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, worldHeight + 10f, mask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i].collider;
                if (col == null || col.transform == go.transform || col.transform.IsChildOf(go.transform))
                    continue;
                if (hits[i].normal.y < 0.45f)
                    continue;
                if (hits[i].distance >= nearest)
                    continue;
                nearest = hits[i].distance;
                floorY = hits[i].point.y;
                hitFloor = true;
            }

            if (hitFloor)
            {
                float footOffset = 0f;
                if (controller != null)
                    footOffset = controller.center.y - controller.height * 0.5f;
                pos.y = floorY - footOffset;
                go.transform.position = pos;
            }

            if (controller != null)
                controller.enabled = wasEnabled;
        }

        void RescueIfFallen()
        {
            if (_player == null || _player.transform.position.y > -3f)
                return;
            Vector3 spawn = _venue != null && _venue.PlayerSpawn != null
                ? _venue.PlayerSpawn.position
                : PlayerSpawn;
            Quaternion rot = _venue != null && _venue.PlayerSpawn != null
                ? _venue.PlayerSpawn.rotation
                : Quaternion.identity;
            Teleport(_player, spawn, rot);
        }

        static void FaceEachOther(Transform self, Transform other)
        {
            if (self == null || other == null)
                return;
            Vector3 delta = other.position - self.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.001f)
                return;
            self.rotation = Quaternion.LookRotation(delta, Vector3.up);
        }

        static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        static bool FirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            return (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                   (pad != null && pad.rightTrigger.wasPressedThisFrame);
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        void ClearMatchActors()
        {
            DestroyViewmodel();
            if (_motor != null)
                _motor.ShotAttempted -= OnPlayerShotAttempted;
            if (_player != null)
                Destroy(_player);
            if (_opponent != null)
                Destroy(_opponent);
            GunslingerAI[] leftovers = FindObjectsByType<GunslingerAI>(FindObjectsInactive.Include);
            for (int i = 0; i < leftovers.Length; i++)
            {
                if (leftovers[i] != null)
                    Destroy(leftovers[i].gameObject);
            }

            _player = null;
            _opponent = null;
            _motor = null;
            _opponentAnim = null;
            _opponentWeapon = null;
            _playerHealth = null;
            _playerWeapon = null;
            _duelResolved = false;
            ClearSelectPreviews();
        }

        void SpawnSelectPreviews()
        {
            ClearSelectPreviews();
            Vector3 banditPos = _venue != null && _venue.SelectBandit != null
                ? _venue.SelectBandit.position
                : new Vector3(0f, 0.05f, -7.55f);
            TrySpawnPreview(banditPrefab, PlayableCharacter.Bandit, banditPos);
        }

        void TrySpawnPreview(GameObject prefab, PlayableCharacter character, Vector3 position)
        {
            if (prefab == null)
                return;
            var go = Instantiate(prefab, position, Quaternion.identity);
            go.name = "Select_" + character;
            foreach (var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is CharacterAnimDriver || behaviour is Health || behaviour is HitscanWeapon)
                    behaviour.enabled = false;
            }

            var target = go.GetComponent<CharacterSelectTarget>();
            if (target == null)
                target = go.AddComponent<CharacterSelectTarget>();
            target.Configure(character);
            go.AddComponent<SelectPreviewSpin>();
            SnapToFloor(go);
            var capsule = go.GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = go.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.45f;
            capsule.center = new Vector3(0f, 1f, 0f);
            _selectPreviews.Add(go);
        }

        void ClearSelectPreviews()
        {
            for (int i = 0; i < _selectPreviews.Count; i++)
            {
                if (_selectPreviews[i] != null)
                    Destroy(_selectPreviews[i]);
            }

            _selectPreviews.Clear();
        }

        void FrameSelectCamera()
        {
            if (cameraRig == null)
                return;
            Vector3 pos = new Vector3(0f, 1.55f, -3.55f);
            Vector3 look = new Vector3(0f, 1.05f, -7.55f);
            if (_venue != null && _venue.SelectCamera != null)
                pos = _venue.SelectCamera.position;
            if (_venue != null && _venue.SelectLook != null)
                look = _venue.SelectLook.position;
            cameraRig.HoldScriptedView(pos, Quaternion.LookRotation(look - pos, Vector3.up), 46f);
        }

        void FrameChooseSaloonCamera()
        {
            if (cameraRig == null)
                return;
            Vector3 pos = new Vector3(9.6f, 15.4f, -14.2f);
            Vector3 look = Vector3.up * 1.4f;
            cameraRig.HoldScriptedView(pos, Quaternion.LookRotation(look - pos, Vector3.up), 42f);
        }

        void EnsureVenues()
        {
            if (venues != null && venues.Length >= 2 && venues[0] != null && venues[1] != null
                && WesternSaloonBuilder.MatchesCurrentLayout(venues[1]))
                return;

            var found = FindObjectsByType<SaloonVenue>(FindObjectsInactive.Include);
            if (found != null && found.Length >= 2)
            {
                SaloonVenue classic = null;
                SaloonVenue western = null;
                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] == null)
                        continue;
                    if (found[i].name.Contains("Western"))
                        western = found[i];
                    else if (classic == null)
                        classic = found[i];
                }

                if (classic != null && western != null && WesternSaloonBuilder.MatchesCurrentLayout(western))
                {
                    venues = new[] { classic, western };
                    return;
                }
            }

            var list = new List<SaloonVenue>();
            SaloonVenue classicVenue = CreateClassicVenue();
            if (classicVenue != null)
                list.Add(classicVenue);

            SaloonVenue westernVenue = FindVenueByName("Western");
            if (westernVenue != null && !WesternSaloonBuilder.MatchesCurrentLayout(westernVenue))
            {
                Object.DestroyImmediate(westernVenue.gameObject);
                westernVenue = null;
            }

            if (westernVenue == null)
                westernVenue = WesternSaloonBuilder.Build(null, westernSaloonPrefab);
            if (westernVenue != null)
            {
                westernVenue.SetVenueActive(false);
                list.Add(westernVenue);
            }

            venues = list.ToArray();
        }

        SaloonVenue CreateClassicVenue()
        {
            SaloonVenue existing = FindVenueByName("Classic");
            if (existing != null)
                return existing;

            var root = new GameObject("Venue_Classic");
            var extras = new List<GameObject>();
            var env = GameObject.Find("Environment");
            if (env != null)
            {
                Transform shell = env.transform.Find("SaloonShell");
                if (shell != null)
                    extras.Add(shell.gameObject);
            }

            var stations = GameObject.Find("GameplayStations");
            if (stations != null)
                extras.Add(stations);
            var nav = GameObject.Find("Navigation");
            if (nav != null)
                extras.Add(nav);

            Transform spawn = Marker(root.transform, "PlayerSpawn", PlayerSpawn, Vector3.forward);
            Transform male = Marker(root.transform, "SelectMale", new Vector3(-1.45f, 0.05f, -7.55f), Vector3.forward);
            Transform female = Marker(root.transform, "SelectFemale", new Vector3(1.45f, 0.05f, -7.55f), Vector3.forward);
            Transform bandit = Marker(root.transform, "SelectBandit", new Vector3(0f, 0.05f, -7.55f), Vector3.forward);
            Transform cam = Marker(root.transform, "SelectCamera", new Vector3(0f, 1.55f, -3.55f), Vector3.back);
            Transform look = Marker(root.transform, "SelectLook", new Vector3(0f, 1.05f, -7.55f), Vector3.forward);

            DuelArenaAnchors duel = FindClassicDuelAnchors();
            InteractableStation station = FindClassicDuelStation();

            var venue = root.AddComponent<SaloonVenue>();
            venue.Configure("Classic Saloon", extras.ToArray(), spawn, male, female, bandit, cam, look, duel, station);
            return venue;
        }

        static bool IsUnderWesternVenue(Component component)
        {
            if (component == null)
                return false;
            Transform current = component.transform;
            while (current != null)
            {
                if (current.name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                current = current.parent;
            }

            return false;
        }

        static DuelArenaAnchors FindClassicDuelAnchors()
        {
            DuelArenaAnchors[] all = FindObjectsByType<DuelArenaAnchors>(FindObjectsInactive.Include);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && !IsUnderWesternVenue(all[i]))
                    return all[i];
            }

            return null;
        }

        static InteractableStation FindClassicDuelStation()
        {
            InteractableStation[] all = FindObjectsByType<InteractableStation>(FindObjectsInactive.Include);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].StationType == SaloonStationType.DuelArena && !IsUnderWesternVenue(all[i]))
                    return all[i];
            }

            return null;
        }

        GameObject PrefabFor(PlayableCharacter character)
        {
            _ = character;
            return banditPrefab;
        }

        static SaloonVenue FindVenueByName(string token)
        {
            var found = FindObjectsByType<SaloonVenue>(FindObjectsInactive.Include);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return found[i];
            }

            return null;
        }

        static Transform Marker(Transform parent, string name, Vector3 pos, Vector3 forward)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward, Vector3.up));
            return go.transform;
        }

        void TryClickWorldSelect()
        {
            if (!ClickPressed(out Vector2 screen))
                return;
            Camera cam = cameraRig != null ? cameraRig.GameplayCamera : Camera.main;
            if (cam == null)
                return;
            Ray ray = cam.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore))
                return;
            var target = hit.collider.GetComponentInParent<CharacterSelectTarget>();
            if (target != null)
                OnCharacterChosen(target.Character);
        }

        static void ClearLegacyDummies()
        {
            DestroyIfFound("WalkPreview");
            DestroyIfFound("SK_Male_Placeholder");
            DestroyIfFound("SK_Female_Placeholder");
            DestroyIfFound("_MeasureMale");
            DestroyIfFound("_GlbMeasure");
        }

        static void DestroyIfFound(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Destroy(go);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        static bool ClickPressed(out Vector2 screen)
        {
            screen = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return false;
            screen = mouse.position.ReadValue();
            return true;
#else
            if (!Input.GetMouseButtonDown(0))
                return false;
            screen = Input.mousePosition;
            return true;
#endif
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        void BuildRuntimeUi()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Transform uiRoot = transform;
            var existing = GameObject.Find("UI");
            if (existing != null)
                uiRoot = existing.transform;

            if (saloonSelectUi == null)
            {
                var canvas = CreateOverlay(uiRoot, "SaloonSelect", 22);
                AddLabel(canvas.transform, "Title", "THE LAST BET", new Vector2(0f, 360f), 42, new Color(0.92f, 0.80f, 0.48f), font);
                AddLabel(canvas.transform, "Subtitle", "CHOOSE YOUR SALOON", new Vector2(0f, 290f), 22, new Color(0.78f, 0.68f, 0.42f), font);
                Button classic = CreateCard(canvas.transform, "ClassicCard", "CLASSIC SALOON", new Vector2(-260f, -40f), font);
                Button western = CreateCard(canvas.transform, "WesternCard", "WESTERN SALOON", new Vector2(260f, -40f), font);
                saloonSelectUi = canvas.AddComponent<SaloonSelectUI>();
                saloonSelectUi.Bind(classic, western);
            }

            if (selectUi == null)
            {
                var canvas = CreateOverlay(uiRoot, "CharacterSelect", 20);
                AddLabel(canvas.transform, "Title", "THE LAST BET", new Vector2(0f, 360f), 42, new Color(0.92f, 0.80f, 0.48f), font);
                AddLabel(canvas.transform, "Subtitle", "THE BANDIT", new Vector2(0f, 290f), 22, new Color(0.78f, 0.68f, 0.42f), font);
                Button bandit = CreateCard(canvas.transform, "BanditCard", "BANDIT", new Vector2(0f, -40f), font);
                selectUi = canvas.AddComponent<CharacterSelectUI>();
                selectUi.Bind(null, null, bandit);
                canvas.SetActive(false);
            }

            if (hud == null)
            {
                var canvas = CreateOverlay(uiRoot, "Hud", 15);
                Text health = AddLabel(canvas.transform, "Health", "HEALTH  100 / 100", new Vector2(-700f, -460f), 22, new Color(0.90f, 0.78f, 0.46f), font);
                Text ammo = AddLabel(canvas.transform, "Ammo", "AMMO  6 / 6", new Vector2(700f, -460f), 22, new Color(0.90f, 0.78f, 0.46f), font);
                Text remaining = AddLabel(canvas.transform, "Remaining", "GUNSLINGERS  5", new Vector2(0f, 430f), 20, new Color(0.86f, 0.74f, 0.44f), font);
                Text sight = AddLabel(canvas.transform, "Crosshair", "+", Vector2.zero, 36, new Color(0.95f, 0.86f, 0.55f, 0.8f), font);
                hud = canvas.AddComponent<HudView>();
                hud.Bind(health, ammo, remaining, sight);
                canvas.SetActive(false);
            }

            if (result == null)
            {
                var canvas = CreateOverlay(uiRoot, "Result", 25);
                Text title = AddLabel(canvas.transform, "Title", "YOU STAND", new Vector2(0f, 70f), 72, new Color(0.96f, 0.86f, 0.55f), font);
                Text subtitle = AddLabel(canvas.transform, "Subtitle", "THE LAST BET IS YOURS", new Vector2(0f, -20f), 22, new Color(0.78f, 0.68f, 0.42f), font);
                Button again = CreateCard(canvas.transform, "PlayAgain", "PLAY AGAIN", new Vector2(0f, -180f), font);
                result = canvas.AddComponent<ResultView>();
                result.Bind(title, subtitle, again);
                canvas.SetActive(false);
            }
        }

        static GameObject CreateOverlay(Transform parent, string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static Text AddLabel(Transform parent, string name, string text, Vector2 anchored, int size, Color color, Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(900f, 120f);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.font = font;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        static Button CreateCard(Transform parent, string name, string label, Vector2 anchored, Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(320f, 180f);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.16f, 0.09f, 0.05f, 0.92f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.42f, 0.28f, 0.12f, 1f);
            colors.pressedColor = new Color(0.55f, 0.38f, 0.16f, 1f);
            button.colors = colors;
            AddLabel(go.transform, "Label", label, Vector2.zero, 28, new Color(0.92f, 0.80f, 0.48f), font);
            return button;
        }
    }
}
