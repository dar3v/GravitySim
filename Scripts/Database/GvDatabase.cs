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
            objCmd.Parameters.AddWithValue("$name", obj.Name);
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
}
