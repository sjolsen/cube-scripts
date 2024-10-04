using System;
using System.Collections.Generic;
using UnityEngine;

public class Nearest
{
    static public int VectorIndex(Vector3 target, IList<Vector3> candidates)
    {
        var angles = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            angles[i] = Vector3.Angle(target, candidates[i]);
        int min_index = 0;
        for (int i = 1; i < candidates.Count; ++i)
            if (angles[i] < angles[min_index])
                min_index = i;
        return min_index;
    }

    static public Vector3 Vector(Vector3 target, IList<Vector3> candidates)
    {
        return candidates[VectorIndex(target, candidates)];
    }

    static public int PlaneIndex(Vector3 target, IList<Plane> candidates)
    {
        var distances = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            distances[i] = Mathf.Abs(candidates[i].GetDistanceToPoint(target));
        int min_index = 0;
        for (int i = 1; i < candidates.Count; ++i)
            if (distances[i] < distances[min_index])
                min_index = i;
        return min_index;
    }

    static public Plane Plane(Vector3 target, IList<Plane> candidates)
    {
        return candidates[PlaneIndex(target, candidates)];
    }

    static public int[] PlaneSortIndex(Vector3 target, IList<Plane> candidates)
    {
        var distances = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            distances[i] = Mathf.Abs(candidates[i].GetDistanceToPoint(target));
        var indices = new int[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            indices[i] = i;
        Array.Sort(distances, indices);
        return indices;
    }

    static public Plane[] PlaneSort(Vector3 target, IList<Plane> candidates)
    {
        var indices = PlaneSortIndex(target, candidates);
        var result = new Plane[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            result[i] = candidates[indices[i]];
        return result;
    }

    static public int QuaternionIndex(Quaternion target, IList<Quaternion> candidates)
    {
        var angles = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; ++i)
            angles[i] = UnityEngine.Quaternion.Angle(target, candidates[i]);
        int min_index = 0;
        for (int i = 1; i < candidates.Count; ++i)
            if (angles[i] < angles[min_index])
                min_index = i;
        return min_index;
    }

    static public Quaternion Quaternion(Quaternion target, IList<Quaternion> candidates)
    {
        return candidates[QuaternionIndex(target, candidates)];
    }
}
