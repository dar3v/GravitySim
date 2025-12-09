using Godot;
using System;
using System.Collections.Generic;

public partial class Globals : Node
{
    // public const float G = 0.1f; // Gravitational constant
    // public const float SchwarzG = 1.0f; // Schwarzschild
    // public const float CSquared = 1.0f; // Speed of light squared
    
    private static readonly List<GvObject> _gvObjects = [];

    public static void AddGvObject(GvObject gvObject)
    {
        if (!_gvObjects.Contains(gvObject))
        {
            _gvObjects.Add(gvObject);
            GD.Print($"Registered GvObject: {gvObject.Name}");
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
