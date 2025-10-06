# Configure Unity to Use Local Cache Server

## Quick Setup (Unity 6)

### Step 1: Start the Cache Server
Make sure the cache server is running:
```powershell
.\.unity-cache-server\start.ps1
```

You should see:
```
[Cluster:M] Cache Server version 6.4.0
[Cluster:1] Cache Server ready on 127.0.0.1:8126
[Cluster:2] Cache Server ready on 127.0.0.1:8126
```

### Step 2: Configure Unity Editor

1. **Open Unity** (close and reopen if already open)

2. **Go to Preferences:**
   - Menu: **Edit → Preferences** (Windows/Linux)
   - Or: **Unity → Settings** (Mac)

3. **Find Cache Server Settings:**
   - Look in the left sidebar for **"Cache Server (v2)"**
   - If you don't see it, look for **"Asset Pipeline"** section

4. **Configure:**
   ```
   Mode: Remote (or Enabled)
   Default IP address: 127.0.0.1
   Default port: 8126
   ```

5. **Test Connection:**
   - Click **"Check Connection"** button
   - Should show: ✓ Connected to 127.0.0.1:8126

6. **Enable:**
   - Make sure the checkbox is enabled/checked

### Step 3: Verify It's Working

After configuring, check the Unity Console for messages like:
```
AssetDatabase: Connected to Cache Server at 127.0.0.1:8126
Cache Server: Downloaded <asset-name>
```

Also check the cache server terminal - you should see activity:
```
[INFO] GET /resource/<hash>
[INFO] PUT /resource/<hash>
```

## Troubleshooting

### "Cannot connect to cache server"

**Check 1:** Is the server running?
```powershell
.\.unity-cache-server\check-status.ps1
```

**Check 2:** Is Unity using the correct settings?
- Double-check IP is `127.0.0.1` (not `localhost`)
- Double-check port is `8126`
- Try toggling the enable checkbox off and on

**Check 3:** Windows Firewall
If you're running Unity as Administrator, the firewall might block localhost connections:
```powershell
# Allow Node.js through firewall (run as Administrator)
New-NetFirewallRule -DisplayName "Unity Cache Server" -Direction Inbound -Program "C:\Program Files\nodejs\node.exe" -Action Allow
```

### "Not seeing performance improvements"

**First import won't be faster** - The cache needs to be populated first.

Try this test:
1. Let Unity finish importing everything (cache is now warm)
2. Delete the `Library` folder
3. Reopen Unity
4. Imports should now be MUCH faster (using cached data)

### Check if cache is being used

Watch the cache server terminal for activity. You should see:
```
[INFO] GET requests (Unity downloading from cache)
[INFO] PUT requests (Unity uploading to cache)
```

## Alternative: Project-Level Settings

You can also configure per-project by editing `ProjectSettings/EditorSettings.asset`:

```yaml
cacheServerMode: 2  # 0=Disabled, 1=Local, 2=Remote
cacheServerEndpoint: 127.0.0.1:8126
cacheServerEnableDownload: 1
cacheServerEnableUpload: 1
```

Then restart Unity to apply changes.

## Performance Expectations

- **First import**: No benefit (cache is empty)
- **Re-import same project**: 2-10x faster
- **Switch platforms**: 5-15x faster
- **Clean Library folder**: 3-8x faster
- **Multiple machines**: Huge benefit (shared cache)

## Stop the Server

When done, press `Ctrl+C` in the cache server terminal window.
