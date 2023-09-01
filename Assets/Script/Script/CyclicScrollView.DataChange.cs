using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Knivt.Tools.UI
{
    public abstract partial class UICyclicScrollList<C, D> : MonoBehaviour where C : MonoBehaviour
    {

        /// <summary>
        /// 刷新界面内对应index的Cell的显示信息
        /// </summary>
        public void ElementAtDataChange(int index)
        {
            if (index < 0 || index >= Datas.Count)
            {
                throw new System.Exception("错误,索引越界...");
            }
            if (viewCellBundles.Count == 0)
            {
                return;
            }

            int firstIndex = viewCellBundles.First.Value.index;
            int lastIndex = viewCellBundles.Last.Value.index;
            int targetItemIndex = index / _ItemCellCount;
            int targetIndex = index % _ItemCellCount;//计算出对应的索引

            if (targetItemIndex >= firstIndex && targetItemIndex <= lastIndex)
            {
                C cell = viewCellBundles.Single((a) => a.index == targetItemIndex).Cells[targetIndex];
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
            LinkedListNode<ViewCellBundle<C>> curNode = viewCellBundles.First;
            bool flag = false;
            int count = 0;

            foreach (var bundle in viewCellBundles)
            {
                if (flag == true)
                {
                    break;
                }
                count++;
                int startIndex = bundle.index * _ItemCellCount;
                int endIndex = startIndex + bundle.Cells.Length - 1;

                //防止越界...
                if (endIndex >= Datas.Count)
                {
                    flag = true;
                    endIndex = Datas.Count - 1;
                }
                int i = startIndex, j = 0;
                for (; i <= endIndex && j < bundle.Cells.Length; i++, j++)
                {
                    ResetCellData(bundle.Cells[j], Datas.ElementAt(i), i);
                }
                if (flag == true)
                {
                    while (j < bundle.Cells.Length)
                    {
                        try
                        {
                            bundle.Cells[j++].gameObject.SetActive(false);
                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            int remainCount = viewCellBundles.Count() - count;
            while (remainCount > 0)
            {
                remainCount--;
                ReleaseViewBundle(viewCellBundles.Last.Value);
                viewCellBundles.RemoveLast();
            }
        }
        /// <summary>
        /// 刷新Content的尺寸,当删除元素或者增加元素的时候请调用它
        /// </summary>
        public void RecalculateContentSize(bool resetContentPos)
        {
            int itemCount = ItemCount;
            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                content.anchorMin = VerticalContentAnchorMin;
                content.anchorMax = VerticalContentAnchorMax;
                content.sizeDelta = new Vector2(content.sizeDelta.x, itemCount * ItemSize.y - _cellSpace.y);
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                content.anchorMin = HorizontalContentAnchorMin;
                content.anchorMax = HorizontalContentAnchorMax;
                content.sizeDelta = new Vector2(itemCount * ItemSize.x - _cellSpace.x, content.sizeDelta.y);
            }
            if (resetContentPos)
            {
                content.anchoredPosition = Vector2.zero;
            }
        }
    }
}