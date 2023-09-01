using System.Linq;
using UnityEngine;
using System.Collections.Generic;
namespace Knivt.Tools.UI
{
    public interface IPoolObject
    {
        void Clear();
    }
    public abstract partial class UICyclicScrollList<C, D> : MonoBehaviour where C : MonoBehaviour
    {
        private readonly Queue<ViewCellBundle<C>> _cellBundlePool = new Queue<ViewCellBundle<C>>();
        private readonly Vector2 _cellPivot = new Vector2(0, 1);
        private readonly Vector2 _cellAnchorMin = new Vector2(0, 1);
        private readonly Vector2 _cellAnchorMax = new Vector2(0, 1);
        private void ReleaseViewBundle(ViewCellBundle<C> viewCellBundle)
        {
            viewCellBundle.Clear();
            _cellBundlePool.Enqueue(viewCellBundle);
        }

        private ViewCellBundle<C> GetViewBundle(int itemIndex, Vector2 postion, Vector2 cellSize, Vector2 cellSpace)
        {
            ViewCellBundle<C> bundle;
            Vector2 cellOffset = default;
            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                cellOffset = new Vector2(cellSize.x + cellSpace.x, 0);
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                cellOffset = new Vector2(0, -(cellSize.y + cellSpace.y));
            }

            if (_cellBundlePool.Count == 0)
            {
                bundle = new ViewCellBundle<C>(_ItemCellCount);
                bundle.position = postion;
                bundle.index = itemIndex;
                int i = itemIndex * _ItemCellCount;
                int length = itemIndex * _ItemCellCount + bundle.Cells.Length;

                for (int j = 0; j < bundle.Cells.Length && i < length; j++, i++)
                {
                    bundle.Cells[j] = InstantiateCell();
                    bundle.Cells[j].gameObject.SetActive(false);
                    RectTransform rectTransform = bundle.Cells[j].GetComponent<RectTransform>();
                    ResetRectTransform(rectTransform);
                    rectTransform.anchoredPosition = postion + j * cellOffset;

                    if (i < 0 || i >= Datas.Count)
                    {
                        continue;
                    }
                    ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
                }
            }
            else
            {
                bundle = _cellBundlePool.Dequeue();
                bundle.position = postion;
                bundle.index = itemIndex;
                int i = itemIndex * _ItemCellCount;
                int celllength = itemIndex * _ItemCellCount + bundle.Cells.Length;
                int j = 0;
                for (; j < bundle.Cells.Length && i < celllength; j++, i++)
                {
                    RectTransform rectTransform = bundle.Cells[j].GetComponent<RectTransform>();
                    ResetRectTransform(rectTransform);
                    rectTransform.anchoredPosition = postion + j * cellOffset;
                    if (i < 0 || i >= Datas.Count)
                    {
                        continue;
                    }
                    ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
                }
            }
            return bundle;
        }

        private void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.pivot = _cellPivot;
            rectTransform.anchorMin = _cellAnchorMin;
            rectTransform.anchorMax = _cellAnchorMax;
        }
        /// <summary>
        /// 调用此方法刷新UI
        /// </summary>
        /// <param name="cell">对应数据的UI</param>
        /// <param name="data">数据</param>
        protected abstract void ResetCellData(C cell, D data, int dataIndex);

        /// <summary>
        /// 生成对应的Cell
        /// </summary>
        /// <returns></returns>
        protected virtual C InstantiateCell()
        {
            return Instantiate(_cellObject, content);
        }
        /// <summary>
        /// 清除所有对象池中的物体
        /// </summary>
        public void ClearPoolItem()
        {
            _cellBundlePool.Clear();
        }
        public void ClearAllUiItem()
        {
            viewCellBundles.Clear();
            _cellBundlePool.Clear();
        }
    }
}