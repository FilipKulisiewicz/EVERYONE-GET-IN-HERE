using UnityEngine;
using TMPro;

public class CardStats : MonoBehaviour
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

    private bool isAlive = true;
    public Color color = Color.white;

    public void Start(){
        currentHealth = initialHealth;
        currentMaxHealth = initialHealth;
        currentManaCost = initialManaCost;
        currentAttack = initialAttack;
        UpdateAllTextField();
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

    public Color Color
    {
        get => color;
        set
        {
            color = value;
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
                color = Color.gray;
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

    private void UpdateTextField(TextMeshPro textField, int value, int referenceValue)
    {
        if (textField == null) return;

        textField.text = value.ToString();

        if (value < referenceValue)
            textField.color = damagedColor;
        else if (value > referenceValue)
            textField.color = buffColor;
        else
            textField.color = normalColor;
    }


    private void UpdateAllTextField(){
        UpdateTextField(healthTextField, currentHealth, currentMaxHealth);
        UpdateTextField(attackTextField, currentAttack, initialAttack);
        UpdateTextField(manaTextField, currentManaCost, initialManaCost);
    }
}
