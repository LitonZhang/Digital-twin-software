using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 简单示例,如果想提高运行速度，请修改程序为独立线程
public class HDES : MonoBehaviour
{
    public static bool RunSimulaion;
    public static bool RunMonitor;
    //时钟
    public static float DTclock;
    public static float VirtualClock;
    public static float ClockSpeed;
    // Entities
    public static List<Machine> MachinList;
    public static List<MAE> MAEList;
    public static List<PE> TargetList;
    public string PEname;
    public static bool RunMFSM;
    public ECPA ecpa;
    //生产逻辑变量
    public static Machine Source;
    public static Machine station1;
    public static Machine PartEnd;
    public static List<List<string>> RTEventList;//Real-time event list

    static HDES()
    {
        MachinList = new() ;
    }
    void Start()
    {
        RunSimulaion = true;
        ecpa = new();
        DTclock = 0;
        ClockSpeed = 100000;//倍速
        RunMFSM = true;
        // 启动while循环
        StartCoroutine(ControlledWhileLoop());
    }
    IEnumerator ControlledWhileLoop()
    {
        float startTime = Time.time; // 获取循环开始的时间
        float maxDuration = 20.0f;    // 设置超时时间，2秒钟

        Source = new Machine("Source", 1000); //Name,ProcessTime
        station1 = new Machine("CNC_01", 2000);
        PartEnd = new Machine("PartEnd",0);
        MachinList = new() { Source, station1, PartEnd };
        Source.Entrance = true;
        while (station1.StatNumIn <= 10 )
        {
            
            // 检查是否已经超时
            if (Time.time - startTime >= maxDuration)
            {
                Debug.Log("Loop timed out.");
                break;
            }

            if (RunMFSM)
            {
                // scan MFSM
                SourceIN();
                SourceOUT();
                station1IN();
                station1OUT();
                PartEndIN();
            }
            StartCoroutine(ecpa.Run());
            yield return null; // 等待一帧，防止阻塞主线程
        }  
        Debug.Log("仿真结束.");
    }
    private void SourceIN()
    {
        if (Source.Empty && station1.Empty && Source.Entrance == true)
        {
            PEname = "PE0000" + (Source.StatNumIn + 1).ToString();
            Source.CreateEvent(PEname);
            Source.Entrance = false;
        }
    }
    private void SourceOUT()
    {
        if (!Source.Empty && station1.Empty && Source.Exit == true)
        {
            Source.MoveEvent(station1, PEname);
            Source.Exit = false;
        }
    }
    private void station1IN()
    {
        if (!station1.Empty && station1.Entrance == true)
        {
            StartCoroutine(station1.Process()); // 等待
            station1.Entrance = false;
        }
    }
    private void station1OUT()
    {
        if(!station1.Empty && station1.Exit == true)
        {
            station1.MoveEvent(PartEnd, PEname);
            station1.Exit = false;
        }
    }
    private void PartEndIN()
    {
        if (PartEnd.Entrance == true)
        {
            Utilities.DestoryPart(PartEnd);
            PartEnd.PartDestory();
            PartEnd.Entrance = false;
            Source.Entrance = true;
            if (PartEnd.StatNumIn==10)
            {
                Source.GetStatistics();
                station1.GetStatistics();
                PartEnd.GetStatistics();
            }
        }
    }
}
public enum MachineState
{
    Waiting,
    Working,
    Blocked,
    Failed,
    Paused
}

//Source,Station,PartEnd存在多个通用函数，此例子写为一个类
public class Machine
{
    public string Name { get; private set; }
    public MachineState State { get; private set; }
    public Vector3 Pose { get; private set; }
    public Vector3 Position { get; private set; }

    public List<string> MaterialList { get; private set; }
    public int StatNumIn;
    public int StatNumOut;
    private float TotalWorkingTime;
    static public double TotalRunTime;
    private int ProcessTime;
    public bool Empty;
    public bool Entrance;
    public bool Exit;


    public Machine(string name, int Ptime)
    {
        Name = name;
        State = MachineState.Waiting;
        MaterialList = new List<string>();
        Pose = new Vector3(0, 0, 0);
        Position = new Vector3(0, 0, 0);
        ProcessTime = Ptime;
        TotalWorkingTime = 0.0f;
        StatNumIn = 0;
        StatNumOut = 0;
        Empty = true;
        Entrance = false;
        Exit =false;
    }
    // 定义一个等待几秒的函数
    public IEnumerator Process()
    {
        float seconds = ProcessTime/HDES.ClockSpeed;
        // 等待指定的秒数
        yield return new WaitForSeconds(seconds);

    }
    public void MaterialEnter(string material)
    {
        MaterialList.Add(material);
        Debug.Log($"Material {material} entered the {Name}.");
        ChangeStateIn(ProcessTime);
        Exit = true;
    }

