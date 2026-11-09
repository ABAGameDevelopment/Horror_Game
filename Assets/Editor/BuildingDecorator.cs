using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class BuildingDecorator
{
    private const string PrefabFolder = "Assets/Packages/nappin/HouseInteriorPack/Prefabs/";
    private const string PackMatFolder = "Assets/Packages/nappin/HouseInteriorPack/Materials/";
    private const string CuadrosAssetFolder = "Assets/Materials/Objects/Cuadros/";
    private const string VideoFramesFolder = "Assets/Materials/Objects/VideoFrames/";
    private const string RootDecorationName = "Decoracion_Edificio";
    private const string VersionMarkerName = "Version_v3_PuertasInodorosYVideo";

    static BuildingDecorator()
    {
        EditorApplication.delayCall += AutoApplyIfNeeded;
    }

    private static void AutoApplyIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        GameObject edificioV = GameObject.Find("EdificioV");
        if (edificioV == null)
        {
            return;
        }

        Transform existing = edificioV.transform.Find(RootDecorationName);
        if (existing == null || existing.Find(VersionMarkerName) == null)
        {
            DecorateBuilding();
        }
    }

    [MenuItem("Tools/Decoracion Edificio/Aplicar o Regenerar Decoracion Completa")]
    public static void DecorateBuildingMenu()
    {
        DecorateBuilding();
    }

    [MenuItem("Tools/Decoracion Edificio/Reparar Materiales URP (HouseInteriorPack)")]
    public static void RepairPackMaterialsMenu()
    {
        RepairHouseInteriorMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log("[BuildingDecorator] Materiales de HouseInteriorPack actualizados a URP Lit.");
    }

    public static void DecorateBuilding()
    {
        GameObject edificioV = GameObject.Find("EdificioV");
        if (edificioV == null)
        {
            Debug.LogWarning("[BuildingDecorator] No se encontró el objeto 'EdificioV' en la escena actual.");
            return;
        }

        AssetDatabase.Refresh();
        RepairHouseInteriorMaterials();
        Material[] cuadroMaterials = EnsureCuadroMaterials();
        Texture2D[] videoFrames = LoadVideoFrames();
        Material videoScreenMat = EnsureVideoScreenMaterial(videoFrames);
        Material glassWindowMat = EnsureTransparentGlassMaterial();
        Material windowFrameMat = AssetDatabase.LoadAssetAtPath<Material>(PackMatFolder + "(Mat)MetallicBlack.mat");
        Material wallPartitionMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wall/White.mat");
        Material bathroomMirrorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Bathroom/Mirror.mat");
        Material frameDarkMat = AssetDatabase.LoadAssetAtPath<Material>(PackMatFolder + "(Mat)GradientDarkBrown.mat");

        Transform oldDecor = edificioV.transform.Find(RootDecorationName);
        if (oldDecor != null)
        {
            Undo.DestroyObjectImmediate(oldDecor.gameObject);
        }

        GameObject rootDecor = new GameObject(RootDecorationName);
        Undo.RegisterCreatedObjectUndo(rootDecor, "Decorar EdificioV v3");
        rootDecor.transform.SetParent(edificioV.transform, false);
        rootDecor.transform.localPosition = Vector3.zero;
        rootDecor.transform.localRotation = Quaternion.identity;
        rootDecor.transform.localScale = Vector3.one;

        GameObject versionMarker = new GameObject(VersionMarkerName);
        versionMarker.transform.SetParent(rootDecor.transform, false);

        Transform lampsParent = CreateGroup(rootDecor.transform, "Lamparas_Y_Luces");
        Transform cuadrosParent = CreateGroup(rootDecor.transform, "Cuadros_Y_Espejos");
        Transform officesParent = CreateGroup(rootDecor.transform, "Oficinas_Con_Ventanillas");
        Transform videoParent = CreateGroup(rootDecor.transform, "Video_Sala_Audiovisual");
        Transform bathroomStallsParent = CreateGroup(rootDecor.transform, "Puertas_Inodoros_Banos");
        Transform furnitureParent = CreateGroup(rootDecor.transform, "Muebles_Y_Objetos");

        SetupBathroomDoorsAndToiletStallDoors(edificioV.transform, bathroomStallsParent);
        PlaceAllLamps(lampsParent);
        PlaceAllCuadrosAndMirrors(cuadrosParent, cuadroMaterials, bathroomMirrorMat, frameDarkMat);
        BuildEastWingOffices(officesParent, edificioV.transform, glassWindowMat, windowFrameMat, wallPartitionMat, cuadroMaterials, frameDarkMat);
        SetupAudiovisualHorrorVideo(videoParent, edificioV.transform, videoScreenMat, videoFrames);
        PlaceAllFurnitureAndProps(furnitureParent, edificioV.transform);

        EditorSceneManager.MarkSceneDirty(edificioV.scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"[BuildingDecorator] ¡Decoración v3 aplicada! ({videoFrames.Length} frames de video cargados, puertas de baños cerradas e interactivas, y puertas en todos los inodoros).");
    }

    private static Transform CreateGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static void SnapToFloor(GameObject instance, Transform edificioRoot, float targetFloorLocalY = 0.01f)
    {
        if (instance == null || edificioRoot == null)
        {
            return;
        }

        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        float currentMinLocalY = edificioRoot.InverseTransformPoint(combined.min).y;
        Vector3 localPos = instance.transform.localPosition;
        localPos.y += (targetFloorLocalY - currentMinLocalY);
        instance.transform.localPosition = localPos;
    }

    private static float GetTopSurfaceLocalY(GameObject instance, Transform edificioRoot, float fallbackY = 0.8f)
    {
        if (instance == null || edificioRoot == null)
        {
            return fallbackY;
        }

        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return fallbackY;
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        return edificioRoot.InverseTransformPoint(combined.max).y;
    }

    private static void AttachDoorInteractable(Transform doorRoot, float angle, bool invert, float triggerDist = 2.0f)
    {
        if (doorRoot == null)
        {
            return;
        }

        // Si tiene un hijo "door_body", el panel móvil es ese hijo (para que el marco no gire)
        Transform target = doorRoot;
        foreach (Transform t in doorRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.ToLower().Contains("door_body"))
            {
                target = t;
                break;
            }
        }

        DoorInteractable di = target.GetComponent<DoorInteractable>();
        if (di == null)
        {
            di = target.gameObject.AddComponent<DoorInteractable>();
        }
        di.Configure(angle, invert, triggerDist);
    }

    private static void SetupBathroomDoorsAndToiletStallDoors(Transform edificioRoot, Transform stallsParent)
    {
        // 1. Mantener CERRADAS y activas las puertas originales de los baños ((Msh)Door (1) y (Msh)Door (2))
        // y las puertas de Audiovisual y Classroom, agregándoles DoorInteractable para que el jugador las abra.
        Transform bathroom = edificioRoot.Find("Bathroom");
        if (bathroom != null)
        {
            Transform door1 = bathroom.Find("(Msh)Door (1)");
            if (door1 != null)
            {
                door1.gameObject.SetActive(true);
                AttachDoorInteractable(door1, 88f, false, 2.1f);
            }

            Transform door2 = bathroom.Find("(Msh)Door (2)");
            if (door2 != null)
            {
                door2.gameObject.SetActive(true);
                AttachDoorInteractable(door2, 88f, false, 2.1f);
            }
        }

        Transform audiovisual = edificioRoot.Find("Audiovisual");
        if (audiovisual != null)
        {
            Transform avDoor = audiovisual.Find("(Msh)Door");
            if (avDoor != null)
            {
                avDoor.gameObject.SetActive(true);
                AttachDoorInteractable(avDoor, 88f, false, 2.1f);
            }
        }

        Transform classroom = edificioRoot.Find("Classroom");
        if (classroom != null)
        {
            Transform clDoor = classroom.Find("(Msh)Door (3)");
            if (clDoor != null)
            {
                clDoor.gameObject.SetActive(true);
                AttachDoorInteractable(clDoor, 88f, false, 2.1f);
            }
        }

        // 2. Poner puertas a los 6 inodoros (3 en el baño izquierdo y 3 en el baño derecho)
        Material stallWallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Bathroom/Brown.mat");
        Material stallDoorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Bathroom/Door.mat");
        Material handleMat = AssetDatabase.LoadAssetAtPath<Material>(PackMatFolder + "(Mat)MetallicBlack.mat");

        if (bathroom == null)
        {
            return;
        }

        // Detectar las posiciones Z exactas de los 6 inodoros en espacio local de EdificioV
        List<float> leftToiletsZ = new List<float>();
        List<float> rightToiletsZ = new List<float>();

        foreach (Transform child in bathroom)
        {
            if (child.name.Contains("Toilet"))
            {
                Renderer r = child.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    Vector3 localCenter = edificioRoot.InverseTransformPoint(r.bounds.center);
                    if (localCenter.x < 1.4f)
                    {
                        leftToiletsZ.Add(localCenter.z);
                    }
                    else
                    {
                        rightToiletsZ.Add(localCenter.z);
                    }
                }
            }
        }

        leftToiletsZ.Sort();
        rightToiletsZ.Sort();

        // Respaldo por si algún bounds no estuviera listo
        if (leftToiletsZ.Count < 3)
        {
            leftToiletsZ = new List<float> { 36.22f, 37.22f, 38.25f };
        }
        if (rightToiletsZ.Count < 3)
        {
            rightToiletsZ = new List<float> { 36.20f, 37.20f, 38.20f };
        }

        // Agregar el 4to muro divisorio en ambos baños para cerrar el último cubículo de inodoro
        float leftEndWallZ = leftToiletsZ[leftToiletsZ.Count - 1] + 0.52f;
        GameObject leftDividerEnd = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftDividerEnd.name = "Divisor_CubiculoIzq_Final";
        leftDividerEnd.transform.SetParent(stallsParent, false);
        leftDividerEnd.transform.localPosition = new Vector3(-0.985f, 1.2655f, leftEndWallZ);
        leftDividerEnd.transform.localScale = new Vector3(1.59f, 2.5f, 0.1f);
        if (stallWallMat != null)
        {
            leftDividerEnd.GetComponent<MeshRenderer>().sharedMaterial = stallWallMat;
        }

        float rightEndWallZ = rightToiletsZ[rightToiletsZ.Count - 1] + 0.52f;
        GameObject rightDividerEnd = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightDividerEnd.name = "Divisor_CubiculoDer_Final";
        rightDividerEnd.transform.SetParent(stallsParent, false);
        rightDividerEnd.transform.localPosition = new Vector3(3.85f, 1.2655f, rightEndWallZ);
        rightDividerEnd.transform.localScale = new Vector3(1.70f, 2.5f, 0.1f);
        if (stallWallMat != null)
        {
            rightDividerEnd.GetComponent<MeshRenderer>().sharedMaterial = stallWallMat;
        }

        // Crear las 3 puertas de los inodoros del Baño Izquierdo (frente de los cubículos en x = -1.78f)
        for (int i = 0; i < leftToiletsZ.Count; i++)
        {
            CreateStallDoor(
                stallsParent,
                $"Puerta_Inodoro_Izq_{i + 1}",
                new Vector3(-1.76f, 1.28f, leftToiletsZ[i]),
                0.92f,
                2.25f,
                true,
                stallDoorMat != null ? stallDoorMat : stallWallMat,
                handleMat);
        }

        // Crear las 3 puertas de los inodoros del Baño Derecho (frente de los cubículos en x = 4.70f)
        for (int i = 0; i < rightToiletsZ.Count; i++)
        {
            CreateStallDoor(
                stallsParent,
                $"Puerta_Inodoro_Der_{i + 1}",
                new Vector3(4.68f, 1.28f, rightToiletsZ[i]),
                0.92f,
                2.25f,
                false,
                stallDoorMat != null ? stallDoorMat : stallWallMat,
                handleMat);
        }
    }

    private static void CreateStallDoor(
        Transform parent,
        string name,
        Vector3 centerLocalPos,
        float widthZ,
        float heightY,
        bool isLeftBathroom,
        Material doorMat,
        Material handleMat)
    {
        GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorPanel.name = name;
        doorPanel.transform.SetParent(parent, false);
        doorPanel.transform.localPosition = centerLocalPos;
        doorPanel.transform.localScale = new Vector3(0.06f, heightY, widthZ);

        if (doorMat != null)
        {
            doorPanel.GetComponent<MeshRenderer>().sharedMaterial = doorMat;
        }

        // Manija / cerrojo de la puerta del inodoro
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Manija";
        Object.DestroyImmediate(handle.GetComponent<Collider>());
        handle.transform.SetParent(doorPanel.transform, false);
        float handleSideX = isLeftBathroom ? -0.8f : 0.8f;
        handle.transform.localPosition = new Vector3(handleSideX, -0.05f, 0.36f);
        handle.transform.localScale = new Vector3(1.4f, 0.05f, 0.12f);
        if (handleMat != null)
        {
            handle.GetComponent<MeshRenderer>().sharedMaterial = handleMat;
        }

        // Hacer que el jugador pueda abrir y cerrar la puerta del inodoro
        DoorInteractable di = doorPanel.AddComponent<DoorInteractable>();
        di.Configure(85f, !isLeftBathroom, 1.45f);
    }

    private static void RepairHouseInteriorMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Packages/nappin/HouseInteriorPack/Materials" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Skybox"))
            {
                continue;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                continue;
            }

            if (mat.shader != urpLit)
            {
                Texture mainTex = mat.GetTexture("_BaseMap");
                if (mainTex == null)
                {
                    mainTex = mat.GetTexture("_MainTex");
                }

                Color baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
                mat.shader = urpLit;
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                }
                mat.SetColor("_BaseColor", baseCol);

                if (path.Contains("EmissiveWarm"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(1.45f, 1.15f, 0.72f, 1f));
                }

                EditorUtility.SetDirty(mat);
            }
        }
    }

    private static Material EnsureTransparentGlassMaterial()
    {
        string matPath = "Assets/Materials/Objects/Mat_VidrioVentanilla.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (mat == null && urpLit != null)
        {
            mat = new Material(urpLit);
            mat.name = "Mat_VidrioVentanilla";
            AssetDatabase.CreateAsset(mat, matPath);
        }

        if (mat != null && urpLit != null)
        {
            mat.shader = urpLit;
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");

            mat.SetColor("_BaseColor", new Color(0.72f, 0.85f, 0.92f, 0.18f));
            mat.SetFloat("_Smoothness", 0.92f);
            mat.SetFloat("_Metallic", 0.1f);
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    private static Texture2D[] LoadVideoFrames()
    {
        List<Texture2D> list = new List<Texture2D>();
        for (int i = 1; i <= 16; i++)
        {
            string frameName = i < 10 ? ("Frame_0" + i) : ("Frame_" + i);
            string path = VideoFramesFolder + frameName + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                list.Add(tex);
            }
        }
        return list.ToArray();
    }

    private static Material EnsureVideoScreenMaterial(Texture2D[] frames)
    {
        string matPath = "Assets/Materials/Objects/Mat_PantallaVideoTerror.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Shader targetShader = urpUnlit != null ? urpUnlit : urpLit;

        if (mat == null && targetShader != null)
        {
            mat = new Material(targetShader);
            mat.name = "Mat_PantallaVideoTerror";
            AssetDatabase.CreateAsset(mat, matPath);
        }

        if (mat != null)
        {
            if (targetShader != null)
            {
                mat.shader = targetShader;
            }
            mat.SetColor("_BaseColor", Color.white);
            if (frames != null && frames.Length > 0 && frames[0] != null)
            {
                mat.SetTexture("_BaseMap", frames[0]);
                mat.SetTexture("_MainTex", frames[0]);
                if (mat.HasProperty("_EmissionMap"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetTexture("_EmissionMap", frames[0]);
                    mat.SetColor("_EmissionColor", Color.white);
                }
            }
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    private static Material[] EnsureCuadroMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials/Objects/Cuadros"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Objects"))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Objects");
            }
            AssetDatabase.CreateFolder("Assets/Materials/Objects", "Cuadros");
        }

        string[] texNames =
        {
            "Cuadro_BosqueOscuro",
            "Cuadro_Silueta",
            "Cuadro_RetratoAntiguo",
            "Cuadro_Anatomia",
            "Cuadro_PasilloRojo",
            "Cuadro_Eclipse"
        };

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        List<Material> materials = new List<Material>();

        foreach (string texName in texNames)
        {
            string texPath = CuadrosAssetFolder + texName + ".png";
            string matPath = CuadrosAssetFolder + "Mat_" + texName + ".mat";

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null && urpLit != null)
            {
                mat = new Material(urpLit);
                mat.name = "Mat_" + texName;
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (mat != null)
            {
                if (urpLit != null)
                {
                    mat.shader = urpLit;
                }
                mat.SetColor("_BaseColor", new Color(0.9f, 0.88f, 0.85f, 1f));
                mat.SetFloat("_Smoothness", 0.25f);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                }
                EditorUtility.SetDirty(mat);
                materials.Add(mat);
            }
        }

        return materials.ToArray();
    }

    private static GameObject SpawnPrefab(
        string prefabFileName,
        Transform parent,
        string instanceName,
        Vector3 localPos,
        Vector3 localEuler,
        Vector3 localScale)
    {
        string path = PrefabFolder + prefabFileName;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("[BuildingDecorator] No se encontró el prefab: " + path);
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = instanceName;
        instance.transform.localPosition = localPos;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale = localScale;
        return instance;
    }

    private static void AddHorrorLight(
        GameObject lampObj,
        Vector3 localOffset,
        Color color,
        float intensity,
        float range,
        bool flicker = false,
        float minFlicker = 0.2f,
        float maxFlicker = 2.0f)
    {
        if (lampObj == null)
        {
            return;
        }

        GameObject lightGo = new GameObject("Luz_Horror");
        lightGo.transform.SetParent(lampObj.transform, false);
        lightGo.transform.localPosition = localOffset;
        lightGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.75f;

        if (flicker)
        {
            HorrorLightFlicker flickerComp = lightGo.AddComponent<HorrorLightFlicker>();
            flickerComp.Configure(minFlicker, maxFlicker, Random.Range(4.5f, 8.5f), 0.012f);
        }
    }

    private static void SetupAudiovisualHorrorVideo(
        Transform videoParent,
        Transform edificioRoot,
        Material videoMat,
        Texture2D[] videoFrames)
    {
        // 1. Pantalla Grande de Proyección en Sala Audiovisual (usando Cube delgado para que sea visible desde cualquier ángulo)
        GameObject bigScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bigScreen.name = "PantallaGrande_VideoTerror";
        Object.DestroyImmediate(bigScreen.GetComponent<Collider>());
        bigScreen.transform.SetParent(videoParent, false);
        bigScreen.transform.localPosition = new Vector3(-13.90f, 3.35f, 19.08f);
        bigScreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        bigScreen.transform.localScale = new Vector3(6.85f, 3.25f, 0.06f);

        MeshRenderer screenRenderer = bigScreen.GetComponent<MeshRenderer>();
        if (screenRenderer != null && videoMat != null)
        {
            screenRenderer.sharedMaterial = videoMat;
            screenRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        // Luz del proyector hacia las butacas que cambia con el video de terror
        GameObject projLightGo = new GameObject("Luz_ProyectorVideo");
        projLightGo.transform.SetParent(bigScreen.transform, false);
        projLightGo.transform.localPosition = new Vector3(0f, 0f, -2.0f);
        projLightGo.transform.localRotation = Quaternion.Euler(12f, 180f, 0f);

        Light projLight = projLightGo.AddComponent<Light>();
        projLight.type = LightType.Spot;
        projLight.spotAngle = 85f;
        projLight.innerSpotAngle = 45f;
        projLight.range = 15f;
        projLight.intensity = 2.4f;
        projLight.color = new Color(0.65f, 0.75f, 0.88f);
        projLight.shadows = LightShadows.Soft;

        HorrorVideoScreen horrorAnim = bigScreen.AddComponent<HorrorVideoScreen>();
        horrorAnim.Setup(videoFrames, projLight, 8f);

        // También aplicar el video directamente sobre Walls/Cube (20) (la pantalla verde original) para evitar cualquier ocultamiento
        Transform walls = edificioRoot.Find("Walls");
        if (walls != null)
        {
            Transform greenBoard = walls.Find("Cube (20)");
            if (greenBoard != null)
            {
                MeshRenderer gbRenderer = greenBoard.GetComponent<MeshRenderer>();
                if (gbRenderer != null && videoMat != null)
                {
                    gbRenderer.sharedMaterial = videoMat;
                }
                HorrorVideoScreen gbVideo = greenBoard.GetComponent<HorrorVideoScreen>();
                if (gbVideo == null)
                {
                    gbVideo = greenBoard.gameObject.AddComponent<HorrorVideoScreen>();
                }
                gbVideo.Setup(videoFrames, null, 8f);
            }
        }

        // 2. Consola de medios en el centro de la tarima con su TV también reproduciendo el video de terror
        GameObject mediaConsole = SpawnPrefab("(Prb)MediaConsole.prefab", videoParent, "ConsolaMedios_Audiovisual", new Vector3(-14.0f, 1.0f, 18.5f), new Vector3(0f, 90f, 0f), new Vector3(1.05f, 1.05f, 1.05f));
        SnapToFloor(mediaConsole, edificioRoot, 1.0f);

        GameObject tvScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tvScreen.name = "PantallaTV_VideoTerror";
        Object.DestroyImmediate(tvScreen.GetComponent<Collider>());
        tvScreen.transform.SetParent(videoParent, false);
        tvScreen.transform.localPosition = new Vector3(-14.0f, 2.27f, 18.25f);
        tvScreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        tvScreen.transform.localScale = new Vector3(1.32f, 0.75f, 0.04f);

        MeshRenderer tvRenderer = tvScreen.GetComponent<MeshRenderer>();
        if (tvRenderer != null && videoMat != null)
        {
            tvRenderer.sharedMaterial = videoMat;
            tvRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        HorrorVideoScreen tvAnim = tvScreen.AddComponent<HorrorVideoScreen>();
        tvAnim.Setup(videoFrames, null, 8f);
    }

    private static void BuildEastWingOffices(
        Transform officesParent,
        Transform edificioRoot,
        Material glassMat,
        Material frameMat,
        Material wallMat,
        Material[] cuadroMats,
        Material cuadroFrameMat)
    {
        Transform windowsGroup = CreateGroup(officesParent, "Ventanillas_Transparentes");

        int numWindows = 4;
        float zStart = 10.15f;
        float zEnd = 34.55f;
        float totalLen = zEnd - zStart;
        float winLen = totalLen / numWindows;

        for (int i = 0; i < numWindows; i++)
        {
            float z0 = zStart + i * winLen;
            float z1 = z0 + winLen;
            float zMid = (z0 + z1) * 0.5f;

            GameObject glassPane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassPane.name = $"Cristal_Ventanilla_Oficina_{i + 1}";
            glassPane.transform.SetParent(windowsGroup, false);
            glassPane.transform.localPosition = new Vector3(14.18f, 2.40f, zMid);
            glassPane.transform.localScale = new Vector3(0.04f, 2.04f, winLen - 0.14f);
            MeshRenderer gmr = glassPane.GetComponent<MeshRenderer>();
            if (gmr != null && glassMat != null)
            {
                gmr.sharedMaterial = glassMat;
                gmr.shadowCastingMode = ShadowCastingMode.Off;
            }

            GameObject mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mullion.name = $"MarcoVertical_Ventanilla_{i + 1}";
            mullion.transform.SetParent(windowsGroup, false);
            mullion.transform.localPosition = new Vector3(14.18f, 2.40f, z0);
            mullion.transform.localScale = new Vector3(0.42f, 2.12f, 0.14f);
            if (frameMat != null)
            {
                mullion.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
            }

            GameObject subMullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            subMullion.name = $"SubMarco_Ventanilla_{i + 1}";
            subMullion.transform.SetParent(windowsGroup, false);
            subMullion.transform.localPosition = new Vector3(14.18f, 2.40f, zMid);
            subMullion.transform.localScale = new Vector3(0.12f, 2.08f, 0.08f);
            if (frameMat != null)
            {
                subMullion.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
            }

            GameObject hBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBar.name = $"BarraHorizontal_Ventanilla_{i + 1}";
            hBar.transform.SetParent(windowsGroup, false);
            hBar.transform.localPosition = new Vector3(14.18f, 1.95f, zMid);
            hBar.transform.localScale = new Vector3(0.10f, 0.06f, winLen - 0.12f);
            if (frameMat != null)
            {
                hBar.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
            }
        }

        Transform partitionsGroup = CreateGroup(officesParent, "Paredes_Divisorias_Oficinas");
        for (int i = 1; i <= numWindows; i++)
        {
            float zWall = zStart + i * winLen;
            GameObject partWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            partWall.name = $"MuroDivisorio_Oficina_{i}";
            partWall.transform.SetParent(partitionsGroup, false);
            partWall.transform.localPosition = new Vector3(18.64f, 2.75f, zWall);
            partWall.transform.localScale = new Vector3(5.48f, 5.50f, 0.22f);
            if (wallMat != null)
            {
                partWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
            }

            GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = $"DintelPaso_Oficina_{i}";
            lintel.transform.SetParent(partitionsGroup, false);
            lintel.transform.localPosition = new Vector3(15.12f, 4.40f, zWall);
            lintel.transform.localScale = new Vector3(1.55f, 2.20f, 0.22f);
            if (wallMat != null)
            {
                lintel.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
            }
        }

        Color officeLightCol = new Color(1.0f, 0.84f, 0.62f);
        for (int i = 0; i < numWindows; i++)
        {
            float zMid = zStart + (i + 0.5f) * winLen;
            Transform offRoom = CreateGroup(officesParent, $"Oficina_{i + 1}");

            GameObject desk = SpawnPrefab("(Prb)Desk.prefab", offRoom, $"Escritorio_Oficina_{i + 1}", new Vector3(18.2f, 0.01f, zMid), new Vector3(0f, 180f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
            SnapToFloor(desk, edificioRoot, 0.01f);
            float deskTopY = GetTopSurfaceLocalY(desk, edificioRoot, 0.82f);

            GameObject dLight = SpawnPrefab("(Prb)DeskLight.prefab", offRoom, $"LamparaEscritorio_Oficina_{i + 1}", new Vector3(18.3f, deskTopY, zMid - 0.65f), new Vector3(0f, 145f, 0f), Vector3.one);
            SnapToFloor(dLight, edificioRoot, deskTopY);

            GameObject plant = SpawnPrefab("(Prb)CoffeTable_smallPlant.prefab", offRoom, $"Planta_Oficina_{i + 1}", new Vector3(18.1f, deskTopY, zMid + 0.65f), Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
            SnapToFloor(plant, edificioRoot, deskTopY);

            GameObject execChair = SpawnPrefab("(Prb)Chair1.prefab", offRoom, $"SillaEjecutiva_Oficina_{i + 1}", new Vector3(19.35f, 0.01f, zMid), new Vector3(0f, 180f, 0f), new Vector3(1.05f, 1.05f, 1.05f));
            SnapToFloor(execChair, edificioRoot, 0.01f);

            GameObject visChair1 = SpawnPrefab("(Prb)Chair2.prefab", offRoom, $"SillaVisita_{i + 1}A", new Vector3(16.95f, 0.01f, zMid - 0.55f), new Vector3(0f, 0f, 0f), Vector3.one);
            SnapToFloor(visChair1, edificioRoot, 0.01f);

            GameObject visChair2 = SpawnPrefab("(Prb)Chair2.prefab", offRoom, $"SillaVisita_{i + 1}B", new Vector3(16.95f, 0.01f, zMid + 0.55f), new Vector3(0f, 0f, 0f), Vector3.one);
            SnapToFloor(visChair2, edificioRoot, 0.01f);

            GameObject shelf = SpawnPrefab("(Prb)Storage2.prefab", offRoom, $"Librero_Oficina_{i + 1}", new Vector3(20.85f, 0.01f, zMid - 1.65f), new Vector3(0f, 180f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
            SnapToFloor(shelf, edificioRoot, 0.01f);

            GameObject cabinet = SpawnPrefab("(Prb)Storage1.prefab", offRoom, $"Archivero_Oficina_{i + 1}", new Vector3(20.85f, 0.01f, zMid + 1.65f), new Vector3(0f, 180f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
            SnapToFloor(cabinet, edificioRoot, 0.01f);

            if (cuadroMats != null && cuadroMats.Length > 0)
            {
                Material cMat = cuadroMats[i % cuadroMats.Length];
                SpawnCuadro(offRoom, $"Cuadro_Oficina_{i + 1}", new Vector3(21.35f, 2.40f, zMid), 180f, new Vector3(1.0f, 0.9f, 1.5f), cMat, cuadroFrameMat, (i % 3) + 1);
            }

            GameObject offLamp = SpawnPrefab("(Prb)CeilingLight.prefab", offRoom, $"LamparaTecho_Oficina_{i + 1}", new Vector3(18.2f, 5.45f, zMid), Vector3.zero, Vector3.one);
            AddHorrorLight(offLamp, new Vector3(0f, -1.65f, 0f), officeLightCol, 1.55f, 7.0f, i == 2, 0.2f, 1.65f);
        }
    }

    private static void PlaceAllLamps(Transform parent)
    {
        Color warmDim = new Color(1.0f, 0.78f, 0.52f);
        Color eerieAmber = new Color(1.0f, 0.55f, 0.32f);
        Color coldFluorescent = new Color(0.76f, 0.88f, 1.0f);
        Color sicklyBath = new Color(0.68f, 0.92f, 0.85f);
        Color crimsonAccent = new Color(0.95f, 0.28f, 0.22f);

        Vector3[] leftCorridorCeilings =
        {
            new Vector3(-5.8f, 5.45f, 6.0f),
            new Vector3(-5.8f, 5.45f, 14.0f),
            new Vector3(-5.8f, 5.45f, 22.0f),
            new Vector3(-5.8f, 5.45f, 30.0f)
        };
        for (int i = 0; i < leftCorridorCeilings.Length; i++)
        {
            GameObject lamp = SpawnPrefab("(Prb)CeilingLight.prefab", parent, $"Lampara_PasilloIzq_{i + 1}", leftCorridorCeilings[i], Vector3.zero, Vector3.one);
            bool shouldFlicker = (i == 1 || i == 3);
            AddHorrorLight(lamp, new Vector3(0f, -1.65f, 0f), warmDim, 1.6f, 8.5f, shouldFlicker, 0.2f, 1.8f);
        }

        Vector3[] rightCorridorCeilings =
        {
            new Vector3(10.2f, 5.45f, 6.0f),
            new Vector3(10.2f, 5.45f, 14.0f),
            new Vector3(10.2f, 5.45f, 22.0f),
            new Vector3(10.2f, 5.45f, 30.0f)
        };
        for (int i = 0; i < rightCorridorCeilings.Length; i++)
        {
            GameObject lamp = SpawnPrefab("(Prb)CeilingLight.prefab", parent, $"Lampara_PasilloDer_{i + 1}", rightCorridorCeilings[i], Vector3.zero, Vector3.one);
            bool shouldFlicker = (i == 2);
            AddHorrorLight(lamp, new Vector3(0f, -1.65f, 0f), warmDim, 1.6f, 8.5f, shouldFlicker, 0.15f, 1.75f);
        }

        GameObject lobbyLamp = SpawnPrefab("(Prb)RoomLight.prefab", parent, "Lampara_LobbyEntrada", new Vector3(1.7f, 5.45f, 4.5f), Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f));
        AddHorrorLight(lobbyLamp, new Vector3(0f, -1.45f, 0f), warmDim, 2.0f, 10.0f, false);

        Vector3[] planterCorners =
        {
            new Vector3(-3.8f, 0.01f, 10.8f),
            new Vector3(7.2f, 0.01f, 10.8f),
            new Vector3(-3.8f, 0.01f, 27.4f),
            new Vector3(7.2f, 0.01f, 27.4f)
        };
        for (int i = 0; i < planterCorners.Length; i++)
        {
            GameObject cLight = SpawnPrefab("(Prb)CornerLight.prefab", parent, $"LamparaPie_Jardinera_{i + 1}", planterCorners[i], new Vector3(0f, i * 90f, 0f), Vector3.one);
            AddHorrorLight(cLight, new Vector3(0f, 2.1f, 0f), eerieAmber, 1.1f, 6.0f, i == 2, 0.25f, 1.2f);
        }

        GameObject bathHallLampL = SpawnPrefab("(Prb)Lamp.prefab", parent, "Lampara_EntradaBanoIzq", new Vector3(-3.5f, 5.4f, 32.5f), Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
        AddHorrorLight(bathHallLampL, new Vector3(0f, -0.95f, 0f), sicklyBath, 1.8f, 7.5f, false);

        GameObject bathHallLampR = SpawnPrefab("(Prb)Lamp.prefab", parent, "Lampara_EntradaBanoDer", new Vector3(6.4f, 5.4f, 32.5f), Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
        AddHorrorLight(bathHallLampR, new Vector3(0f, -0.95f, 0f), sicklyBath, 1.8f, 7.5f, false);

        Vector3[] avCeilings =
        {
            new Vector3(-14.0f, 5.45f, 5.5f),
            new Vector3(-14.0f, 5.45f, 11.5f),
            new Vector3(-14.0f, 5.45f, 16.5f)
        };
        for (int i = 0; i < avCeilings.Length; i++)
        {
            GameObject avLamp = SpawnPrefab("(Prb)CeilingLight.prefab", parent, $"Lampara_Audiovisual_{i + 1}", avCeilings[i], new Vector3(0f, 90f, 0f), Vector3.one);
            AddHorrorLight(avLamp, new Vector3(0f, -1.65f, 0f), crimsonAccent, 1.0f, 7.5f, i == 1, 0.1f, 1.2f);
        }

        float[] avSconceZ = { 6.2f, 10.2f, 14.2f };
        float[] avSconceY = { 3.1f, 2.65f, 2.25f };
        for (int i = 0; i < avSconceZ.Length; i++)
        {
            GameObject sconceW = SpawnPrefab("(Prb)MirrorLight.prefab", parent, $"Aplique_AV_Oeste_{i + 1}", new Vector3(-18.68f, avSconceY[i], avSconceZ[i]), new Vector3(0f, 0f, 0f), Vector3.one);
            AddHorrorLight(sconceW, new Vector3(0.25f, -0.1f, 0f), eerieAmber, 0.85f, 4.5f, false);

            GameObject sconceE = SpawnPrefab("(Prb)MirrorLight.prefab", parent, $"Aplique_AV_Este_{i + 1}", new Vector3(-9.60f, avSconceY[i], avSconceZ[i]), new Vector3(0f, 180f, 0f), Vector3.one);
            AddHorrorLight(sconceE, new Vector3(0.25f, -0.1f, 0f), eerieAmber, 0.85f, 4.5f, false);
        }

        Vector3[] classroomCeilings =
        {
            new Vector3(16.6f, 5.45f, 3.2f),
            new Vector3(19.6f, 5.45f, 3.2f),
            new Vector3(16.6f, 5.45f, 7.2f),
            new Vector3(19.6f, 5.45f, 7.2f)
        };
        for (int i = 0; i < classroomCeilings.Length; i++)
        {
            GameObject clLamp = SpawnPrefab("(Prb)CeilingLight.prefab", parent, $"Lampara_Salon_{i + 1}", classroomCeilings[i], Vector3.zero, Vector3.one);
            AddHorrorLight(clLamp, new Vector3(0f, -1.65f, 0f), coldFluorescent, 1.35f, 6.5f, i == 0, 0.1f, 1.5f);
        }

        GameObject bathLampL = SpawnPrefab("(Prb)RoomLight.prefab", parent, "Lampara_BanoIzq", new Vector3(-2.7f, 5.45f, 36.8f), Vector3.zero, Vector3.one);
        AddHorrorLight(bathLampL, new Vector3(0f, -1.45f, 0f), sicklyBath, 1.6f, 7.5f, true, 0.2f, 1.7f);

        GameObject bathLampR = SpawnPrefab("(Prb)RoomLight.prefab", parent, "Lampara_BanoDer", new Vector3(5.5f, 5.45f, 36.8f), Vector3.zero, Vector3.one);
        AddHorrorLight(bathLampR, new Vector3(0f, -1.45f, 0f), sicklyBath, 1.6f, 7.5f, false);

        SpawnPrefab("(Prb)MirrorLight.prefab", parent, "LuzEspejo_BanoIzq_1", new Vector3(-5.31f, 2.65f, 35.8f), new Vector3(0f, 0f, 0f), Vector3.one);
        SpawnPrefab("(Prb)MirrorLight.prefab", parent, "LuzEspejo_BanoIzq_2", new Vector3(-5.31f, 2.65f, 37.6f), new Vector3(0f, 0f, 0f), Vector3.one);
        SpawnPrefab("(Prb)MirrorLight.prefab", parent, "LuzEspejo_BanoDer_1", new Vector3(8.19f, 2.65f, 35.8f), new Vector3(0f, 180f, 0f), Vector3.one);
        SpawnPrefab("(Prb)MirrorLight.prefab", parent, "LuzEspejo_BanoDer_2", new Vector3(8.19f, 2.65f, 37.6f), new Vector3(0f, 180f, 0f), Vector3.one);

        GameObject nwLamp1 = SpawnPrefab("(Prb)RoomLight.prefab", parent, "Lampara_EnfermeriaNO", new Vector3(-14.0f, 5.45f, 24.2f), Vector3.zero, Vector3.one);
        AddHorrorLight(nwLamp1, new Vector3(0f, -1.45f, 0f), sicklyBath, 1.15f, 6.5f, true, 0.1f, 1.25f);

        GameObject nwLamp2 = SpawnPrefab("(Prb)RoomLight.prefab", parent, "Lampara_RecepcionNO", new Vector3(-11.5f, 5.45f, 34.2f), Vector3.zero, Vector3.one);
        AddHorrorLight(nwLamp2, new Vector3(0f, -1.45f, 0f), warmDim, 1.4f, 7.5f, false);
    }

    private static void SpawnCuadro(
        Transform parent,
        string name,
        Vector3 localPos,
        float rotY,
        Vector3 scale,
        Material canvasMat,
        Material frameMat,
        int mirrorVariant = 1)
    {
        string prbName = $"(Prb)Mirror{Mathf.Clamp(mirrorVariant, 1, 3)}.prefab";
        GameObject cuadro = SpawnPrefab(prbName, parent, name, localPos, new Vector3(0f, rotY, 0f), scale);
        if (cuadro == null)
        {
            return;
        }

        MeshRenderer mr = cuadro.GetComponentInChildren<MeshRenderer>();
        if (mr != null && frameMat != null)
        {
            Material[] shared = mr.sharedMaterials;
            if (shared.Length > 0)
            {
                shared[0] = frameMat;
                if (shared.Length > 1 && canvasMat != null)
                {
                    shared[1] = canvasMat;
                }
                mr.sharedMaterials = shared;
            }
        }

        if (canvasMat != null)
        {
            GameObject lienzo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            lienzo.name = "Lienzo_Pintura";
            Object.DestroyImmediate(lienzo.GetComponent<Collider>());
            lienzo.transform.SetParent(cuadro.transform, false);
            lienzo.transform.localPosition = new Vector3(0.052f, 0f, 0f);
            lienzo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            lienzo.transform.localScale = new Vector3(0.84f, 1.22f, 1f);

            MeshRenderer quadRenderer = lienzo.GetComponent<MeshRenderer>();
            if (quadRenderer != null)
            {
                quadRenderer.sharedMaterial = canvasMat;
                quadRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }

    private static void PlaceAllCuadrosAndMirrors(
        Transform parent,
        Material[] cuadroMats,
        Material bathroomMirrorMat,
        Material frameMat)
    {
        if (cuadroMats == null || cuadroMats.Length == 0)
        {
            return;
        }

        Vector3 portraitScale = new Vector3(1.0f, 1.05f, 1.05f);
        Vector3 landscapeScale = new Vector3(1.0f, 0.85f, 1.55f);
        Vector3 largeScale = new Vector3(1.1f, 1.35f, 1.85f);

        int idx = 0;
        Material NextMat() => cuadroMats[(idx++) % cuadroMats.Length];

        float[] leftHallZ = { 6.5f, 10.0f, 13.5f, 17.0f, 20.5f, 24.0f, 27.5f };
        for (int i = 0; i < leftHallZ.Length; i++)
        {
            Vector3 sc = (i % 2 == 0) ? portraitScale : landscapeScale;
            SpawnCuadro(parent, $"Cuadro_PasilloIzq_{i + 1}", new Vector3(-8.56f, 2.35f, leftHallZ[i]), 0f, sc, NextMat(), frameMat, (i % 3) + 1);
        }

        SpawnCuadro(parent, "Cuadro_Central_EntreBanos", new Vector3(1.44f, 2.4f, 33.73f), 90f, largeScale, NextMat(), frameMat, 1);

        SpawnCuadro(parent, "Cuadro_LobbySur_1", new Vector3(-6.8f, 2.4f, 0.67f), -90f, largeScale, NextMat(), frameMat, 1);
        SpawnCuadro(parent, "Cuadro_LobbySur_2", new Vector3(11.5f, 2.4f, 0.67f), -90f, landscapeScale, NextMat(), frameMat, 2);

        SpawnCuadro(parent, "Cuadro_PasilloDer_1", new Vector3(13.92f, 2.35f, 5.2f), 180f, portraitScale, NextMat(), frameMat, 1);
        SpawnCuadro(parent, "Cuadro_PasilloDer_2", new Vector3(13.92f, 2.35f, 8.8f), 180f, landscapeScale, NextMat(), frameMat, 3);
        SpawnCuadro(parent, "Cuadro_PasilloDer_3", new Vector3(13.92f, 2.35f, 35.0f), 180f, portraitScale, NextMat(), frameMat, 2);

        float[] avZ = { 4.2f, 8.2f, 12.2f, 16.2f };
        float[] avY = { 3.1f, 2.75f, 2.4f, 2.15f };
        for (int i = 0; i < avZ.Length; i++)
        {
            SpawnCuadro(parent, $"Cuadro_AudiovisualOeste_{i + 1}", new Vector3(-18.68f, avY[i], avZ[i]), 0f, portraitScale, NextMat(), frameMat, (i % 3) + 1);
        }
        SpawnCuadro(parent, "Cuadro_AudiovisualEste_1", new Vector3(-9.60f, 2.65f, 8.2f), 180f, landscapeScale, NextMat(), frameMat, 2);
        SpawnCuadro(parent, "Cuadro_AudiovisualEste_2", new Vector3(-9.60f, 2.25f, 12.2f), 180f, landscapeScale, NextMat(), frameMat, 3);

        SpawnCuadro(parent, "Cuadro_SalonEste_1", new Vector3(21.36f, 2.35f, 3.0f), 180f, portraitScale, cuadroMats[3], frameMat, 1);
        SpawnCuadro(parent, "Cuadro_SalonEste_2", new Vector3(21.36f, 2.35f, 5.6f), 180f, landscapeScale, cuadroMats[2], frameMat, 2);
        SpawnCuadro(parent, "Cuadro_SalonEste_3", new Vector3(21.36f, 2.35f, 8.2f), 180f, portraitScale, cuadroMats[3], frameMat, 3);
        SpawnCuadro(parent, "Cuadro_SalonNorte_1", new Vector3(16.5f, 2.35f, 10.02f), 90f, landscapeScale, NextMat(), frameMat, 1);
        SpawnCuadro(parent, "Cuadro_SalonNorte_2", new Vector3(19.5f, 2.35f, 10.02f), 90f, portraitScale, NextMat(), frameMat, 2);

        float[] nwRoomZ = { 21.8f, 26.0f, 32.0f, 36.5f };
        for (int i = 0; i < nwRoomZ.Length; i++)
        {
            SpawnCuadro(parent, $"Cuadro_SalaNoroeste_{i + 1}", new Vector3(-18.68f, 2.4f, nwRoomZ[i]), 0f, portraitScale, NextMat(), frameMat, (i % 3) + 1);
        }

        Vector3 mirrorScale = new Vector3(1.0f, 1.0f, 1.15f);
        Vector3[] bathMirrorsPos =
        {
            new Vector3(-5.31f, 1.85f, 35.8f),
            new Vector3(-5.31f, 1.85f, 37.6f),
            new Vector3(8.19f, 1.85f, 35.8f),
            new Vector3(8.19f, 1.85f, 37.6f)
        };
        float[] bathMirrorsRotY = { 0f, 0f, 180f, 180f };
        for (int i = 0; i < bathMirrorsPos.Length; i++)
        {
            GameObject mirror = SpawnPrefab("(Prb)Mirror1.prefab", parent, $"Espejo_Bano_{i + 1}", bathMirrorsPos[i], new Vector3(0f, bathMirrorsRotY[i], 0f), mirrorScale);
            if (mirror != null && bathroomMirrorMat != null)
            {
                MeshRenderer mr = mirror.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    Material[] mats = mr.sharedMaterials;
                    if (mats.Length > 1)
                    {
                        mats[1] = bathroomMirrorMat;
                        mr.sharedMaterials = mats;
                    }
                }
            }
        }
    }

    private static void PlaceAllFurnitureAndProps(Transform parent, Transform edificioRoot)
    {
        float[] gardenZ = { 13.0f, 16.0f, 19.0f, 22.0f, 25.0f };
        for (int i = 0; i < gardenZ.Length; i++)
        {
            GameObject wg = SpawnPrefab("(Prb)WaterGarden.prefab", parent, $"Jardin_Central_{i + 1}", new Vector3(1.7f, 0.52f, gardenZ[i]), new Vector3(0f, i * 45f, 0f), new Vector3(4.5f, 4.5f, 4.5f));
            SnapToFloor(wg, edificioRoot, 0.51f);

            string vasePrb = $"(Prb)LaunchTable_vase{(i % 4) + 1}.prefab";
            GameObject vase = SpawnPrefab(vasePrb, parent, $"Florero_Jardinera_{i + 1}", new Vector3(0.2f + (i % 2) * 3.0f, 0.52f, gardenZ[i]), Vector3.zero, new Vector3(2.2f, 2.2f, 2.2f));
            SnapToFloor(vase, edificioRoot, 0.51f);
        }

        GameObject sofaL = SpawnPrefab("(Prb)Sofa_pillowT.prefab", parent, "Sofa_Lobby_Izq", new Vector3(-0.8f, 0.01f, 7.2f), new Vector3(0f, 0f, 0f), new Vector3(1.8f, 1.8f, 1.8f));
        SnapToFloor(sofaL, edificioRoot, 0.01f);

        GameObject sofaR = SpawnPrefab("(Prb)Sofa.prefab", parent, "Sofa_Lobby_Der", new Vector3(4.2f, 0.01f, 7.2f), new Vector3(0f, 180f, 0f), new Vector3(1.8f, 1.8f, 1.8f));
        SnapToFloor(sofaR, edificioRoot, 0.01f);

        GameObject cTable = SpawnPrefab("(Prb)CoffeTable.prefab", parent, "MesaCentro_Lobby", new Vector3(1.7f, 0.01f, 7.2f), Vector3.zero, new Vector3(1.1f, 1.1f, 1.1f));
        SnapToFloor(cTable, edificioRoot, 0.01f);
        float cTableTop = GetTopSurfaceLocalY(cTable, edificioRoot, 0.65f);

        GameObject lobbyVase = SpawnPrefab("(Prb)LaunchTable_vase1.prefab", parent, "Florero_MesaLobby", new Vector3(1.7f, cTableTop, 7.2f), Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f));
        SnapToFloor(lobbyVase, edificioRoot, cTableTop);

        GameObject lib1 = SpawnPrefab("(Prb)Storage2.prefab", parent, "Librero_PasilloIzq_1", new Vector3(-8.1f, 0.01f, 8.2f), new Vector3(0f, 0f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        SnapToFloor(lib1, edificioRoot, 0.01f);

        GameObject lib2 = SpawnPrefab("(Prb)Storage2.prefab", parent, "Librero_PasilloIzq_2", new Vector3(-8.1f, 0.01f, 25.8f), new Vector3(0f, 0f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        SnapToFloor(lib2, edificioRoot, 0.01f);

        SpawnPrefab("(Prb)CoatHanger.prefab", parent, "Perchero_EntradaLobby", new Vector3(-8.45f, 1.65f, 5.5f), new Vector3(0f, 0f, 0f), Vector3.one);

        GameObject bench = SpawnPrefab("(Prb)ShoeRack.prefab", parent, "Banca_EntradaLobby", new Vector3(-7.2f, 0.01f, 1.2f), new Vector3(0f, -90f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        SnapToFloor(bench, edificioRoot, 0.01f);

        SpawnPrefab("(Prb)SoapDispenser.prefab", parent, "Jabon_BanoIzq_1", new Vector3(-5.31f, 1.15f, 35.3f), new Vector3(0f, 0f, 0f), new Vector3(2.0f, 2.0f, 2.0f));
        SpawnPrefab("(Prb)SoapDispenser.prefab", parent, "Jabon_BanoIzq_2", new Vector3(-5.31f, 1.15f, 37.1f), new Vector3(0f, 0f, 0f), new Vector3(2.0f, 2.0f, 2.0f));
        SpawnPrefab("(Prb)SoapDispenser.prefab", parent, "Jabon_BanoDer_1", new Vector3(8.19f, 1.15f, 35.3f), new Vector3(0f, 180f, 0f), new Vector3(2.0f, 2.0f, 2.0f));
        SpawnPrefab("(Prb)SoapDispenser.prefab", parent, "Jabon_BanoDer_2", new Vector3(8.19f, 1.15f, 37.1f), new Vector3(0f, 180f, 0f), new Vector3(2.0f, 2.0f, 2.0f));
        SpawnPrefab("(Prb)SmokeVent.prefab", parent, "Ventilacion_BanoIzq", new Vector3(-2.6f, 4.4f, 39.42f), new Vector3(0f, 90f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
        SpawnPrefab("(Prb)SmokeVent.prefab", parent, "Ventilacion_BanoDer", new Vector3(5.5f, 4.4f, 39.42f), new Vector3(0f, 90f, 0f), new Vector3(0.8f, 0.8f, 0.8f));

        GameObject profChair = SpawnPrefab("(Prb)Chair1.prefab", parent, "Silla_Profesor_Salon", new Vector3(15.5f, 0.01f, 1.4f), new Vector3(0f, 0f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
        SnapToFloor(profChair, edificioRoot, 0.01f);

        GameObject clShelf = SpawnPrefab("(Prb)Storage2.prefab", parent, "Librero_Salon", new Vector3(20.8f, 0.01f, 9.2f), new Vector3(0f, 180f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
        SnapToFloor(clShelf, edificioRoot, 0.01f);

        SpawnPrefab("(Prb)CoatHanger.prefab", parent, "Perchero_Salon", new Vector3(21.34f, 1.65f, 1.8f), new Vector3(0f, 180f, 0f), Vector3.one);

        GameObject bed1 = SpawnPrefab("(Prb)SingleBed.prefab", parent, "Camilla_Enfermeria_1", new Vector3(-18.4f, 0.01f, 22.0f), new Vector3(0f, 0f, 0f), new Vector3(0.85f, 0.85f, 0.85f));
        SnapToFloor(bed1, edificioRoot, 0.01f);

        GameObject bed2 = SpawnPrefab("(Prb)SingleBed.prefab", parent, "Camilla_Enfermeria_2", new Vector3(-18.4f, 0.01f, 26.0f), new Vector3(0f, 0f, 0f), new Vector3(0.85f, 0.85f, 0.85f));
        SnapToFloor(bed2, edificioRoot, 0.01f);

        GameObject buro = SpawnPrefab("(Prb)BedsideTable.prefab", parent, "Buro_Enfermeria_1", new Vector3(-18.1f, 0.01f, 24.0f), new Vector3(0f, 0f, 0f), new Vector3(2.0f, 2.0f, 2.0f));
        SnapToFloor(buro, edificioRoot, 0.01f);
        float buroTop = GetTopSurfaceLocalY(buro, edificioRoot, 0.8f);

        GameObject bLight = SpawnPrefab("(Prb)BedsideLight.prefab", parent, "LamparaBuro_Enfermeria", new Vector3(-18.1f, buroTop, 24.0f), Vector3.zero, new Vector3(1.3f, 1.3f, 1.3f));
        SnapToFloor(bLight, edificioRoot, buroTop);

        GameObject ward = SpawnPrefab("(Prb)Wardrobe.prefab", parent, "Casillero_Enfermeria", new Vector3(-10.2f, 0.01f, 24.0f), new Vector3(0f, 180f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
        SnapToFloor(ward, edificioRoot, 0.01f);
    }
}
