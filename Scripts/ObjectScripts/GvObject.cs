using System;
using Godot;

// GvObject is the universal body class. Any object with mass (Sun, Planet, Satellite) 
// uses this script for movement and gravitational influence.
public partial class GvObject : Node3D
{
    [Export] public float Mass { get; set; } = 1.0f;
    [Export] public float Radius { get; set; } = 1.0f;
    [Export] public Vector3 InitialVelocity = Vector3.Zero; 
    
    // public float SchwarzschildRadius { get; private set; } = 0.0f;
    public Vector3 Velocity = Vector3.Zero;
    private MeshInstance3D _meshInstance;  // Reference to MeshInstance3D

    public override void _Ready()
    {
        // r_s = 2GM / c^2
        // SchwarzschildRadius = 2.0f * Globals.G * Mass / Globals.CSquared;

        // Assuming the MeshInstance3D is the first child of GvObject
        // NOTE: this will bug out if MeshInstance is not the first child of this Object
        _meshInstance = GetChild<MeshInstance3D>(0);
        if (_meshInstance is not null)
        {
            _meshInstance.Scale = new Vector3(Radius, Radius, Radius);
        }

        Velocity = InitialVelocity;
        Globals.AddGvObject(this);
    }

    public override void _ExitTree()
    {
        Globals.RmGvObject(this);
    }
}