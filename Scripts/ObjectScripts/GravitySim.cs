using Godot;
using System.Collections.Generic;

public partial class GravitySim : Node3D
{
    [Export] public float G = 1.0f;
    [Export] public float Softening = 0.01f;
    [Export] public bool RemoveCenterOfMassDrift = true;

    private Vector3[] _accelerations;

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        List<GvObject> objs = Globals.GetAllGvObjects();
        int count = objs.Count;
        if (count < 2)
            return;

        EnsureAccelerationBuffer(count);

        // --- First acceleration pass ---
        ComputeAccelerations(objs);

        // --- Half-step velocity update ---
        for (int i = 0; i < count; i++)
            objs[i].Velocity += _accelerations[i] * (0.5f * dt);

        // --- Position update ---
        for (int i = 0; i < count; i++)
            objs[i].GlobalPosition += objs[i].Velocity * dt;

        // --- Recompute accelerations ---
        ComputeAccelerations(objs);

        // --- Final velocity update ---
        for (int i = 0; i < count; i++)
            objs[i].Velocity += _accelerations[i] * (0.5f * dt);

        if (RemoveCenterOfMassDrift)
            StabilizeCenterOfMass(objs);
    }

    private void ComputeAccelerations(List<GvObject> objs)
    {
        int count = objs.Count;

        // Clear acceleration buffer
        for (int i = 0; i < count; i++)
            _accelerations[i] = Vector3.Zero;

        float softSq = Softening * Softening;

        for (int i = 0; i < count; i++)
        {
            GvObject a = objs[i];

            for (int j = i + 1; j < count; j++)
            {
                GvObject b = objs[j];

                Vector3 offset = b.GlobalPosition - a.GlobalPosition;
                float distSq = offset.LengthSquared() + softSq;

                float dist = Mathf.Sqrt(distSq);
                float invDist = 1.0f / dist;
                float invDist3 = invDist * invDist * invDist;

                Vector3 forceDir = offset * invDist3;

                Vector3 accA = forceDir * (G * b.Mass);
                Vector3 accB = forceDir * (G * a.Mass);

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
