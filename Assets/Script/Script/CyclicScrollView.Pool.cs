using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public interface IPoolObject
{
    void Clear();
}
public abstract partial class UICyclicScrollList<TCell, TData> : MonoBehaviour where TCell : MonoBehaviour
{
    private readonly Queue<ViewCellBundle<TCell>> cellBundlePool = new Queue<ViewCellBundle<TCell>>();
    private readonly Vector2 cellPivot = new Vector2(0, 1);
    private readonly Vector2 cellAnchorMin = new Vector2(0, 1);
    private readonly Vector2 cellAnchorMax = new Vector2(0, 1);
    private void ReleaseViewBundle(ViewCellBundle<TCell> viewCellBundle)
    {
        viewCellBundle.Clear();
        cellBundlePool.Enqueue(viewCellBundle);
    }

    private ViewCellBundle<TCell> GetViewBundle(int itemIndex, Vector2 postion, Vector2 cellSize, Vector2 cellSpace)
    {
        ViewCellBundle<TCell> bundle;
        Vector2 cellOffset = default;
        if (viewDirection == UICyclicScrollDirection.Vertical)
        {
            cellOffset = new Vector2(cellSize.x + cellSpace.x, 0);
        }
        else if (viewDirection == UICyclicScrollDirection.Horizontal)
        {
            cellOffset = new Vector2(0, -(cellSize.y + cellSpace.y));
        }

        if (cellBundlePool.Count == 0)
        {
            bundle = new ViewCellBundle<TCell>(ItemCellCount);
            bundle.position = postion;
            bundle.index = itemIndex;
            int i = itemIndex * ItemCellCount;
            int length = itemIndex * ItemCellCount + bundle.Cells.Length;

            for (int j = 0; j < bundle.Cells.Length && i < length; j++, i++)
            {
                bundle.Cells[j] = Instantiate(_cellObject, _contentRectTransform);
                RectTransform rectTransform = bundle.Cells[j].GetComponent<RectTransform>();
                ResetRectTransform(rectTransform);
                rectTransform.anchoredPosition = postion + j * cellOffset;

                if (i < 0 || i > Datas.Count()) break;

                ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
            }
        }
        else
        {
            bundle = cellBundlePool.Dequeue();
            bundle.index = itemIndex;
            bundle.position = postion;
            int i = itemIndex * ItemCellCount;
            int celllength = itemIndex * ItemCellCount + bundle.Cells.Length;
            for (int j = 0; j < bundle.Cells.Length && i < celllength; j++, i++)
            {
                RectTransform rectTransform = bundle.Cells[j].GetComponent<RectTransform>();
                ResetRectTransform(rectTransform);
                rectTransform.anchoredPosition = postion + j * cellOffset;
                if (i < 0 || i >= Datas.Count())
                {
                    break;
                }
                ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
            }
        }
        return bundle;
    }
    private void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.pivot = cellPivot;
        rectTransform.anchorMin = cellAnchorMin;
        rectTransform.anchorMax = cellAnchorMax;
    }
    protected abstract void ResetCellData(TCell cell, TData data, int dataIndex);
    public void ClearPoolItem()
    {
        cellBundlePool.Clear();
    }
    public void ClearAllUiItem()
    {
        viewCellBundles.Clear();
        cellBundlePool.Clear();
    }
}