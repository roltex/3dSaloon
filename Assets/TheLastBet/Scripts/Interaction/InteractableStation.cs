using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastBet.Saloon
{
    public enum SaloonStationType
    {
        Bar,
        PokerGreen,
        PokerRed,
        Roulette,
        Slots,
        CentralLounge,
        DuelArena,
        Gallows,
        Entrance,
        Custom
    }

    /// <summary>
    /// Generic authoring component for an interactable saloon station.
    /// It stores scene anchors only; game rules should live in dedicated gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractableStation : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string stationId = "station";
        [SerializeField] private string displayName = "Station";
        [SerializeField] private SaloonStationType stationType = SaloonStationType.Custom;

        [Header("Interaction")]
        [Min(0.25f)]
        [SerializeField] private float interactionRadius = 2.0f;
        [SerializeField] private bool stationEnabled = true;
        [SerializeField] private Collider interactionTrigger;

        [Header("Anchors")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform playerAnchor;
        [SerializeField] private Transform opponentAnchor;
        [SerializeField] private List<Transform> spectatorAnchors = new();
        [SerializeField] private Transform audioAnchor;

        public string StationId => stationId;
        public string DisplayName => displayName;
        public SaloonStationType StationType => stationType;
        public float InteractionRadius => interactionRadius;
        public bool IsEnabled => stationEnabled;
        public Collider InteractionTrigger => interactionTrigger;
        public Transform CameraAnchor => cameraAnchor;
        public Transform PlayerAnchor => playerAnchor;
        public Transform OpponentAnchor => opponentAnchor;
        public IReadOnlyList<Transform> SpectatorAnchors => spectatorAnchors;
        public Transform AudioAnchor => audioAnchor != null ? audioAnchor : transform;

        public event Action<InteractableStation, bool> EnabledChanged;

        public void ConfigureDuel(string id, Transform player, Transform opponent, float radius)
        {
            stationId = id;
            displayName = "Duel Ring";
            stationType = SaloonStationType.DuelArena;
            interactionRadius = radius;
            playerAnchor = player;
            opponentAnchor = opponent;
        }

        public void SetEnabled(bool value)
        {
            if (stationEnabled == value)
                return;

            stationEnabled = value;
            if (interactionTrigger != null)
                interactionTrigger.enabled = value;

            EnabledChanged?.Invoke(this, value);
        }

        public bool CanInteract(Vector3 worldPosition)
        {
            if (!stationEnabled)
                return false;

            Vector3 center = playerAnchor != null ? playerAnchor.position : transform.position;
            return Vector3.SqrMagnitude(worldPosition - center) <= interactionRadius * interactionRadius;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stationId))
                stationId = gameObject.name.ToLowerInvariant().Replace(' ', '-');

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = gameObject.name;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(playerAnchor != null ? playerAnchor.position : transform.position, interactionRadius);
        }
#endif
    }
}
