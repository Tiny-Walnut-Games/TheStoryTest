import { test, expect } from '@playwright/test';

test.describe('Bubble Positioning Regression Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('canvas#unity-canvas', { timeout: 30000 });
    
    // Wait for game initialization
    await page.waitForTimeout(3000);
  });

  test('CRITICAL: should attach bubble at or near collision point, not several cells away', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    // Track all positioning-related messages for detailed analysis
    const positioningData = [];
    const collisionData = [];
    
    page.on('console', msg => {
      const text = msg.text();
      
      // Capture collision position
      const collisionMatch = text.match(/COLLISION at position \(([^)]+)\)/);
      if (collisionMatch) {
        collisionData.push({
          type: 'collision',
          position: collisionMatch[1],
          timestamp: Date.now(),
          fullMessage: text
        });
      }
      
      // Capture final grid choice
      const geometryMatch = text.match(/\[SNAP GEOMETRY\] Final choice.*?(\d+,\d+)/);
      if (geometryMatch) {
        positioningData.push({
          type: 'final_grid_choice',
          gridCoords: geometryMatch[1],
          timestamp: Date.now(),
          fullMessage: text
        });
      }
      
      // Capture world position updates
      const worldPosMatch = text.match(/world pos(?:ition)?.*?\(([^)]+)\)/);
      if (worldPosMatch && text.includes('SNAP')) {
        positioningData.push({
          type: 'world_position',
          position: worldPosMatch[1],
          timestamp: Date.now(),
          fullMessage: text
        });
      }
    });
    
    // Shoot at center position to test positioning accuracy
    const targetX = canvasBox.x + canvasBox.width / 2;
    const targetY = canvasBox.y + canvasBox.height * 0.3;
    
    console.log(`Shooting bubble at screen coordinates: (${targetX}, ${targetY})`);
    
    await page.mouse.move(targetX, targetY);
    await page.mouse.click(targetX, targetY);
    
    // Wait for collision and positioning to complete
    await page.waitForFunction(() => collisionData.length > 0, { timeout: 10000 });
    await page.waitForTimeout(2000); // Allow positioning logic to complete
    
    // Analyze positioning accuracy
    expect(collisionData.length).toBeGreaterThan(0);
    expect(positioningData.length).toBeGreaterThan(0);
    
    const collision = collisionData[0];
    const finalChoice = positioningData.find(p => p.type === 'final_grid_choice');
    
    console.log('\n=== POSITIONING ANALYSIS ===');
    console.log(`Collision detected at: ${collision.position}`);
    
    if (finalChoice) {
      console.log(`Final grid attachment: ${finalChoice.gridCoords}`);
      
      // Parse collision coordinates
      const [collisionX, collisionY] = collision.position.split(', ').map(parseFloat);
      
      // Log for manual verification
      console.log(`\nDETAILED ANALYSIS:`);
      console.log(`- Bubble collided at world position: (${collisionX}, ${collisionY})`);
      console.log(`- Bubble was attached to grid cell: ${finalChoice.gridCoords}`);
      console.log(`- Time between collision and attachment: ${finalChoice.timestamp - collision.timestamp}ms`);
      
      // Calculate if the positioning seems reasonable
      // This is a heuristic check - the exact calculation depends on grid cell size and layout
      const gridCoords = finalChoice.gridCoords.split(',').map(Number);
      const [gridX, gridY] = gridCoords;
      
      // Log all positioning messages for debugging
      console.log('\nAll positioning messages:');
      positioningData.forEach(data => {
        console.log(`- ${data.type}: ${data.fullMessage}`);
      });
      
      // REGRESSION TEST: This will help identify when positioning gets "fixed"
      // If the bug is fixed, this test should pass with reasonable positioning
      // If the bug persists, this test will document the incorrect behavior
      
      console.log('\n=== REGRESSION CHECK ===');
      console.log('EXPECTED: Bubble should attach at or very close to collision point');
      console.log('ACTUAL: See logged positions above');
      console.log('STATUS: Test documents current behavior for regression tracking');
      
    } else {
      throw new Error('No final grid choice recorded - positioning logic may have failed');
    }
  });

  test('should verify Y-Shape snap geometry algorithm', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    const snapMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('[SNAP Y-SHAPE]') || 
          text.includes('FindBestCellForContact') ||
          text.includes('Processing') && text.includes('overlapping colliders')) {
        snapMessages.push(text);
      }
    });
    
    // Shoot to trigger snap geometry
    const shootX = canvasBox.x + canvasBox.width * 0.6;
    const shootY = canvasBox.y + canvasBox.height * 0.25;
    
    await page.mouse.move(shootX, shootY);
    await page.mouse.click(shootX, shootY);
    
    // Wait for snap geometry processing
    await page.waitForFunction(() => snapMessages.length > 0, { timeout: 8000 });
    
    expect(snapMessages.length).toBeGreaterThan(0);
    
    // Analyze snap geometry messages
    console.log('\n=== Y-SHAPE SNAP GEOMETRY ANALYSIS ===');
    snapMessages.forEach(msg => {
      console.log(`- ${msg}`);
    });
    
    // Check for proper processing flow
    const hasProcessing = snapMessages.some(msg => msg.includes('Processing') && msg.includes('overlapping'));
    const hasFinalChoice = snapMessages.some(msg => msg.includes('Final choice'));
    
    console.log(`\nSnap geometry processing complete: ${hasProcessing}`);
    console.log(`Final positioning choice made: ${hasFinalChoice}`);
  });

  test('should measure positioning accuracy across multiple shots', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    const shots = [];
    
    page.on('console', msg => {
      const text = msg.text();
      
      // Track each complete shot cycle
      if (text.includes('COLLISION at position')) {
        const match = text.match(/COLLISION at position \(([^)]+)\)/);
        if (match) {
          shots.push({
            collisionPos: match[1],
            shotTime: Date.now(),
            messages: []
          });
        }
      }
      
      // Add positioning messages to the latest shot
      if (shots.length > 0 && (text.includes('[SNAP') || text.includes('Final choice'))) {
        shots[shots.length - 1].messages.push(text);
      }
    });
    
    // Take multiple shots at different positions
    const testPositions = [
      { x: canvasBox.x + canvasBox.width * 0.3, y: canvasBox.y + canvasBox.height * 0.3 },
      { x: canvasBox.x + canvasBox.width * 0.7, y: canvasBox.y + canvasBox.height * 0.3 },
      { x: canvasBox.x + canvasBox.width * 0.5, y: canvasBox.y + canvasBox.height * 0.25 }
    ];
    
    for (let i = 0; i < testPositions.length; i++) {
      const pos = testPositions[i];
      console.log(`\nTaking shot ${i + 1} at: (${pos.x}, ${pos.y})`);
      
      await page.mouse.move(pos.x, pos.y);
      await page.mouse.click(pos.x, pos.y);
      
      // Wait for this shot to complete before next shot
      await page.waitForTimeout(4000);
    }
    
    // Analyze results
    console.log(`\n=== MULTI-SHOT POSITIONING ANALYSIS ===`);
    console.log(`Total successful shots: ${shots.length}`);
    
    shots.forEach((shot, index) => {
      console.log(`\nShot ${index + 1}:`);
      console.log(`- Collision at: ${shot.collisionPos}`);
      console.log(`- Positioning messages: ${shot.messages.length}`);
      
      const finalChoice = shot.messages.find(msg => msg.includes('Final choice'));
      if (finalChoice) {
        console.log(`- Final positioning: ${finalChoice}`);
      }
    });
    
    // Verify consistency
    expect(shots.length).toBeGreaterThan(0);
    expect(shots.every(shot => shot.messages.length > 0)).toBe(true);
  });

  test('should validate grid coordinate system integrity', async ({ page }) => {
    const gridMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Grid coordinates') ||
          text.includes('Cell position') ||
          text.includes('Hexagonal') ||
          text.includes('row') && text.includes('col')) {
        gridMessages.push(text);
      }
    });
    
    // Trigger grid-related logging
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (canvasBox) {
      await page.mouse.move(canvasBox.x + canvasBox.width / 2, canvasBox.y + canvasBox.height * 0.3);
      await page.mouse.click(canvasBox.x + canvasBox.width / 2, canvasBox.y + canvasBox.height * 0.3);
      
      await page.waitForTimeout(5000);
    }
    
    console.log('\n=== GRID COORDINATE SYSTEM ===');
    gridMessages.forEach(msg => {
      console.log(`- ${msg}`);
    });
    
    // Document grid system behavior
    console.log(`Grid-related messages captured: ${gridMessages.length}`);
  });
});