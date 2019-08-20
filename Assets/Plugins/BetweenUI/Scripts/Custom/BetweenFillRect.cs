using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BetweenFillRect : BetweenBase
{
    public Image image;

    public Image Image
    {
        get
        {
            if (this.image == null)
            {
                this.image = this.GetComponent<Image>();
            }

            return this.image;
        }
    }

    protected override void OnUpdate(float timeFactor)
    {
        this.image.fillAmount = timeFactor;
    }
}
