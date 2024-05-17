#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using Google.Apis.Sheets.v4;

public class SheetConvert<T> where T : new()
{
    /// <summary>
    /// Sheet의 이름과 해당 타입을 구성하는 매개변수의 이름이 같은것만 가져옴.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="result"></param>
    public void UpdateSheetConvert(IList<IList<object>> service, ref List<T> resultList)
    {
        int k = 0;
        for (int i = 1; i < service.Count; i++)
        {
            T result = new T();
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            k = 0;
            for (int j = 0; j < service[i].Count; j++)
            {
                if (fields[k].Name == service[0][j].ToString())
                {
                    if (service[i][j].ToString() == string.Empty) continue;
                    Type type = fields[k].FieldType;
                    object ob = null;
                    if (fields[k].FieldType.GetTypeInfo().IsEnum) // to fields[k].Enum
                    {
                        ob = Enum.Parse(type, service[i][j].ToString());
                    }
                    else
                    {
                        if (type.IsArray) //to type[]
                        {
                            string[] spiltValue = service[i][j].ToString().Split(";");
                            Type makeType = type.GetElementType();

                            List<object> obArray = new List<object>();
                            foreach (string value in spiltValue)
                            {
                                obArray.Add(Convert.ChangeType(value, makeType));
                            }
                            Array fielArray = Array.CreateInstance(makeType, obArray.ToArray().Length);
                            Array.Copy(obArray.ToArray(), fielArray, obArray.ToArray().Length);
                            ob = fielArray;
                        }
                        else
                        {
                            if (type == typeof(bool))
                            {
                                if (int.TryParse(service[i][j].ToString(), out int Value))
                                {
                                    ob = Convert.ChangeType(Value == 1 ? true : false, typeof(bool));
                                }
                                else
                                { ob = false; }
                            }
                            else
                            {
                                ob = Convert.ChangeType(service[i][j], type);
                            }
                        }
                    }
                    fields[k].SetValue(result, ob);
                    k++;
                }
            }
            resultList.Add(result);
        }
    }

    void UpdateStatsSheetC(IList<IList<object>> ss, ref List<T> resultList)
    {
        UpdateSheetConvert(ss, ref resultList);
    }

    public void UpDateMethodSheet(IList<IList<object>> ss,ref List<T> result)
    {
        UpdateStatsSheetC(ss, ref result);
    }
}
#endif