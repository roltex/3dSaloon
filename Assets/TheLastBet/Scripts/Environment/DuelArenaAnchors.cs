using UnityEngine;

namespace TheLastBet.Saloon
{
    /// <summary>
    /// Authoring references used by the duel gameplay/cinematic system.
    /// This component contains no duel win logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DuelArenaAnchors : MonoBehaviour
    {
        [Header("Competitors")]
        [SerializeField] private Transform playerA;
        [SerializeField] private Transform playerB;

        [Header("Cameras")]
        [SerializeField] private Transform introCamera;
        [SerializeField] private Transform gameplayCamera;
        [SerializeField] private Transform resultCameraA;
        [SerializeField] private Transform resultCameraB;

        [Header("Spectators")]
        [SerializeField] private Transform[] spectatorAnchors;

        public Transform PlayerA => playerA;
        public Transform PlayerB => playerB;

        public void Configure(Transform a, Transform b)
        {
            playerA = a;
            playerB = b;
        }
        public Transform IntroCamera => introCamera;
        public Transform GameplayCamera => gameplayCamera;
        public Transform ResultCameraA => resultCameraA;
        public Transform ResultCameraB => resultCameraB;
        public Transform[] SpectatorAnchors => spectatorAnchors;
    }
}
