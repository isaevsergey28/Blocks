using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCubesSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _constructionCubePrefab;

    private Queue<GameObject> _acitiveCube = new Queue<GameObject>();
    private Dictionary<GameObject, ConstructionCubeBehaviour> _allCubes = new Dictionary<GameObject, ConstructionCubeBehaviour>();

    private void Start()
    {
        ConstructionCubeBehaviour.onReachingTarget += ReturnCube;
        SpawnCubes();
    }

    private void SpawnCubes()
    {
        for (int i = 0; i < 500; i++)
        {
            GameObject cube = Instantiate(_constructionCubePrefab, transform.position, Quaternion.identity, transform);
            ConstructionCubeBehaviour cubeScript = cube.GetComponent<ConstructionCubeBehaviour>();
            cube.SetActive(false);
            _allCubes.Add(cube, cubeScript);
            _acitiveCube.Enqueue(cube);
        }
    }

    private void OnDisable()
    {
        ConstructionCubeBehaviour.onReachingTarget -= ReturnCube;
    }

    public void RunCube(Vector3 target, MeshRenderer mesh)
    {
        GameObject cube = _acitiveCube.Dequeue();
        cube.SetActive(true);
        _allCubes[cube].MoveToVoxel(target, mesh);

    }

    private void ReturnCube(GameObject cube)
    {
        cube.transform.position = transform.position;
        cube.SetActive(false);
        _acitiveCube.Enqueue(cube);
    }
}
