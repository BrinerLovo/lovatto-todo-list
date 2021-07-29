using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class bl_DevNotesCategorys : EditorWindow
{
    private bl_DevNotesInfo Notes;
    public bl_DevNoteList ListManager;
    private bool openAdd = false;
    private string CatAdd = "";
    private Color ColorAdd = new Color(1, 1, 1, 1);

    private void OnEnable()
    {
        titleContent = new GUIContent("Category");
    }
    public void SetNotes(bl_DevNotesInfo not, bl_DevNoteList manager)
    {
        Notes = not;
        ListManager = manager;
    }

    private void OnGUI()
    {
        if (Notes == null)
        {
            GUILayout.Label("No notes!");
            return;
        }
  

        if (Notes.AllCategorys != null && Notes.AllCategorys.Count > 0)
        {
            GUILayout.BeginVertical("box");
            for (int i = 0; i < Notes.AllCategorys.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Notes.AllCategorys[i].Name = GUILayout.TextField(Notes.AllCategorys[i].Name);
                Notes.AllCategorys[i].Color = EditorGUILayout.ColorField(Notes.AllCategorys[i].Color,GUILayout.Width(100));
                if (GUILayout.Button("✓", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                {
                    UpdateText();
                }
                if (GUILayout.Button("✘", EditorStyles.wordWrappedLabel, GUILayout.Width(16)))
                {
                    Notes.AllCategorys.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.BeginVertical("box");
        if (openAdd)
        {
            GUILayout.BeginHorizontal();
            CatAdd = GUILayout.TextField(CatAdd);
            ColorAdd = EditorGUILayout.ColorField(ColorAdd);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add"))
            {
                bl_DevNotesInfo.Categorys cat = new bl_DevNotesInfo.Categorys();
                cat.Name = CatAdd;
                cat.Color = ColorAdd;
                Notes.AllCategorys.Add(cat);
                openAdd = false;
                CatAdd = string.Empty;
                ColorAdd = Color.white;
                UpdateText();
            }
        }
        else
        {
            if (GUILayout.Button("Add New Category"))
            {
                openAdd = true;
            }
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateText()
    {
        ListManager.settings.SaveSettings();
    }
}