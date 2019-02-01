using System;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Pipeline;
using HeavyMetalMachines.Render;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class CarComponentHub : ComponentHub
	{
		[NonSerialized]
		public PlayerData Player;

		[NonSerialized]
		public Transform RenderTransform;

		[NonSerialized]
		public CombatObject combatObject;

		[NonSerialized]
		public CarGenerator carGenerator;

		[NonSerialized]
		public CarAudioController carAudioController;

		[NonSerialized]
		public VoiceOverController VoiceOverController;

		[NonSerialized]
		public CarInput carInput;

		[NonSerialized]
		public CarMovement carMovement;

		[NonSerialized]
		public SpawnController spawnController;

		[NonSerialized]
		public BotAIGoalManager botAIGoalManager;

		[NonSerialized]
		public BotAIGadgetShop botAIGadgetShop;

		[NonSerialized]
		public BotAIPathFind botAIPathFind;

		[NonSerialized]
		public CombatFeedback combatFeedback;

		[NonSerialized]
		public CDummy dummy;

		[NonSerialized]
		public CarMovementFeedback carMovementFeedback;

		[NonSerialized]
		public CarIndicator carIndicator;

		[NonSerialized]
		public CombatData combatData;

		[NonSerialized]
		public PlayerController playerController;

		[NonSerialized]
		public SurfaceEffect surfaceEffect;

		[NonSerialized]
		public ArtReference ArtReference;
	}
}
