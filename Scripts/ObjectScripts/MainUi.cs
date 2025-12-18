using Godot;
using System;

public partial class MainUi : CanvasLayer
{
    // MenuButtons
    [Export] private Button _saveMenu;
    [Export] private MenuButton _loadMenu;
    [Export] private MenuButton _simMenu;

    // etc
    [Export] private ItemList _objectList;
    [Export] private Button _addObject;

    public override void _Ready()
    {
        // Get the popups
        var loadPopup = _loadMenu.GetPopup();
        var simPopup = _simMenu.GetPopup();

        // load buttons
        loadPopup.AddItem("Load Simulation", 1);
        loadPopup.AddItem("Delete Simulation", 2);

        // sim buttons
        simPopup.AddItem("Pause Simulation", 1);
        simPopup.AddItem("Reset Simulation", 2);
        simPopup.AddItem("Clear Simulation", 3);

        // Subscribe to events
        _saveMenu.Pressed += OnSaveMenuPressed;
        loadPopup.IndexPressed += OnLoadMenuPressed;
        simPopup.IndexPressed += OnSimMenuPressed;
        _addObject.Pressed += OnAddObjectPressed;
        Globals.ObjectsChanged += UpdateObjectList;
    }

    public override void _ExitTree()
    {
        // Unsubscribe when this node is removed
        Globals.ObjectsChanged -= UpdateObjectList;
    }

    private void OnSaveMenuPressed()
    {
        var dialog = new ConfirmationDialog();
        dialog.Title = "Save Simulation";
        dialog.Size = new Vector2I(400, 200);

        var vbox = new VBoxContainer();

        // Simulation name
        vbox.AddChild(new Label { Text = "Simulation Name:" });
        var nameInput = new LineEdit { PlaceholderText = "Enter name" };
        vbox.AddChild(nameInput);

        // Info
        var objectCount = Globals.GetAllGvObjects().Count;
        vbox.AddChild(new Label { Text = $"Objects to save: {objectCount}" });

        dialog.AddChild(vbox);

        dialog.Confirmed += () =>
        {
            if (string.IsNullOrWhiteSpace(nameInput.Text))
            {
                ShowMessage("Please enter a valid name");
                return;
            }

            try
            {
                GvDatabase.SaveSimulation(nameInput.Text, Globals.G, Globals.GetAllGvObjects());
                ShowMessage($"Simulation '{nameInput.Text}' saved successfully!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Error saving: {ex.Message}");
            }

            dialog.QueueFree();
        };

        dialog.CloseRequested += dialog.QueueFree;

        AddChild(dialog);
        dialog.PopupCentered();
    }

    private void ShowMessage(string message)
    {
        var msgDialog = new AcceptDialog();
        msgDialog.DialogText = message;
        msgDialog.CloseRequested += msgDialog.QueueFree;
        AddChild(msgDialog);
        msgDialog.PopupCentered();
    }


    private void OnLoadMenuPressed(long index)
    {
        GD.Print($"File menu item {index} pressed");

        switch (index)
        {
            case 0: // Load
                ShowLoadDialog();
                break;
            case 1: // Delete
                ShowDeleteDialog();
                break;
        }
    }

    private void OnSimMenuPressed(long index)
    {
        GD.Print($"Simulation menu item {index} pressed");

        switch (index)
        {
            case 0: // Pause
                Globals.TogglePaused();
                break;
            case 1: // Reset
                GravitySim.ResetSimulation();
                break;
            case 2: // Clear
                Globals.ClearSimulation(GetTree().CurrentScene);
                break;
        }
    }

    private void ShowLoadDialog()
    {
        var simulations = GvDatabase.ListSimulations();

        if (simulations.Count == 0)
        {
            ShowMessage("No saved simulations found");
            return;
        }

        var dialog = new ConfirmationDialog();
        dialog.Title = "Load Simulation";
        dialog.Size = new Vector2I(500, 400);

        var vbox = new VBoxContainer();

        vbox.AddChild(new Label { Text = "Select a simulation to load:" });

        var list = new ItemList();
        list.CustomMinimumSize = new Vector2(0, 300);

        foreach (var sim in simulations)
        {
            string displayText = $"{sim.Name} | G={sim.GConstant:F4} | {sim.CreatedAt:yyyy-MM-dd HH:mm}";
            list.AddItem(displayText);
            list.SetItemMetadata(list.ItemCount - 1, sim.Id);
        }

        vbox.AddChild(list);
        dialog.AddChild(vbox);

        dialog.Confirmed += () =>
        {
            if (list.GetSelectedItems().Length > 0)
            {
                int selectedIndex = list.GetSelectedItems()[0];
                int simId = (int)list.GetItemMetadata(selectedIndex);
                LoadSimulation(simId);
            }
            dialog.QueueFree();
        };

        dialog.CloseRequested += dialog.QueueFree;

        AddChild(dialog);
        dialog.PopupCentered();
    }

    private void LoadSimulation(int simulationId)
    {
        try
        {
            // Find parent node
            var root = GetTree().Root;
            Node3D parent = null;

            foreach (Node child in root.GetChildren())
            {
                if (child is Node3D node3d && child.Name != "MainUI")
                {
                    parent = node3d;
                    break;
                }
            }

            if (parent == null)
            {
                ShowMessage("Error: No suitable parent node found");
                return;
            }

            GvDatabase.LoadSimulation(simulationId, null, parent);
            ShowMessage("Simulation loaded successfully!");
        }
        catch (Exception ex)
        {
            ShowMessage($"Error loading: {ex.Message}");
            GD.PrintErr($"Load error: {ex}");
        }
    }
    private void ShowDeleteDialog()
    {
        var simulations = GvDatabase.ListSimulations();

        if (simulations.Count == 0)
        {
            ShowMessage("No saved simulations found");
            return;
        }

        var dialog = new ConfirmationDialog();
        dialog.Title = "Delete Simulation";
        dialog.OkButtonText = "Delete";
        dialog.Size = new Vector2I(500, 400);

        var vbox = new VBoxContainer();

        vbox.AddChild(new Label { Text = "⚠️ Select a simulation to DELETE (cannot be undone):" });

        var list = new ItemList();
        list.CustomMinimumSize = new Vector2(0, 300);

        foreach (var sim in simulations)
        {
            string displayText = $"{sim.Name} | {sim.CreatedAt:yyyy-MM-dd HH:mm}";
            list.AddItem(displayText);
            list.SetItemMetadata(list.ItemCount - 1, sim.Id);
        }

        vbox.AddChild(list);
        dialog.AddChild(vbox);

        dialog.Confirmed += () =>
        {
            if (list.GetSelectedItems().Length > 0)
            {
                int selectedIndex = list.GetSelectedItems()[0];
                int simId = (int)list.GetItemMetadata(selectedIndex);

                if (GvDatabase.DeleteSimulation(simId))
                {
                    ShowMessage("Simulation deleted successfully!");
                }
                else
                {
                    ShowMessage("Error deleting simulation");
                }
            }
            dialog.QueueFree();
        };

        dialog.CloseRequested += dialog.QueueFree;

        AddChild(dialog);
        dialog.PopupCentered();
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

    private void OnAddObjectPressed()
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
