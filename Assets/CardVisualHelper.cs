using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public static class CardVisualHelper
{
    public static void SetGlow(GameObject obj, bool state, string colorName = null)
    {
        ColorHolder card = ColorHolder.GetColorHolderFromObj(obj);
        Color glowColor = card.Color;
        if(!ColorUtility.TryParseHtmlString(colorName, out glowColor)){
            glowColor = card.Color;
        }
        ApplyGlow(obj, state, glowColor);
    }

    public static void SetGlow(GameObject obj, bool state, Color color)
    {
        ColorHolder card = ColorHolder.GetColorHolderFromObj(obj);
        Color glowColor = state ? color : card.Color;
        ApplyGlow(obj, state, glowColor);
    }
    
    private static void ApplyGlow(GameObject obj, bool state, Color glowColor)
    {
        // Change main sprite
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = glowColor;
        }

        // Change all child sprite renderers
        var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = glowColor;
        }

        // Change TextMeshProUGUI and TextMeshPro
        var tmpTexts = obj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        foreach (var tmp in tmpTexts)
        {
            tmp.color = glowColor;
        }

        var tmpWorldTexts = obj.GetComponentsInChildren<TMPro.TextMeshPro>(true);
        foreach (var tmp in tmpWorldTexts)
        {
            tmp.color = glowColor;
        }

        // Change all 3D Renderers
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer is SpriteRenderer) continue;
            foreach (var material in renderer.materials)
            {
                material.color = glowColor;
            }
        }
        Debug.Log("SetGlow - applied: " + glowColor);
    }

    // Support common color names
    private static bool TryGetNamedColor(string name, out Color color)
    {
        switch (name)
        {
            case "red": color = Color.red; return true;
            case "green": color = Color.green; return true;
            case "blue": color = Color.blue; return true;
            case "yellow": color = Color.yellow; return true;
            case "cyan": color = Color.cyan; return true;
            case "magenta": color = Color.magenta; return true;
            case "black": color = Color.black; return true;
            case "gray":
            case "grey": color = Color.gray; return true;
            case "white": color = Color.white; return true;
            default: color = Color.clear; return false;
        }
    }

    public static void TriggerAnimationByNameMatch(GameObject obj, string match)
    {
        var animator = obj.GetComponentInChildren<Animator>();
        if (animator == null) return;

        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                (param.name.StartsWith(match) || param.name.EndsWith(match)))
            {
                animator.SetTrigger(param.name);
                break;
            }
        }
    }

    public static async Task WaitUntilAnimationFinished(GameObject obj, string match)
    {
        var animator = obj.GetComponentInChildren<Animator>();
        if (animator == null) return;
        bool animationReachedEnd = false;
        string clipName = null;

        // Step 1: Wait until animation starts
        while (true)
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0 && clipInfo[0].clip.name.Contains(match))
                break;

            await Task.Yield();
        }

        // Step 2: Wait until animation finishes first full cycle
        while (true)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            // detect when animation is about to end 
            if (clipInfo[0].clip.name.Contains(match) && stateInfo.normalizedTime >= 0.975f)
            {
                animationReachedEnd = true;
                clipName = clipInfo[0].clip.name;
                if(stateInfo.normalizedTime >= 0.995f){
                    break;
                }
            }
            // after reaching 0.95, animation might change before reaching 
            if (animationReachedEnd && clipName != clipInfo[0].clip.name)
            {
                break;
            }

            await Task.Yield();
        }
    }
}
