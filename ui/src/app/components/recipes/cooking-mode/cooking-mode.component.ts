import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { CookingService } from '../../../services/cooking.service';
import { RecipeService } from '../../../services/recipe.service';
import { Recipe } from '../../../models/recipe.model';
import { CookingData, CookingStep, ActiveTimer } from '../../../models/cooking-mode.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-cooking-mode',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatCardModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './cooking-mode.component.html',
  styleUrls: ['./cooking-mode.component.scss']
})
export class CookingModeComponent implements OnInit, OnDestroy {
  recipe?: Recipe;
  cookingData?: CookingData;
  currentStepIndex = 0;
  ingredientChecklist: boolean[] = [];
  isLoading = true;
  error: string | null = null;
  
  isVoiceEnabled = false;
  isListening = false;
  wakeLock: any = null;
  activeTimers: ActiveTimer[] = [];

  readonly voiceCommandExamples: string[] = [
    '"Next step"',
    '"Previous step"',
    '"Go to step 4"',
    '"Read step"',
    '"Start timer"',
    '"Pause timer" / "Resume timer" / "Reset timer"',
    '"What step am I on?"',
    '"Ingredients"'
  ];
  
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private cookingService: CookingService
  ) {}

  ngOnInit(): void {
    const recipeId = this.route.snapshot.paramMap.get('id');
    if (!recipeId) {
      this.router.navigate(['/']);
      return;
    }

    this.loadRecipe(recipeId);
    this.loadCookingData(recipeId);
    this.requestWakeLock();
    this.requestNotificationPermission();

    // Subscribe to active timers
    this.cookingService.getActiveTimers()
      .pipe(takeUntil(this.destroy$))
      .subscribe(timers => {
        this.activeTimers = timers;
      });

    // Subscribe to listening state
    this.cookingService.getIsListening()
      .pipe(takeUntil(this.destroy$))
      .subscribe(listening => {
        this.isListening = listening;
      });
  }

  ngOnDestroy(): void {
    this.stopVoiceControl();
    this.releaseWakeLock();
    this.cookingService.clearAllTimers();
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadRecipe(id: string): void {
    this.recipeService.getRecipe(id).subscribe({
      next: recipe => {
        this.recipe = recipe;
        this.ingredientChecklist = new Array(recipe.ingredients.length).fill(false);
      },
      error: err => {
        console.error('Failed to load recipe:', err);
        this.error = 'Failed to load recipe';
        this.isLoading = false;
      }
    });
  }

  private loadCookingData(id: string): void {
    this.cookingService.getCookingData(id).subscribe({
      next: data => {
        this.cookingData = data;
        this.isLoading = false;
      },
      error: err => {
        console.error('Failed to load cooking data:', err);
        this.error = 'Failed to load cooking instructions';
        this.isLoading = false;
      }
    });
  }

  get currentStep(): CookingStep | undefined {
    return this.cookingData?.steps[this.currentStepIndex];
  }

  get totalSteps(): number {
    return this.cookingData?.steps.length || 0;
  }

  get stepTimers(): any[] {
    if (!this.currentStep || !this.cookingData) return [];
    return this.currentStep.timerIndexes
      .map(idx => this.cookingData!.timers.find(t => t.index === idx))
      .filter(t => t !== undefined);
  }

  nextStep(): void {
    if (this.currentStepIndex < this.totalSteps - 1) {
      this.currentStepIndex++;
      this.cookingService.stopSpeaking();
    }
  }

  previousStep(): void {
    if (this.currentStepIndex > 0) {
      this.currentStepIndex--;
      this.cookingService.stopSpeaking();
    }
  }

  readStep(): void {
    if (this.currentStep) {
      this.cookingService.speak(this.currentStep.instruction);
    }
  }

  toggleVoiceControl(): void {
    if (this.isVoiceEnabled) {
      this.stopVoiceControl();
    } else {
      this.startVoiceControl();
    }
  }

  private startVoiceControl(): void {
    this.isVoiceEnabled = true;
    this.cookingService.startListening((command) => this.handleVoiceCommand(command));
  }

  private stopVoiceControl(): void {
    this.isVoiceEnabled = false;
    this.cookingService.stopListening();
  }

  private handleVoiceCommand(command: string): void {
    console.log('Processing voice command:', command);

    const normalized = command.toLowerCase();

    if (this.tryHandleStepNavigation(normalized)) {
      return;
    }

    if (normalized.includes('read') || normalized.includes('repeat')) {
      this.readStep();
      return;
    }

    if (normalized.includes('pause') && normalized.includes('timer')) {
      this.pauseAllTimers();
      return;
    }

    if ((normalized.includes('resume') || normalized.includes('continue')) && normalized.includes('timer')) {
      this.resumeAllTimers();
      return;
    }

    if (normalized.includes('reset') && normalized.includes('timer')) {
      this.resetAllTimers();
      return;
    }

    if (normalized.includes('timer') || normalized.includes('start')) {
      this.startCurrentStepTimer();
      return;
    }

    if (normalized.includes('what step') || normalized.includes('which step')) {
      this.cookingService.speak(`Step ${this.currentStepIndex + 1} of ${this.totalSteps}`);
      return;
    }

    if (normalized.includes('ingredients')) {
      this.cookingService.speak(`Recipe uses ${this.recipe?.ingredients.length ?? 0} ingredients`);
      return;
    }

    this.cookingService.speak('Sorry, I did not catch that command. Try saying next step, read step, or start timer.');
  }

  private tryHandleStepNavigation(command: string): boolean {
    if (command.includes('next')) {
      this.nextStep();
      this.cookingService.speak(`Step ${this.currentStepIndex + 1}`);
      return true;
    }

    if (command.includes('previous') || command.includes('back')) {
      this.previousStep();
      this.cookingService.speak(`Step ${this.currentStepIndex + 1}`);
      return true;
    }

    const stepMatch = command.match(/(?:go to|jump to|step)\s+(\d+)/);
    if (!stepMatch) {
      return false;
    }

    const requestedStep = Number.parseInt(stepMatch[1], 10);
    if (Number.isNaN(requestedStep)) {
      return false;
    }

    if (requestedStep < 1 || requestedStep > this.totalSteps) {
      this.cookingService.speak(`That recipe has ${this.totalSteps} steps.`);
      return true;
    }

    this.currentStepIndex = requestedStep - 1;
    this.cookingService.stopSpeaking();
    this.cookingService.speak(`Step ${requestedStep}`);
    return true;
  }

  private startCurrentStepTimer(): void {
    const timers = this.stepTimers;
    if (timers.length === 0) {
      this.cookingService.speak('No timer found for this step.');
      return;
    }

    this.cookingService.startTimer(timers[0]);
    this.cookingService.speak(`Timer started: ${timers[0].label}`);
  }

  private pauseAllTimers(): void {
    const runningTimers = this.activeTimers.filter(timer => timer.isRunning);
    if (runningTimers.length === 0) {
      this.cookingService.speak('No running timers to pause.');
      return;
    }

    runningTimers.forEach(timer => this.cookingService.pauseTimer(timer.timer.index));
    this.cookingService.speak(`Paused ${runningTimers.length} timer${runningTimers.length > 1 ? 's' : ''}.`);
  }

  private resumeAllTimers(): void {
    const pausedTimers = this.activeTimers.filter(timer => timer.isPaused);
    if (pausedTimers.length === 0) {
      this.cookingService.speak('No paused timers to resume.');
      return;
    }

    pausedTimers.forEach(timer => this.cookingService.startTimer(timer.timer));
    this.cookingService.speak(`Resumed ${pausedTimers.length} timer${pausedTimers.length > 1 ? 's' : ''}.`);
  }

  private resetAllTimers(): void {
    if (this.activeTimers.length === 0) {
      this.cookingService.speak('No active timers to reset.');
      return;
    }

    this.activeTimers.forEach(timer => this.cookingService.resetTimer(timer.timer.index));
    this.cookingService.speak(`Reset ${this.activeTimers.length} timer${this.activeTimers.length > 1 ? 's' : ''}.`);
  }

  startTimer(timer: any): void {
    this.cookingService.startTimer(timer);
  }

  pauseTimer(timer: ActiveTimer): void {
    this.cookingService.pauseTimer(timer.timer.index);
  }

  resetTimer(timer: ActiveTimer): void {
    this.cookingService.resetTimer(timer.timer.index);
  }

  resumeTimer(timer: ActiveTimer): void {
    this.cookingService.startTimer(timer.timer);
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  exitCookingMode(): void {
    if (this.recipe) {
      this.router.navigate(['/recipes', this.recipe.recipeResourceId]);
    } else {
      this.router.navigate(['/']);
    }
  }

  private async requestWakeLock(): Promise<void> {
    this.wakeLock = await this.cookingService.requestWakeLock();
  }

  private releaseWakeLock(): void {
    if (this.wakeLock) {
      this.cookingService.releaseWakeLock(this.wakeLock);
      this.wakeLock = null;
    }
  }

  private requestNotificationPermission(): void {
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }
  }
}
