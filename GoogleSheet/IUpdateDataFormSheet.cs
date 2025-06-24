#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class UpdateDataFormSheet
{
    public static void CreateData<Data>(List<string> datas, string path) where Data : ScriptableObject, IUpdateDataFormSheet, new()
    {
        Data data = new Data();
        var fields = typeof(Data).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance);
        string ID = "";
        for (int i = 0; i < fields.Length; i++)
        {
            if (datas.Count <= i)
                break;
            if (i == 0)
                ID = datas[i];
            FieldInfo field = fields[i];
            Type fieldType = fields[i].FieldType;

            object valueToSet;
            object value = datas[i];

            try
            {
                if (fieldType.IsEnum)
                {
                    if (int.TryParse(datas[i], out int enumNumber))
                        valueToSet = Enum.ToObject(fieldType, enumNumber);
                    else
                        valueToSet = Enum.Parse(fieldType, datas[i]);
                }
                else
                {
                    if (fieldType == typeof(string))//안드로이드 경우 \r가 붙어서 제거.
                        valueToSet = ((string)(Convert.ChangeType(value, fieldType))).Replace("\r", "") ?? string.Empty;
                    else
                        valueToSet = Convert.ChangeType(value, fieldType);
                }

                field.SetValue(data, valueToSet);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                Debug.LogError($"i : {i}, fieldType : {fieldType}, datas[i] : {datas[i]}");
            }
        }

        MakeScriptable(data, path, ID);
    }

    public static void MakeScriptable<Data>(Data data, string path, string ID) where Data : ScriptableObject, new()
    {
        Data d = data;
        string foldPath = path + $"/{typeof(Data).Name}";
        if (AssetDatabase.IsValidFolder(foldPath) == false)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        string datapath = $"{foldPath}/{typeof(Data).Name}_{ID}.asset";
        AssetDatabase.CreateAsset(data, datapath);
    }
}
#endif
public interface IUpdateDataFormSheet
{
}
