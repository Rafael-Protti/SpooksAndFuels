using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla o UI de um ingrediente individual em uma receita de crafting.
/// Exibe um ícone e a quantidade necessária.
/// </summary>
public class RecipeIngredientUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    public void Setup(Sprite icon, int amount)
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (amountText != null) 
        {
            amountText.text = amount.ToString();
        }
    }
}
