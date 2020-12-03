using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Wave
{
    public enum Mobs
    {
        Goblin, 
        Slime, 
        Bat
    }

    public Mobs mobs; 
    public int level; 
    public int quantity; 
}