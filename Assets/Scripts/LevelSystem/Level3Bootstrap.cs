using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level3Bootstrap
{
    private const string EscenaNivel3 = "Level3";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (!EsNivel3(escena))
            return;

        CrearSistemas(escena);
    }

    private static bool EsNivel3(Scene escena)
    {
        if (escena.name.Contains("Level2") || (!string.IsNullOrEmpty(escena.path) && escena.path.Contains("Level2")))
            return false;

        if (escena.name == EscenaNivel3 || (!string.IsNullOrEmpty(escena.path) && escena.path.Contains("Level3")))
            return true;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.GetComponentInChildren<Level3Controller>(true) != null)
                return true;
        }

        return false;
    }

    private static void CrearSistemas(Scene escena)
    {
        // Limpiar cualquier UI rezagada del Nivel 2 o Nivel 1 si existiera
        LimpiarUIRezagada();

        if (UnityEngine.Object.FindAnyObjectByType<LevelStateManager>() != null)
            return;

        GameObject sistemas = new GameObject("Sistemas_Nivel3");
        LevelStateManager stateManager = sistemas.AddComponent<LevelStateManager>();
        stateManager.Configurar("Nivel3", "University", 1);

        // Configuracion para el Nivel 3
        sistemas.AddComponent<Level3HUDController>();
        sistemas.AddComponent<PauseMenuController>();
        sistemas.AddComponent<LevelCompleteController>();
        sistemas.AddComponent<GameOverController>();
        sistemas.AddComponent<TituloIntroNivel3>();

        // Si no hay Level3Controller en la escena, asegurarlo
        if (UnityEngine.Object.FindAnyObjectByType<Level3Controller>() == null)
        {
            GameObject ctrlObj = new GameObject("Controlador_Nivel3");
            ctrlObj.AddComponent<Level3Controller>();
        }

        // Asegurar NotasUI en un Canvas dedicado de maximo sorting order
        NotasUI nUI = UnityEngine.Object.FindAnyObjectByType<NotasUI>();
        if (nUI == null)
        {
            GameObject nGo = new GameObject("NotasUI_Global");
            nUI = nGo.AddComponent<NotasUI>();
        }
        nUI.AsegurarUI();

        // Vincular notasUI con InteraccionJugador
        InteraccionJugador ij = UnityEngine.Object.FindAnyObjectByType<InteraccionJugador>();
        if (ij != null)
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(InteraccionJugador).GetField("notasUI", flags)?.SetValue(ij, nUI);
        }

        AsegurarDesbloqueoJugador();
        AsegurarSaludJugador();
        AsegurarMonstruo();
        AsegurarSueloYLimites();

        Debug.Log("[Nivel 3] Sistemas de estado, HUD y menus iniciados correctamente en la escena: " + escena.name);
    }

    private static void LimpiarUIRezagada()
    {
        string[] nombresRezagados = {
            "Sistemas_Nivel2",
            "Canvas_Nivel2",
            "HUD_Nivel2",
            "Canvas_TituloIntroNivel2",
            "GestorObjetivos"
        };

        foreach (string n in nombresRezagados)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        }
    }

    private static void AsegurarDesbloqueoJugador()
    {
        // 0. Asegurar EventSystem si falta
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador == null)
        {
            PlayerController controlador = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (controlador != null)
                jugador = controlador.gameObject;
        }

        // Si no hay jugador ni camara en la escena, instanciar el prefab de Player
        if (jugador == null)
        {
            GameObject playerPrefab = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "Player" && go.GetComponent<PlayerController>() != null);
#if UNITY_EDITOR
            if (playerPrefab == null)
                playerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
