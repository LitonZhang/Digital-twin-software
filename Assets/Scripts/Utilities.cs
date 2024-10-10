using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Utilities : MonoBehaviour
{
    //函数用于控制子对象在父对象之间转移
    public static void MovePart(Machine AEOUT, Machine AEIN)
    {
        GameObject aeOUT = GameObject.Find(AEOUT.Name);
        GameObject aeIN = GameObject.Find(AEIN.Name);
        GameObject Part = aeOUT.transform.Find("Container").GetChild(0).gameObject;
        Transform PartTransform = Part.transform;
        Vector3 PartHigh = new(0, Part.GetComponent<Renderer>().bounds.size.y / 2, 0);
        PartTransform.SetPositionAndRotation(aeIN.transform.Find("Container").position + PartHigh, aeIN.transform.rotation);
        PartTransform.SetParent(aeIN.transform.Find("Container"));

    }

    public static void CreatePart(Machine AE, string PEname)//AE:active entity，比如机床； PE:passive entity，比如零件
    {
        GameObject ae = GameObject.Find(AE.Name);
        GameObject pe = Resources.Load<GameObject>("PEPrefab");
        Transform ContainerTransform = ae.transform.Find("Container");
        Vector3 ContainerPosition = ContainerTransform.transform.position;
        Quaternion ContainerRotation = ContainerTransform.transform.rotation;
        Vector3 InstantiatedPosition = new(ContainerPosition.x, ContainerPosition.y + pe.GetComponent<Renderer>().bounds.size.y / 2, ContainerPosition.z);
        GameObject InstantiatedPart = Instantiate(pe, InstantiatedPosition, ContainerRotation, ContainerTransform);
        InstantiatedPart.name= PEname;
    }
    public static void DestoryPart(Machine AE)
    {
        GameObject ae = GameObject.Find(AE.Name);
        GameObject Part = ae.transform.Find("Container").GetChild(0).gameObject;
        Destroy(Part);
    }

    //计算移动消耗时间
    public static float RouteTime(MAE Mae, PE Target)
    {
        GameObject mae = GameObject.Find(Mae.Name);
        GameObject target = GameObject.Find(Target.Name);
        float Time;
        float MAESpeed = Mae.Speed;
        Vector3 PointA = mae.transform.position;
        Vector3 PointB = target.transform.position;
        Vector2 NewPointA = new(PointA.x, PointA.z);
        Vector2 NewPointB = new(PointB.x, PointB.z);
        Time = Vector2.Distance(NewPointA, NewPointB)/ MAESpeed;
        return Time;
    }

    //函数用于MAE移动到坐标点
    public static void RouteToMark(MAE Mae, PE Target)
    {
        GameObject mae = GameObject.Find(Mae.Name);
        GameObject target = GameObject.Find(Target.Name);
        // 获取目标对象的位置
        Vector3 targetPosition = target.transform.position;
        // 保持当前Y坐标，只更新X和Z坐标
        Vector3 newPosition = new(targetPosition.x, mae.transform.position.y, targetPosition.z);

        float MAESpeed = Mae.Speed;
        float Distance = CalculateDistance(mae, target);

        if (HDES.ClockSpeed * MAESpeed*Time.deltaTime > Distance)
        {
            mae.transform.position = newPosition;
        }
        else
        {
            // 使对象朝向目标位置
            //transform.LookAt(newPosition);

            // 计算对象向目标位置移动的方向
            Vector3 moveDirection = (newPosition - mae.transform.position).normalized;

            // 移动对象
            mae.transform.Translate(HDES.ClockSpeed*MAESpeed * Time.deltaTime * moveDirection);
        }
    }

    public static (bool,string) Process(GameObject Machine, float ProcessTime, float timestamp)
    {
        bool ExitCtrl = false;
        string Pose = "0";
        Dictionary<string, (float, float, float, float, float)> machineValues = new()
        {
            { "cnc1", (45, -98, 270, 1200, 950) },
            { "cnc2", (-78, 70, 180, 800, 450) },
            { "cnc3", (90, 25, 45, 1600, 750) },
            { "cnc4", (0, -115, 310, 600, 600) },
            { "cnc5", (112, 88, 90, 1750, 850) },
            { "cnc6", (-30, 0, 0, 1100, 550) },
            { "cnc7", (76, 120, 155, 950, 725) },
            { "cnc8", (-105, -40, 35, 1350, 975) },
            { "cnc9", (30, -75, 200, 1750, 825) },
            { "cnc10", (-88, 95, 330, 700, 500) },
            { "cnc11", (60, -20, 280, 1300, 900) },
            { "cnc12", (-20, 60, 75, 550, 420) },
            // Add default values here if needed
        };

        float j1, j2, j3, j4, j5;

        if (machineValues.ContainsKey(Machine.name))
        {
            (j1, j2, j3, j4, j5) = machineValues[Machine.name];
        }
        else
        {
            // Assign default values if the machine name is not found
            (j1, j2, j3, j4, j5) = (115, 37, 190, 1850, 700);
        }

        //一帧的仿真时间大于处理时间时，动画无意义
        if (HDES.ClockSpeed * Time.deltaTime < ProcessTime)
        {
            if (timestamp < ProcessTime)
            {
                Transform PartTransform = Machine.transform.Find("Container").GetChild(0).transform;
                // 计算旋转的百分比
                float p = timestamp / ProcessTime;
                // 使用 Quaternion.Lerp 实现插值旋转
                PartTransform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 180, 0), p);
                Pose= Convert.ToString(Math.Round(p * j1,1)) +"/"+ Convert.ToString(Math.Round(p * j2, 1)) + "/" + Convert.ToString(Math.Round(p * j3, 1)) + "/" + Convert.ToString(Math.Round(p * j4, 1)) + "/" + Convert.ToString(Math.Round(p * j5, 1));
            }
            else
            {
                ExitCtrl = true;
                HDES.DTclock += ProcessTime;
                Pose = Convert.ToString(j1) + "/" + Convert.ToString(j2) + "/" + Convert.ToString(j3) + "/" + Convert.ToString(j4) + "/" + Convert.ToString(j5);
            }
        }
        else
        {
            ExitCtrl = true;
            HDES.DTclock += ProcessTime;
        }
        return (ExitCtrl, Pose);
    }

    // 函数用于比较两个物体的坐标是否相等
    public static bool ArePositionsEqual(GameObject object1, GameObject object2)
    {
        Transform object1Transform = object1.transform;
        Transform object2Transform = object2.transform;
        Vector2 object1Position = new(object1Transform.position.x, object1Transform.position.z);
        Vector2 object2Position = new(object2Transform.position.x, object2Transform.position.z);

        float distance = Vector3.Distance(object1Position, object2Position);
        return distance <= 0.01;
    }
    public static bool WaitForSeconds(float timestamp, float secondsToWait)
    {
        bool Complete;
        float MaxSpeed = 1.0f/Time.deltaTime;//MaxSpeed=实时帧率，大于实时帧率时，动画无意义
        if (HDES.ClockSpeed <= MaxSpeed)
        {
            if (timestamp >= secondsToWait)
            {
                Complete = true;
            }
            else
            {
                Complete = false;
            }
        }
        else
        {
            Complete = true;
        }  
        return Complete;
    }

    public static float CalculateDistance(GameObject A,GameObject B)
    {
        float Distance;
        Vector2 NewPointA = new(A.transform.position.x, A.transform.position.z);
        Vector2 NewPointB = new(B.transform.position.x, B.transform.position.z);
        Distance = Vector2.Distance(NewPointA, NewPointB);
        return Distance;
    }
    public static bool SetScriptValueBool(GameObject AE,string variable,bool value)
    {
        //查找脚本，修改变量
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // 检查每个组件是否是指定的脚本
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//设置值
                    //Debug.Log("HDES修改脚本变量" + variable +"为true ");
                    return true; // 找到并修改变量后退出循环
                }
                else
                {
                    //Debug.LogError("HDES未找到变量："+ variable);
                }
            }
        }
        return false;
    }
    public static bool SetScriptValueString(GameObject AE, string variable, string value)
    {
        //查找脚本，修改变量
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // 检查每个组件是否是指定的脚本
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//设置值
                    //Debug.Log("HDES修改脚本变量" + variable +"为true ");
                    return true; // 找到并修改变量后退出循环
                }
                else
                {
                    //Debug.LogError("HDES未找到变量："+ variable);
                }
            }
        }
        return false;
    }
    public static void SetScriptValueFloat(GameObject AE, string variable, float value)
    {
        //查找脚本，修改变量
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // 检查每个组件是否是指定的脚本
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//设置值
                    //Debug.Log("HDES修改脚本变量" + variable +"为true ");
                }
                else
                {
                    //Debug.LogError("HDES未找到变量："+ variable);
                }
            }
        }
    }
    public static string ConvertToNewDate(string InputTime, float AddTime)
    {
        //切分
        //string SplitDate= InputTime.Substring(0, Math.Min(10, InputTime.Length));//前10个字符是日期
        //string SplitTime = InputTime.Substring(0, Math.Max(0, InputTime.Length-8));//后8个字符是时间
        //转换
        DateTime parsedDate =DateTime.Parse(InputTime);
        long AddTimeStampInTicks = (long)(AddTime * TimeSpan.TicksPerSecond);
        DateTime NewdateTime = new(parsedDate.Ticks + AddTimeStampInTicks, DateTimeKind.Utc);
        return NewdateTime.ToString();
    }
    public static string ConvertToDate(float timeInSeconds)
    {
        // 转换为天
        int timeInDays = (int)(timeInSeconds / (24 * 3600)); // 1 天 = 24 小时 * 3600 秒

        // 获取剩余的小时数
        int remainingHours = (int)((timeInSeconds % (24 * 3600)) / 3600); // 1 小时 = 3600 秒

        // 获取剩余的分钟数
        int remainingMinutes = (int)((timeInSeconds % 3600) / 60); // 1 分钟 = 60 秒

        // 获取剩余的秒数
        int remainingSeconds = (int)(timeInSeconds % 60);

        string NewdateTime = Convert.ToString(timeInDays) +":"+ Convert.ToString(remainingHours) + ":" + Convert.ToString(remainingMinutes) + ":" + Convert.ToString(remainingSeconds);
        return NewdateTime;
    }

    public static double ReadScriptDoubleVariable(GameObject Entity, string VariableName)
    {
        //查找脚本，修改变量
        string scriptName = Entity.name + "MFSM";
        Component[] components = Entity.GetComponents(typeof(Component));
        double DoubleValue = 0;
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // 检查每个组件是否是指定的脚本
            {
                Type type = component.GetType();//利用字符串查找脚本
                System.Reflection.FieldInfo field = type.GetField(VariableName);//利用反射查找变量
                if (field != null)
                {
                    var Value = field.GetValue(component);
                    DoubleValue = Math.Round(Convert.ToDouble(Value), 4); //转换为double并保留2位小数
                }
            }
        }
        return DoubleValue;
    }
    public static string ReadScriptStringVariable(GameObject Entity, string VariableName)
    {
        //查找脚本，修改变量
        string Value = "null";
        string scriptName = Entity.name + "MFSM";
        Component[] components = Entity.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // 检查每个组件是否是指定的脚本
            {
                Type type = component.GetType();//利用字符串查找脚本
                System.Reflection.FieldInfo field = type.GetField(VariableName);//利用反射查找变量
                if (field != null)
                {
                    Value = Convert.ToString(field.GetValue(component));
                }
            }
        }
        return Value;
    }
    public static string GetVariableinConmoment(GameObject AE,string scriptName,string variableName)
    {
        string variableValue = "error";
        // 获取当前对象上指定名称的脚本
        Component component = AE.GetComponent(scriptName);

        if (component != null && component is MonoBehaviour)
        {
            // 转换为 MonoBehaviour 类型，以便访问公共变量
            MonoBehaviour yourScript = (MonoBehaviour)component;

            // 访问指定脚本的公共变量
            variableValue = Convert.ToString(yourScript.GetType().GetField(variableName).GetValue(yourScript));
            //Debug.Log("Value of yourVariable in " + scriptName + ": " + variableValue);
        }
        else
        {
            Debug.LogError(scriptName + " component not found!");
        }
        return variableValue;
    }
}
