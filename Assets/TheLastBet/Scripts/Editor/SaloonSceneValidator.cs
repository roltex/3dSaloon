#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

namespace TheLastBet.Saloon.Editor
{
    public static class SaloonSceneValidator
    {
        [UnityEditor.MenuItem("The Last Bet/Validate Saloon Scene")]
        public static void ValidateScene()
        {
            int errors = 0;
            int warnings = 0;

            InteractableStation[] stations = UnityEngine.Object.FindObjectsByType<InteractableStation>(
                FindObjectsInactive.Include);

            if (stations.Length == 0)
            {
                Debug.LogWarning("[The Last Bet] No InteractableStation components found in the open scene.");
                return;
            }

            var duplicateIds = stations
                .GroupBy(s => s.StationId)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (duplicateIds.Length > 0)
            {
                Debug.LogError("[The Last Bet] Duplicate station IDs: " + string.Join(", ", duplicateIds));
                errors++;
            }

            foreach (InteractableStation station in stations)
            {
                if (station.CameraAnchor == null)
                {
                    Debug.LogWarning($"[The Last Bet] {station.name}: camera anchor is missing.", station);
                    warnings++;
                }

                if (station.PlayerAnchor == null)
                {
                    Debug.LogWarning($"[The Last Bet] {station.name}: player anchor is missing.", station);
                    warnings++;
                }
            }

            SaloonStationType[] required =
            {
                SaloonStationType.Bar,
                SaloonStationType.PokerGreen,
                SaloonStationType.PokerRed,
                SaloonStationType.Roulette,
                SaloonStationType.Slots,
                SaloonStationType.CentralLounge,
                SaloonStationType.DuelArena,
                SaloonStationType.Gallows,
                SaloonStationType.Entrance
            };

            foreach (SaloonStationType type in required)
            {
                if (stations.All(s => s.StationType != type))
                {
                    Debug.LogError($"[The Last Bet] Missing required station type: {type}");
                    errors++;
                }
            }

            InteractableStation lounge = stations.FirstOrDefault(s => s.StationType == SaloonStationType.CentralLounge);
            if (lounge != null)
            {
                Transform[] children = lounge.GetComponentsInChildren<Transform>(true);
                Transform chandelier = children.FirstOrDefault(t =>
                    t.name.IndexOf("Chandelier", StringComparison.OrdinalIgnoreCase) >= 0);
                if (chandelier != null)
                {
                    Debug.LogError("[The Last Bet] Central lounge contains a chandelier.", chandelier);
                    errors++;
                }
            }

            DuelArenaAnchors duel = UnityEngine.Object.FindAnyObjectByType<DuelArenaAnchors>(FindObjectsInactive.Include);
            if (duel == null)
            {
                Debug.LogError("[The Last Bet] DuelArenaAnchors is missing from the open scene.");
                errors++;
            }
            else
            {
                if (duel.PlayerA == null || duel.PlayerB == null)
                {
                    Debug.LogError("[The Last Bet] Duel competitor anchors are missing.", duel);
                    errors++;
                }

                if (duel.IntroCamera == null || duel.GameplayCamera == null ||
                    duel.ResultCameraA == null || duel.ResultCameraB == null)
                {
                    Debug.LogError("[The Last Bet] Duel camera anchors are incomplete.", duel);
                    errors++;
                }
            }

            if (errors == 0)
                Debug.Log($"[The Last Bet] Saloon validation complete. Stations found: {stations.Length}. Warnings: {warnings}.");
            else
                Debug.LogError($"[The Last Bet] Saloon validation failed. Errors: {errors}. Warnings: {warnings}. Stations: {stations.Length}.");
        }
    }
}
#endif
