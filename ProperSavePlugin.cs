using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HG;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using UnityEngine;
using Zio;
using Zio.FileSystems;
using Bandito;

namespace ProperSave
{
	// Token: 0x0200000A RID: 10
	[BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "2.11.2")]
	public class ProperSavePlugin : BaseUnityPlugin
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00003DEE File Offset: 0x00001FEE
		// (set) Token: 0x0600003C RID: 60 RVA: 0x00003DF5 File Offset: 0x00001FF5
		internal static ProperSavePlugin Instance { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00003DFD File Offset: 0x00001FFD
		internal static ManualLogSource InstanceLogger
		{
			get
			{
				ProperSavePlugin instance = ProperSavePlugin.Instance;
				if (instance == null)
				{
					return null;
				}
				return instance.Logger;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600003E RID: 62 RVA: 0x00003E0F File Offset: 0x0000200F
		// (set) Token: 0x0600003F RID: 63 RVA: 0x00003E16 File Offset: 0x00002016
		internal static FileSystem SavesFileSystem { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00003E1E File Offset: 0x0000201E
		// (set) Token: 0x06000041 RID: 65 RVA: 0x00003E25 File Offset: 0x00002025
		internal static UPath SavesPath { get; private set; } = "/ProperSave" / "Saves";

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00003E2D File Offset: 0x0000202D
		// (set) Token: 0x06000043 RID: 67 RVA: 0x00003E34 File Offset: 0x00002034
		private static string SavesDirectory { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00003E3C File Offset: 0x0000203C
		// (set) Token: 0x06000045 RID: 69 RVA: 0x00003E43 File Offset: 0x00002043
		internal static SaveFile CurrentSave { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00003E4B File Offset: 0x0000204B
		// (set) Token: 0x06000047 RID: 71 RVA: 0x00003E52 File Offset: 0x00002052
		internal static string ContentHash { get; private set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00003E5A File Offset: 0x0000205A
		// (set) Token: 0x06000049 RID: 73 RVA: 0x00003E61 File Offset: 0x00002061
		internal static ConfigEntry<bool> UseCloudStorage { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00003E69 File Offset: 0x00002069
		// (set) Token: 0x0600004B RID: 75 RVA: 0x00003E70 File Offset: 0x00002070
		internal static ConfigEntry<string> CloudStorageSubDirectory { get; private set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00003E78 File Offset: 0x00002078
		// (set) Token: 0x0600004D RID: 77 RVA: 0x00003E7F File Offset: 0x0000207F
		internal static ConfigEntry<string> UserSavesDirectory { get; private set; }

		// Token: 0x0600004E RID: 78 RVA: 0x00003E88 File Offset: 0x00002088
		private void Start()
		{
			ProperSavePlugin.Instance = this;
			ProperSavePlugin.UseCloudStorage = base.Config.Bind<bool>("Main", "UseCloudStorage", false, "Store files in Steam/EpicGames cloud. Enabling this feature would not preserve current saves and disabling it wouldn't clear the cloud.");
			ProperSavePlugin.CloudStorageSubDirectory = base.Config.Bind<string>("Main", "CloudStorageSubDirectory", "", "Sub directory name for cloud storage. Changing it allows to use different save files for different mod profiles.");
			ProperSavePlugin.UserSavesDirectory = base.Config.Bind<string>("Main", "SavesDirectory", "", "Directory where save files will be stored. \"ProperSave\" directory will be created in the directory you have specified. If the directory doesn't exist the default one will be used.");
			RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(delegate()
			{
				if (ProperSavePlugin.UseCloudStorage.Value)
				{
					ProperSavePlugin.SavesFileSystem = RoR2Application.cloudStorage;
					if (!string.IsNullOrWhiteSpace(ProperSavePlugin.CloudStorageSubDirectory.Value))
					{
						if (ProperSavePlugin.CloudStorageSubDirectory.Value.IndexOfAny(ProperSavePlugin.invalidSubDirectoryCharacters) != -1)
						{
							base.Logger.LogError("Config entry \"CloudStorageSubDirectory\" contains invalid characters. Falling back to default location.");
						}
						else
						{
							ProperSavePlugin.SavesPath /= ProperSavePlugin.CloudStorageSubDirectory.Value;
						}
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(ProperSavePlugin.UserSavesDirectory.Value))
					{
						if (!Directory.Exists(ProperSavePlugin.UserSavesDirectory.Value))
						{
							base.Logger.LogError("SavesDirectory from the config doesn't exists, using Application.persistentDataPath");
							ProperSavePlugin.SavesDirectory = Application.persistentDataPath;
						}
						else
						{
							ProperSavePlugin.SavesDirectory = ProperSavePlugin.UserSavesDirectory.Value;
						}
					}
					else
					{
						ProperSavePlugin.SavesDirectory = Application.persistentDataPath;
					}
					if (string.IsNullOrWhiteSpace(ProperSavePlugin.SavesDirectory))
					{
						base.Logger.LogError("Application.persistentDataPath is empty. Use SavesDirectory config option to specify a folder.");
					}
					PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
					ProperSavePlugin.SavesFileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(ProperSavePlugin.SavesDirectory), true);
				}
				SaveFileMetadata.PopulateSavesMetadata();
			}));
			ModSupport.GatherLoadedPlugins();
			ModSupport.RegisterHooks();
			Saving.RegisterHooks();
			Loading.RegisterHooks();
			LobbyUI.RegisterHooks();
			LostNetworkUser.Subscribe();
			Language.collectLanguageRootFolders += this.CollectLanguageRootFolders;
			ContentManager.onContentPacksAssigned += this.ContentManagerOnContentPacksAssigned;
      			SaveFile.OnGatherSaveData += GatherPersistentDesperadoData;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003F64 File Offset: 0x00002164
		private void Destroy()
		{
			ProperSavePlugin.Instance = null;
			ModSupport.UnregisterHooks();
			Saving.UnregisterHooks();
			Loading.UnregisterHooks();
			LobbyUI.UnregisterHooks();
			LostNetworkUser.Unsubscribe();
			Language.collectLanguageRootFolders -= this.CollectLanguageRootFolders;
			ContentManager.onContentPacksAssigned -= this.ContentManagerOnContentPacksAssigned;
      			SaveFile.OnGatherSaveData -= GatherPersistentDesperadoData;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003FB2 File Offset: 0x000021B2
		public void CollectLanguageRootFolders(List<string> folders)
		{
			folders.Add(Path.Combine(Path.GetDirectoryName(base.Info.Location), "Language"));
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003FD4 File Offset: 0x000021D4
		private void ContentManagerOnContentPacksAssigned(ReadOnlyArray<ReadOnlyContentPack> contentPacks)
		{
			MD5 md = MD5.Create();
			ProperSavePlugin.<>c__DisplayClass45_0 CS$<>8__locals1;
			CS$<>8__locals1.writer = new StringWriter();
			try
			{
				foreach (ReadOnlyContentPack readOnlyContentPack in contentPacks)
				{
					CS$<>8__locals1.writer.Write(readOnlyContentPack.identifier);
					CS$<>8__locals1.writer.Write(';');
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<ArtifactDef>(readOnlyContentPack.artifactDefs, "artifactDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<GameObject>(readOnlyContentPack.bodyPrefabs, "bodyPrefabs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<EquipmentDef>(readOnlyContentPack.equipmentDefs, "equipmentDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<ExpansionDef>(readOnlyContentPack.expansionDefs, "expansionDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<GameObject>(readOnlyContentPack.gameModePrefabs, "gameModePrefabs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<ItemDef>(readOnlyContentPack.itemDefs, "itemDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<ItemTierDef>(readOnlyContentPack.itemTierDefs, "itemTierDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<GameObject>(readOnlyContentPack.masterPrefabs, "masterPrefabs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<SceneDef>(readOnlyContentPack.sceneDefs, "sceneDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<SkillDef>(readOnlyContentPack.skillDefs, "skillDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<SkillFamily>(readOnlyContentPack.skillFamilies, "skillFamilies", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<SurvivorDef>(readOnlyContentPack.survivorDefs, "survivorDefs", ref CS$<>8__locals1);
					ProperSavePlugin.<ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<UnlockableDef>(readOnlyContentPack.unlockableDefs, "unlockableDefs", ref CS$<>8__locals1);
				}
				ProperSavePlugin.ContentHash = Convert.ToBase64String(md.ComputeHash(Encoding.UTF8.GetBytes(CS$<>8__locals1.writer.ToString())));
			}
			finally
			{
				if (CS$<>8__locals1.writer != null)
				{
					CS$<>8__locals1.writer.Dispose();
				}
			}
		}
		
		private void GatherPersistentDesperadoData(Dictionary<string, object> saveData)
		{
  			foreach (var player in PlayerCharacterMasterController.instances)
			{
			    var userName = player.master.GetBody().GetUserName();
			    var buffCount = player.master.GetBody().GetBuffCount(RoR2Content.Buffs.BanditSkull);
			    saveData[userName] = buffCount;
			}
		}
		
		// Token: 0x06000055 RID: 85 RVA: 0x0000430C File Offset: 0x0000250C
		[CompilerGenerated]
		internal static void <ContentManagerOnContentPacksAssigned>g__WriteCollection|45_0<T>(ReadOnlyNamedAssetCollection<T> collection, string collectionName, ref ProperSavePlugin.<>c__DisplayClass45_0 A_2)
		{
			A_2.writer.Write(collectionName);
			int num = 0;
			foreach (T t in collection)
			{
				A_2.writer.Write(num);
				A_2.writer.Write('_');
				A_2.writer.Write(collection.GetAssetName(t) ?? string.Empty);
				A_2.writer.Write(';');
				num++;
			}
		}

		// Token: 0x04000022 RID: 34
		public const string GUID = "com.KingEnderBrine.ProperSave";

		// Token: 0x04000023 RID: 35
		public const string Name = "Proper Save";

		// Token: 0x04000024 RID: 36
		public const string Version = "2.11.2";

		// Token: 0x04000025 RID: 37
		private static readonly char[] invalidSubDirectoryCharacters = new char[]
		{
			'\\',
			'/',
			'.'
		};
	}
}
