// À placer dans Assets/Editor/BuildMaleFreeController.cs (le dossier "Editor" est obligatoire).
// Menu : Tools > Build MaleFree Controller
//
// Construit Assets/MaleFreeController.controller à partir des animations humanoïdes
// du pack "Supercyan Character Pack Free Sample" (common_people@*.FBX).
// Les clips sont copiés dans Assets/Animations/MaleFree/ pour pouvoir régler leur boucle.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BuildMaleFreeController
{
    const string ClipFolder = "Assets/Animations/MaleFree";
    const string OutputPath = "Assets/MaleFreeController.controller";

    // Seuils du Blend Tree : PlayerControler envoie |Input.GetAxis| * MoveSpeed (0 à 5).
    const float WalkSpeed = 2.5f;
    const float RunSpeed = 5f;

    // Le clip "wave" est long : on revient à Locomotion à cette fraction du clip (0 à 1).
    const float CastExitTime = 0.35f;

    static List<string> fbxPaths;

    [MenuItem("Tools/Build MaleFree Controller")]
    static void Build()
    {
        Directory.CreateDirectory(ClipFolder);
        AssetDatabase.Refresh();

        fbxPaths = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("common_people t:Model"))
            fbxPaths.Add(AssetDatabase.GUIDToAssetPath(guid));

        AnimationClip idle    = Import("idle",       "MF_Idle",    true);
        AnimationClip walk    = Import("walk",       "MF_Walk",    true);
        AnimationClip run     = Import("run",        "MF_Run",     true);
        AnimationClip jumpUp  = Import("jump-up",    "MF_JumpUp",  false);
        AnimationClip jumpMid = Import("jump-float", "MF_Float",   true);
        AnimationClip jumpEnd = Import("jump-down",  "MF_Land",    false);
        AnimationClip castC   = Import("wave",       "MF_Cast",    false);
        AnimationClip pickupC = Import("pickup",     "MF_Pickup",  false);
        AnimationClip victory = Import("wave",       "MF_Victory", true);

        if (idle == null || walk == null || run == null || jumpUp == null || jumpMid == null ||
            jumpEnd == null || castC == null || pickupC == null || victory == null)
        {
            Debug.LogError("Un clip est introuvable : voir les erreurs ci-dessus. Contrôleur non créé.");
            return;
        }

        AssetDatabase.DeleteAsset(OutputPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);

        // Les noms "MoveSpeed", "Grounded" et "Jump" sont ceux utilisés par PlayerControler.cs
        ctrl.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Bool);   // écrit par le script, non utilisé ici
        ctrl.AddParameter("Cast", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Pickup", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Victory", AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;

        // Locomotion : idle / marche / course (état par défaut)
        BlendTree tree;
        AnimatorState loco = ctrl.CreateBlendTreeInController("Locomotion", out tree);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "MoveSpeed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f);
        tree.AddChild(walk, WalkSpeed);
        tree.AddChild(run, RunSpeed);

        var jump    = State(sm, "Jump", jumpUp);
        var airborne = State(sm, "Airborne", jumpMid);
        var land    = State(sm, "Land", jumpEnd);
        var cast    = State(sm, "Cast", castC);
        var pickup  = State(sm, "Pickup", pickupC);
        var victoryState = State(sm, "Victory", victory);

        // Saut : montée, vol, atterrissage
        var t = Go(loco, jump, false, 0f, 0.05f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocityY");

        t = Go(loco, airborne, false, 0f, 0.1f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        t.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");

        t = Go(jump, land, false, 0f, 0.05f);
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");

        t = Go(jump, airborne, true, 0.8f, 0.15f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        t = Go(airborne, land, false, 0f, 0.05f);
        t.AddCondition(AnimatorConditionMode.If, 0, "Grounded");

        // Land : se coupe dès que le joueur se déplace, sinon se termine seul
        t = Go(land, loco, false, 0f, 0.1f);
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveSpeed");
        Go(land, loco, true, 0.6f, 0.1f);

        // Sort et ramassage : déclenchés depuis n'importe quel état
        AnyTrigger(sm, cast, "Cast");
        AnyTrigger(sm, pickup, "Pickup");
        Go(cast, loco, true, CastExitTime, 0.1f);
        Go(pickup, loco, true, 0.9f, 0.1f);

        // Victoire (bool)
        var v = sm.AddAnyStateTransition(victoryState);
        v.hasExitTime = false;
        v.duration = 0.1f;
        v.canTransitionToSelf = false;
        v.AddCondition(AnimatorConditionMode.If, 0, "Victory");

        t = Go(victoryState, loco, false, 0f, 0.1f);
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "Victory");

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        Selection.activeObject = ctrl;
        Debug.Log("MaleFreeController créé : " + OutputPath);
    }

    // Trouve common_people@<key>.FBX, copie son clip dans ClipFolder et règle la boucle.
    static AnimationClip Import(string key, string newName, bool loop)
    {
        string wanted = "common_people@" + key;

        foreach (string path in fbxPaths)
        {
            if (Path.GetFileNameWithoutExtension(path) != wanted) continue;

            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var source = o as AnimationClip;
                if (source == null || source.name.StartsWith("__preview__")) continue;

                AnimationClip copy = Object.Instantiate(source);
                copy.name = newName;

                var settings = AnimationUtility.GetAnimationClipSettings(copy);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(copy, settings);

                string target = ClipFolder + "/" + newName + ".anim";
                AssetDatabase.DeleteAsset(target);
                AssetDatabase.CreateAsset(copy, target);
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(target);
            }
        }

        Debug.LogError("Clip introuvable : " + wanted + ".FBX");
        return null;
    }

    static AnimatorState State(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        var s = sm.AddState(name);
        s.motion = clip;
        return s;
    }

    static AnimatorStateTransition Go(AnimatorState from, AnimatorState to,
                                      bool exitTime, float exitAt, float duration)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = exitTime;
        t.exitTime = exitAt;
        t.hasFixedDuration = true;
        t.duration = duration;
        return t;
    }

    static void AnyTrigger(AnimatorStateMachine sm, AnimatorState to, string trigger)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
    }
}