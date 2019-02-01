using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class AWSDCamera : GameHubBehaviour
	{
		public void TurnOn()
		{
			if (GameHubBehaviour.Hub != null)
			{
				if (GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance)
				{
					GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.gameObject.SetActive(false);
				}
				GameHubBehaviour.Hub.CursorManager.Push(false, CursorManager.CursorTypes.GameCursor);
			}
			if (CarCamera.Singleton)
			{
				this.myCamera = CarCamera.Singleton.GetComponent<Camera>();
				CarCamera.Singleton.enabled = false;
			}
			this.myCamera.farClipPlane = 1500f;
			this.myCamera.transform.position = Vector3.zero;
			this.Initialized = true;
		}

		public void TurnOff()
		{
			if (GameHubBehaviour.Hub != null)
			{
				if (GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance)
				{
					GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.gameObject.SetActive(true);
				}
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
			if (CarCamera.Singleton)
			{
				this.myCamera = null;
				CarCamera.Singleton.enabled = true;
			}
			this.Initialized = false;
		}

		protected void LateUpdate()
		{
			if (!this.Initialized)
			{
				return;
			}
			if (CarCamera.Singleton)
			{
				CarCamera.Singleton.enabled = false;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f)
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					this.mouseSpeed += axis + ((axis <= 0f) ? (this.mouseSpeedInc * -1f) : this.mouseSpeedInc);
				}
				else
				{
					this.speed += axis + ((axis <= 0f) ? (this.speedInc * -1f) : this.speedInc);
				}
			}
			this.speed = Mathf.Clamp(this.speed, 1.01f, 1000f);
			this.mouseSpeed = Mathf.Clamp(this.mouseSpeed, 1.01f, 1000f);
			float axis2 = Input.GetAxis("Vertical");
			float axis3 = Input.GetAxis("Horizontal");
			float d = (float)((!Input.GetKey(KeyCode.Q)) ? ((!Input.GetKey(KeyCode.E)) ? 0 : 1) : -1);
			this.myCamera.transform.position += (this.myCamera.transform.forward * axis2 + this.myCamera.transform.right * axis3 + this.myCamera.transform.up * d) * this.speed * Time.deltaTime;
			this.mouse += new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f) * this.mouseSpeed * Time.deltaTime;
			this.myCamera.transform.rotation = Quaternion.Euler(this.mouse.y, this.mouse.x, 0f);
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				this.speed = 60f;
				this.mouseSpeed = 60f;
				this.myCamera.transform.position = Vector3.zero;
			}
		}

		private Camera myCamera;

		public float speed = 60f;

		public float mouseSpeed = 60f;

		public float speedInc;

		public float mouseSpeedInc;

		private bool Initialized;

		private Vector3 mouse;
	}
}
