using Godot;
using Microsoft.Data.Sqlite;
using System.IO;

public static class GvMeshLibrary
{
    /* =======================
     * Paths
     * ======================= */

    private static string MeshDir =>
        Path.Combine(OS.GetUserDataDir(), "meshes");

    /* =======================
     * Mesh Registration
     * ======================= */

    public static int RegisterMesh(string sourceFile, string name)
    {
        if (!Directory.Exists(MeshDir))
            Directory.CreateDirectory(MeshDir);

        string fileName = Path.GetFileName(sourceFile);
        string targetPath = Path.Combine(MeshDir, fileName);

        File.Copy(sourceFile, targetPath, overwrite: true);

        using var conn = new SqliteConnection(GvDatabase.ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
        INSERT INTO meshes (name, file_path, created_at)
        VALUES ($name, $path, $time);
        """;

        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$path", targetPath);
        cmd.Parameters.AddWithValue("$time", System.DateTime.UtcNow.ToString("o"));
        cmd.ExecuteNonQuery();

        using var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)idCmd.ExecuteScalar();
    }

    /* =======================
     * Lookup
     * ======================= */

    public static string GetMeshPath(int meshId)
    {
        using var conn = new SqliteConnection(GvDatabase.ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
        SELECT file_path
        FROM meshes
        WHERE id = $id;
        """;

        cmd.Parameters.AddWithValue("$id", meshId);

        return cmd.ExecuteScalar() as string;
    }
}
