using Godot;

public partial class User : Camera3D
{
    /* =======================
     * Tunables
     * ======================= */

    [Export] public float MouseSensitivity = 0.15f;
    [Export] public float MoveSpeed = 5f;
    [Export] public float FastMoveMultiplier = 2f;
    [Export] public float MassEditFactor = 1.2f;

    /* =======================
     * State
     * ======================= */

    private bool _isLooking;
    private Vector2 _mouseDelta;

    private float _pitch;
    private float _yaw;

    private GvObject _selectedObject;

    /* =======================
     * Lifecycle
     * ======================= */

    public override void _Ready()
    {
        Current = true; // THIS IS CRITICAL
        _pitch = RotationDegrees.X;
        _yaw = RotationDegrees.Y;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Input(InputEvent @event)
    {
        HandleMouseButtons(@event);
        HandleMouseMotion(@event);
        HandleMassEditing(@event);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        UpdateLook();
        UpdateMovement(dt);

        _mouseDelta = Vector2.Zero;
    }

    /* =======================
     * Input handling
     * ======================= */

    private void HandleMouseButtons(InputEvent e)
    {
        if (e is not InputEventMouseButton mb)
            return;

        // RMB → look
        if (mb.ButtonIndex == MouseButton.Right)
        {
            _isLooking = mb.Pressed;
            Input.MouseMode = _isLooking
                ? Input.MouseModeEnum.Captured
                : Input.MouseModeEnum.Visible;
        }

        // LMB → select
        if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            TrySelectObject(mb.Position);
        }
    }

    private void HandleMouseMotion(InputEvent e)
    {
        if (!_isLooking)
            return;

        if (e is InputEventMouseMotion motion)
            _mouseDelta += motion.Relative;
    }

    private void HandleMassEditing(InputEvent e)
    {
        if (_selectedObject == null)
            return;

        if (e is not InputEventKey key || !key.Pressed)
            return;

        switch (key.Keycode)
        {
            case Key.Bracketright:
                AdjustMass(_selectedObject, MassEditFactor);
                break;

            case Key.Bracketleft:
                AdjustMass(_selectedObject, 1f / MassEditFactor);
                break;
        }
    }

    /* =======================
     * Camera behavior
     * ======================= */

    private void UpdateLook()
    {
        if (!_isLooking)
            return;

        _yaw -= _mouseDelta.X * MouseSensitivity;
        _pitch -= _mouseDelta.Y * MouseSensitivity;

        _pitch = Mathf.Clamp(_pitch, -89f, 89f);

        RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }

    private void UpdateMovement(float dt)
    {
        Vector3 direction = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W)) direction -= Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.S)) direction += Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.A)) direction -= Transform.Basis.X;
        if (Input.IsKeyPressed(Key.D)) direction += Transform.Basis.X;

        if (_isLooking)
        {
            if (Input.IsKeyPressed(Key.Q)) direction -= Transform.Basis.Y;
            if (Input.IsKeyPressed(Key.E)) direction += Transform.Basis.Y;
        }

        if (direction == Vector3.Zero)
            return;

        float speed = Input.IsKeyPressed(Key.Shift)
            ? MoveSpeed * FastMoveMultiplier
            : MoveSpeed;

        GlobalPosition += direction.Normalized() * speed * dt;
    }

    /* =======================
     * Selection
     * ======================= */

    private void TrySelectObject(Vector2 screenPos)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        Vector3 origin = ProjectRayOrigin(screenPos);
        Vector3 dir = ProjectRayNormal(screenPos);
        Vector3 end = origin + dir * 10_000f;

        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var hit = spaceState.IntersectRay(query);

        if (hit.Count == 0)
        {
            ClearSelection();
            return;
        }

        Node node = hit["collider"].As<Node>();

        if (hit["collider"].As<Node>() is Node collider &&
            collider.HasMeta("owner"))
        {
            SetSelectedObject((GvObject)collider.GetMeta("owner"));
        }

        if (node is GvObject gv)
        {
            SetSelectedObject(gv);
            GD.Print($"Selected: {gv.Name} | Mass: {gv.Mass} | Radius: {gv.Radius}");
        }
        else
        {
            ClearSelection();
        }
    }

    private void SetSelectedObject(GvObject gv)
    {
        if (_selectedObject == gv)
            return;

        ClearSelection();

        _selectedObject = gv;
        _selectedObject.SetSelected(true);
    }

    private void ClearSelection()
    {
        if (_selectedObject == null)
            return;

        _selectedObject.SetSelected(false);
        _selectedObject = null;
    }

    private static void AdjustMass(GvObject obj, float factor)
    {
        obj.Mass *= factor;
        GD.Print($"Mass updated → {obj.Mass}");
    }
}
