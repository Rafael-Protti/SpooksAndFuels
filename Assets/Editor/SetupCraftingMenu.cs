using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SetupCraftingMenu
{
    [MenuItem("SpooksAndFuels/Gerar Estacoes de Crafting")]
    public static void GenerateStations()
    {
        GameObject player = GameObject.FindObjectOfType<PlayerController>()?.gameObject;
        Vector3 spawnPos = player != null ? player.transform.position + Vector3.forward * 3 : Vector3.zero;

        GameObject container = new GameObject("Crafting Stations");
        container.transform.position = spawnPos;

        // 1. Locomotive (Vagão 0)
        CreateStation(container.transform, "Crafting_Locomotiva", CraftingStation.StationType.Locomotive, new Vector3(-3, 0, 0), new List<CraftingStation.UpgradeTier>
        {
            new CraftingStation.UpgradeTier { upgradeName = "Craft Locomotiva", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Locomotiva 1", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 10 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Locomotiva 2", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 15 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 15 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Locomotiva 3", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 20 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Iron, amount = 20 } } }
        });

        // 2. Broom (Vagão 1)
        CreateStation(container.transform, "Crafting_Vassoura", CraftingStation.StationType.Broom, new Vector3(-1, 0, 0), new List<CraftingStation.UpgradeTier>
        {
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Vassoura 1", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 10 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Vassoura 2", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 20 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Vassoura 3", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 30 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Iron, amount = 10 } } }
        });

        // 3. Axe (Vagão 2)
        CreateStation(container.transform, "Crafting_Machado", CraftingStation.StationType.Axe, new Vector3(1, 0, 0), new List<CraftingStation.UpgradeTier>
        {
            new CraftingStation.UpgradeTier { upgradeName = "Craft Machado", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 5 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Machado 1", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 10 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Machado 2", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 20 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Machado 3", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 30 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Iron, amount = 10 } } }
        });

        // 4. Pickaxe (Vagão 3)
        CreateStation(container.transform, "Crafting_Picareta", CraftingStation.StationType.Pickaxe, new Vector3(3, 0, 0), new List<CraftingStation.UpgradeTier>
        {
            new CraftingStation.UpgradeTier { upgradeName = "Craft Picareta", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 5 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 5 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Picareta 1", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 10 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Ectoplasm, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Picareta 2", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 20 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Wood, amount = 10 } } },
            new CraftingStation.UpgradeTier { upgradeName = "Upgrade Picareta 3", requirements = new List<CraftingStation.ResourceRequirement> { new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Stone, amount = 30 }, new CraftingStation.ResourceRequirement { itemType = PlayerItems.ItemType.Iron, amount = 10 } } }
        });

        // Mark scene as dirty so it can be saved
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        Debug.Log("Estações de Crafting geradas com sucesso perto do jogador!");
    }

    private static void CreateStation(Transform parent, string name, CraftingStation.StationType type, Vector3 localPos, List<CraftingStation.UpgradeTier> upgrades)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1, 1, 1);
        col.isTrigger = false;

        CraftingStation station = go.AddComponent<CraftingStation>();
        station.stationType = type;
        station.upgrades = upgrades;
        
        // Cubo visual provisório
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(go.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        GameObject.DestroyImmediate(cube.GetComponent<Collider>());
    }
}
