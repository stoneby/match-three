using Assets.Scripts.Common;
using System.Collections.Generic;
using UnityEngine;

// Spawn System:
// Preload GameObject to reuse them later, avoiding to Instantiate them.
// Very useful for mobile platforms.

public class CFX_SpawnSystem : MonoSingleton<CFX_SpawnSystem>
{
    public bool EditorMode;

    /// <summary>
    /// Get the next available preloaded Object.
    /// </summary>
    /// <returns>
    /// The next available preloaded Object.
    /// </returns>
    /// <param name='sourceObj'>
    /// The source Object from which to get a preloaded copy.
    /// </param>
    /// <param name="target">Target transform.</param>
    /// <param name='activateObject'>
    /// Activates the object before returning it.
    /// </param>
    public static GameObject GetNextObject(GameObject sourceObj, Transform target, bool activateObject = true)
    {
        return GetNextObject(sourceObj, target == null ? sourceObj.transform.position : target.position,
            activateObject);
    }

    public static GameObject GetNextObject(GameObject sourceObj, Vector3 targetPosition, bool activateObject = true)
    {
        instance.Initialize();

        int uniqueId = sourceObj.GetInstanceID();

        if (!instance.poolCursors.ContainsKey(uniqueId))
        {
            Debug.LogError("[CFX_SpawnSystem.GetNextPoolObject()] Object hasn't been preloaded: " + sourceObj.name + " (ID:" + uniqueId + ")");
            return null;
        }

        int cursor = instance.poolCursors[uniqueId];
        instance.poolCursors[uniqueId]++;
        if (instance.poolCursors[uniqueId] >= instance.instantiatedObjects[uniqueId].Count)
        {
            instance.poolCursors[uniqueId] = 0;
        }

        var returnObj = instance.instantiatedObjects[uniqueId][cursor];
        returnObj.transform.position = targetPosition;

        if (activateObject)
#if UNITY_3_5
			returnObj.SetActiveRecursively(true);
#else
            returnObj.SetActive(true);
#endif

        return returnObj;
    }

    /// <summary>
    /// Get next object by name.
    /// </summary>
    /// <param name="sourceName">Prefab name.</param>
    /// <param name="activateObject">Active object to use.</param>
    /// <returns></returns>
    public static GameObject GetNextObject(string sourceName, Transform target = null, bool activateObject = true)
    {
        return GetNextObject(sourceName, target == null ? Vector3.zero : target.position, activateObject);
    }

    public static GameObject GetNextObject(string sourceName, Vector3 targetPosition, bool activateObject = true)
    {
        instance.Initialize();

        if (!preloadDict.ContainsKey(sourceName))
        {
            Debug.LogError("[CFX_SpawnSystem.GetNextObject()] No pooling game object with name: " + sourceName);
            return null;
        }
        return GetNextObject(preloadDict[sourceName], targetPosition, activateObject);
    }

    /// <summary>
    /// Preloads an object a number of times in the pool.
    /// </summary>
    /// <param name='sourceObj'>
    /// The source Object.
    /// </param>
    /// <param name='poolSize'>
    /// The number of times it will be instantiated in the pool (i.e. the max number of same object that would appear simultaneously in your Scene).
    /// </param>
    public static void PreloadObject(GameObject sourceObj, int poolSize = 1)
    {
        instance.addObjectToPool(sourceObj, poolSize);
    }

    /// <summary>
    /// Unloads all the preloaded objects from a source Object.
    /// </summary>
    /// <param name='sourceObj'>
    /// Source object.
    /// </param>
    public static void UnloadObjects(GameObject sourceObj)
    {
        instance.removeObjectsFromPool(sourceObj);
    }

    /// <summary>
    /// Gets a value indicating whether all objects defined in the Editor are loaded or not.
    /// </summary>
    /// <value>
    /// <c>true</c> if all objects are loaded; otherwise, <c>false</c>.
    /// </value>
    public static bool AllObjectsLoaded
    {
        get
        {
            return instance.allObjectsLoaded;
        }
    }


    public GameObject[] objectsToPreload = new GameObject[0];
    public int[] objectsToPreloadTimes = new int[0];
    public bool hideObjectsInHierarchy;

    private bool allObjectsLoaded;
    private Dictionary<int, List<GameObject>> instantiatedObjects = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, int> poolCursors = new Dictionary<int, int>();

    private static readonly Dictionary<string, GameObject> preloadDict = new Dictionary<string, GameObject>();

    private void addObjectToPool(GameObject sourceObject, int number)
    {
        int uniqueId = sourceObject.GetInstanceID();

        //Add new entry if it doesn't exist
        if (!instantiatedObjects.ContainsKey(uniqueId))
        {
            instantiatedObjects.Add(uniqueId, new List<GameObject>());
            poolCursors.Add(uniqueId, 0);
        }

        //Add the new objects
        GameObject newObj;
        for (int i = 0; i < number; i++)
        {
            newObj = Instantiate(sourceObject);
            newObj.AddComponent<EffectLayerController>();

#if UNITY_3_5
			newObj.SetActiveRecursively(false);
#else
            newObj.SetActive(false);
#endif

            if (!EditorMode)
                DontDestroyOnLoad(newObj);

            instantiatedObjects[uniqueId].Add(newObj);

            if (hideObjectsInHierarchy)
                newObj.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    private void removeObjectsFromPool(GameObject sourceObject)
    {
        int uniqueId = sourceObject.GetInstanceID();

        if (!instantiatedObjects.ContainsKey(uniqueId))
        {
            Debug.LogWarning("[CFX_SpawnSystem.removeObjectsFromPool()] There aren't any preloaded object for: " + sourceObject.name + " (ID:" + uniqueId + ")");
            return;
        }

        //Destroy all objects
        for (int i = instantiatedObjects[uniqueId].Count - 1; i >= 0; i--)
        {
            GameObject obj = instantiatedObjects[uniqueId][i];
            instantiatedObjects[uniqueId].RemoveAt(i);
            Destroy(obj);
        }

        //Remove pool entry
        instantiatedObjects.Remove(uniqueId);
        poolCursors.Remove(uniqueId);
    }

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (allObjectsLoaded)
            return;

        for (int i = 0; i < objectsToPreload.Length; i++)
        {
            var go = objectsToPreload[i];
            if (go != null)
            {
                PreloadObject(go, objectsToPreloadTimes[i]);

                var key = go.name;
                if (!preloadDict.ContainsKey(key))
                    preloadDict.Add(key, go);
            }
        }

        allObjectsLoaded = true;
    }
}
