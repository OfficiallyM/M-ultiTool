using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnerTLD.Modules
{
	internal class Noclip : MonoBehaviour
	{
		private Keybinds binds;
		private Logger logger;

		public void constructor(Keybinds _binds, Logger _logger)
		{
			binds = _binds;
			logger = _logger;
		}

		public float cameraSensitivity = 90f;
		public float climbSpeed = 10f;
		public float normalMoveSpeed = 10f;
		public float fastMoveFactor = 3f;
		private float rotationX;
		private float rotationY;

		private void Update()
		{
			rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, -90f, 90f);

			transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

			float speed = normalMoveSpeed;
			float climbSpeed = this.climbSpeed;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).key))
			{
                speed *= fastMoveFactor;
				climbSpeed *= fastMoveFactor;
			}

            transform.position += transform.forward * speed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * speed * Input.GetAxis("Horizontal") * Time.deltaTime;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipUp).key))
                transform.position += Vector3.up * climbSpeed * Time.deltaTime;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).key))
                transform.position -= Vector3.up * climbSpeed * Time.deltaTime;
        }
	}
}
