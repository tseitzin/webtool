import type { 
    ChartOptions as ChartJsOptions,
    FontSpec
  } from 'chart.js'
  
  // Scale types
  export interface ScaleTickOptions {
    callback?: (value: number) => string;
    font?: Partial<FontSpec>;
    maxRotation?: number;
    minRotation?: number;
  }
  
  export interface ScaleConfig {
    ticks?: ScaleTickOptions;
    title?: {
      display?: boolean;
      text?: string;
    };
    grid?: {
      display?: boolean;
      color?: string;
    };
    beginAtZero?: boolean;
  }
  
  // Chart scales
  export interface ChartScales {
    x: ScaleConfig;
    y: ScaleConfig;
  }
  
  // Plugin types
  export interface ChartPlugins {
    legend?: {
      position?: 'top' | 'left' | 'bottom' | 'right';
      align?: 'start' | 'center' | 'end';
      labels?: {
        boxWidth?: number;
        padding?: number;
        font?: Partial<FontSpec>;
      };
    };
    title?: {
      display?: boolean;
      text?: string;
      font?: {
        size?: number;
        weight?: 'normal' | 'bold' | 'lighter' | 'bolder';
        family?: string;
      };
      padding?: {
        top?: number;
        bottom?: number;
      };
    };
    tooltip?: {
      callbacks?: {
        label?: (context: any) => string;
      };
    };
  }
  
  // Main chart options type
  export interface ChartOptions extends Omit<ChartJsOptions<'line'>, 'scales' | 'plugins'> {
    scales?: ChartScales;
    plugins?: ChartPlugins;
    responsive?: boolean;
    maintainAspectRatio?: boolean;
  }