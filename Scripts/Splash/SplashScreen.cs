using Godot;

public partial class SplashScreen : Control
{
	[Export] public string NextScenePath = "res://Scenes/GravitySim.tscn";

	public override void _Ready()
	{
		SplashLayout.Apply(this);
		SplashProgress.Init(this);

		SplashAnimator.Play(
			this,
			onComplete: () =>
			{
				SplashSceneLoader.SwitchTo(NextScenePath);
			}
		);
	}
}
