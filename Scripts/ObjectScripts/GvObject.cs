using Godot;

// GvObject is the universal body class. Any object with mass (Sun, Planet, Satellite) 
// uses this script for movement and gravitational influence.
public partial class GvObject : Node3D
{
    [Export] public float Mass { get; set; } = 1.0f; // Defines gravitational strength (small for satellites, large for suns)
    [Export] public Vector3 InitialVelocity = Vector3.Zero; 
    
    private Vector3 _velocity = Vector3.Zero;
    public float SchwarzschildRadius { get; private set; } = 0.0f;

    public override void _Ready()
    {
        // r_s = 2GM / c^2
        SchwarzschildRadius = 2.0f * Globals.G * Mass / Globals.CSquared;
        
        _velocity = InitialVelocity;
        Globals.AddGvObject(this);
    }

    public override void _ExitTree()
    {
        Globals.RmGvObject(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        // All bodies, regardless of mass, use the stable RK4 integration.
        IntegrateMotion((float)delta);
    }

    // Calculates the total relativistic acceleration this body experiences 
    private Vector3 GetTotalAcceleration(Vector3 tempPosition, Vector3 tempVelocity)
    {
        Vector3 totalAcceleration = Vector3.Zero;

        foreach (GvObject other in Globals.GetAllGvObjects())
        {
            if (other == this) continue; // Skip calculating gravity on self

            Vector3 r_vector = other.GlobalPosition - tempPosition;
            float r = r_vector.Length();
            float r_squared = r * r;

            if (r < 0.01f) continue;

            // Newtonian base acceleration: G * M / r^2
            float newtonianMagnitude = Globals.G * other.Mass / r_squared;
            
            // --- Post-Newtonian (Relativistic) Correction ---
            Vector3 angularMomentum_vector = r_vector.Cross(tempVelocity);
            float L_squared = angularMomentum_vector.LengthSquared();
            float c_squared = Globals.CSquared;

            float r_fourth = r_squared * r_squared;
            float correctionFactor = 3.0f * L_squared / (c_squared * r_fourth);
            
            float totalMagnitude = newtonianMagnitude * (1.0f + correctionFactor);

            totalAcceleration += r_vector.Normalized() * totalMagnitude;
        }
        return totalAcceleration;
    }
    
    private void IntegrateMotion(float dt)
    {
        Vector3 x0 = GlobalPosition;
        Vector3 v0 = _velocity;

        // --- RK4 Steps: (The calculation remains identical) ---
        Vector3 k1_v = v0;
        Vector3 k1_a = GetTotalAcceleration(x0, v0);

        Vector3 x_mid1 = x0 + k1_v * dt / 2.0f;
        Vector3 v_mid1 = v0 + k1_a * dt / 2.0f;
        Vector3 k2_v = v_mid1;
        Vector3 k2_a = GetTotalAcceleration(x_mid1, v_mid1);

        Vector3 x_mid2 = x0 + k2_v * dt / 2.0f;
        Vector3 v_mid2 = v0 + k2_a * dt / 2.0f;
        Vector3 k3_v = v_mid2;
        Vector3 k3_a = GetTotalAcceleration(x_mid2, v_mid2);

        Vector3 x_end = x0 + k3_v * dt;
        Vector3 v_end = v0 + k3_a * dt;
        Vector3 k4_v = v_end;
        Vector3 k4_a = GetTotalAcceleration(x_end, v_end);

        // --- Final Weighted Average and Update ---
        Vector3 avg_v = (k1_v + 2.0f * k2_v + 2.0f * k3_v + k4_v) / 6.0f;
        Vector3 avg_a = (k1_a + 2.0f * k2_a + 2.0f * k3_a + k4_a) / 6.0f;

        GlobalPosition += avg_v * dt;
        _velocity += avg_a * dt;
    }
}