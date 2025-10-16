Unknown pseudo class "last-child" in StyleSheet BubbleShooterStyles
UnityEngine.UIElements.StyleSheet:OnEnable ()

[Story Test] Loaded settings for project: YourProjectName
UnityEngine.Debug:Log (object)
TinyWalnutGames.StoryTest.Shared.StoryTestSettings:LoadSettings () (at ./Library/PackageCache/com.tinywalnutgames.storytest@8aeeae493581/Runtime/Shared/StoryTestSettings.cs:64)
TinyWalnutGames.StoryTest.Shared.StoryTestSettings:get_Instance () (at ./Library/PackageCache/com.tinywalnutgames.storytest@8aeeae493581/Runtime/Shared/StoryTestSettings.cs:43)
TinyWalnutGames.StoryTest.ProductionExcellenceStoryTest:SafeGetSettings () (at ./Library/PackageCache/com.tinywalnutgames.storytest@8aeeae493581/Runtime/ProductionExcellenceStoryTest.cs:490)
TinyWalnutGames.StoryTest.ProductionExcellenceStoryTest:Start () (at ./Library/PackageCache/com.tinywalnutgames.storytest@8aeeae493581/Runtime/ProductionExcellenceStoryTest.cs:90)

[Story Test] Runner ready. Waiting for manual validation trigger...
UnityEngine.Debug:Log (object)
TinyWalnutGames.StoryTest.ProductionExcellenceStoryTest:Start () (at ./Library/PackageCache/com.tinywalnutgames.storytest@8aeeae493581/Runtime/ProductionExcellenceStoryTest.cs:106)

Camera setup: gridTop=16.9736, gridBottom=-0.3464012, shooterY=-5.542401, orthoSize=12.124
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:SetupCamera () (at Assets/Scripts/BubbleShooterGameSetup.cs:73)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:32)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

BubbleShooterUI.uxml not found in Resources/UI/. Creating basic UI programmatically.
UnityEngine.Debug:LogWarning (object)
BubbleShooter.BubbleShooterGameSetup:SetupUI () (at Assets/Scripts/BubbleShooterGameSetup.cs:94)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:35)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

Creating BubbleShooterGameManager
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateGameManager () (at Assets/Scripts/BubbleShooterGameSetup.cs:242)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:38)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

BubbleShooterGameManager created and configured
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateGameManager () (at Assets/Scripts/BubbleShooterGameSetup.cs:258)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:38)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

Creating BubbleShooter
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateShooter () (at Assets/Scripts/BubbleShooterGameSetup.cs:263)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:41)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

Danger line created at Y: -7.2744
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateDangerLine () (at Assets/Scripts/BubbleShooterGameSetup.cs:373)
BubbleShooter.BubbleShooterGameSetup:CreateShooter () (at Assets/Scripts/BubbleShooterGameSetup.cs:306)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:41)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

Boundary colliders created: left=-21.05378, right=21.05378, top=17.4736, bottom=-6.042401
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateBoundaryColliders () (at Assets/Scripts/BubbleShooterGameSetup.cs:337)
BubbleShooter.BubbleShooterGameSetup:CreateShooter () (at Assets/Scripts/BubbleShooterGameSetup.cs:309)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:41)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

BubbleShooter created and positioned at Y: -5.542401
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameSetup:CreateShooter () (at Assets/Scripts/BubbleShooterGameSetup.cs:311)
BubbleShooter.BubbleShooterGameSetup:SetupGame () (at Assets/Scripts/BubbleShooterGameSetup.cs:41)
BubbleShooter.BubbleShooterGameSetup:Start () (at Assets/Scripts/BubbleShooterGameSetup.cs:26)

Loaded current bubble at position: (0.00, -5.54, 0.00), scale: (1.00, 1.00, 1.00)
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:LoadNextBubble () (at Assets/Scripts/BubbleShooter.cs:197)
BubbleShooter.BubbleShooter:InitializeShooter () (at Assets/Scripts/BubbleShooter.cs:36)
BubbleShooter.BubbleShooter:Start () (at Assets/Scripts/BubbleShooter.cs:23)

Shooting bubble in direction: (0.52, 0.85)
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:87)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble sprite enabled with color: RGBA(0.000, 0.000, 1.000, 1.000)
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:100)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble.SetMoving(True) called
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:152)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:104)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Rigidbody2D set - isKinematic: False, velocity: (0.00, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:164)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:104)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

========== [BARREL FIRED] SHOT 841e55ef ==========
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:174)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble.SetMoving(True) called
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:152)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:176)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Rigidbody2D set - isKinematic: False, velocity: (0.00, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:164)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:176)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble.Shoot called with direction: (0.52, 0.85), speed: 10
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:178)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble velocity set to: (5.21, 8.54)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:191)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Shot bubble collider disabled - will re-enable based on time/distance
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:Shoot (UnityEngine.Vector2,single) (at Assets/Scripts/Bubble.cs:198)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:108)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble shot with speed: 10 from position: (0.00, -5.54, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:110)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Bubble in flight: True - Grid descent PAUSED
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameManager:SetBubbleInFlight (bool) (at Assets/Scripts/BubbleShooterGameManager.cs:315)
BubbleShooter.BubbleShooter:ShootBubble () (at Assets/Scripts/BubbleShooter.cs:113)
BubbleShooter.BubbleShooter:HandleInput () (at Assets/Scripts/BubbleShooter.cs:59)
BubbleShooter.BubbleShooter:Update () (at Assets/Scripts/BubbleShooter.cs:28)

