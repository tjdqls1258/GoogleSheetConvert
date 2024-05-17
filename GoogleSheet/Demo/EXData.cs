using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR //�ʿ�� �ۿ��� ����
using UnityEngine.Events;
using System.Linq;
using UnityEditor;
#endif

public class EXData : MonoBehaviour
#if UNITY_EDITOR 
    , IUpdateDataFormSheet
#endif
{
    [Serializable] //���� ����� ������ Inspectorâ���� Ȯ���ϱ� ����.
    public class ExDataClass
    {
        public int ID;
        public string Name;
        public int HP;
        public int ATK;
        public int DEF;
    }
    public List<ExDataClass> ExDatas;

#if UNITY_EDITOR
    readonly string SheetID = "SheetID In URL";
    readonly string SheetName = "SheetName Under Bar";
    readonly string pathName = $"{Application.dataPath}/Data/{nameof(EXData)}.prefab";

    void UpdateStats(UnityAction<IList<IList<object>>> callback, string sheetName)
    {
        GoogleSheetReder.TestCSV02(callback,SheetID,sheetName);
    }

    void UpdateMethod(IList<IList<object>> ss)
    {
        SheetConvert<ExDataClass> sheetConvert_I = new SheetConvert<ExDataClass>();
        sheetConvert_I.UpDateMethodSheet(ss, ref ExDatas);

        var SceneObject = Instantiate(gameObject);
        PrefabUtility.SaveAsPrefabAsset(SceneObject, pathName);
        DestroyImmediate(SceneObject);

        Debug.Log($"{gameObject.name} Update End");
    }

    void InitData()
    {
        ExDatas = new List<ExDataClass>();
    }

    public void ReadAndUpdateSheetData()
    {
        InitData();
        UpdateStats(UpdateMethod, SheetName);
    }
#endif
}
