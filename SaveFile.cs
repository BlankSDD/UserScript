using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using ProperSave.Data;
using ProperSave.SaveData;
using RoR2;

namespace ProperSave
{
	// Token: 0x0200000B RID: 11
	public class SaveFile
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000056 RID: 86 RVA: 0x000043A8 File Offset: 0x000025A8
		// (set) Token: 0x06000057 RID: 87 RVA: 0x000043B0 File Offset: 0x000025B0
		[DataMember(Name = "r")]
		public RunData RunData { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000058 RID: 88 RVA: 0x000043B9 File Offset: 0x000025B9
		// (set) Token: 0x06000059 RID: 89 RVA: 0x000043C1 File Offset: 0x000025C1
		[DataMember(Name = "t")]
		public TeamData TeamData { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600005A RID: 90 RVA: 0x000043CA File Offset: 0x000025CA
		// (set) Token: 0x0600005B RID: 91 RVA: 0x000043D2 File Offset: 0x000025D2
		[DataMember(Name = "ra")]
		public RunArtifactsData RunArtifactsData { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600005C RID: 92 RVA: 0x000043DB File Offset: 0x000025DB
		// (set) Token: 0x0600005D RID: 93 RVA: 0x000043E3 File Offset: 0x000025E3
		[DataMember(Name = "a")]
		public ArtifactsData ArtifactsData { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600005E RID: 94 RVA: 0x000043EC File Offset: 0x000025EC
		// (set) Token: 0x0600005F RID: 95 RVA: 0x000043F4 File Offset: 0x000025F4
		[DataMember(Name = "p")]
		public List<PlayerData> PlayersData { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000060 RID: 96 RVA: 0x000043FD File Offset: 0x000025FD
		// (set) Token: 0x06000061 RID: 97 RVA: 0x00004405 File Offset: 0x00002605
		[DataMember(Name = "md")]
		public Dictionary<string, ModdedData> ModdedData { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000062 RID: 98 RVA: 0x0000440E File Offset: 0x0000260E
		// (set) Token: 0x06000063 RID: 99 RVA: 0x00004416 File Offset: 0x00002616
		[DataMember(Name = "ch")]
		public string ContentHash { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000064 RID: 100 RVA: 0x0000441F File Offset: 0x0000261F
		// (set) Token: 0x06000065 RID: 101 RVA: 0x00004427 File Offset: 0x00002627
		[IgnoreDataMember]
		public SaveFileMetadata SaveFileMeta { get; set; }

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000066 RID: 102 RVA: 0x00004430 File Offset: 0x00002630
		// (remove) Token: 0x06000067 RID: 103 RVA: 0x00004464 File Offset: 0x00002664
		public static event Action<Dictionary<string, object>> OnGatherSaveData;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000068 RID: 104 RVA: 0x00004497 File Offset: 0x00002697
		// (remove) Token: 0x06000069 RID: 105 RVA: 0x0000449F File Offset: 0x0000269F
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use OnGatherSaveData without a typo", true)]
		public static event Action<Dictionary<string, object>> OnGatgherSaveData
		{
			add
			{
				SaveFile.OnGatherSaveData += value;
			}
			remove
			{
				SaveFile.OnGatherSaveData -= value;
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x000044A8 File Offset: 0x000026A8
		internal SaveFile()
		{
			this.RunData = new RunData();
			this.TeamData = new TeamData();
			this.RunArtifactsData = new RunArtifactsData();
			this.ArtifactsData = new ArtifactsData();
			this.PlayersData = new List<PlayerData>();
			foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
			{
				LostNetworkUser lostNetworkUser = null;
				if (playerCharacterMasterController.networkUser || LostNetworkUser.TryGetUser(playerCharacterMasterController.master, out lostNetworkUser))
				{
					this.PlayersData.Add(new PlayerData(playerCharacterMasterController, lostNetworkUser));
				}
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Action<Dictionary<string, object>> onGatherSaveData = SaveFile.OnGatherSaveData;
			if (onGatherSaveData != null)
			{
				onGatherSaveData(dictionary);
			}
			this.ModdedData = dictionary.ToDictionary((KeyValuePair<string, object> el) => el.Key, (KeyValuePair<string, object> el) => new ModdedData
			{
				ObjectType = el.Value.GetType().AssemblyQualifiedName,
				Value = el.Value
			});
			this.ContentHash = ProperSavePlugin.ContentHash;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000045C8 File Offset: 0x000027C8
		internal void LoadRun()
		{
			this.RunData.LoadData();
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000045D5 File Offset: 0x000027D5
		internal void LoadArtifacts()
		{
			this.RunArtifactsData.LoadData();
			this.ArtifactsData.LoadData();
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000045ED File Offset: 0x000027ED
		internal void LoadTeam()
		{
			this.TeamData.LoadData();
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000045FC File Offset: 0x000027FC
		internal void LoadPlayers()
		{
			List<PlayerData> list = this.PlayersData.ToList<PlayerData>();
			using (IEnumerator<NetworkUser> enumerator = NetworkUser.readOnlyInstancesList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NetworkUser user = enumerator.Current;
					PlayerData playerData = list.FirstOrDefault((PlayerData el) => el.userId.Load().Equals(user.id));
					if (playerData != null)
					{
						list.Remove(playerData);
						playerData.LoadPlayer(user);
					        if (this.ModdedData.ContainsKey(user.GetNetworkUserName()))
					        {
					        	int buffCount = (int)this.ModdedData[user.GetNetworkUserName()].Value;
					                var body = user.master.GetBody();
					                for (int i = 0; i < buffCount; i++)
					                {
					                    body.AddBuff(RoR2Content.Buffs.BanditSkull);
					                }
						}
					}
				}
      			}
    		}
		// Token: 0x0600006F RID: 111 RVA: 0x00004684 File Offset: 0x00002884
		public T GetModdedData<T>(string key)
		{
			return (T)((object)this.ModdedData[key].Value);
		}
	}
}
