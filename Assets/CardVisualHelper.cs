using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public static class CardVisualHelper
{
    public static void SetGlow(GameObject obj, bool state, string colorName = "yellow")
    {
        Color defColor = CardInteractionHandler.GetCardFromObj(obj).Color;
        Color glowColor = Color.white;

        if (state)
        {
            if (!ColorUtility.TryParseHtmlString(colorName, out glowColor))
            {
                if (!TryGetNamedColor(colorName.ToLower(), out glowColor))
                {
                    Debug.LogWarning($"Unknown color name '{colorName}', defaulting to yellow.");
                    glowColor = defColor;
                }
            }
        }

        // Change color for main SpriteRenderer
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = state ? glowColor : defColor;
        }

        // Change color for all SpriteRenderers in children
        var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = state ? glowColor : defColor;
        }

        // Change color for all TextMeshPro components in children
        var tmpTexts = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmpTexts)
        {
            tmp.color = state ? glowColor : defColor;
        }

        var tmpWorldTexts = obj.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var tmp in tmpWorldTexts)
        {
            tmp.color = state ? glowColor : defColor;
        }
       
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer is SpriteRenderer)
            {
                continue;
            }
            var materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].color = state ? glowColor : defColor;
            }
        }
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

    public static async Task WaitUntilAnimationNotPlaying(GameObject obj, string match)
    {
        var animator = obj.GetComponentInChildren<Animator>();
        if (animator == null) return;

        while (true)
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            string currentClipName = (clipInfo.Length > 0) ? clipInfo[0].clip.name : "";

            if (!currentClipName.Contains(match))
                break;

            await Task.Yield();
        }
    }
}
