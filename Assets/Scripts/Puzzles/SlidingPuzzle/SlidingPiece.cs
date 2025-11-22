using UnityEngine;

public class SlidingPiece
{
    private int id;
    private Transform transform;

    public int Id => id;
    public Transform Transform => transform;

    public SlidingPiece(int id, Transform transform)
    {
        this.id = id;
        this.transform = transform;
    }
}
