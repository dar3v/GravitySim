using Godot;
using System;

public partial class MusicManager : Node
{
	[Export]
	public string MusicPath { get; set; } = "res://Assets/space.mp3";

	[Export]
	public float VolumeDb { get; set; } = -3f;

	[Export]
	public bool Loop { get; set; } = true;

	private AudioStreamPlayer _player;

	public static MusicManager Instance { get; private set; }

	public override void _EnterTree()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		AddChild(_player);

		var stream = GD.Load<AudioStream>(MusicPath);
		if (stream == null)
		{
			GD.PrintErr($"MusicManager: could not load audio at '{MusicPath}'");
			return;
		}

		_player.Stream = stream;
		_player.VolumeDb = VolumeDb;

		if (Loop)
		{
			_player.Connect("finished", new Callable(this, nameof(OnFinished)));
		}

		_player.Play();
	}

	private void OnFinished()
	{
		if (_player != null)
			_player.Play();
	}

	public void PlayMusic()
	{
		if (_player != null && !_player.Playing)
			_player.Play();
	}

	public void StopMusic()
	{
		if (_player != null && _player.Playing)
			_player.Stop();
	}

	public void SetVolumeDb(float db)
	{
		if (_player != null)
			_player.VolumeDb = db;
	}

	public bool IsPlaying()
	{
		return _player != null && _player.Playing;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}
}
