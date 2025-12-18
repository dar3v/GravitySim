using Godot;
using System;

// Loads and displays a 3D skybox from a .glb file
public partial class SkyboxLoader : Node3D
{
	// Path to the skybox .glb scene
	[Export]
	public string SkyPackedScenePath { get; set; } = "res://Assets/skybox.glb";

	// Scale factor to make the skybox large
	[Export]
	public float ScaleMultiplier { get; set; } = 200f;

	// If true, skybox follows the camera position
	[Export]
	public bool FollowCamera { get; set; } = true;

	// Instance of the loaded skybox
	private Node3D _skyInstance;

	public override void _Ready()
	{
		// Load the skybox scene
		var packed = GD.Load<PackedScene>(SkyPackedScenePath);
		if (packed == null)
		{
			GD.PrintErr($"SkyboxLoader: could not load PackedScene at '{SkyPackedScenePath}'");
			return;
		}

		// Instantiate the skybox
		var inst = packed.Instantiate();
		_skyInstance = inst as Node3D;
		if (_skyInstance == null)
		{
			GD.PrintErr("SkyboxLoader: root of the .glb scene is not a Node3D.");
			return;
		}

		// Scale the skybox to surround the scene
		_skyInstance.Scale = _skyInstance.Scale * ScaleMultiplier;

		// Add skybox to the scene
		AddChild(_skyInstance);
	}

	public override void _Process(double delta)
	{
		// Keep skybox centered on the camera
		if (!FollowCamera || _skyInstance == null)
			return;

		var cam = GetViewport().GetCamera3D();
		if (cam != null)
			_skyInstance.GlobalPosition = cam.GlobalPosition;
	}
}
