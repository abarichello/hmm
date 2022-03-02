using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public class UnityAIStaticSceneElement : MonoBehaviour, IAIStaticSceneElement
	{
		public Vector2 BoundsMin
		{
			get
			{
				return this._boundsMin;
			}
		}

		public Vector2 BoundsMax
		{
			get
			{
				return this._boundsMax;
			}
		}

		public Collider ElementCollider
		{
			get
			{
				return this._collider;
			}
		}

		public TeamKind AffectedTeam
		{
			get
			{
				return this._affectedTeam;
			}
		}

		public IAIElementKind Kind
		{
			get
			{
				return this._elementKind;
			}
		}

		private void Awake()
		{
			this._collider = base.GetComponent<Collider>();
			Bounds bounds = this._collider.bounds;
			this._boundsMin = bounds.min.ToVector2XZ();
			this._boundsMax = bounds.max.ToVector2XZ();
		}

		private void OnEnable()
		{
			if (this._sceneCollection == null)
			{
				return;
			}
			this._sceneCollection.AddElement(this);
		}

		private void OnDisable()
		{
			if (this._sceneCollection == null)
			{
				return;
			}
			this._sceneCollection.RemoveElement(this);
		}

		[SerializeField]
		private UnityAIElementKind _elementKind;

		[SerializeField]
		private TeamKind _affectedTeam;

		private Vector2 _boundsMin;

		private Vector2 _boundsMax;

		private Collider _collider;

		[InjectOnServer]
		private IAIStaticSceneElementCollection _sceneCollection;
	}
}
