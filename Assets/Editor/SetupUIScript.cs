using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupUIScript
{
    [MenuItem("Tools/Setup UI Elements")]
    public static void SetupUI()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        LocomotiveController loco = Object.FindAnyObjectByType<LocomotiveController>();

        if (player == null || loco == null)
        {
            Debug.LogError("PlayerController ou LocomotiveController não encontrados na cena!");
            return;
        }

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- DELETAR OBJETOS ANTIGOS DE UI SE EXISTIREM PARA LIMPEZA ---
        GameObject oldHUD = GameObject.Find("PlayerHUDCanvas");
        if (oldHUD != null) Object.DestroyImmediate(oldHUD);

        GameObject oldLocoWorld = GameObject.Find("LocomotiveWorldCanvas");
        if (oldLocoWorld != null) Object.DestroyImmediate(oldLocoWorld);

        GameObject oldPrompt = GameObject.Find("InteractionPromptCanvas");
        if (oldPrompt != null) Object.DestroyImmediate(oldPrompt);

        // --- 1. PLAYER HUD CANVAS (Screen Space Overlay - Canto Superior Esquerdo) ---
        GameObject hudCanvasObj = new GameObject("PlayerHUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PlayerHUDUI));
        
        Canvas hudCanvas = hudCanvasObj.GetComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = hudCanvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        PlayerHUDUI playerHUD = hudCanvasObj.GetComponent<PlayerHUDUI>();

        GameObject healthPanelObj = new GameObject("HealthPanel", typeof(RectTransform), typeof(Image));
        healthPanelObj.transform.SetParent(hudCanvasObj.transform, false);

        RectTransform hpRect = healthPanelObj.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0, 1);
        hpRect.anchorMax = new Vector2(0, 1);
        hpRect.pivot = new Vector2(0, 1);
        hpRect.anchoredPosition = new Vector2(30, -30);
        hpRect.sizeDelta = new Vector2(280, 65);

        Image hpBg = healthPanelObj.GetComponent<Image>();
        hpBg.color = new Color(0.08f, 0.1f, 0.15f, 0.85f);

        GameObject heartObj = new GameObject("HeartIcon", typeof(RectTransform), typeof(Image));
        heartObj.transform.SetParent(healthPanelObj.transform, false);
        RectTransform heartRect = heartObj.GetComponent<RectTransform>();
        heartRect.anchorMin = new Vector2(0, 0.5f);
        heartRect.anchorMax = new Vector2(0, 0.5f);
        heartRect.pivot = new Vector2(0, 0.5f);
        heartRect.anchoredPosition = new Vector2(28, 0);
        heartRect.sizeDelta = new Vector2(40, 40);
        Image heartImg = heartObj.GetComponent<Image>();
        heartImg.color = new Color(0.95f, 0.25f, 0.35f);

        GameObject barBgObj = new GameObject("HealthBarBG", typeof(RectTransform), typeof(Image));
        barBgObj.transform.SetParent(healthPanelObj.transform, false);
        RectTransform barBgRect = barBgObj.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0);
        barBgRect.anchorMax = new Vector2(0, 0);
        barBgRect.pivot = new Vector2(0, 0);
        barBgRect.anchoredPosition = new Vector2(58, 12);
        barBgRect.sizeDelta = new Vector2(205, 18);
        Image barBgImg = barBgObj.GetComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        GameObject barFillObj = new GameObject("HealthBarFill", typeof(RectTransform), typeof(Image));
        barFillObj.transform.SetParent(barBgObj.transform, false);
        RectTransform barFillRect = barFillObj.GetComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.sizeDelta = Vector2.zero;
        Image barFillImg = barFillObj.GetComponent<Image>();
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 1.0f;
        barFillImg.color = new Color(0.9f, 0.2f, 0.3f);

        GameObject textObj = new GameObject("HealthText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(healthPanelObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(58, -8);
        textRect.sizeDelta = new Vector2(-65, 24);
        Text hpText = textObj.GetComponent<Text>();
        if (defaultFont != null) hpText.font = defaultFont;
        hpText.fontSize = 18;
        hpText.fontStyle = FontStyle.Bold;
        hpText.color = Color.white;
        hpText.alignment = TextAnchor.MiddleLeft;
        hpText.text = "10 / 10";

        SerializedObject hudSO = new SerializedObject(playerHUD);
        hudSO.FindProperty("playerController").objectReferenceValue = player;
        hudSO.FindProperty("healthFillBar").objectReferenceValue = barFillImg;
        hudSO.FindProperty("healthText").objectReferenceValue = hpText;
        hudSO.ApplyModifiedProperties();


        // --- 2. LOCOMOTIVE WORLD-SPACE CANVAS (Vida Acima, Combustível Intermediário) ---
        GameObject locoCanvasObj = new GameObject("LocomotiveWorldCanvas", typeof(RectTransform), typeof(Canvas), typeof(LocomotiveUI));
        locoCanvasObj.transform.SetParent(loco.transform, false);

        Canvas locoCanvas = locoCanvasObj.GetComponent<Canvas>();
        locoCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform locoCanvasRect = locoCanvasObj.GetComponent<RectTransform>();
        locoCanvasRect.sizeDelta = new Vector2(300, 140);
        locoCanvasRect.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        LocomotiveUI locoUI = locoCanvasObj.GetComponent<LocomotiveUI>();

        // Barra de Vida da Locomotiva (Mais Acima)
        GameObject locoHpPanel = new GameObject("LocoHealthPanel", typeof(RectTransform), typeof(Image));
        locoHpPanel.transform.SetParent(locoCanvasObj.transform, false);
        RectTransform locoHpRect = locoHpPanel.GetComponent<RectTransform>();
        locoHpRect.anchoredPosition = new Vector2(0, 35);
        locoHpRect.sizeDelta = new Vector2(260, 42);
        Image locoHpBg = locoHpPanel.GetComponent<Image>();
        locoHpBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

        GameObject locoHpFillObj = new GameObject("LocoHealthFill", typeof(RectTransform), typeof(Image));
        locoHpFillObj.transform.SetParent(locoHpPanel.transform, false);
        RectTransform locoHpFillRect = locoHpFillObj.GetComponent<RectTransform>();
        locoHpFillRect.anchorMin = Vector2.zero;
        locoHpFillRect.anchorMax = Vector2.one;
        locoHpFillRect.sizeDelta = Vector2.zero;
        Image locoHpFillImg = locoHpFillObj.GetComponent<Image>();
        locoHpFillImg.type = Image.Type.Filled;
        locoHpFillImg.fillMethod = Image.FillMethod.Horizontal;
        locoHpFillImg.fillAmount = 1.0f;
        locoHpFillImg.color = new Color(0.85f, 0.2f, 0.2f);

        GameObject locoHpTextObj = new GameObject("LocoHealthText", typeof(RectTransform), typeof(Text));
        locoHpTextObj.transform.SetParent(locoHpPanel.transform, false);
        RectTransform locoHpTextRect = locoHpTextObj.GetComponent<RectTransform>();
        locoHpTextRect.anchorMin = Vector2.zero;
        locoHpTextRect.anchorMax = Vector2.one;
        locoHpTextRect.sizeDelta = Vector2.zero;
        Text locoHpText = locoHpTextObj.GetComponent<Text>();
        if (defaultFont != null) locoHpText.font = defaultFont;
        locoHpText.fontSize = 20;
        locoHpText.fontStyle = FontStyle.Bold;
        locoHpText.color = Color.white;
        locoHpText.alignment = TextAnchor.MiddleCenter;
        locoHpText.text = "VIDA 5 / 5";

        // Barra de Combustível da Locomotiva (Entre a Locomotiva e a Barra de Vida)
        GameObject locoFuelPanel = new GameObject("LocoFuelPanel", typeof(RectTransform), typeof(Image));
        locoFuelPanel.transform.SetParent(locoCanvasObj.transform, false);
        RectTransform locoFuelRect = locoFuelPanel.GetComponent<RectTransform>();
        locoFuelRect.anchoredPosition = new Vector2(0, -20);
        locoFuelRect.sizeDelta = new Vector2(260, 42);
        Image locoFuelBg = locoFuelPanel.GetComponent<Image>();
        locoFuelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

        GameObject locoFuelFillObj = new GameObject("LocoFuelFill", typeof(RectTransform), typeof(Image));
        locoFuelFillObj.transform.SetParent(locoFuelPanel.transform, false);
        RectTransform locoFuelFillRect = locoFuelFillObj.GetComponent<RectTransform>();
        locoFuelFillRect.anchorMin = Vector2.zero;
        locoFuelFillRect.anchorMax = Vector2.one;
        locoFuelFillRect.sizeDelta = Vector2.zero;
        Image locoFuelFillImg = locoFuelFillObj.GetComponent<Image>();
        locoFuelFillImg.type = Image.Type.Filled;
        locoFuelFillImg.fillMethod = Image.FillMethod.Horizontal;
        locoFuelFillImg.fillAmount = 1.0f;
        locoFuelFillImg.color = new Color(0.95f, 0.65f, 0.1f);

        GameObject locoFuelTextObj = new GameObject("LocoFuelText", typeof(RectTransform), typeof(Text));
        locoFuelTextObj.transform.SetParent(locoFuelPanel.transform, false);
        RectTransform locoFuelTextRect = locoFuelTextObj.GetComponent<RectTransform>();
        locoFuelTextRect.anchorMin = Vector2.zero;
        locoFuelTextRect.anchorMax = Vector2.one;
        locoFuelTextRect.sizeDelta = Vector2.zero;
        Text locoFuelText = locoFuelTextObj.GetComponent<Text>();
        if (defaultFont != null) locoFuelText.font = defaultFont;
        locoFuelText.fontSize = 20;
        locoFuelText.fontStyle = FontStyle.Bold;
        locoFuelText.color = Color.white;
        locoFuelText.alignment = TextAnchor.MiddleCenter;
        locoFuelText.text = "COMBUSTÍVEL 100 / 100";

        SerializedObject locoSO = new SerializedObject(locoUI);
        locoSO.FindProperty("locomotive").objectReferenceValue = loco;
        locoSO.FindProperty("heightOffset").floatValue = 3.8f;
        locoSO.FindProperty("healthFillBar").objectReferenceValue = locoHpFillImg;
        locoSO.FindProperty("healthText").objectReferenceValue = locoHpText;
        locoSO.FindProperty("fuelFillBar").objectReferenceValue = locoFuelFillImg;
        locoSO.FindProperty("fuelText").objectReferenceValue = locoFuelText;
        locoSO.ApplyModifiedProperties();


        // --- 3. INTERACTION PROMPT WORLD CANVAS (Ícone E/Y + Texto Ação) ---
        GameObject promptCanvasObj = new GameObject("InteractionPromptCanvas", typeof(RectTransform), typeof(Canvas), typeof(InteractionPromptUI));
        Canvas promptCanvas = promptCanvasObj.GetComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform promptCanvasRect = promptCanvasObj.GetComponent<RectTransform>();
        promptCanvasRect.sizeDelta = new Vector2(180, 140);
        promptCanvasRect.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        InteractionPromptUI promptUI = promptCanvasObj.GetComponent<InteractionPromptUI>();

        GameObject containerObj = new GameObject("PromptContainer", typeof(RectTransform));
        containerObj.transform.SetParent(promptCanvasObj.transform, false);
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;

        GameObject btnBoxObj = new GameObject("ButtonBox", typeof(RectTransform), typeof(Image));
        btnBoxObj.transform.SetParent(containerObj.transform, false);
        RectTransform btnBoxRect = btnBoxObj.GetComponent<RectTransform>();
        btnBoxRect.anchoredPosition = new Vector2(0, 22);
        btnBoxRect.sizeDelta = new Vector2(58, 58);
        Image btnBoxImg = btnBoxObj.GetComponent<Image>();
        btnBoxImg.color = new Color(0.15f, 0.18f, 0.25f, 0.95f);

        GameObject btnTextObj = new GameObject("ButtonText", typeof(RectTransform), typeof(Text));
        btnTextObj.transform.SetParent(btnBoxObj.transform, false);
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        Text btnText = btnTextObj.GetComponent<Text>();
        if (defaultFont != null) btnText.font = defaultFont;
        btnText.fontSize = 34;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "E";

        GameObject actTextObj = new GameObject("ActionText", typeof(RectTransform), typeof(Text));
        actTextObj.transform.SetParent(containerObj.transform, false);
        RectTransform actTextRect = actTextObj.GetComponent<RectTransform>();
        actTextRect.anchoredPosition = new Vector2(0, -32);
        actTextRect.sizeDelta = new Vector2(160, 32);
        Text actText = actTextObj.GetComponent<Text>();
        if (defaultFont != null) actText.font = defaultFont;
        actText.fontSize = 20;
        actText.fontStyle = FontStyle.Bold;
        actText.color = new Color(1.0f, 0.9f, 0.3f);
        actText.alignment = TextAnchor.MiddleCenter;
        actText.text = "Ligar";

        PlayerInput pInput = player.GetComponent<PlayerInput>();
        SerializedObject promptSO = new SerializedObject(promptUI);
        promptSO.FindProperty("buttonText").objectReferenceValue = btnText;
        promptSO.FindProperty("actionText").objectReferenceValue = actText;
        promptSO.FindProperty("promptContainer").objectReferenceValue = containerObj;
        promptSO.FindProperty("playerInput").objectReferenceValue = pInput;
        promptSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SetupUIScript] Interfaces de Usuário criadas e configuradas com sucesso na cena!");
    }
}
