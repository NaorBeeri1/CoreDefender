# 🛡️ Core Defender

**Core Defender** is a tactical 2D tower defense game built from scratch in Unity. Defend your energy core against waves of hostile units by strategically placing, upgrading, and managing thermal limits across a variety of defensive structures.

🎮 **[Play Core Defender on Itch.io](https://studios-nb.itch.io/coredefender)**

---

## ✨ Key Features
* **Tactical Tower Variety:** Build Base Turrets, Laser Snipers, Cryo Emitters, Mass Drivers, and Ion Beacons—each featuring unique multi-tier upgrade paths and visual evolution.
* **Thermal Management System:** Turrets generate heat when firing and require careful cycle management to avoid overheating.
* **Dynamic Wave Scaling:** Face escalating threats, from standard ground units to floating laser drones, tracked in real-time via a custom HUD.
* **Advanced AI & Systems:** Implements an Enemy State Machine architecture, a global spatial Targeting Manager, and a custom Object Pooler for high-performance memory optimization.

---

## 💻 Technical Architecture & Design Patterns
* **Engine:** Unity (Universal Render Pipeline - URP 2D)
* **Language:** C# (.NET)
* **Design Patterns Used:**
  * **Singleton Pattern:** Managed across core systems (ShopManager, TargetingManager, PlayerStats, ScoreManager).
  * **Event Bus Pattern (`GameEventBus`):** Decouples gameplay systems for modular event-driven communication (e.g., handling score and credit rewards on enemy destruction).
  * **State Machine Pattern (`IEnemyState`, `EnemyMovingState`):** Modularizes enemy behaviors and movement states cleanly.
  * **Object Pooling (`ObjectPooler`):** Reduces garbage collection overhead by recycling projectiles.

---

## 🚀 How to Play
1. Visit the [Itch.io Page](https://studios-nb.itch.io/coredefender) to download the build.
2. Extract the `.zip` archive.
3. Run `Core Defender.exe` and protect the core!
