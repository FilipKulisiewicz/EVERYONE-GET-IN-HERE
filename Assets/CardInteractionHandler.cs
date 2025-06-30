using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CardInteractionHandler : MonoBehaviour
{
    public HandHandler handHandler;

    void Start()
    {
        handHandler.TwoCardInteraction += HandleTwoCardInteraction;
    }

    private async void HandleTwoCardInteraction(GameObject attackerObj, GameObject targetObj)
    {
        var attackerCard = GetCardFromObj(attackerObj);
        var targetCard = GetCardFromObj(targetObj);

        if (attackerCard != null && targetCard != null && attackerCard != targetCard && attackerCard.IsAlive && targetCard.IsAlive)
        {
            const string triggerMatch = "AttackTrigger";
            const string animationMatch = "Attack";

            CardVisualHelper.TriggerAnimationByNameMatch(attackerObj, triggerMatch);
            CardVisualHelper.TriggerAnimationByNameMatch(targetObj, triggerMatch);

            await Task.Delay(1); // allow state change
            await CardVisualHelper.WaitUntilAnimationNotPlaying(attackerObj, animationMatch);
            await CardVisualHelper.WaitUntilAnimationNotPlaying(targetObj, animationMatch);

            ApplyAttackDamage(attackerObj, targetObj);
        }
    }

    private void ApplyAttackDamage(GameObject attackerObj, GameObject targetObj)
    {
        var attacker = GetCardFromObj(attackerObj);
        var target = GetCardFromObj(targetObj);
        attacker.CurrentHealth = attacker.CurrentHealth - target.CurrentAttack;
        target.CurrentHealth = target.CurrentHealth - attacker.CurrentAttack;
        if(attacker.IsAlive == false){
            CardVisualHelper.SetGlow(attackerObj, true, "gray");
        }
        if(target.IsAlive == false){
            CardVisualHelper.SetGlow(targetObj, true, "gray");
        }
        
        Debug.Log(attacker.name + " attacked " + target.name + " for " + attacker.CurrentAttack + " damage. Remaining health: " + target.CurrentHealth);
    }

    public static CardStats GetCardFromObj(GameObject obj)
    {
        return obj.GetComponent<CardStats>();
    }
}

