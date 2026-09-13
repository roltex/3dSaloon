using UnityEditor;
using UnityEngine;

namespace JohnStairs.RPG.Character.Controller {
    [CustomEditor(typeof(RPGController))]
    public class RPGCameraEditor : Editor {
        static bool showCameraControlSettings = true;
        static bool showShakingSettings = true;

        RPGController Component;
        SerializedProperty Script;

        #region Camera related variables
        SerializedProperty InvertYawAxis;
        SerializedProperty InvertPitchAxis;
        SerializedProperty YawSensitivity;
        SerializedProperty PitchSensitivity;
        SerializedProperty ZoomSensitivity;
        SerializedProperty SyncRotations;
        SerializedProperty AlignCameraOnCharacterMovement;
        #endregion        

        #region Shaking variables
        SerializedProperty ShakeFrequency;
        SerializedProperty ShakeAmplitude;
        SerializedProperty ShakeAmplitudeVariance;
        #endregion

        public void OnEnable() {
            Script = serializedObject.FindProperty("m_Script");
            Component = (RPGController)serializedObject.targetObject;

            #region Camera related variables
            InvertYawAxis = serializedObject.FindProperty("InvertYawAxis");
            InvertPitchAxis = serializedObject.FindProperty("InvertPitchAxis");
            YawSensitivity = serializedObject.FindProperty("YawSensitivity");
            PitchSensitivity = serializedObject.FindProperty("PitchSensitivity");
            ZoomSensitivity = serializedObject.FindProperty("ZoomSensitivity");
            SyncRotations = serializedObject.FindProperty("SyncRotations");
            AlignCameraOnCharacterMovement = serializedObject.FindProperty("AlignCameraOnCharacterMovement");
            #endregion

            #region Shaking variables
            ShakeFrequency = serializedObject.FindProperty("ShakeFrequency");
            ShakeAmplitude = serializedObject.FindProperty("ShakeAmplitude");
            ShakeAmplitudeVariance = serializedObject.FindProperty("ShakeAmplitudeVariance");
            #endregion
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(Script);
            GUI.enabled = true;

            if (GUILayout.Button("Check input setup")) {
                Debug.Log("> Input setup check started");
                Component.InitializeInputActions(true);
                Debug.Log("> Input setup check done. If there were no warnings logged, all inputs could be found");
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Camera related settings
            showCameraControlSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCameraControlSettings, "Camera control");
            if (showCameraControlSettings) {
                EditorGUILayout.PropertyField(InvertYawAxis);
                EditorGUILayout.PropertyField(YawSensitivity);
                EditorGUILayout.PropertyField(InvertPitchAxis);
                EditorGUILayout.PropertyField(PitchSensitivity);
                EditorGUILayout.PropertyField(ZoomSensitivity);
                EditorGUILayout.PropertyField(SyncRotations);
                EditorGUILayout.PropertyField(AlignCameraOnCharacterMovement, new GUIContent("Align On Character Movement"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Shaking settings
            showShakingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showShakingSettings, "Camera shaking");
            if (showShakingSettings) {
                EditorGUILayout.PropertyField(ShakeFrequency);
                EditorGUILayout.PropertyField(ShakeAmplitude);
                EditorGUILayout.PropertyField(ShakeAmplitudeVariance);
                string buttonText = "Enter play mode first";
                if (Application.isPlaying) {
                    buttonText = Component.IsShakingCamera() ? "Stop simulation" : "Start simulation";
                }
                if (GUILayout.Button(buttonText)) {
                    ToggleShakingSimulation(Component);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void ToggleShakingSimulation(RPGController rpgController) {
            if (!Application.isPlaying) {
                Debug.Log("You have to enter play mode first to be able to simulate camera shaking");
                return;
            }

            if (rpgController.IsShakingCamera()) {
                rpgController.StopShakingCamera();
            } else {
                rpgController.StartShakingCamera(ShakeFrequency.floatValue, ShakeAmplitude.floatValue, ShakeAmplitudeVariance.floatValue);
            }
        }
    }
}