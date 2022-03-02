using System;
using Pocketverse;

namespace HeavyMetalMachines.Audio
{
	public class VoiceOver : GameHubScriptableObject
	{
		public void PreloadPlayer()
		{
			this.Gadgets_Hit_G00.Preload();
			this.Gadgets_Miss_G00.Preload();
			this.Gadgets_CD_G00.Preload();
			this.Gadgets_Hit_G01.Preload();
			this.Gadgets_Miss_G01.Preload();
			this.Gadgets_CD_G01.Preload();
			this.Almost_Dying.Preload();
		}

		public void PreloadTeamMember()
		{
			this.Bomb_Drop_Purposeful.Preload();
			this.Bomb_Drop_Yellow.Preload();
			this.Bomb_Pick.Preload();
		}

		public void Preload()
		{
			this.Bomb_Drop_Death.Preload();
			this.Movement_Forced.Preload();
			this.Gadgets_Use_G01.Preload();
			this.Gadgets_Use_G00.Preload();
			this.Damage_Regular.Preload();
			this.Damage_Massive.Preload();
			this.Bomb_HP_Repaired.Preload();
		}

		public VoiceOverLine PickScreen_Confirmation;

		public VoiceOverLine Match_Win;

		public VoiceOverLine Match_Lose;

		public VoiceOverLine Movement_Nitro;

		public VoiceOverLine Kill_Ultimate;

		public VoiceOverLine Kill_Enemy;

		public VoiceOverLine Kill_Revenge;

		public VoiceOverLine Kill_Streak02;

		public VoiceOverLine Kill_Streak03;

		public VoiceOverLine Kill_Streak04;

		public VoiceOverLine Kill_Streak05;

		public VoiceOverLine Damage_Regular;

		public VoiceOverLine Damage_Massive;

		public VoiceOverLine Dying_Client;

		public VoiceOverLine Dying_Others;

		public VoiceOverLine Respawn;

		public VoiceOverLine Disarmed;

		public VoiceOverLine Gadgets_Available_Nitro;

		public VoiceOverLine Gadgets_Available_G00;

		public VoiceOverLine Gadgets_Use_G00;

		public VoiceOverLine Gadgets_Hit_G00;

		public VoiceOverLine Gadgets_Miss_G00;

		public VoiceOverLine Gadgets_CD_G00;

		public VoiceOverLine Gadgets_Available_G01;

		public VoiceOverLine Gadgets_Use_G01;

		public VoiceOverLine Gadgets_Hit_G01;

		public VoiceOverLine Gadgets_Miss_G01;

		public VoiceOverLine Gadgets_CD_G01;

		public VoiceOverLine Gadgets_Available_Ult;

		public VoiceOverLine Gadgets_Use_Ult;

		public VoiceOverLine Gadgets_2nd_Use_Ult;

		public VoiceOverLine Gadgets_Hit_Ult;

		public VoiceOverLine Gadgets_Miss_Ult;

		public VoiceOverLine Gadgets_CD_Ult;

		public VoiceOverLine Bomb_Near_Death;

		public VoiceOverLine Bomb_Pick;

		public VoiceOverLine Bomb_Drop_Purposeful;

		public VoiceOverLine Bomb_Drop_Yellow;

		public VoiceOverLine Bomb_Drop_Death;

		public VoiceOverLine Bomb_Enter_Track_Allied;

		public VoiceOverLine Bomb_First_Curve_Allied;

		public VoiceOverLine Bomb_Enter_Track_Enemy;

		public VoiceOverLine Bomb_First_Curve_Enemy;

		public VoiceOverLine Bomb_Almost_Delivered;

		public VoiceOverLine Bomb_Delivered;

		public VoiceOverLine Bomb_HP_Repaired;

		public VoiceOverLine Bomb_Movement_Forced;

		public VoiceOverLine Almost_Dying;

		public VoiceOverLine Movement_Forced;

		public VoiceOverLine Upgrade_Open_Shop;

		public VoiceOverLine Upgrade_Buy;

		public VoiceOverLine Upgrade_Ult_Buy;

		public VoiceOverLine Receive_Resource;

		public VoiceOverLine QuickChat_GiveMe_Bomb;

		public VoiceOverLine QuickChat_Dropping_Bomb;

		public VoiceOverLine QuickChat_Protect_Bomb;

		public VoiceOverLine QuickChat_Get_Bomb;

		public VoiceOverLine QuickChat_OnMyWay;

		public VoiceOverLine QuickChat_ImOut;

		public VoiceOverLine Talk_GoodGame;

		public VoiceOverLine Talk_Thanks;

		public VoiceOverLine Talk_Sorry;

		public VoiceOverLine Talk_GoodLuck;

		public VoiceOverLine Taunt;

		public VoiceOverLine QuickChat_Attack_Interceptors;

		public VoiceOverLine QuickChat_Attack_Supporters;

		public VoiceOverLine QuickChat_Attack_Transporters;

		public VoiceOverLine QuickChat_Group_Up;

		public VoiceOverLine QuickChat_Need_Repair;

		public VoiceOverLine QuickChat_Need_Repair_Intense;

		public VoiceOverLine QuickChat_Ok;

		public VoiceOverLine QuickChat_Special_Almost_Ready;

		public VoiceOverLine QuickChat_Special_Not_Ready;

		public VoiceOverLine QuickChat_Special_Ready;

		public VoiceOverLine CounselorOnLoading;
	}
}
