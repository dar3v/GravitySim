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
    public static readonly List<GvObject> GvObjects = [];

    public override void _Ready()
    {
        GvDatabase.Initialize();
        GD.Print("Initialized Database");
    }

    public static void AddGvObject(GvObject gvObject)
    {
        if (!GvObjects.Contains(gvObject))
        {
            GvObjects.Add(gvObject);
            GD.Print($"Registered GvObject: {gvObject.Name}");
            ObjectsChanged?.Invoke();
        }
    }

    public static void RmGvObject(GvObject gvObject)
    {
        GvObjects.Remove(gvObject);
    }

    public static List<GvObject> GetAllGvObjects()
    {
        return GvObjects;
    }

    public static void ClearAll()
    {
        GvObjects.Clear();
        ObjectsChanged?.Invoke();
    }
}
