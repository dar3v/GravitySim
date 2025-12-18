using Godot;
using System;

public partial class MainUi : CanvasLayer
{
    [Export] private MenuButton _fileMenu;
    [Export] private ItemList _objectList;
    [Export] private Button _addObject;

    public override void _Ready()
    {
        // Get the popups
        var filePopup = _fileMenu.GetPopup();

        // Add items
        filePopup.AddItem("Save Simulation", 0);
        filePopup.AddSeparator();
        filePopup.AddItem("Load Simulation", 1);
        filePopup.AddItem("Delete Simulation", 2);

        filePopup.IndexPressed += OnFileMenuPressed;
        _addObject.Pressed += ShowAddObjectDialog;
    }

    private void OnFileMenuPressed(long index)
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
        // Parameters: Vector3 Position, float Mass, float Density, Vector3 InitialVelocity
        GD.Print("Add Object clicked - show dialog here");
    }
}
