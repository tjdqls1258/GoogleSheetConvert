using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ScriptDataLoader<DATA> where DATA : CSVData, new()
{
    readonly static string CSV_PATH = "Assets/Util/GoogleSheet/CSVData/";

    public static Dictionary<int, DATA> ReadFile((string, Type) strFileType, ICsvListHelper csvList) 
    {
        string reader = strFileType.Item1;
        string[] dataList = reader.Split("\n");
        int currentLine = 0;
        bool isLast = false;

        Type type = strFileType.Item2;
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<int, DATA> result = new();

        while (!isLast)
        {
            DATA data = new();
            
            if (currentLine >= dataList.Length)
            {
                isLast = true;
                break;
            }

            string datas = dataList[currentLine];
            currentLine++;

            var dataSplit = datas.Split(',');

            for (int i = 0; i < dataSplit.Length; i++) 
            {
                FieldInfo field = fieldInfos[i];
                Type fieldType = fieldInfos[i].FieldType;

                object value;
                object stringData = dataSplit[i];

                try
                {
                    if (fieldType.IsEnum)
                    {
                        if (int.TryParse(dataSplit[i], out int enumNumber))
                            value = Enum.ToObject(fieldType, enumNumber);
                        else
                            value = Enum.Parse(fieldType, dataSplit[i]);
                    }
                    else
                    {
                        if (fieldType == typeof(string))//안드로이드 경우 \r가 붙어서 제거.
                            value = ((string)(Convert.ChangeType(stringData, fieldType))).Replace("\r", "") ?? string.Empty;
                        else
                            value = Convert.ChangeType(stringData, fieldType);
                    }

                    field.SetValue(data, value);
                }
                catch (Exception e)
                {
                    Logger.LogError("ex : " + e.Message);
                    Logger.LogError($"i : {i}, fieldType : {fieldType}, list[i] : {dataSplit[i]}");
                }
            }

            result.Add(data.GetID(), data);
        }

        return result;
    }
}

public class CSVHelper
{
    private const string PATH = "CSVData/{0}.csv";
    private readonly string PATH_LOCAL = $"{Application.dataPath}/Util/GoogleSheet/CSVData/{{0}}.csv";
    protected Dictionary<Type, object> m_scriptDataList = new();

    public enum CSVFile
    {
        CharacterData
    }

    private readonly (CSVFile, ICsvListHelper)[] m_csvData =
    {
        (CSVFile.CharacterData ,(new CharacterDataList())),
    };

    public void InitCSVData()
    {
        foreach(var data in m_csvData)
        {
            StreamReader reader = new StreamReader(string.Format(PATH_LOCAL, data.Item1.ToString()));
            data.Item2.SetDatas(reader.ReadToEnd());
            m_scriptDataList.Add(data.Item2.GetType(), data.Item2);
        }
    }

    public async UniTask InitCSVDataAsync()
    {
        List<UniTask> TaskList = new(); 
        foreach(var data in m_csvData)
        {
            TaskList.Add(InitItem(data));
        }

        await UniTask.WhenAll(TaskList);

        async UniTask InitItem((CSVFile, ICsvListHelper) data)
        {
            var file = await AddressableManager.Instance.LoadAssetAndCacheAsync<TextAsset>(string.Format(PATH, data.Item1.ToString()));
            data.Item2.SetDatas(file.text);
            m_scriptDataList.Add(data.Item2.GetType(), data.Item2);
        }
    }

    public T GetScripteData<T>() where T : class
    {
        return m_scriptDataList[typeof(T)] as T;
    }
}

public abstract class CSVData
{
    public abstract int GetID();
}

public class CSVDataList<Data> : ICsvListHelper where Data : CSVData, new()
{
    protected Dictionary<int, Data> m_dataList = new();

    public virtual void SetDatas(TextAsset file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile((file.text, typeof(Data)), this);
    }

    public virtual void SetDatas(string file)
    {
        m_dataList = ScriptDataLoader<Data>.ReadFile((file, typeof(Data)), this);
    }

    public virtual Data GetData(int id)
    {
        return m_dataList[id];
    }
}

public interface ICsvListHelper
{
    public void SetDatas(TextAsset csvFile);
    public void SetDatas(string csvFilePath);
}
