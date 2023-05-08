using UnityEngine;
using System.Collections.Generic;

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
    protected override void ResetCellData(ViewCell cell, TestData data, int dataIndex)
    {
        if (cell == null)
            return;
        cell.gameObject.SetActive(true);
        cell.UpdateDisplay(data.iconSprite, data.name);
    }
}