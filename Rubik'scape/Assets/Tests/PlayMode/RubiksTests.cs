using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;

namespace RubiksTests
{
    // --- Mocks ---
    public class Mock
    {
        public class RubiksAPIManagerMock : RubiksAPIManager
        {
            public bool getAllLevelsCalled = false;
            public bool getLevelCalled = false;

            public override IEnumerator GetAllLevels()
            {
                getAllLevelsCalled = true;
                var response = new APIResponse
                {
                    levels = new List<Level>
                    {
                        new Level { id = "1", name = "Test Level", cube_size = 3 }
                    }
                };
                dataReceiver?.HandleAllLevels(response.levels);
                yield return null;
            }

            public override IEnumerator GetLevel(string levelId)
            {
                getLevelCalled = true;
                yield return null;
            }
        }

        public class RubikGenMock : RubikGen
        {
            public bool initializeCalled = false;

            public override void InitializeRubiksCube(string levelName, int levelSize, int[][] facesData)
            {
                base.InitializeRubiksCube(levelName, levelSize, facesData);
                initializeCalled = true;
            }
        }
    }

    // --- Classe de Test ---
    public class RubiksTests
    {
        private GameObject testGameObject;
        private FetchCanvas fetchCanvas;
        private Mock.RubiksAPIManagerMock apiManager;
        private Mock.RubikGenMock rubikGen;

        [SetUp]
        public void Setup()
        {
            // Création de l'objet principal de test
            testGameObject = new GameObject("TestGameObject");

            // Ajout des composants nécessaires
            apiManager = testGameObject.AddComponent<Mock.RubiksAPIManagerMock>();
            fetchCanvas = testGameObject.AddComponent<FetchCanvas>();
            rubikGen = testGameObject.AddComponent<Mock.RubikGenMock>();

            // --- Initialisation pour FetchCanvas ---

            // Création d'un ContentContainer (transform d'un GameObject)
            GameObject contentContainerGO = new GameObject("ContentContainer");
            fetchCanvas.ContentContainer = contentContainerGO.transform;

            // Création d'un PanelPrefab "dummy" avec la hiérarchie attendue
            GameObject panelPrefab = new GameObject("PanelPrefab");

            // Création de TitleContainer et LevelName (avec TextMeshProUGUI)
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(panelPrefab.transform);
            GameObject levelName = new GameObject("LevelName");
            levelName.transform.SetParent(titleContainer.transform);
            // Utilisation de TextMeshProUGUI à la place de TMP_Text
            levelName.AddComponent<TextMeshProUGUI>();

            // Création de SizeContainer et LevelSize (avec TextMeshProUGUI)
            GameObject sizeContainer = new GameObject("SizeContainer");
            sizeContainer.transform.SetParent(panelPrefab.transform);
            GameObject levelSize = new GameObject("LevelSize");
            levelSize.transform.SetParent(sizeContainer.transform);
            levelSize.AddComponent<TextMeshProUGUI>();

            // Création de SelectButton et LevelSelectorButton (avec Button)
            GameObject selectButton = new GameObject("SelectButton");
            selectButton.transform.SetParent(panelPrefab.transform);
            GameObject levelSelectorButton = new GameObject("LevelSelectorButton");
            levelSelectorButton.transform.SetParent(selectButton.transform);
            levelSelectorButton.AddComponent<Button>();

            // Assignation du prefab correctement configuré
            fetchCanvas.PanelPrefab = panelPrefab;

            // --- Initialisation pour RubikGen ---

            // Assigner un cubePrefab "dummy" pour éviter l'erreur d'Instantiate dans GenerateRubiksCube
            rubikGen.cubePrefab = new GameObject("CubePrefab");
            // Assigner un tableau minimal pour planePrefabs
            rubikGen.planePrefabs = new GameObject[] { new GameObject("PlanePrefab0") };

            // Configuration des dépendances entre composants
            apiManager.dataReceiver = fetchCanvas;
            fetchCanvas.apiManager = apiManager;
            // (Optionnel) assigner curvedUnityCanvas si nécessaire pour d'autres tests
            fetchCanvas.curvedUnityCanvas = new GameObject("CurvedUnityCanvas");
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(testGameObject);
        }

        [UnityTest]
        public IEnumerator TestGetAllLevels()
        {
            yield return apiManager.GetAllLevels();

            Assert.IsTrue(apiManager.getAllLevelsCalled, "GetAllLevels n'a pas été appelé");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestGetSpecificLevel()
        {
            string testLevelId = "test123";
            yield return apiManager.GetLevel(testLevelId);
            Assert.IsTrue(apiManager.getLevelCalled, "GetLevel n'a pas été appelé");
        }

        [Test]
        public void TestRubikGenProperties()
        {
            Assert.AreEqual(3, rubikGen.cubeSize, "La taille par défaut du cube devrait être 3");
            Assert.AreEqual(1.0f, rubikGen.offset, "L'offset par défaut devrait être 1.0f");
        }

        [UnityTest]
        public IEnumerator TestRubiksCubeInitialization()
        {
            string testLevelName = "Test Level";
            int testSize = 3;
            int[][] testFacesData = new int[6][];
            for (int i = 0; i < 6; i++)
            {
                testFacesData[i] = new int[testSize * testSize];
            }

            rubikGen.InitializeRubiksCube(testLevelName, testSize, testFacesData);
            yield return null;

            Assert.IsTrue(rubikGen.initializeCalled, "InitializeRubiksCube n'a pas été appelé");
            Assert.IsNotNull(rubikGen.cubeArray, "CubeArray ne devrait pas être null après l'initialisation");
            Assert.AreEqual(testSize, rubikGen.cubeArray.GetLength(0), "Dimension X incorrecte");
            Assert.AreEqual(testSize, rubikGen.cubeArray.GetLength(1), "Dimension Y incorrecte");
            Assert.AreEqual(testSize, rubikGen.cubeArray.GetLength(2), "Dimension Z incorrecte");
        }
    }
}
