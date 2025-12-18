using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Globals : Node
{
    [Export] private float Gravitationalconstant = 1.0f;
    public static float G { get; set; } // Gravitational constant

    public static event Action ObjectsChanged;
    public static List<GvObject> GvObjects = [];

    public static bool SimulationPaused { get; private set; }
    public static event Action<bool> PauseChanged;

    public override void _Ready()
    {
        GvDatabase.Initialize();
        GD.Print("Initialized Database");
        G = Gravitationalconstant;
    }

    public static float GetG()
    {
        return G;
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

    public static void SetPaused(bool paused)
    {
        if (SimulationPaused == paused)
            return;

        SimulationPaused = paused;
        PauseChanged?.Invoke(paused);
    }

    public static void TogglePaused()
    {
        SetPaused(!SimulationPaused);
    }

    public static List<GvObject> GetAllGvObjects()
    {
        return GvObjects;
    }

    public static void ClearSimulation(Node root)
    {
        if (root == null)
            return;

        var toDelete = new List<GvObject>();

        foreach (Node child in root.GetChildren())
        {
            if (child is GvObject gv)
                toDelete.Add(gv);
        }

        foreach (var gv in toDelete)
            gv.QueueFree();

        GvObjects.Clear();
        ObjectsChanged.Invoke();

        GD.Print("Simulation cleared.");
    }
}
