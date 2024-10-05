using System;
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

		private float climbSpeed = 10f;
		private float normalMoveSpeed = 10f;
		private float fastMoveFactor = 10f;

        private ladderscript ladder = new ladderscript();

		internal void constructor(Keybinds _binds, Config _config)
		{
			binds = _binds;
			config = _config;

            ladder.T = mainscript.s.player.transform;
		}

		private void Update()
        {
            // Fake player being on a ladder, manipulates game to disable the player gravity.
            fpscontroller player = mainscript.s.player;
            if (player == null) return;
            player.ladderV = 1;
            player.TLadder = ladder;

            float speed = normalMoveSpeed;
            float climbSpeed = this.climbSpeed;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).key))
            {
                speed *= config.GetNoclipFastMoveFactor(fastMoveFactor);
                climbSpeed *= config.GetNoclipFastMoveFactor(fastMoveFactor);
            }

            if (Input.GetButton("forward"))
                mainscript.s.player.transform.root.position += Vector3.ProjectOnPlane(mainscript.s.player.BodyRot.forward, Vector3.up) * speed * Time.deltaTime;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipUp).key))
                mainscript.s.player.transform.root.position += Vector3.up * climbSpeed * Time.deltaTime;
            if (Input.GetKey(binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).key))
                mainscript.s.player.transform.root.position += -Vector3.up * climbSpeed * Time.deltaTime;
            if (Input.GetButton("backward"))
                mainscript.s.player.transform.root.position += Vector3.ProjectOnPlane(-mainscript.s.player.BodyRot.forward, Vector3.up) * speed * Time.deltaTime;
            if (Input.GetButton("right"))
                mainscript.s.player.transform.root.position += Vector3.ProjectOnPlane(mainscript.s.player.BodyRot.right, Vector3.up) * speed * Time.deltaTime;
            if (Input.GetButton("left"))
                mainscript.s.player.transform.root.position += Vector3.ProjectOnPlane(-mainscript.s.player.BodyRot.right, Vector3.up) * speed * Time.deltaTime;
        }
	}
}
