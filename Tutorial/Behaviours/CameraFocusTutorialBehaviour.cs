using System;
using System.Collections;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class CameraFocusTutorialBehaviour : InGameTutorialBehaviourPathSystemHolder
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			if (GameHubBehaviour.Hub.Net.isTest)
			{
				return;
			}
			base.SetPlayerInputsActive(false);
			this._gameCameraEngine.Disable();
			this._initialPosition = this._gameCameraEngine.CameraTransform.position;
			this._initialRotation = this._gameCameraEngine.CameraTransform.rotation;
			this._lookTarget = new GameObject("look_target");
			this._lookTarget.transform.parent = base.transform;
			this._lookTarget.transform.localScale = Vector3.one;
			this._lookTarget.transform.position = base.playerController.transform.position;
			PostProcessingState postProcessingState = this._postProcessing.Request("FocusTutorial", () => !this._gameCameraEngine.IsEnabled(), false);
			if (postProcessingState != null)
			{
				postProcessingState.Enabled = true;
				postProcessingState.CRTMonitor.Enabled = true;
				postProcessingState.CRTMonitor.Parameters.LineStrength = 0.5f;
			}
			this.MoveToNextNode();
		}

		private void MoveToNextNode()
		{
			base.StartCoroutine(this.MoveToNextNodeCoroutine());
		}

		private IEnumerator MoveToNextNodeCoroutine()
		{
			TutorialPathNode tNextNode = this.path.GetNextNode();
			if (tNextNode == null)
			{
				yield return this.OnMoveComplete();
				yield break;
			}
			CameraFocusNodeInfo tCameraInfo = tNextNode.GetComponentInChildren<CameraFocusNodeInfo>();
			this._targeTweener = new CameraFocusTutorialBehaviour.Tweener(this._lookTarget.transform, tCameraInfo.nextNodeTransitionTime).Position(this._lookTarget.transform.position, tCameraInfo.target.transform.position);
			this._cameraTweener = new CameraFocusTutorialBehaviour.Tweener(this._gameCameraEngine.CameraTransform, tCameraInfo.nextNodeTransitionTime).Position(this._gameCameraEngine.CameraTransform.position, tNextNode.transform.position);
			yield return new WaitForSeconds(tCameraInfo.nextNodeTransitionTime);
			yield return base.StartCoroutine(this.StartNodeActions(tNextNode));
			yield return new WaitForSeconds(tCameraInfo.waitTime);
			yield return base.StartCoroutine(this.EndNodeActions(tNextNode));
			this.MoveToNextNode();
			yield break;
		}

		protected override void DrawGizmoForNode(TutorialPathNode tNode, Color pColor)
		{
			base.DrawGizmoForNode(tNode, pColor);
			CameraFocusNodeInfo componentInChildren = tNode.GetComponentInChildren<CameraFocusNodeInfo>();
			if (componentInChildren != null)
			{
				this.DrawViewport(pColor, tNode.transform.position, componentInChildren.target.transform.position);
				Gizmos.color = pColor;
				Gizmos.DrawWireSphere(componentInChildren.target.transform.position, 1f);
			}
		}

		private void DrawViewport(Color color, Vector3 from, Vector3 to)
		{
			Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
			Gizmos.DrawLine(from - Vector3.left - Vector3.back, to - Vector3.left - Vector3.back);
			Gizmos.DrawLine(from - Vector3.right - Vector3.forward, to - Vector3.right - Vector3.forward);
			Gizmos.DrawLine(from - Vector3.forward - Vector3.left, to - Vector3.forward - Vector3.left);
			Gizmos.DrawLine(from - Vector3.back - Vector3.right, to - Vector3.back - Vector3.right);
		}

		private IEnumerator StartNodeActions(TutorialPathNode pNextNode)
		{
			DialogBehaviour tDialog = pNextNode.GetComponent<DialogBehaviour>();
			if (tDialog == null)
			{
				yield break;
			}
			tDialog.ShowDialogForBehaviour();
			if (tDialog.requiredToCompletedStep)
			{
				while (!tDialog.behaviourCompleted)
				{
					yield return null;
				}
			}
			yield break;
		}

		private IEnumerator EndNodeActions(TutorialPathNode pNextNode)
		{
			DialogBehaviour component = pNextNode.GetComponent<DialogBehaviour>();
			if (component == null)
			{
				yield break;
			}
			component.OnStepCompleted();
			yield break;
		}

		private IEnumerator OnMoveComplete()
		{
			this._cameraTweener = new CameraFocusTutorialBehaviour.Tweener(this._gameCameraEngine.CameraTransform, this.backToPlayerTime).Position(this._gameCameraEngine.CameraTransform.position, this._initialPosition).Rotation(this._gameCameraEngine.CameraTransform.rotation, this._initialRotation);
			yield return new WaitForSeconds(this.backToPlayerTime);
			this.CompleteBehaviourAndSync();
			Object.Destroy(this._lookTarget);
			yield break;
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			this._targeTweener = null;
			this._cameraTweener = null;
			this._gameCameraEngine.Enable();
			base.SetPlayerInputsActive(true);
		}

		protected override void UpdateOnClient()
		{
			base.UpdateOnClient();
			if (!base.behaviourCompleted && this._cameraTweener != null)
			{
				this._cameraTweener.Update();
				if (this._targeTweener != null && this._targeTweener.Update())
				{
					this._gameCameraEngine.CameraTransform.LookAt(this._lookTarget.transform);
				}
			}
		}

		public float backToPlayerTime = 50f;

		[InjectOnClient]
		private IGamePostProcessing _postProcessing;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		[HideInInspector]
		private GameObject _lookTarget;

		private Vector3 _initialPosition;

		private Quaternion _initialRotation;

		private CameraFocusTutorialBehaviour.Tweener _cameraTweener;

		private CameraFocusTutorialBehaviour.Tweener _targeTweener;

		private sealed class Tweener
		{
			public Tweener(Transform transform, float duration)
			{
				if (duration <= 0f)
				{
					throw new ArgumentOutOfRangeException("duration", "duration must be positive");
				}
				this._duration = duration;
				this._startTime = Time.time;
				this._transform = transform;
			}

			public CameraFocusTutorialBehaviour.Tweener Position(Vector3 start, Vector3 end)
			{
				this._position = true;
				this._startPos = start;
				this._endPos = end;
				return this;
			}

			public CameraFocusTutorialBehaviour.Tweener Rotation(Quaternion start, Quaternion end)
			{
				this._rotation = true;
				this._startRot = start;
				this._endRot = end;
				return this;
			}

			public bool Update()
			{
				float num = Mathf.SmoothStep(0f, 1f, (Time.time - this._startTime) / this._duration);
				Vector3 vector = Vector3.Lerp(this._startPos, this._endPos, num);
				Quaternion quaternion = Quaternion.Slerp(this._startRot, this._endRot, num);
				if (this._position && this._rotation)
				{
					this._transform.SetPositionAndRotation(vector, quaternion);
				}
				else if (this._position)
				{
					this._transform.position = vector;
				}
				else if (this._rotation)
				{
					this._transform.rotation = quaternion;
				}
				return num < 1f;
			}

			private readonly Transform _transform;

			private float _duration;

			private float _startTime;

			private bool _position;

			private Vector3 _startPos;

			private Vector3 _endPos;

			private bool _rotation;

			private Quaternion _startRot;

			private Quaternion _endRot;
		}
	}
}
