using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ECPA
{
    public static List<List<string>> EventList;
    private string EventName;
    private string AEINname;
    private string AEOUTname;
    private Machine AEIN;
    private Machine AEOUT;
    private MAE mae;
    private PE target;
    private string PEname;
    private string MAEname;
    private string targetname;
    public static float LastEventTime;
    public static float RealTimeDuration;
    public static float simulatedTime;

    static ECPA()
    {
        EventList = new();
        RealTimeDuration = 0;
        LastEventTime = 0;
    }

    public IEnumerator Run()
    {
        if (HDES.RunSimulaion)
        {
            HDES.VirtualClock += Time.deltaTime * HDES.ClockSpeed;
            if (EventList.Count != 0)
            {
                //�¼�������
                EventList = EventList.OrderBy(x => float.Parse(x[4])).ToList();

                //�����¼��б������¼�
                for (int i = 0; i < EventList.Count; i++)
                {
                    List<string> DTevent = EventList[0]; //�¼��б�
                    EventName = DTevent[0]; //ִ������

                    //ִ�������¼����ƽ�����ʱ��
                    float EventStartTime = float.Parse(DTevent[4]);//�¼�����ʱ��
                    float EventInsertTime = float.Parse(DTevent[5]);
                    if (HDES.VirtualClock >= EventStartTime)
                    {
                        //����AE
                        AEINname = DTevent[2];
                        AEOUTname = DTevent[1];
                        PEname = DTevent[3];
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
                        //����Route�¼�ʱ
                        if (EventName != "Route")
                        {
                            //�¼�ִ��
                            if (EventName == "Create")
                            {
                                AEIN.Create(PEname);
                            }
                            else if (EventName == "Fault")
                            {

                            }
                            else if (EventName == "Move")
                            {
                                AEOUT.Move(AEIN, PEname);
                            }
                            EventList.RemoveAt(0);//ִ����ɾ���¼�   
                            //�ƽ�ʱ��                                                 
                            RealTimeDuration = (EventStartTime - LastEventTime) / HDES.ClockSpeed;
                            HDES.DTclock = EventStartTime;
                            LastEventTime = HDES.DTclock;
                            HDES.VirtualClock = HDES.DTclock;
                            yield return new WaitForSeconds(RealTimeDuration); // �ȴ��¼���ʱ����
                        }
                        else//��Route�¼�ʱ
                        {
                            //����mae
                            MAEname = DTevent[2];
                            foreach (MAE obj in HDES.MAEList)
                            {
                                if (obj.Name == MAEname)
                                {
                                    mae = obj;
                                }
                            }
                            //����target
                            targetname = DTevent[3];
                            foreach (PE obj in HDES.TargetList)
                            {
                                if (obj.Name == targetname)
                                {
                                    target = obj;
                                }
                            }
                            if (DTevent[4] != DTevent[6])//��ʼִ��Route
                            {
                                DTevent[4] = DTevent[6];
                                EventList[0][4] = DTevent[4];
                                mae.TargetlList.Add(target);
                                mae.RouteToMark(target, true);
                                mae.DestinationWasReached = false;
                                EventList = EventList.OrderBy(x => float.Parse(x[4])).ToList();//�¼�������
                            }
                            else//����ִ��Route
                            {
                                mae.RouteToMark(target, false);
                                EventList.RemoveAt(0);//ִ����ɾ��Route�¼�
                                mae.DestinationWasReached = true;
                                //�ƽ�ʱ��
                                RealTimeDuration = (EventStartTime - LastEventTime) / HDES.ClockSpeed;
                                HDES.DTclock = EventStartTime;
                                LastEventTime = HDES.DTclock;
                                HDES.VirtualClock = HDES.DTclock;
                            }
                        }
                    }
                }
            }
            yield return null; // �ȴ�һ֡����ֹ�������߳�
        } 
    }
}

