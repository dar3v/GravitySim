using Godot;
using System.Collections.Generic;

public partial class GravitySim : Node3D
{
    [Export] public float G = 1.0f;

    // Increased softening for real-time stability
    [Export] public float Softening = 0.5f;

    [Export] public bool RemoveCenterOfMassDrift = true;

    // Safety limits
    [Export] public float MaxAcceleration = 1000f;
    [Export] public float MaxVelocity = 500f;

    private Vector3[] _accelerations;
    private int _comFrameCounter = 0;

    public override void _PhysicsProcess(double delta)
    {
        if (Globals.SimulationPaused)
          return;

        float dt = Mathf.Min((float)delta, 1f / 30f);

        List<GvObject> objs = Globals.GetAllGvObjects();
        int count = objs.Count;
        if (count < 2)
            return;

        EnsureAccelerationBuffer(count);

        ComputeAccelerations(objs);

        // Half-step velocity
        for (int i = 0; i < count; i++)
            objs[i].Velocity += _accelerations[i] * (0.5f * dt);

        // Position update
        for (int i = 0; i < count; i++)
            objs[i].GlobalPosition += objs[i].Velocity * dt;

        ComputeAccelerations(objs);

        // Final velocity update + clamp
        for (int i = 0; i < count; i++)
        {
            objs[i].Velocity += _accelerations[i] * (0.5f * dt);

            if (objs[i].Velocity.Length() > MaxVelocity)
                objs[i].Velocity =
                    objs[i].Velocity.Normalized() * MaxVelocity;
        }

        // Apply COM stabilization only occasionally
        if (RemoveCenterOfMassDrift && (++_comFrameCounter % 10 == 0))
            StabilizeCenterOfMass(objs);
    }

    public static void ResetSimulation()
    {
        foreach (var obj in Globals.GetAllGvObjects())
        {
            obj.ResetState();
        }
        GD.Print("Simulation Reset (soft)");
    }

    private void ComputeAccelerations(List<GvObject> objs)
    {
        int count = objs.Count;
        for (int i = 0; i < count; i++)
            _accelerations[i] = Vector3.Zero;

        float softSq = Softening * Softening;

        for (int i = 0; i < count; i++)
        {
            var a = objs[i];

            for (int j = i + 1; j < count; j++)
            {
                var b = objs[j];

                Vector3 r = b.GlobalPosition - a.GlobalPosition;
                float distSq = r.LengthSquared() + softSq;

                float invDist = 1.0f / Mathf.Sqrt(distSq);
                float invDist3 = invDist * invDist * invDist;

                Vector3 acc = r * invDist3;

                Vector3 accA = acc * (G * b.Mass);
                Vector3 accB = acc * (G * a.Mass);

                // Clamp acceleration
                if (accA.Length() > MaxAcceleration)
                    accA = accA.Normalized() * MaxAcceleration;
                if (accB.Length() > MaxAcceleration)
                    accB = accB.Normalized() * MaxAcceleration;

                _accelerations[i] += accA;
                _accelerations[j] -= accB;
            }
        }
    }

    private static void StabilizeCenterOfMass(List<GvObject> objs)
    {
        Vector3 totalMomentum = Vector3.Zero;
        float totalMass = 0f;

        foreach (var o in objs)
        {
            totalMomentum += o.Velocity * o.Mass;
            totalMass += o.Mass;
        }

        if (totalMass <= 0f)
            return;

        Vector3 drift = totalMomentum / totalMass;

        foreach (var o in objs)
            o.Velocity -= drift;
    }

    private void EnsureAccelerationBuffer(int count)
    {
        if (_accelerations == null || _accelerations.Length != count)
            _accelerations = new Vector3[count];
    }
}
