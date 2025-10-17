import { Page, Locator } from '@playwright/test';

export class BubbleShooterPage {
  readonly page: Page;
  readonly canvas: Locator;
  
  constructor(page: Page) {
    this.page = page;
    this.canvas = page.locator('canvas#unity-canvas');
  }

  async goto() {
    await this.page.goto('/');
    await this.canvas.waitFor({ timeout: 30000 });
    await this.page.waitForTimeout(3000); // Allow Unity to initialize
  }

  async getCanvasBounds() {
    return await this.canvas.boundingBox();
  }

  async shootAt(screenX: number, screenY: number) {
    await this.page.mouse.move(screenX, screenY);
    await this.page.mouse.click(screenX, screenY);
  }

  async shootAtCenter() {
    const bounds = await this.getCanvasBounds();
    if (!bounds) throw new Error('Canvas bounds not found');
    
    const centerX = bounds.x + bounds.width / 2;
    const centerY = bounds.y + bounds.height / 2;
    
    await this.shootAt(centerX, centerY);
  }

  async shootAtUpperArea(xPercent = 0.5, yPercent = 0.3) {
    const bounds = await this.getCanvasBounds();
    if (!bounds) throw new Error('Canvas bounds not found');
    
    const targetX = bounds.x + bounds.width * xPercent;
    const targetY = bounds.y + bounds.height * yPercent;
    
    await this.shootAt(targetX, targetY);
  }

  async waitForUnityReady() {
    // Wait for Unity WebGL to be fully loaded
    await this.page.waitForFunction(() => {
      return typeof (window as any).unityInstance !== 'undefined';
    }, { timeout: 30000 });
  }

  async waitForGameSetup() {
    // Wait for essential game setup messages
    const setupPromise = this.page.waitForFunction(() => {
      return window.performance.getEntriesByName('unity-setup-complete').length > 0;
    }, { timeout: 15000 }).catch(() => {
      // Fallback: just wait for a reasonable time if no specific marker exists
      return this.page.waitForTimeout(5000);
    });
    
    return setupPromise;
  }

  // Utility method to capture console messages of interest
  captureConsoleMessages(categories: string[] = ['collision', 'position', 'score', 'gamestate']) {
    const messages: Array<{ category: string; text: string; timestamp: number }> = [];
    
    this.page.on('console', msg => {
      const text = msg.text();
      
      for (const category of categories) {
        switch (category) {
          case 'collision':
            if (text.includes('COLLISION') || text.includes('Collision with')) {
              messages.push({ category, text, timestamp: Date.now() });
            }
            break;
          case 'position':
            if (text.includes('[SNAP') || text.includes('position') || text.includes('coordinates')) {
              messages.push({ category, text, timestamp: Date.now() });
            }
            break;
          case 'score':
            if (text.toLowerCase().includes('score') || text.includes('Points')) {
              messages.push({ category, text, timestamp: Date.now() });
            }
            break;
          case 'gamestate':
            if (text.includes('Bubble in flight') || text.includes('Game Over') || text.includes('Bubble attached')) {
              messages.push({ category, text, timestamp: Date.now() });
            }
            break;
        }
      }
    });
    
    return messages;
  }
}