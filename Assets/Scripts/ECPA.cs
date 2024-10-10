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
                //事件重排序
                EventList = EventList.OrderBy(x => float.Parse(x[4])).ToList();

                //遍历事件列表所有事件
                for (int i = 0; i < EventList.Count; i++)
                {
                    List<string> DTevent = EventList[0]; //事件列表
                    EventName = DTevent[0]; //执行名称

                    //执行其他事件，推进仿真时钟
                    float EventStartTime = float.Parse(DTevent[4]);//事件结束时间
                    float EventInsertTime = float.Parse(DTevent[5]);
                    if (HDES.VirtualClock >= EventStartTime)
                    {
                        //查找AE
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
                        //不是Route事件时
                        if (EventName != "Route")
                        {
                            //事件执行
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
                            EventList.RemoveAt(0);//执行完删除事件   
                            //推进时钟                                                 
                            RealTimeDuration = (EventStartTime - LastEventTime) / HDES.ClockSpeed;
                            HDES.DTclock = EventStartTime;
                            LastEventTime = HDES.DTclock;
                            HDES.VirtualClock = HDES.DTclock;
                            yield return new WaitForSeconds(RealTimeDuration); // 等待事件耗时结束
                        }
                        else//当Route事件时
                        {
                            //查找mae
                            MAEname = DTevent[2];
                            foreach (MAE obj in HDES.MAEList)
                            {
                                if (obj.Name == MAEname)
                                {
                                    mae = obj;
                                }
                            }
                            //查找target
                            targetname = DTevent[3];
                            foreach (PE obj in HDES.TargetList)
                            {
                                if (obj.Name == targetname)
                                {
                                    target = obj;
                                }
                            }
                            if (DTevent[4] != DTevent[6])//开始执行Route
                            {
                                DTevent[4] = DTevent[6];
                                EventList[0][4] = DTevent[4];
                                mae.TargetlList.Add(target);
                                mae.RouteToMark(target, true);
                                mae.DestinationWasReached = false;
                                EventList = EventList.OrderBy(x => float.Parse(x[4])).ToList();//事件重排序
                            }
                            else//结束执行Route
                            {
                                mae.RouteToMark(target, false);
                                EventList.RemoveAt(0);//执行完删除Route事件
                                mae.DestinationWasReached = true;
                                //推进时钟
                                RealTimeDuration = (EventStartTime - LastEventTime) / HDES.ClockSpeed;
                                HDES.DTclock = EventStartTime;
                                LastEventTime = HDES.DTclock;
                                HDES.VirtualClock = HDES.DTclock;
                            }
                        }
                    }
                }
            }
            yield return null; // 等待一帧，防止阻塞主线程
        } 
    }
}

