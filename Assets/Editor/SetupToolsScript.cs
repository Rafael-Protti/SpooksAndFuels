using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupToolsScript
{
    [MenuItem("Tools/Setup 3 Game Tools")]
    public static void SetupTools()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        Vector3 spawnBasePos = player != null ? player.transform.position : Vector3.zero;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Material padrão de ferramentas
        Material metallicMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        metallicMat.color = new Color(0.85f, 0.88f, 0.92f);
        metallicMat.SetFloat("_Metallic", 0.8f);
        metallicMat.SetFloat("_Smoothness", 0.7f);

        Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodMat.color = new Color(0.45f, 0.28f, 0.15f);

        // --- 1. ESPADA (SwordPrefab) ---
        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SwordPrefab.prefab");
        if (swordPrefab == null)
        {
            GameObject swordRoot = new GameObject("SwordPrefab");
            
            // Lâmina
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(swordRoot.transform, false);
            blade.transform.localPosition = new Vector3(0, 0.6f, 0);
            blade.transform.localScale = new Vector3(0.08f, 0.8f, 0.15f);
            blade.GetComponent<Renderer>().sharedMaterial = metallicMat;

            // Guarda/Hilt
            GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hilt.transform.SetParent(swordRoot.transform, false);
            hilt.transform.localPosition = new Vector3(0, 0.2f, 0);
            hilt.transform.localScale = new Vector3(0.35f, 0.06f, 0.1f);
            hilt.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Cabo/Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(swordRoot.transform, false);
            handle.transform.localPosition = new Vector3(0, 0.05f, 0);
            handle.transform.localScale = new Vector3(0.05f, 0.12f, 0.05f);
            handle.GetComponent<Renderer>().sharedMaterial = woodMat;

            BoxCollider col = swordRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.4f, 0);
            col.size = new Vector3(0.5f, 1.0f, 0.5f);
            col.isTrigger = true;

            ToolItem tool = swordRoot.AddComponent<ToolItem>();
            SerializedObject toolSO = new SerializedObject(tool);
            toolSO.FindProperty("toolType").enumValueIndex = (int)ToolType.Sword;
            toolSO.FindProperty("toolName").stringValue = "Espada";
            toolSO.ApplyModifiedProperties();

            swordPrefab = PrefabUtility.SaveAsPrefabAsset(swordRoot, "Assets/Prefabs/SwordPrefab.prefab");
            Object.DestroyImmediate(swordRoot);
        }

        // --- 2. PICARETA (PickaxePrefab) ---
        GameObject pickaxePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PickaxePrefab.prefab");
        if (pickaxePrefab == null)
        {
            GameObject pickRoot = new GameObject("PickaxePrefab");

            // Cabo
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(pickRoot.transform, false);
            handle.transform.localPosition = new Vector3(0, 0.4f, 0);
            handle.transform.localScale = new Vector3(0.06f, 0.45f, 0.06f);
            handle.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Cabeça T da Picareta
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(pickRoot.transform, false);
            head.transform.localPosition = new Vector3(0, 0.8f, 0);
            head.transform.localScale = new Vector3(0.7f, 0.08f, 0.12f);
            head.GetComponent<Renderer>().sharedMaterial = metallicMat;

            BoxCollider col = pickRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.4f, 0);
            col.size = new Vector3(0.8f, 0.9f, 0.5f);
            col.isTrigger = true;

            ToolItem tool = pickRoot.AddComponent<ToolItem>();
            SerializedObject toolSO = new SerializedObject(tool);
            toolSO.FindProperty("toolType").enumValueIndex = (int)ToolType.Pickaxe;
            toolSO.FindProperty("toolName").stringValue = "Picareta";
            toolSO.ApplyModifiedProperties();

            pickaxePrefab = PrefabUtility.SaveAsPrefabAsset(pickRoot, "Assets/Prefabs/PickaxePrefab.prefab");
            Object.DestroyImmediate(pickRoot);
        }

        // --- 3. MACHADO (AxePrefab) ---
        GameObject axePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/AxePrefab.prefab");
        if (axePrefab == null)
        {
            GameObject axeRoot = new GameObject("AxePrefab");

            // Cabo
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(axeRoot.transform, false);
            handle.transform.localPosition = new Vector3(0, 0.4f, 0);
            handle.transform.localScale = new Vector3(0.06f, 0.45f, 0.06f);
            handle.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Cabeça do Machado
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(axeRoot.transform, false);
            head.transform.localPosition = new Vector3(0.18f, 0.75f, 0);
            head.transform.localScale = new Vector3(0.35f, 0.25f, 0.08f);
            head.GetComponent<Renderer>().sharedMaterial = metallicMat;

            BoxCollider col = axeRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0.1f, 0.4f, 0);
            col.size = new Vector3(0.6f, 0.9f, 0.5f);
            col.isTrigger = true;

            ToolItem tool = axeRoot.AddComponent<ToolItem>();
            SerializedObject toolSO = new SerializedObject(tool);
            toolSO.FindProperty("toolType").enumValueIndex = (int)ToolType.Axe;
            toolSO.FindProperty("toolName").stringValue = "Machado";
            toolSO.ApplyModifiedProperties();

            axePrefab = PrefabUtility.SaveAsPrefabAsset(axeRoot, "Assets/Prefabs/AxePrefab.prefab");
            Object.DestroyImmediate(axeRoot);
        }

        // --- INSTANCIAR FERRAMENTAS NA CENA PRÓXIMAS AO JOGADOR ---
        GameObject oldSword = GameObject.Find("SwordItem_Scene");
        if (oldSword == null)
        {
            GameObject swordObj = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab);
            swordObj.name = "SwordItem_Scene";
            swordObj.transform.position = spawnBasePos + new Vector3(-3.0f, 0.3f, 2.0f);
        }

        GameObject oldPick = GameObject.Find("PickaxeItem_Scene");
        if (oldPick == null)
        {
            GameObject pickObj = (GameObject)PrefabUtility.InstantiatePrefab(pickaxePrefab);
            pickObj.name = "PickaxeItem_Scene";
            pickObj.transform.position = spawnBasePos + new Vector3(-1.5f, 0.3f, 2.0f);
        }

        GameObject oldAxe = GameObject.Find("AxeItem_Scene");
        if (oldAxe == null)
        {
            GameObject axeObj = (GameObject)PrefabUtility.InstantiatePrefab(axePrefab);
            axeObj.name = "AxeItem_Scene";
            axeObj.transform.position = spawnBasePos + new Vector3(0.0f, 0.3f, 2.0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SetupToolsScript] As 3 Ferramentas (Espada, Picareta e Machado) foram criadas e adicionadas à cena com sucesso!");
    }
}
