using UnityEngine;

public enum ElementType { None, Brush, River, Earth }

[CreateAssetMenu]
public class TerrashapeProfile : ScriptableObject
{
    public ElementType element;
    public float bonusAttackRange;
    public GameObject weaponVFX;

}