#endif
            if (playerPrefab != null)
            {
                jugador = UnityEngine.Object.Instantiate(playerPrefab, new Vector3(1.1f, 1.25f, -1.5f), Quaternion.Euler(0f, 8f, 0f));
                jugador.name = "Player";
                Debug.Log("<color=green>[Nivel 3]</color> Player y Camara instanciados correctamente en la carretera.");
            }
            else
            {
                Debug.LogError("[Nivel 3] No se encontro el prefab Player en Assets/Prefabs/Player/Player.prefab");
            }
        }

        if (jugador != null)
        {
            jugador.tag = "Player";

            PlayerController pc = jugador.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = true;

            CharacterController cc = jugador.GetComponent<CharacterController>();
            if (cc != null)
            {
                if (jugador.transform.position.y < 0.9f)
                {
                    cc.enabled = false;
                    jugador.transform.position = new Vector3(jugador.transform.position.x, 1.25f, jugador.transform.position.z);
                    cc.enabled = true;
                }
                else
                {
                    cc.enabled = true;
                }
            }

            IntroNivel[] intros = jugador.GetComponentsInChildren<IntroNivel>(true);
            foreach (var intro in intros)
            {
                intro.enabled = false;
                UnityEngine.Object.Destroy(intro);
            }
        }
    }

    private static void AsegurarSaludJugador()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador == null)
        {
            PlayerController controlador = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (controlador != null)
                jugador = controlador.gameObject;
        }

        if (jugador != null && jugador.GetComponent<PlayerHealth>() == null)
            jugador.AddComponent<PlayerHealth>();
    }

    private static void AsegurarMonstruo()
    {
        // 1. Quitar obstaculos que bloquean la pasada en la carretera
        string[] bloqueos = { "Zona_6_Bloqueo_PuenteColapsado", "Senal_Puente_Colapsado", "Roca_Derrumbe", "Tronco_Caido" };
        var todos = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in todos)
        {
            if (obj != null && bloqueos.Any(b => obj.name.Contains(b)))
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        // 2. Generar NavMesh en la carretera para que el monstruo camine por toda la carretera
        GameObject road = GameObject.Find("Asfalto_Carretera");
        if (road == null) road = GameObject.Find("Carretera_Y_Entorno");
        if (road == null) road = GameObject.Find("Road");

        if (road != null)
        {
            var surface = road.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
            if (surface == null)
                surface = road.AddComponent<Unity.AI.Navigation.NavMeshSurface>();

            surface.collectObjects = Unity.AI.Navigation.CollectObjects.All;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            Debug.Log("[Nivel 3] NavMesh generado correctamente en la carretera.");
        }

        // 3. Vincular jugador y muerte
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador == null)
        {
            PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) jugador = pc.gameObject;
        }

        MuerteJugador muerte = UnityEngine.Object.FindAnyObjectByType<MuerteJugador>();
        if (muerte == null && jugador != null)
        {
            muerte = CrearSistemaMuerte(jugador);
        }

        // 4. Configurar o instanciar el monstruo oficial
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        MonstruoIA ia = UnityEngine.Object.FindAnyObjectByType<MonstruoIA>();

        if (ia == null)
        {
            GameObject enemyObj = new GameObject("Enemy");
            enemyObj.transform.position = new Vector3(-2.8f, 0.2f, 22f); // Cerca del inicio para que el jugador lo vea pasear de inmediato
            enemyObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            enemyObj.transform.localScale = Vector3.one * 1.5f;

            MeshFilter mf = enemyObj.AddComponent<MeshFilter>();
            MeshRenderer mr = enemyObj.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Prefabs/Enemy/Enemy.glb");
            foreach (var a in assets)
            {
                if (a is Mesh m && mf.sharedMesh == null) mf.sharedMesh = m;
                if (a is Material mat && mr.sharedMaterial == null) mr.sharedMaterial = mat;
            }
#endif
            if (mf.sharedMesh == null)
            {
                var meshes = Resources.FindObjectsOfTypeAll<Mesh>();
                mf.sharedMesh = meshes.FirstOrDefault(m => m.name.ToLower().Contains("enemy") || m.name.ToLower().Contains("monster"));
            }
            if (mr.sharedMaterial == null)
            {
                var mats = Resources.FindObjectsOfTypeAll<Material>();
                mr.sharedMaterial = mats.FirstOrDefault(m => m.name.ToLower().Contains("skin") || m.name.ToLower().Contains("enemy"));
            }

            CapsuleCollider col = enemyObj.AddComponent<CapsuleCollider>();
            col.center = Vector3.zero;
            col.radius = 0.35f;
            col.height = 1.99f;

            UnityEngine.AI.NavMeshAgent ag = enemyObj.AddComponent<UnityEngine.AI.NavMeshAgent>();
            ag.radius = 0.35f;
            ag.height = 1.9923558f;
            ag.baseOffset = 0.9961779f;
            ag.speed = 2.2f;
            ag.acceleration = 8f;
            ag.angularSpeed = 360f;
            ag.stoppingDistance = 1.2f;

            GameObject pvGo = new GameObject("PuntoVision");
            pvGo.transform.SetParent(enemyObj.transform);
            pvGo.transform.localPosition = new Vector3(0f, 0.81f, 0f);

            AudioSource audioMonstruo = enemyObj.AddComponent<AudioSource>();
            audioMonstruo.spatialBlend = 1.0f;
            audioMonstruo.minDistance = 2f;
            audioMonstruo.maxDistance = 28f;
            audioMonstruo.loop = true;
            audioMonstruo.volume = 0.6f;

            AudioClip clipLlorando = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(c => c.name.ToLower().Contains("llorando") || c.name.ToLower().Contains("screamer"));
            if (clipLlorando != null)
            {
                audioMonstruo.clip = clipLlorando;
                audioMonstruo.Play();
            }

            ia = enemyObj.AddComponent<MonstruoIA>();
            typeof(MonstruoIA).GetField("agente", flags)?.SetValue(ia, ag);
            typeof(MonstruoIA).GetField("puntoVision", flags)?.SetValue(ia, pvGo.transform);
            typeof(MonstruoIA).GetField("velocidadPatrulla", flags)?.SetValue(ia, 2.0f);
            typeof(MonstruoIA).GetField("velocidadPersecucion", flags)?.SetValue(ia, 3.8f);
            typeof(MonstruoIA).GetField("distanciaVision", flags)?.SetValue(ia, 13f);
            typeof(MonstruoIA).GetField("anguloVision", flags)?.SetValue(ia, 100f);
            typeof(MonstruoIA).GetField("distanciaPerdida", flags)?.SetValue(ia, 22f);
            typeof(MonstruoIA).GetField("tiempoBusqueda", flags)?.SetValue(ia, 4.5f);
            typeof(MonstruoIA).GetField("distanciaCaptura", flags)?.SetValue(ia, 1.5f);
            Debug.Log("<color=green>[Nivel 3]</color> Monstruo Enemy instanciado automaticamente patrullando carretera.");
        }

        if (ia != null)
        {
            if (jugador != null)
            {
                typeof(MonstruoIA).GetField("jugador", flags)?.SetValue(ia, jugador.transform);
            }
            if (muerte != null)
            {
                typeof(MonstruoIA).GetField("muerteJugador", flags)?.SetValue(ia, muerte);
            }

            // Puntos de patrullaje a lo largo de toda la carretera (Z = 18 a 145)
            float[] zPositions = new float[] { 20f, 38f, 58f, 78f, 98f, 118f, 138f };
            GameObject grupoPuntos = GameObject.Find("Puntos_Patrullaje_Carretera_Generados");
            if (grupoPuntos == null) grupoPuntos = new GameObject("Puntos_Patrullaje_Carretera_Generados");

            Transform[] puntos = new Transform[zPositions.Length];
            for (int i = 0; i < zPositions.Length; i++)
            {
                string pName = "PuntoPatrulla_" + i;
                Transform existing = grupoPuntos.transform.Find(pName);
                GameObject pGo = existing != null ? existing.gameObject : new GameObject(pName);
                pGo.transform.SetParent(grupoPuntos.transform);
                float x = (i % 2 == 0) ? -2.8f : 2.8f;
                pGo.transform.position = new Vector3(x, 0.2f, zPositions[i]);
                puntos[i] = pGo.transform;
            }
            typeof(MonstruoIA).GetField("puntosPatrullaje", flags)?.SetValue(ia, puntos);

            // Snap del agente sobre el NavMesh y activacion de movimiento
            UnityEngine.AI.NavMeshAgent agent = ia.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                UnityEngine.AI.NavMeshHit hit;
                Vector3 targetSpawn = new Vector3(-2.8f, 0.2f, 22f);
                if (UnityEngine.AI.NavMesh.SamplePosition(targetSpawn, out hit, 15f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    ia.transform.position = hit.position;
                }
                agent.enabled = true;
                agent.speed = 2.0f;
                agent.acceleration = 8f;
                agent.isStopped = false;
                if (UnityEngine.AI.NavMesh.SamplePosition(ia.transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            ia.enabled = true;
            typeof(MonstruoIA).GetMethod("ElegirDestinoPatrulla", flags)?.Invoke(ia, null);
            Debug.Log("<color=green>[Nivel 3]</color> Monstruo activado exitosamente patrullando en Z=22m con screamer funcional.");
        }
    }

    private static MuerteJugador CrearSistemaMuerte(GameObject jugador)
    {
        GameObject canvasGo = new GameObject("Canvas_MuerteJugador");
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

        // 2. Panel Muerte
        GameObject pmGo = new GameObject("PanelMuerte");
        pmGo.transform.SetParent(canvasGo.transform, false);
        UnityEngine.UI.Image imgMuerte = pmGo.AddComponent<UnityEngine.UI.Image>();
        imgMuerte.color = new Color(0.02f, 0.02f, 0.02f, 0.95f);
        RectTransform rtMuerte = pmGo.GetComponent<RectTransform>();
        rtMuerte.anchorMin = Vector2.zero;
        rtMuerte.anchorMax = Vector2.one;
        rtMuerte.offsetMin = Vector2.zero;
        rtMuerte.offsetMax = Vector2.zero;

        // Texto
        GameObject txtGo = new GameObject("TextoMuerte");
        txtGo.transform.SetParent(pmGo.transform, false);
        var txt = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
        txt.font = RuntimeUI.FuenteTitulo;
        txt.text = "HAS CAIDO EN LA OSCURIDAD";
        txt.fontSize = 58f;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
        rtTxt.anchorMin = new Vector2(0.5f, 0.65f);
        rtTxt.anchorMax = new Vector2(0.5f, 0.65f);
        rtTxt.sizeDelta = new Vector2(900f, 100f);

        // Boton Reintentar
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

        // Boton Menu
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
        tM.text = "MENU PRINCIPAL";
        tM.fontSize = 28f;
        tM.alignment = TMPro.TextAlignmentOptions.Center;
        tM.color = Color.white;
        RectTransform rtTM = txtBtnM.GetComponent<RectTransform>();
        rtTM.anchorMin = Vector2.zero;
        rtTM.anchorMax = Vector2.one;
        rtTM.offsetMin = Vector2.zero;
        rtTM.offsetMax = Vector2.zero;

        pmGo.SetActive(false);

        GameObject goControl = new GameObject("ControlMuerte");
        goControl.transform.SetParent(canvasGo.transform, false);
        MuerteJugador mj = goControl.AddComponent<MuerteJugador>();

        Transform camTransform = Camera.main != null ? Camera.main.transform : (jugador != null ? jugador.transform.Find("Camera") : null);

        // AudioSource dedicado y en 2D directo para el jumpscare
        AudioSource directScreamerAudio = (camTransform != null ? camTransform.gameObject : goControl).AddComponent<AudioSource>();
        directScreamerAudio.spatialBlend = 0f; // 2D Stereo
        directScreamerAudio.volume = 1f;
        directScreamerAudio.playOnAwake = false;

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        if (jugador != null)
        {
            typeof(MuerteJugador).GetField("playerController", flags)?.SetValue(mj, jugador.GetComponent<PlayerController>());
            typeof(MuerteJugador).GetField("playerInput", flags)?.SetValue(mj, jugador.GetComponent<UnityEngine.InputSystem.PlayerInput>());
            typeof(MuerteJugador).GetField("characterController", flags)?.SetValue(mj, jugador.GetComponent<CharacterController>());
        }
        if (camTransform != null)
        {
            typeof(MuerteJugador).GetField("camaraJugador", flags)?.SetValue(mj, camTransform);
        }

        AudioClip clipScreamer = Resources.FindObjectsOfTypeAll<AudioClip>().FirstOrDefault(c => c.name.ToLower().Contains("screamer"));
#if UNITY_EDITOR
        if (clipScreamer == null)
            clipScreamer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/Screamer.mp3");
#endif
        typeof(MuerteJugador).GetField("audioScreamer", flags)?.SetValue(mj, directScreamerAudio);
        typeof(MuerteJugador).GetField("sonidoScreamer", flags)?.SetValue(mj, clipScreamer);
        typeof(MuerteJugador).GetField("duracionScreamer", flags)?.SetValue(mj, 2.5f);
        typeof(MuerteJugador).GetField("posicionScreamer", flags)?.SetValue(mj, new Vector3(0f, -1.1f, 0.95f));
        typeof(MuerteJugador).GetField("distanciaAtaque", flags)?.SetValue(mj, 0.35f);
        typeof(MuerteJugador).GetField("velocidadAtaque", flags)?.SetValue(mj, 8f);
        typeof(MuerteJugador).GetField("panelOscuro", flags)?.SetValue(mj, imgOscuro);
        typeof(MuerteJugador).GetField("panelMuerte", flags)?.SetValue(mj, pmGo);
        typeof(MuerteJugador).GetField("duracionDesvanecimiento", flags)?.SetValue(mj, 1.5f);
        typeof(MuerteJugador).GetField("escenaReintentar", flags)?.SetValue(mj, "Level3");
        typeof(MuerteJugador).GetField("escenaMenuPrincipal", flags)?.SetValue(mj, "MainMenu");

        bR.onClick.AddListener(mj.Reintentar);
        bM.onClick.AddListener(mj.IrAlMenuPrincipal);

        return mj;
    }
    private static void AsegurarSueloYLimites()
    {
        // 1. Quitar los triangulos del terreno nativo
        Terrain[] terrenos = UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
        Material matSuelo = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == "Mat_AsfaltoEstacionamiento" || m.name == "Ground");

        foreach (var t in terrenos)
        {
            if (t == null) continue;
            if (matSuelo != null) t.materialTemplate = matSuelo;
            t.drawHeightmap = false; // Desactivar la malla wireframe sin textura del terreno nativo
        }

        // 2. Asegurar plano de suelo oscuro para el bosque y la carretera
        if (GameObject.Find("Suelo_Bosque_Carretera") == null)
        {
            GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            suelo.name = "Suelo_Bosque_Carretera";
            suelo.transform.position = new Vector3(0f, -0.2f, 75f);
            suelo.transform.localScale = new Vector3(220f, 0.2f, 320f);
            if (matSuelo != null) suelo.GetComponent<Renderer>().sharedMaterial = matSuelo;
        }

        // 3. Limite de fin de carretera (para que el jugador no caiga al vacio)
        if (GameObject.Find("Limite_Fin_Carretera") == null)
        {
            GameObject limFin = new GameObject("Limite_Fin_Carretera");
            limFin.transform.position = new Vector3(0f, 2f, 168f);
            BoxCollider colFin = limFin.AddComponent<BoxCollider>();
            colFin.size = new Vector3(30f, 10f, 2f);
        }

        // 4. Limite de inicio de carretera
        if (GameObject.Find("Limite_Inicio_Carretera") == null)
        {
            GameObject limIni = new GameObject("Limite_Inicio_Carretera");
            limIni.transform.position = new Vector3(0f, 2f, -24f);
            BoxCollider colIni = limIni.AddComponent<BoxCollider>();
            colIni.size = new Vector3(30f, 10f, 2f);
        }
    }
}
