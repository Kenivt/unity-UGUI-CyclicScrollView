using UnityEngine;
using UnityEngine.UI;
//数据结构体
[System.Serializable]
public struct TestData
{
    public Sprite iconSprite;
    public string name;
}
//对应的视图UI
public class ViewCell : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public void UpdateDisplay(Sprite sprite, string text)
    {
        icon.sprite = sprite;
        nameText.text = text;
    }
}