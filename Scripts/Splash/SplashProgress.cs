using Godot;
using System.Threading.Tasks;

public static class SplashProgress
{
	// Total fake loading time (seconds)
	private const float LOAD_DURATION = 4.0f;

	// Initializes the progress bar
	public static void Init(Control root)
	{
		var bar = root.GetNode<ProgressBar>("ColorRect/ProgressBar");
		bar.MinValue = 0;
		bar.MaxValue = 100;
		bar.Value = 0;
	}

	// Runs the loading animation
	public static async Task Run(Control root)
	{
		var bar = root.GetNode<ProgressBar>("ColorRect/ProgressBar");

		float elapsed = 0f;

		// Update progress over time
		while (elapsed < LOAD_DURATION)
		{
			await root.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);

			elapsed += (float)root.GetProcessDeltaTime();
			float t = elapsed / LOAD_DURATION;

			// Smooth progress increase
			bar.Value = Mathf.Lerp(0f, 100f, t);
		}

		// Ensure progress is complete
		bar.Value = 100;
	}
}
