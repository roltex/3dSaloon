using System.Collections.Generic;
using UnityEngine;

namespace JohnStairs.RPG.Character.Cam.Subcomponents {
    public interface IOcclusionHandler {
        /// <summary>
        /// Computes the closest hit distance for the given object hits
        /// </summary>
        /// <param name="objectHits">Object hits to process</param>
        /// <param name="objectsInBetween">Objects before the closest object hit</param>
        /// <returns></returns>
        float GetClosestHitDistance(RaycastHit[] objectHits, out List<GameObject> objectsInBetween);

        /// <summary>
        /// Handles the visibility of the given objects
        /// </summary>
        /// <param name="objects">Objects to process</param>
        void HandleObjectVisibility(List<GameObject> objects);
    }
}
