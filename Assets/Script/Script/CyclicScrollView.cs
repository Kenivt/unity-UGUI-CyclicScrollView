using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Knivt.Tools.UI
{
    public enum UICyclicScrollDirection
    {
        Vertical,
        Horizontal
    }

    public class ViewCellBundle<TCell> : IPoolObject where TCell : MonoBehaviour
    {
        public int index;
        public Vector2 position;
        public TCell[] Cells { get; private set; }
        public int CellCapacity => Cells.Length;
        public ViewCellBundle(int gameObjectCapacity)
        {
            Cells = new TCell[gameObjectCapacity];
        }
        public void Clear()
        {
            index = -1;
            foreach (var cell in Cells)
            {
                if (cell != null)
                {
                    cell.gameObject.SetActive(false);
                }
            }
        }
    }

    public abstract partial class UICyclicScrollList<C, D> : MonoBehaviour where C : MonoBehaviour
    {
        public UICyclicScrollDirection viewDirection;
        public ICollection<D> Datas { get; private set; }

        [SerializeField] protected C _cellObject;
        [SerializeField] protected RectTransform content;

        [SerializeField] private RectTransform _viewRange;
        [SerializeField] private Vector2 _cellSpace;
        [SerializeField] private int _ItemCellCount;
        public Vector2 ContentPos => content.position;
        public Vector2 ContentSize => content.sizeDelta;
        public Vector2 CellSize => _cellRectTransform.sizeDelta;
        public Vector2 ItemSize => CellSize + _cellSpace;
        private RectTransform _cellRectTransform;

        private readonly Vector2 HorizontalContentAnchorMin = new Vector2(0, 0);
        private readonly Vector2 HorizontalContentAnchorMax = new Vector2(0, 1);
        private readonly Vector2 VerticalContentAnchorMin = new Vector2(0, 1);
        private readonly Vector2 VerticalContentAnchorMax = new Vector2(1, 1);
        private readonly LinkedList<ViewCellBundle<C>> viewCellBundles = new LinkedList<ViewCellBundle<C>>();

        public int ItemCount
        {
            get
            {
                int cellcount = Datas.Count;
                return cellcount % _ItemCellCount == 0 ? cellcount / _ItemCellCount : cellcount / _ItemCellCount + 1;
            }
        }
        /// <summary>
        /// Scroll的初始化和数据的长度发生变化都需要使用这个函数,这个函数只涉及到Content的大小的变化..
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="resetPos"></param>
        public virtual void Initlize(ICollection<D> datas, bool resetPos = false)
        {
            if (datas == null)
            {
                throw new System.Exception("Error,Datas is vaild!");
            }
            _cellRectTransform = _cellObject.GetComponent<RectTransform>();
            Datas = datas;
            RecalculateContentSize(resetPos);
            //清除头部和尾部
            UpdateDisplay();
            RefrashViewRangeData();
        }
        /// <summary>
        /// 刷新一下，数据的长度发生变化的时候使用这个函数
        /// </summary>
        public void Refrash(bool resetContentPos = false)
        {
            RecalculateContentSize(resetContentPos);
            content.anchoredPosition = resetContentPos ? Vector2.zero : content.anchoredPosition;
            UpdateDisplay();
            RefrashViewRangeData();
        }

        protected virtual void Update()
        {
            if (Datas == null)
            {
                return;
            }
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            RemoveHead();
            RemoveTail();
            if (viewCellBundles.Count == 0)
            {
                RefreshAllCellInViewRange();
            }
            else
            {
                AddHead();
                AddTail();
            }
            //清除越界,比如数据减少,此时就要清理在视野内的,在数据之外的UI
            RemoveItemOutOfListRange();
        }

        public void RefreshAllCellInViewRange()
        {
            int itemCount = ItemCount;
            Vector2 viewRangeSize = _viewRange.sizeDelta;
            Vector2 itemSize = ItemSize;
            Vector2 cellSize = CellSize;
            Vector2 cellSpace = _cellSpace;

            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                Vector2 topPos = -content.anchoredPosition;
                Vector2 bottomPos = new Vector2(topPos.x, topPos.y - viewRangeSize.y);
                int startIndex = GetIndex(topPos);
                int endIndex = GetIndex(bottomPos);
                for (int i = startIndex; i <= endIndex && i < itemCount; i++)
                {
                    Vector2 pos = new Vector2(content.anchoredPosition.x, -i * itemSize.y);
                    var bundle = GetViewBundle(i, pos, cellSize, cellSpace);
                    viewCellBundles.AddLast(bundle);
                }
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                Vector2 leftPos = -content.anchoredPosition;
                Vector2 rightPos = new Vector2(leftPos.x + viewRangeSize.x, leftPos.y);

                int startIndex = GetIndex(leftPos);
                int endIndex = GetIndex(rightPos);

                for (int i = startIndex; i <= endIndex && i < itemCount; i++)
                {
                    Vector2 pos = new Vector2(i * itemSize.x, content.anchoredPosition.y);
                    var bundle = GetViewBundle(i, pos, cellSize, cellSpace);
                    viewCellBundles.AddLast(bundle);
                }
            }
        }

        private void AddHead()
        {
            //以头部元素向外计算出新头部的位置,计算该位置是否在显示区域，如果在显示区域则生成对应项目
            ViewCellBundle<C> bundle = viewCellBundles.First.Value;

            Vector2 offset = default;
            if (viewDirection == UICyclicScrollDirection.Vertical)
                offset = new Vector2(0, ItemSize.y);
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
                offset = new Vector2(-ItemSize.x, 0);

            Vector2 newHeadBundlePos = bundle.position + offset;

            while (OnViewRange(newHeadBundlePos))
            {
                int caculatedIndex = GetIndex(newHeadBundlePos);
                int index = bundle.index - 1;

                if (index < 0) break;
                if (caculatedIndex != index)
                    Debug.LogError($"计算索引:{caculatedIndex},计数索引{index}计算出的索引和计数的索引值不相等...");

                bundle = GetViewBundle(index, newHeadBundlePos, CellSize, _cellSpace);
                viewCellBundles.AddFirst(bundle);

                newHeadBundlePos = bundle.position + offset;
            }
        }

        private void RemoveHead()
        {
            if (viewCellBundles.Count == 0)
                return;

            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                ViewCellBundle<C> bundle = viewCellBundles.First.Value;
                while (AboveViewRange(bundle.position))
                {
                    //进入对象池
                    ReleaseViewBundle(bundle);
                    viewCellBundles.RemoveFirst();

                    if (viewCellBundles.Count == 0) break;

                    bundle = viewCellBundles.First.Value;
                }
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                ViewCellBundle<C> bundle = viewCellBundles.First.Value;
                while (InViewRangeLeft(bundle.position))
                {
                    //进入对象池
                    ReleaseViewBundle(bundle);
                    viewCellBundles.RemoveFirst();

                    if (viewCellBundles.Count == 0) break;

                    bundle = viewCellBundles.First.Value;
                }
            }
        }

        private void AddTail()
        {
            //以尾部元素向外计算出新头部的位置,计算该位置是否在显示区域，如果在显示区域则生成对应项目
            ViewCellBundle<C> bundle = viewCellBundles.Last.Value;
            Vector2 offset = default;
            if (viewDirection == UICyclicScrollDirection.Vertical)
                offset = new Vector2(0, -ItemSize.y);
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
                offset = new Vector2(ItemSize.x, 0);

            Vector2 newTailBundlePos = bundle.position + offset;

            while (OnViewRange(newTailBundlePos))
            {
                int caculatedIndex = GetIndex(newTailBundlePos);
                int index = bundle.index + 1;

                if (index >= ItemCount) break;
                if (caculatedIndex != index)
                    Debug.LogError($"计算索引:{caculatedIndex},计数索引{index}计算出的索引和计数的索引值不相等...");

                bundle = GetViewBundle(index, newTailBundlePos, CellSize, _cellSpace);
                viewCellBundles.AddLast(bundle);

                newTailBundlePos = bundle.position + offset;
            }
        }

        private void RemoveTail()
        {
            if (viewCellBundles.Count == 0)
                return;

            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                ViewCellBundle<C> bundle = viewCellBundles.Last.Value;
                while (UnderViewRange(bundle.position))
                {
                    //进入对象池
                    ReleaseViewBundle(bundle);
                    viewCellBundles.RemoveLast();

                    if (viewCellBundles.Count == 0) break;

                    bundle = viewCellBundles.Last.Value;
                }
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                ViewCellBundle<C> bundle = viewCellBundles.Last.Value;
                while (InViewRangeRight(bundle.position))
                {
                    //进入对象池
                    ReleaseViewBundle(bundle);
                    viewCellBundles.RemoveLast();

                    if (viewCellBundles.Count == 0) break;

                    bundle = viewCellBundles.Last.Value;
                }
            }
        }

        private void RemoveItemOutOfListRange()
        {
            if (viewCellBundles.Count() == 0)
                return;
            var bundle = viewCellBundles.Last.Value;
            int lastItemIndex = ItemCount - 1;
            while (bundle.index > lastItemIndex && viewCellBundles.Count() > 0)
            {
                viewCellBundles.RemoveLast();
                ReleaseViewBundle(bundle);
            }
        }

        public virtual Vector2 CaculateRelativePostion(Vector2 curPosition)
        {
            Vector2 relativePosition = default;
            if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                relativePosition = new Vector2(curPosition.x + content.anchoredPosition.x, curPosition.y);
            }
            else if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                relativePosition = new Vector2(curPosition.x, curPosition.y + content.anchoredPosition.y);
            }
            return relativePosition;
        }

        public int GetIndex(Vector2 position)
        {
            int index = -1;
            if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                index = Mathf.RoundToInt(-position.y / ItemSize.y);
                return index;
            }
            else if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                index = Mathf.RoundToInt(position.x / ItemSize.x);
            }
            return index;
        }

        public bool AboveViewRange(Vector2 position)
        {
            Vector2 relativePos = CaculateRelativePostion(position);
            return relativePos.y > ItemSize.y;
        }

        public bool UnderViewRange(Vector2 position)
        {
            Vector2 relativePos = CaculateRelativePostion(position);
            return relativePos.y < -_viewRange.sizeDelta.y;
        }

        public bool InViewRangeLeft(Vector2 position)
        {
            Vector2 relativePos = CaculateRelativePostion(position);
            return relativePos.x < -ItemSize.x;
        }

        public bool InViewRangeRight(Vector2 position)
        {
            Vector2 relativePos = CaculateRelativePostion(position);
            return relativePos.x > _viewRange.sizeDelta.x;
        }

        public bool OnViewRange(Vector2 position)
        {
            if (viewDirection == UICyclicScrollDirection.Horizontal)
            {
                return !InViewRangeLeft(position) && !InViewRangeRight(position);
            }
            else if (viewDirection == UICyclicScrollDirection.Vertical)
            {
                return !AboveViewRange(position) && !UnderViewRange(position);
            }
            return false;
        }
    }
}