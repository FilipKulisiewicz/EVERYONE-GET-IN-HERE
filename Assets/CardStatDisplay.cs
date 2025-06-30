using UnityEngine;
using TMPro;

public class CardStatDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshPro manaTextField;
    [SerializeField] public TextMeshPro attackTextField;
    [SerializeField] public TextMeshPro healthTextField;

    private Color normalColor = Color.white;
    private Color damagedColor = Color.red;
    private Color buffColor = Color.green;

    [SerializeField] private int initialManaCost;
    [SerializeField] private int initialAttack;
    [SerializeField] private int initialHealth;
    private int currentManaCost;
    private int currentAttack;
    private int currentHealth;
    private int currentMaxHealth;
            
    public void Start(){
        currentHealth = initialHealth;
        currentMaxHealth = initialHealth;
        UpdateHealthText();
    }
    // Properties
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
            currentHealth = value;
            UpdateHealthText();
        }
    }

    public int CurrentMaxHealth
    {
        get => currentMaxHealth;
        set
        {
            currentMaxHealth = value;
            UpdateHealthText();
        }
    }

    private void UpdateHealthText()
    {
        if (healthTextField == null) return;

        healthTextField.text = currentHealth.ToString();

        if (currentHealth < currentMaxHealth)
            healthTextField.color = damagedColor;
        else if (currentHealth > currentMaxHealth)
            healthTextField.color = buffColor;
        else
            healthTextField.color = normalColor;
    }
}
