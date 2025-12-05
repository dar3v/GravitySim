using System;
using System.Collections.Generic;
using Godot;

public partial class GravitySim : Node3D
{
    [Export] public float G = 6.6743f; // gravitational constant

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        List<GvObject> objs = Globals.GetAllGvObjects();
        int count = objs.Count;

        // store computed accelerations per object
        Vector3[] accelerations = new Vector3[count];

        // pairwise loop, O(N^2) time complexity, O(N) space complexity
        // TODO: move ts comment to docu 🦖
        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                var obj1 = objs[i];
                var obj2 = objs[j];

                Vector3 offset = obj2.GlobalPosition - obj1.GlobalPosition;
                float distanceSq = offset.LengthSquared();

                if (distanceSq < 0.0001f) continue; // avoid singularities

                // F = (G * M_1 * M_2) / r^2
                float gforce = G * obj1.Mass * obj2.Mass / distanceSq;

                float distance = MathF.Sqrt(distanceSq); // idk why this is but yes
                Vector3 direction = offset / distance;

                // a = F / m
                Vector3 acc1 = direction * (gforce / obj1.Mass);
                Vector3 acc2 = direction * (gforce / obj2.Mass);

                accelerations[i] += acc1;
                accelerations[j] -= acc2;
            }
        }

        // update velocities
        for (int i = 0; i < count; i++)
        {
            objs[i].Velocity += accelerations[i] * dt;
        }
        
        // update positions
        for (int i = 0; i < count; i++)
        {
            objs[i].GlobalPosition += objs[i].Velocity * dt;
            GD.Print($"Updating Position: {objs[i].GlobalPosition}");
        }

        // if (obj2 == obj) { continue; };

        // float dx = obj.Position.X - obj2.Position.X;
        // float dz = obj.Position.Z - obj2.Position.Z;
        // float distance = MathF.Sqrt(dx*dx + dz*dz);
        // Vector3 direction = new(dx / distance, 0, dz / distance);

        // float gforce = G * obj.Mass * obj2.Mass / (distance * distance);
        // float acc1 = gforce / obj.Mass;
        // Vector3 acc = new(acc1 * direction.X, 0, acc1 * direction.Z);

        // obj.GlobalPosition += acc;
        // GD.Print($"{obj.Position}");
    }
}