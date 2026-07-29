// FlipUV.hlsl
void FlipUV_float(float2 UV, out float2 Out)
{
    // SpriteRenderer applies negative scale for flipping.
    // Detect negative X/Y scale from object-to-world matrix.
    float flipX = sign(UNITY_MATRIX_M[0][0]); // -1 if flipped
    float flipY = sign(UNITY_MATRIX_M[1][1]);

    Out.x = flipX < 0 ? 1.0 - UV.x : UV.x;
    Out.y = flipY < 0 ? 1.0 - UV.y : UV.y;
}