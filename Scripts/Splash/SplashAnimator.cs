using Godot;
using System;
using System.Threading.Tasks;

public static class SplashAnimator
{
	public static async void Play(Control root, Action onComplete)
	{
		var bg = root.GetNode<ColorRect>("ColorRect");
		bg.Modulate = new Color("#242424");

		await Fade(bg, 0, 1, 1.0f); // Fade in
		await SplashProgress.Run(root); // Progress load
		await Fade(bg, 1, 0, 1.0f); // Fade out

		onComplete?.Invoke();
	}

	private static async Task Fade(CanvasItem item, float from, float to, float duration)
	{
		float t = 0;
		item.Modulate = new Color(1, 1, 1, from);

		while (t < duration)
		{
			await item.ToSignal(item.GetTree(), SceneTree.SignalName.ProcessFrame);
			t += (float)item.GetProcessDeltaTime();
			item.Modulate = new Color(1, 1, 1, Mathf.Lerp(from, to, t / duration));
		}

		item.Modulate = new Color(1, 1, 1, to);
	}
}
