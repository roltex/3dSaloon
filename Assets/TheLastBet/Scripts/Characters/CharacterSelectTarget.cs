using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class CharacterSelectTarget : MonoBehaviour
    {
        [SerializeField] private PlayableCharacter character;

        public PlayableCharacter Character => character;

        public void Configure(PlayableCharacter id)
        {
            character = id;
        }
    }
}
