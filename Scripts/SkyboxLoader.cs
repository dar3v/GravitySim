using Godot;
using System;

public partial class SkyboxLoader : Node3D
{
	[Export]
	public string SkyPackedScenePath { get; set; } = "res://Assets/skybox.glb";

	[Export]
	public float ScaleMultiplier { get; set; } = 200f;

	[Export]
	public bool FollowCamera { get; set; } = true;

	private Node3D _skyInstance;

	public override void _Ready()
	{
		var packed = GD.Load<PackedScene>(SkyPackedScenePath);
		if (packed == null)
		{
			GD.PrintErr($"SkyboxLoader: could not load PackedScene at '{SkyPackedScenePath}'");
			return;
		}

		var inst = packed.Instantiate();
		_skyInstance = inst as Node3D;
		if (_skyInstance == null)
		{
			GD.PrintErr("SkyboxLoader: root of the .glb scene is not a Node3D. Make sure the .glb's root is a Node3D/Spatial.");
			return;
		}

		_skyInstance.Scale = _skyInstance.Scale * ScaleMultiplier;

		AddChild(_skyInstance);
	}

	public override void _Process(double delta)
	{
		if (!FollowCamera || _skyInstance == null)
			return;

		var cam = GetViewport().GetCamera3D();
		if (cam != null)
			_skyInstance.GlobalPosition = cam.GlobalPosition;
	}
}
