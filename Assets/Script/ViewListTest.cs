using UnityEngine;
using Knivt.Tools.UI;
using System.Collections.Generic;

public class ViewListTest : UICyclicScrollList<ViewCell, TestData>
{
    public List<TestData> dataTemplates;

    private List<TestData> dataList1;
    private List<TestData> dataList2;
    private TestData[] datas;

    private void Start()
    {
        dataList1 = new List<TestData>();
        datas = new TestData[100];
        for (int i = 0; i < datas.Length; i++)
        {
            int j = i % dataTemplates.Count;
            datas[i] = dataTemplates[j];
        }
        Initlize(datas);
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < 11111; i++)
            {
                dataList1[i] = dataTemplates[0];
                //刷新单个元素，如果在范围内则刷新对应界面的UI元素
                ElementAtDataChange(i);
            }
        }
        if (Input.GetKey(KeyCode.G))
        {
            for (int i = 0; i < 300; i++)
            {
                dataList1.Add(dataTemplates[0]);
                ElementAtDataChange(dataList1.Count - 1);
                RecalculateContentSize(false);
            }
        }
        if (Input.GetKey(KeyCode.F))
        {
            for (int i = 0; i < 300 && dataList1.Count > 300; i++)
            {
                dataList1.RemoveAt(dataList1.Count - 1);
                ElementAtDataChange(dataList1.Count - 1);
                RecalculateContentSize(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            dataList2 = new List<TestData>();
            for (int i = 0; i < 5; i++)
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
        cell.gameObject.SetActive(true);
        cell.UpdateDisplay(data.iconSprite, data.name);
    }
}