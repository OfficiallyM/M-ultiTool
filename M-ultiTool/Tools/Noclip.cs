using MultiTool.Services;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class NoclipTool : Tool
	{
		public override string Name => "Noclip";

		private float _climbSpeed = 10f;
		private float _normalMoveSpeed = 10f;
		private ladderscript _ladder = new ladderscript();

		public override void OnActivate()
		{
			_ladder.T = mainscript.M.player.transform;

			// Disable colliders.
			foreach (Collider collider in mainscript.M.player.C)
			{
				collider.enabled = false;
			}
		}

		public override void OnDeactivate()
		{
			// Re-enable colliders.
			foreach (Collider collider in mainscript.M.player.C)
			{
				collider.enabled = true;
			}
		}

		public override void ControlRender()
		{
			if (GUILayout.Button(Tools.GetAccessibleName(Id), GUILayout.MaxWidth(200)))
				MultiTool.Tools.Toggle(Id);
		}

		public override void Update()
		{
			// Fake player being on a ladder, manipulates game to disable the player gravity.
			fpscontroller player = mainscript.M.player;
			if (player == null) return;
			player.ladderV = 1;
			player.TLadder = _ladder;

			float speed = _normalMoveSpeed;
			float climbSpeed = this._climbSpeed;
			if (Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
			{
				speed *= Services.Configuration.Config.NoclipFastMoveFactor;
				climbSpeed *= Services.Configuration.Config.NoclipFastMoveFactor;
			}

			if (Input.GetButton("forward"))
				mainscript.M.player.transform.root.position += Vector3.ProjectOnPlane(mainscript.M.player.Tb.forward, Vector3.up) * speed * Time.deltaTime;
			if (Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipUp).AssignedKey))
				mainscript.M.player.transform.root.position += Vector3.up * climbSpeed * Time.deltaTime;
			if (Input.GetKey(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
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
