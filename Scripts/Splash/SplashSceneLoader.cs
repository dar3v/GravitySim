using Godot;

public static class SplashSceneLoader
{
	// Switches to another scene by file path
	public static void SwitchTo(string scenePath)
	{
		// Get the current SceneTree
		var tree = Engine.GetMainLoop() as SceneTree;

		// Load and change to the target scene
		tree?.ChangeSceneToFile(scenePath);
	}
}
