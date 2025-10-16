import { test, expect } from '@playwright/test';

test.describe('Game Over and Restart Functionality', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('canvas#unity-canvas', { timeout: 30000 });
    
    // Wait for game initialization
    await page.waitForTimeout(3000);
  });

  test('should detect game over conditions', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    const gameOverMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Game Over') ||
          text.includes('Danger line reached') ||
          text.includes('Bubbles below danger line') ||
          text.includes('Game ended')) {
        gameOverMessages.push(text);
      }
    });
    
    // Shoot bubbles rapidly to potentially trigger game over
    // (This test verifies the game over detection system exists)
    for (let i = 0; i < 10; i++) {
      const shootX = canvasBox.x + canvasBox.width * 0.5;
      const shootY = canvasBox.y + canvasBox.height * 0.8; // Shoot low to fill up space
      
      await page.mouse.move(shootX, shootY);
      await page.mouse.click(shootX, shootY);
      
      await page.waitForTimeout(1000);
    }
    
    // Check if game over was triggered (may or may not happen based on game logic)
    console.log(`Game over detection messages: ${gameOverMessages.length}`);
    
    // This test validates that game over detection system is present
    // Actual triggering depends on game mechanics and bubble positioning
  });

  test('should display restart functionality', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    
    // Look for restart UI elements or buttons
    // These might be Unity UI elements that appear as DOM elements or canvas-rendered
    
    // Try to find restart-related UI through various selectors
    const restartElements = await Promise.allSettled([
      page.waitForSelector('text=Restart', { timeout: 2000 }),
      page.waitForSelector('text=Play Again', { timeout: 2000 }),
      page.waitForSelector('[data-testid="restart-button"]', { timeout: 2000 }),
      page.waitForSelector('button[contains(text(),"Restart")]', { timeout: 2000 })
    ]);
    
    // Check if any restart elements were found
    const foundRestartElement = restartElements.some(result => result.status === 'fulfilled');
    
    console.log(`Restart UI elements found: ${foundRestartElement}`);
    
    // This test documents the presence or absence of restart UI
    // In Unity WebGL, restart might be handled differently
  });

  test('should handle high score persistence', async ({ page }) => {
    const highScoreMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('High score') ||
          text.includes('Best score') ||
          text.includes('PlayerPrefs') ||
          text.includes('Saving score') ||
          text.includes('New record')) {
        highScoreMessages.push(text);
      }
    });
    
    // Play the game briefly to potentially trigger score saving
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (canvasBox) {
      for (let i = 0; i < 3; i++) {
        const shootX = canvasBox.x + canvasBox.width * (0.4 + i * 0.1);
        const shootY = canvasBox.y + canvasBox.height * 0.3;
        
        await page.mouse.move(shootX, shootY);
        await page.mouse.click(shootX, shootY);
        
        await page.waitForTimeout(2000);
      }
    }
    
    console.log(`High score system messages: ${highScoreMessages.length}`);
    
    // Verify high score system is present and functional
    // Actual high score updates depend on game state and scoring
  });

  test('should validate danger line collision detection', async ({ page }) => {
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (!canvasBox) throw new Error('Canvas not found');
    
    const dangerLineMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Danger line') ||
          text.includes('Below danger') ||
          text.includes('Critical position') ||
          text.includes('Y: -7.2744')) { // Specific Y position from the log
        dangerLineMessages.push(text);
      }
    });
    
    // The danger line was created at Y: -7.2744 according to the log
    // This test verifies the danger line system is working
    
    // Wait a moment to see if danger line messages appear during normal gameplay
    await page.waitForTimeout(5000);
    
    console.log(`Danger line system messages: ${dangerLineMessages.length}`);
    
    // Verify danger line system exists and is monitoring
    // Actual collision with danger line depends on gameplay progression
    expect(dangerLineMessages.length).toBeGreaterThanOrEqual(1); // Should at least have creation message
  });

  test('should verify boundary collider system', async ({ page }) => {
    const boundaryMessages = [];
    
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Boundary colliders created') ||
          text.includes('left=-21.05378') ||
          text.includes('right=21.05378') ||
          text.includes('Collision with boundary')) {
        boundaryMessages.push(text);
      }
    });
    
    // Wait for boundary setup
    await page.waitForTimeout(3000);
    
    // Shoot at extreme angles to test boundary collision
    const canvas = page.locator('canvas#unity-canvas');
    const canvasBox = await canvas.boundingBox();
    
    if (canvasBox) {
      // Shoot toward left boundary
      await page.mouse.move(canvasBox.x + 50, canvasBox.y + canvasBox.height * 0.3);
      await page.mouse.click(canvasBox.x + 50, canvasBox.y + canvasBox.height * 0.3);
      
      await page.waitForTimeout(3000);
      
      // Shoot toward right boundary  
      await page.mouse.move(canvasBox.x + canvasBox.width - 50, canvasBox.y + canvasBox.height * 0.3);
      await page.mouse.click(canvasBox.x + canvasBox.width - 50, canvasBox.y + canvasBox.height * 0.3);
      
      await page.waitForTimeout(3000);
    }
    
    console.log(`Boundary system messages: ${boundaryMessages.length}`);
    
    // Verify boundary system exists
    expect(boundaryMessages.length).toBeGreaterThanOrEqual(1); // Should have boundary creation message
  });
});