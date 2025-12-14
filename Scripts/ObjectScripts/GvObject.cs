using Godot;

public partial class GvObject : Node3D
{
    [Export] public float Density { get; set; } = 1.0f;
    [Export] public Vector3 InitialVelocity = Vector3.Zero;

    [Export]
    public float Mass
    {
        get => _mass;
        set
        {
            if (value <= 0f)
                return;

            _mass = value;
            UpdateGvObject();
        }
    }
    private float _mass = 1.0f;

    public float Radius { get; private set; }
    public Vector3 Velocity { get; set; }
    public bool IsSelected { get; private set; }

    private MeshInstance3D _meshInstance;
    private OmniLight3D _light;

    private Material _baseMaterial;
    private StandardMaterial3D _selectionMaterial;
    private StaticBody3D _body;

    public override void _Ready()
    {
        _meshInstance = GetChild<MeshInstance3D>(0);
        Velocity = InitialVelocity;

        // sanity check collision mask and layer
        _body = GetNode<StaticBody3D>("StaticBody3D");
        _body.CollisionMask = 1;
        _body.CollisionLayer = 1;

        SetupMaterials();
        SetupLight();

        Globals.AddGvObject(this);
        UpdateGvObject();
    }

    public override void _ExitTree()
    {
        Globals.RmGvObject(this);
    }

    private void SetupMaterials()
    {
        if (_meshInstance?.MaterialOverride is not StandardMaterial3D mat)
            return;

        _baseMaterial = mat;

        mat.EmissionEnabled = true;
        mat.Emission = Colors.White;
        mat.EmissionEnergyMultiplier = 2.0f;

        _selectionMaterial = new StandardMaterial3D
        {
            AlbedoColor = Colors.White,
            EmissionEnabled = true,
            Emission = new Color(1f, 0.9f, 0.3f),
            EmissionEnergyMultiplier = 6.0f,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
    }

    private void SetupLight()
    {
        _light = new OmniLight3D
        {
            LightColor = Colors.White,
            LightEnergy = 1.0f
        };

        AddChild(_light);
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected)
            return;

        IsSelected = selected;

        if (_meshInstance == null)
            return;

        _meshInstance.MaterialOverride = selected
            ? _selectionMaterial
            : _baseMaterial;
    }

    private void UpdateGvObject()
    {
        // R = (3M / (4πρ))^(1/3)
        Radius = Mathf.Pow(
            (3f * _mass) / (4f * Mathf.Pi * Density),
            1f / 3f
        );

        // handle scalings
        if (_meshInstance != null)
            _meshInstance.Scale = Vector3.One * Radius;
        if (_body != null)
            _body.Scale = Vector3.One * Radius;
        if (_light != null)
        {
            _light.OmniRange = Radius * 4f;
            _light.LightEnergy = Mathf.Log(_mass + 1f);
        }

    }
}