    public void MaterialExit(string material)
    {
        if (MaterialList.Contains(material))
        {
            MaterialList.Remove(material);
            Debug.Log($"Material {material} exited the {Name}.");
            ChangeStateOut();
        }
        else
        {
            Debug.Log($"Material {material} not found.");
        }
    }

    public void ChangeStateIn(int processTime = 0)
    {
        Empty = false;
        StatNumIn++;
        UpdateWorkingTime(processTime);
        State = MachineState.Working;
        //Debug.Log($"{Name} state changed to {State}.");
    }
    public void ChangeStateOut()
    {
        Empty = true;
        StatNumOut++;
        State = MachineState.Waiting;
        //Debug.Log($"{Name} state changed to {State}.");
    }
    public void Move(Machine AEIN, string PE)
    {
        MaterialExit(PE);
        AEIN.Entrance = true;
        AEIN.MaterialEnter(PE);
        Utilities.MovePart(this,AEIN);
    }
    public void MoveEvent(Machine AEIN, string PE)
    {
        float EventInsertTime = HDES.DTclock;
        float EventStartTime = EventInsertTime + ProcessTime;
        List<string> DTevent = new() { "Move", Name, AEIN.Name, PE, EventStartTime.ToString(), EventInsertTime.ToString() };
        ECPA.EventList.Add(DTevent);
    }
    public void Create(string PE)
    {
        MaterialEnter(PE);
        Utilities.CreatePart(this,PE);
    }
    public void CreateEvent(string PE)
    {
        float EventInsertTime = HDES.DTclock;
        float EventStartTime = EventInsertTime;
        List<string> DTevent = new() { "Create", Name, Name, PE, EventStartTime.ToString(), EventInsertTime.ToString() };
        ECPA.EventList.Add(DTevent);

}
    public void PartDestory()
    {
        if (MaterialList.Count > 0)
        {
            MaterialList.Clear();
            Entrance = true;
            Debug.Log($"Material has destoried.");
        }
        Empty = true;
    }
    public void Fault(Machine AE, float FP, float ProcessTime)
    {
        double doubleFP = FP;
        System.Random random = new();
        double minValue = 0.0; // 最小值
        double maxValue = 1.0; // 最大值
        double randomNumber = random.NextDouble() * (maxValue - minValue) + minValue;
        Debug.Log(randomNumber);
        if (randomNumber <= doubleFP)
        {
            float EventInsertTime = HDES.DTclock;
            float EventStartTime = EventInsertTime;
            float EventEndTime = EventStartTime + ProcessTime;
            List<string> DTevent1 = new() { "Fault", null, AE.Name, null, EventStartTime.ToString(), EventInsertTime.ToString(), "Start" };
            List<string> DTevent2 = new() { "Fault", null, AE.Name, null, EventEndTime.ToString(), EventInsertTime.ToString(), "End" };
            ECPA.EventList.Add(DTevent1);
            ECPA.EventList.Add(DTevent2);
        }
    }
    private void UpdateWorkingTime(float processTime)
    {
        TotalWorkingTime += processTime;
        TotalRunTime += processTime;
    }
    public void UpdatePose(Vector3 newPose)
    {
        Pose = newPose;
    }
    public void GetStatistics()
    {
        Debug.Log("===== Machine Statistics =====");
        Debug.Log($"Machine Name: {Name}");
        Debug.Log($"Current State: {State}");
        Debug.Log($"Total Working Time: {TotalWorkingTime} seconds");
        Debug.Log($"Total Runtime: {TotalRunTime} seconds");
        Debug.Log($"Utilization Rate: {TotalWorkingTime / TotalRunTime * 100:F2}%");
        Debug.Log($"Material Enter Count: {StatNumIn}");
        Debug.Log($"Material Exit Count: {StatNumOut}");
    }
}
public class MAE
{
    public string Name { get; private set; }
    public MachineState State { get; private set; }
    public Vector3 Pose { get; private set; }
    public Vector3 Position { get; private set; }
    public List<string> MaterialList { get; private set; }
    public List<PE> TargetlList { get; private set; }
    private int StatNumIn;
    private int StatNumOut;
    private float TotalWorkingTime;
    static public double TotalRunTime;
    private int ProcessTime;
    public bool Empty;
    public float Speed;
    public bool DestinationWasReached;

