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
            await WhenNCompleted(tasks, tasks.Count - 1);

            ApplyAttackDamageWithRevenge(attackerObj, targetObj);

            var deathTasks = new List<Task>();
            var attackerStats = GetStatsHolderFromObj(attackerObj);
            var targetStats = GetStatsHolderFromObj(targetObj);
            if (!attackerStats.IsAlive)
            {
                deathTasks.Add(HandleDeath(attackerObj));
            }
            if (!targetStats.IsAlive)
            {
                deathTasks.Add(HandleDeath(targetObj));
            }
            // Wait for all deaths to finish (if any)
            if (deathTasks.Count > 0)
            {
                deathTasks.Add(Task.Delay(1000));
                await WhenNCompleted(deathTasks, deathTasks.Count);
            }
        }
    }

    private async Task HandleCardVsHero(GameObject attackerObj, GameObject targetHeroObj, Card attackerCard, Hero targetHero)
    {
        if (attackerCard.Owner == null && attackerCard.IsAlive && targetHero.IsAlive)
        {     
            targetHero.AddCard(attackerCard);
            attackerCard.Color = targetHero.Color;
            CardVisualHelper.SetGlow(attackerObj, false);
            Debug.Log("added card");
        }
        if (attackerCard.Owner != null && attackerCard.Owner != targetHero && attackerCard.IsAlive && targetHero.IsAlive)
        {
            const string triggerMatch = "AttackTrigger";
            const string animationMatch = "Attack";

            CardVisualHelper.TriggerAnimationByNameMatch(attackerObj, triggerMatch);

            var tasks = new List<Task>
            {
                CardVisualHelper.WaitUntilAnimationFinished(attackerObj, animationMatch),
                Task.Delay(1000)
            };
            await WhenNCompleted(tasks, tasks.Count - 1);

            ApplyAttackDamage(attackerObj, targetHeroObj);

            var deathTasks = new List<Task>();
            var attackerStats = GetStatsHolderFromObj(attackerObj);
            var targetStats = GetStatsHolderFromObj(targetHeroObj);
            if (!attackerStats.IsAlive)
            {
                deathTasks.Add(HandleDeath(attackerObj));
            }
            if (!targetStats.IsAlive)
            {
                deathTasks.Add(HandleDeath(targetHeroObj));
            }
            // Wait for all deaths to finish (if any)
            if (deathTasks.Count > 0)
            {
                deathTasks.Add(Task.Delay(1000));
                await WhenNCompleted(deathTasks, deathTasks.Count);
            }
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

    private async void ApplyAttackDamageWithRevenge(GameObject attackerObj, GameObject targetObj)
    {
        ApplyAttackDamage(attackerObj, targetObj);
        ApplyAttackDamage(targetObj, attackerObj);
    }

    private async void ApplyAttackDamage(GameObject attackerObj, GameObject targetObj)
    {
        var attacker = GetStatsHolderFromObj(attackerObj);
        var target = GetStatsHolderFromObj(targetObj);
        target.CurrentHealth -= attacker.CurrentAttack;
    }

    private async Task HandleDeath(GameObject cardObj)
    {
        const string triggerMatch = "DeathTrigger";
        const string animationMatch = "Death";
        Color deathColor = Color.gray;

        Animator animator = cardObj.GetComponentInChildren<Animator>();
        StatsHolder statsHolder = GetStatsHolderFromObj(cardObj); // Use central method for consistency

        if (animator == null)
        {
            Debug.LogWarning($"Missing Animator component on '{cardObj.name}' during death handling.");
            return;
        }

        CardVisualHelper.TriggerAnimationByNameMatch(cardObj, triggerMatch);
        // await Task.Delay(500); // Let state transition into death animation
        await CardVisualHelper.WaitUntilAnimationFinished(cardObj, animationMatch);

        animator.enabled = false;           // Freeze the pose
        statsHolder.Color = deathColor;            // Store gray as original for un-glow
        CardVisualHelper.SetGlow(cardObj, true, deathColor); // Visually mark as dead
        cardObj.tag = "DeadCard";
    }

    public static Card GetCardFromObj(GameObject obj) //TODO change to Get_StatsHolder
    {
        if (obj == null) return null;
        return obj.GetComponent<Card>();
    }

    private Hero GetHeroFromObj(GameObject obj)
    {
        if (obj == null) return null;
        return obj.GetComponent<Hero>();
    }

    public static StatsHolder GetStatsHolderFromObj(GameObject obj) //TODO change to Get_StatsHolder
    {
        if (obj == null) return null;
        return obj.GetComponent<StatsHolder>();
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

