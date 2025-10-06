# Unity Cache Server - Local Setup

This directory contains a local Unity Cache Server to speed up Unity imports and builds.

## What It Does

The Unity Cache Server caches imported assets, so when you:
- Reimport assets
- Switch platforms
- Work on multiple machines
- Rebuild the project

...Unity can retrieve cached data instead of reimporting everything from scratch.

## Quick Start

### 1. Start the Server

Run in PowerShell:
```powershell
.\.unity-cache-server\start.ps1
```

Or manually:
```powershell
cd .unity-cache-server
npx unity-cache-server -P "./cache" -h "127.0.0.1" -p 8126 -w 2 -l 3
```

You should see output like:
```
[INFO] Cache Server listening on 127.0.0.1:8126
[INFO] Ready to serve requests
```

**Keep this terminal window open** while working with Unity.

### 2. Configure Unity to Use the Cache Server

#### Option A: Unity Editor (Per-Project)
1. Open your Unity project
2. Go to **Edit → Preferences → Cache Server** (Unity 2020+) or **Edit → Project Settings → Editor → Cache Server** (older versions)
3. Set:
   - **Cache Server Mode**: Enabled
   - **Endpoint**: `127.0.0.1:8126`
   - **Default IP**: `127.0.0.1`
   - **Default Port**: `8126`
4. Click **Check Connection** to verify

#### Option B: Command Line (Global)
Set environment variable before launching Unity:
```powershell
$env:UNITY_CACHE_SERVER="127.0.0.1"
$env:UNITY_CACHE_SERVER_PORT="8126"
```

### 3. Verify It's Working

In Unity Console, you should see messages like:
```
CacheServer: Connected to 127.0.0.1:8126
CacheServer: Downloaded asset 'xyz' from server
```

## Performance Tips

### Adjust Worker Threads
In `start.ps1`, change `-w 2` based on your CPU:
- **2-4 cores**: `-w 2`
- **6-8 cores**: `-w 4`
- **10+ cores**: `-w 6` or more

### Monitor Cache Size
The cache grows over time. Check size:
```powershell
Get-ChildItem ./cache -Recurse | Measure-Object -Property Length -Sum
```

Clear cache if needed:
```powershell
Remove-Item ./cache/* -Recurse -Force
```

## Expected Speed Improvements

- **Initial Import**: No benefit (cache is empty)
- **Reimport Same Assets**: 2-10x faster
- **Platform Switch**: 3-15x faster
- **Clean Build**: 2-5x faster (if cache is warm)

## Troubleshooting

### "Cannot connect to cache server"
- Ensure the server is running (check PowerShell window)
- Verify Windows Firewall isn't blocking port 8126
- Check Unity's cache server settings match `127.0.0.1:8126`

### "Performance not improved"
- First import won't benefit (cache must be populated first)
- Clear Unity's local cache: Delete `Library/ArtifactDB` folder
- Ensure cache server shows activity in its log

### Stop the Server
Press `Ctrl+C` in the PowerShell window running the server.

## Advanced Configuration

Edit `config.yml` to customize settings, then start with:
```powershell
npx unity-cache-server --config ./config.yml
```

## More Information

Official Unity Cache Server docs:
https://docs.unity3d.com/Manual/CacheServer.html
