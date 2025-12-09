using Godot;
using System.Collections.Generic;

public partial class SpacetimeGrid : MeshInstance3D
{
    [Export] public int GridResolution = 10;  // Resolution of the grid (number of cells in each axis)
    [Export] public float PlaneSize = 10f;    // Size of the grid (total width/height of the grid)
    [Export] public float CurvatureScale = 0.05f;  // How strongly the mass affects curvature

    private List<GvObject> objs;  // List of all the objects that affect the curvature
    private ShaderMaterial gridMaterial; // Reference to the ShaderMaterial

    public override void _Ready()
    {
        objs = Globals.GetAllGvObjects();
        // Ensure MaterialOverride is a ShaderMaterial and fetch the reference
        if (MaterialOverride is ShaderMaterial mat)
        {
            gridMaterial = mat;
            gridMaterial.SetShaderParameter("cell_size", PlaneSize / (GridResolution - 1));
            gridMaterial.SetShaderParameter("curvature_scale", CurvatureScale);
        }
        else
        {
            GD.PrintErr("MaterialOverride is not a ShaderMaterial!");
        }

        // Generate the grid mesh once at the start
        GenerateGridMesh();
    }

    public override void _Process(double delta)
    {
        // Ensure gridMaterial is not null before using it
        if (gridMaterial != null)
        {
            var cam = GetViewport().GetCamera3D();
            if (cam != null)
            {
                // Set the camera position to be used in the shader
                gridMaterial.SetShaderParameter("camera_pos", cam.GlobalPosition);
            }

            // Pass object data to the shader for curvature calculation
            int objCount = objs.Count;
            gridMaterial.SetShaderParameter("object_count", objCount);

            Vector3[] objectPositions = new Vector3[16];  // max 16 objects for this example
            float[] objectMasses = new float[16];

            for (int i = 0; i < objCount; i++)
            {
                objectPositions[i] = objs[i].GlobalPosition;
                objectMasses[i] = objs[i].Mass;
            }

            gridMaterial.SetShaderParameter("object_positions", objectPositions);
            gridMaterial.SetShaderParameter("object_masses", objectMasses);
        }
        else
        {
            GD.PrintErr("Shader material is not set!");
        }
    }

    private void GenerateGridMesh()
    {
        // Use SurfaceTool to generate the grid mesh once (static mesh)
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Lines);  // Start in line mode for a grid

        int N = GridResolution;
        float half = PlaneSize * 0.5f;
        float cell = PlaneSize / (N - 1);

        Vector3[,] verts = new Vector3[N, N];

        // Create the vertices for the grid (initially set to Y=0)
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                float px = -half + x * cell;
                float py = -half + y * cell;

                verts[x, y] = new Vector3(px, 0f, py);  // Initially set Z to 0
            }
        }

        // Add vertical lines (along Z axis)
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N - 1; y++)
            {
                st.AddVertex(verts[x, y]);
                st.AddVertex(verts[x, y + 1]);
            }
        }

        // Add horizontal lines (along X axis)
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N - 1; x++)
            {
                st.AddVertex(verts[x, y]);
                st.AddVertex(verts[x + 1, y]);
            }
        }

        // Commit the mesh and assign it
        Mesh = st.Commit();
    }
}
