using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Hero : ColorHolder
{
    // [SerializeField] public TextMeshPro manaTextField;
    // [SerializeField] public TextMeshPro attackTextField;
    [SerializeField] public TextMeshPro healthTextField;

    private Color normalColor = Color.white;
    private Color damagedColor = Color.red;
    private Color buffColor = Color.green;

    // [SerializeField] private int initialManaCost;
    // [SerializeField] private int initialAttack;
    [SerializeField] private int initialHealth;
    private int currentManaCost;
    private int currentAttack;
    private int currentHealth;
    private int currentMaxHealth;

    private bool isAlive = true;

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

    // Properties
    public bool IsAlive
    {
        get => isAlive;
        set
        {
            isAlive = value;
        }
    }

    public int CurrentAttack
    {
        get => currentAttack;
        set
        {
            currentAttack = value;
        }
    }

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            if (value < 0){
                value = 0;
            }
            currentHealth = value;
            if (currentHealth == 0){
                isAlive = false;
            }
            UpdateTextField(healthTextField, currentHealth, currentMaxHealth);
        }
    }

    public int CurrentMaxHealth
    {
        get => currentMaxHealth;
        set
        {
            currentMaxHealth = value;
            UpdateTextField(healthTextField, currentHealth, currentMaxHealth);
        }
    }
    
    public void Start(){
        currentHealth = initialHealth;
        currentMaxHealth = initialHealth;
        // currentManaCost = initialManaCost;
        // currentAttack = initialAttack;
        UpdateAllTextField();
        CardVisualHelper.SetGlow(gameObject, true, Color);
    }

    private void UpdateTextField(TextMeshPro textField, int value, int referenceValue)
    {
        if (textField == null) return;

        textField.text = value.ToString();

        if(isAlive){
            if (value < referenceValue)
                textField.color = damagedColor;
            else if (value > referenceValue)
                textField.color = buffColor;
            else
                textField.color = normalColor;
        }
        else{
            textField.color = Color;
        }
    }

    public void UpdateAllTextField(){
        // UpdateTextField(healthTextField, currentHealth, currentMaxHealth);
        // UpdateTextField(attackTextField, currentAttack, initialAttack);
        // UpdateTextField(manaTextField, currentManaCost, initialManaCost);
    }
}
