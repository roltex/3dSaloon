#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace TheLastBet.Saloon.Editor
{
    internal static class SaloonAssetPaths
    {
        public const string Materials = "Assets/TheLastBet/Materials";
        public const string Textures = "Assets/TheLastBet/Art/Textures/Generated";
        public const string Meshes = "Assets/TheLastBet/Art/Meshes/Generated";
        public const string Prefabs = "Assets/TheLastBet/Prefabs/Saloon";
        public const string PropPrefabs = "Assets/TheLastBet/Prefabs/Saloon/Props";
        public const string Settings = "Assets/TheLastBet/Settings";
        public const string Scene = "Assets/TheLastBet/Scenes/SaloonScene.unity";
        public const string VolumeProfile = "Assets/TheLastBet/Settings/SaloonVolumeProfile.asset";
        public const string BarCounterModel = "Assets/Models/Bar Counter Optimized.glb";
        public const string EntranceModel = "Assets/Models/Entrance Gate Optimized.glb";
        public const string OvalPokerModel = "Assets/Models/Oval Poker Table Optimized.glb";
        public const string RoundPokerModel = "Assets/Models/Round Poker Table Optimized.glb";
        public const string RouletteModel = "Assets/Models/Roulette Table Optimized.glb";
        public const string SlotWallModel = "Assets/Models/Slot Machine Wall Optimized.glb";
        public const string LoungeModel = "Assets/Models/Lounge Area Optimized.glb";
        public const string DuelArenaModel = "Assets/Models/Duel Arena Optimized.glb";
        public const string GallowsModel = "Assets/Models/Gallows Optimized.glb";
        public const string CharacterPrefabs = "Assets/TheLastBet/Prefabs/Characters";
        public const string CharacterAnimations = "Assets/TheLastBet/Art/Animations";
        public const string MaleIdle = "Assets/Models/characters/newmale/Male Gunslinger V3 Rigged.glb";
        public const string MaleWalk = "Assets/Models/characters/newmale/Male Gunslinger V3 Rigged Walking.glb";
        public const string MaleRun = "Assets/Models/characters/newmale/Male Gunslinger V3 Rigged Running.glb";
        public const string MaleJump = "Assets/Models/characters/newmale/Male V3 Jump.glb";
        public const string MaleShoot = "Assets/Models/characters/newmale/Male V3 Shoot.glb";
        public const string MaleDeath = "Assets/Models/characters/newmale/Male V3 Death.glb";
        public const string FemaleIdle = "Assets/Models/characters/Female Gunslinger Rigged.glb";
        public const string FemaleWalk = "Assets/Models/characters/Female Gunslinger Rigged Walking.glb";
        public const string FemaleRun = "Assets/Models/characters/Female Gunslinger Rigged Running.glb";
        public const string FemaleJump = "Assets/Models/characters/Female Jump.glb";
        public const string FemaleShoot = "Assets/Models/characters/Female Shoot.glb";
        public const string FemaleDeath = "Assets/Models/characters/Female Death.glb";
        public const string MalePrefab = "Assets/TheLastBet/Prefabs/Characters/PF_MaleGunslinger.prefab";
        public const string FemalePrefab = "Assets/TheLastBet/Prefabs/Characters/PF_FemaleGunslinger.prefab";
        public const string BanditPrefab = "Assets/TheLastBet/Prefabs/Characters/PF_Bandit.prefab";
        public const string BanditVisual = "Assets/Bandit/Mesh/BanditMain.fbx";
        public const string MaleController = "Assets/TheLastBet/Art/Animations/AC_MaleGunslinger.controller";
        public const string FemaleController = "Assets/TheLastBet/Art/Animations/AC_FemaleGunslinger.controller";
        public const string BanditController = "Assets/TheLastBet/Art/Animations/AC_Bandit.controller";
        public const string HbmMaleIdle = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx";
        public const string HbmMaleWalk = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx";
        public const string HbmMaleRun = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx";
        public const string HbmMaleSprint = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Sprint/HumanM@Sprint01_Forward.fbx";
        public const string HbmMaleJump = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Jump01.fbx";
        public const string HbmMaleFall = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Fall01.fbx";
        public const string HbmFemaleIdle = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Idles/HumanF@Idle01.fbx";
        public const string HbmFemaleWalk = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Walk/HumanF@Walk01_Forward.fbx";
        public const string HbmFemaleRun = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Run/HumanF@Run01_Forward.fbx";
        public const string HbmFemaleSprint = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Sprint/HumanF@Sprint01_Forward.fbx";
        public const string HbmFemaleJump = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Jump/HumanF@Jump01.fbx";
        public const string HbmFemaleFall = "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Jump/HumanF@Fall01.fbx";
        public const string HbmGrip = "Assets/Kevin Iglesias/Human Animations/Animations/Masked Poses/Human@ObjectGripHands01.fbx";
        public const string Audio = "Assets/TheLastBet/Audio";
        public const string FireSound = "Assets/TheLastBet/Audio/Sfx_RevolverFire.wav";
        public const string ReloadSound = "Assets/TheLastBet/Audio/Sfx_RevolverReload.wav";
        public const string MalePortrait = "Assets/Models/characters/male.png";
        public const string FemalePortrait = "Assets/Models/characters/famale.png";
        public const string FirstPersonGun = "Assets/Models/characters/first person gun/Meshy_AI_model.glb";
        public const string WesternSaloon = "Assets/Models/day_5._finalthe_western_saloon.glb";

        public static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public static void RecreateFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                AssetDatabase.DeleteAsset(assetPath);
            EnsureFolder(assetPath);
        }

        public static void EnsureParentFolders()
        {
            EnsureFolder(Materials);
            EnsureFolder(Textures);
            EnsureFolder(Meshes);
            EnsureFolder(Prefabs);
            EnsureFolder(PropPrefabs);
            EnsureFolder(Settings);
            EnsureFolder("Assets/TheLastBet/Scenes");
            EnsureFolder(CharacterPrefabs);
            EnsureFolder(CharacterAnimations);
            EnsureFolder(Audio);
        }

        public static bool FileExists(string assetPath) => File.Exists(assetPath);
    }
}
#endif
