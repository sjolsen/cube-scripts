using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public GameObject CenterPrefab;
    public GameObject EdgePrefab;
    public GameObject CornerPrefab;

    private GameObject _InstantiateCubie(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var obj = Instantiate(prefab, transform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        Cubie.RemapColors(obj);
        return obj;
    }

    void Awake()
    {
        /* Instantiate centers */
        _InstantiateCubie(CenterPrefab, Vector3.right, Quaternion.identity);
        _InstantiateCubie(CenterPrefab, Vector3.left, Quaternion.AngleAxis(180, Vector3.up));
        _InstantiateCubie(CenterPrefab, Vector3.up, Quaternion.AngleAxis(90, Vector3.forward));
        _InstantiateCubie(CenterPrefab, Vector3.down, Quaternion.AngleAxis(90, Vector3.back));
        _InstantiateCubie(CenterPrefab, Vector3.forward, Quaternion.AngleAxis(90, Vector3.down));
        _InstantiateCubie(CenterPrefab, Vector3.back, Quaternion.AngleAxis(90, Vector3.up));
        /* Instantiate edges */
        for (int xy = 0; xy < 4; ++xy)
        {
            var rotation = Quaternion.AngleAxis(xy * 90, Vector3.forward);
            var position = rotation * (Vector3.right + Vector3.up);
            _InstantiateCubie(EdgePrefab, position, rotation);
        }
        for (int xz = 0; xz < 4; ++xz)
        {
            var rotation = Quaternion.AngleAxis(xz * 90, Vector3.up);
            var position = rotation * (Vector3.right + Vector3.forward);
            _InstantiateCubie(EdgePrefab, position, rotation * Quaternion.AngleAxis(90, Vector3.right));
        }
        for (int yz = 0; yz < 4; ++yz)
        {
            var rotation = Quaternion.AngleAxis(yz * 90, Vector3.right);
            var position = rotation * (Vector3.up + Vector3.forward);
            _InstantiateCubie(EdgePrefab, position, rotation * Quaternion.AngleAxis(90, Vector3.down));
        }
        /* Instantiate corners */
        for (int ylayer = 0; ylayer < 2; ++ylayer)
        {
            var layer_rotation = Quaternion.AngleAxis(ylayer * 90, Vector3.right);
            for (int xz = 0; xz < 4; ++xz)
            {
                var rotation = Quaternion.AngleAxis(xz * 90, Vector3.up) * layer_rotation;
                var position = rotation * Vector3.one;
                _InstantiateCubie(CornerPrefab, position, rotation);
            }
        }
    }
}
