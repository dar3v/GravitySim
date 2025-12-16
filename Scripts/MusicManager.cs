using Godot;
using System;

/// <summary>
/// Simple OOP music manager for Godot C#.
/// - Loads an audio file at runtime (mp3/ogg/wav)
/// - Plays it and optionally loops it
/// - Exposes Play/Stop/SetVolume methods and a singleton Instance
/// </summary>
public partial class MusicManager : Node
{
	// Path to your mp3 file. Set this in the Inspector or leave default.
	[Export]
	public string MusicPath { get; set; } = "res://Assets/space.mp3";

	[Export]
	public float VolumeDb { get; set; } = -3f;

	[Export]
	public bool Loop { get; set; } = true;

	private AudioStreamPlayer _player;

	// Optional: global access to the music manager (singleton pattern)
	public static MusicManager Instance { get; private set; }

	public override void _EnterTree()
	{
		// Enforce single instance if multiple added accidentally
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}

	public override void _Ready()
	{
		// Create an AudioStreamPlayer at runtime and attach to this node.
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

		// If Loop is requested, re-play when finished (robust regardless of import settings).
		if (Loop)
		{
			// Connect finished signal -> restart playback
			_player.Connect("finished", new Callable(this, nameof(OnFinished)));
		}

		_player.Play();
	}

	private void OnFinished()
	{
		// Restart playback for looping
		if (_player != null)
			_player.Play();
	}

	// --- OOP-friendly API ---
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
