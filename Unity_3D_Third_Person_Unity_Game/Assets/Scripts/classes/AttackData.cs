using UnityEngine;

public class AttackData
{
    public int damage;
    public Vector3 point;
    public bool minor;
    public AttackData(int damage, Vector3 point, bool minor)
    {
        this.damage = damage;
        this.point = point;
        this.minor = minor;
    }
}
