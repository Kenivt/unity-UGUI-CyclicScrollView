using System.Linq;
using UnityEngine;

public abstract partial class UICyclicScrollList<TCell, TData> : MonoBehaviour where TCell : MonoBehaviour
{
    /// <summary>
    /// 刷新界面内对应index的Cell的显示信息
    /// </summary>
    public void ElementAtDataChange(int index)
    {
        if (index < 0 || index >= Datas.Count())
        {
            throw new System.Exception("错误,索引越界...");
        }
        int firstIndex = viewCellBundles.First.Value.index;
        int lastIndex = viewCellBundles.Last.Value.index;
        int targetItemIndex = index / ItemCellCount;
        int targetIndex = index % ItemCellCount;//计算出对应的索引

        if (targetItemIndex >= firstIndex && targetItemIndex <= lastIndex)
        {
            TCell cell = viewCellBundles.Single((a) => a.index == targetItemIndex).Cells[targetIndex];
            ResetCellData(cell, Datas.ElementAt(index), index);
        }
    }
    /// <summary>
    /// 刷新界面内所有cell的显示信息
    /// </summary>
    public void RefrashViewRangeData()
    {
        if (viewCellBundles.Count() == 0)
        {
            return;
        }
        foreach (var bundle in viewCellBundles)
        {
            int startIndex = bundle.index * ItemCellCount;
            int endIndex = startIndex + bundle.Cells.Length - 1;
            for (int i = startIndex, j = 0; i <= endIndex && j < bundle.Cells.Length; i++, j++)
            {
                ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
            }
        }
    }
    public void RecaculateContentSize()
    {
        int itemCount = ItemCount;
        if (viewDirection == UICyclicScrollDirection.Vertical)
        {
            _contentRectTransform.anchorMin = VerticalContentAnchorMin;
            _contentRectTransform.anchorMax = VerticalContentAnchorMax;
            _contentRectTransform.sizeDelta = new Vector2(_contentRectTransform.sizeDelta.x, itemCount * ItemSize.y - CellSpace.y);
        }
        else if (viewDirection == UICyclicScrollDirection.Horizontal)
        {
            _contentRectTransform.anchorMin = HorizontalContentAnchorMin;
            _contentRectTransform.anchorMax = HorizontalContentAnchorMax;
            _contentRectTransform.sizeDelta = new Vector2(itemCount * ItemSize.x - CellSpace.x, _contentRectTransform.sizeDelta.y);
        }
    }
}
