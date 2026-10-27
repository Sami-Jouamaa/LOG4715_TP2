#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// À placer dans Assets/Editor/SlimeTestArena.cs
// Menu : Tools > Slime > Créer l'arène de test
// L'arène est générée à la position de l'objet sélectionné (sinon à l'origine).
// Le joueur est supposé au centre, regardant vers +Z.
public static class SlimeTestArena
{
    [MenuItem("Tools/Slime/Créer l'arène de test")]
    static void Create()
    {
        Vector3 origin = Selection.activeTransform ? Selection.activeTransform.position : Vector3.zero;

        var root = new GameObject("SlimeTestArena");
        Undo.RegisterCreatedObjectUndo(root, "Créer arène de test slime");
        root.transform.position = origin;

        var walls = Group(root, "Murs");
        var platforms = Group(root, "Plateformes");
        var ramps = Group(root, "Pentes");
        var misc = Group(root, "Divers");

        // ---------- MURS ----------
        Block(walls, "Mur_Plat",        new Vector3(0, 2, 10),    new Vector3(8, 4, 0.5f));
        Block(walls, "Mur_Haut",        new Vector3(-6, 4, 10),   new Vector3(3, 8, 0.5f));
        // Coin à 90° (pour tester l'étalement du slime dans un angle)
        Block(walls, "Coin_A",          new Vector3(10, 2, 6),    new Vector3(0.5f, 4, 6));
        Block(walls, "Coin_B",          new Vector3(7, 2, 9),     new Vector3(6, 4, 0.5f));
        // Couloir étroit (~1,5 m de large)
        Block(walls, "Couloir_Gauche",  new Vector3(-13, 2, 0),   new Vector3(0.5f, 4, 8));
        Block(walls, "Couloir_Droite",  new Vector3(-11, 2, 0),   new Vector3(0.5f, 4, 8));

        // ---------- PLATEFORMES ----------
        // Escalier de hauteurs croissantes avec 1 m d'écart entre chaque
        Block(platforms, "Plateforme_1m", new Vector3(4, 0.5f, -4),  new Vector3(3, 1, 3));
        Block(platforms, "Plateforme_2m", new Vector3(8, 1f, -4),    new Vector3(3, 2, 3));
        Block(platforms, "Plateforme_3m", new Vector3(12, 1.5f, -4), new Vector3(3, 3, 3));
        Block(platforms, "Plateforme_5m", new Vector3(16, 2.5f, -4), new Vector3(3, 5, 3));
        // Plateforme fine suspendue (tester le slime sur le dessus et le dessous)
        Block(platforms, "Plateforme_Fine", new Vector3(0, 2, -4),   new Vector3(3, 0.2f, 3));
        // Surplomb / plafond
        Block(platforms, "Surplomb",      new Vector3(0, 3.5f, -10), new Vector3(5, 0.3f, 5));

        // ---------- PENTES (15°, 30°, 45°, 60°) ----------
        float[] angles = { 15f, 30f, 45f, 60f };
        for (int i = 0; i < angles.Length; i++)
        {
            float a = angles[i];
            float halfLen = 3f; // la rampe fait 6 m de long
            float y = halfLen * Mathf.Sin(a * Mathf.Deg2Rad);
            Block(ramps, $"Pente_{a:0}deg",
                new Vector3(-6 + i * 4, y, -18),
                new Vector3(3, 0.3f, 6),
                new Vector3(-a, 0, 0));
        }

        // ---------- DIVERS ----------
        // Fosse-bordure : petit mur bas pour tester le slime qui déborde
        Block(misc, "Muret_Bas", new Vector3(-4, 0.25f, 4), new Vector3(6, 0.5f, 0.5f));

        Selection.activeGameObject = root;
        Debug.Log("Arène de test slime créée (Ctrl+Z pour annuler).");
    }

    static Transform Group(GameObject root, string name)
    {
        var g = new GameObject(name);
        g.transform.SetParent(root.transform, false);
        return g.transform;
    }

    static GameObject Block(Transform parent, string name, Vector3 localPos, Vector3 scale, Vector3? euler = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(go, "Créer " + name);
        go.name = name;
        go.isStatic = true;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.transform.localEulerAngles = euler ?? Vector3.zero;
        return go;
    }
}
#endif
