using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using static AppsConfigurator;

[CustomEditor(typeof (AppsConfigurator))]
public class CustomListFormEditor : Editor
{
    AppsConfigurator t;

    SerializedObject GetTarget;

    SerializedProperty ThisList;

    bool showPosition = true;

    List<string> configs = new List<string>();

    int selectedFouldout = 0;

    async void OnEnable()
    {
        t = (AppsConfigurator) target;

        GetTarget = new SerializedObject(t);
        ThisList = GetTarget.FindProperty("listOfConfigs"); // Find the List in our script and create a refrence of it
        configs =
            t
                .listOfConfigs
                .Select(item =>
                {
                    if (item.AppName == "")
                    {
                        return "No Name";
                    }

                    return item.AppName;
                })
                .ToList();
    }

    public override async void OnInspectorGUI()
    {
        GetTarget.Update();
        SerializedProperty ChoosedConfig = GetTarget.FindProperty("appConfig");

        //Configurations List
        EditorGUILayout.Space();

        for (int i = 0; i < ThisList.arraySize; i++)
        {
            SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
            SerializedProperty AppName =
                MyListRef.FindPropertyRelative("AppName");

            SerializedProperty CompanyName =
                MyListRef.FindPropertyRelative("CompanyName");
            SerializedProperty BuildVersion =
                MyListRef.FindPropertyRelative("BuildVersion");
            SerializedProperty BundleVersion =
                MyListRef.FindPropertyRelative("BundleVersion");
            SerializedProperty PackageName =
                MyListRef.FindPropertyRelative("PackageName");
            SerializedProperty IosCameraDescription =
                MyListRef.FindPropertyRelative("IosCameraDescription");
            SerializedProperty IosLocationDescription =
                MyListRef.FindPropertyRelative("IosLocationDescription");

            string groupName =
                t.listOfConfigs[i].AppName == ""
                    ? "Configuration " + (i + 1)
                    : t.listOfConfigs[i].AppName;

            showPosition =
                EditorGUILayout
                    .BeginFoldoutHeaderGroup(selectedFouldout == i, groupName);

            if (showPosition)
            {
                EditorGUILayout.PropertyField (CompanyName);
                EditorGUILayout.PropertyField (AppName);
                EditorGUILayout.PropertyField (BuildVersion);
                EditorGUILayout.PropertyField (BundleVersion);
                EditorGUILayout.PropertyField (PackageName);
                EditorGUILayout.PropertyField (IosCameraDescription);
                EditorGUILayout.PropertyField (IosLocationDescription);
                EditorGUILayout.Space();
                GUILayout.Label("Api settings");
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (i >= 3)
                {
                    if (
                        GUILayout
                            .Button("Remove",
                            EditorStyles.miniButtonRight,
                            GUILayout.Width(70))
                    )
                    {
                        t.listOfConfigs.RemoveAt (i);
                        UpdateConfigsList();
                    }
                }

                EditorGUILayout.EndHorizontal();

                selectedFouldout = i;
            }
            else
            {
                showPosition = false;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        //TODO: Find a way how to set this parameter for NavApi, for now this file is used only for building purpose.
        // GUILayout.Label("Dev Api");
        // SerializedProperty IsDev = GetTarget.FindProperty("isDev");
        // EditorGUILayout.PropertyField (IsDev);
        GUILayout.Label("Add Configuration");
        if (GUILayout.Button("Add"))
        {
            t.listOfConfigs.Add(new BaseAppConfig());
            UpdateConfigsList();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Save Configurations  List"))
        {
            UpdateConfigsList();
            GetTarget.Update();
        }

        GUILayout.Label("Choose Configuration");
        EditorGUILayout.Space();
        t.choosedConfig =
            EditorGUILayout.Popup(t.choosedConfig, configs.ToArray());

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Apply choosed Configuration");
        if (GUILayout.Button("Apply", GUILayout.Width(150)))
        {
            t.listOfConfigs[t.choosedConfig].ChangeConfiguration();
        }

        GetTarget.ApplyModifiedProperties();
    }

    public void UpdateConfigsList()
    {
        configs.Clear();
        configs =
            t
                .listOfConfigs
                .Select(item =>
                {
                    if (item.AppName == "")
                    {
                        return "No Name";
                    }

                    return item.AppName;
                })
                .ToList();
    }
}
