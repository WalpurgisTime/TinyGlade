# 🌿 Tiny Glade – Unity Fan Version  


> Adaptation personnelle des prototypes de **Tiny Glade** ([Anastasia Opara](https://anastasiaopara.com)), réalisée avec Unity.  
> Une tentative de recréer les sensations douces et organiques du jeu, avec uniquement les outils natifs du moteur Unity.

---

## 🎯 Objectif du projet

Ce projet m’a permis d’explorer en profondeur :
- Le rendu procédural dans Unity ([instancing](https://docs.unity3d.com/Manual/DrawCallBatching.html), shaders, courbes dynamiques)
- L’architecture orientée moteur natif (sans ECS externe, sans OpenGL personnalisé)
- Les fondations graphiques d’un jeu comme *Tiny Glade*, pour mieux en comprendre la conception artistique et technique

---

## 🛠️ Technologies utilisées

- [Unity](https://unity.com/) (2022.x ou 2023.x)
- [C#](https://learn.microsoft.com/fr-fr/dotnet/csharp/)
- [Shader Graph](https://unity.com/shader-graph) / HLSL
- [ComputeBuffer](https://docs.unity3d.com/ScriptReference/ComputeBuffer.html)
- Pas d’OpenGL natif, pas d’ECS externe : tout est fait avec les outils Unity standards

---

## 📦 Lancer le projet

1. Cloner ce dépôt :
   ```bash
   git clone https://github.com/WalpurgisTime/TinyGlade.git
