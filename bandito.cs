using System;
using BepInEx;
using BepInEx.Configuration;
using EntityStates.Bandit2.Weapon;
using On.EntityStates.Bandit2.Weapon;
using On.RoR2;
using RoR2;

namespace Bandito
{
	// Token: 0x02000002 RID: 2
	[BepInDependency("com.bepis.r2api", 1)]
	[BepInPlugin("com.OldFaithless.PersistentDesperado", "Persistent Desperado", "1.1.1")]
	public class BanditInfiniteScaling : BaseUnityPlugin
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002057 File Offset: 0x00000257
		public static ConfigEntry<double> StackValue { get; set; }

		// Token: 0x06000003 RID: 3 RVA: 0x00002060 File Offset: 0x00000260
		public void Awake()
		{
			BanditInfiniteScaling.StackValue = base.Config.Bind<double>("Bandit", "StackBonus", 0.02, "The percentage damage increase per stack of Desperado, expressed as a decimal (0.02f is equivalent to 2% per stack).");
			FireSidearmSkullRevolver.ModifyBullet += delegate(FireSidearmSkullRevolver.orig_ModifyBullet orig, FireSidearmSkullRevolver self, BulletAttack bulletAttack)
			{
				float damage = bulletAttack.damage;
				orig.Invoke(self, bulletAttack);
				float num = (float)((double)(bulletAttack.damage - damage) * (BanditInfiniteScaling.StackValue.Value / 0.1));
				bulletAttack.damage = damage + num;
			};
			PreGameController.StartRun += delegate(PreGameController.orig_StartRun orig, PreGameController self)
			{
				this.BanditName1 = "";
				this.BanditName2 = "";
				this.BanditName3 = "";
				this.BanditName4 = "";
				this.BanditName5 = "";
				this.BanditName6 = "";
				this.BanditName7 = "";
				this.BanditName8 = "";
				this.BanditCount1 = 0;
				this.BanditCount2 = 0;
				this.BanditCount3 = 0;
				this.BanditCount4 = 0;
				this.BanditCount5 = 0;
				this.BanditCount6 = 0;
				this.BanditCount7 = 0;
				this.BanditCount8 = 0;
				orig.Invoke(self);
			};
			CharacterBody.RecalculateStats += delegate(CharacterBody.orig_RecalculateStats orig, CharacterBody self)
			{
				bool flag = self.isPlayerControlled && self.teamComponent.teamIndex == 1;
				bool flag2 = flag;
				bool flag3 = flag2;
				if (flag3)
				{
					string userName = self.GetUserName();
					int num = this.FindName(userName);
					int count = this.GetCount(num);
					bool flag4 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull) < count;
					bool flag5 = flag4;
					if (flag5)
					{
						int buffCount = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						for (int i = 0; i < count - buffCount; i++)
						{
							self.AddBuff(RoR2Content.Buffs.BanditSkull);
						}
					}
					bool flag6 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull) > count;
					bool flag7 = flag6;
					if (flag7)
					{
						bool flag8 = num == 1;
						if (flag8)
						{
							this.BanditCount1 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag9 = num == 2;
						if (flag9)
						{
							this.BanditCount2 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag10 = num == 3;
						if (flag10)
						{
							this.BanditCount3 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag11 = num == 4;
						if (flag11)
						{
							this.BanditCount4 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag12 = num == 5;
						if (flag12)
						{
							this.BanditCount5 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag13 = num == 6;
						if (flag13)
						{
							this.BanditCount6 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag14 = num == 7;
						if (flag14)
						{
							this.BanditCount7 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
						bool flag15 = num == 8;
						if (flag15)
						{
							this.BanditCount8 = self.GetBuffCount(RoR2Content.Buffs.BanditSkull);
						}
					}
				}
				orig.Invoke(self);
			};
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020E0 File Offset: 0x000002E0
		public int FindName(string bName)
		{
			bool flag = bName == this.BanditName1 || this.BanditName1 == "";
			int result;
			if (flag)
			{
				this.BanditName1 = bName;
				result = 1;
			}
			else
			{
				bool flag2 = bName == this.BanditName2 || this.BanditName2 == "";
				if (flag2)
				{
					this.BanditName2 = bName;
					result = 2;
				}
				else
				{
					bool flag3 = bName == this.BanditName3 || this.BanditName3 == "";
					if (flag3)
					{
						this.BanditName3 = bName;
						result = 3;
					}
					else
					{
						bool flag4 = bName == this.BanditName4 || this.BanditName4 == "";
						if (flag4)
						{
							this.BanditName4 = bName;
							result = 4;
						}
						else
						{
							bool flag5 = bName == this.BanditName5 || this.BanditName5 == "";
							if (flag5)
							{
								this.BanditName5 = bName;
								result = 5;
							}
							else
							{
								bool flag6 = bName == this.BanditName6 || this.BanditName6 == "";
								if (flag6)
								{
									this.BanditName6 = bName;
									result = 6;
								}
								else
								{
									bool flag7 = bName == this.BanditName7 || this.BanditName7 == "";
									if (flag7)
									{
										this.BanditName7 = bName;
										result = 7;
									}
									else
									{
										bool flag8 = bName == this.BanditName8 || this.BanditName8 == "";
										if (flag8)
										{
											this.BanditName8 = bName;
											result = 8;
										}
										else
										{
											result = -1;
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002294 File Offset: 0x00000494
		public int GetCount(int num)
		{
			bool flag = num == 1;
			int result;
			if (flag)
			{
				result = this.BanditCount1;
			}
			else
			{
				bool flag2 = num == 2;
				if (flag2)
				{
					result = this.BanditCount2;
				}
				else
				{
					bool flag3 = num == 3;
					if (flag3)
					{
						result = this.BanditCount3;
					}
					else
					{
						bool flag4 = num == 4;
						if (flag4)
						{
							result = this.BanditCount4;
						}
						else
						{
							bool flag5 = num == 5;
							if (flag5)
							{
								result = this.BanditCount5;
							}
							else
							{
								bool flag6 = num == 6;
								if (flag6)
								{
									result = this.BanditCount6;
								}
								else
								{
									bool flag7 = num == 7;
									if (flag7)
									{
										result = this.BanditCount7;
									}
									else
									{
										bool flag8 = num == 8;
										if (flag8)
										{
											result = this.BanditCount8;
										}
										else
										{
											result = -1;
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x04000002 RID: 2
		public string BanditName1;

		// Token: 0x04000003 RID: 3
		public string BanditName2;

		// Token: 0x04000004 RID: 4
		public string BanditName3;

		// Token: 0x04000005 RID: 5
		public string BanditName4;

		// Token: 0x04000006 RID: 6
		public string BanditName5;

		// Token: 0x04000007 RID: 7
		public string BanditName6;

		// Token: 0x04000008 RID: 8
		public string BanditName7;

		// Token: 0x04000009 RID: 9
		public string BanditName8;

		// Token: 0x0400000A RID: 10
		public int BanditCount1;

		// Token: 0x0400000B RID: 11
		public int BanditCount2;

		// Token: 0x0400000C RID: 12
		public int BanditCount3;

		// Token: 0x0400000D RID: 13
		public int BanditCount4;

		// Token: 0x0400000E RID: 14
		public int BanditCount5;

		// Token: 0x0400000F RID: 15
		public int BanditCount6;

		// Token: 0x04000010 RID: 16
		public int BanditCount7;

		// Token: 0x04000011 RID: 17
		public int BanditCount8;
	}
}
