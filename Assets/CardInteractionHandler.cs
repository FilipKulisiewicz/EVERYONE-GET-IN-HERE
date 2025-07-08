using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CardInteractionHandler : MonoBehaviour
{
    public HandHandler handHandler;
    Card attackerCard = null, targetCard = null;
    Hero attackerHero = null, targetHero = null;

    void Start()
    {
        handHandler.TwoObjectInteraction += HandleTwoObjectInteraction;
    }

    private async void HandleTwoObjectInteraction(GameObject attackerObj, GameObject targetObj)
    {
        TryResolveObject(attackerObj, out var attackerCard, out var attackerHero);
        TryResolveObject(targetObj, out var targetCard, out var targetHero);

        if (attackerCard != null && targetCard != null)
        {
            await HandleCardVsCard(attackerObj, targetObj, attackerCard, targetCard);
            return;
        }

        if (attackerCard != null && targetHero != null)
        {
            await HandleCardVsHero(attackerObj, targetObj, attackerCard, targetHero);
            return;
        }

        if (attackerHero != null && targetCard != null)
        {
            await HandleHeroVsCard(attackerObj, targetObj);
            return;
        }

        if (attackerHero != null && targetHero != null)
        {
            await HandleHeroVsHero(attackerObj, targetObj);
            return;
        }

        Debug.LogWarning("Unsupported interaction type or null object.");
    }

    private void TryResolveObject(GameObject obj, out Card card, out Hero hero)
    {
        card = null;
        hero = null;

        if (obj.CompareTag("Hero"))
            hero = GetHeroFromObj(obj);
        else if (obj.CompareTag("Card"))
            card = GetCardFromObj(obj);
    }

    private async Task HandleCardVsCard(GameObject attackerObj, GameObject targetObj, Card attackerCard, Card targetCard)
    {
        if (attackerCard != targetCard && attackerCard.IsAlive && targetCard.IsAlive)
        {
            Debug.LogWarning("HandleCardVsCard");
            const string triggerMatch = "AttackTrigger";
            const string animationMatch = "Attack";

            CardVisualHelper.TriggerAnimationByNameMatch(attackerObj, triggerMatch);
            CardVisualHelper.TriggerAnimationByNameMatch(targetObj, triggerMatch);

            var tasks = new List<Task>
            {
                CardVisualHelper.WaitUntilAnimationFinished(attackerObj, animationMatch),
                CardVisualHelper.WaitUntilAnimationFinished(targetObj, animationMatch),
                Task.Delay(1000)
            };
            await WhenNCompleted(tasks, 2);

            ApplyAttackDamage(attackerObj, targetObj);

            var deathTasks = new List<Task>();
            attackerCard = GetCardFromObj(attackerObj);
            targetCard = GetCardFromObj(targetObj);
            if (!attackerCard.IsAlive)
            {
                deathTasks.Add(HandleCardDeath(attackerObj));
            }
            if (!targetCard.IsAlive)
            {
                deathTasks.Add(HandleCardDeath(targetObj));
            }
            // Wait for all deaths to finish (if any)
            if (deathTasks.Count > 0)
            {
                deathTasks.Add(Task.Delay(1000));
                await WhenNCompleted(deathTasks, deathTasks.Count);
            }
        }
    }

    private async Task HandleCardVsHero(GameObject card, GameObject hero, Card attackerCard, Hero targetHero)
    {
        if (attackerCard.Owner == null && attackerCard.IsAlive && targetHero.IsAlive)
        {     
            targetHero.AddCard(attackerCard);
            attackerCard.Color = targetHero.Color;
            CardVisualHelper.SetGlow(card, false);
            Debug.Log("added card");
        }
        if (attackerCard.Owner != null && attackerCard.Owner != targetHero && attackerCard.IsAlive && targetHero.IsAlive)
        {
            //attack enemy hero
        }
    }

    private async Task HandleHeroVsCard(GameObject hero, GameObject card)
    {
        Debug.Log($"Hero {hero.name} attacks {card.name}!");
        // Rare case? Defensive ability? Handle accordingly
    }

    private async Task HandleHeroVsHero(GameObject attacker, GameObject target)
    {
        Debug.Log($"Hero {attacker.name} attacks Hero {target.name}!");
        // Add any special logic: damage, dialog, animation, etc.
    }

    private async void ApplyAttackDamage(GameObject attackerObj, GameObject targetObj)
    {
        var attacker = GetCardFromObj(attackerObj);
        var target = GetCardFromObj(targetObj);

        attacker.CurrentHealth -= target.CurrentAttack;
        target.CurrentHealth -= attacker.CurrentAttack;

        Debug.Log(attacker.name + " attacked " + target.name + " for " + attacker.CurrentAttack + " damage. Remaining health: " + target.CurrentHealth);
    }

    private async Task HandleCardDeath(GameObject cardObj)
    {
        const string triggerMatch = "DeathTrigger";
        const string animationMatch = "Death";
        Color deathColor = Color.gray;

        Animator animator = cardObj.GetComponentInChildren<Animator>();
        Card card = GetCardFromObj(cardObj); // Use central method for consistency

        if (animator == null || card == null)
        {
            Debug.LogWarning($"Missing Animator or Card component on '{cardObj.name}' during death handling.");
            return;
        }

        CardVisualHelper.TriggerAnimationByNameMatch(cardObj, triggerMatch);
        // await Task.Delay(500); // Let state transition into death animation
        await CardVisualHelper.WaitUntilAnimationFinished(cardObj, animationMatch);

        animator.enabled = false;           // Freeze the pose
        card.Color = deathColor;            // Store gray as original for un-glow
        CardVisualHelper.SetGlow(cardObj, true, deathColor); // Visually mark as dead
        cardObj.tag = "DeadCard";
    }

    public static Card GetCardFromObj(GameObject obj)
    {
        if (obj == null) return null;
        return obj.GetComponent<Card>();
    }

    private Hero GetHeroFromObj(GameObject obj)
    {
        if (obj == null) return null;
        return obj.GetComponent<Hero>();
    }

    public static async Task<List<Task>> WhenNCompleted(List<Task> tasks, int n)
    {
        var completed = new List<Task>();
        var remaining = new List<Task>(tasks);

        while (completed.Count < n && remaining.Count > 0)
        {
            var finished = await Task.WhenAny(remaining);
            completed.Add(finished);
            remaining.Remove(finished);
        }

        return completed;
    }
}

