export interface CookingData {
  recipeResourceId: string;
  steps: CookingStep[];
  timers: Timer[];
  temperatures: Temperature[];
}

export interface CookingStep {
  stepNumber: number;
  instruction: string;
  timerIndexes: number[];
}

export interface Timer {
  index: number;
  label: string;
  seconds: number;
  stepNumber: number;
}

export interface Temperature {
  stepNumber: number;
  value: number;
  unit: string; // 'F' or 'C'
  context: string;
}

export interface ActiveTimer {
  timer: Timer;
  remainingSeconds: number;
  isRunning: boolean;
  isPaused: boolean;
}
