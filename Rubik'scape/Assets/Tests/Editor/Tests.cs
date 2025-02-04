using NUnit.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;

public class LevelTests
{
    [Test]
    public void JsonDeserialization_WorksCorrectly()
    {
        string json = @"{
            ""levels"": [
                { ""id"": ""1"", ""name"": ""Test Level"", ""cube_size"": 3, ""faces_data"": { ""face_1"": [1,2,3], ""face_2"": [4,5,6] } }
            ]
        }";

        APIResponse response = JsonConvert.DeserializeObject<APIResponse>(json);
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.levels);
        Assert.AreEqual(1, response.levels.Count);
        Assert.AreEqual("Test Level", response.levels[0].name);
    }
}
