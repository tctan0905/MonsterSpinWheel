using UnityEngine;
using UnityEngine.UI;

public class SlotItemView : MonoBehaviour
{
    public int SlotIndex;
    public GameObject SlotItem;

    public Image ImgMonster;

    public Image ImgDark;

    public void SetDark(bool value)
    {
        ImgDark.gameObject.SetActive(value);
    }
}
