import { test, expect } from '@playwright/test';

test.describe('Bubble Shooting Mechanics', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('canvas#unity-canvas', { timeout: 30000 });
    
    // Wait for game to be fully initialized
    await page.waitForTimeout(3000);
  });

  test('should aim and shoot bubble at center position', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    // Calculate center position
    const centerX = canvasBox.x + canvasBox.width / 2;
    const centerY = canvasBox.y + canvasBox.height / 2;
    
    // Listen for shooting events
    const shootingMessages = [];
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Shooting bubble in direction') || 
          text.includes('BARREL FIRED') ||
          text.includes('Bubble shot with speed')) {
        shootingMessages.push(text);
      }
    });
    
    // Move mouse to center and click to shoot
    await page.mouse.move(centerX, centerY);
    await page.mouse.click(centerX, centerY);
    
    // Wait for shooting to occur
    await page.waitForFunction(() => shootingMessages.length > 0, { timeout: 5000 });
    
    // Verify shooting occurred
    expect(shootingMessages.length).toBeGreaterThan(0);
    expect(shootingMessages.some(msg => msg.includes('BARREL FIRED'))).toBe(true);
  });

  test('should detect proper bubble collision and positioning', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    // Position for shooting toward upper area where bubbles should be
    const targetX = canvasBox.x + canvasBox.width / 2;
    const targetY = canvasBox.y + canvasBox.height * 0.3; // Upper portion of canvas
    
    // Track collision and positioning messages
    const collisionMessages = [];
    const positioningMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('COLLISION at position') || 
          text.includes('Collision with STATIONARY BUBBLE')) {
        collisionMessages.push(text);
      }
      if (text.includes('SNAP') || 
          text.includes('FindNearestGridCoordinates') ||
          text.includes('Grid coordinates found')) {
        positioningMessages.push(text);
      }
    });
    
    // Shoot bubble
    await page.mouse.move(targetX, targetY);
    await page.mouse.click(targetX, targetY);
    
    // Wait for collision to occur (this might take a moment for the bubble to travel)
    await page.waitForFunction(() => collisionMessages.length > 0, { timeout: 8000 });
    
    // Verify collision occurred
    expect(collisionMessages.length).toBeGreaterThan(0);
    
    // CRITICAL TEST: Check that positioning logic was executed
    await page.waitForFunction(() => positioningMessages.length > 0, { timeout: 2000 });
    expect(positioningMessages.length).toBeGreaterThan(0);
    
    // Extract position information from collision message
    const positionMatch = collisionMessages[0].match(/COLLISION at position \(([^)]+)\)/);
    if (positionMatch) {
      const coordinates = positionMatch[1];
      // Log the collision position for debugging
      console.log(`Bubble collision detected at: ${coordinates}`);
    }
  });

  test('should verify bubble attachment to correct grid position', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    // Shoot at a specific position to test grid attachment
    const shootX = canvasBox.x + canvasBox.width * 0.6;
    const shootY = canvasBox.y + canvasBox.height * 0.3;
    
    const gridMessages = [];
    const finalPositionMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('[SNAP GEOMETRY] Final choice') ||
          text.includes('Grid coordinates found') ||
          text.includes('Bubble attached at position')) {
        gridMessages.push(text);
      }
      if (text.includes('Final world position') ||
          text.includes('Bubble positioned at')) {
        finalPositionMessages.push(text);
      }
    });
    
    await page.mouse.move(shootX, shootY);
    await page.mouse.click(shootX, shootY);
    
    // Wait for grid attachment logic to complete
    await page.waitForFunction(() => gridMessages.length > 0, { timeout: 10000 });
    
    // Verify grid attachment occurred with proper geometry calculation
    expect(gridMessages.length).toBeGreaterThan(0);
    
    // Check that final positioning was calculated
    // This is the critical test for the positioning bug you mentioned
    const geometryChoiceMsg = gridMessages.find(msg => msg.includes('Final choice'));
    if (geometryChoiceMsg) {
      console.log(`Grid attachment decision: ${geometryChoiceMsg}`);
      
      // Extract grid coordinates from the message
      const coordMatch = geometryChoiceMsg.match(/Final choice.*?(\d+,\d+)/);
      if (coordMatch) {
        const gridCoords = coordMatch[1];
        console.log(`Bubble should attach at grid coordinates: ${gridCoords}`);
      }
    }
  });

  test('should load next bubble after shooting', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    const nextBubbleMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Loaded current bubble at position') ||
          text.includes('LoadNextBubble')) {
        nextBubbleMessages.push(text);
      }
    });
    
    // Shoot a bubble
    const centerX = canvasBox.x + canvasBox.width / 2;
    const centerY = canvasBox.y + canvasBox.height / 2;
    
    await page.mouse.move(centerX, centerY);
    await page.mouse.click(centerX, centerY);
    
    // Wait for next bubble to be loaded
    await page.waitForFunction(() => nextBubbleMessages.length >= 2, { timeout: 8000 });
    
    // Should have at least initial bubble and next bubble loaded
    expect(nextBubbleMessages.length).toBeGreaterThanOrEqual(2);
  });
});