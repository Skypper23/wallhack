using System.Numerics;

namespace wallhack_cs
{
    public static class Calculate
    {
        public static Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height)
        {
            float screenW = (matrix.m41 * pos.X) + (matrix.m42 * pos.Y) + (matrix.m43 * pos.Z) + matrix.m44;

            if (screenW > 0.001f)
            {
                float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;
                float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;

                float camX = width * 0.5f; // Multiplicação mais rápida que divisão
                float camY = height * 0.5f;

                return new Vector2(
                    camX + (camX * screenX / screenW),
                    camY - (camY * screenY / screenW)
                );
            }

            return new Vector2(-99, -99);
        }
    }
}