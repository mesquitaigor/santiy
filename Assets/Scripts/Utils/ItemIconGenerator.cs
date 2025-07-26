using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

public class ItemIconGenerator : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera iconCamera;
    public RenderTexture renderTexture;
    
    [Header("Scene Object")]
    public GameObject targetSceneObject; // Objeto da cena para gerar ícone
    public string iconFileName = "GeneratedIcon"; // Nome do arquivo de ícone
    
    [Header("Icon Settings")]
    public int iconSize = 256;
    [Range(0.1f, 2.0f)]
    public float framingSize = 0.6f; // Controle de enquadramento (0.1 = muito próximo, 2.0 = muito longe)
    [Range(0.5f, 10.0f)]
    public float cameraDistance = 2f; // Distância da câmera do objeto
    public LayerMask iconLayer = 1 << 8; // Layer específica para objetos que devem aparecer no ícone
    public bool useLayerFiltering = true; // Se deve usar filtro de layer ou capturar tudo
    
    [Header("Save Settings")]
    public string savePath = "Assets/GeneratedIcons/";
    
    public Texture2D GenerateItemIcon(GameObject itemModel, bool preserveOriginal = false)
    {
        // Criar RenderTexture se não existir
        if (renderTexture == null)
        {
            // Criar RenderTexture com formato que suporta transparência
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(iconSize, iconSize, RenderTextureFormat.ARGB32, 24);
            descriptor.sRGB = false; // Desabilitar sRGB para melhor controle de transparência
            descriptor.msaaSamples = 1; // Sem antialiasing para preservar alpha
            renderTexture = new RenderTexture(descriptor);
            renderTexture.name = "IconRenderTexture";
            renderTexture.Create();
            
            Debug.Log($"RenderTexture criada: {renderTexture.format}, sRGB: {renderTexture.sRGB}");
        }
        
        // Salvar configurações originais da câmera
        var originalTarget = iconCamera.targetTexture;
        var originalBg = iconCamera.backgroundColor;
        var originalFlags = iconCamera.clearFlags;
        var originalMask = iconCamera.cullingMask;
        var originalOrthographic = iconCamera.orthographic;
        var originalOrthoSize = iconCamera.orthographicSize;
        var originalCameraPosition = iconCamera.transform.position;
        var originalCameraRotation = iconCamera.transform.rotation;
        var originalNearClip = iconCamera.nearClipPlane;
        var originalFarClip = iconCamera.farClipPlane;
        
        // Setup da camera para ícone com transparência real
        iconCamera.targetTexture = renderTexture;
        iconCamera.backgroundColor = Color.clear; // Fundo completamente transparente
        iconCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // Configurações específicas para renderização com transparência
        iconCamera.allowHDR = false; // Desabilitar HDR para evitar problemas de cor
        iconCamera.allowMSAA = false; // Desabilitar antialiasing que pode afetar alpha
        
        // Configurar culling mask baseado na opção de filtro de layer
        if (useLayerFiltering)
        {
            iconCamera.cullingMask = iconLayer; // Usar apenas a layer específica
            Debug.Log($"Usando filtro de layer: {iconLayer.value}");
        }
        else
        {
            iconCamera.cullingMask = -1; // Ver todas as layers
            Debug.Log("Capturando todas as layers");
        }
        
        iconCamera.orthographic = true;
        
        GameObject tempModel = itemModel;
        Vector3 originalPosition = Vector3.zero;
        Quaternion originalRotation = Quaternion.identity;
        int originalLayer = tempModel.layer;
        
        // Se queremos preservar o original, salvar posição/rotação/layer
        if (preserveOriginal)
        {
            originalPosition = tempModel.transform.position;
            originalRotation = tempModel.transform.rotation;
        }
        
        // Se estiver usando filtro de layer, mover objeto para a layer correta
        if (useLayerFiltering)
        {
            int targetLayer = Mathf.RoundToInt(Mathf.Log(iconLayer.value, 2));
            SetLayerRecursively(tempModel, targetLayer);
            Debug.Log($"Objeto movido para layer: {targetLayer}");
        }
        
        // Adicionar luz mais suave para iluminar o objeto uniformemente
        GameObject lightObj = new GameObject("TempLight");
        Light tempLight = lightObj.AddComponent<Light>();
        tempLight.type = LightType.Directional;
        tempLight.intensity = 1.5f; // Luz mais forte para garantir que o objeto seja bem visível
        tempLight.color = Color.white;
        tempLight.shadows = LightShadows.None; // Sem sombras para ícones
        tempLight.cullingMask = useLayerFiltering ? iconLayer : -1; // Iluminar apenas a layer correta
        lightObj.transform.rotation = Quaternion.Euler(30f, -45f, 0f); // Ângulo mais suave
        
        Debug.Log($"Capturando objeto: {tempModel.name} na posição {tempModel.transform.position}");
        
        // Posicionar camera para enquadrar o objeto
        FrameObject(tempModel, iconCamera);
        
        // Limpar RenderTexture manualmente para garantir transparência
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentActiveRT;
        
        // Renderizar
        iconCamera.Render();
        Debug.Log("Renderização concluída");
        
        // Debug adicional: verificar se RenderTexture tem transparência
        RenderTexture.active = renderTexture;
        Texture2D debugTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, false);
        debugTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        debugTex.Apply();
        Color debugCorner = debugTex.GetPixel(0, 0);
        Color debugCenter = debugTex.GetPixel(renderTexture.width/2, renderTexture.height/2);
        Debug.Log($"Debug - Canto: {debugCorner} (Alpha: {debugCorner.a}), Centro: {debugCenter} (Alpha: {debugCenter.a})");
        DestroyImmediate(debugTex);
        RenderTexture.active = null;
        
        // Capturar a texture sem processamento de transparência
        Texture2D icon = CaptureRenderTexture(renderTexture);
        
        // Limpar recursos dependendo do modo
        if (preserveOriginal)
        {
            // Restaurar posição/rotação/layer do objeto original
            tempModel.transform.position = originalPosition;
            tempModel.transform.rotation = originalRotation;
            
            // Restaurar layer original se foi alterada
            if (useLayerFiltering)
            {
                SetLayerRecursively(tempModel, originalLayer);
                Debug.Log($"Layer original restaurada: {originalLayer}");
            }
        }
        else
        {
            // Destruir objeto se for uma cópia
            DestroyImmediate(tempModel);
        }
        
        DestroyImmediate(lightObj); // Limpar a luz
        
        // Restaurar configurações originais da câmera
        iconCamera.targetTexture = originalTarget;
        iconCamera.backgroundColor = originalBg;
        iconCamera.clearFlags = originalFlags;
        iconCamera.cullingMask = originalMask;
        iconCamera.orthographic = originalOrthographic;
        iconCamera.orthographicSize = originalOrthoSize;
        iconCamera.transform.position = originalCameraPosition;
        iconCamera.transform.rotation = originalCameraRotation;
        iconCamera.nearClipPlane = originalNearClip;
        iconCamera.farClipPlane = originalFarClip;
        
        return icon;
    }
    
    private void FrameObject(GameObject obj, Camera cam)
    {
        Bounds bounds = GetObjectBounds(obj);
        
        // Se não há bounds, usar valores padrão baseados na posição do objeto
        if (bounds.size == Vector3.zero)
        {
            bounds = new Bounds(obj.transform.position, Vector3.one);
            Debug.LogWarning($"Objeto {obj.name} não tem Renderer, usando bounds padrão");
        }
        
        // Para câmera ortográfica, ajustar o tamanho baseado nos bounds
        if (cam.orthographic)
        {
            // Usar o maior tamanho entre largura e altura (ignorar profundidade para enquadramento 2D)
            float maxSizeXY = Mathf.Max(bounds.size.x, bounds.size.y);
            
            // Aplicar framing size diretamente (sem margem extra automática)
            cam.orthographicSize = maxSizeXY * framingSize;
            
            // Posicionar a câmera atrás do objeto, considerando o centro dos bounds
            Vector3 cameraOffset = Vector3.back * cameraDistance;
            cam.transform.position = bounds.center + cameraOffset;
            cam.transform.LookAt(bounds.center);
            
            // Configurar clipping planes de forma mais conservadora
            float objectDepth = bounds.size.z;
            cam.nearClipPlane = Mathf.Max(0.1f, cameraDistance - objectDepth - 1f);
            cam.farClipPlane = cameraDistance + objectDepth + 2f;
            
            Debug.Log($"Objeto: {obj.name}, Bounds: {bounds.size}, Ortho size: {cam.orthographicSize}");
        }
        else
        {
            // Para câmera perspectiva
            float distance = bounds.size.magnitude / (2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
            Vector3 direction = cam.transform.forward;
            cam.transform.position = bounds.center - direction * distance * framingSize;
            cam.transform.LookAt(bounds.center);
        }
    }
    
    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds();
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
    
    // Método principal para gerar e salvar PNG
    public void GenerateAndSaveIcon(GameObject itemModel, string fileName, bool preserveOriginal = false)
    {
        Texture2D icon = GenerateItemIcon(itemModel, preserveOriginal);
        SaveTextureAsPNG(icon, fileName);
        DestroyImmediate(icon); // Limpar da memória após salvar
    }
    
    // Salvar Texture2D como arquivo PNG
    private void SaveTextureAsPNG(Texture2D texture, string fileName)
    {
        Debug.Log($"Salvando arquivo: {fileName}");
        
        // Criar diretório se não existir
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Converter para PNG
        byte[] pngData = texture.EncodeToPNG();
        
        // Salvar arquivo
        string fullPath = Path.Combine(savePath, fileName + ".png");
        File.WriteAllBytes(fullPath, pngData);
        
        Debug.Log($"Ícone PNG salvo em: {fullPath}");
        
        #if UNITY_EDITOR
        // Atualizar o banco de dados do Unity Editor
        UnityEditor.AssetDatabase.Refresh();
        
        // Configurar como Sprite automaticamente
        ConfigureAsSprite(fullPath);
        #endif
    }
    
    #if UNITY_EDITOR
    private void ConfigureAsSprite(string assetPath)
    {
        UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
        if (importer != null)
        {
            importer.textureType = UnityEditor.TextureImporterType.Sprite;
            importer.spriteImportMode = UnityEditor.SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed; // Sem compressão para preservar qualidade
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            
            Debug.Log($"Sprite configurado com transparência: {assetPath}");
        }
    }
    #endif
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        
        // Aplicar a layer a todos os filhos também
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    // Método simplificado para capturar RenderTexture preservando o modelo
    private Texture2D CaptureRenderTexture(RenderTexture rt)
    {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;
        
        // Usar TextureFormat.RGBA32 com linear=false para preservar transparência
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        
        RenderTexture.active = previousActive;
        
        // Debug: verificar alguns pixels para transparência
        Color centerPixel = texture.GetPixel(rt.width/2, rt.height/2);
        Color cornerPixel = texture.GetPixel(0, 0);
        Debug.Log($"Pixel central: {centerPixel}, Pixel canto: {cornerPixel}");
        
        return texture;
    }
    
    // Método para gerar ícone de objeto existente na cena (sem mover ou destruir)
    public void GenerateIconFromSceneObject(GameObject sceneObject, string fileName)
    {
        if (sceneObject == null)
        {
            Debug.LogError("Objeto da cena não foi fornecido!");
            return;
        }
        
        Debug.Log($"Gerando ícone do objeto da cena: {sceneObject.name}");
        
        Texture2D icon = GenerateItemIcon(sceneObject, preserveOriginal: true);
        SaveTextureAsPNG(icon, fileName);
        DestroyImmediate(icon);
        
        Debug.Log($"Ícone salvo como: {fileName}.png");
    }
    
    // CONTEXT MENUS ESSENCIAIS
    
    // Método conveniente para gerar ícone do objeto especificado no Inspector
    [ContextMenu("Generate Icon from Target Object")]
    public void GenerateIconFromTargetObject()
    {
        if (targetSceneObject == null)
        {
            Debug.LogError("Target Scene Object não foi especificado! Configure o campo 'Target Scene Object' no Inspector.");
            return;
        }
        
        if (iconCamera == null)
        {
            Debug.LogError("IconCamera não está configurada!");
            return;
        }
        
        if (string.IsNullOrEmpty(iconFileName))
        {
            iconFileName = targetSceneObject.name + "_Icon";
        }
        
        GenerateIconFromSceneObject(targetSceneObject, iconFileName);
        
        Debug.Log($"Ícone gerado para: {targetSceneObject.name} como {iconFileName}.png");
    }
    
    // Método para configurar automaticamente a layer do objeto para ícones
    [ContextMenu("Setup Object for Icon Layer")]
    public void SetupObjectForIconLayer()
    {
        if (targetSceneObject == null)
        {
            Debug.LogError("Target Scene Object não especificado!");
            return;
        }
        
        int iconLayerNumber = Mathf.RoundToInt(Mathf.Log(iconLayer.value, 2));
        SetLayerRecursively(targetSceneObject, iconLayerNumber);
        
        Debug.Log($"Objeto {targetSceneObject.name} movido para layer de ícones: {iconLayerNumber}");
        Debug.Log("Agora use 'Use Layer Filtering = true' para capturar apenas este objeto");
    }
    
    // Método para restaurar o objeto para a layer padrão
    [ContextMenu("Restore Object to Default Layer")]
    public void RestoreObjectToDefaultLayer()
    {
        if (targetSceneObject == null)
        {
            Debug.LogError("Target Scene Object não especificado!");
            return;
        }
        
        SetLayerRecursively(targetSceneObject, 0); // Layer padrão
    }
    
    // Método de debug essencial
    [ContextMenu("Debug Icon Generation")]
    public void DebugIconGeneration()
    {
        if (targetSceneObject == null)
        {
            Debug.LogError("Target Scene Object não especificado!");
            return;
        }
        
        if (iconCamera == null)
        {
            Debug.LogError("Icon Camera não especificada!");
            return;
        }
        
        Renderer[] renderers = targetSceneObject.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            Debug.Log($"  - {r.gameObject.name}: bounds={r.bounds.size}, material={r.material?.name}");
        }
        
        // Verificar se object está na layer correta
        int iconLayerNumber = Mathf.RoundToInt(Mathf.Log(iconLayer.value, 2));
        bool objectInCorrectLayer = targetSceneObject.layer == iconLayerNumber;
        
        if (useLayerFiltering && !objectInCorrectLayer)
        {
            Debug.LogWarning($"ATENÇÃO: Objeto está na layer {targetSceneObject.layer}, mas o filtro espera layer {iconLayerNumber}");
            Debug.LogWarning("Use 'Setup Object for Icon Layer' para corrigir automaticamente");
        }
    }
}
