using Godot;
using System;
using System.Threading.Tasks;

public static class SplashAnimator
{
	// Plays the splash animation sequence
	public static async void Play(Control root, Action onComplete)
	{
		// Get background ColorRect
		var bg = root.GetNode<ColorRect>("ColorRect");

		// Set background color (#042434)
		bg.Modulate = new Color("#042434");

		// Fade in background
		await Fade(bg, 1, 1, 1.0f);

		// Run loading progress
		await SplashProgress.Run(root);

		// Fade out background
		await Fade(bg, 1, 1, 1.0f);

		// Notify when splash is done
		onComplete?.Invoke();
	}

	// Handles fade animation using alpha
	private static async Task Fade(CanvasItem item, float from, float to, float duration)
	{
		float t = 0;

		// Set starting alpha
		item.Modulate = new Color(1, 1, 1, from);

		// Animate fade over time
		while (t < duration)
		{
			await item.ToSignal(item.GetTree(), SceneTree.SignalName.ProcessFrame);
			t += (float)item.GetProcessDeltaTime();

			// Smooth alpha transition
			item.Modulate = new Color(1, 1, 1, Mathf.Lerp(from, to, t / duration));
		}

		// Ensure final alpha value
		item.Modulate = new Color(1, 1, 1, to);
	}
}
