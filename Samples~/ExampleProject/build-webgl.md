# WebGL Build Instructions for E2E Testing

To run the E2E tests, you need to create a WebGL build of the Unity project.

## Steps to Create WebGL Build:

1. **Open Unity Project**
   - Open the project in Unity Editor
   - Ensure all scenes are properly configured

2. **Switch to WebGL Platform**
   - Go to File → Build Settings
   - Select "WebGL" from the platform list
   - Click "Switch Platform" if not already selected

3. **Configure WebGL Settings**
   - Go to Edit → Project Settings → Player → WebGL Settings
   - Set the following recommended settings:
     - **Compression Format**: Disabled (for testing) or Gzip
     - **WebGL Template**: Default or Minimal
     - **Color Space**: Linear (if using URP)

4. **Build the Project**
   - In Build Settings, click "Build"
   - Choose a folder named `WebGL-Build` in the project root
   - Wait for the build to complete

5. **Directory Structure After Build**
   ```
   ExampleProject/
   ├── WebGL-Build/
   │   ├── index.html
   │   ├── Build/
   │   │   ├── WebGL-Build.loader.js
   │   │   ├── WebGL-Build.wasm
   │   │   └── WebGL-Build.data
   │   └── TemplateData/
   └── tests/e2e/
   ```

## Running E2E Tests:

Once the WebGL build is ready:

```bash
# Install dependencies (if not already done)
npm install

# Run all E2E tests
npm test

# Run tests in UI mode (interactive)
npm run test:ui

# Run specific test file
npx playwright test bubble-positioning-regression.spec.ts

# Run tests with debug mode
npm run test:debug
```

## Test Server Configuration:

The tests automatically start a local Python HTTP server on port 8080 to serve the WebGL build. Ensure:

1. Python is installed on your system
2. Port 8080 is available
3. The `WebGL-Build` folder exists in the project root

## Troubleshooting:

- **Unity WebGL fails to load**: Check browser console for errors, ensure all assets are properly included
- **Tests timeout**: Increase timeout values in test files if Unity takes longer to initialize
- **Server not starting**: Manually start server with `python -m http.server 8080` in WebGL-Build folder
- **Canvas not found**: Ensure Unity WebGL template includes canvas with id="unity-canvas"

## Test Categories:

1. **Game Startup Tests**: Verify Unity loads and initializes properly
2. **Shooting Mechanics**: Test bubble shooting and trajectory
3. **Positioning Regression**: Critical tests for bubble attachment accuracy
4. **Collision Detection**: Verify collision system works correctly  
5. **Scoring System**: Test match detection and scoring
6. **Game Over Logic**: Test end-game conditions and restart

The positioning regression tests specifically address the issue where bubbles contact at one point but attach several cells away from the intended spot.