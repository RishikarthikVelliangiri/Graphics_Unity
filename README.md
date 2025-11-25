# Graphics_Unity

A Unity project showcasing graphics techniques and visual experiments. This repository contains scenes, shaders, art assets, and scripts used to explore lighting, shading, post-processing, and rendering pipelines in Unity.

> Note: This README is a general, ready-to-use explanation. If you want a README tailored to the exact files and scenes in this repo, I can inspect the repository and update this file with precise scene names, script descriptions, screenshots, and the exact Unity version used.

---

## Table of Contents
- [Project Overview](#project-overview)
- [Features](#features)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [How to Run](#how-to-run)
- [Controls](#controls)
- [Scenes & What They Demonstrate](#scenes--what-they-demonstrate)
- [Key Scripts & Shaders](#key-scripts--shaders)
- [Performance & Profiling](#performance--profiling)
- [Building](#building)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)
- [License & Credits](#license--credits)
- [Contact](#contact)

---

## Project Overview
This Unity project is built to demonstrate and experiment with real-time graphics techniques. It includes sample scenes for evaluating different rendering approaches, custom shaders, lighting setups, camera effects, and post-processing. Useful as a learning reference, demo portfolio, or starting point for graphics-focused projects.

## Features
- Example scenes showcasing lighting and shading setups
- Custom surface/fragment/compute shaders (examples)
- Post-processing stack (bloom, color grading, motion blur, etc.)
- Camera controllers for orbit and first-person preview
- Materials and prefabs for quick prototyping
- A small toolset for toggling rendering options at runtime

## Requirements
- Unity Editor (recommended: specify the exact version used in this repo; if unknown, try Unity 2021.3 LTS, 2022.3 LTS, or 2023.3+ LTS)
- Git (for cloning)
- Optional: Unity Hub for managing editor versions
- (If used) Scriptable Render Pipeline: URP or HDRP — check ProjectSettings or Packages for which pipeline is configured

## Getting Started
1. Clone the repository:
   - git clone https://github.com/RishikarthikVelliangiri/Graphics_Unity.git
2. Open Unity Hub → Add the project folder you cloned.
3. Open the project with the Unity Editor version that matches the project's ProjectVersion.txt (if present). If you're unsure, open with an LTS release and allow Unity to upgrade packages if necessary.
4. In the Editor, open the Scenes folder and double-click a scene to load it.

## Project Structure (typical)
- Assets/ - main Unity assets (Scenes, Scripts, Materials, Shaders, Prefabs, Textures)
  - Assets/Scenes/ - demo scenes
  - Assets/Scripts/ - gameplay, camera, and utilities
  - Assets/Shaders/ - custom shader source files
  - Assets/Materials/ - material setups for scenes
  - Assets/Prefabs/ - reusable prefabs
  - Assets/Art/ - textures, models, and other art assets
- Packages/ - Unity packages used by the project
- ProjectSettings/ - project configuration files
- README.md - this file

Note: Update this section with actual folder names from the repository for more precision.

## How to Run
- Open a scene from Assets/Scenes/ (e.g., DemoScene.unity).
- Press the Play button in the Unity Editor to run the scene.
- Use the in-scene UI (if provided) to toggle effects and rendering settings.

## Controls
Common control schemes included in graphics demos:
- Orbit Camera: Mouse drag to rotate, scroll to zoom
- First-Person Preview: WASD movement, mouse look
- Toggle UI: Tab or UI buttons
- If custom controls exist, see the script comments (e.g., CameraController.cs) for exact input mapping.

## Scenes & What They Demonstrate
(Replace these placeholders with the actual scenes in the repo)
- Demo_Lighting.unity — demo of dynamic and baked lighting, shadows, and light probes
- Demo_ShaderSamples.unity — sample materials using custom shaders (PBR, rim lighting, toon shading)
- Demo_PostProcessing.unity — post-processing stack comparison (no post-processing, basic, advanced)

## Key Scripts & Shaders
(Replace placeholders with actual filenames and brief descriptions)
- Scripts/CameraController.cs — orbit and FPS camera controller
- Scripts/LightingManager.cs — runtime toggles for lights and shadow quality
- Shaders/CustomPBR.shader — example PBR shader illustrating custom BRDF tweaks
- Shaders/FX_Bloom.shader — simple screen-space bloom shader

Include inline comments in scripts to explain algorithmic or tricky parts, and consider adding small README snippets next to complex shaders.

## Performance & Profiling
- Use the built-in Unity Profiler (Window → Analysis → Profiler) to inspect CPU, GPU, and memory usage.
- Use the Frame Debugger (Window → Analysis → Frame Debugger) to step through draw calls and diagnose overdraw or redundant passes.
- Tips:
  - Reduce shadow resolution and cascades for better framerate on lower-end machines.
  - Combine meshes and use GPU instancing where suitable.
  - Use LOD groups for large scenes.

## Building
To create a standalone build:
1. File → Build Settings.
2. Add the scene(s) you want in the build.
3. Select target platform (e.g., PC/Mac/Linux Standalone) and set build options.
4. Click Build and choose an output folder.

For mobile or WebGL builds, configure quality and graphics settings accordingly (check Player Settings and Graphics settings for pipeline-specific adjustments).

## Contributing
Contributions are welcome:
- Report issues and bugs in GitHub Issues.
- Open pull requests for fixes, improvements, or new demo scenes.
- Keep commits focused and document changes in commit messages.
- Include screenshots or short GIFs for visual changes.

If you want, I can create initial Issues or a CONTRIBUTING.md template for this repo.

## Troubleshooting
- "Missing Scripts" warnings: re-link scripts on prefabs if file paths changed.
- Shader compile errors: check the Unity Console for the shader error line and shader target/platform directives.
- Package incompatibilities: open Packages/manifest.json and verify package versions; update Unity Editor if required.

## License & Credits
- Add a LICENSE file to indicate project license (MIT, Apache-2.0, etc.). If not present, specify a license you prefer.
- Credit any third-party assets or tutorials used in the project.

## Contact
Project maintained by RishikarthikVelliangiri.
- GitHub: https://github.com/RishikarthikVelliangiri


