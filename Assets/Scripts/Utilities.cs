using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Utilities : MonoBehaviour
{
    //�������ڿ����Ӷ����ڸ�����֮��ת��
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

    public static void CreatePart(Machine AE, string PEname)//AE:active entity����������� PE:passive entity���������
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

    //�����ƶ�����ʱ��
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

    //��������MAE�ƶ��������
    public static void RouteToMark(MAE Mae, PE Target)
    {
        GameObject mae = GameObject.Find(Mae.Name);
        GameObject target = GameObject.Find(Target.Name);
        // ��ȡĿ������λ��
        Vector3 targetPosition = target.transform.position;
        // ���ֵ�ǰY���ֻ꣬����X��Z����
        Vector3 newPosition = new(targetPosition.x, mae.transform.position.y, targetPosition.z);

        float MAESpeed = Mae.Speed;
        float Distance = CalculateDistance(mae, target);

        if (HDES.ClockSpeed * MAESpeed*Time.deltaTime > Distance)
        {
            mae.transform.position = newPosition;
        }
        else
        {
            // ʹ������Ŀ��λ��
            //transform.LookAt(newPosition);

            // ���������Ŀ��λ���ƶ��ķ���
            Vector3 moveDirection = (newPosition - mae.transform.position).normalized;

            // �ƶ�����
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

        //һ֡�ķ���ʱ����ڴ���ʱ��ʱ������������
        if (HDES.ClockSpeed * Time.deltaTime < ProcessTime)
        {
            if (timestamp < ProcessTime)
            {
                Transform PartTransform = Machine.transform.Find("Container").GetChild(0).transform;
                // ������ת�İٷֱ�
                float p = timestamp / ProcessTime;
                // ʹ�� Quaternion.Lerp ʵ�ֲ�ֵ��ת
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

    // �������ڱȽ���������������Ƿ����
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
        float MaxSpeed = 1.0f/Time.deltaTime;//MaxSpeed=ʵʱ֡�ʣ�����ʵʱ֡��ʱ������������
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
        //���ҽű����޸ı���
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // ���ÿ������Ƿ���ָ���Ľű�
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//����ֵ
                    //Debug.Log("HDES�޸Ľű�����" + variable +"Ϊtrue ");
                    return true; // �ҵ����޸ı������˳�ѭ��
                }
                else
                {
                    //Debug.LogError("HDESδ�ҵ�������"+ variable);
                }
            }
        }
        return false;
    }
    public static bool SetScriptValueString(GameObject AE, string variable, string value)
    {
        //���ҽű����޸ı���
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // ���ÿ������Ƿ���ָ���Ľű�
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//����ֵ
                    //Debug.Log("HDES�޸Ľű�����" + variable +"Ϊtrue ");
                    return true; // �ҵ����޸ı������˳�ѭ��
                }
                else
                {
                    //Debug.LogError("HDESδ�ҵ�������"+ variable);
                }
            }
        }
        return false;
    }
    public static void SetScriptValueFloat(GameObject AE, string variable, float value)
    {
        //���ҽű����޸ı���
        string scriptName = AE.name + "MFSM";
        Component[] components = AE.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // ���ÿ������Ƿ���ָ���Ľű�
            {
                Type type = component.GetType();
                System.Reflection.FieldInfo field = type.GetField(variable);

                if (field != null)
                {
                    field.SetValue(component, value);//����ֵ
                    //Debug.Log("HDES�޸Ľű�����" + variable +"Ϊtrue ");
                }
                else
                {
                    //Debug.LogError("HDESδ�ҵ�������"+ variable);
                }
            }
        }
    }
    public static string ConvertToNewDate(string InputTime, float AddTime)
    {
        //�з�
        //string SplitDate= InputTime.Substring(0, Math.Min(10, InputTime.Length));//ǰ10���ַ�������
        //string SplitTime = InputTime.Substring(0, Math.Max(0, InputTime.Length-8));//��8���ַ���ʱ��
        //ת��
        DateTime parsedDate =DateTime.Parse(InputTime);
        long AddTimeStampInTicks = (long)(AddTime * TimeSpan.TicksPerSecond);
        DateTime NewdateTime = new(parsedDate.Ticks + AddTimeStampInTicks, DateTimeKind.Utc);
        return NewdateTime.ToString();
    }
    public static string ConvertToDate(float timeInSeconds)
    {
        // ת��Ϊ��
        int timeInDays = (int)(timeInSeconds / (24 * 3600)); // 1 �� = 24 Сʱ * 3600 ��

        // ��ȡʣ���Сʱ��
        int remainingHours = (int)((timeInSeconds % (24 * 3600)) / 3600); // 1 Сʱ = 3600 ��

        // ��ȡʣ��ķ�����
        int remainingMinutes = (int)((timeInSeconds % 3600) / 60); // 1 ���� = 60 ��

        // ��ȡʣ�������
        int remainingSeconds = (int)(timeInSeconds % 60);

        string NewdateTime = Convert.ToString(timeInDays) +":"+ Convert.ToString(remainingHours) + ":" + Convert.ToString(remainingMinutes) + ":" + Convert.ToString(remainingSeconds);
        return NewdateTime;
    }

    public static double ReadScriptDoubleVariable(GameObject Entity, string VariableName)
    {
        //���ҽű����޸ı���
        string scriptName = Entity.name + "MFSM";
        Component[] components = Entity.GetComponents(typeof(Component));
        double DoubleValue = 0;
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // ���ÿ������Ƿ���ָ���Ľű�
            {
                Type type = component.GetType();//�����ַ������ҽű�
                System.Reflection.FieldInfo field = type.GetField(VariableName);//���÷�����ұ���
                if (field != null)
                {
                    var Value = field.GetValue(component);
                    DoubleValue = Math.Round(Convert.ToDouble(Value), 4); //ת��Ϊdouble������2λС��
                }
            }
        }
        return DoubleValue;
    }
    public static string ReadScriptStringVariable(GameObject Entity, string VariableName)
    {
        //���ҽű����޸ı���
        string Value = "null";
        string scriptName = Entity.name + "MFSM";
        Component[] components = Entity.GetComponents(typeof(Component));
        foreach (Component component in components)
        {
            if (component.GetType().Name == scriptName) // ���ÿ������Ƿ���ָ���Ľű�
            {
                Type type = component.GetType();//�����ַ������ҽű�
                System.Reflection.FieldInfo field = type.GetField(VariableName);//���÷�����ұ���
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
        // ��ȡ��ǰ������ָ�����ƵĽű�
        Component component = AE.GetComponent(scriptName);

        if (component != null && component is MonoBehaviour)
        {
            // ת��Ϊ MonoBehaviour ���ͣ��Ա���ʹ�������
            MonoBehaviour yourScript = (MonoBehaviour)component;

            // ����ָ���ű��Ĺ�������
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
