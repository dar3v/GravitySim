using Godot;
using System;

public partial class User : Camera3D
{
    [Export] public float MouseSensitivity = 0.15f;
    [Export] public float MoveSpeed = 5f;
    [Export] public float FastMoveMultiplier = 2f;

    private Vector2 _mouseDelta = Vector2.Zero;
    private bool _isLooking = false;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    public override void _Ready()
    {
        _rotationX = RotationDegrees.X;
        _rotationY = RotationDegrees.Y;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Input(InputEvent @event)
    {
        // Start looking with RMB
        if (@event is InputEventMouseButton mb)
        {
            if (mb.Pressed)
            {
                _isLooking = true;
                Input.MouseMode = Input.MouseModeEnum.Captured; // Captured cursor
            }
            else
            {
                _isLooking = false;
                Input.MouseMode = Input.MouseModeEnum.Visible; // Visible cursor
            }
        }

        if (@event is InputEventMouseMotion motion && _isLooking)
        {
            _mouseDelta = motion.Relative;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Handle looking
        if (_isLooking)
        {
            _rotationY -= _mouseDelta.X * MouseSensitivity;
            _rotationX -= _mouseDelta.Y * MouseSensitivity;

            _rotationX = Mathf.Clamp(_rotationX, -89f, 89f);

            RotationDegrees = new Vector3(_rotationX, _rotationY, 0);
        }

        _mouseDelta = Vector2.Zero;

        // Handle movement
        Vector3 velocity = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W)) velocity -= Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.S)) velocity += Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.A)) velocity -= Transform.Basis.X;
        if (Input.IsKeyPressed(Key.D)) velocity += Transform.Basis.X;

        // Up/down only if right-clicking
        if (_isLooking)
        {
            if (Input.IsKeyPressed(Key.Q)) velocity -= Transform.Basis.Y;
            if (Input.IsKeyPressed(Key.E)) velocity += Transform.Basis.Y;
        }

        if (velocity != Vector3.Zero)
        {
            float speed = MoveSpeed;
            if (Input.IsKeyPressed(Key.Shift)) speed *= FastMoveMultiplier;

            GlobalPosition += velocity.Normalized() * speed * dt;
        }
    }
}
