using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public string id, name;
    public int cube_size;
    public Dictionary<string, int[]> faces_data;
}

[Serializable]
public class APIResponse
{
    public List<Level> levels;
    public Level level;
}