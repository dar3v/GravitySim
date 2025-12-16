using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Globals : Node
{
    // public const float G = 0.1f; // Gravitational constant
    // public const float SchwarzG = 1.0f; // Schwarzschild
    // public const float CSquared = 1.0f; // Speed of light squared

    public static event Action ObjectsChanged;
    private static readonly List<GvObject> _gvObjects = [];

    public override void _Ready()
    {
        GvDatabase.Initialize();
        GD.Print("Initialized Database");
    }

    public static void AddGvObject(GvObject gvObject)
    {
        if (!_gvObjects.Contains(gvObject))
        {
            _gvObjects.Add(gvObject);
            GD.Print($"Registered GvObject: {gvObject.Name}");
            ObjectsChanged?.Invoke();
        }
    }

    public static void RmGvObject(GvObject gvObject)
    {
        _gvObjects.Remove(gvObject);
    }

    public static List<GvObject> GetAllGvObjects()
    {
        return _gvObjects;
    }
}
