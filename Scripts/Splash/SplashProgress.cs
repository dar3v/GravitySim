using Godot;
using System.Threading.Tasks;

public static class SplashProgress
{
	// Total duration of the progress bar (seconds)
	private const float LOAD_DURATION = 4.0f; // ← make this bigger to slow it more

	public static void Init(Control root)
	{
		var bar = root.GetNode<ProgressBar>("ColorRect/ProgressBar");
		bar.MinValue = 0;
		bar.MaxValue = 100;
		bar.Value = 0;
	}

	public static async Task Run(Control root)
	{
		var bar = root.GetNode<ProgressBar>("ColorRect/ProgressBar");

		float elapsed = 0f;

		while (elapsed < LOAD_DURATION)
		{
			await root.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);

			elapsed += (float)root.GetProcessDeltaTime();
			float t = elapsed / LOAD_DURATION;

			bar.Value = Mathf.Lerp(0f, 100f, t);
		}

		bar.Value = 100;
	}
}
