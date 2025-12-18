using Godot;
using System;

public partial class MainUi : CanvasLayer
{
    // MenuButtons
    [Export] private MenuButton _saveMenu;
    [Export] private MenuButton _loadMenu;
    [Export] private MenuButton _simMenu;

    // etc
    [Export] private ItemList _objectList;
    [Export] private Button _addObject;

    public override void _Ready()
    {
        // Get the popups
        var savePopup = _saveMenu.GetPopup();
        var loadPopup = _saveMenu.GetPopup();

        // Add items
        // savePopup.AddItem("Save Simulation", 0);

        loadPopup.AddItem("Load Simulation", 1);
        loadPopup.AddItem("Delete Simulation", 2);

        // Subscribe to events
        loadPopup.IndexPressed += OnLoadMenuPressed;
        _addObject.Pressed += ShowAddObjectDialog;
        Globals.ObjectsChanged += UpdateObjectList;
    }

    public override void _ExitTree()
    {
        // Unsubscribe when this node is removed
        Globals.ObjectsChanged -= UpdateObjectList;
    }

    private void UpdateObjectList()
    {
        _objectList.Clear();
        var objects = Globals.GetAllGvObjects();

        foreach (var obj in objects)
        {
            // Display format: Name | Mass | Density
            string displayText = $"{obj.Name} | M={obj.Mass:F1} D={obj.Density:F1}";
            _objectList.AddItem(displayText);
        }
    }

    private void OnLoadMenuPressed(long index)
    {
        GD.Print($"File menu item {index} pressed");

        switch (index)
        {
            case 0: // Save
                break;
            case 1: // Load
                break;
            case 2: // Delete
                break;
        }
    }

    private void ShowAddObjectDialog()
    {
        var dialog = new ConfirmationDialog();
        dialog.Title = "Add Object";
        dialog.Size = new Vector2I(450, 400);

        var vbox = new VBoxContainer();

        // Name
        vbox.AddChild(new Label { Text = "Name:" });
        var nameInput = new LineEdit { PlaceholderText = "Object name" };
        vbox.AddChild(nameInput);

        // Position
        vbox.AddChild(new Label { Text = "Position (X, Y, Z):" });
        var posHBox = new HBoxContainer();
        var posX = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        var posY = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        var posZ = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        posHBox.AddChild(posX);
        posHBox.AddChild(posY);
        posHBox.AddChild(posZ);
        vbox.AddChild(posHBox);

        // Mass
        vbox.AddChild(new Label { Text = "Mass:" });
        var mass = new SpinBox { MinValue = 0.1, MaxValue = 10000, Step = 0.1, Value = 1.0 };
        vbox.AddChild(mass);

        // Density
        vbox.AddChild(new Label { Text = "Density:" });
        var density = new SpinBox { MinValue = 0.1, MaxValue = 10000, Step = 0.1, Value = 1.0 };
        vbox.AddChild(density);

        // Initial Velocity
        vbox.AddChild(new Label { Text = "Initial Velocity (X, Y, Z):" });
        var velHBox = new HBoxContainer();
        var velX = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        var velY = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        var velZ = new SpinBox { MinValue = -1000, MaxValue = 1000, Step = 0.1 };
        velHBox.AddChild(velX);
        velHBox.AddChild(velY);
        velHBox.AddChild(velZ);
        vbox.AddChild(velHBox);

        dialog.AddChild(vbox);

        // When confirmed, create the object
        dialog.Confirmed += () =>
        {
            CreateObject(
                nameInput.Text,
                new Vector3((float)posX.Value, (float)posY.Value, (float)posZ.Value),
                (float)mass.Value,
                (float)density.Value,
                new Vector3((float)velX.Value, (float)velY.Value, (float)velZ.Value)
            );
            dialog.QueueFree();
        };

        dialog.CloseRequested += dialog.QueueFree;

        AddChild(dialog);
        dialog.PopupCentered();
    }

    private void CreateObject(string name, Vector3 position, float mass, float density, Vector3 velocity)
    {
        var scene = GD.Load<PackedScene>("res://Scenes/Objects/GvObject.tscn");
        var obj = scene.Instantiate<GvObject>();

        obj.Name = string.IsNullOrWhiteSpace(name) ? $"Object_{Globals.GetAllGvObjects().Count + 1}" : name;
        obj.Mass = mass;
        obj.InitialVelocity = velocity;

        // Find parent node
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Node3D node3d && child.Name != "MainUI")
            {
                node3d.AddChild(obj);
                obj.GlobalPosition = position;
                GD.Print($"Added {obj.Name}");
                return;
            }
        }
    }
}
