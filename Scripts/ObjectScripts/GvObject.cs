using System;
using Godot;

public partial class GvObject : Node3D
{
    [Export] public float Velocity;
    [Export] public float Radius;
    [Export] public float Mass;

    public override void _Ready()
    {
        Mass *= MathF.Pow(10, 22);

        // add any GvObject instaniated into the Global GvObject List
        Globals.GvObjects.Add(this);
    }
}