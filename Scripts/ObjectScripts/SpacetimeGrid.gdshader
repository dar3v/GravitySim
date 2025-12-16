shader_type spatial;
render_mode unshaded;

uniform int object_count;
uniform vec3 object_positions[16]; // Max 16 objects
uniform float object_masses[16];
uniform float curvature_scale;

float curvature(vec3 p) {
    float h = 0.0;
    for (int i = 0; i < object_count; i++) {
        float r = distance(p, object_positions[i]);
        float rs = object_masses[i] * curvature_scale;
        if (r >= rs)
            h += 2.0 * sqrt(rs * (r - rs));  // Schwarzschild curvature formula
            h += rs / (r + 0.1);
    }
    return h;
}

void vertex() {
    // Only apply curvature to the Y-position (height) of the grid
    VERTEX.y = curvature(VERTEX);
}
