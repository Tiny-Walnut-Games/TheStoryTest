import { Page } from '@playwright/test';

export class UnityTestHelpers {
  static async waitForUnityLoad(page: Page, timeout = 30000) {
    await page.waitForSelector('canvas#unity-canvas', { timeout });
    
    // Wait for Unity instance to be available
    await page.waitForFunction(() => {
      return typeof (window as any).unityInstance !== 'undefined';
    }, { timeout });
    
    // Additional wait for game initialization
    await page.waitForTimeout(3000);
  }

  static async getUnityCanvasBounds(page: Page) {
    const canvas = page.locator('canvas#unity-canvas');
    const bounds = await canvas.boundingBox();
    
    if (!bounds) {
      throw new Error('Unity canvas not found or not visible');
    }
    
    return bounds;
  }

  static async shootBubbleAtPosition(page: Page, screenX: number, screenY: number) {
    await page.mouse.move(screenX, screenY);
    await page.mouse.click(screenX, screenY);
  }

  static async shootBubbleAtPercent(page: Page, xPercent: number, yPercent: number) {
    const bounds = await this.getUnityCanvasBounds(page);
    const targetX = bounds.x + bounds.width * xPercent;
    const targetY = bounds.y + bounds.height * yPercent;
    
    await this.shootBubbleAtPosition(page, targetX, targetY);
  }

  static createConsoleMessageCollector(page: Page) {
    const messages: Array<{ text: string; timestamp: number; type: string }> = [];
    
    page.on('console', msg => {
      const text = msg.text();
      let type = 'general';
      
      // Categorize messages
      if (text.includes('COLLISION') || text.includes('Collision with')) {
        type = 'collision';
      } else if (text.includes('[SNAP') || text.includes('Grid coordinates') || text.includes('position')) {
        type = 'positioning';
      } else if (text.includes('BARREL FIRED') || text.includes('Shooting bubble')) {
        type = 'shooting';
      } else if (text.includes('Game Over') || text.includes('Bubble in flight')) {
        type = 'gamestate';
      } else if (text.toLowerCase().includes('score') || text.includes('Points')) {
        type = 'scoring';
      }
      
      messages.push({
        text,
        timestamp: Date.now(),
        type
      });
    });
    
    return {
      messages,
      getByType: (type: string) => messages.filter(m => m.type === type),
      getAfterTime: (timestamp: number) => messages.filter(m => m.timestamp > timestamp),
      clear: () => messages.length = 0
    };
  }

  static async waitForConsoleMessage(page: Page, messagePattern: string | RegExp, timeout = 5000) {
    return new Promise<string>((resolve, reject) => {
      const timer = setTimeout(() => {
        reject(new Error(`Console message matching "${messagePattern}" not found within ${timeout}ms`));
      }, timeout);
      
      const handler = (msg: any) => {
        const text = msg.text();
        const matches = typeof messagePattern === 'string' 
          ? text.includes(messagePattern)
          : messagePattern.test(text);
          
        if (matches) {
          clearTimeout(timer);
          page.off('console', handler);
          resolve(text);
        }
      };
      
      page.on('console', handler);
    });
  }

  static async performMultipleShots(page: Page, positions: Array<{x: number, y: number}>, delayBetweenShots = 2000) {
    const results = [];
    
    for (let i = 0; i < positions.length; i++) {
      const pos = positions[i];
      const startTime = Date.now();
      
      await this.shootBubbleAtPosition(page, pos.x, pos.y);
      
      // Wait for shot to complete
      await page.waitForTimeout(delayBetweenShots);
      
      results.push({
        shotIndex: i,
        position: pos,
        timestamp: startTime
      });
    }
    
    return results;
  }

  static async analyzePositioningAccuracy(
    collisionPosition: string, 
    finalGridCoords: string,
    gridCellSize: number = 1.732 // Approximate hexagonal grid cell size
  ) {
    // Parse collision coordinates
    const [colX, colY] = collisionPosition.split(', ').map(parseFloat);
    
    // Parse grid coordinates  
    const [gridCol, gridRow] = finalGridCoords.split(',').map(Number);
    
    // This is a simplified analysis - actual grid positioning depends on the specific
    // hexagonal grid implementation and world-to-grid coordinate conversion
    
    return {
      collisionWorldPos: { x: colX, y: colY },
      finalGridCell: { col: gridCol, row: gridRow },
      analysis: {
        collisionX: colX,
        collisionY: colY,
        gridCol: gridCol,
        gridRow: gridRow,
        // Note: Accurate distance calculation would require the actual grid coordinate system
        estimatedCellDistance: Math.abs(gridCol - Math.round(colX / gridCellSize)),
        notes: 'Actual positioning accuracy depends on Unity game coordinate system'
      }
    };
  }

  static generateTestPositions(canvasBounds: { x: number, y: number, width: number, height: number }) {
    return [
      // Center shots
      { x: canvasBounds.x + canvasBounds.width * 0.5, y: canvasBounds.y + canvasBounds.height * 0.3, name: 'center' },
      { x: canvasBounds.x + canvasBounds.width * 0.5, y: canvasBounds.y + canvasBounds.height * 0.4, name: 'center-lower' },
      
      // Left side shots
      { x: canvasBounds.x + canvasBounds.width * 0.3, y: canvasBounds.y + canvasBounds.height * 0.3, name: 'left' },
      { x: canvasBounds.x + canvasBounds.width * 0.2, y: canvasBounds.y + canvasBounds.height * 0.4, name: 'far-left' },
      
      // Right side shots
      { x: canvasBounds.x + canvasBounds.width * 0.7, y: canvasBounds.y + canvasBounds.height * 0.3, name: 'right' },
      { x: canvasBounds.x + canvasBounds.width * 0.8, y: canvasBounds.y + canvasBounds.height * 0.4, name: 'far-right' },
      
      // Upper shots (for potential early collision)
      { x: canvasBounds.x + canvasBounds.width * 0.5, y: canvasBounds.y + canvasBounds.height * 0.2, name: 'high-center' },
      { x: canvasBounds.x + canvasBounds.width * 0.4, y: canvasBounds.y + canvasBounds.height * 0.25, name: 'high-left' },
      { x: canvasBounds.x + canvasBounds.width * 0.6, y: canvasBounds.y + canvasBounds.height * 0.25, name: 'high-right' }
    ];
  }
}