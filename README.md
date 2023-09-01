# CyclicScrollView

 无限滚动列表是通过对象池回收视图范围之外的物体，重复利用UI元素以此减少UI的生成提高列表的性能。

​    **一.无限滚动列表有以下实现的要点。**

​      1.判断是否有超出显示范围的元素，如果有回收物品到对象池中。

​      2.从对象池中获得对应的Ui元素，根据当前视图的位置计算出Ui元素相对Content的位置，并把Ui元素放置到此位置。

​    无限滚动列表主要处理的就是这两方面的逻辑。现在我们按照流程具体分析一下个步骤的操作。



![img](https://article.biliimg.com/bfs/article/0117cbba35e51b0bce5f8c2f6a838e8a087e8ee7.png)

​    **二.简易的流程**

​    **1.**列表的长度取决于数据的大小，根据数据的长度来初始化Content的大小。根据数据初始化滚动列表。

​    2.随后就进入Update()函数中，执行更新元素的操作。想检查是否有不在显示范围内的ui元素并移出链表。

​    3.如果ui被全部删除，这是说明列表被快速滚动，这是上一帧的所有UI元素都不在显示范围之内，所以需要计算显示范围对应Content的位置并填充对应的Ui元素

​    4.如果ui没有被全部删除，此时可以在剩余的Ui元素基础上从两侧扩展添加Ui。

![img](https://article.biliimg.com/bfs/article/9db5cc0630a940eb85dcacbdfd28ad8243ae8820.png)流程图



![img](https://article.biliimg.com/bfs/article/0117cbba35e51b0bce5f8c2f6a838e8a087e8ee7.png)

  三.代码结构

​     1.抽象类

```csharp
//这是滚动列表的抽象类,通过继承此类并实现抽象类，来使用滚动列表的功能
//TCell是UI视图类，TData是Ui视图对应的数据类
public abstract partial class UICyclicScrollList<TCell, TData> : MonoBehaviour where TCell : MonoBehaviour
{
 //类内部调用此抽象方法设置对应的数据
 protected abstract void ResetCellData(TCell cell, TData data);
}
```

​    2.数据结构

```csharp
//这是一个数据bundle,用来表示多行/列的数据
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
```



```csharp
//当前视口内的元素我们用双向链表来存储,为什么用双向链表呢？因为滚动列表在平常使用中最常在头部和尾部插入数据，双向链表保存有头尾指针可以方便的头插和尾插。
private readonly LinkedList<ViewCellBundle<TCell>> viewCellBundles = new LinkedList<ViewCellBundle<TCell>>();
```

​    3.刷新逻辑（注意Head和Tail指的是当前UI链表的头部可尾部）

```csharp
public void UpdateDisplay()
{
  			//先移除,后添加
        RemoveHead();
        RemoveTail();
        if (viewCellBundles.Count == 0)
        {
            RefreshAllCellInViewRange();
            Debug.Log("全部刷新了！！！");
        }
        else
        {
            AddHead();
            AddTail();
        }
  			//删除越界的元素
  			//比如在ui运行的过程中数据链表Datas发生了变化,当Datas的数量变少，而此时显示范围内正好有这些未删除的越界的Ui元素，此函数可以清除这些无用的Ui元素放到对象队列中。
        RemoveItemOutOfListRange();
    }
```



![img](https://article.biliimg.com/bfs/article/0117cbba35e51b0bce5f8c2f6a838e8a087e8ee7.png)

四.视图范围

  1.Pivot设置到左上角,这样方便进行ui是否越界的逻辑的判断，方便对ui元素的位置计算。

![img](https://article.biliimg.com/bfs/article/7d0bf2d42810b61685b3d7d30ba25e2ec09e35ab.png)

2.每个UI元素的锚点也在左上角，这样的话ui元素的anchorPosition就会以Content的左上角为起点，这样用来计算当前物品的位置会很方便。

![img](https://article.biliimg.com/bfs/article/cc1736e37e9eb3e9de81bd93fb014ef7763ce72e.png)

3.显示区域

![img](https://article.biliimg.com/bfs/article/8502e24efac86c47109c86de4ad73b563c0088b0.png)

​    以垂直模式为例子，在判断范围时候只会判断ui元素的y轴方向的坐标是否在此范围之内。Top的坐标设置为Vector2（0,UI元素的高度+元素间隔），Bottom坐标为Vector2(0,-视图范围的高度)，通过计算Ui元素的相对于视口的坐标并与Top和Bottom坐标进行比对，进而确定范围。

五.使用示例

  通过继承并实现抽象类，实现ui元素与对应数据的赋值。

```csharp
public class ViewListTest : UICyclicScrollList<ViewCell, TestData>
{
    public List<TestData> dataTemplates;

    private List<TestData> dataList1;
    private List<TestData> dataList2;
    private void Start()
    {
        dataList1 = new List<TestData>();

        for (int i = 0; i < 1000; i++)
        {
            int j = i % dataTemplates.Count;
            dataList1.Add(dataTemplates[j]);
        }
        Initlize(dataList1);
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < 10; i++)
            {
                dataList1[i] = dataTemplates[0];
                //刷新单个元素，如果在范围内则刷新对应界面的UI元素
                ElementAtDataChange(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            dataList2 = new List<TestData>();
            for (int i = 0; i < 100; i++)
            {
                dataList2.Add(dataTemplates[0]);
            }
            //刷新一下界面
            Initlize(dataList2);
            //刷新视图
            //初始化函数中包含这两个函数
            //RefrashViewRangeData();
            //RecaculateContentSize();
        }
    }
    protected override void ResetCellData(ViewCell cell, TestData data)
    {
        if (cell == null)
            return;
        cell.gameObject.SetActive(true);
        cell.UpdateDisplay(data.iconSprite, data.name);
    }
}
```
