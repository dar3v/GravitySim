using Godot;

public static class SplashSceneLoader
{
	public static void SwitchTo(string scenePath)
	{
		var tree = Engine.GetMainLoop() as SceneTree;
		tree?.ChangeSceneToFile(scenePath);
	}
}
