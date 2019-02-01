using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class JoystickSchemeController : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (this.ignoreControllerInput)
			{
				this.Configure(!this.ignoreControllerInput);
			}
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
			if (!this.CameraGetUseController())
			{
				return;
			}
			UICamera.onSchemeChange = (UICamera.OnSchemeChange)Delegate.Remove(UICamera.onSchemeChange, new UICamera.OnSchemeChange(this.OnSchemeChange));
		}

		private bool CameraGetUseController()
		{
			return !UICamera.disableController;
		}

		private void CameraSetUseController(bool use)
		{
			use &= !this.ignoreControllerInput;
			if (UICamera.first != null)
			{
				UICamera.first.useController = use;
			}
			UICamera.disableController = !use;
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			if (pChangedstate is Game)
			{
				this.CameraSetUseController(true);
				this.Configure(true);
			}
			else
			{
				this.CameraSetUseController(false);
				this.Configure(false);
			}
		}

		private void Configure(bool on)
		{
			if (on)
			{
				UICamera.onSchemeChange = (UICamera.OnSchemeChange)Delegate.Combine(UICamera.onSchemeChange, new UICamera.OnSchemeChange(this.OnSchemeChange));
				this.Activators = new List<JoystickSchemeButtonActivator>(16);
				this.PreRegisteredActivators = new List<JoystickSchemeButtonActivator>(16);
				this.PreUnRegisteredActivators = new List<JoystickSchemeButtonActivator>(16);
			}
			else
			{
				UICamera.onSchemeChange = (UICamera.OnSchemeChange)Delegate.Remove(UICamera.onSchemeChange, new UICamera.OnSchemeChange(this.OnSchemeChange));
			}
		}

		private void OnSchemeChange()
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (UICamera.currentScheme == UICamera.ControlScheme.Controller)
			{
				this.ActivateActivators(true);
			}
			else
			{
				this.ActivateActivators(false);
			}
		}

		private void Update()
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			this.ProcessUnRegister();
			this.ProcessRegister();
			this.VerifyJoystickButtons();
		}

		private void ActivateActivators(bool action)
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.Game)
			{
			}
			if (this.Activators != null && this.Activators.Count > 0)
			{
				for (int i = 0; i < this.Activators.Count; i++)
				{
					JoystickSchemeButtonActivator joystickSchemeButtonActivator = this.Activators[i];
					if (joystickSchemeButtonActivator.Activated != action)
					{
						joystickSchemeButtonActivator.Activate(action);
					}
				}
			}
		}

		public void PreRegister(JoystickSchemeButtonActivator activator)
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (this.PreRegisteredActivators == null)
			{
				this.PreRegisteredActivators = new List<JoystickSchemeButtonActivator>(16);
			}
			this.PreRegisteredActivators.Add(activator);
		}

		private void ProcessRegister()
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (this.Activators == null)
			{
				this.Activators = new List<JoystickSchemeButtonActivator>(16);
			}
			if (this.PreRegisteredActivators == null)
			{
				return;
			}
			for (int i = 0; i < this.PreRegisteredActivators.Count; i++)
			{
				JoystickSchemeButtonActivator item = this.PreRegisteredActivators[i];
				this.Activators.Add(item);
			}
			this.PreRegisteredActivators.Clear();
		}

		public void PreUnRegister(JoystickSchemeButtonActivator activator)
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (this.PreUnRegisteredActivators == null)
			{
				this.PreUnRegisteredActivators = new List<JoystickSchemeButtonActivator>(16);
			}
			this.PreUnRegisteredActivators.Add(activator);
		}

		private void ProcessUnRegister()
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (this.PreUnRegisteredActivators == null)
			{
				return;
			}
			for (int i = 0; i < this.PreUnRegisteredActivators.Count; i++)
			{
				JoystickSchemeButtonActivator item = this.PreUnRegisteredActivators[i];
				this.Activators.Remove(item);
			}
			this.PreUnRegisteredActivators.Clear();
		}

		private void VerifyJoystickButtons()
		{
			if (!this.CameraGetUseController())
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton0))
			{
				this.JoystickButtonA_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonA);
			}
			else
			{
				this.JoystickButtonA_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton1))
			{
				this.JoystickButtonB_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonB);
			}
			else
			{
				this.JoystickButtonB_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton2))
			{
				this.JoystickButtonX_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonX);
			}
			else
			{
				this.JoystickButtonX_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton3))
			{
				this.JoystickButtonY_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonY);
			}
			else
			{
				this.JoystickButtonY_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton4))
			{
				this.JoystickButtonLB_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonLB);
			}
			else
			{
				this.JoystickButtonLB_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton5))
			{
				this.JoystickButtonRB_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonRB);
			}
			else
			{
				this.JoystickButtonRB_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton6))
			{
				this.JoystickButtonBack_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonBack);
			}
			else
			{
				this.JoystickButtonBack_pressed = false;
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton7))
			{
				this.JoystickButtonStart_pressed = true;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonStart);
			}
			else
			{
				this.JoystickButtonStart_pressed = false;
			}
			if (Input.GetAxis("Joy1 Axis 3") > 0f && !this.JoystickButtonLT_pressed && !this.JoystickButtonRT_pressed)
			{
				this.JoystickButtonLT_pressed = true;
				this.JoystickButtonRT_pressed = false;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonLT);
			}
			else if (Input.GetAxis("Joy1 Axis 3") < 0f && !this.JoystickButtonLT_pressed && !this.JoystickButtonRT_pressed)
			{
				this.JoystickButtonRT_pressed = true;
				this.JoystickButtonLT_pressed = false;
				this.ActivateButtonActivator(JoystickSchemeController.JoystickButtons.JoystickButtonRT);
			}
			if (Input.GetAxis("Joy1 Axis 3") == 0f)
			{
				this.JoystickButtonRT_pressed = false;
				this.JoystickButtonLT_pressed = false;
			}
		}

		private void ActivateButtonActivator(JoystickSchemeController.JoystickButtons joykind)
		{
			List<JoystickSchemeButtonActivator> activators = this.Activators;
			for (int i = 0; i < activators.Count; i++)
			{
				JoystickSchemeButtonActivator joystickSchemeButtonActivator = activators[i];
				if (joystickSchemeButtonActivator.JoystickButtonKind == joykind)
				{
					if (joystickSchemeButtonActivator.gameObject.activeSelf)
					{
						if (joystickSchemeButtonActivator.enabled)
						{
							if (joystickSchemeButtonActivator.targetButton.GetComponent<Collider>().enabled)
							{
								Vector2 vector = UICamera.mainCamera.WorldToViewportPoint(joystickSchemeButtonActivator.targetButton.transform.position);
								if (joystickSchemeButtonActivator.TargetButtonWidget.isVisible && vector.x < 1f && vector.x > 0f && vector.y < 1f && vector.y > 0f)
								{
									Vector3 origin = joystickSchemeButtonActivator.targetButton.transform.position - Vector3.forward * 10f;
									this.ray = new Ray(origin, Vector3.forward);
									this.rayHits = Physics.RaycastAll(this.ray, (float)LayerMask.NameToLayer("UserInterface"));
									if (this.rayHits != null)
									{
										bool flag = false;
										for (int j = 0; j < this.rayHits.Length; j++)
										{
											RaycastHit raycastHit = this.rayHits[j];
											if (!raycastHit.transform.gameObject.name.Contains("Tutorial"))
											{
												if (!(raycastHit.transform.gameObject == joystickSchemeButtonActivator.TargetButtonWidget.gameObject))
												{
													UIWidget component = raycastHit.transform.GetComponent<UIWidget>();
													if (component != null && component.raycastDepth > joystickSchemeButtonActivator.TargetButtonWidget.raycastDepth)
													{
														flag = true;
													}
													UIWidget componentInChildren = raycastHit.transform.GetComponentInChildren<UIWidget>();
													if (componentInChildren != null && componentInChildren.raycastDepth > joystickSchemeButtonActivator.TargetButtonWidget.raycastDepth)
													{
														flag = true;
													}
												}
											}
										}
										if (!flag)
										{
											joystickSchemeButtonActivator.targetButton.SendMessage("OnClick");
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private bool IsVisible(GameObject go)
		{
			if (!go)
			{
				return false;
			}
			if (!go.activeSelf)
			{
				return false;
			}
			Vector2 vector = UICamera.mainCamera.WorldToViewportPoint(go.transform.position);
			if (vector.x >= 1f || vector.x <= 0f || vector.y >= 1f || vector.y <= 0f)
			{
				Debug.Log("not on sreen: ", go);
				return false;
			}
			Vector3 origin = go.transform.position - Vector3.forward * 10f;
			this.ray = new Ray(origin, Vector3.forward);
			this.rayHits = Physics.RaycastAll(this.ray, (float)LayerMask.NameToLayer("UserInterface"));
			if (this.rayHits != null)
			{
				for (int i = 0; i < this.rayHits.Length; i++)
				{
					RaycastHit raycastHit = this.rayHits[i];
					if (!(raycastHit.transform.gameObject == go))
					{
						UIWidget component = go.GetComponent<UIButton>().tweenTarget.GetComponent<UIWidget>();
						UIButton component2 = raycastHit.transform.GetComponent<UIButton>();
						if (component2 != null && component2.tweenTarget.GetComponent<UIWidget>().raycastDepth > component.raycastDepth)
						{
							return false;
						}
						UIWidget component3 = raycastHit.transform.GetComponent<UIWidget>();
						if (component3 != null && component3.raycastDepth > component.raycastDepth)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		public bool GetPressed(JoystickSchemeController.JoystickButtons kind)
		{
			switch (kind)
			{
			case JoystickSchemeController.JoystickButtons.JoystickButtonA:
				return this.JoystickButtonA_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonB:
				return this.JoystickButtonB_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonX:
				return this.JoystickButtonX_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonY:
				return this.JoystickButtonY_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonBack:
				return this.JoystickButtonBack_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonStart:
				return this.JoystickButtonStart_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonLB:
				return this.JoystickButtonLT_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonRB:
				return this.JoystickButtonRB_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonLT:
				return this.JoystickButtonLT_pressed;
			case JoystickSchemeController.JoystickButtons.JoystickButtonRT:
				return this.JoystickButtonRT_pressed;
			default:
				return false;
			}
		}

		public UIPanel Mainpanel;

		public bool ignoreControllerInput;

		public List<JoystickSchemeButtonActivator> Activators;

		public List<JoystickSchemeButtonActivator> PreRegisteredActivators;

		public List<JoystickSchemeButtonActivator> PreUnRegisteredActivators;

		public bool NoController;

		public bool JoystickButtonA_pressed;

		public bool JoystickButtonB_pressed;

		public bool JoystickButtonX_pressed;

		public bool JoystickButtonY_pressed;

		public bool JoystickButtonBack_pressed;

		public bool JoystickButtonStart_pressed;

		public bool JoystickButtonLB_pressed;

		public bool JoystickButtonRB_pressed;

		public bool JoystickButtonLT_pressed;

		public bool JoystickButtonRT_pressed;

		private Ray ray;

		private RaycastHit[] rayHits;

		public enum JoystickButtons
		{
			None = -1,
			JoystickButtonA,
			JoystickButtonB,
			JoystickButtonX,
			JoystickButtonY,
			JoystickButtonBack,
			JoystickButtonStart,
			JoystickButtonLB,
			JoystickButtonRB,
			JoystickButtonLT,
			JoystickButtonRT
		}
	}
}
