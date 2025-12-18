using Godot;

public partial class SplashScreen : Control
{
	// Path to the scene that will load after the splash screen
	[Export] public string NextScenePath = "res://Scenes/GravitySim.tscn";

	public override void _Ready()
	{
		// Apply splash screen layout settings
		SplashLayout.Apply(this);

		// Initialize loading/progress visuals
		SplashProgress.Init(this);

		// Play splash animation, then switch to the next scene
		SplashAnimator.Play(
			this,
			onComplete: () =>
			{
				// Load the main scene after animation finishes
				SplashSceneLoader.SwitchTo(NextScenePath);
			}
		);
	}
}
