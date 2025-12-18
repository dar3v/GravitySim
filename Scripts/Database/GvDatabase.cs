using Microsoft.Data.Sqlite;
using Godot;
using System;
using System.IO;
using System.Collections.Generic;

public static class GvDatabase
{
    /* =======================
     * Paths & Connection
     * ======================= */

    public static string DbPath =>
        Path.Combine(OS.GetUserDataDir(), "gravity_sim.db");

    public static string ConnectionString =>
        $"Data Source={DbPath}";

    /* =======================
     * Initialization
     * ======================= */

    public static void Initialize()
    {
        Directory.CreateDirectory(OS.GetUserDataDir());

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS simulations (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            g_constant REAL,
            created_at TEXT
        );

        CREATE TABLE IF NOT EXISTS meshes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            file_path TEXT NOT NULL,
            scale REAL DEFAULT 1.0,
            created_at TEXT
        );

        CREATE TABLE IF NOT EXISTS objects (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            simulation_id INTEGER,
            mesh_id INTEGER,
            name TEXT,
            mass REAL,
            density REAL,
            pos_x REAL,
            pos_y REAL,
            pos_z REAL,
            vel_x REAL,
            vel_y REAL,
            vel_z REAL
        );
        """;

        cmd.ExecuteNonQuery();
    }

    /* =======================
     * Save Simulation
     * ======================= */

    public static void SaveSimulation(
        string name,
        float g,
        IReadOnlyList<GvObject> objects
    )
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // --- Insert simulation ---
        using var simCmd = connection.CreateCommand();
        simCmd.CommandText = """
        INSERT INTO simulations (name, g_constant, created_at)
        VALUES ($name, $g, $time);
        """;

        simCmd.Parameters.AddWithValue("$name", name);
        simCmd.Parameters.AddWithValue("$g", g);
        simCmd.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("o"));
        simCmd.ExecuteNonQuery();

        // --- Get simulation ID ---
        long simId;
        using (var idCmd = connection.CreateCommand())
        {
            idCmd.CommandText = "SELECT last_insert_rowid();";
            simId = (long)idCmd.ExecuteScalar();
        }

        // --- Insert objects ---
        foreach (var obj in objects)
        {
            using var objCmd = connection.CreateCommand();
            objCmd.CommandText = """
            INSERT INTO objects (
                simulation_id,
                mesh_id,
                name,
                mass,
                density,
                pos_x, pos_y, pos_z,
                vel_x, vel_y, vel_z
            ) VALUES (
                $sid,
                $mesh,
                $name,
                $mass,
                $density,
                $px, $py, $pz,
                $vx, $vy, $vz
            );
            """;

            objCmd.Parameters.AddWithValue("$sid", simId);
            objCmd.Parameters.AddWithValue("$mesh", obj.MeshId);
            objCmd.Parameters.AddWithValue("$name", obj.Name.ToString());
            objCmd.Parameters.AddWithValue("$mass", obj.Mass);
            objCmd.Parameters.AddWithValue("$density", obj.Density);

            objCmd.Parameters.AddWithValue("$px", obj.GlobalPosition.X);
            objCmd.Parameters.AddWithValue("$py", obj.GlobalPosition.Y);
            objCmd.Parameters.AddWithValue("$pz", obj.GlobalPosition.Z);

            objCmd.Parameters.AddWithValue("$vx", obj.Velocity.X);
            objCmd.Parameters.AddWithValue("$vy", obj.Velocity.Y);
            objCmd.Parameters.AddWithValue("$vz", obj.Velocity.Z);

            objCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public static void LoadSimulation(
        int simulationId,
        GravitySim sim,
        Node3D parent
    )
    {
        ClearCurrentSimulation(parent);
        if (parent == null)
        {
            GD.PushError("LoadSimulation: parent is null");
            return;
        }

        PackedScene gvScene =
            GD.Load<PackedScene>("res://Scenes/Objects/GvObject.tscn");

        if (gvScene == null)
        {
            GD.PushError("LoadSimulation: Failed to load GvObject.tscn");
            return;
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
    SELECT
        name, mass, density,
        pos_x, pos_y, pos_z,
        vel_x, vel_y, vel_z
    FROM objects
    WHERE simulation_id = $sid;
    """;
        cmd.Parameters.AddWithValue("$sid", simulationId);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var obj = gvScene.Instantiate<GvObject>();

            obj.Name = reader.GetString(0);
            obj.Mass = reader.GetFloat(1);
            obj.Density = reader.GetFloat(2);

            // Set InitialVelocity BEFORE adding to tree
            obj.InitialVelocity = new Vector3(
                reader.GetFloat(6),
                reader.GetFloat(7),
                reader.GetFloat(8)
            );

            parent.AddChild(obj);

            obj.GlobalPosition = new Vector3(
                reader.GetFloat(3),
                reader.GetFloat(4),
                reader.GetFloat(5)
            );
        }

        GD.Print($"Loaded simulation {simulationId}");
    }

    public static List<SimulationInfo> ListSimulations()
    {
        var result = new List<SimulationInfo>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
        SELECT
            id,
            name,
            g_constant,
            created_at
        FROM simulations
        ORDER BY created_at DESC;
    """;

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new SimulationInfo
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                GConstant = reader.IsDBNull(2)
                    ? 0f
                    : (float)reader.GetDouble(2),
                CreatedAt = reader.IsDBNull(3)
                    ? DateTime.MinValue
                    : DateTime.Parse(reader.GetString(3))
            });
        }

        return result;
    }

    /* ts vibe coded ahh hell bruh 💔 */

    public static bool RenameSimulation(int simulationId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            GD.PrintErr("RenameSimulation failed: name is empty.");
            return false;
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
        UPDATE simulations
        SET name = $name
        WHERE id = $id;
    """;

        cmd.Parameters.AddWithValue("$name", newName.Trim());
        cmd.Parameters.AddWithValue("$id", simulationId);

        int rowsAffected = cmd.ExecuteNonQuery();

        // rowsAffected == 0 means the simulation ID does not exist
        return rowsAffected > 0;
    }

    public static bool DeleteSimulation(int simulationId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Delete objects belonging to the simulation
            using (var objCmd = connection.CreateCommand())
            {
                objCmd.CommandText = """
                DELETE FROM objects
                WHERE simulation_id = $sid;
            """;
                objCmd.Parameters.AddWithValue("$sid", simulationId);
                objCmd.ExecuteNonQuery();
            }

            // 2. Delete the simulation itself
            int rowsAffected;
            using (var simCmd = connection.CreateCommand())
            {
                simCmd.CommandText = """
                DELETE FROM simulations
                WHERE id = $sid;
            """;
                simCmd.Parameters.AddWithValue("$sid", simulationId);
                rowsAffected = simCmd.ExecuteNonQuery();
            }

            // If no simulation row was deleted, ID didn't exist
            if (rowsAffected == 0)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            GD.PrintErr($"DeleteSimulation failed: {e.Message}");
            return false;
        }
    }

    public static void ResetDatabase()
    {
        if (File.Exists(DbPath))
        {
            File.Delete(DbPath);
            GD.Print("Database deleted");
        }

        Initialize(); // Recreate fresh database
        GD.Print("Database reset complete");
    }

    public static void ClearCurrentSimulation(Node root)
    {
        if (root == null)
            return;

        var toDelete = new List<GvObject>();

        foreach (Node child in root.GetChildren())
        {
            if (child is GvObject gv)
                toDelete.Add(gv);
        }

        // Important: free after iteration
        foreach (var gv in toDelete)
            gv.QueueFree();

        Globals.ClearAll();
    }
}
