using System;
using Hoplon.GadgetScript;
using Hoplon.Unity.Loading;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public static class CreateGadgetBody
	{
		public static PositionDirection GetPositionAndDirection(GadgetBodyCreation creation)
		{
			if (creation.HmmGadgetContext.IsClient)
			{
				return CreateGadgetBody.GetPositionAndDirectionForClient(creation);
			}
			return CreateGadgetBody.GetPositionAndDirectionForServer(creation);
		}

		private static PositionDirection GetPositionAndDirectionForClient(GadgetBodyCreation creation)
		{
			creation.HmmEventContext.LoadParameter(creation.FinalPositionParameter);
			creation.HmmEventContext.LoadParameter(creation.FinalDirectionParameter);
			return new PositionDirection
			{
				Position = creation.FinalPositionParameter.GetValue(creation.HmmGadgetContext),
				Direction = creation.FinalDirectionParameter.GetValue(creation.HmmGadgetContext)
			};
		}

		private static PositionDirection GetPositionAndDirectionForServer(GadgetBodyCreation creation)
		{
			PositionDirection result;
			if (creation.DummyKind == CDummy.DummyKind.None)
			{
				if (creation.ParentTransform == null)
				{
					result = CreateGadgetBody.CalculatePositionAndDirectionWithDummy(creation);
				}
				else
				{
					result = CreateGadgetBody.CalculatePositionAndDirectionWithParent(creation);
				}
			}
			else
			{
				Transform dummy = creation.HmmGadgetContext.Owner.Dummy.GetDummy(creation.DummyKind, creation.CustomDummyName, null);
				result = new PositionDirection
				{
					Position = dummy.position,
					Direction = dummy.forward
				};
			}
			result.Position.y = 0f;
			result.Direction.y = 0f;
			creation.FinalPositionParameter.SetValue(creation.HmmGadgetContext, result.Position);
			creation.FinalDirectionParameter.SetValue(creation.HmmGadgetContext, result.Direction);
			creation.HmmEventContext.SaveParameter(creation.FinalPositionParameter);
			creation.HmmEventContext.SaveParameter(creation.FinalDirectionParameter);
			return result;
		}

		private static PositionDirection CalculatePositionAndDirectionWithDummy(GadgetBodyCreation creation)
		{
			Transform dummy = creation.HmmGadgetContext.Owner.Dummy.GetDummy(creation.DummyKind, creation.CustomDummyName, null);
			Vector3 vector = dummy.forward;
			if (creation.DirectionParameter != null)
			{
				vector = creation.DirectionParameter.GetValue<Vector3>(creation.HmmGadgetContext);
				if (creation.UseRelativeDirection)
				{
					vector = creation.OwnerTransform.TransformDirection(vector);
				}
			}
			Vector3 vector2;
			if (creation.PositionParameter != null)
			{
				vector2 = creation.PositionParameter.GetValue<Vector3>(creation.HmmGadgetContext);
				if (creation.UsePositionAsOffset)
				{
					vector2 += dummy.position;
				}
				if (vector == Vector3.zero)
				{
					vector = (vector2 - creation.OwnerTransform.position).normalized;
				}
			}
			else
			{
				vector2 = dummy.position;
			}
			return new PositionDirection
			{
				Position = vector2,
				Direction = vector
			};
		}

		private static PositionDirection CalculatePositionAndDirectionWithParent(GadgetBodyCreation creation)
		{
			Vector3 vector = creation.ParentTransform.forward;
			if (creation.DirectionParameter != null)
			{
				vector = creation.DirectionParameter.GetValue<Vector3>(creation.HmmGadgetContext);
				if (creation.UseRelativeDirection)
				{
					vector = creation.ParentTransform.TransformDirection(vector);
				}
			}
			Vector3 vector2;
			if (creation.PositionParameter != null)
			{
				vector2 = creation.PositionParameter.GetValue<Vector3>(creation.HmmGadgetContext);
				if (creation.UsePositionAsOffset)
				{
					vector2 += creation.ParentTransform.position;
				}
			}
			else
			{
				vector2 = creation.ParentTransform.position;
			}
			return new PositionDirection
			{
				Position = vector2,
				Direction = vector
			};
		}

		public static bool TryInstantiatePrefab(PositionDirection positionDirection, Transform prefab, out Component bodyObject)
		{
			Quaternion rotation = (!(positionDirection.Direction == Vector3.zero)) ? Quaternion.LookRotation(positionDirection.Direction, Vector3.up) : Quaternion.identity;
			bodyObject = ResourceLoader.Instance.PrefabCacheInstantiate(prefab, positionDirection.Position, rotation);
			if (bodyObject == null)
			{
				bodyObject = null;
				return false;
			}
			return true;
		}

		public static void AddBodyToEventContext(IHMMGadgetContext hmmGadgetContext, IHMMEventContext hmmEventContext, IGadgetBody body, bool forceSendToClient)
		{
			hmmGadgetContext.Bodies.Add(body.Id, body);
			if (hmmGadgetContext.IsServer)
			{
				hmmEventContext.AddBody(body.Id);
			}
			Identifiable identifiable = body.Identifiable;
			bool flag = identifiable != null;
			if (flag)
			{
				identifiable.Register(body.Id);
			}
			if (forceSendToClient || flag)
			{
				hmmEventContext.SendToClient();
			}
		}

		public static Transform GetPrefab(string prefabName)
		{
			return (Transform)Loading.Content.GetAsset(prefabName).Asset;
		}

		public static Transform GetParentTransform(IHMMGadgetContext hmmGadgetContext, BaseParameter parentBodyParameter)
		{
			if (parentBodyParameter == null)
			{
				return null;
			}
			IGadgetBody gadgetBody = parentBodyParameter.ParameterTomate.GetBoxedValue(hmmGadgetContext) as IGadgetBody;
			if (gadgetBody == null)
			{
				return null;
			}
			return gadgetBody.Transform;
		}

		public static Transform GetOwnerTransform(IHMMGadgetContext hmmGadgetContext)
		{
			return ((Identifiable)hmmGadgetContext.Owner.Identifiable).transform;
		}
	}
}
