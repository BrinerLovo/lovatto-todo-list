using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public class bl_DevNotesInfo
{
    public List<Info> Notes = new List<Info>();
    public List<Info> HistoryNotes = new List<Info>();
    public List<Categorys> AllCategorys = new List<Categorys>() { new Categorys() { Name = "Default", Color = Color.white } };


    [System.Serializable]
    public class Info
    {
        public string Note;
        public string Comment;
        public int CategoryID;
        public string CreateDate;
        public string CompleteDate;

        public NoteType noteType = NoteType.Note;
    }

    [System.Serializable]
    public class Categorys
    {
        public string Name;
        public Color Color;
    }

    [System.Serializable]
    public enum NoteType
    {
        Note = 0,
        Separator,
    }
}