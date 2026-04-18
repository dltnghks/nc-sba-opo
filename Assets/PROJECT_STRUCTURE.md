# Unity Project Structure

This project uses numbered top-level folders under `Assets/` to keep the main categories pinned in a stable order.

## Top-level folders

- `01.Animations`: Shared animation clips and controllers.
- `02.Art`: Source visual assets such as sprites, models, textures, materials, and VFX assets.
- `03.Audio`: BGM and SFX assets.
- `04.Data`: ScriptableObjects, localization files, and data tables.
- `05.Fonts`: Shared font assets used outside UI-specific packaging.
- `06.Prefabs`: Reusable prefab assets grouped by domain.
- `07.Scenes`: Scene files grouped by purpose.
- `08.Scripts`: Project code grouped by technical responsibility.
- `09.Settings`: Render pipeline, input, and other Unity settings assets.
- `10.Shaders`: Custom shaders and shader graphs.
- `11.Tests`: EditMode and PlayMode tests.
- `12.UI`: UI atlases, icons, and UI-only resources.
- `Plugins`: External Unity packages and third-party runtime assets.
- `Resources`: Only assets that must be loaded through `Resources.Load`.
- `TutorialInfo`: Unity starter/tutorial assets. Remove later if no longer needed.

## Scene policy

- `07.Scenes/Bootstrap`: Entry scene, initialization, persistent systems.
- `07.Scenes/Dev`: Temporary test and debug scenes.
- `07.Scenes/Gameplay`: Main gameplay scenes.
- `07.Scenes/UI`: UI showcase or isolated flow scenes.

## Script policy

- `08.Scripts/Common`: Shared helpers, extensions, constants, and utilities.
- `08.Scripts/Core`: Application bootstrap, managers, service registration, and global systems.
- `08.Scripts/Data`: Data models, config readers, save/load, and ScriptableObject logic.
- `08.Scripts/Editor`: Editor-only tooling.
- `08.Scripts/Gameplay`: Gameplay domain logic.
- `08.Scripts/UI`: UI controllers, presenters, view models, and UI-specific logic.

## Working rules

- Apply numbering only to stable top-level project folders.
- Keep subfolders semantic and do not continue numbering below the top level.
- Put reusable assets in technical folders, not inside scene folders.
- Keep scene-specific temporary assets near the scene only if reuse is unlikely.
- Keep a `.gitkeep` file in reserved empty folders so the intended Unity structure survives clone.
- Use `Resources` only when another loading path is not practical.
- Put third-party assets in `Plugins` and do not mix them with project-owned code.
- Promote stable feature data to `04.Data/ScriptableObjects` instead of hard-coding values.
