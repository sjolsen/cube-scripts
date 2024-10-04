using System;
using System.Collections.Generic;
using UnityEngine;

using Pixel = System.Tuple<int, int>;

public class Cubie
{
    public enum Color
    {
        RED,
        WHITE,
        GREEN,
        YELLOW,
        BLUE,
        ORANGE,
    };

    private static readonly Pixel[] _colorToPixel = new Pixel[] {
        /* [RED] = */ new Pixel(1, 0),
        /* [WHITE] = */ new Pixel(0, 1),
        /* [GREEN] = */ new Pixel(1, 1),
        /* [YELLOW] = */ new Pixel(2, 1),
        /* [BLUE] = */ new Pixel(3, 1),
        /* [ORANGE] = */ new Pixel(1, 2),
    };

    private static readonly Color?[,] _pixelToColor;

    static Cubie()
    {
        _pixelToColor = new Color?[4, 4];
        for (int color = 0; color < _colorToPixel.Length; ++color)
        {
            (var x, var y) = _colorToPixel[color];
            _pixelToColor[x, y] = (Color)color;
        }
    }

    private static Vector2 _colorToUv(Texture texture, Color color)
    {
        int u_unnorm = _colorToPixel[(int)color].Item1;
        int v_unnorm = texture.height - _colorToPixel[(int)color].Item2 - 1;
        float u_norm = ((float)u_unnorm + 0.5f) / (float)texture.width;
        float v_norm = ((float)v_unnorm + 0.5f) / (float)texture.height;
        return new Vector2(u_norm, v_norm);
    }

    private static Color? _uvToColor(Texture texture, Vector2 uv)
    {
        int u_unnorm = (int)Math.Floor(uv.x * texture.width);
        int v_unnorm = (int)Math.Floor(uv.y * texture.height);
        int pixel_x = u_unnorm;
        int pixel_y = texture.height - v_unnorm - 1;
        return _pixelToColor[pixel_x, pixel_y];
    }

    private static readonly Vector3[] _colorToOrientation = new Vector3[] {
        /* [RED] = */ Vector3.forward,
        /* [WHITE] = */ Vector3.right,
        /* [GREEN] = */ Vector3.down,
        /* [YELLOW] = */ Vector3.left,
        /* [BLUE] = */ Vector3.up,
        /* [ORANGE] = */ Vector3.back,
    };

    private static Color _orientationToColor(Vector3 orientation)
    {
        return (Color)Nearest.VectorIndex(orientation, _colorToOrientation);
    }

    private static void _remapColors(Mesh mesh, Texture texture, Quaternion rotation)
    {
        var color_map = new Dictionary<Color, Color>();
        for (int i = 0; i < _colorToOrientation.Length; ++i)
        {
            Vector3 old_orientation = _colorToOrientation[i];
            Vector3 new_orientation = rotation * old_orientation;
            color_map[(Color)i] = _orientationToColor(new_orientation);
        }
        var mesh_uv = mesh.uv.Clone() as Vector2[];
        for (int i = 0; i < mesh_uv.Length; ++i)
            if (_uvToColor(texture, mesh_uv[i]) is Color old_color)
                if (color_map.TryGetValue(old_color, out Color new_color))
                    mesh_uv[i] = _colorToUv(texture, new_color);
        mesh.uv = mesh_uv;
    }

    public static void RemapColors(GameObject obj)
    {
        _remapColors(
            obj.GetComponent<MeshFilter>().mesh,
            obj.GetComponent<MeshRenderer>().material.mainTexture,
            obj.transform.rotation);
    }
}
