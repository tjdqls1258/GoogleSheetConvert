#if UNITY_EDITOR
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Unity.Android.Gradle.Manifest;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class GoogleSheetReder
{
    private static readonly string ClientName = "user";
    private static readonly string dataPath = $"{UnityEngine.Application.dataPath.Replace("Assets", "")} Youer Google client_secret.json".Replace(" ","");

    public static SheetsService CreateService()
    {
        var scopes = new string[] { SheetsService.Scope.SpreadsheetsReadonly };
        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //pass, 
            GoogleClientSecrets.FromFile($"{dataPath}").Secrets,
            scopes,
            ClientName,
            CancellationToken.None).Result;

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
        });

        return service;
    }

    public static void TestCSV02(UnityAction<IList<IList<object>>> callback, string ID, string Name)
    {
        CreateService_CSVObject(CreateService(), callback, Name, ID);
    }

    private static void CreateService_CSVObject(SheetsService service, UnityAction<IList<IList<object>>> callback, string ID, string Name)
    {
        ValueRange value;
        try
        {
            value = service.Spreadsheets.Values.Get(Name, ID).Execute();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{Name} request Error : {ex.Message}");
            return;
        }

        callback.Invoke(value.Values);
    }

    private static void CreateService_CSV(SheetsService service, string sheetName, string spreadSheetID)
    {
        ValueRange value;
        try
        {
            value = service.Spreadsheets.Values.Get(spreadSheetID, sheetName).Execute();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{sheetName} request Error : {ex.Message}");
            return;
        }

        var values = value.Values;
        int columnCount = values[0].Count;
        List<int> ignoreIndex = new();
        for (int i = 0; i < values[0].Count; i++)
        {
            string title = values[0][i].ToString();
            if (string.IsNullOrEmpty(values[1][i].ToString()))
            {
                continue;
            }
            if (title.IndexOf("(") != -1 || title.IndexOf(")") != -1 ||
                title.IndexOf("[") != -1 || title.IndexOf("]") != -1)
            {
                ignoreIndex.Add(i);
            }
        }

        StringBuilder csvInfo = new StringBuilder();
        for (int x = 1; x < values.Count; x++) //Çà
        {
            if (x != 1)
                csvInfo.AppendLine();
            for (int y = 0; y < columnCount; y++) //¿­
            {
                if (ignoreIndex.Exists(i => i == y)) continue;

                if (y != 0)
                {
                    csvInfo.Append(",");
                }
                if (values[x].Count <= y)
                    continue;

                string str = values[x][y].ToString();

                if (str.IndexOf(",") != -1)
                {
                    csvInfo.Append("\"");
                    csvInfo.Append(str);
                    csvInfo.Append("\"");
                }
                else
                {
                    csvInfo.Append(str);
                }
            }
        }
        Debug.Log(csvInfo);
        //Set CSV csvInfo
    }
}
#endif
