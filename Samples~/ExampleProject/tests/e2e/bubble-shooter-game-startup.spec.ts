import { test, expect } from '@playwright/test';

test.describe('Bubble Shooter Game Startup', () => {
  test('should load game and initialize properly', async ({ page }) => {
    await page.goto('/');
    
    // Wait for Unity WebGL to fully load
    await page.waitForSelector('canvas#unity-canvas', { timeout: 30000 });
    
    // Wait for the Story Test framework to be ready
    await page.waitForFunction(() => {
      const logs = performance.getEntriesByName('mark');
      return logs.some(log => log.name.includes('Story Test') || log.name.includes('Runner ready'));
    }, { timeout: 15000 });
    
    // Check that the canvas is visible and has proper dimensions
    const canvas = page.locator('canvas#unity-canvas');
    await expect(canvas).toBeVisible();
    
    const canvasBox = await canvas.boundingBox();
    expect(canvasBox?.width).toBeGreaterThan(800);
    expect(canvasBox?.height).toBeGreaterThan(600);
    
    // Check for game UI elements
    await expect(page.locator('[data-testid="score-display"]').or(page.locator('text=Score')).or(page.locator('text=0'))).toBeVisible({ timeout: 10000 });
  });

  test('should display game title and UI elements', async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('canvas#unity-canvas', { timeout: 30000 });
    
    // Wait for game initialization
    await page.waitForTimeout(3000);
    
    // Check that the page title contains bubble shooter reference
    const title = await page.title();
    expect(title.toLowerCase()).toContain('bubble');
    
    // Verify Unity WebGL is running
    const unityInstance = await page.evaluate(() => {
      return typeof (window as any).unityInstance !== 'undefined';
    });
    expect(unityInstance).toBe(true);
  });

  test('should initialize camera and grid setup', async ({ page }) => {
    await page.goto('/');
    await page.waitForSelector('canvas#unity-canvas');
    
    // Listen for specific console messages indicating proper setup
    const setupMessages = [];
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('Camera setup') || 
          text.includes('BubbleShooterGameManager created') ||
          text.includes('Creating BubbleShooter') ||
          text.includes('Boundary colliders created')) {
        setupMessages.push(text);
      }
    });
    
    // Wait for setup messages
    await page.waitForFunction(() => {
      return setupMessages.length >= 2;
    }, { timeout: 15000 });
    
    // Verify essential setup occurred
    expect(setupMessages.some(msg => msg.includes('Camera setup'))).toBe(true);
  });
});