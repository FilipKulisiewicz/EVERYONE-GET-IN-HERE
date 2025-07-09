using UnityEngine;
using TMPro;

public class Card : StatsHolder
{
    private Hero owner = null;

    public void Start(){
        Color = Color.white;
        CurrentHealth = initialHealth;
        CurrentMaxHealth = initialHealth;
        CurrentManaCost = initialManaCost;
        CurrentAttack = initialAttack;
        UpdateAllTextField();
    }

    // Properties
    public Hero Owner
    {
        get => owner;
        set
        {
            owner = value;
        }
    }
}