    public MAE(string name, int Ptime, float speed)
    {
        Name = name;
        State = MachineState.Waiting;
        MaterialList = new List<string>();
        Pose = new Vector3(0, 0, 0);
        Position = new Vector3(0, 0, 0);
        ProcessTime = Ptime;
        TotalWorkingTime = 0.0f;
        StatNumIn = 0;
        StatNumOut = 0;
        Empty = true;
        Speed = speed;
        DestinationWasReached = true;
    }
    public void RouteToMark(PE Target, bool Animation)
    {
        // 获取目标对象的位置
        Vector3 targetPosition = Target.Position;
        // A的y坐标
        float fixedY = Position.y;
        // 保持当前Y坐标，只更新X和Z坐标
        Vector3 newPosition = new(targetPosition.x, fixedY, targetPosition.z);

        float MAESpeed = Speed;
        float Distance = CalculateDistance(Position, targetPosition);

        if (Animation)
        {
            // 在XZ平面上移动A逐渐接近B
            Vector3 newA = Vector3.Lerp(Position, Target.Position, HDES.ClockSpeed * MAESpeed * Time.deltaTime);
            // 保持y不变
            newA.y = fixedY;
            // 更新A的位置
            Position = newA;
            
        }
        else
        {
            Position = newPosition;
        }
    }
    public float CalculateDistance(Vector3 A, Vector3 B)
    {
        float Distance;
        Vector2 NewPointA = new(A.x, A.z);
        Vector2 NewPointB = new(B.x, B.z);
        Distance = Vector2.Distance(NewPointA, NewPointB);
        return Distance;
    }
    public void MaterialEnter(string material)
    {
        MaterialList.Add(material);
        Debug.Log($"Material {material} entered the {Name}.");
        ChangeStateIn(ProcessTime);
    }
    public void MaterialExit(string material)
    {
        if (MaterialList.Contains(material))
        {
            MaterialList.Remove(material);
            Debug.Log($"Material {material} exited the {Name}.");
            ChangeStateOut();
        }
        else
        {
            Debug.Log($"Material {material} not found.");
        }
    }
    public void ChangeStateIn(int processTime = 0)
    {
        Empty = false;
        StatNumIn++;
        UpdateWorkingTime(processTime);
        State = MachineState.Working;
        //Debug.Log($"{Name} state changed to {State}.");
    }
    public void ChangeStateOut()
    {
        Empty = true;
        StatNumOut++;
        State = MachineState.Waiting;
        //Debug.Log($"{Name} state changed to {State}.");
    }
    public void Move(Machine AEIN, string PE)
    {
        MaterialExit(PE);
        AEIN.MaterialEnter(PE);
    }
    public void MoveEvent(Machine AEIN, string PE)
    {
        float EventInsertTime = HDES.DTclock;
        float EventStartTime = EventInsertTime + ProcessTime;
        List<string> DTevent = new() { "Move", Name, AEIN.Name, PE, EventStartTime.ToString(), EventInsertTime.ToString() };
        ECPA.EventList.Add(DTevent);
    }
    public void Fault()
    {

    }
    private void UpdateWorkingTime(float processTime)
    {
        TotalWorkingTime += processTime;
        TotalRunTime += processTime;
    }
    public void UpdatePose(Vector3 newPose)
    {
        Pose = newPose;
    }
    public void GetStatistics()
    {
        Debug.Log("===== Machine Statistics =====");
        Debug.Log($"Machine Name: {Name}");
        Debug.Log($"Current State: {State}");
        Debug.Log($"Total Working Time: {TotalWorkingTime} seconds");
        Debug.Log($"Total Runtime: {TotalRunTime} seconds");
        Debug.Log($"Utilization Rate: {TotalWorkingTime / TotalRunTime * 100:F2}%");
        Debug.Log($"Material Enter Count: {StatNumIn}");
        Debug.Log($"Material Exit Count: {StatNumOut}");
    }
    public int GetStatNumIn()
    {
        return StatNumIn;
    }
    public int GetStatNumOut()
    {
        return StatNumOut;
    }
}
public class PE
{
    public string Name { get; private set; }
    public Vector3 Position { get; private set; }

    public PE(string name)
    {
        Name = name;
        Position = new Vector3(0, 0, 0);
    }
}