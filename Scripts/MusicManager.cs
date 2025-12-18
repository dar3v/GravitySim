using Godot;
using System;

// Manages background music playback
public partial class MusicManager : Node
{
	// Path to the background music file
	[Export]
	public string MusicPath { get; set; } = "res://Assets/space.mp3";

	// Volume level in decibels
	[Export]
	public float VolumeDb { get; set; } = -3f;

	// Enable or disable looping
	[Export]
	public bool Loop { get; set; } = true;

	// Audio player instance
	private AudioStreamPlayer _player;

	// Singleton instance for global access
	public static MusicManager Instance { get; private set; }

	public override void _EnterTree()
	{
		// Ensure only one MusicManager exists
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}

	public override void _Ready()
	{
		// Create and attach the audio player
		_player = new AudioStreamPlayer();
		AddChild(_player);

		// Load the music file
		var stream = GD.Load<AudioStream>(MusicPath);
		if (stream == null)
		{
			GD.PrintErr($"MusicManager: could not load audio at '{MusicPath}'");
			return;
		}

		// Apply audio settings
		_player.Stream = stream;
		_player.VolumeDb = VolumeDb;

		// Restart music when it finishes (looping)
		if (Loop)
		{
			_player.Connect("finished", new Callable(this, nameof(OnFinished)));
		}

		// Start playing music
		_player.Play();
	}

	// Called when the music finishes
	private void OnFinished()
	{
		if (_player != null)
			_player.Play();
	}

	// Play the background music
	public void PlayMusic()
	{
		if (_player != null && !_player.Playing)
			_player.Play();
	}

	// Stop the background music
	public void StopMusic()
	{
		if (_player != null && _player.Playing)
			_player.Stop();
	}

	// Change the music volume
	public void SetVolumeDb(float db)
	{
		if (_player != null)
			_player.VolumeDb = db;
	}

	// Check if music is currently playing
	public bool IsPlaying()
	{
		return _player != null && _player.Playing;
	}

	public override void _ExitTree()
	{
		// Clear singleton when node is removed
		if (Instance == this)
			Instance = null;
	}
}
