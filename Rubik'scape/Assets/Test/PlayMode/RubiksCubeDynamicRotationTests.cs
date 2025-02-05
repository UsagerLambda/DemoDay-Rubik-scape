using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class RubikRotationTests : MonoBehaviour
{
    private GameObject rubikObject;
    private RubikRotation rubikRotation;
    private RubikGen rubikGen;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Création du Rubik's cube
        rubikObject = new GameObject("RubikCube");
        rubikRotation = rubikObject.AddComponent<RubikRotation>();
        rubikGen = rubikObject.AddComponent<RubikGen>();

        // Initialisation du cube
        yield return new WaitForSeconds(0.1f);
        rubikGen.GenerateRubiksCube();
        
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(rubikObject);
    }

    [UnityTest]
    public IEnumerator TestRightFaceRotation()
    {
        yield return TestFaceRotation(0, true, Vector3.right);
    }

    [UnityTest]
    public IEnumerator TestLeftFaceRotation()
    {
        yield return TestFaceRotation(1, true, Vector3.left);
    }

    [UnityTest]
    public IEnumerator TestTopFaceRotation()
    {
        yield return TestFaceRotation(2, true, Vector3.up);
    }

    [UnityTest]
    public IEnumerator TestBottomFaceRotation()
    {
        yield return TestFaceRotation(3, true, Vector3.down);
    }

    [UnityTest]
    public IEnumerator TestFrontFaceRotation()
    {
        yield return TestFaceRotation(4, true, Vector3.forward);
    }

    [UnityTest]
    public IEnumerator TestBackFaceRotation()
    {
        yield return TestFaceRotation(5, true, Vector3.back);
    }

    [UnityTest]
    public IEnumerator TestAntiClockwiseRotations()
    {
        for (int i = 0; i < 6; i++)
        {
            yield return TestFaceRotation(i, false, GetFaceNormal(i));
            yield return new WaitForSeconds(0.2f);
        }
    }

    [UnityTest]
    public IEnumerator TestMultipleRotationsSequence()
    {
        int[] rotationSequence = { 0, 2, 4, 1, 3, 5 };
        bool[] directions = { true, false, true, false, true, false };

        for (int i = 0; i < rotationSequence.Length; i++)
        {
            yield return TestFaceRotation(rotationSequence[i], directions[i], GetFaceNormal(rotationSequence[i]));
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator TestFaceRotation(int faceIndex, bool clockwise, Vector3 faceNormal)
    {
        // Sauvegarde de l'état initial
        var initialState = CaptureCurrentState(faceIndex);

        // Simulation de la rotation en appelant directement la méthode RotateFace
        var rotationMethod = rubikRotation.GetType().GetMethod("RotateFace", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (rotationMethod != null)
        {
            object[] parameters = new object[] { faceIndex, clockwise };
            IEnumerator rotationCoroutine = (IEnumerator)rotationMethod.Invoke(rubikRotation, parameters);
            
            while (rotationCoroutine.MoveNext())
            {
                yield return rotationCoroutine.Current;
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Vérification des changements
        VerifyRotationResults(faceIndex, initialState);
    }

    private GameObject[] CaptureCurrentState(int faceIndex)
    {
        var getFaceCubesMethod = rubikRotation.GetType().GetMethod("GetFaceCubes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (getFaceCubesMethod != null)
        {
            return (GameObject[])getFaceCubesMethod.Invoke(rubikRotation, new object[] { faceIndex });
        }
        
        return new GameObject[0];
    }

    private void VerifyRotationResults(int faceIndex, GameObject[] initialState)
    {
        GameObject[] currentState = CaptureCurrentState(faceIndex);
        
        // Vérifie que nous avons bien des cubes à comparer
        Assert.IsTrue(initialState.Length > 0, "La face initiale devrait contenir des cubes");
        Assert.IsTrue(currentState.Length > 0, "La face actuelle devrait contenir des cubes");
        
        // Vérifie que le nombre de cubes reste constant
        Assert.AreEqual(initialState.Length, currentState.Length, 
            $"Le nombre de cubes sur la face {faceIndex} doit rester constant");

        // Vérifie que les positions ont changé
        bool positionsChanged = false;
        for (int i = 0; i < currentState.Length; i++)
        {
            if (Vector3.Distance(currentState[i].transform.position, initialState[i].transform.position) > 0.01f)
            {
                positionsChanged = true;
                break;
            }
        }
        
        Assert.IsTrue(positionsChanged, 
            $"Les positions des cubes de la face {faceIndex} doivent changer après la rotation");
    }

    private Vector3 GetFaceNormal(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: return Vector3.right;
            case 1: return Vector3.left;
            case 2: return Vector3.up;
            case 3: return Vector3.down;
            case 4: return Vector3.forward;
            case 5: return Vector3.back;
            default: return Vector3.zero;
        }
    }
}