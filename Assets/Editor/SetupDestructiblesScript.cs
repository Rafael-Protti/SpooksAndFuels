using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupDestructiblesScript
{
    [MenuItem("Tools/Setup Destructible Objects")]
    public static void SetupDestructibles()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        Vector3 basePos = player != null ? player.transform.position : Vector3.zero;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // --- MATERIAL DA PEDRA ---
        Material rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        rockMat.color = new Color(0.48f, 0.46f, 0.44f);
        rockMat.SetFloat("_Smoothness", 0.15f);

        // --- MATERIAL DA CAIXA ---
        Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodMat.color = new Color(0.55f, 0.35f, 0.15f);
        woodMat.SetFloat("_Smoothness", 0.2f);

        Material woodDarkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodDarkMat.color = new Color(0.35f, 0.2f, 0.08f);


        // =====================================================
        // PEDRA (RockPrefab)
        // =====================================================
        GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RockPrefab.prefab");
        if (rockPrefab == null)
        {
            GameObject rockRoot = new GameObject("RockPrefab");

            // Corpo principal da pedra (bloco irregular)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(rockRoot.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.0f, 0.75f, 0.9f);
            body.transform.localRotation = Quaternion.Euler(0, 15f, 5f);
            body.GetComponent<Renderer>().sharedMaterial = rockMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Pedaço superior
            GameObject topChunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topChunk.transform.SetParent(rockRoot.transform, false);
            topChunk.transform.localPosition = new Vector3(0.1f, 0.45f, -0.05f);
            topChunk.transform.localScale = new Vector3(0.55f, 0.4f, 0.55f);
            topChunk.transform.localRotation = Quaternion.Euler(10f, -10f, 8f);
            topChunk.GetComponent<Renderer>().sharedMaterial = rockMat;
            Object.DestroyImmediate(topChunk.GetComponent<Collider>());

            // Colider único unificado
            BoxCollider col = rockRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.3f, 0);
            col.size = new Vector3(1.2f, 1.0f, 1.1f);

            DestructibleObject destructible = rockRoot.AddComponent<DestructibleObject>();
            SerializedObject so = new SerializedObject(destructible);
            so.FindProperty("objectType").enumValueIndex = (int)DestructibleType.Rock;
            so.FindProperty("health").intValue = 3;
            so.ApplyModifiedProperties();

            rockPrefab = PrefabUtility.SaveAsPrefabAsset(rockRoot, "Assets/Prefabs/RockPrefab.prefab");
            Object.DestroyImmediate(rockRoot);
        }


        // =====================================================
        // CAIXA (CratePrefab)
        // =====================================================
        GameObject cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CratePrefab.prefab");
        if (cratePrefab == null)
        {
            GameObject crateRoot = new GameObject("CratePrefab");

            // Corpo principal da caixa
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(crateRoot.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            body.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Tábua horizontal frente/costas
            foreach (float z in new float[] { 0.45f, -0.45f })
            {
                GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.transform.SetParent(crateRoot.transform, false);
                plank.transform.localPosition = new Vector3(0f, 0f, z);
                plank.transform.localScale = new Vector3(0.92f, 0.08f, 0.02f);
                plank.GetComponent<Renderer>().sharedMaterial = woodDarkMat;
                Object.DestroyImmediate(plank.GetComponent<Collider>());

                // Tábua no meio
                GameObject plankMid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plankMid.transform.SetParent(crateRoot.transform, false);
                plankMid.transform.localPosition = new Vector3(0f, 0.3f, z);
                plankMid.transform.localScale = new Vector3(0.92f, 0.06f, 0.02f);
                plankMid.GetComponent<Renderer>().sharedMaterial = woodDarkMat;
                Object.DestroyImmediate(plankMid.GetComponent<Collider>());
            }

            // Colider único
            BoxCollider col = crateRoot.AddComponent<BoxCollider>();
            col.center = Vector3.zero;
            col.size = new Vector3(1.0f, 1.0f, 1.0f);

            DestructibleObject destructible = crateRoot.AddComponent<DestructibleObject>();
            SerializedObject so = new SerializedObject(destructible);
            so.FindProperty("objectType").enumValueIndex = (int)DestructibleType.Crate;
            so.FindProperty("health").intValue = 3;
            so.ApplyModifiedProperties();

            cratePrefab = PrefabUtility.SaveAsPrefabAsset(crateRoot, "Assets/Prefabs/CratePrefab.prefab");
            Object.DestroyImmediate(crateRoot);
        }


        // =====================================================
        // INSTANCIAR NA CENA PRÓXIMOS AO JOGADOR
        // =====================================================
        if (GameObject.Find("RockInstance_Scene") == null)
        {
            GameObject rockObj = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
            rockObj.name = "RockInstance_Scene";
            rockObj.transform.position = basePos + new Vector3(3.5f, 0.4f, 3.0f);
        }

        if (GameObject.Find("CrateInstance_Scene") == null)
        {
            GameObject crateObj = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crateObj.name = "CrateInstance_Scene";
            crateObj.transform.position = basePos + new Vector3(-3.5f, 0.45f, 3.0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SetupDestructiblesScript] Pedra e Caixa criadas com prefabs e adicionadas à cena com sucesso!");
    }
}
