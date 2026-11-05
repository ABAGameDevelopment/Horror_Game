using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class Level3SceneryBuilder
{
    private const string ScenePath = "Assets/Scenes/Level3/Level3.unity";
    private const string SceneAltPath = "Assets/Scenes/Level3/University.unity";
    private const string SceneryVersionName = "Escenografia_Nivel3_LaCalle_v4";
    private const string EnemyGlbPath = "Assets/Prefabs/Enemy/Enemy.glb";

    // Prefabs de Vehículos
    private const string CarRedPrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Red Variant.prefab";
    private const string CarBluePrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Blue Variant.prefab";

    // Prefabs de Entorno
    private const string StreetLightPackPath = "Assets/Sat Productions/G-01/Street Lights Pack 01/Prefabs/";
    private const string ConiferTallPath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Tall BOTD URP.prefab";
    private const string ConiferMedPath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Medium BOTD URP.prefab";
    private const string ConiferBarePath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Bare BOTD URP.prefab";

    // Modelos 3D de Estructuras y Props
    private const string JerseyBarrierModelPath = "Assets/Models/Level2/JerseyBarrier.obj";
    private const string GuardBoothModelPath = "Assets/Models/Level2/GuardBooth.obj";
    private const string ParkingCanopyModelPath = "Assets/Models/Level2/ParkingCanopy.obj";
    private const string TrafficConeModelPath = "Assets/Models/Level2/TrafficCone.obj";
    private const string WoodCrateModelPath = "Assets/Models/Level2/WoodCrate.obj";
    private const string OilDrumModelPath = "Assets/Models/Level2/OilDrum.obj";
    private const string DumpsterModelPath = "Assets/Models/Level2/Dumpster.obj";
    private const string TireStackModelPath = "Assets/Models/Level2/TireStack.obj";
    private const string CarHoodModelPath = "Assets/Models/Level2/CarHood.obj";
    private const string CarEngineBayModelPath = "Assets/Models/Level2/CarEngineBay.obj";

    // Materiales y Texturas
    private const string MatAsfaltoPath = "Assets/Materials/Level2/Mat_AsfaltoEstacionamiento.mat";
    private const string MatLineasPath = "Assets/Materials/Level2/Mat_LineasAmarillas.mat";
    private const string MatCharcoAguaPath = "Assets/Materials/Level2/Mat_CharcoAgua.mat";
    private const string MatBarreraConcretoPath = "Assets/Materials/Level2/Mat_BarreraConcreto.mat";
    private const string MatMarquesinaPath = "Assets/Materials/Level2/Mat_MarquesinaMetal.mat";
    private const string MatCajaMaderaPath = "Assets/Materials/Level2/Mat_CajaMadera.mat";
    private const string MatBarrilRojoPath = "Assets/Materials/Level2/Mat_BarrilRojo.mat";
    private const string MatBarrilAceitePath = "Assets/Materials/Level2/Mat_BarrilAceite.mat";
    private const string MatContenedorPath = "Assets/Materials/Level2/Mat_ContenedorVerde.mat";
    private const string SkyMidnightPath = "Assets/Scenes/Level3/SkyMidnight.mat";

    // Fotos de Pistas
    private const string Nota1Path = "Assets/Photos/Nota1.png";
    private const string Nota2Path = "Assets/Photos/Nota2.png";
    private const string Nota3Path = "Assets/Photos/Nota3.png";

    static Level3SceneryBuilder()
    {
        EditorApplication.delayCall += AutoBuildIfNeeded;
    }

    private static void AutoBuildIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name == "Level3" || (!string.IsNullOrEmpty(activeScene.path) && activeScene.path.Contains("Level3")))
        {
            CleanLegacyUI();
            PurgeLevel2Residues();

            GameObject root = EncontrarRaizNivel3();
            if (root == null || !root.name.Contains(SceneryVersionName))
            {
                ConstruirEscenografiaCarretera();
            }
        }
    }

    private static GameObject EncontrarRaizNivel3()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (obj != null && obj.name.StartsWith("Escenografia_LaCalle_Nivel3"))
                return obj;
        }
        return null;
    }

    [MenuItem("Tools/Nivel 3/Purgar Residuos Nivel 2")]
    public static void PurgeLevel2Residues()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (obj == null) continue;

            if (obj.name.StartsWith("Escenografia_Estacionamiento") ||
                obj.name.StartsWith("Sistemas_Nivel2") ||
                obj.name.StartsWith("Canvas_TituloIntroNivel2") ||
                obj.name.StartsWith("HUD_Nivel2") ||
                obj.name.StartsWith("Canvas_Nivel2"))
            {
                Undo.DestroyObjectImmediate(obj);
                continue;
            }

            if (obj.GetComponent<CarEscapeController>() != null)
            {
                Undo.DestroyObjectImmediate(obj);
                continue;
            }

            if (obj.GetComponent<Level2ItemPickup>() != null)
            {
                Undo.DestroyObjectImmediate(obj);
                continue;
            }
        }
    }

    [MenuItem("Tools/Nivel 3/Construir Escenografia Carretera (Nivel 3)")]
    [MenuItem("Tools/Horror Game/Construir Escenografia Nivel 3")]
    public static void ConstruirEscenografiaCarretera()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "Level3" && (string.IsNullOrEmpty(activeScene.path) || !activeScene.path.Contains("Level3")))
        {
            if (EditorUtility.DisplayDialog("Cambiar de escena", "¿Deseas abrir la escena del Nivel 3?", "Sí", "No"))
            {
                if (System.IO.File.Exists(ScenePath))
                    EditorSceneManager.OpenScene(ScenePath);
                else if (System.IO.File.Exists(SceneAltPath))
                    EditorSceneManager.OpenScene(SceneAltPath);
            }
            else return;
        }

        Debug.Log("<color=orange>[Nivel 3]</color> Construyendo escenografía enriquecida de la Carretera (Nivel 3 — La Calle)...");

        CleanLegacyUI();
        HideOldInterior();
        PurgeLevel2Residues();

        // Destruir escenografías previas
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (obj != null && obj.name.StartsWith("Escenografia_LaCalle_Nivel3"))
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }

        // Purgar cualquier residuo de escenografía de estacionamiento
        PurgeLevel2Residues();

        // Crear raíz
        GameObject root = new GameObject("Escenografia_LaCalle_Nivel3_" + SceneryVersionName);
        Undo.RegisterCreatedObjectUndo(root, "Crear Escenografia Nivel 3");

        // Construir atmósfera y sistemas
        SetupAtmosphere(root);
        DisableLegacyLights(root);

        // Carretera, asfalto mojado, charcos y bosque
        BuildCarreteraYBosque(root);

        // Zona 1: Auto averiado del jugador (humo, radio, intermitentes, herramientas)
        GameObject autoAveriado = BuildAutoAveriado(root);

        // Zona 2: Hito kilométrico y Pista 1 (Fotografía del campus)
        BuildZonaHitoYPista1(root);

        // Zona 3: Accidente del camión / vehículo azul y Pista 2 (Reporte confidencial)
        BuildZonaAccidenteYPista2(root);

        // Zona 4: Silueta espectral del ente en la niebla (Jumpscare ambiental)
        GameObject siluetaEnte = BuildSiluetaEnte(root);

        // Monstruo oficial del Nivel 1 patrullando la carretera
        BuildMonstruoNivel1(root);

        // Zona 5: Parada Universitaria, teléfono y Pista 3 (Expediente clasificado)
        List<Light> farolasParada = new List<Light>();
        BuildZonaParadaYExpediente(root, farolasParada);

        // Zona 6: La Bicicleta de escape
        BuildBicicletaEscape(root);

        // Zona 7: Derrumbe / Puente colapsado (Bloqueo insalvable)
        // BuildBloqueoCarretera eliminado a peticion del usuario (camino libre)

        // Configurar gestor de eventos de terror de la carretera
        CarreteraEventosTerror eventos = root.AddComponent<CarreteraEventosTerror>();
        ConfigurarEventosTerror(eventos, autoAveriado.transform, siluetaEnte, farolasParada.ToArray());

        // Asegurar lector de documentos UI en la escena
        if (Object.FindAnyObjectByType<LectorDocumentoUI>() == null)
        {
            GameObject uiGo = new GameObject("LectorDocumentos_Manager");
            uiGo.transform.SetParent(root.transform);
            uiGo.AddComponent<LectorDocumentoUI>();
        }

        // Reposicionar al jugador
        UbicarJugador();

        // Hornear NavMesh para que el monstruo patrulle la carretera
        try
        {
            var surf = root.GetComponentInChildren<Unity.AI.Navigation.NavMeshSurface>();
            if (surf != null) surf.BuildNavMesh();
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("<color=green>[Nivel 3]</color> NavMesh horneado exitosamente sobre el asfalto.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Nivel 3] No se pudo hornear NavMesh automáticamente: " + ex.Message);
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("<color=green>[Nivel 3]</color> ¡Escenografía completada con éxito! Zonas: Auto Averiado, Hito, Accidente, Parada, Expediente y Bicicleta.");
    }

    private static void CleanLegacyUI()
    {
        string[] uiNames = {
            "CanvasObjetivos", "GestorObjetivos", "IntroLevel",
            "PanelNuevoObjetivo", "TextoObjetivo", "PanelObjetivosHUD",
            "PanelTutorialLinterna", "TextoTutorialLinterna", "Canvas_Nivel2", "HUD_Nivel2",
            "Canvas_TituloIntroNivel2", "Sistemas_Nivel2"
        };

        foreach (string n in uiNames)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }
    }

    private static void HideOldInterior()
    {
        string[] interiorObjects = {
            "EdificioV", "Classroom", "Audiovisual", "Walls", "Lamparas_Y_Luces",
            "Paredes_Divisorias_Oficinas", "Oficina_1", "Oficina_2", "Oficina_3", "Oficina_4",
            "Oficinas_Con_Ventanillas", "Muebles_Y_Objetos", "Cuadros_Y_Espejos",
            "Video_Sala_Audiovisual", "Decoracion_Edificio", "Escenografia_Estacionamiento"
        };

        foreach (string name in interiorObjects)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) obj.SetActive(false);
        }
    }

    private static void DisableLegacyLights(GameObject root)
    {
        GameObject player = GameObject.FindWithTag("Player");
        Transform playerTransform = player != null ? player.transform : null;

        Light[] luces = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Light luz in luces)
        {
            if (luz.transform.IsChildOf(root.transform)) continue;
            if (playerTransform != null && luz.transform.IsChildOf(playerTransform)) continue;
            luz.enabled = false;
        }
    }

    private static void SetupAtmosphere(GameObject root)
    {
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(SkyMidnightPath);
        if (skyMat != null) RenderSettings.skybox = skyMat;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.002f, 0.003f, 0.005f, 1f);
        RenderSettings.ambientIntensity = 0.04f;
        RenderSettings.reflectionIntensity = 0.02f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.002f, 0.003f, 0.006f, 1f); // Niebla gélida de terror
        RenderSettings.fogDensity = 0.038f;

        // Luz de luna fría
        GameObject moonGo = new GameObject("Luz_Luna_Carretera");
        moonGo.transform.SetParent(root.transform);
        moonGo.transform.rotation = Quaternion.Euler(52f, -38f, 0f);
        Light moon = moonGo.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.12f, 0.16f, 0.28f);
        moon.intensity = 0.08f;
        moon.shadows = LightShadows.Soft;
    }

    private static void BuildLimitesCarretera(GameObject env, GameObject jerseyPrefab, Material matBarrera)
    {
        // 1. Limite Final de la Carretera (Z = 168m)
        GameObject finCarretera = new GameObject("Zona_Limite_Fin_Carretera");
        finCarretera.transform.SetParent(env.transform);
        finCarretera.transform.position = new Vector3(0f, 0f, 168f);

        // Fila de barreras de concreto bloqueando la vía
        if (jerseyPrefab != null)
        {
            float[] barrierXs = { -6.5f, -3.25f, 0f, 3.25f, 6.5f };
            for (int i = 0; i < barrierXs.Length; i++)
            {
                GameObject b = Object.Instantiate(jerseyPrefab, new Vector3(barrierXs[i], 0f, 168f), Quaternion.identity, finCarretera.transform);
                b.name = "Barrera_FinCarretera_" + i;
                if (matBarrera != null) b.GetComponentInChildren<Renderer>().sharedMaterial = matBarrera;
            }
        }

        // Luces rojas y ambar de peligro intermitente / baliza en el bloqueo
        for (int lado = -1; lado <= 1; lado += 2)
        {
            GameObject luzPeligro = new GameObject("Luz_Peligro_FinCarretera");
            luzPeligro.transform.SetParent(finCarretera.transform);
            luzPeligro.transform.position = new Vector3(lado * 4.5f, 1.2f, 168f);
            Light lp = luzPeligro.AddComponent<Light>();
            lp.type = LightType.Point;
            lp.color = new Color(1f, 0.2f, 0.1f);
            lp.range = 7f;
            lp.intensity = 2.5f;
            luzPeligro.AddComponent<HorrorLightFlicker>().Configure(0.2f, 3.0f, 4.0f, 0.08f);
        }

        // Pared invisible física para impedir que el jugador cruce hacia el vacío
        GameObject colParedFin = new GameObject("Colisionador_FinCarretera");
        colParedFin.transform.SetParent(finCarretera.transform);
        colParedFin.transform.position = new Vector3(0f, 3f, 168.5f);
        BoxCollider bcFin = colParedFin.AddComponent<BoxCollider>();
        bcFin.size = new Vector3(26f, 8f, 2f);

        // 2. Limite Inicial detrás del auto del jugador (Z = -24m)
        GameObject iniCarretera = new GameObject("Zona_Limite_Inicio_Carretera");
        iniCarretera.transform.SetParent(env.transform);
        iniCarretera.transform.position = new Vector3(0f, 0f, -24f);

        if (jerseyPrefab != null)
        {
            float[] barrierXs = { -6.5f, -3.25f, 0f, 3.25f, 6.5f };
            for (int i = 0; i < barrierXs.Length; i++)
            {
                GameObject b = Object.Instantiate(jerseyPrefab, new Vector3(barrierXs[i], 0f, -24f), Quaternion.identity, iniCarretera.transform);
                b.name = "Barrera_InicioCarretera_" + i;
                if (matBarrera != null) b.GetComponentInChildren<Renderer>().sharedMaterial = matBarrera;
            }
        }

        GameObject colParedIni = new GameObject("Colisionador_InicioCarretera");
        colParedIni.transform.SetParent(iniCarretera.transform);
        colParedIni.transform.position = new Vector3(0f, 3f, -24.5f);
        BoxCollider bcIni = colParedIni.AddComponent<BoxCollider>();
        bcIni.size = new Vector3(26f, 8f, 2f);
    }

    private static void BuildCarreteraYBosque(GameObject root)
    {
        GameObject env = new GameObject("Carretera_Y_Entorno");
        env.transform.SetParent(root.transform);

        Material matAsfalto = AssetDatabase.LoadAssetAtPath<Material>(MatAsfaltoPath);
        Material matLineas = AssetDatabase.LoadAssetAtPath<Material>(MatLineasPath);
        Material matCharco = AssetDatabase.LoadAssetAtPath<Material>(MatCharcoAguaPath);
        Material matBarrera = AssetDatabase.LoadAssetAtPath<Material>(MatBarreraConcretoPath);

        // 0. Suelo base amplio del bosque y carretera (elimina cualquier vacio o triangulo de terreno)
        GameObject groundForest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        groundForest.name = "Suelo_Bosque_Carretera";
        groundForest.transform.SetParent(env.transform);
        groundForest.transform.position = new Vector3(0f, -0.2f, 75f);
        groundForest.transform.localScale = new Vector3(220f, 0.2f, 320f); // de X=-110 a 110, Z=-85 a 235
        if (matAsfalto != null) groundForest.GetComponent<Renderer>().sharedMaterial = matAsfalto;

        // Desactivar la malla con triangulos del terreno nativo si existe
        Terrain terrain = Object.FindAnyObjectByType<Terrain>();
        if (terrain != null)
        {
            if (matAsfalto != null) terrain.materialTemplate = matAsfalto;
            terrain.drawHeightmap = false;
        }

        // 1. Asfalto principal (14m de ancho x 210m de largo: de z = -30 a z = 180)
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Asfalto_Carretera";
        road.transform.SetParent(env.transform);
        road.transform.position = new Vector3(0f, -0.05f, 75f);
        road.transform.localScale = new Vector3(14f, 0.3f, 210f);
        if (matAsfalto != null) road.GetComponent<Renderer>().sharedMaterial = matAsfalto;
        road.isStatic = true;
        GameObjectUtility.SetStaticEditorFlags(road, StaticEditorFlags.NavigationStatic);
        var navSurf = road.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        navSurf.collectObjects = Unity.AI.Navigation.CollectObjects.All;
        navSurf.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;

        // 2. Acotamientos laterales (tierra / grava)
        for (int lado = -1; lado <= 1; lado += 2)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shoulder.name = (lado == -1) ? "Acotamiento_Izquierdo" : "Acotamiento_Derecho";
            shoulder.transform.SetParent(env.transform);
            shoulder.transform.position = new Vector3(lado * 9.5f, -0.1f, 75f);
            shoulder.transform.localScale = new Vector3(5f, 0.25f, 210f);
            if (matAsfalto != null) shoulder.GetComponent<Renderer>().sharedMaterial = matAsfalto;
        }

        // 3. Líneas amarillas centrales
        for (float z = -25f; z < 165f; z += 5.5f)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = "Linea_Amarilla_Centro";
            dash.transform.SetParent(env.transform);
            dash.transform.position = new Vector3(0f, 0.11f, z);
            dash.transform.localScale = new Vector3(0.22f, 0.02f, 3f);
            if (matLineas != null) dash.GetComponent<Renderer>().sharedMaterial = matLineas;
        }

        // 4. Líneas laterales blancas de acotamiento
        for (int lado = -1; lado <= 1; lado += 2)
        {
            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.name = "Linea_Blanca_Acotamiento";
            sideLine.transform.SetParent(env.transform);
            sideLine.transform.position = new Vector3(lado * 6.5f, 0.11f, 75f);
            sideLine.transform.localScale = new Vector3(0.18f, 0.02f, 210f);
        }

        // 5. Charcos de agua reflectantes sobre el asfalto mojado
        if (matCharco != null)
        {
            float[] puddleZs = { -15f, -2f, 18f, 45f, 68f, 92f, 115f, 138f, 155f };
            float[] puddleXs = { -1.5f, 1.2f, -2.5f, 3.1f, -1.8f, 2.4f, -3.2f, 0.5f, 2.0f };
            for (int i = 0; i < puddleZs.Length; i++)
            {
                GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puddle.name = "Charco_Agua_" + i;
                puddle.transform.SetParent(env.transform);
                puddle.transform.position = new Vector3(puddleXs[i], 0.12f, puddleZs[i]);
                puddle.transform.localScale = new Vector3(Random.Range(2.5f, 4.5f), 0.01f, Random.Range(3f, 6f));
                puddle.GetComponent<Renderer>().sharedMaterial = matCharco;
                Object.DestroyImmediate(puddle.GetComponent<Collider>());
            }
        }

        // 6. Barreras Jersey de concreto a los costados
        GameObject jerseyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(JerseyBarrierModelPath);
        if (jerseyPrefab != null)
        {
            for (float z = -20f; z <= 165f; z += 11f)
            {
                GameObject bIzq = Object.Instantiate(jerseyPrefab, new Vector3(-7.4f, 0f, z), Quaternion.Euler(0f, 90f, 0f), env.transform);
                bIzq.name = "Barrera_Jersey_Izq";
                if (matBarrera != null) bIzq.GetComponentInChildren<Renderer>().sharedMaterial = matBarrera;

                GameObject bDer = Object.Instantiate(jerseyPrefab, new Vector3(7.4f, 0f, z), Quaternion.Euler(0f, -90f, 0f), env.transform);
                bDer.name = "Barrera_Jersey_Der";
                if (matBarrera != null) bDer.GetComponentInChildren<Renderer>().sharedMaterial = matBarrera;
            }
        }

        // 7. Farolas de luz de sodio con parpadeo misterioso
        GameObject streetLightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StreetLightPackPath + "Street_Light_A_01.prefab");
        for (float z = -10f; z <= 155f; z += 28f)
        {
            float side = ((int)(z / 28f) % 2 == 0) ? 7.6f : -7.6f;
            float rotY = (side > 0) ? -90f : 90f;

            if (streetLightPrefab != null)
            {
                GameObject lamp = Object.Instantiate(streetLightPrefab, new Vector3(side, 0f, z), Quaternion.Euler(0f, rotY, 0f), env.transform);
                lamp.name = "Poste_Luz_" + (int)z;

                Light l = lamp.GetComponentInChildren<Light>();
                if (l != null)
                {
                    l.color = new Color(1f, 0.82f, 0.5f);
                    l.intensity = 1.9f;
                    l.range = 20f;
                    l.gameObject.AddComponent<HorrorLightFlicker>().Configure(0.15f, 2.2f, 7.5f, 0.045f);
                }
            }
        }

        // 8. Bosque de pinos densos en ambos flancos (Corredor tenebroso)
        GameObject coniferPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConiferTallPath);
        if (coniferPrefab == null) coniferPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConiferMedPath);

        if (coniferPrefab != null)
        {
            GameObject forest = new GameObject("Bosque_Carretera");
            forest.transform.SetParent(env.transform);

            for (float z = -30f; z <= 180f; z += 5f)
            {
                for (int capa = 1; capa <= 4; capa++)
                {
                    float distIzq = -11f - (capa * 4.2f) + Random.Range(-1.5f, 1.5f);
                    float distDer = 11f + (capa * 4.2f) + Random.Range(-1.5f, 1.5f);
                    float zOffset = z + Random.Range(-2f, 2f);

                    GameObject tIzq = Object.Instantiate(coniferPrefab, new Vector3(distIzq, 0f, zOffset), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), forest.transform);
                    tIzq.name = "Pino_Bosque";
                    tIzq.transform.localScale = Vector3.one * Random.Range(0.85f, 1.35f);

                    GameObject tDer = Object.Instantiate(coniferPrefab, new Vector3(distDer, 0f, zOffset), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), forest.transform);
                    tDer.name = "Pino_Bosque";
                    tDer.transform.localScale = Vector3.one * Random.Range(0.85f, 1.35f);
                }
            }
        }

        // 9. Limites físicos y visuales en los extremos de la carretera
        BuildLimitesCarretera(env, jerseyPrefab, matBarrera);
    }

    private static GameObject BuildAutoAveriado(GameObject root)
    {
        GameObject carStation = new GameObject("Estacion_Auto_Averiado");
        carStation.transform.SetParent(root.transform);

        // 1. Huellas de derrape (Skidmarks) en el asfalto antes de detenerse
        for (int i = 0; i < 4; i++)
        {
            GameObject skid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            skid.name = "Huella_Frenado";
            skid.transform.SetParent(carStation.transform);
            skid.transform.position = new Vector3(2.2f + (i % 2 == 0 ? -0.8f : 0.8f), 0.12f, -12f + (i * 3.5f));
            skid.transform.localScale = new Vector3(0.28f, 0.01f, 3f);
            skid.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MatAsfaltoPath);
            Object.DestroyImmediate(skid.GetComponent<Collider>());
        }

        // 2. Auto rojo del jugador cruzado
        GameObject carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarRedPrefabPath);
        GameObject car = null;
        if (carPrefab != null)
        {
            car = Object.Instantiate(carPrefab, new Vector3(2.8f, 0.05f, 3f), Quaternion.Euler(0f, 18f, 0f), carStation.transform);
            car.name = "Auto_Jugador_Averiado";

            car.AddComponent<CarBreakdownSequence>();

            // Luces delanteras alumbrando la niebla
            GameObject fIzq = new GameObject("Faro_Delantero_Izq");
            fIzq.transform.SetParent(car.transform);
            fIzq.transform.localPosition = new Vector3(-0.65f, 0.55f, 1.8f);
            Light lightFIzq = fIzq.AddComponent<Light>();
            lightFIzq.type = LightType.Spot;
            lightFIzq.color = new Color(1f, 0.95f, 0.85f);
            lightFIzq.intensity = 2.4f;
            lightFIzq.range = 25f;
            lightFIzq.spotAngle = 55f;

            GameObject fDer = new GameObject("Faro_Delantero_Der");
            fDer.transform.SetParent(car.transform);
            fDer.transform.localPosition = new Vector3(0.65f, 0.55f, 1.8f);
            Light lightFDer = fDer.AddComponent<Light>();
            lightFDer.type = LightType.Spot;
            lightFDer.color = new Color(1f, 0.95f, 0.85f);
            lightFDer.intensity = 2.4f;
            lightFDer.range = 25f;
            lightFDer.spotAngle = 55f;

            // Intermitentes ámbar
            CrearLuzIntermitente(car.transform, new Vector3(0.72f, 0.55f, 1.75f));
            CrearLuzIntermitente(car.transform, new Vector3(-0.72f, 0.55f, 1.75f));
            CrearLuzIntermitente(car.transform, new Vector3(0.72f, 0.55f, -1.85f));
            CrearLuzIntermitente(car.transform, new Vector3(-0.72f, 0.55f, -1.85f));

            // Motor Bay y Cofre abierto
            GameObject motorBayModel = AssetDatabase.LoadAssetAtPath<GameObject>(CarEngineBayModelPath);
            if (motorBayModel != null)
            {
                Object.Instantiate(motorBayModel, car.transform.position + new Vector3(0f, 0.5f, 1.1f), car.transform.rotation, carStation.transform);
            }

            GameObject hoodModel = AssetDatabase.LoadAssetAtPath<GameObject>(CarHoodModelPath);
            if (hoodModel != null)
            {
                GameObject hood = Object.Instantiate(hoodModel, car.transform.position + new Vector3(0f, 0.75f, 1.25f), car.transform.rotation * Quaternion.Euler(-38f, 0f, 0f), carStation.transform);
                hood.name = "Cofre_Abierto_Vapor";
            }
        }

        // 3. Conos de tráfico caídos detrás
        GameObject conePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrafficConeModelPath);
        if (conePrefab != null)
        {
            Object.Instantiate(conePrefab, new Vector3(2.6f, 0f, -3.5f), Quaternion.Euler(0f, 0f, 80f), carStation.transform);
            Object.Instantiate(conePrefab, new Vector3(1.7f, 0f, -8f), Quaternion.identity, carStation.transform);
        }

        return car != null ? car : carStation;
    }

    private static void CrearLuzIntermitente(Transform parent, Vector3 localPos)
    {
        GameObject lGo = new GameObject("Luz_Ambar_Intermitente");
        lGo.transform.SetParent(parent);
        lGo.transform.localPosition = localPos;
        Light l = lGo.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.55f, 0.05f);
        l.range = 4f;
        l.intensity = 2f;
    }

    private static void BuildZonaHitoYPista1(GameObject root)
    {
        GameObject z1 = new GameObject("Zona_1_Hito_Campus");
        z1.transform.SetParent(root.transform);
        z1.transform.position = new Vector3(-4.8f, 0f, 34f);

        // Letrero vial oxidado de desvío
        GameObject signPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        signPole.name = "Poste_Senal_Desvio";
        signPole.transform.SetParent(z1.transform);
        signPole.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        signPole.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);

        GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signBoard.name = "Letrero_Campus_Restringido";
        signBoard.transform.SetParent(z1.transform);
        signBoard.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        signBoard.transform.localScale = new Vector3(2.4f, 1.1f, 0.1f);
        signBoard.transform.localRotation = Quaternion.Euler(0f, 15f, 0f);

        // Hito kilométrico de concreto
        GameObject hito = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hito.name = "Hito_Km_Concreto";
        hito.transform.SetParent(z1.transform);
        hito.transform.localPosition = new Vector3(1.2f, 0.45f, 0.5f);
        hito.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);

        // PISTA 1: FOTOGRAFÍA DEL CAMPUS
        Sprite spNota1 = AssetDatabase.LoadAssetAtPath<Sprite>(Nota1Path);
        GameObject pista1 = new GameObject("Pista_1_FotografiaCampus");
        pista1.transform.SetParent(z1.transform);
        pista1.transform.localPosition = new Vector3(1.2f, 0.98f, 0.5f);

        GameObject docMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
        docMesh.name = "Malla_Foto";
        docMesh.transform.SetParent(pista1.transform);
        docMesh.transform.localPosition = Vector3.zero;
        docMesh.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        docMesh.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // Halo de luz misterioso sobre la foto
        GameObject halo = new GameObject("Luz_Halo_Pista1");
        halo.transform.SetParent(pista1.transform);
        halo.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        Light lHalo = halo.AddComponent<Light>();
        lHalo.type = LightType.Point;
        lHalo.color = new Color(1f, 0.85f, 0.4f);
        lHalo.range = 2.5f;
        lHalo.intensity = 1.2f;

        BoxCollider col1 = pista1.AddComponent<BoxCollider>();
        col1.isTrigger = true;
        col1.size = new Vector3(3f, 2.5f, 3f);

        PistaInteractuableLevel3 p1Comp = pista1.AddComponent<PistaInteractuableLevel3>();
        ConfigurarPista(
            p1Comp,
            "Fotografía Aérea del Campus (Octubre 2026)",
            "En el reverso dice con tinta apresurada:\n\n'La anomalía no fue un fallo del sistema. La liberaron desde el sótano del edificio central cuando cerraron los accesos. Quien encuentre esto: no intentes salir del perímetro, la criatura vigila las fronteras. La respuesta está en el laboratorio subterráneo.'",
            false,
            spNota1
        );
    }

    private static void BuildZonaAccidenteYPista2(GameObject root)
    {
        GameObject z2 = new GameObject("Zona_2_Accidente_VehiculoSuministros");
        z2.transform.SetParent(root.transform);
        z2.transform.position = new Vector3(0f, 0f, 75f);

        // 1. Vehículo de suministros azul chocado contra las barreras de la derecha
        GameObject carBluePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarBluePrefabPath);
        if (carBluePrefab != null)
        {
            GameObject crashCar = Object.Instantiate(carBluePrefab, new Vector3(6.5f, 0.4f, 75f), Quaternion.Euler(15f, -40f, -12f), z2.transform);
            crashCar.name = "Furgoneta_Suministros_Chocada";

            // Faro parpadeante roto
            GameObject brokenLight = new GameObject("Luz_Chasis_Parpadeante");
            brokenLight.transform.SetParent(crashCar.transform);
            brokenLight.transform.localPosition = new Vector3(-0.6f, 0.5f, 1.6f);
            Light bL = brokenLight.AddComponent<Light>();
            bL.type = LightType.Spot;
            bL.color = new Color(1f, 0.9f, 0.7f);
            bL.intensity = 2f;
            bL.range = 14f;
            brokenLight.AddComponent<HorrorLightFlicker>().Configure(0.1f, 2.5f, 12f, 0.08f);
        }

        // 2. Cajas de suministros volcadas y bidones
        GameObject cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WoodCrateModelPath);
        if (cratePrefab != null)
        {
            Object.Instantiate(cratePrefab, new Vector3(6.8f, 0f, 73f), Quaternion.Euler(0f, 22f, 0f), z2.transform);
            Object.Instantiate(cratePrefab, new Vector3(7.2f, 0.4f, 73.2f), Quaternion.Euler(0f, -15f, 0f), z2.transform);
            Object.Instantiate(cratePrefab, new Vector3(6.2f, 0f, 77f), Quaternion.Euler(20f, 45f, 0f), z2.transform);
        }

        GameObject drumPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OilDrumModelPath);
        if (drumPrefab != null)
        {
            Object.Instantiate(drumPrefab, new Vector3(6.2f, 0.35f, 76f), Quaternion.Euler(0f, 0f, 90f), z2.transform);
            Object.Instantiate(drumPrefab, new Vector3(6.5f, 0f, 74f), Quaternion.identity, z2.transform);
        }

        GameObject dumpsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DumpsterModelPath);
        if (dumpsterPrefab != null)
        {
            Object.Instantiate(dumpsterPrefab, new Vector3(7.8f, 0f, 79f), Quaternion.Euler(0f, -110f, 0f), z2.transform);
        }

        // PISTA 2: REPORTE DE INCIDENTE BIOLÓGICO
        Sprite spNota2 = AssetDatabase.LoadAssetAtPath<Sprite>(Nota2Path);
        GameObject pista2 = new GameObject("Pista_2_ReporteIncidente");
        pista2.transform.SetParent(z2.transform);
        pista2.transform.position = new Vector3(6.8f, 0.85f, 73.2f);

        GameObject docMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
        docMesh.name = "Malla_Reporte";
        docMesh.transform.SetParent(pista2.transform);
        docMesh.transform.localPosition = Vector3.zero;
        docMesh.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        docMesh.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        BoxCollider col2 = pista2.AddComponent<BoxCollider>();
        col2.isTrigger = true;
        col2.size = new Vector3(3f, 2.5f, 3f);

        PistaInteractuableLevel3 p2Comp = pista2.AddComponent<PistaInteractuableLevel3>();
        ConfigurarPista(
            p2Comp,
            "Reporte Médico de Incidente — División Biológica",
            "INFORME PRELIMINAR:\n\n'El espécimen 07 ha roto el confinamiento de nivel 4. Muestra un comportamiento altamente fotosensible y agresivo hacia ondas electromagnéticas. El transporte intentó evacuar al personal hacia la carretera, pero la criatura nos emboscó antes del puente.\n\nADVERTENCIA: Si el ente escapa a la ciudad, será imparable. El único protocolo de neutralización está registrado en el expediente confidencial de la parada de transbordo.'",
            false,
            spNota2
        );
    }

    private static GameObject BuildSiluetaEnte(GameObject root)
    {
        // Silueta misteriosa que se asoma entre los árboles en z = 68m a la izquierda
        GameObject silueta = new GameObject("Silueta_Ente_Niebla");
        silueta.transform.SetParent(root.transform);
        silueta.transform.position = new Vector3(-13.5f, 0f, 68f);

        // Cuerpo alto y tenebroso
        GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cuerpo.name = "Sombra_Cuerpo";
        cuerpo.transform.SetParent(silueta.transform);
        cuerpo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cuerpo.transform.localScale = new Vector3(0.55f, 1.6f, 0.55f);
        cuerpo.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MatAsfaltoPath);

        // Ojos brillantes rojos en la oscuridad
        for (int ojo = -1; ojo <= 1; ojo += 2)
        {
            GameObject ojoGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ojoGo.name = "Ojo_Ente";
            ojoGo.transform.SetParent(silueta.transform);
            ojoGo.transform.localPosition = new Vector3(ojo * 0.14f, 2.85f, 0.28f);
            ojoGo.transform.localScale = Vector3.one * 0.08f;
        }

        GameObject ojoLuz = new GameObject("Luz_Ojos_Ente");
        ojoLuz.transform.SetParent(silueta.transform);
        ojoLuz.transform.localPosition = new Vector3(0f, 2.85f, 0.4f);
        Light lOjos = ojoLuz.AddComponent<Light>();
        lOjos.type = LightType.Point;
        lOjos.color = new Color(0.95f, 0.15f, 0.15f);
        lOjos.range = 3.5f;
        lOjos.intensity = 1.6f;

        silueta.SetActive(false);
        return silueta;
    }

    private static GameObject BuildMonstruoNivel1(GameObject root)
    {
        // 1. GameObject Enemy idéntico al Nivel 1 (de pie, vertical, escala 1.5)
        GameObject contenedorMonstruo = new GameObject("Enemy");
        contenedorMonstruo.transform.SetParent(root.transform);
        contenedorMonstruo.transform.position = new Vector3(-4.5f, 1.5f, 52f);
        contenedorMonstruo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        contenedorMonstruo.transform.localScale = Vector3.one * 1.5f;

        // Asignación directa de MeshFilter y MeshRenderer (como en Nivel 1, de pie sin la rotación del GLB)
        MeshFilter mf = contenedorMonstruo.AddComponent<MeshFilter>();
        MeshRenderer mr = contenedorMonstruo.AddComponent<MeshRenderer>();
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(EnemyGlbPath);
        foreach (var a in allAssets)
        {
            if (a is Mesh m && mf.sharedMesh == null) mf.sharedMesh = m;
            if (a is Material mat && mr.sharedMaterial == null) mr.sharedMaterial = mat;
        }

        // 2. Colisionador físico
        CapsuleCollider col = contenedorMonstruo.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0f, 0f);
        col.radius = 0.35f;
        col.height = 1.99f;

        // 3. NavMeshAgent idéntico a Nivel 1
        UnityEngine.AI.NavMeshAgent agent = contenedorMonstruo.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.radius = 0.35f;
        agent.height = 1.9923558f;
        agent.baseOffset = 0.9961779f;
        agent.speed = 1.8f;
        agent.acceleration = 8f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1.2f;

        // 4. Punto de visión para raycast a la misma altura de Nivel 1
        GameObject pvGo = new GameObject("PuntoVision");
        pvGo.transform.SetParent(contenedorMonstruo.transform);
        pvGo.transform.localPosition = new Vector3(0f, 0.81f, 0f);

        // 5. Audio 3D en el monstruo
        AudioSource audioMonstruo = contenedorMonstruo.AddComponent<AudioSource>();
        audioMonstruo.spatialBlend = 1.0f; // 3D
        audioMonstruo.minDistance = 2f;
        audioMonstruo.maxDistance = 25f;
        audioMonstruo.loop = true;
        audioMonstruo.volume = 0.55f;
        audioMonstruo.rolloffMode = AudioRolloffMode.Logarithmic;

        AudioClip clipMujer = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/MujerLlorando.mp3");
        if (clipMujer != null)
        {
            audioMonstruo.clip = clipMujer;
            audioMonstruo.Play();
        }

        // 6. Puntos de Patrullaje distribuidos por la carretera
        GameObject grupoPuntos = new GameObject("Puntos_Patrullaje_Monstruo");
        grupoPuntos.transform.SetParent(root.transform);

        Vector3[] posicionesPatrulla = new Vector3[]
        {
            new Vector3(-3.5f, 1.5f, 18f),  // Cerca del auto averiado y el hito
            new Vector3(4.0f,  1.5f, 42f),  // Curva abierta
            new Vector3(-4.5f, 1.5f, 68f),  // Choque de camion / auto azul
            new Vector3(3.0f,  1.5f, 92f),  // Tramo oscuro entre coniferas
            new Vector3(-3.0f, 1.5f, 118f), // Frente a la caseta con el expediente
            new Vector3(1.5f,  1.5f, 138f)  // Cerca del derrumbe y la bicicleta
        };

        Transform[] transformsPuntos = new Transform[posicionesPatrulla.Length];
        for (int i = 0; i < posicionesPatrulla.Length; i++)
        {
            GameObject pObj = new GameObject($"PuntoPatrulla_{i + 1}");
            pObj.transform.SetParent(grupoPuntos.transform);
            pObj.transform.position = posicionesPatrulla[i];
            transformsPuntos[i] = pObj.transform;
        }

        // 7. Sistema de MuerteJugador (screamer jumpscare y pantalla de derrota)
        MuerteJugador muerte = BuildMuerteJugador(root, contenedorMonstruo.transform, audioMonstruo);

        // 8. Componente MonstruoIA original de Nivel 1
        MonstruoIA ia = contenedorMonstruo.AddComponent<MonstruoIA>();
        ConfigurarMonstruoIA(ia, agent, pvGo.transform, transformsPuntos, muerte);

        Debug.Log("<color=green>[Nivel 3]</color> Monstruo original del Nivel 1 de pie y con screamer añadido con éxito.");
        return contenedorMonstruo;
    }

    private static MuerteJugador BuildMuerteJugador(GameObject root, Transform monstruo, AudioSource audioMonstruo)
    {
        GameObject canvasGo = new GameObject("Canvas_MuerteJugador");
        canvasGo.transform.SetParent(root.transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        UnityEngine.UI.CanvasScaler scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 1. Panel Oscuro para fade out
        GameObject poGo = new GameObject("PanelOscuro");
        poGo.transform.SetParent(canvasGo.transform, false);
        UnityEngine.UI.Image imgOscuro = poGo.AddComponent<UnityEngine.UI.Image>();
        imgOscuro.color = new Color(0f, 0f, 0f, 0f);
        imgOscuro.raycastTarget = false;
        RectTransform rtOscuro = poGo.GetComponent<RectTransform>();
        rtOscuro.anchorMin = Vector2.zero;
        rtOscuro.anchorMax = Vector2.one;
        rtOscuro.offsetMin = Vector2.zero;
        rtOscuro.offsetMax = Vector2.zero;

        // 2. Panel Muerte (Ventana de derrota con botones)
        GameObject pmGo = new GameObject("PanelMuerte");
        pmGo.transform.SetParent(canvasGo.transform, false);
        UnityEngine.UI.Image imgMuerte = pmGo.AddComponent<UnityEngine.UI.Image>();
        imgMuerte.color = new Color(0.02f, 0.02f, 0.02f, 0.95f);
        RectTransform rtMuerte = pmGo.GetComponent<RectTransform>();
        rtMuerte.anchorMin = Vector2.zero;
        rtMuerte.anchorMax = Vector2.one;
        rtMuerte.offsetMin = Vector2.zero;
        rtMuerte.offsetMax = Vector2.zero;

        // Texto "HAS CAÍDO EN LA OSCURIDAD"
        GameObject txtGo = new GameObject("TextoMuerte");
        txtGo.transform.SetParent(pmGo.transform, false);
        var txt = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
        txt.font = RuntimeUI.FuenteTitulo;
        txt.text = "HAS CAÍDO EN LA OSCURIDAD";
        txt.fontSize = 58f;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
        rtTxt.anchorMin = new Vector2(0.5f, 0.65f);
        rtTxt.anchorMax = new Vector2(0.5f, 0.65f);
        rtTxt.sizeDelta = new Vector2(900f, 100f);

        // Botón Reintentar
        GameObject btnReintentar = new GameObject("BtnReintentar");
        btnReintentar.transform.SetParent(pmGo.transform, false);
        UnityEngine.UI.Image imgBtnR = btnReintentar.AddComponent<UnityEngine.UI.Image>();
        imgBtnR.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        UnityEngine.UI.Button bR = btnReintentar.AddComponent<UnityEngine.UI.Button>();
        RectTransform rtBtnR = btnReintentar.GetComponent<RectTransform>();
        rtBtnR.anchorMin = new Vector2(0.5f, 0.45f);
        rtBtnR.anchorMax = new Vector2(0.5f, 0.45f);
        rtBtnR.sizeDelta = new Vector2(300f, 60f);

        GameObject txtBtnR = new GameObject("Texto");
        txtBtnR.transform.SetParent(btnReintentar.transform, false);
        var tR = txtBtnR.AddComponent<TMPro.TextMeshProUGUI>();
        tR.font = RuntimeUI.FuenteTexto;
        tR.text = "REINTENTAR";
        tR.fontSize = 28f;
        tR.alignment = TMPro.TextAlignmentOptions.Center;
        tR.color = Color.white;
        RectTransform rtTR = txtBtnR.GetComponent<RectTransform>();
        rtTR.anchorMin = Vector2.zero;
        rtTR.anchorMax = Vector2.one;
        rtTR.offsetMin = Vector2.zero;
        rtTR.offsetMax = Vector2.zero;

        // Botón Menú Principal
        GameObject btnMenu = new GameObject("BtnMenu");
        btnMenu.transform.SetParent(pmGo.transform, false);
        UnityEngine.UI.Image imgBtnM = btnMenu.AddComponent<UnityEngine.UI.Image>();
        imgBtnM.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        UnityEngine.UI.Button bM = btnMenu.AddComponent<UnityEngine.UI.Button>();
        RectTransform rtBtnM = btnMenu.GetComponent<RectTransform>();
        rtBtnM.anchorMin = new Vector2(0.5f, 0.33f);
        rtBtnM.anchorMax = new Vector2(0.5f, 0.33f);
        rtBtnM.sizeDelta = new Vector2(300f, 60f);

        GameObject txtBtnM = new GameObject("Texto");
        txtBtnM.transform.SetParent(btnMenu.transform, false);
        var tM = txtBtnM.AddComponent<TMPro.TextMeshProUGUI>();
        tM.font = RuntimeUI.FuenteTexto;
        tM.text = "MENÚ PRINCIPAL";
        tM.fontSize = 28f;
        tM.alignment = TMPro.TextAlignmentOptions.Center;
        tM.color = Color.white;
        RectTransform rtTM = txtBtnM.GetComponent<RectTransform>();
        rtTM.anchorMin = Vector2.zero;
        rtTM.anchorMax = Vector2.one;
        rtTM.offsetMin = Vector2.zero;
        rtTM.offsetMax = Vector2.zero;

        pmGo.SetActive(false);

        // Componente MuerteJugador
        GameObject goControl = new GameObject("ControlMuerte");
        goControl.transform.SetParent(canvasGo.transform, false);
        MuerteJugador mj = goControl.AddComponent<MuerteJugador>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        Transform camTransform = Camera.main != null ? Camera.main.transform : (player != null ? player.transform.Find("Camera") : null);
        AudioClip clipScreamer = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Screamer.mp3");

        var so = new SerializedObject(mj);
        if (player != null)
        {
            so.FindProperty("playerController").objectReferenceValue = player.GetComponent<PlayerController>();
            so.FindProperty("playerInput").objectReferenceValue = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            so.FindProperty("characterController").objectReferenceValue = player.GetComponent<CharacterController>();
        }
        if (camTransform != null)
        {
            so.FindProperty("camaraJugador").objectReferenceValue = camTransform;
        }
        AudioSource directScreamer = (camTransform != null ? camTransform.gameObject : goControl).AddComponent<AudioSource>();
        directScreamer.spatialBlend = 0f;
        directScreamer.volume = 1f;
        directScreamer.playOnAwake = false;
        so.FindProperty("audioScreamer").objectReferenceValue = directScreamer;
        so.FindProperty("sonidoScreamer").objectReferenceValue = clipScreamer;
        so.FindProperty("duracionScreamer").floatValue = 2.5f;
        so.FindProperty("posicionScreamer").vector3Value = new Vector3(0f, -1.1f, 0.95f);
        so.FindProperty("distanciaAtaque").floatValue = 0.35f;
        so.FindProperty("velocidadAtaque").floatValue = 8f;
        so.FindProperty("panelOscuro").objectReferenceValue = imgOscuro;
        so.FindProperty("panelMuerte").objectReferenceValue = pmGo;
        so.FindProperty("duracionDesvanecimiento").floatValue = 1.5f;
        so.FindProperty("escenaReintentar").stringValue = "Level3";
        so.FindProperty("escenaMenuPrincipal").stringValue = "MainMenu";
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(bR.onClick, mj.Reintentar);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(bM.onClick, mj.IrAlMenuPrincipal);

        return mj;
    }

    private static void ConfigurarMonstruoIA(MonstruoIA ia, UnityEngine.AI.NavMeshAgent agent, Transform puntoVision, Transform[] waypoints, MuerteJugador muerte)
    {
        var so = new SerializedObject(ia);
        so.FindProperty("agente").objectReferenceValue = agent;
        so.FindProperty("puntoVision").objectReferenceValue = puntoVision;
        so.FindProperty("velocidadPatrulla").floatValue = 1.5f;
        so.FindProperty("velocidadPersecucion").floatValue = 3.5f;
        so.FindProperty("distanciaVision").floatValue = 11.5f;
        so.FindProperty("anguloVision").floatValue = 100f;
        so.FindProperty("distanciaPerdida").floatValue = 20f;
        so.FindProperty("tiempoBusqueda").floatValue = 4.5f;
        so.FindProperty("distanciaCaptura").floatValue = 1.5f;

        SerializedProperty propPuntos = so.FindProperty("puntosPatrullaje");
        propPuntos.arraySize = waypoints.Length;
        for (int i = 0; i < waypoints.Length; i++)
        {
            propPuntos.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            so.FindProperty("jugador").objectReferenceValue = player.transform;
        }

        if (muerte != null)
        {
            so.FindProperty("muerteJugador").objectReferenceValue = muerte;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildZonaParadaYExpediente(GameObject root, List<Light> farolasParada)
    {
        GameObject z4 = new GameObject("Zona_4_ParadaUniversitaria_Y_Expediente");
        z4.transform.SetParent(root.transform);
        z4.transform.position = new Vector3(0f, 0f, 122f);

        Material matBarrilRojo = AssetDatabase.LoadAssetAtPath<Material>(MatBarrilRojoPath);
        Material matCajaMadera = AssetDatabase.LoadAssetAtPath<Material>(MatCajaMaderaPath);

        // 1. Marquesina metálica de autobús
        GameObject canopyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ParkingCanopyModelPath);
        if (canopyPrefab != null)
        {
            GameObject canopy = Object.Instantiate(canopyPrefab, new Vector3(6.2f, 0f, 122f), Quaternion.Euler(0f, -90f, 0f), z4.transform);
            canopy.name = "Marquesina_Parada_Bus";
        }

        // 2. Caseta de vigilancia lateral
        GameObject boothPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GuardBoothModelPath);
        if (boothPrefab != null)
        {
            GameObject booth = Object.Instantiate(boothPrefab, new Vector3(8.2f, 0f, 127f), Quaternion.Euler(0f, -90f, 0f), z4.transform);
            booth.name = "Caseta_Telefonica";
        }

        // 3. Mesa / Banco dentro de la parada (posicionado correctamente bajo la marquesina en Z=122)
        GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bench.name = "Banco_Espera";
        bench.transform.SetParent(z4.transform);
        bench.transform.position = new Vector3(6.2f, 0.45f, 122f);
        bench.transform.localScale = new Vector3(1.2f, 0.45f, 2.5f);
        if (matCajaMadera != null) bench.GetComponent<Renderer>().sharedMaterial = matCajaMadera;

        // 4. Farola potente sobre la caseta iluminando toda la parada y la carretera
        GameObject lampGo = new GameObject("Luz_Parada_Autobus");
        lampGo.transform.SetParent(z4.transform);
        lampGo.transform.position = new Vector3(6.2f, 3.5f, 122f);
        Light lampLight = lampGo.AddComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = new Color(1f, 0.92f, 0.72f);
        lampLight.intensity = 3.2f;
        lampLight.range = 18f;
        lampGo.AddComponent<HorrorLightFlicker>().Configure(0.2f, 3.4f, 7.5f, 0.05f);
        farolasParada.Add(lampLight);

        // 5. Señal vial de parada de autobús al borde del asfalto
        GameObject busPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        busPole.name = "Poste_Senal_Parada";
        busPole.transform.SetParent(z4.transform);
        busPole.transform.position = new Vector3(5.2f, 1.3f, 119.5f);
        busPole.transform.localScale = new Vector3(0.08f, 1.3f, 0.08f);

        GameObject signPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signPlate.name = "Placa_Parada_Autobus";
        signPlate.transform.SetParent(z4.transform);
        signPlate.transform.position = new Vector3(5.2f, 2.3f, 119.5f);
        signPlate.transform.localScale = new Vector3(0.1f, 0.7f, 0.9f);
        if (matBarrilRojo != null) signPlate.GetComponent<Renderer>().sharedMaterial = matBarrilRojo;

        // PISTA 3: EL EXPEDIENTE CLASIFICADO (Carpeta roja con papeles y luz dorada)
        Sprite spNota3 = AssetDatabase.LoadAssetAtPath<Sprite>(Nota3Path);
        GameObject expObj = new GameObject("Pista_3_ExpedienteClasificado");
        expObj.transform.SetParent(z4.transform);
        expObj.transform.position = new Vector3(6.2f, 0.72f, 122f);

        // Carpeta roja
        GameObject folder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        folder.name = "Carpeta_Expediente_Rojo";
        folder.transform.SetParent(expObj.transform);
        folder.transform.localPosition = Vector3.zero;
        folder.transform.localScale = new Vector3(0.55f, 0.06f, 0.42f);
        folder.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
        if (matBarrilRojo != null) folder.GetComponent<Renderer>().sharedMaterial = matBarrilRojo;

        // Hojas de expediente asomando
        GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paper.name = "Hojas_Expediente";
        paper.transform.SetParent(expObj.transform);
        paper.transform.localPosition = new Vector3(0.02f, 0.04f, 0.02f);
        paper.transform.localScale = new Vector3(0.48f, 0.02f, 0.38f);
        paper.transform.localRotation = Quaternion.Euler(0f, 24f, 0f);

        // Baliza de luz dorada brillante sobre el expediente (muy visible desde la carretera)
        GameObject halo = new GameObject("Luz_Beacon_Pista3");
        halo.transform.SetParent(expObj.transform);
        halo.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        Light lHalo = halo.AddComponent<Light>();
        lHalo.type = LightType.Point;
        lHalo.color = new Color(1f, 0.82f, 0.25f);
        lHalo.range = 6f;
        lHalo.intensity = 3.5f;

        BoxCollider expCol = expObj.AddComponent<BoxCollider>();
        expCol.isTrigger = true;
        expCol.size = new Vector3(3.5f, 2.5f, 3.5f);

        PistaInteractuableLevel3 pExpComp = expObj.AddComponent<PistaInteractuableLevel3>();
        ConfigurarPista(
            pExpComp,
            "Expediente Clasificado — Protocolo Génesis",
            "DOCUMENTO FINAL DE SEGURIDAD // CONFIDENCIAL:\n\n'La criatura está enlazada al reactor experimental del sótano de la universidad. Mientras el generador auxiliar siga activo, el ente se regenerará indefinidamente y no podrá morir.\n\nCONCLUSIÓN INELUDIBLE:\nHuir por la carretera es inútil; la carretera hacia la ciudad ha sido destruida por el ejército para contener el brote. La ÚNICA forma de sobrevivir y detenerlo para siempre es REGRESAR A LA UNIVERSIDAD, infiltrarse en el laboratorio y apagar el núcleo.'",
            true,
            spNota3
        );
    }

    private static void BuildBicicletaEscape(GameObject root)
    {
        GameObject biciZone = new GameObject("Zona_5_Bicicleta_Regreso");
        biciZone.transform.SetParent(root.transform);
        biciZone.transform.position = new Vector3(5.2f, 0f, 127f);

        // Bicicleta apoyada en la barrera Jersey
        GameObject biciObj = new GameObject("Bicicleta_Regreso_Universidad");
        biciObj.transform.SetParent(biciZone.transform);
        biciObj.transform.localPosition = Vector3.zero;
        biciObj.transform.localRotation = Quaternion.Euler(0f, 185f, 6f); // Apuntando hacia atrás (hacia la universidad)

        // Rueda Trasera
        GameObject ruedaT = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ruedaT.name = "Rueda_Trasera";
        ruedaT.transform.SetParent(biciObj.transform);
        ruedaT.transform.localPosition = new Vector3(0f, 0.35f, -0.65f);
        ruedaT.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        ruedaT.transform.localScale = new Vector3(0.72f, 0.06f, 0.72f);

        // Rueda Delantera
        GameObject ruedaD = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ruedaD.name = "Rueda_Delantera";
        ruedaD.transform.SetParent(biciObj.transform);
        ruedaD.transform.localPosition = new Vector3(0f, 0.35f, 0.65f);
        ruedaD.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        ruedaD.transform.localScale = new Vector3(0.72f, 0.06f, 0.72f);

        // Cuadro
        GameObject cuadro = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cuadro.name = "Cuadro";
        cuadro.transform.SetParent(biciObj.transform);
        cuadro.transform.localPosition = new Vector3(0f, 0.58f, 0f);
        cuadro.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
        cuadro.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);

        // Manillar
        GameObject manillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        manillar.name = "Manillar";
        manillar.transform.SetParent(biciObj.transform);
        manillar.transform.localPosition = new Vector3(0f, 0.95f, 0.55f);
        manillar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        manillar.transform.localScale = new Vector3(0.06f, 0.38f, 0.06f);

        // Asiento
        GameObject asiento = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asiento.name = "Asiento";
        asiento.transform.SetParent(biciObj.transform);
        asiento.transform.localPosition = new Vector3(0f, 0.85f, -0.25f);
        asiento.transform.localScale = new Vector3(0.22f, 0.06f, 0.3f);

        // Farol de dinamo delantero que ilumina el camino de regreso
        GameObject farolGo = new GameObject("Farol_Dinamo_Bicicleta");
        farolGo.transform.SetParent(biciObj.transform);
        farolGo.transform.localPosition = new Vector3(0f, 0.95f, 0.7f);
        Light farolLight = farolGo.AddComponent<Light>();
        farolLight.type = LightType.Spot;
        farolLight.color = new Color(0.95f, 0.98f, 1f);
        farolLight.intensity = 3.5f;
        farolLight.range = 35f;
        farolLight.spotAngle = 55f;
        farolGo.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);

        // Luz de baliza cian sobre la bicicleta (indicador visual de escape)
        GameObject beaconBici = new GameObject("Luz_Beacon_Bicicleta");
        beaconBici.transform.SetParent(biciObj.transform);
        beaconBici.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light lBici = beaconBici.AddComponent<Light>();
        lBici.type = LightType.Point;
        lBici.color = new Color(0.3f, 0.85f, 1f);
        lBici.range = 6.5f;
        lBici.intensity = 2.5f;

        // Eliminar colliders de partes
        Collider[] childCols = biciObj.GetComponentsInChildren<Collider>();
        foreach (var c in childCols)
        {
            if (c.gameObject != biciObj) Object.DestroyImmediate(c);
        }

        BoxCollider triggerBici = biciObj.AddComponent<BoxCollider>();
        triggerBici.isTrigger = true;
        triggerBici.center = new Vector3(0f, 0.6f, 0f);
        triggerBici.size = new Vector3(2.5f, 2f, 2.5f);

        biciObj.AddComponent<BicicletaInteractuable>();
    }

    private static void BuildBloqueoCarretera(GameObject root)
    {
        GameObject blockZone = new GameObject("Zona_6_Bloqueo_PuenteColapsado");
        blockZone.transform.SetParent(root.transform);
        blockZone.transform.position = new Vector3(0f, 0f, 150f);

        // Rocas masivas que sellan la carretera
        for (int i = -4; i <= 4; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Roca_Derrumbe";
            rock.transform.SetParent(blockZone.transform);
            rock.transform.localPosition = new Vector3(i * 1.9f, Random.Range(1.2f, 2.5f), Random.Range(-1.5f, 2f));
            rock.transform.localScale = new Vector3(Random.Range(2.2f, 3.8f), Random.Range(2.5f, 4f), Random.Range(2.2f, 3.8f));
            rock.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f));
        }

        // Señal grande de PUENTE COLAPSADO
        GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signBoard.name = "Senal_Puente_Colapsado";
        signBoard.transform.SetParent(blockZone.transform);
        signBoard.transform.localPosition = new Vector3(0f, 1.8f, -4f);
        signBoard.transform.localScale = new Vector3(4.5f, 1.5f, 0.15f);

        // Troncos cruzados
        for (int t = 0; t < 3; t++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "Tronco_Caido";
            log.transform.SetParent(blockZone.transform);
            log.transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f + (t * 0.4f), -2f + (t * 1.2f));
            log.transform.localRotation = Quaternion.Euler(0f, Random.Range(30f, 75f), 90f);
            log.transform.localScale = new Vector3(0.45f, 4.5f, 0.45f);
        }
    }

    private static void UbicarJugador()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player != null)
        {
            player.tag = "Player";

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = true;

            // Posicion con altura suficiente (Y = 1.25f) para que el CharacterController (altura 2m, centro 0m)
            // quede reposando sobre el asfalto (Y = 0.10f) sin quedar atascado dentro del colisionador.
            player.transform.position = new Vector3(1.1f, 1.25f, -1.5f);
            player.transform.rotation = Quaternion.Euler(0f, 8f, 0f);

            if (cc != null) cc.enabled = true;

            // Desactivar y destruir IntroNivel residual de Nivel 1 en Nivel 3 para garantizar control inmediato
            IntroNivel intro = player.GetComponentInChildren<IntroNivel>(true);
            if (intro != null)
            {
                intro.enabled = false;
                if (intro.gameObject != player)
                    Undo.DestroyObjectImmediate(intro.gameObject);
                else
                    Undo.DestroyObjectImmediate(intro);
            }

            Debug.Log("<color=green>[Nivel 3]</color> Jugador posicionado y desbloqueado correctamente a un costado del auto averiado.");
        }
    }

    private static void ConfigurarPista(PistaInteractuableLevel3 pista, string nombre, string descripcion, bool esExpediente, Sprite img)
    {
        var so = new SerializedObject(pista);
        so.FindProperty("nombrePista").stringValue = nombre;
        so.FindProperty("descripcionPista").stringValue = descripcion;
        so.FindProperty("esExpedientePrincipal").boolValue = esExpediente;
        if (img != null) so.FindProperty("imagenNota").objectReferenceValue = img;
        so.ApplyModifiedProperties();
    }

    private static void ConfigurarEventosTerror(CarreteraEventosTerror eventos, Transform autoTransform, GameObject silueta, Light[] farolas)
    {
        var so = new SerializedObject(eventos);
        so.FindProperty("posicionAutoAveriado").objectReferenceValue = autoTransform;
        so.FindProperty("siluetaEnteNiebla").objectReferenceValue = silueta;
        SerializedProperty farolasProp = so.FindProperty("farolasParada");
        farolasProp.arraySize = farolas.Length;
        for (int i = 0; i < farolas.Length; i++)
        {
            farolasProp.GetArrayElementAtIndex(i).objectReferenceValue = farolas[i];
        }
        so.ApplyModifiedProperties();
    }
}
