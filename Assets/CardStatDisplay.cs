using UnityEngine;
using TMPro;

public class CardStatDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshPro attackText;
    [SerializeField] public TextMeshPro healthText;

    [SerializeField] private Color statChangedColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.3f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;
    [SerializeField] private Color overhealColor = Color.green;

    private int attack;
    private int currentHealth;
    private int maxHealth;

    // Properties
    public int Attack
    {
        get => attack;
        set
        {
            attack = value;
            
        }
    }

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            UpdateHealthText();
        }
    }

    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            UpdateHealthText();
        }
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;

        healthText.text = currentHealth.ToString();

        if (currentHealth < maxHealth)
            healthText.color = damagedColor;
        else if (currentHealth > maxHealth)
            healthText.color = overhealColor;
        else
            healthText.color = normalColor;
    }

    // For testing in editor or from another script
    public void SetStats(int atk, int currentHp, int maxHp)
    {
        Attack = atk;
        MaxHealth = maxHp;
        CurrentHealth = currentHp;
    }
}
