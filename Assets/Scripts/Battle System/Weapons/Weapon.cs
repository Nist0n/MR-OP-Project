using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public float Damage;

    public WeaponType Type;
}

public enum WeaponType
{
    Racket,
    Blaster,
    MagicOrb
}