# Delivery Rush — Unity Performance & Architecture Assessment

A **Unity 6 (C#)** technical assessment: take a working 2D game, profile it, fix what's slow, refactor its save system behind an abstraction, and connect it to a cloud backend — **without rewriting the project or breaking the game loop**.

The starting point was [an existing project](https://github.com/nicolascolsupc/DeliveryRushExam) provided by the course. This repo contains my analysis and implementation. Full technical report in [`/docs`](docs/).

*Coursework — Virtual Environments Programming II, Technical Degree in Video Game Development & Production, Universidad Provincial de Córdoba.*

---

## The brief

Diagnose the project with Unity Profiler, implement targeted optimizations, decouple the save system, and integrate Unity Gaming Services — each decision justified in writing. The constraint that shaped everything: **no rewrites, no overengineering, keep the game running.**

## Architecture work

**Save system decoupled behind an interface.** `SaveManager` originally constructed `LocalSaveService` directly, making the implementation impossible to swap. I introduced `ISaveService` (`LoadAsync` / `SaveAsync`), had both the local and cloud services implement it, and moved resolution to `ServiceLocator.Get<ISaveService>()`. `SaveServicesInstaller` now registers the right implementation from an inspector enum — switching Local ↔ Cloud is a dropdown, not a code change.

**Unity Cloud Save + anonymous Authentication.** Implemented `UgsCloudSaveService` against the real UGS API: lazy initialization cached as a `Task` so concurrent calls can't double-initialize, six discrete keys (readable in the Dashboard rather than an opaque JSON blob), missing-key handling that falls back to model defaults on a player's first save, and error handling that degrades gracefully on load but re-throws on save. The whole implementation sits behind a `DELIVERY_RUSH_UGS` compile symbol so the project still builds on a fresh clone without the UGS packages.

The payoff: activating cloud saves required **zero changes** to `SaveManager`, `LocalSaveService`, or the installer — which is the argument for having done the refactor first.

## Performance work

Profiled first, then optimized — every change below came from a measured hot path.

| Problem found | Fix |
|---|---|
| `Instantiate`/`Destroy` cycle for score popups on every completed order | `ObjectPool<ScorePopupView>` with lifecycle callbacks; popups return themselves via a callback instead of self-destructing |
| Order buttons fully destroyed and rebuilt on every list change (~every 2.5s) | Second `ObjectPool<OrderButtonView>`, same pattern |
| HUD polling score/coins/order count every frame | Observer pattern — subscribe to `ScoreChanged` / `OrdersChanged` in `OnEnable`, unsubscribe in `OnDisable` |
| Up to 18 `TMP_Text` string reassignments per frame with unchanged values | Dirty-check against the last displayed integer second; static text assigned once in `Setup` |
| LINQ (`Where().Count()`, `RemoveAll`, `FirstOrDefault`) in `OrderManager.Update` | Single reverse `for` loop doing decrement and removal in one pass; `System.Linq` dropped entirely |
| `GetComponent`, `transform`, `Canvas` and `FindFirstObjectByType` resolved per frame | Cached in `Awake`, including the `Action` delegates passed to `Setup` |
| `LayoutRebuilder.ForceRebuildLayoutImmediate` called 60×/second | Removed — Unity rebuilds layout automatically when children change |

## What I deliberately left alone

`GameManager`, `ScoreManager`, `OrderData`, `PlayerProgressData`, `ServiceLocator` and `UgsInitializer` were not modified: no critical hot path, or data contracts whose change would break persistence, or already correct. Knowing what *not* to touch was part of the assessment.

## Stack

Unity 6000.3.10f1 · C# · `UnityEngine.Pool` · Unity Gaming Services (Authentication, Cloud Save) · Unity Profiler

---

Built by **Luciana Caminos Cano** — Game Developer & Producer, Córdoba, Argentina · [LinkedIn](https://linkedin.com/in/lucianacaminos) · [itch.io](https://tuxiara.itch.io)
