using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupEnemiesScript
{
    [MenuItem("Tools/Setup Enemies & Spawner")]
    public static void SetupEnemies()
    {
        // Encontrar ou criar o GameObject EnemySpawner na cena
        GameObject spawnerObj = GameObject.Find("EnemySpawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("EnemySpawner");
        }

        EnemySpawner spawner = spawnerObj.GetComponent<EnemySpawner>() ?? spawnerObj.AddComponent<EnemySpawner>();

        // Criar ou obter Prefabs Placeholders na pasta Assets/Prefabs (se não existirem)
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject commonGhostPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CommonGhostPrefab.prefab");
        if (commonGhostPrefab == null)
        {
            GameObject tempCommon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempCommon.name = "CommonGhostPrefab";
            Collider col = tempCommon.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Renderer rend = tempCommon.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                rend.sharedMaterial.color = new Color(0.2f, 0.85f, 0.95f, 0.8f); // Ciano translúcido
            }

            GhostEnemy ghostComp = tempCommon.AddComponent<GhostEnemy>();
            SerializedObject ghostSO = new SerializedObject(ghostComp);
            ghostSO.FindProperty("ghostType").enumValueIndex = (int)GhostType.Common;
            ghostSO.FindProperty("maxHealth").intValue = 1;
            ghostSO.FindProperty("moveSpeed").floatValue = 4.0f;
            ghostSO.ApplyModifiedProperties();

            commonGhostPrefab = PrefabUtility.SaveAsPrefabAsset(tempCommon, "Assets/Prefabs/CommonGhostPrefab.prefab");
            Object.DestroyImmediate(tempCommon);
        }

        GameObject rareGhostPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RareGhostPrefab.prefab");
        if (rareGhostPrefab == null)
        {
            GameObject tempRare = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempRare.name = "RareGhostPrefab";
            tempRare.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            Collider col = tempRare.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Renderer rend = tempRare.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                rend.sharedMaterial.color = new Color(0.85f, 0.1f, 0.9f, 0.9f); // Roxo/Magenta vibrante
            }

            GhostEnemy ghostComp = tempRare.AddComponent<GhostEnemy>();
            SerializedObject ghostSO = new SerializedObject(ghostComp);
            ghostSO.FindProperty("ghostType").enumValueIndex = (int)GhostType.Rare;
            ghostSO.FindProperty("maxHealth").intValue = 2;
            ghostSO.FindProperty("moveSpeed").floatValue = 6.0f;
            ghostSO.ApplyModifiedProperties();

            rareGhostPrefab = PrefabUtility.SaveAsPrefabAsset(tempRare, "Assets/Prefabs/RareGhostPrefab.prefab");
            Object.DestroyImmediate(tempRare);
        }

        // Conectar referências no EnemySpawner
        SerializedObject spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("commonGhostPrefab").objectReferenceValue = commonGhostPrefab;
        spawnerSO.FindProperty("rareGhostPrefab").objectReferenceValue = rareGhostPrefab;
        spawnerSO.FindProperty("spawnInterval").floatValue = 4.0f;
        spawnerSO.FindProperty("rareGhostProbability").floatValue = 0.2f; // 20% de chance para fantasmas Raros
        spawnerSO.FindProperty("minDistanceFromPlayer").floatValue = 15.0f; // Distância de segurança
        spawnerSO.FindProperty("minDistanceFromLocomotive").floatValue = 15.0f; // Distância de segurança
        spawnerSO.FindProperty("mapLimitX").floatValue = 100.0f;
        spawnerSO.FindProperty("mapLimitZ").floatValue = 100.0f;
        spawnerSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SetupEnemiesScript] Sistema de Spawner e Prefabs de Fantasmas Comuns/Raros configurados com sucesso!");
    }
}
