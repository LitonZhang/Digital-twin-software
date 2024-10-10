using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : MonoBehaviour
{
    public static float ControllerSpeed;
    public static bool simulation;
    public static bool monitor;
    public Machine AEOUT;
    public Machine AEIN;
    public string AEOUTname;
    public string AEINname;
    public GameObject PE;
    public static List<string[]> EventList = new List<string[]>();
    private int ListIndex;

    // Start is called before the first frame update
    void Start()
    {
        ControllerSpeed = 1;
        simulation = false;
        ListIndex = 0;

        //Debug.Log(DTEventList);

    }
    public string TransferName(string rawName)
    {
        string DTname="null";
        //对象名称转换
        if (rawName == "Robot")
        {
            DTname = "Robot1";
        }
        else if (rawName == "S1")
        {
            DTname = "Station1";
        }
        else if (rawName == "AGV001")
        {
            DTname = "AGV1";
        }
        else if (rawName == "worker1")
        {
            DTname = "Worker1";
        }
        else if (rawName == "worker2")
        {
            DTname = "Worker2";
        }
        else
        {
            DTname = rawName;
        }
        return DTname;
    }
    public void EventTransfer(string[] EventMessage,int ListIndex)
    {
        string Event = EventMessage[0];
        string PEid = EventMessage[1]; 
        string Realtime = EventMessage[3];
        //对象名称转换
        EventMessage[2] = TransferName(EventMessage[2]);
        string AEname = EventMessage[2];

        //Debug.Log(EventMessage[2]);
        if (Event == "IN")
        {

            if (AEname == "Source")//Source是创建PE，其他是移动PE
            {             
                Utilities.CreatePart(HDES.Source,PEid);
                //PE = GameObject.Find(EventMessage[1]);
            }
            else //其他AE
            {
                for (int i = ListIndex-1; i >= 0; i--)//从执行事件行往上查找OUT事件
                {
                    string a = EventList[i][0];
                    string b = EventList[i][1];
                    string c = EventList[i][2];
                    string d = EventList[i][3];
                    if (EventList[i][0] == "OUT" && EventList[i][1]== EventList[ListIndex][1]) 
                    {
                        AEOUTname = EventList[i][2];
                        AEINname = AEname;
                        foreach (Machine obj in HDES.MachinList)
                        {
                            if (obj.Name == AEOUTname)
                            {
                                AEOUT = obj;
                            }
                            if (obj.Name == AEINname)
                            {
                                AEIN = obj;
                            }
                        }
                        Utilities.MovePart(AEOUT,AEIN);
                        break;
                    }
                }
            }
        }
        else if (Event == "OUT")
        {
            if (EventMessage[2] == "Station1")
            {
                foreach (Machine obj in HDES.MachinList)
                {
                    if (obj.Name == AEname)
                    {
                        AEIN = obj;
                    }
                }
                Utilities.DestoryPart(AEIN);
            }
        }
        else if (Event != "OUT" && Event != "IN")
        {
            Debug.LogError("事件数据类型出错");
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        if (EventList.Count> ListIndex)  //逐行执行
        {
            //Debug.Log("EventList.Count"+ EventList.Count+ "ListIndex" + ListIndex);
            EventTransfer(EventList[ListIndex], ListIndex);
            ListIndex ++;
        }
    }
}
