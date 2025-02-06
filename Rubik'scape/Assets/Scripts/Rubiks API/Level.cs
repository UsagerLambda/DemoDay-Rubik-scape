using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable] // convertie le JSON reçu en Object
public class APIResponse
{
    [JsonProperty("levels")]
    public List<Level> levels; // Liste des niveaux reçus depuis l'API
    [JsonProperty("level")]
    public Level level; // Niveau unique reçu depuis l'API
}

[Serializable] // convertie le JSON reçu en Object
public class Level
{
    [JsonProperty("id")]
    public string id;
    [JsonProperty("name")]
    public string name;
    [JsonProperty("cube_size")]
    public int cube_size;
    [JsonProperty("faces_data")]
    public Dictionary<string, int[]> faces_data;
}
