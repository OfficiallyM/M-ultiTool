using MultiTool.Services;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class Noclip : MonoBehaviour
	{
		private float _climbSpeed = 10f;
		private float _normalMoveSpeed = 10f;

		private ladderscript _ladder = new ladderscript();

		private void Start()
		{
			_ladder.T = mainscript.M.player.transform;
		}

		private void Update()
		{
			// Fake player being on a ladder, manipulates game to disable the player gravity.
			fpscontroller player = mainscript.M.player;
			if (player == null) return;
			player.ladderV = 1;
			player.TLadder = _ladder;

			float speed = _normalMoveSpeed;
			float climbSpeed = this._climbSpeed;
			if (Input.GetKey(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
			{
				speed *= MultiTool.Configuration.Config.NoclipFastMoveFactor;
				climbSpeed *= MultiTool.Configuration.Config.NoclipFastMoveFactor;
			}

			if (Input.GetButton("forward"))
				mainscript.M.player.transform.root.position += Vector3.ProjectOnPlane(mainscript.M.player.Tb.forward, Vector3.up) * speed * Time.deltaTime;
			if (Input.GetKey(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipUp).AssignedKey))
				mainscript.M.player.transform.root.position += Vector3.up * climbSpeed * Time.deltaTime;
			if (Input.GetKey(MultiTool.Binds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
				mainscript.M.player.transform.root.position += -Vector3.up * climbSpeed * Time.deltaTime;
			if (Input.GetButton("backward"))
				mainscript.M.player.transform.root.position += Vector3.ProjectOnPlane(-mainscript.M.player.Tb.forward, Vector3.up) * speed * Time.deltaTime;
			if (Input.GetButton("right"))
				mainscript.M.player.transform.root.position += Vector3.ProjectOnPlane(mainscript.M.player.Tb.right, Vector3.up) * speed * Time.deltaTime;
			if (Input.GetButton("left"))
				mainscript.M.player.transform.root.position += Vector3.ProjectOnPlane(-mainscript.M.player.Tb.right, Vector3.up) * speed * Time.deltaTime;
		}
	}
}
