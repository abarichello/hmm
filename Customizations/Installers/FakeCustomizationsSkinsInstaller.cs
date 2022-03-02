using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Skins;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Customizations.Skins;
using HeavyMetalMachines.Matches;
using Hoplon;
using Zenject;

namespace HeavyMetalMachines.Customizations.Installers
{
	public class FakeCustomizationsSkinsInstaller : MonoInstaller<FakeCustomizationsSkinsInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGetLocalPlayerAvailableSkins>().To<FakeGetLocalPlayerAvailableSkins>().AsTransient();
			base.Container.Bind<IEquippedSkinsProvider>().To<FakeCustomizationsSkinsInstaller.FakeEquippedSkinsProvider>().AsTransient();
		}

		public class FakeEquippedSkinsProvider : IEquippedSkinsProvider
		{
			public FakeEquippedSkinsProvider(IGetLocalPlayerAvailableSkins getLocalPlayerAvailableSkins, IRandom random, IGetCurrentMatch getCurrentMatch, IGetCharacterData getCharacterData)
			{
				this._getLocalPlayerAvailableSkins = getLocalPlayerAvailableSkins;
				this._random = random;
				this._getCurrentMatch = getCurrentMatch;
				this._getCharacterData = getCharacterData;
			}

			public List<MatchClientEquippedSkins> Get()
			{
				List<MatchClientEquippedSkins> list = new List<MatchClientEquippedSkins>();
				foreach (MatchClient client in this._getCurrentMatch.GetIfExisting().Value.Clients)
				{
					List<CharacterEquippedSkin> list2 = new List<CharacterEquippedSkin>();
					foreach (CharacterData characterData in this._getCharacterData.GetAll())
					{
						List<SkinData> list3 = this._getLocalPlayerAvailableSkins.Get(characterData.Id);
						int index = this._random.Range(0, list3.Count);
						list2.Add(new CharacterEquippedSkin
						{
							CharacterId = characterData.Id,
							SkinId = list3[index].Id
						});
					}
					list.Add(new MatchClientEquippedSkins
					{
						Client = client,
						EquippedSkins = list2.ToArray()
					});
				}
				return list;
			}

			private readonly IGetLocalPlayerAvailableSkins _getLocalPlayerAvailableSkins;

			private readonly IRandom _random;

			private readonly IGetCurrentMatch _getCurrentMatch;

			private readonly IGetCharacterData _getCharacterData;
		}
	}
}
