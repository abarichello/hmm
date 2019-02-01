using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Scene;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class SceneryGadgetActivator : GameHubBehaviour, IActivatable, ISerializationCallbackReceiver
	{
		public void Activate(bool enable, int causer)
		{
			this._enabled = enable;
		}

		private void Awake()
		{
			if (this.curveInfo.lockedCurve)
			{
				this.UnlockCurve();
			}
			if (this.visualGadget)
			{
				this.visualGadgetInstance = UnityEngine.Object.Instantiate<SceneryVisualGadget>(this.visualGadget);
				this.visualGadgetInstance.transform.parent = base.transform;
				this.visualGadgetInstance.transform.localPosition = this.visualGadget.transform.localPosition;
				this.visualGadgetInstance.transform.localRotation = this.visualGadget.transform.localRotation;
			}
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this.Gadget.Activate();
			if (this.StartDisabled)
			{
				this.Activate(false, -1);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient() || !this._enabled)
			{
				return;
			}
			if (other.gameObject.layer != 10 && other.gameObject.layer != 11)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat == null || !combat.IsPlayer || (this.Team != TeamKind.Neutral && this.Team != combat.Team))
			{
				return;
			}
			this.Gadget.Combat = combat;
			this.Gadget.Parent = combat.Id;
			this.Gadget.Target = this.Target.position;
			if (base.Id != null && this.Gadget is BasicCannon)
			{
				((BasicCannon)this.Gadget).TargetId = base.Id.ObjId;
			}
			this.Gadget.ForceFire();
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawIcon(base.transform.position, "ramp.png", true);
			if ((this.gizmosRenderers == null || this.lastGizmoTarget != this.visualGadget) && this.visualGadget != null)
			{
				this.gizmosRenderers = this.visualGadget.GetComponentsInChildren<Renderer>(true);
			}
			if (this.gizmosRenderers == null)
			{
				return;
			}
			if (this.previewMaterial == null || this.lastGizmoTarget != this.visualGadget)
			{
				this.previewMaterial = new Material[this.gizmosRenderers.Length];
				for (int i = 0; i < this.previewMaterial.Length; i++)
				{
					this.previewMaterial[i] = new Material(this.gizmosRenderers[i].GetComponent<Renderer>().sharedMaterial);
					this.previewMaterial[i].shader = Shader.Find("Hidden/EditorPreviewGizmos");
					this.previewMaterial[i].color = new Color(1f, 1f, 1f, 0.5f);
				}
			}
			this.lastGizmoTarget = this.visualGadget;
			for (int j = 0; j < this.gizmosRenderers.Length; j++)
			{
				Renderer renderer = this.gizmosRenderers[j];
				if (!(renderer == null))
				{
					Mesh sharedMesh;
					if (renderer is SkinnedMeshRenderer)
					{
						sharedMesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
					}
					else
					{
						MeshFilter component = renderer.GetComponent<MeshFilter>();
						if (!component)
						{
							goto IL_1D0;
						}
						sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
					}
					for (int k = 0; k < this.previewMaterial[j].passCount; k++)
					{
						this.previewMaterial[j].SetPass(k);
						Graphics.DrawMeshNow(sharedMesh, base.transform.localToWorldMatrix * renderer.transform.localToWorldMatrix);
					}
				}
				IL_1D0:;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (this.curveInfo.Source == null || this.curveInfo.Target == null)
			{
				return;
			}
			if (this.visualGadget == null)
			{
				return;
			}
			if (this.visualGadget is SpringJumper)
			{
				SpringJumper springJumper = (SpringJumper)this.visualGadget;
				springJumper.DrawCurve(this.curveInfo);
			}
		}

		public void LockCurve()
		{
			for (int i = 0; i < this.curveInfo.Steps.Length; i++)
			{
				CurveInfo.StepPoint stepPoint = this.curveInfo.Steps[i];
				stepPoint.Center = this.curveInfo.Source.InverseTransformPoint(stepPoint.Center + this.curveInfo.Source.position);
				stepPoint.Tangent = this.curveInfo.Source.InverseTransformDirection(stepPoint.Tangent);
				this.curveInfo.Steps[i] = stepPoint;
			}
			this.curveInfo.TargetTangent = this.curveInfo.Source.InverseTransformDirection(this.curveInfo.TargetTangent);
			this.curveInfo.SourceTangent = this.curveInfo.Source.InverseTransformDirection(this.curveInfo.SourceTangent);
			this.curveInfo.lockedCurve = true;
		}

		public void UnlockCurve()
		{
			for (int i = 0; i < this.curveInfo.Steps.Length; i++)
			{
				CurveInfo.StepPoint stepPoint = this.curveInfo.Steps[i];
				stepPoint.Center = this.curveInfo.Source.TransformPoint(stepPoint.Center) - this.curveInfo.Source.position;
				stepPoint.Tangent = this.curveInfo.Source.TransformDirection(stepPoint.Tangent);
				this.curveInfo.Steps[i] = stepPoint;
			}
			this.curveInfo.TargetTangent = this.curveInfo.Source.TransformDirection(this.curveInfo.TargetTangent);
			this.curveInfo.SourceTangent = this.curveInfo.Source.TransformDirection(this.curveInfo.SourceTangent);
			this.curveInfo.lockedCurve = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SceneryGadgetActivator));

		public GadgetBehaviour Gadget;

		public Transform Target;

		public TeamKind Team;

		private bool _ready;

		[SerializeField]
		public CurveInfo curveInfo;

		public SceneryVisualGadget visualGadget;

		[NonSerialized]
		public SceneryVisualGadget visualGadgetInstance;

		public bool StartDisabled;

		private bool _enabled = true;

		private Renderer[] gizmosRenderers;

		private Material[] previewMaterial;

		private SceneryVisualGadget lastGizmoTarget;
	}
}
