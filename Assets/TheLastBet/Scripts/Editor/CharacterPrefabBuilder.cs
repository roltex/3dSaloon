#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TheLastBet.Saloon.Editor.SaloonBuildUtility;

namespace TheLastBet.Saloon.Editor
{
    internal static class CharacterPrefabBuilder
    {
        [MenuItem("The Last Bet/Rebuild Characters")]
        public static void RebuildCharacters()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[The Last Bet] Stop Play Mode before rebuilding characters.");
                return;
            }

            BuildAssets();
            if (EditorSceneManager.GetActiveScene().path != SaloonAssetPaths.Scene)
                EditorSceneManager.OpenScene(SaloonAssetPaths.Scene, OpenSceneMode.Single);
            SinglePlayerSceneWire.AddToOpenScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[The Last Bet] Character prefabs, animators, and single-player wiring are ready.");
        }

        public static void BuildAssets()
        {
            SaloonAssetPaths.EnsureParentFolders();
            EnsureLayers();
            BuildAudioClips();
            UpgradeBanditMaterialsToUrp();

            BuildPlayable(
                "Bandit",
                "PF_Bandit",
                SaloonAssetPaths.BanditVisual,
                SaloonAssetPaths.HbmMaleIdle,
                SaloonAssetPaths.HbmMaleWalk,
                SaloonAssetPaths.HbmMaleRun,
                SaloonAssetPaths.HbmMaleSprint,
                SaloonAssetPaths.HbmMaleJump,
                SaloonAssetPaths.HbmGrip,
                SaloonAssetPaths.HbmMaleFall,
                SaloonAssetPaths.BanditController,
                SaloonAssetPaths.BanditPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void EnsureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            EnsureLayerSlot(layers, "Player");
            EnsureLayerSlot(layers, "Enemy");
            EnsureLayerSlot(layers, "Viewmodel");
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureLayerSlot(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == name)
                    return;
            }

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue))
                    continue;
                slot.stringValue = name;
                return;
            }
        }

        static void BuildPlayable(
            string id,
            string rootName,
            string visualPath,
            string idlePath,
            string walkPath,
            string runPath,
            string sprintPath,
            string jumpPath,
            string shootPath,
            string deathPath,
            string controllerPath,
            string prefabPath)
        {
            AnimationClip idle = FindClipInAsset(idlePath);
            AnimationClip walk = FindClipInAsset(walkPath);
            AnimationClip run = FindClipInAsset(runPath);
            AnimationClip sprint = FindClipInAsset(sprintPath);
            AnimationClip jump = FindClipInAsset(jumpPath);
            AnimationClip shoot = FindClipInAsset(shootPath);
            AnimationClip death = FindClipInAsset(deathPath);
            if (walk == null || run == null)
            {
                Debug.LogError($"[The Last Bet] Missing Human Basic Motions clips for {id}.");
                return;
            }

            if (idle == null)
                idle = walk;
            if (sprint == null)
                sprint = run;
            if (jump == null)
                jump = walk;
            if (shoot == null)
                shoot = idle;
            if (death == null)
                death = jump;

            AnimatorController controller = BuildController(controllerPath, idle, walk, run, sprint, jump, shoot, death);
            BuildPrefab(id, rootName, visualPath, controller, prefabPath);
        }

        static AnimatorController BuildController(
            string path,
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip sprint,
            AnimationClip jump,
            AnimationClip shoot,
            AnimationClip death)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Shoot", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            var blend = new BlendTree
            {
                name = "Locomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(blend, controller);
            blend.AddChild(idle, 0f);
            blend.AddChild(walk, 2.2f);
            blend.AddChild(run, 4.8f);
            blend.AddChild(sprint, 6.4f);

            AnimatorState loco = root.AddState("Locomotion");
            loco.motion = blend;
            loco.iKOnFeet = true;
            root.defaultState = loco;

            AnimatorState jumpState = root.AddState("Jump");
            jumpState.motion = jump;
            jumpState.iKOnFeet = true;

            AnimatorState shootState = root.AddState("Shoot");
            shootState.motion = shoot;
            shootState.speed = 4f;

            AnimatorState deathState = root.AddState("Death");
            deathState.motion = death;

            AnimatorStateTransition locoJump = loco.AddTransition(jumpState);
            locoJump.hasExitTime = false;
            locoJump.hasFixedDuration = true;
            locoJump.duration = 0.08f;
            locoJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

            AnimatorStateTransition jumpLoco = jumpState.AddTransition(loco);
            jumpLoco.hasExitTime = true;
            jumpLoco.exitTime = 0.82f;
            jumpLoco.hasFixedDuration = true;
            jumpLoco.duration = 0.12f;

            AnimatorStateTransition locoShoot = loco.AddTransition(shootState);
            locoShoot.hasExitTime = false;
            locoShoot.hasFixedDuration = true;
            locoShoot.duration = 0.06f;
            locoShoot.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");

            AnimatorStateTransition shootLoco = shootState.AddTransition(loco);
            shootLoco.hasExitTime = true;
            shootLoco.exitTime = 0.88f;
            shootLoco.hasFixedDuration = true;
            shootLoco.duration = 0.1f;

            AnimatorStateTransition anyDeath = root.AddAnyStateTransition(deathState);
            anyDeath.hasExitTime = false;
            anyDeath.hasFixedDuration = true;
            anyDeath.duration = 0.12f;
            anyDeath.canTransitionToSelf = false;
            anyDeath.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        static void BuildPrefab(string id, string rootName, string visualPath, RuntimeAnimatorController controller, string prefabPath)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);
            if (source == null)
            {
                Debug.LogError("[The Last Bet] Missing character model at " + visualPath);
                return;
            }

            var root = new GameObject(rootName);
            var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (visual == null)
                visual = Object.Instantiate(source);
            if (PrefabUtility.IsPartOfPrefabInstance(visual))
                PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            foreach (var listener in visual.GetComponentsInChildren<AudioListener>(true))
                Object.DestroyImmediate(listener);
            foreach (var cam in visual.GetComponentsInChildren<Camera>(true))
                Object.DestroyImmediate(cam);

            var animator = visual.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = controller;
            GroundVisualToFloor(visual);
            string avatarPath = $"{SaloonAssetPaths.CharacterAnimations}/AV_{id}.asset";
            bool keepImported = id == "Bandit" && animator.avatar != null && animator.avatar.isHuman && animator.avatar.isValid;
            if (!keepImported)
            {
                Avatar avatar = BuildHumanoidAvatar(visual, avatarPath);
                if (avatar != null)
                    animator.avatar = avatar;
            }

            var muzzle = Create("Muzzle", root);
            muzzle.transform.localPosition = new Vector3(0.18f, 1.35f, 0.38f);

            var driver = root.AddComponent<CharacterAnimDriver>();
            var driverSo = new SerializedObject(driver);
            driverSo.FindProperty("animator").objectReferenceValue = animator;
            driverSo.ApplyModifiedPropertiesWithoutUndo();

            root.AddComponent<Health>();
            var weapon = root.AddComponent<HitscanWeapon>();
            var weaponSo = new SerializedObject(weapon);
            weaponSo.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            weaponSo.FindProperty("fireClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SaloonAssetPaths.FireSound);
            weaponSo.FindProperty("reloadClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SaloonAssetPaths.ReloadSound);
            weaponSo.ApplyModifiedPropertiesWithoutUndo();

            FitCharacterController(root, visual);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                AssetDatabase.DeleteAsset(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        static void GroundVisualToFloor(GameObject visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            Bounds bounds = EncapsulateRenderers(visual);
            visual.transform.position += new Vector3(0f, -bounds.min.y, 0f);
        }

        static void FitCharacterController(GameObject root, GameObject visual)
        {
            var controller = root.GetComponent<CharacterController>();
            if (controller == null)
                controller = root.AddComponent<CharacterController>();

            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.radius = 0.28f;
            controller.skinWidth = 0.03f;
            controller.minMoveDistance = 0f;
            controller.stepOffset = 0.45f;
            controller.slopeLimit = 50f;
        }

        static AnimationClip FindClipInAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) == null)
                return null;

            AnimationClip best = null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset is not AnimationClip clip || clip.name.StartsWith("__preview"))
                    continue;
                if (best == null || clip.length > best.length)
                    best = clip;
            }

            return best;
        }

        static Avatar BuildHumanoidAvatar(GameObject visual, string assetPath)
        {
            Transform[] transforms = visual.GetComponentsInChildren<Transform>(true);
            var skeleton = new SkeletonBone[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                skeleton[i] = new SkeletonBone
                {
                    name = t.name,
                    position = t.localPosition,
                    rotation = t.localRotation,
                    scale = t.localScale
                };
            }

            var humans = new System.Collections.Generic.List<HumanBone>();
            string[] traitNames = HumanTrait.BoneName;
            for (int i = 0; i < traitNames.Length; i++)
            {
                if (IsFingerTrait(traitNames[i]))
                    continue;
                Transform bone = FindHumanBone(visual.transform, traitNames[i]);
                if (bone == null)
                    continue;
                humans.Add(new HumanBone
                {
                    humanName = traitNames[i],
                    boneName = bone.name,
                    limit = LimitForBone(traitNames[i])
                });
            }

            var description = new HumanDescription
            {
                human = humans.ToArray(),
                skeleton = skeleton,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };

            Avatar avatar = AvatarBuilder.BuildHumanAvatar(visual, description);
            if (avatar == null || !avatar.isValid)
            {
                Debug.LogWarning("[The Last Bet] Could not build a Humanoid avatar for " + visual.name);
                return null;
            }

            if (AssetDatabase.LoadAssetAtPath<Avatar>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(avatar, assetPath);
            return AssetDatabase.LoadAssetAtPath<Avatar>(assetPath);
        }

        static Transform FindHumanBone(Transform root, string humanName)
        {
            string key = NormalizeBone(humanName);
            Transform inverted = FindInvertedSpineBone(root, key);
            if (inverted != null)
                return inverted;

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (NormalizeBone(all[i].name) == key)
                    return all[i];
            }

            string[] aliases = BoneAliases(key);
            for (int a = 0; a < aliases.Length; a++)
            {
                string alias = aliases[a];
                for (int i = 0; i < all.Length; i++)
                {
                    if (NormalizeBone(all[i].name) == alias)
                        return all[i];
                }
            }

            return null;
        }

        // Mixamo-style gunslinger: Hips > Spine02 > Spine01 > Spine (shoulders/neck).
        static Transform FindInvertedSpineBone(Transform root, string key)
        {
            Transform spine02 = FindExactBone(root, "spine02");
            Transform spine01 = FindExactBone(root, "spine01");
            Transform spine = FindExactBone(root, "spine");
            if (spine02 == null || spine01 == null || spine == null)
                return null;
            if (!IsAncestor(spine02, spine01) || !IsAncestor(spine01, spine))
                return null;

            switch (key)
            {
                case "spine":
                    return spine02;
                case "chest":
                    return spine01;
                case "upperchest":
                    return spine;
                default:
                    return null;
            }
        }

        static Transform FindExactBone(Transform root, string normalizedName)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (NormalizeBone(all[i].name) == normalizedName)
                    return all[i];
            }

            return null;
        }

        static bool IsAncestor(Transform ancestor, Transform descendant)
        {
            Transform current = descendant;
            while (current != null)
            {
                if (current == ancestor)
                    return true;
                current = current.parent;
            }

            return false;
        }

        static string[] BoneAliases(string normalizedHumanName)
        {
            switch (normalizedHumanName)
            {
                case "hips": return new[] { "pelvis", "hip", "root" };
                case "spine": return new[] { "spine1", "spine01" };
                case "chest": return new[] { "spine2", "spine02" };
                case "upperchest": return new[] { "spine3", "spine03" };
                case "neck": return new[] { "neck01", "neck1" };
                case "head": return new[] { "head01" };
                case "leftupperleg": return new[] { "thighl", "lthigh", "leftupleg", "upperlegl" };
                case "rightupperleg": return new[] { "thighr", "rthigh", "rightupleg", "upperlegr" };
                case "leftlowerleg": return new[] { "calfl", "llowleg", "leftleg", "lowerlegl" };
                case "rightlowerleg": return new[] { "calfr", "rlowleg", "rightleg", "lowerlegr" };
                case "leftfoot": return new[] { "footl", "lfoot" };
                case "rightfoot": return new[] { "footr", "rfoot" };
                case "lefttoes": return new[] { "lefttoebase", "toebasel" };
                case "righttoes": return new[] { "righttoebase", "toebaser" };
                case "leftshoulder": return new[] { "claviclel", "lclavicle" };
                case "rightshoulder": return new[] { "clavicler", "rclavicle" };
                case "leftupperarm": return new[] { "upperarml", "lupperarm", "leftarm" };
                case "rightupperarm": return new[] { "upperarmr", "rupperarm", "rightarm" };
                case "leftlowerarm": return new[] { "lowerarml", "llowerarm", "leftforearm" };
                case "rightlowerarm": return new[] { "lowerarmr", "rlowerarm", "rightforearm" };
                case "lefthand": return new[] { "handl", "lhand" };
                case "righthand": return new[] { "handr", "rhand" };
                default: return System.Array.Empty<string>();
            }
        }

        static bool IsFingerTrait(string humanName)
        {
            string key = NormalizeBone(humanName);
            return key.Contains("thumb") || key.Contains("index") || key.Contains("middle") ||
                   key.Contains("ring") || key.Contains("little");
        }

        static HumanLimit LimitForBone(string humanName)
        {
            string key = NormalizeBone(humanName);
            if (key == "lefthand" || key == "righthand")
            {
                return new HumanLimit
                {
                    useDefaultValues = false,
                    min = Vector3.zero,
                    max = Vector3.zero,
                    center = Vector3.zero,
                    axisLength = 0.08f
                };
            }

            return new HumanLimit { useDefaultValues = true };
        }

        static void UpgradeBanditMaterialsToUrp()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                Debug.LogWarning("[The Last Bet] URP Lit shader missing; Bandit materials were not upgraded.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Bandit" });
            int upgraded = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null)
                    continue;
                if (material.shader == lit || material.shader.name.Contains("Universal Render Pipeline"))
                    continue;

                Texture albedo = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
                Texture bump = material.HasProperty("_BumpMap") ? material.GetTexture("_BumpMap") : null;
                Texture metallic = material.HasProperty("_MetallicGlossMap") ? material.GetTexture("_MetallicGlossMap") : null;
                Texture occlusion = material.HasProperty("_OcclusionMap") ? material.GetTexture("_OcclusionMap") : null;
                Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
                bool cutout = material.IsKeywordEnabled("_ALPHATEST_ON") ||
                              material.name.IndexOf("hair", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                              material.name.IndexOf("lash", System.StringComparison.OrdinalIgnoreCase) >= 0;
                bool twoSided = material.shader.name.IndexOf("DoubleSided", System.StringComparison.OrdinalIgnoreCase) >= 0;

                material.shader = lit;
                if (material.HasProperty("_BaseMap") && albedo != null)
                    material.SetTexture("_BaseMap", albedo);
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", color);
                if (material.HasProperty("_BumpMap") && bump != null)
                    material.SetTexture("_BumpMap", bump);
                if (material.HasProperty("_MetallicGlossMap") && metallic != null)
                    material.SetTexture("_MetallicGlossMap", metallic);
                if (material.HasProperty("_OcclusionMap") && occlusion != null)
                    material.SetTexture("_OcclusionMap", occlusion);
                if (cutout && material.HasProperty("_AlphaClip"))
                {
                    material.SetFloat("_AlphaClip", 1f);
                    material.EnableKeyword("_ALPHATEST_ON");
                    if (material.HasProperty("_Cutoff"))
                        material.SetFloat("_Cutoff", 0.4f);
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                }

                if (twoSided && material.HasProperty("_Cull"))
                    material.SetFloat("_Cull", 0f);

                EditorUtility.SetDirty(material);
                upgraded++;
            }

            if (upgraded > 0)
                Debug.Log("[The Last Bet] Upgraded " + upgraded + " Bandit materials to URP Lit.");
        }

        static string NormalizeBone(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            string n = name.ToLowerInvariant();
            int colon = n.LastIndexOf(':');
            if (colon >= 0 && colon < n.Length - 1)
                n = n.Substring(colon + 1);
            return n.Replace(" ", string.Empty).Replace("_", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
        }

        static AnimationClip FindSourceClip(string glbPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(glbPath) == null)
            {
                Debug.LogError("[The Last Bet] Missing character GLB: " + glbPath);
                return null;
            }

            AnimationClip best = null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(glbPath))
            {
                if (asset is not AnimationClip clip || clip.name.StartsWith("__preview"))
                    continue;
                if (best == null || clip.length > best.length)
                    best = clip;
            }

            if (best == null)
                Debug.LogError("[The Last Bet] No animation clip in " + glbPath);
            return best;
        }

        static AnimationClip MakePose(AnimationClip source, string destPath)
        {
            if (source == null)
                return null;
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath) != null)
                AssetDatabase.DeleteAsset(destPath);

            var pose = new AnimationClip { name = Path.GetFileNameWithoutExtension(destPath), legacy = false };
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
                if (curve == null || curve.length == 0)
                    continue;
                float value = curve.keys[0].value;
                AnimationUtility.SetEditorCurve(pose, binding, new AnimationCurve(new Keyframe(0f, value), new Keyframe(0.5f, value)));
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
            {
                ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(source, binding);
                if (keys == null || keys.Length == 0)
                    continue;
                AnimationUtility.SetObjectReferenceCurve(pose, binding, new[]
                {
                    new ObjectReferenceKeyframe { time = 0f, value = keys[0].value },
                    new ObjectReferenceKeyframe { time = 0.5f, value = keys[0].value }
                });
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(pose);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(pose, settings);
            AssetDatabase.CreateAsset(pose, destPath);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath);
        }

        static AnimationClip CopyClip(AnimationClip source, string destPath, bool loop)
        {
            if (source == null)
                return null;
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath) != null)
                AssetDatabase.DeleteAsset(destPath);

            var copy = Object.Instantiate(source);
            copy.name = Path.GetFileNameWithoutExtension(destPath);
            copy.legacy = false;
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(copy);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(copy, settings);
            AssetDatabase.CreateAsset(copy, destPath);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath);
        }

        static bool ClipFitsHierarchy(AnimationClip clip, string modelPath)
        {
            if (clip == null)
                return false;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
                return false;

            var probe = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (PrefabUtility.IsPartOfPrefabInstance(probe))
                PrefabUtility.UnpackPrefabInstance(probe, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            Transform root = probe.transform;
            var animator = probe.GetComponentInChildren<Animator>();
            if (animator != null)
                root = animator.transform;

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            int transformBindings = 0;
            int matched = 0;
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i].type != typeof(Transform))
                    continue;
                transformBindings++;
                if (string.IsNullOrEmpty(bindings[i].path) || root.Find(bindings[i].path) != null)
                    matched++;
            }

            Object.DestroyImmediate(probe);
            return transformBindings == 0 || matched >= transformBindings * 0.55f;
        }

        static void BuildAudioClips()
        {
            WriteToneWav(SaloonAssetPaths.FireSound, 0.16f, (t, i) =>
            {
                float env = Mathf.Exp(-t * 26f);
                float bang = Mathf.Sin(t * 180f * Mathf.PI * 2f) * 0.35f;
                float click = Mathf.Sin(t * 920f * Mathf.PI * 2f) * 0.18f;
                float grit = (Hash(i) * 2f - 1f) * 0.55f;
                return env * (bang + click + grit);
            });
            WriteToneWav(SaloonAssetPaths.ReloadSound, 0.28f, (t, i) =>
            {
                float env = Mathf.Exp(-Mathf.Abs(t - 0.08f) * 18f);
                return env * (Mathf.Sin(t * 640f * Mathf.PI * 2f) * 0.4f + Mathf.Sin(t * 1400f * Mathf.PI * 2f) * 0.12f);
            });
            AssetDatabase.ImportAsset(SaloonAssetPaths.FireSound);
            AssetDatabase.ImportAsset(SaloonAssetPaths.ReloadSound);
        }

        static void WriteToneWav(string assetPath, float seconds, System.Func<float, int, float> sample)
        {
            const int hz = 44100;
            int count = Mathf.RoundToInt(hz * seconds);
            string full = Path.GetFullPath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full) ?? string.Empty);
            using var stream = new FileStream(full, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            int byteCount = count * 2;
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + byteCount);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(hz);
            writer.Write(hz * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(byteCount);
            for (int i = 0; i < count; i++)
            {
                float v = Mathf.Clamp(sample(i / (float)hz, i), -1f, 1f);
                writer.Write((short)Mathf.RoundToInt(v * 32767f));
            }
        }

        static float Hash(int value)
        {
            uint x = (uint)value * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return (x & 0xFFFF) / 65535f;
        }
    }
}
#endif
