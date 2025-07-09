using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Hero : StatsHolder
{
    private List<Card> cards = new List<Card>();

    public void AddCard(Card newCard)
    {
        cards.Add(newCard);
        newCard.Owner = this;
    }

    public void RemoveCard(Card cardToRemove)
    {
        cards.Remove(cardToRemove);
        cardToRemove.Owner = null;
    }
    
    public void Start(){
        currentHealth = initialHealth;
        currentMaxHealth = initialHealth;
        currentManaCost = initialManaCost;
        currentAttack = initialAttack;
        UpdateAllTextField();
        CardVisualHelper.SetGlow(gameObject, true, Color);
    }
}
