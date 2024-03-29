﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Modules
{
	internal class Noclip : MonoBehaviour
	{
		private Keybinds binds;
		private Config config;

		public float cameraSensitivity = 90f;
		public float climbSpeed = 10f;
		public float normalMoveSpeed = 10f;
		public float fastMoveFactor = 10f;
		private float rotationX;
		private float rotationY;

		public void constructor(Keybinds _binds, Config _config)
		{
			binds = _binds;
			config = _config;
		}

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
				speed *= config.GetNoclipFastMoveFactor(fastMoveFactor);
				climbSpeed *= config.GetNoclipFastMoveFactor(fastMoveFactor);
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
