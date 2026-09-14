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

		// Hold-to-repeat state for the speed factor keys.
		private float _increaseHoldDuration = 0f;
		private float _decreaseHoldDuration = 0f;
		private float _increaseNextRepeatTime = 0f;
		private float _decreaseNextRepeatTime = 0f;

		// Delay before hold-repeat kicks in.
		private const float RepeatInitialDelay = 0.25f;
		// Repeat interval when hold-repeat starts.
		private const float RepeatMaxInterval = 0.15f;
		// Fastest repeat interval once fully accelerated.
		private const float RepeatMinInterval = 0.02f;
		// Time taken to go from max to min interval.
		private const float RepeatAccelerationTime = 2f;

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

			HandleFactorStep(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey, 1, ref _increaseHoldDuration, ref _increaseNextRepeatTime);
			HandleFactorStep(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey, -1, ref _decreaseHoldDuration, ref _decreaseNextRepeatTime);
		}

		// Applies a single step on key-down, then repeats on hold, accelerating from RepeatMaxInterval down to RepeatMinInterval.
		private void HandleFactorStep(KeyCode key, int direction, ref float holdDuration, ref float nextRepeatTime)
		{
			if (Input.GetKeyDown(key))
			{
				AdjustFactor(direction);
				holdDuration = 0f;
				nextRepeatTime = Time.time + RepeatInitialDelay;
				return;
			}

			if (!Input.GetKey(key))
			{
				holdDuration = 0f;
				return;
			}

			holdDuration += Time.deltaTime;
			if (Time.time < nextRepeatTime) return;

			AdjustFactor(direction);

			float t = Mathf.Clamp01((holdDuration - RepeatInitialDelay) / RepeatAccelerationTime);
			float interval = Mathf.Lerp(RepeatMaxInterval, RepeatMinInterval, t);
			nextRepeatTime = Time.time + interval;
		}

		private void AdjustFactor(int direction)
		{
			Services.Configuration.Update(c => { c.NoclipFastMoveFactor = Mathf.Clamp(c.NoclipFastMoveFactor + direction, 2, 100); });
		}

		public override void HudRender()
		{
			float fullWidth = Screen.width * 0.2f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.Button("Noclip sprint speed");
			GUILayout.Button(Services.Configuration.Config.NoclipFastMoveFactor.ToString("F2"));

			GUILayout.BeginHorizontal();
			GUILayout.Button("Increase", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Decrease", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
	}
}
