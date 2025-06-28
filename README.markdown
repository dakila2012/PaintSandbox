# PaintSandbox

A 2D multiplayer sandbox game inspired by *Microsoft Paint*, built in Unity 2022.3. Players can draw colored pixels on a grid, toggle the brush, and select colors via a UI, with plans for Photon PUN multiplayer support.

## Project Overview
- **Genre**: 2D Creative Sandbox
- **Platform**: Unity 2022.3
- **Features**:
  - Cursor movement (mouse or WASD, toggled with `M` key).
  - Brush tool to draw colored pixels (red, blue, green) on a 512x512 grid.
  - UI with buttons for toggling the brush and selecting colors.
  - TextMeshPro for UI text.
  - Future: Multiplayer drawing with Photon PUN.
- **Repository**: https://github.com/dakila2012/PaintSandbox

## Getting Started

### Prerequisites
- **Unity**: Version 2022.3.x (install via Unity Hub).
- **Git**: For version control (install from https://git-scm.com/).
- **GitHub Account**: For repository access.
- **TextMeshPro**: Included in Unity (import essentials via `Window > TextMeshPro > Import TMP Essential Resources`).
- **Photon PUN**: For multiplayer (included in `Assets/Photon`).

### Setup
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/dakila2012/PaintSandbox.git
   cd PaintSandbox
   ```
2. **Open in Unity**:
   - Open Unity Hub, click **Add**, select the `PaintSandbox` folder.
   - Open the project in Unity 2022.3.x.
3. **Configure Input System**:
   - Go to **Edit > Project Settings > Player > Active Input Handling**.
   - Set to **Input System Package (New)** or **Both**.
4. **Import TextMeshPro Essentials**:
   - In Unity, go to **Window > TextMeshPro > Import TMP Essential Resources**.
5. **Verify Scene**:
   - Open `Assets/Scenes/MainCanvas.unity`.
   - Check **Hierarchy**:
     - `Main Camera` (orthographic, size=1, position=(0, 0, -10)).
     - `Global Light 2D`.
     - `cursor` (with `CursorFollow.cs`, Sorting Order=2).
     - `background` (with `background_grid.png`, Sorting Order=0).
     - `UICanvas` (with `ToggleBrushButton`, `RedButton`, `BlueButton`, `GreenButton`).
     - `EventSystem` (with `InputSystemUIInputModule`).
     - `CanvasParent` (empty).
     - `ToolManager` (with `BrushTool.cs`, `PixelRed.prefab`, `CanvasParent`).

### Running the Game
- Press **Play** in Unity.
- **Controls**:
  - Move cursor: Mouse or WASD (toggle with `M`).
  - Toggle brush: Click `ToggleBrushButton`.
  - Draw: Hold left mouse button to place colored pixels (snapped to grid).
  - Change color: Click `RedButton`, `BlueButton`, or `GreenButton`.
- **Debug**: Check Console for logs (e.g., “Drew pixel at (x, y, 0)”).

## Contributing
We use Git and GitHub for version control. Follow these steps to contribute:

1. **Create a Feature Branch**:
   ```bash
   git checkout -b feature-<your-feature>
   ```
   - Example: `feature-multiplayer`, `feature-ui-improvements`.

2. **Make Changes**:
   - Edit scenes, scripts, or assets in Unity.
   - Avoid editing `MainCanvas.unity` simultaneously to prevent conflicts.
   - Prefer prefabs (e.g., `UICanvas.prefab`) for UI changes.

3. **Commit and Push**:
   ```bash
   git add .
   git commit -m "Add <feature-description>"
   git push origin feature-<your-feature>
   ```

4. **Create a Pull Request**:
   - On GitHub, create a PR from your branch to `main`.
   - Assign a reviewer (e.g., dakila2012 or friend).

5. **Resolve Conflicts**:
   - Use UnityYAMLMerge for `.unity` file conflicts:
     ```bash
     git mergetool
     ```
   - Configure UnityYAMLMerge:
     ```bash
     git config --global mergetool.unityyamlmerge.cmd '"C:/Program Files/Unity/Hub/Editor/2022.3.x/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
     git config --global merge.tool unityyamlmerge
     ```

6. **Pull Frequently**:
   ```bash
   git checkout main
   git pull origin main
   git checkout feature-<your-feature>
   git merge main
   ```

## Project Structure
- **Assets/Scenes**: Contains `MainCanvas.unity` (main game scene).
- **Assets/Scripts**:
  - `BrushTool.cs`: Handles brush toggling, color selection, and pixel drawing.
  - `CursorFollow.cs`: Manages cursor movement (mouse/WASD).
- **Assets/Prefabs**: Contains `PixelRed.prefab` for drawn pixels.
- **Assets/Sprites**: Includes `cursor.png`, `background_grid.png`.
- **Assets/TextMesh Pro**: TMP assets for UI text.
- **Assets/Photon**: Photon PUN for multiplayer (WIP).

## Known Issues
- None currently; brush and UI are functional.
- Future: Implement Photon PUN multiplayer for pixel syncing.

## Team
- **dakila2012**: Gameplay, brush tool, cursor movement.
- **Friend**: UI design, TMP font fixes.

## License
MIT License (see `LICENSE` file, to be added).

## Contact
- **Discord**: Coordinate tasks and share PRs.
- **Trello**: Track tasks (e.g., “Add multiplayer”, “Polish UI”).