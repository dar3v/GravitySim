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
            case Key.F9:
                LoadTest();
                break;
            case Key.F6:
                PrintSimulations();
                break;
            case Key.F7:
                DeleteLastSimulation();
                break;
        }
    }

    public void DeleteLastSimulation()
    {
        var sims = GvDatabase.ListSimulations();
        if (sims.Count == 0)
        {
            GD.Print("No simulations to delete.");
            return;
        }

        int id = sims[0].Id; // newest (ordered DESC)
        bool ok = GvDatabase.DeleteSimulation(id);

        GD.Print(ok
            ? $"Deleted simulation {id}"
            : $"Failed to delete simulation {id}");
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

    private void LoadTest()
    {
        GD.Print("Loading test simulation...");

        // load most recent simulation
        int lastId = GetLastSimulationId();
        if (lastId < 0)
            return;

        GvDatabase.LoadSimulation(lastId, GravitySim, WorldRoot);
    }

    private int GetLastSimulationId()
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(
            $"Data Source={OS.GetUserDataDir()}/gravity_sim.db"
        );

        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(id) FROM simulations;";

        var result = cmd.ExecuteScalar();

        if (result == null || result is DBNull)
        {
            GD.PrintErr("No simulations found in database.");
            return -1;
        }

        return Convert.ToInt32(result);
    }
}
