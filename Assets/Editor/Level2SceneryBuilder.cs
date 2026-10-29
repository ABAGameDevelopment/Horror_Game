using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class Level2SceneryBuilder
{
    private const string ScenePath = "Assets/Scenes/Level2/University.unity";
    private const string SceneCopyPath = "Assets/Scenes/Level2/Level2.unity";

    // Prefabs de Vehículos
    private const string CarRedPrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Red Variant.prefab";
    private const string CarBluePrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Blue Variant.prefab";
    private const string CarGrayPrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Gray Variant.prefab";
    private const string CarPurplePrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car Purple Variant.prefab";

    // Modelos 3D de Items
    private const string BatteryModelPath = "Assets/Models/Level2/Battery.obj";
    private const string OilBottleModelPath = "Assets/Models/Level2/OilBottle.obj";
    private const string GasolineCanModelPath = "Assets/Models/Level2/GasolineCan.obj";
    private const string CarKeyModelPath = "Assets/Models/Level2/CarKey.obj";

    // Modelos 3D de Estructuras y Props
    private const string ParkingGateModelPath = "Assets/Models/Level2/ParkingGate.obj";
    private const string GuardBoothModelPath = "Assets/Models/Level2/GuardBooth.obj";
    private const string StreetLampModelPath = "Assets/Models/Level2/StreetLamp.obj";
    private const string OilDrumModelPath = "Assets/Models/Level2/OilDrum.obj";
    private const string TrafficConeModelPath = "Assets/Models/Level2/TrafficCone.obj";
    private const string ParkingCanopyModelPath = "Assets/Models/Level2/ParkingCanopy.obj";
    private const string JerseyBarrierModelPath = "Assets/Models/Level2/JerseyBarrier.obj";
    private const string DumpsterModelPath = "Assets/Models/Level2/Dumpster.obj";
    private const string FuelTankModelPath = "Assets/Models/Level2/FuelTank.obj";
    private const string TireStackModelPath = "Assets/Models/Level2/TireStack.obj";
    private const string WoodCrateModelPath = "Assets/Models/Level2/WoodCrate.obj";
    private const string SecurityCameraModelPath = "Assets/Models/Level2/SecurityCamera.obj";
    private const string BoomBarrierModelPath = "Assets/Models/Level2/BoomBarrier.obj";
    private const string CarHoodModelPath = "Assets/Models/Level2/CarHood.obj";
    private const string CarEngineBayModelPath = "Assets/Models/Level2/CarEngineBay.obj";

    // Prefabs de Entorno
    private const string ConiferBarePath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Bare BOTD URP.prefab";
    private const string ConiferTallPath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Tall BOTD URP.prefab";
    private const string ConiferMedPath = "Assets/Packages/Tree/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Medium BOTD URP.prefab";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const string Building20Path = "Assets/Packages/Building/prefabs/Buildings/Building_20.prefab";
    private const string Building23Path = "Assets/Packages/Building/prefabs/Buildings/Building_23.prefab";
    private const string StorageShelfPath = "Assets/Packages/nappin/HouseInteriorPack/Prefabs/(Prb)Storage2.prefab";
    private const string Shelf1Path = "Assets/Packages/nappin/HouseInteriorPack/Prefabs/(Prb)Shelf1.prefab";

    // Materiales URP
    private const string MatAsfaltoPath = "Assets/Materials/Level2/Mat_AsfaltoEstacionamiento.mat";
    private const string MatLineasPath = "Assets/Materials/Level2/Mat_LineasAmarillas.mat";
    private const string MatBateriaPath = "Assets/Materials/Level2/Mat_Bateria.mat";
    private const string MatAceitePath = "Assets/Materials/Level2/Mat_Aceite.mat";
    private const string MatGasolinaPath = "Assets/Materials/Level2/Mat_Gasolina.mat";
    private const string MatLlavesPath = "Assets/Materials/Level2/Mat_Llaves.mat";
    private const string MatPortonPath = "Assets/Materials/Level2/Mat_PortonMetal.mat";
    private const string MatCasetaPath = "Assets/Materials/Level2/Mat_CasetaVigilancia.mat";
    private const string MatPosteLuzPath = "Assets/Materials/Level2/Mat_PosteLuz.mat";
    private const string MatFocoLuzPath = "Assets/Materials/Level2/Mat_FocoLuzEmisiva.mat";
    private const string MatBarrilAceitePath = "Assets/Materials/Level2/Mat_BarrilAceite.mat";
    private const string MatBarrilRojoPath = "Assets/Materials/Level2/Mat_BarrilRojo.mat";
    private const string MatConoTraficoPath = "Assets/Materials/Level2/Mat_ConoTrafico.mat";
    private const string MatMarquesinaPath = "Assets/Materials/Level2/Mat_MarquesinaMetal.mat";
    private const string MatBarreraConcretoPath = "Assets/Materials/Level2/Mat_BarreraConcreto.mat";
    private const string MatContenedorPath = "Assets/Materials/Level2/Mat_ContenedorVerde.mat";
    private const string MatTanqueCombustiblePath = "Assets/Materials/Level2/Mat_TanqueCombustible.mat";
    private const string MatNeumaticosPath = "Assets/Materials/Level2/Mat_Neumaticos.mat";
    private const string MatCajaMaderaPath = "Assets/Materials/Level2/Mat_CajaMadera.mat";
    private const string MatPlumaPath = "Assets/Materials/Level2/Mat_PlumaVehicular.mat";
    private const string MatCharcoAguaPath = "Assets/Materials/Level2/Mat_CharcoAgua.mat";
    private const string SkyMidnightPath = "Assets/Scenes/Level2/SkyMidnight.mat";

    static Level2SceneryBuilder()
    {
        EditorApplication.delayCall += AutoBuildIfNeeded;
    }

    private static void AutoBuildIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path == ScenePath || activeScene.path == SceneCopyPath)
        {
            // Ocultar o eliminar letreros de objetivos
            CleanObjectiveSigns();

            if (GameObject.Find("Escenografia_Estacionamiento") == null)
            {
                BuildLevel2Scenery();
            }
        }
    }

    [MenuItem("Tools/Nivel 2/Construir Escenografia Estacionamiento (Completa y Rica)")]
    public static void BuildLevel2Scenery()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path != ScenePath && activeScene.path != SceneCopyPath)
        {
            if (System.IO.File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        Debug.Log("<color=cyan>[Nivel 2]</color> Construyendo escenografia detallada del Estacionamiento...");

        // 1. Quitar los letreros de objetivos del Nivel 1 y ocultar el interior
        CleanObjectiveSigns();
        HideOldInterior();

        // 2. Limpiar o recrear el root
        GameObject root = GameObject.Find("Escenografia_Estacionamiento");
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
        }
        root = new GameObject("Escenografia_Estacionamiento");
        Undo.RegisterCreatedObjectUndo(root, "Crear Escenografia Estacionamiento");

        // 3. Atmósfera de Horror y Skybox
        SetupAtmosphere(root);

        // 4. Suelo de asfalto, charcos reflectantes, cajones y pasos de peatones
        BuildRealisticParkingGround(root);

        // 5. Marquesinas de estacionamiento techadas
        BuildParkingCanopies(root);

        // 6. Muros perimetrales y vallas de seguridad
        BuildPerimeterEnclosure(root);

        // 7. Fachada posterior del edificio universitario con muelle de carga
        BuildUniversityExitFacade(root);

        // 8. Postes de luz de vapor de sodio y luces parpadeantes
        BuildLightingSystem(root);

        // 9. Automóvil de escape principal (con capó abierto e interactuable)
        BuildEscapeVehicle(root);

        // 10. Flota de 10+ autos abandonados, chocados y desalineados
        BuildAbandonedVehicles(root);

        // 11. Las 4 Zonas de búsqueda ultra-detalladas
        BuildZone1SecurityBooth(root);      // Llaves
        BuildZone2MaintenanceWorkshop(root); // Aceite
        BuildZone3ElectricalSubstation(root);// Batería
        BuildZone4FuelDepot(root);          // Gasolina

        // 12. Portón de escape industrial institucional con pluma vehicular
        BuildExitGateComplex(root);

        // 13. Atrezo exterior: conos, barriles, barreras jersey, cámaras de seguridad
        BuildEnvironmentalClutter(root);

        // 14. Bosque denso perimetral de coníferas siniestras
        BuildDarkForestPerimeter(root);

        // 15. Posicionar al Jugador saliendo de la universidad
        SetupPlayerStart(root);

        // 16. Nodos de acecho de la criatura
        BuildCreatureWaypoints(root);

        // Guardar cambios
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        if (!System.IO.File.Exists(SceneCopyPath))
        {
            AssetDatabase.CopyAsset(ScenePath, SceneCopyPath);
        }

        Debug.Log("<color=green>[Nivel 2]</color> ¡Escenografia completa del Estacionamiento terminada exitosamente!");
    }

    public static void CleanObjectiveSigns()
    {
        string[] uiNames = {
            "CanvasObjetivos", "GestorObjetivos", "IntroLevel",
            "PanelNuevoObjetivo", "TextoObjetivo", "PanelObjetivosHUD",
            "PanelTutorialLinterna", "TextoTutorialLinterna"
        };

        foreach (string n in uiNames)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
            {
                go.SetActive(false);
            }
        }
    }

    private static void HideOldInterior()
    {
        // Ocultar EdificioV o moverlo para que no interfiera con el estacionamiento
        GameObject interior = GameObject.Find("EdificioV");
        if (interior != null)
        {
            interior.SetActive(false);
        }
    }

    private static void SetupAtmosphere(GameObject root)
    {
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(SkyMidnightPath);
        if (skyMat != null) RenderSettings.skybox = skyMat;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.035f, 0.05f, 0.09f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.025f, 0.04f, 0.07f, 1f);
        RenderSettings.fogDensity = 0.032f;

        // Luz de luna fría
        GameObject moonGo = new GameObject("Luz_Luna_Exterior");
        moonGo.transform.SetParent(root.transform);
        moonGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        Light moonLight = moonGo.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = new Color(0.38f, 0.48f, 0.68f);
        moonLight.intensity = 0.16f;
        moonLight.shadows = LightShadows.Soft;
    }

    private static void BuildRealisticParkingGround(GameObject root)
    {
        GameObject groundGroup = new GameObject("Pavimento_Y_Senalizacion");
        groundGroup.transform.SetParent(root.transform);

        // 1. Asfalto Principal (100m ancho x 80m largo)
        GameObject asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asphalt.name = "Asfalto_Explanada";
        asphalt.transform.SetParent(groundGroup.transform);
        asphalt.transform.position = new Vector3(0f, -0.1f, 0f);
        asphalt.transform.localScale = new Vector3(100f, 0.2f, 80f);
        Material matAsfalto = AssetDatabase.LoadAssetAtPath<Material>(MatAsfaltoPath);
        if (matAsfalto != null) asphalt.GetComponent<Renderer>().sharedMaterial = matAsfalto;

        // 2. Charcos de agua reflectantes
        Material matCharco = AssetDatabase.LoadAssetAtPath<Material>(MatCharcoAguaPath);
        Vector3[] puddlePositions = {
            new Vector3(-5f, 0.015f, -10f),
            new Vector3(8f, 0.015f, 5f),
            new Vector3(-18f, 0.015f, 18f),
            new Vector3(22f, 0.015f, -14f),
            new Vector3(0f, 0.015f, 24f),
            new Vector3(-28f, 0.015f, -5f)
        };
        Vector2[] puddleSizes = {
            new Vector2(6.5f, 4.2f),
            new Vector2(5.0f, 3.8f),
            new Vector2(7.2f, 5.0f),
            new Vector2(4.5f, 3.2f),
            new Vector2(8.0f, 4.0f),
            new Vector2(5.5f, 3.5f)
        };

        for (int p = 0; p < puddlePositions.Length; p++)
        {
            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Quad);
            puddle.name = string.Format("Charco_Agua_{0}", p + 1);
            puddle.transform.SetParent(groundGroup.transform);
            puddle.transform.position = puddlePositions[p];
            puddle.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 180f), 0f);
            puddle.transform.localScale = new Vector3(puddleSizes[p].x, puddleSizes[p].y, 1f);
            if (matCharco != null) puddle.GetComponent<Renderer>().sharedMaterial = matCharco;
            Object.DestroyImmediate(puddle.GetComponent<Collider>());
        }

        // 3. Pintura vial: Cajones de estacionamiento amarillos
        Material matLineas = AssetDatabase.LoadAssetAtPath<Material>(MatLineasPath);
        float[] rowXCoords = { -24f, -14f, 14f, 24f };
        for (int r = 0; r < rowXCoords.Length; r++)
        {
            float rx = rowXCoords[r];
            for (int zIdx = -5; zIdx <= 5; zIdx++)
            {
                float rz = zIdx * 5.6f;
                // Línea divisoria
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = string.Format("Linea_Cajon_{0}_{1}", r, zIdx);
                line.transform.SetParent(groundGroup.transform);
                line.transform.position = new Vector3(rx, 0.02f, rz);
                line.transform.localScale = new Vector3(5.2f, 0.02f, 0.16f);
                if (matLineas != null) line.GetComponent<Renderer>().sharedMaterial = matLineas;

                // Tope de llanta (Wheel stop)
                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = string.Format("Tope_Rueda_{0}_{1}", r, zIdx);
                curb.transform.SetParent(groundGroup.transform);
                float curbX = (rx < 0) ? rx - 2.3f : rx + 2.3f;
                curb.transform.position = new Vector3(curbX, 0.08f, rz + 2.6f);
                curb.transform.localScale = new Vector3(0.25f, 0.16f, 1.9f);
                if (matLineas != null) curb.GetComponent<Renderer>().sharedMaterial = matLineas;
            }
        }

        // 4. Paso de cebra peatonal frente a la salida del edificio
        for (int c = -4; c <= 4; c++)
        {
            GameObject zebra = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zebra.name = "Paso_Cebra_" + c;
            zebra.transform.SetParent(groundGroup.transform);
            zebra.transform.position = new Vector3(c * 1.4f, 0.02f, -27f);
            zebra.transform.localScale = new Vector3(0.85f, 0.02f, 3.2f);
            if (matLineas != null) zebra.GetComponent<Renderer>().sharedMaterial = matLineas;
        }

        // 5. Flechas direccionales en la avenida central (hacia el portón)
        CreateDirectionArrow(groundGroup, new Vector3(0f, 0.02f, -12f), matLineas);
        CreateDirectionArrow(groundGroup, new Vector3(0f, 0.02f, 12f), matLineas);

        // 6. Topes reductores de velocidad (Speed Bumps) con franjas
        CreateSpeedBump(groundGroup, new Vector3(0f, 0.05f, -20f), 14f, matLineas);
        CreateSpeedBump(groundGroup, new Vector3(0f, 0.05f, 22f), 14f, matLineas);
    }

    private static void CreateDirectionArrow(GameObject parent, Vector3 pos, Material mat)
    {
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "Flecha_Tronco";
        shaft.transform.SetParent(parent.transform);
        shaft.transform.position = pos;
        shaft.transform.localScale = new Vector3(0.4f, 0.02f, 2.8f);
        if (mat != null) shaft.GetComponent<Renderer>().sharedMaterial = mat;

        GameObject headL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headL.name = "Flecha_Punta_Izq";
        headL.transform.SetParent(parent.transform);
        headL.transform.position = pos + new Vector3(-0.45f, 0f, 1.1f);
        headL.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
        headL.transform.localScale = new Vector3(0.35f, 0.02f, 1.2f);
        if (mat != null) headL.GetComponent<Renderer>().sharedMaterial = mat;

        GameObject headR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headR.name = "Flecha_Punta_Der";
        headR.transform.SetParent(parent.transform);
        headR.transform.position = pos + new Vector3(0.45f, 0f, 1.1f);
        headR.transform.rotation = Quaternion.Euler(0f, -35f, 0f);
        headR.transform.localScale = new Vector3(0.35f, 0.02f, 1.2f);
        if (mat != null) headR.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void CreateSpeedBump(GameObject parent, Vector3 pos, float width, Material mat)
    {
        GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bump.name = "Tope_Velocidad";
        bump.transform.SetParent(parent.transform);
        bump.transform.position = pos;
        bump.transform.localScale = new Vector3(width, 0.10f, 0.85f);
        if (mat != null) bump.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void BuildParkingCanopies(GameObject root)
    {
        GameObject canopyGroup = new GameObject("Marquesinas_Techadas");
        canopyGroup.transform.SetParent(root.transform);

        GameObject canopyMesh = AssetDatabase.LoadAssetAtPath<GameObject>(ParkingCanopyModelPath);
        Material matMarquesina = AssetDatabase.LoadAssetAtPath<Material>(MatMarquesinaPath);

        // Marquesina Oeste sobre cajones
        if (canopyMesh != null)
        {
            GameObject c1 = (GameObject)PrefabUtility.InstantiatePrefab(canopyMesh);
            c1.name = "Marquesina_Oeste_1";
            c1.transform.SetParent(canopyGroup.transform);
            c1.transform.position = new Vector3(-19f, 0f, -6f);
            c1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            if (matMarquesina != null) c1.GetComponentInChildren<Renderer>().sharedMaterial = matMarquesina;

            GameObject c2 = (GameObject)PrefabUtility.InstantiatePrefab(canopyMesh);
            c2.name = "Marquesina_Oeste_2";
            c2.transform.SetParent(canopyGroup.transform);
            c2.transform.position = new Vector3(-19f, 0f, 8f);
            c2.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            if (matMarquesina != null) c2.GetComponentInChildren<Renderer>().sharedMaterial = matMarquesina;

            // Marquesina Este
            GameObject c3 = (GameObject)PrefabUtility.InstantiatePrefab(canopyMesh);
            c3.name = "Marquesina_Este_1";
            c3.transform.SetParent(canopyGroup.transform);
            c3.transform.position = new Vector3(19f, 0f, -6f);
            c3.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            if (matMarquesina != null) c3.GetComponentInChildren<Renderer>().sharedMaterial = matMarquesina;

            GameObject c4 = (GameObject)PrefabUtility.InstantiatePrefab(canopyMesh);
            c4.name = "Marquesina_Este_2";
            c4.transform.SetParent(canopyGroup.transform);
            c4.transform.position = new Vector3(19f, 0f, 8f);
            c4.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            if (matMarquesina != null) c4.GetComponentInChildren<Renderer>().sharedMaterial = matMarquesina;
        }
    }

    private static void BuildPerimeterEnclosure(GameObject root)
    {
        GameObject enclosure = new GameObject("Cierre_Perimetral");
        enclosure.transform.SetParent(root.transform);
        Material matWall = AssetDatabase.LoadAssetAtPath<Material>(MatBarreraConcretoPath);

        // Muros exteriores de concreto
        CreateWall(enclosure, new Vector3(-48f, 2.2f, 0f), new Vector3(1.2f, 4.4f, 80f), matWall);
        CreateWall(enclosure, new Vector3(48f, 2.2f, 0f), new Vector3(1.2f, 4.4f, 80f), matWall);

        // Muro Norte (dejando apertura central para el portón: X = -7 a +7)
        CreateWall(enclosure, new Vector3(-28f, 2.2f, 38f), new Vector3(40f, 4.4f, 1.2f), matWall);
        CreateWall(enclosure, new Vector3(28f, 2.2f, 38f), new Vector3(40f, 4.4f, 1.2f), matWall);

        // Muro Sur (dejando apertura para la fachada de la universidad)
        CreateWall(enclosure, new Vector3(-32f, 2.8f, -38f), new Vector3(32f, 5.6f, 1.5f), matWall);
        CreateWall(enclosure, new Vector3(32f, 2.8f, -38f), new Vector3(32f, 5.6f, 1.5f), matWall);
    }

    private static void CreateWall(GameObject parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = "Muro_Perimetral_Hormigon";
        w.transform.SetParent(parent.transform);
        w.transform.position = pos;
        w.transform.localScale = scale;
        if (mat != null) w.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void BuildUniversityExitFacade(GameObject root)
    {
        GameObject facadeGroup = new GameObject("Fachada_Salida_Universidad");
        facadeGroup.transform.SetParent(root.transform);

        // Edificios monumentales de fondo
        GameObject bld20Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Building20Path);
        GameObject bld23Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Building23Path);

        if (bld20Prefab != null)
        {
            GameObject bldCenter = (GameObject)PrefabUtility.InstantiatePrefab(bld20Prefab);
            bldCenter.name = "Edificio_Campus_Centro";
            bldCenter.transform.SetParent(facadeGroup.transform);
            bldCenter.transform.position = new Vector3(0f, 0f, -46f);
            bldCenter.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (bld23Prefab != null)
        {
            GameObject bldWest = (GameObject)PrefabUtility.InstantiatePrefab(bld23Prefab);
            bldWest.name = "Edificio_Campus_Oeste";
            bldWest.transform.SetParent(facadeGroup.transform);
            bldWest.transform.position = new Vector3(-25f, 0f, -48f);
            bldWest.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject bldEast = (GameObject)PrefabUtility.InstantiatePrefab(bld23Prefab);
            bldEast.name = "Edificio_Campus_Este";
            bldEast.transform.SetParent(facadeGroup.transform);
            bldEast.transform.position = new Vector3(25f, 0f, -48f);
            bldEast.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Plataforma de salida / Rampa de acceso
        GameObject dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dock.name = "Muelle_Salida_Emergencia";
        dock.transform.SetParent(facadeGroup.transform);
        dock.transform.position = new Vector3(0f, 0.4f, -36f);
        dock.transform.localScale = new Vector3(14f, 0.8f, 5.0f);
        Material matDock = AssetDatabase.LoadAssetAtPath<Material>(MatBarreraConcretoPath);
        if (matDock != null) dock.GetComponent<Renderer>().sharedMaterial = matDock;

        // Escaleras de bajada a la explanada
        for (int s = 0; s < 3; s++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "Escalon_" + (s + 1);
            step.transform.SetParent(facadeGroup.transform);
            step.transform.position = new Vector3(0f, 0.25f - (s * 0.12f), -33.2f + (s * 0.4f));
            step.transform.localScale = new Vector3(6.5f, 0.15f, 0.45f);
            if (matDock != null) step.GetComponent<Renderer>().sharedMaterial = matDock;
        }

        // Puerta doble de emergencia de metal rojo
        Material matPuertaRoja = AssetDatabase.LoadAssetAtPath<Material>(MatGasolinaPath);
        Material matMarco = AssetDatabase.LoadAssetAtPath<Material>(MatPortonPath);

        GameObject doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorFrame.name = "Marco_Puerta_Emergencia";
        doorFrame.transform.SetParent(facadeGroup.transform);
        doorFrame.transform.position = new Vector3(0f, 2.4f, -38.2f);
        doorFrame.transform.localScale = new Vector3(3.4f, 3.2f, 0.5f);
        if (matMarco != null) doorFrame.GetComponent<Renderer>().sharedMaterial = matMarco;

        GameObject doorL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorL.name = "Puerta_Emergencia_Izq";
        doorL.transform.SetParent(facadeGroup.transform);
        doorL.transform.position = new Vector3(-0.8f, 2.2f, -38.0f);
        doorL.transform.localScale = new Vector3(1.4f, 2.7f, 0.1f);
        if (matPuertaRoja != null) doorL.GetComponent<Renderer>().sharedMaterial = matPuertaRoja;

        GameObject doorR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorR.name = "Puerta_Emergencia_Der";
        doorR.transform.SetParent(facadeGroup.transform);
        doorR.transform.position = new Vector3(0.8f, 2.2f, -38.0f);
        doorR.transform.localScale = new Vector3(1.4f, 2.7f, 0.1f);
        if (matPuertaRoja != null) doorR.GetComponent<Renderer>().sharedMaterial = matPuertaRoja;

        // Cartel luminoso EXIT rojo
        GameObject exitSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitSign.name = "Letrero_EXIT";
        exitSign.transform.SetParent(facadeGroup.transform);
        exitSign.transform.position = new Vector3(0f, 3.85f, -37.9f);
        exitSign.transform.localScale = new Vector3(1.2f, 0.4f, 0.15f);
        if (matPuertaRoja != null) exitSign.GetComponent<Renderer>().sharedMaterial = matPuertaRoja;

        GameObject exitLightGo = new GameObject("Luz_EXIT");
        exitLightGo.transform.SetParent(exitSign.transform);
        exitLightGo.transform.localPosition = new Vector3(0f, 0f, 0.25f);
        Light el = exitLightGo.AddComponent<Light>();
        el.type = LightType.Point;
        el.color = new Color(1.0f, 0.08f, 0.08f);
        el.intensity = 1.6f;
        el.range = 6.0f;
        exitLightGo.AddComponent<HorrorLightFlicker>();

        // Cámaras de seguridad en las esquinas de la fachada
        GameObject camMesh = AssetDatabase.LoadAssetAtPath<GameObject>(SecurityCameraModelPath);
        if (camMesh != null)
        {
            GameObject cam1 = (GameObject)PrefabUtility.InstantiatePrefab(camMesh);
            cam1.name = "Camara_Seguridad_Fachada_Izq";
            cam1.transform.SetParent(facadeGroup.transform);
            cam1.transform.position = new Vector3(-6f, 4.2f, -37.8f);
            cam1.transform.rotation = Quaternion.Euler(25f, 30f, 0f);

            GameObject cam2 = (GameObject)PrefabUtility.InstantiatePrefab(camMesh);
            cam2.name = "Camara_Seguridad_Fachada_Der";
            cam2.transform.SetParent(facadeGroup.transform);
            cam2.transform.position = new Vector3(6f, 4.2f, -37.8f);
            cam2.transform.rotation = Quaternion.Euler(25f, -30f, 0f);
        }
    }

    private static void BuildLightingSystem(GameObject root)
    {
        GameObject lightsGroup = new GameObject("Iluminacion_Farolas_Estacionamiento");
        lightsGroup.transform.SetParent(root.transform);

        GameObject lampMesh = AssetDatabase.LoadAssetAtPath<GameObject>(StreetLampModelPath);
        Material matPoste = AssetDatabase.LoadAssetAtPath<Material>(MatPosteLuzPath);

        Vector3[] lampPositions = {
            new Vector3(-18f, 0f, -22f),
            new Vector3(18f, 0f, -22f),
            new Vector3(-18f, 0f, -4f),
            new Vector3(18f, 0f, -4f),
            new Vector3(-18f, 0f, 14f),
            new Vector3(18f, 0f, 14f),
            new Vector3(-5f, 0f, 32f),
            new Vector3(16f, 0f, 32f)
        };

        for (int i = 0; i < lampPositions.Length; i++)
        {
            Vector3 pos = lampPositions[i];
            GameObject lamp = (lampMesh != null) ? (GameObject)PrefabUtility.InstantiatePrefab(lampMesh) : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lamp.name = "Poste_Farola_" + (i + 1);
            lamp.transform.SetParent(lightsGroup.transform);
            lamp.transform.position = pos;
            if (matPoste != null && lampMesh == null) lamp.GetComponent<Renderer>().sharedMaterial = matPoste;

            // Luz cenital focal
            GameObject lightChild = new GameObject("Cono_Luz_Farola");
            lightChild.transform.SetParent(lamp.transform);
            lightChild.transform.position = pos + new Vector3(0.9f, 5.2f, 0f);
            lightChild.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            Light spot = lightChild.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.spotAngle = 82f;
            spot.innerSpotAngle = 40f;
            spot.range = 18f;
            // Farola 4 apagada / fundida
            if (i == 3)
            {
                spot.enabled = false;
            }
            else
            {
                spot.color = new Color(1.0f, 0.84f, 0.58f);
                spot.intensity = 2.8f;
                spot.shadows = LightShadows.Soft;

                // Farola 2 y 6 parpadean tétricamente
                if (i == 2 || i == 6)
                {
                    lightChild.AddComponent<HorrorLightFlicker>();
                }
            }
        }
    }

    private static void BuildEscapeVehicle(GameObject root)
    {
        GameObject carGroup = new GameObject("Vehiculo_De_Escape_Principal");
        carGroup.transform.SetParent(root.transform);

        GameObject carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarRedPrefabPath);
        if (carPrefab != null)
        {
            GameObject car = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
            car.name = "Automovil_De_Escape";
            car.transform.SetParent(carGroup.transform);
            // Centrado enfilado directo al portón de salida
            car.transform.position = new Vector3(0f, 0.15f, -2f);
            car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            // Script de escape que gestiona la reparación, cofre interactivo e impacto
            CarEscapeController escapeCtrl = car.AddComponent<CarEscapeController>();
            escapeCtrl.AsegurarComponentesCofreYMotor();

            // Triggers interactivos
            // 1. Cofre / Motor (Batería y Aceite)
            GameObject hoodTrigger = new GameObject("Trigger_Motor_Cofre");
            hoodTrigger.transform.SetParent(car.transform);
            hoodTrigger.transform.localPosition = new Vector3(0f, 0.7f, 1.8f);
            BoxCollider hoodCol = hoodTrigger.AddComponent<BoxCollider>();
            hoodCol.isTrigger = true;
            hoodCol.size = new Vector3(1.8f, 1.3f, 1.6f);

            // 2. Tanque (Gasolina)
            GameObject tankTrigger = new GameObject("Trigger_Tanque_Gasolina");
            tankTrigger.transform.SetParent(car.transform);
            tankTrigger.transform.localPosition = new Vector3(-1.15f, 0.7f, -1.2f);
            BoxCollider tankCol = tankTrigger.AddComponent<BoxCollider>();
            tankCol.isTrigger = true;
            tankCol.size = new Vector3(1.2f, 1.2f, 1.2f);

            // 3. Puerta Conductor (Llaves y Conducir)
            GameObject driverTrigger = new GameObject("Trigger_Puerta_Conductor");
            driverTrigger.transform.SetParent(car.transform);
            driverTrigger.transform.localPosition = new Vector3(-1.15f, 0.8f, 0.2f);
            BoxCollider driverCol = driverTrigger.AddComponent<BoxCollider>();
            driverCol.isTrigger = true;
            driverCol.size = new Vector3(1.3f, 1.6f, 1.5f);

            // Destello de faros delanteros tenues
            GameObject leftHeadlight = new GameObject("Faro_Izquierdo");
            leftHeadlight.transform.SetParent(car.transform);
            leftHeadlight.transform.localPosition = new Vector3(-0.75f, 0.6f, 2.2f);
            leftHeadlight.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            Light hl = leftHeadlight.AddComponent<Light>();
            hl.type = LightType.Spot;
            hl.spotAngle = 60f;
            hl.range = 14f;
            hl.color = new Color(1.0f, 0.95f, 0.8f);
            hl.intensity = 1.1f;
        }
    }

    private static void BuildAbandonedVehicles(GameObject root)
    {
        GameObject abandonedGroup = new GameObject("Flota_Autos_Abandonados");
        abandonedGroup.transform.SetParent(root.transform);

        string[] variants = {
            CarBluePrefabPath, CarGrayPrefabPath, CarPurplePrefabPath,
            CarBluePrefabPath, CarGrayPrefabPath, CarPurplePrefabPath,
            CarBluePrefabPath, CarGrayPrefabPath, CarPurplePrefabPath
        };

        Vector3[] positions = {
            new Vector3(-24f, 0.15f, -18f),
            new Vector3(-24f, 0.15f, -7f),
            new Vector3(-24f, 0.15f, 10f),
            new Vector3(-14f, 0.15f, -12f),
            new Vector3(-14f, 0.15f, 16f),
            new Vector3(14f, 0.15f, -17f),
            new Vector3(14f, 0.15f, 6f),
            new Vector3(24f, 0.15f, -10f),
            new Vector3(24f, 0.15f, 8f),
            new Vector3(18f, 0.15f, 24f) // Auto chocado cerca de la salida
        };

        float[] angles = { 0f, 6f, 180f, -12f, 180f, 0f, 175f, -180f, 10f, 45f };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(variants[i % variants.Length]);
            if (prefab != null)
            {
                GameObject car = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                car.name = string.Format("Auto_Abandonado_{0}", i + 1);
                car.transform.SetParent(abandonedGroup.transform);
                car.transform.position = positions[i];
                car.transform.rotation = Quaternion.Euler(0f, angles[i], 0f);
            }
        }
    }

    // ------------------------------------------------------------------------
    // ZONA 1: CASETA DE VIGILANCIA (LLAVES)
    // ------------------------------------------------------------------------
    private static void BuildZone1SecurityBooth(GameObject root)
    {
        GameObject z1 = new GameObject("Zona1_CasetaVigilancia_Llaves");
        z1.transform.SetParent(root.transform);
        z1.transform.position = new Vector3(11f, 0f, 31f);

        // Caseta de vigilancia con ventanales y mostrador
        GameObject boothMesh = AssetDatabase.LoadAssetAtPath<GameObject>(GuardBoothModelPath);
        if (boothMesh != null)
        {
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(boothMesh);
            b.name = "Caseta_Control";
            b.transform.SetParent(z1.transform);
            b.transform.localPosition = Vector3.zero;
            Material matCaseta = AssetDatabase.LoadAssetAtPath<Material>(MatCasetaPath);
            if (matCaseta != null) b.GetComponentInChildren<Renderer>().sharedMaterial = matCaseta;
        }

        // Pluma vehicular cerrada
        GameObject boomMesh = AssetDatabase.LoadAssetAtPath<GameObject>(BoomBarrierModelPath);
        if (boomMesh != null)
        {
            GameObject boom = (GameObject)PrefabUtility.InstantiatePrefab(boomMesh);
            boom.name = "Pluma_Vehicular_Control";
            boom.transform.SetParent(z1.transform);
            boom.transform.localPosition = new Vector3(-3.5f, 0f, 1.5f);
            boom.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Material matPluma = AssetDatabase.LoadAssetAtPath<Material>(MatPlumaPath);
            if (matPluma != null) boom.GetComponentInChildren<Renderer>().sharedMaterial = matPluma;
        }

        // Luz interior de la caseta fría
        GameObject bLight = new GameObject("Luz_Interior_Caseta");
        bLight.transform.SetParent(z1.transform);
        bLight.transform.localPosition = new Vector3(0f, 2.3f, 0f);
        Light l = bLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.7f, 0.85f, 1.0f);
        l.intensity = 1.0f;
        l.range = 4.5f;

        // ITEM: LLAVES DEL AUTOMÓVIL sobre el escritorio
        GameObject keyMesh = AssetDatabase.LoadAssetAtPath<GameObject>(CarKeyModelPath);
        GameObject keyGo = (keyMesh != null) ? (GameObject)PrefabUtility.InstantiatePrefab(keyMesh) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyGo.name = "Item_LlavesDelCarro";
        keyGo.transform.SetParent(z1.transform);
        keyGo.transform.localPosition = new Vector3(0f, 0.96f, -0.9f);
        keyGo.transform.localScale = new Vector3(2.6f, 2.6f, 2.6f);
        Material matKey = AssetDatabase.LoadAssetAtPath<Material>(MatLlavesPath);
        if (matKey != null) keyGo.GetComponentInChildren<Renderer>().sharedMaterial = matKey;

        BoxCollider kCol = keyGo.AddComponent<BoxCollider>();
        kCol.isTrigger = true;
        kCol.size = new Vector3(0.5f, 0.5f, 0.5f);
        var pKey = keyGo.AddComponent<Level2ItemPickup>();
        var soKey = new SerializedObject(pKey);
        soKey.FindProperty("tipoItem").enumValueIndex = (int)TipoItemLevel2.Llaves;
        soKey.FindProperty("nombreItem").stringValue = "Llaves del Automovil";
        soKey.ApplyModifiedProperties();

        // Pequeño brillo tenue destacando las llaves
        GameObject glow = new GameObject("Brillo_Llaves");
        glow.transform.SetParent(keyGo.transform);
        glow.transform.localPosition = Vector3.zero;
        Light gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = new Color(1f, 0.9f, 0.6f);
        gl.range = 1.5f;
        gl.intensity = 0.8f;
    }

    // ------------------------------------------------------------------------
    // ZONA 2: BODEGA Y TALLER DE MANTENIMIENTO (ACEITE)
    // ------------------------------------------------------------------------
    private static void BuildZone2MaintenanceWorkshop(GameObject root)
    {
        GameObject z2 = new GameObject("Zona2_TallerMantenimiento_Aceite");
        z2.transform.SetParent(root.transform);
        z2.transform.position = new Vector3(-38f, 0f, -24f);

        // Cobertizo de herramientas con estanterías
        GameObject shelfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StorageShelfPath);
        if (shelfPrefab != null)
        {
            GameObject s1 = (GameObject)PrefabUtility.InstantiatePrefab(shelfPrefab);
            s1.name = "Estanteria_Herramientas_1";
            s1.transform.SetParent(z2.transform);
            s1.transform.localPosition = new Vector3(0f, 0f, 0f);

            GameObject s2 = (GameObject)PrefabUtility.InstantiatePrefab(shelfPrefab);
            s2.name = "Estanteria_Herramientas_2";
            s2.transform.SetParent(z2.transform);
            s2.transform.localPosition = new Vector3(-2.8f, 0f, 0f);
        }

        // Pilas de neumáticos
        GameObject tireMesh = AssetDatabase.LoadAssetAtPath<GameObject>(TireStackModelPath);
        Material matTire = AssetDatabase.LoadAssetAtPath<Material>(MatNeumaticosPath);
        if (tireMesh != null)
        {
            GameObject t1 = (GameObject)PrefabUtility.InstantiatePrefab(tireMesh);
            t1.name = "Pila_Llantas_1";
            t1.transform.SetParent(z2.transform);
            t1.transform.localPosition = new Vector3(2.4f, 0f, -0.6f);
            if (matTire != null) t1.GetComponentInChildren<Renderer>().sharedMaterial = matTire;

            GameObject t2 = (GameObject)PrefabUtility.InstantiatePrefab(tireMesh);
            t2.name = "Pila_Llantas_2";
            t2.transform.SetParent(z2.transform);
            t2.transform.localPosition = new Vector3(2.4f, 0f, 0.8f);
            if (matTire != null) t2.GetComponentInChildren<Renderer>().sharedMaterial = matTire;
        }

        // Cajas de madera apiladas
        GameObject crateMesh = AssetDatabase.LoadAssetAtPath<GameObject>(WoodCrateModelPath);
        Material matCrate = AssetDatabase.LoadAssetAtPath<Material>(MatCajaMaderaPath);
        if (crateMesh != null)
        {
            GameObject cr1 = (GameObject)PrefabUtility.InstantiatePrefab(crateMesh);
            cr1.transform.SetParent(z2.transform);
            cr1.transform.localPosition = new Vector3(-1.4f, 0f, 1.8f);
            if (matCrate != null) cr1.GetComponentInChildren<Renderer>().sharedMaterial = matCrate;
        }

        // ITEM: BIDÓN DE ACEITE DE MOTOR sobre la estantería
        GameObject oilMesh = AssetDatabase.LoadAssetAtPath<GameObject>(OilBottleModelPath);
        GameObject oilGo = (oilMesh != null) ? (GameObject)PrefabUtility.InstantiatePrefab(oilMesh) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        oilGo.name = "Item_AceiteMotor";
        oilGo.transform.SetParent(z2.transform);
        oilGo.transform.localPosition = new Vector3(0.2f, 1.05f, 0.1f);
        oilGo.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        Material matOil = AssetDatabase.LoadAssetAtPath<Material>(MatAceitePath);
        if (matOil != null) oilGo.GetComponentInChildren<Renderer>().sharedMaterial = matOil;

        BoxCollider oCol = oilGo.AddComponent<BoxCollider>();
        oCol.isTrigger = true;
        oCol.size = new Vector3(0.5f, 0.6f, 0.5f);
        var pOil = oilGo.AddComponent<Level2ItemPickup>();
        var soOil = new SerializedObject(pOil);
        soOil.FindProperty("tipoItem").enumValueIndex = (int)TipoItemLevel2.Aceite;
        soOil.FindProperty("nombreItem").stringValue = "Bidon de Aceite";
        soOil.ApplyModifiedProperties();

        // Luz de trabajo tenue
        GameObject workLight = new GameObject("Luz_Taller_Mantenimiento");
        workLight.transform.SetParent(z2.transform);
        workLight.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        Light wl = workLight.AddComponent<Light>();
        wl.type = LightType.Point;
        wl.color = new Color(1.0f, 0.9f, 0.7f);
        wl.range = 4.5f;
        wl.intensity = 1.0f;
    }

    // ------------------------------------------------------------------------
    // ZONA 3: SUBESTACIÓN ELÉCTRICA Y GENERADOR (BATERÍA)
    // ------------------------------------------------------------------------
    private static void BuildZone3ElectricalSubstation(GameObject root)
    {
        GameObject z3 = new GameObject("Zona3_SubestacionElectrica_Bateria");
        z3.transform.SetParent(root.transform);
        z3.transform.position = new Vector3(-38f, 0f, 24f);

        // Transformador industrial grande
        GameObject transf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        transf.name = "Transformador_Alto_Voltaje";
        transf.transform.SetParent(z3.transform);
        transf.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        transf.transform.localScale = new Vector3(3.2f, 3.0f, 2.4f);
        Material matTransf = AssetDatabase.LoadAssetAtPath<Material>(MatPosteLuzPath);
        if (matTransf != null) transf.GetComponent<Renderer>().sharedMaterial = matTransf;

        // Vallas de seguridad perimetrales de la subestación
        Material matFence = AssetDatabase.LoadAssetAtPath<Material>(MatPortonPath);
        CreateWall(z3, new Vector3(2.2f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 4.2f), matFence);
        CreateWall(z3, new Vector3(-2.2f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 4.2f), matFence);

        // Palet de madera de carga
        GameObject pallet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pallet.name = "Palet_Carga_Bateria";
        pallet.transform.SetParent(z3.transform);
        pallet.transform.localPosition = new Vector3(0f, 0.15f, 2.2f);
        pallet.transform.localScale = new Vector3(1.6f, 0.3f, 1.4f);
        Material matCrate = AssetDatabase.LoadAssetAtPath<Material>(MatCajaMaderaPath);
        if (matCrate != null) pallet.GetComponent<Renderer>().sharedMaterial = matCrate;

        // ITEM: BATERÍA AUTOMOTRIZ sobre el palet
        GameObject batMesh = AssetDatabase.LoadAssetAtPath<GameObject>(BatteryModelPath);
        GameObject batGo = (batMesh != null) ? (GameObject)PrefabUtility.InstantiatePrefab(batMesh) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        batGo.name = "Item_BateriaAutomotriz";
        batGo.transform.SetParent(z3.transform);
        batGo.transform.localPosition = new Vector3(0f, 0.32f, 2.2f);
        batGo.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
        Material matBat = AssetDatabase.LoadAssetAtPath<Material>(MatBateriaPath);
        if (matBat != null) batGo.GetComponentInChildren<Renderer>().sharedMaterial = matBat;

        BoxCollider bCol = batGo.AddComponent<BoxCollider>();
        bCol.isTrigger = true;
        bCol.size = new Vector3(0.6f, 0.5f, 0.5f);
        var pBat = batGo.AddComponent<Level2ItemPickup>();
        var soBat = new SerializedObject(pBat);
        soBat.FindProperty("tipoItem").enumValueIndex = (int)TipoItemLevel2.Bateria;
        soBat.FindProperty("nombreItem").stringValue = "Bateria Automotriz";
        soBat.ApplyModifiedProperties();

        // Chispas / Luz intermitente de alta tensión
        GameObject sparkLight = new GameObject("Luz_Chispa_Subestacion");
        sparkLight.transform.SetParent(z3.transform);
        sparkLight.transform.localPosition = new Vector3(0f, 2.6f, 1.2f);
        Light sl = sparkLight.AddComponent<Light>();
        sl.type = LightType.Point;
        sl.color = new Color(0.4f, 0.7f, 1.0f);
        sl.range = 5f;
        sl.intensity = 1.2f;
        sparkLight.AddComponent<HorrorLightFlicker>();
    }

    // ------------------------------------------------------------------------
    // ZONA 4: DEPÓSITO DE COMBUSTIBLE Y RESIDUOS (GASOLINA)
    // ------------------------------------------------------------------------
    private static void BuildZone4FuelDepot(GameObject root)
    {
        GameObject z4 = new GameObject("Zona4_DepositoCombustible_Gasolina");
        z4.transform.SetParent(root.transform);
        z4.transform.position = new Vector3(38f, 0f, -18f);

        // Tanque cilíndrico grande de combustible
        GameObject tankMesh = AssetDatabase.LoadAssetAtPath<GameObject>(FuelTankModelPath);
        Material matTank = AssetDatabase.LoadAssetAtPath<Material>(MatTanqueCombustiblePath);
        if (tankMesh != null)
        {
            GameObject ft = (GameObject)PrefabUtility.InstantiatePrefab(tankMesh);
            ft.name = "Tanque_Combustible_Industrial";
            ft.transform.SetParent(z4.transform);
            ft.transform.localPosition = new Vector3(0f, 0f, 0f);
            if (matTank != null) ft.GetComponentInChildren<Renderer>().sharedMaterial = matTank;
        }

        // Contenedor grande verde de basura (Dumpster)
        GameObject dumpMesh = AssetDatabase.LoadAssetAtPath<GameObject>(DumpsterModelPath);
        Material matDump = AssetDatabase.LoadAssetAtPath<Material>(MatContenedorPath);
        if (dumpMesh != null)
        {
            GameObject d = (GameObject)PrefabUtility.InstantiatePrefab(dumpMesh);
            d.name = "Contenedor_Basura_Industrial";
            d.transform.SetParent(z4.transform);
            d.transform.localPosition = new Vector3(0f, 0f, 3.5f);
            if (matDump != null) d.GetComponentInChildren<Renderer>().sharedMaterial = matDump;
        }

        // Barriles de combustible apilados (rojos y negros)
        GameObject drumMesh = AssetDatabase.LoadAssetAtPath<GameObject>(OilDrumModelPath);
        Material matDrumRed = AssetDatabase.LoadAssetAtPath<Material>(MatBarrilRojoPath);
        Material matDrumOil = AssetDatabase.LoadAssetAtPath<Material>(MatBarrilAceitePath);

        if (drumMesh != null)
        {
            GameObject dr1 = (GameObject)PrefabUtility.InstantiatePrefab(drumMesh);
            dr1.transform.SetParent(z4.transform);
            dr1.transform.localPosition = new Vector3(-2.2f, 0f, 1.2f);
            if (matDrumRed != null) dr1.GetComponentInChildren<Renderer>().sharedMaterial = matDrumRed;

            GameObject dr2 = (GameObject)PrefabUtility.InstantiatePrefab(drumMesh);
            dr2.transform.SetParent(z4.transform);
            dr2.transform.localPosition = new Vector3(-2.8f, 0f, 1.2f);
            if (matDrumOil != null) dr2.GetComponentInChildren<Renderer>().sharedMaterial = matDrumOil;

            GameObject dr3 = (GameObject)PrefabUtility.InstantiatePrefab(drumMesh);
            dr3.transform.SetParent(z4.transform);
            dr3.transform.localPosition = new Vector3(-2.5f, 0.9f, 1.2f);
            if (matDrumRed != null) dr3.GetComponentInChildren<Renderer>().sharedMaterial = matDrumRed;
        }

        // ITEM: BIDÓN DE GASOLINA ROJO
        GameObject gasMesh = AssetDatabase.LoadAssetAtPath<GameObject>(GasolineCanModelPath);
        GameObject gasGo = (gasMesh != null) ? (GameObject)PrefabUtility.InstantiatePrefab(gasMesh) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        gasGo.name = "Item_Gasolina";
        gasGo.transform.SetParent(z4.transform);
        gasGo.transform.localPosition = new Vector3(-1.2f, 0.1f, 2.2f);
        gasGo.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        Material matGas = AssetDatabase.LoadAssetAtPath<Material>(MatGasolinaPath);
        if (matGas != null) gasGo.GetComponentInChildren<Renderer>().sharedMaterial = matGas;

        BoxCollider gCol = gasGo.AddComponent<BoxCollider>();
        gCol.isTrigger = true;
        gCol.size = new Vector3(0.6f, 0.7f, 0.5f);
        var pGas = gasGo.AddComponent<Level2ItemPickup>();
        var soGas = new SerializedObject(pGas);
        soGas.FindProperty("tipoItem").enumValueIndex = (int)TipoItemLevel2.Gasolina;
        soGas.FindProperty("nombreItem").stringValue = "Bidon de Gasolina";
        soGas.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------------
    // PORTÓN DE SALIDA INSTITUCIONAL (EL CLÍMAX)
    // ------------------------------------------------------------------------
    private static void BuildExitGateComplex(GameObject root)
    {
        GameObject gateComplex = new GameObject("Complejo_Porton_Salida");
        gateComplex.transform.SetParent(root.transform);
        gateComplex.transform.position = new Vector3(0f, 0f, 37.8f);

        // Columnas macizas de hormigón a los costados
        Material matConcrete = AssetDatabase.LoadAssetAtPath<Material>(MatBarreraConcretoPath);
        GameObject colLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colLeft.name = "Pilar_Porton_Izquierdo";
        colLeft.transform.SetParent(gateComplex.transform);
        colLeft.transform.localPosition = new Vector3(-3.8f, 2.0f, 0f);
        colLeft.transform.localScale = new Vector3(1.2f, 4.0f, 1.2f);
        if (matConcrete != null) colLeft.GetComponent<Renderer>().sharedMaterial = matConcrete;

        GameObject colRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colRight.name = "Pilar_Porton_Derecho";
        colRight.transform.SetParent(gateComplex.transform);
        colRight.transform.localPosition = new Vector3(3.8f, 2.0f, 0f);
        colRight.transform.localScale = new Vector3(1.2f, 4.0f, 1.2f);
        if (matConcrete != null) colRight.GetComponent<Renderer>().sharedMaterial = matConcrete;

        // Portón de barras de acero con cadenas y candado
        GameObject gateMesh = AssetDatabase.LoadAssetAtPath<GameObject>(ParkingGateModelPath);
        if (gateMesh != null)
        {
            GameObject gate = (GameObject)PrefabUtility.InstantiatePrefab(gateMesh);
            gate.name = "Porton_Salida_Estacionamiento";
            gate.transform.SetParent(gateComplex.transform);
            gate.transform.localPosition = Vector3.zero;
            Material matPorton = AssetDatabase.LoadAssetAtPath<Material>(MatPortonPath);
            if (matPorton != null) gate.GetComponentInChildren<Renderer>().sharedMaterial = matPorton;

            BoxCollider gateCol = gate.AddComponent<BoxCollider>();
            gateCol.size = new Vector3(6.8f, 3.4f, 0.7f);
            gateCol.center = new Vector3(0f, 1.5f, 0f);
        }

        // Cartel institucional grande: SALIDA DE VEHICULOS
        GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signBoard.name = "Letrero_Salida_Vehiculos";
        signBoard.transform.SetParent(gateComplex.transform);
        signBoard.transform.localPosition = new Vector3(0f, 4.2f, 0f);
        signBoard.transform.localScale = new Vector3(4.8f, 0.8f, 0.15f);
        Material matSign = AssetDatabase.LoadAssetAtPath<Material>(MatGasolinaPath);
        if (matSign != null) signBoard.GetComponent<Renderer>().sharedMaterial = matSign;

        // Barreras Jersey y conos canalizando hacia el portón
        GameObject barrierMesh = AssetDatabase.LoadAssetAtPath<GameObject>(JerseyBarrierModelPath);
        if (barrierMesh != null)
        {
            GameObject jbL = (GameObject)PrefabUtility.InstantiatePrefab(barrierMesh);
            jbL.name = "Barrera_Jersey_Izq";
            jbL.transform.SetParent(gateComplex.transform);
            jbL.transform.localPosition = new Vector3(-4.8f, 0f, -4.0f);
            jbL.transform.localRotation = Quaternion.Euler(0f, 15f, 0f);

            GameObject jbR = (GameObject)PrefabUtility.InstantiatePrefab(barrierMesh);
            jbR.name = "Barrera_Jersey_Der";
            jbR.transform.SetParent(gateComplex.transform);
            jbR.transform.localPosition = new Vector3(4.8f, 0f, -4.0f);
            jbR.transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
        }
    }

    private static void BuildEnvironmentalClutter(GameObject root)
    {
        GameObject clutter = new GameObject("Atrezo_Detalles_Ambiente");
        clutter.transform.SetParent(root.transform);

        GameObject coneMesh = AssetDatabase.LoadAssetAtPath<GameObject>(TrafficConeModelPath);
        Material matCone = AssetDatabase.LoadAssetAtPath<Material>(MatConoTraficoPath);

        Vector3[] conePositions = {
            new Vector3(-2f, 0f, -18f),
            new Vector3(2f, 0f, -18f),
            new Vector3(-3f, 0f, 28f),
            new Vector3(3f, 0f, 28f),
            new Vector3(-12f, 0f, 0f),
            new Vector3(12f, 0f, 0f)
        };

        if (coneMesh != null)
        {
            for (int i = 0; i < conePositions.Length; i++)
            {
                GameObject cone = (GameObject)PrefabUtility.InstantiatePrefab(coneMesh);
                cone.name = "Cono_Trafico_" + (i + 1);
                cone.transform.SetParent(clutter.transform);
                cone.transform.position = conePositions[i];
                if (matCone != null) cone.GetComponentInChildren<Renderer>().sharedMaterial = matCone;
            }
        }
    }

    private static void BuildDarkForestPerimeter(GameObject root)
    {
        GameObject forest = new GameObject("Bosque_Coniferas_Perimetro");
        forest.transform.SetParent(root.transform);

        GameObject treeBare = AssetDatabase.LoadAssetAtPath<GameObject>(ConiferBarePath);
        GameObject treeTall = AssetDatabase.LoadAssetAtPath<GameObject>(ConiferTallPath);
        GameObject treeMed = AssetDatabase.LoadAssetAtPath<GameObject>(ConiferMedPath);

        List<Vector3> trees = new List<Vector3>();

        // Oeste (X = -50 a -58)
        for (float z = -40f; z <= 40f; z += 3.8f)
        {
            trees.Add(new Vector3(-51f + Random.Range(-1.5f, 1.5f), 0f, z + Random.Range(-1f, 1f)));
            trees.Add(new Vector3(-57f + Random.Range(-2f, 2f), 0f, z + Random.Range(-1.5f, 1.5f)));
        }

        // Este (X = +50 a +58)
        for (float z = -40f; z <= 40f; z += 3.8f)
        {
            trees.Add(new Vector3(51f + Random.Range(-1.5f, 1.5f), 0f, z + Random.Range(-1f, 1f)));
            trees.Add(new Vector3(57f + Random.Range(-2f, 2f), 0f, z + Random.Range(-1.5f, 1.5f)));
        }

        // Norte (Z = +40 a +50)
        for (float x = -48f; x <= 48f; x += 4.2f)
        {
            if (Mathf.Abs(x) < 6.5f) continue; // Dejar abierto el camino del portón
            trees.Add(new Vector3(x + Random.Range(-1.5f, 1.5f), 0f, 41f + Random.Range(-1f, 1.5f)));
            trees.Add(new Vector3(x + Random.Range(-2f, 2f), 0f, 47f + Random.Range(-1.5f, 2f)));
        }

        for (int i = 0; i < trees.Count; i++)
        {
            GameObject prefab = (i % 3 == 0) ? treeBare : ((i % 3 == 1) ? treeTall : treeMed);
            if (prefab != null)
            {
                GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                t.name = string.Format("Conifera_{0}", i + 1);
                t.transform.SetParent(forest.transform);
                t.transform.position = trees[i];
                t.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float s = Random.Range(0.85f, 1.35f);
                t.transform.localScale = new Vector3(s, s, s);
            }
        }
    }

    private static void SetupPlayerStart(GameObject root)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                player.name = "Player";
            }
        }

        if (player != null)
        {
            // Ubicado saliendo por la puerta doble de emergencia de la universidad, bajando las escaleras
            player.transform.position = new Vector3(0f, 1.25f, -31f);
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private static void BuildCreatureWaypoints(GameObject root)
    {
        GameObject creatureGroup = new GameObject("Puntos_De_Acecho_Criatura");
        creatureGroup.transform.SetParent(root.transform);

        // Spawn inicial de la criatura en el extremo más oscuro y distante
        GameObject spawn = new GameObject("Spawn_Criatura_Exterior");
        spawn.transform.SetParent(creatureGroup.transform);
        spawn.transform.position = new Vector3(-20f, 0.5f, 34f);

        // Nodos de patrullaje para cuando se active la IA
        Vector3[] waypoints = {
            new Vector3(-20f, 0.5f, 34f),
            new Vector3(-32f, 0.5f, 12f),
            new Vector3(-24f, 0.5f, -8f),
            new Vector3(-5f, 0.5f, 10f),
            new Vector3(5f, 0.5f, -10f),
            new Vector3(24f, 0.5f, -4f),
            new Vector3(32f, 0.5f, 16f),
            new Vector3(15f, 0.5f, 28f)
        };

        for (int w = 0; w < waypoints.Length; w++)
        {
            GameObject wp = new GameObject("Waypoint_Criatura_" + (w + 1));
            wp.transform.SetParent(creatureGroup.transform);
            wp.transform.position = waypoints[w];
        }
    }
}
