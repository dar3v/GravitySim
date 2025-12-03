using System;
using Godot;

public partial class GravitySim : Node3D
{
    [Export] public float G = 6.6743e-11f;

    // public override void _Ready()
    // {

    // }

    public override void _Process(double delta)
    {
        foreach (var obj in Globals.GetAllGvObjects())
        {
            foreach (var obj2 in Globals.GetAllGvObjects())
            {
                if (obj2 == obj) { continue; };

                float dx = obj.Position.X - obj2.Position.X;
                float dz = obj.Position.Z - obj2.Position.Z;
                float distance = MathF.Sqrt(dx*dx + dz*dz);
                Vector3 direction = new(dx / distance, 0, dz / distance);
                distance *= 1000;

                float gforce = G * obj.Mass * obj2.Mass / distance*distance;
                float acc1 = gforce / obj.Mass;
                Vector3 acc = new(acc1 * direction.X, 0, acc1 * direction.Z);

                obj.Position.MoveToward(acc, (float)delta);
                GD.Print($"{obj.Position}");
            }
        }
    }
}