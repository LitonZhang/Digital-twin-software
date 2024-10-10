using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaHelper;
using System;


public class OPCUAhelper : MonoBehaviour
{
    public GameObject worker1;
    public GameObject worker2;
    public GameObject AGV1;
    private string[] MonitorNodeTags = null;
    private float[] Record = null;
    private void Subscription(string[] MonitorNodeTags)
    {
        int ydim = MonitorNodeTags.GetLength(0);
        
        try
        {
            //初始化记录表
            for (int i=0; i< ydim; i++)
            {
                var value = m_OpcUaClient.ReadNode<float>(MonitorNodeTags[i]);
                
                if (Record[i] != value)
                {
                    //Debug.Log(value);
                    Record[i] = value;

                    if (i >= 7 && i <=8)
                    {                        
                        Vector3 pos = worker1.transform.position;
                        pos.x = -Record[6] + 2.1f;
                        pos.z = -Record[7]-2;
                        worker1.transform.position = pos;
                    }
                    if (i >= 10 && i <= 11)
                    {
                        Vector3 pos = AGV1.transform.position;
                        pos.x = -Record[9] + 2.1f;
                        pos.z = -Record[10]-2;
                        AGV1.transform.position = pos;
                    }
                    if (i >= 13 && i <= 14)
                    {
                        Vector3 pos = worker2.transform.position;
                        pos.x = Record[13] + 2.5f;
                        pos.z = -Record[14]+2.5f;
                        worker2.transform.position = pos;
                    }

                };             
            }
        }
        catch (Exception)
        {
            Debug.Log("读取失败！！！");
        }
    }


    // opcuaServer实例化
    OpcUaClient m_OpcUaClient = new OpcUaClient();

    //设置匿名连接
        async void Connect()
    {
        m_OpcUaClient.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
        // 这是一个连接服务器的示例
        try
        {
            await m_OpcUaClient.ConnectServer("opc.tcp://192.168.10.44:49320");
            Debug.Log("OPC UA connected！！！");
        }
        catch (Exception)
        {
            Debug.Log("连接失败！！！");
        }

    }

    
    void Start()
    {
        Connect();
        // 填入多个订阅节点
        MonitorNodeTags = new string[]
        {
           "ns=2;s=Lab.Pose.R1J1",
           "ns=2;s=Lab.Pose.R1J2",
           "ns=2;s=Lab.Pose.R1J3",
           "ns=2;s=Lab.Pose.R1J4",
           "ns=2;s=Lab.Pose.R1J5",
           "ns=2;s=Lab.Pose.R1J6",
           "ns=2;s=Lab.Position.Worker1X",
           "ns=2;s=Lab.Position.Worker1Y",
           "ns=2;s=Lab.Position.Worker1Z",
           "ns=2;s=Lab.Position.AGV001X",
           "ns=2;s=Lab.Position.AGV001Y",
           "ns=2;s=Lab.Position.AGV001Z",
           "ns=2;s=Lab.Position.Worker2X",
           "ns=2;s=Lab.Position.Worker2Y",
           "ns=2;s=Lab.Position.Worker2Z",

        };
        int ydim = MonitorNodeTags.GetLength(0);
        Record = new float[ydim];

    }

    void Update()
    {
        if (HDES.RunMonitor)
        {
            Subscription(MonitorNodeTags);
        }
        
    }
}