Loaded current bubble at position: (0.00, -5.54, 0.00), scale: (1.00, 1.00, 1.00)
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:LoadNextBubble () (at Assets/Scripts/BubbleShooter.cs:197)
BubbleShooter.BubbleShooter:LoadNextBubbleDelayed () (at Assets/Scripts/BubbleShooter.cs:124)

Enabling collider: distance check passed (8.20 units >= 8 units)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:ShouldEnableCollider () (at Assets/Scripts/Bubble.cs:234)
BubbleShooter.Bubble:Update () (at Assets/Scripts/Bubble.cs:212)

Shot bubble collider re-enabled (time/distance check passed)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:Update () (at Assets/Scripts/Bubble.cs:215)

*** COLLISION at position (8.44, 8.29, 0.00) with: Bubble_18_9, collider enabled: True, my velocity: (5.21, 8.54)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:341)

-> Collision with STATIONARY BUBBLE at position: (8.50, 9.18, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:346)

Bubble.SetMoving(False) called
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:152)
BubbleShooter.Bubble:StopMoving () (at Assets/Scripts/Bubble.cs:243)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:387)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

Rigidbody2D set - isKinematic: True, velocity: (0.00, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SetMoving (bool) (at Assets/Scripts/Bubble.cs:164)
BubbleShooter.Bubble:StopMoving () (at Assets/Scripts/Bubble.cs:243)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:387)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

Bubble in flight: False - Grid descent RESUMED
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooterGameManager:SetBubbleInFlight (bool) (at Assets/Scripts/BubbleShooterGameManager.cs:315)
BubbleShooter.Bubble:StopMoving () (at Assets/Scripts/Bubble.cs:251)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:387)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

=== COLLISION START === Bubble at world pos: (8.44, 8.29, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:390)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] About to call FindNearestGridCoordinates()
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:397)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Processing 3 overlapping colliders
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:471)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Skipping: contactBubble null or self
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:479)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Analyzing contact at (18,9)...
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:494)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP GEOMETRY] Final choice for contact (18,9): (18,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindBestCellForContact (int,int) (at Assets/Scripts/Bubble.cs:619)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:495)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] FindBestCellForContact returned (18,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:496)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Contact with bubble at (18,9) → candidate cell (18,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:503)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Analyzing contact at (17,9)...
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:494)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP GEOMETRY] Final choice for contact (17,9): (17,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindBestCellForContact (int,int) (at Assets/Scripts/Bubble.cs:619)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:495)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] FindBestCellForContact returned (17,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:496)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Contact with bubble at (17,9) → candidate cell (17,9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:503)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP Y-SHAPE] Loop complete. Total candidate cells: 2
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:510)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] candidateCells.Count = 2
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:513)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] After sort: 2 cells
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:519)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] Multi-point analysis selected grid (18, 9) with 1 contact votes
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:525)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] Vote breakdown: (18,9):1, (17,9):1
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:FindNearestGridCoordinates () (at Assets/Scripts/Bubble.cs:528)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:398)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] FindNearestGridCoordinates returned: (18, 9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:399)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

Bubble collision: nearest grid coords (18, 9)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:402)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SNAP] About to snap. gridX=18, gridY=9
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:421)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

Snapping to grid (18, 9), world pos: (13.00, 9.18, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:423)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

Final grid coords: (18, 9), Final world pos: (13.00, 9.18, 0.00)
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:426)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SYNC] WARNING: Grid position (18, 9) already occupied! Existing bubble will be replaced!
UnityEngine.Debug:LogWarning (object)
BubbleShooter.Bubble:SnapToGrid (int,int) (at Assets/Scripts/Bubble.cs:679)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:427)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

[SYNC] Bubble snapped to grid (18, 9) at world pos: (13.00, 9.18, 0.00), offset: 0
UnityEngine.Debug:Log (object)
BubbleShooter.Bubble:SnapToGrid (int,int) (at Assets/Scripts/Bubble.cs:690)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:427)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

OnBubbleStopped called - enabling shooting
UnityEngine.Debug:Log (object)
BubbleShooter.BubbleShooter:OnBubbleStopped () (at Assets/Scripts/BubbleShooter.cs:208)
BubbleShooter.Bubble:OnBubbleCollision (BubbleShooter.Bubble) (at Assets/Scripts/Bubble.cs:442)
BubbleShooter.Bubble:OnTriggerEnter2D (UnityEngine.Collider2D) (at Assets/Scripts/Bubble.cs:348)

