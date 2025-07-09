using TMPro;
using UnityEngine;

public class StatsHolder : ColorHolder
{
    [Header("UI References")]
    [SerializeField] public TextMeshPro manaTextField;
    [SerializeField] public TextMeshPro attackTextField;
    [SerializeField] public TextMeshPro healthTextField;

    [Header("Initial Stats")]
    [SerializeField] protected int initialManaCost;
    [SerializeField] protected int initialAttack;
    [SerializeField] protected int initialHealth;

    protected int currentManaCost;
    protected int currentAttack;
    protected int currentHealth;
    protected int currentMaxHealth;

    private readonly Color normalColor = Color.white;
    private readonly Color damagedColor = Color.red;
    private readonly Color buffColor = Color.green;

    protected bool isAlive = true;

    protected virtual void Start()
    {
        Color = Color.white;
        currentHealth = initialHealth;
        currentMaxHealth = initialHealth;
        currentManaCost = initialManaCost;
        currentAttack = initialAttack;
        UpdateAllTextField();
    }

    public bool IsAlive
    {
        get => isAlive;
        set => isAlive = value;
    }

    public int CurrentAttack
    {
        get => currentAttack;
        set
        {
            currentAttack = value;
            UpdateTextField(attackTextField, currentAttack, initialAttack);
        }
    }

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Max(0, value);
            if (currentHealth == 0)
            {
                isAlive = false;
                OnDeath(); // Hook for derived classes
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

    public int CurrentManaCost
    {
        get => currentManaCost;
        set
        {
            currentManaCost = value;
            UpdateTextField(manaTextField, currentManaCost, initialManaCost);
        }
    }

    private void UpdateTextField(TextMeshPro textField, int value, int referenceValue)
    {
        if (textField == null) return;

        textField.text = value.ToString();

        if (isAlive)
        {
            if (value < referenceValue)
                textField.color = damagedColor;
            else if (value > referenceValue)
                textField.color = buffColor;
            else
                textField.color = normalColor;
        }
        else
        {
            textField.color = Color;
        }
    }

    public void UpdateAllTextField()
    {
        UpdateTextField(healthTextField, currentHealth, currentMaxHealth);
        UpdateTextField(attackTextField, currentAttack, initialAttack);
        UpdateTextField(manaTextField, currentManaCost, initialManaCost);
    }

    // Optional virtual hook for death logic
    protected virtual void OnDeath()
    {
        // e.g., override in Hero or Card to null owner, trigger animation, etc.
    }
}
