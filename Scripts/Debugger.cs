using Godot;
using System;

public partial class Debugger : Node
{
    [Export] public GravitySim GravitySim;
    [Export] public Node3D WorldRoot;

    public override void _Input(InputEvent e)
    {
        if (e is not InputEventKey k || !k.Pressed)
            return;

        switch (k.Keycode)
        {
            case Key.F5:
                SaveTest();
                break;
            case Key.F6:
                PrintSimulations();
                break;
            case Key.P:
                Globals.TogglePaused();
                break;
        }
    }

    public void PrintSimulations()
    {
        var sims = GvDatabase.ListSimulations();

        GD.Print($"Found {sims.Count} simulations:");

        foreach (var sim in sims)
        {
            GD.Print(
                $"ID={sim.Id} | Name={sim.Name} | G={sim.GConstant} | Created={sim.CreatedAt}"
            );
        }
    }

    private void SaveTest()
    {
        GD.Print("Saving test simulation...");

        GvDatabase.SaveSimulation(
            "Debug Save",
            GravitySim.G,
            Globals.GetAllGvObjects()
        );
    }
}
