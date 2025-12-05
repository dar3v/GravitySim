using System;
using Godot;

// GvObject is the universal body class. Any object with mass (Sun, Planet, Satellite) 
// uses this script for movement and gravitational influence.
public partial class GvObject : Node3D
{
    [Export] public float Mass { get; set; } = 1.0f; // Defines gravitational strength (small for satellites, large for suns)
    [Export] public Vector3 InitialVelocity = Vector3.Zero; 
    
    public Vector3 Velocity = Vector3.Zero;
    private float _G = Globals.G;
    public float SchwarzschildRadius { get; private set; } = 0.0f;

    public override void _Ready()
    {
        // r_s = 2GM / c^2
        // SchwarzschildRadius = 2.0f * Globals.G * Mass / Globals.CSquared;
        
        Velocity = InitialVelocity;
        Globals.AddGvObject(this);
    }

    public override void _ExitTree()
    {
        Globals.RmGvObject(this);
    }
}