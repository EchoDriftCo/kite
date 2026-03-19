import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, interval } from 'rxjs';
import { ApiService } from './api.service';
import { CookingData, ActiveTimer, Timer } from '../models/cooking-mode.model';

@Injectable({
  providedIn: 'root'
})
export class CookingService {
  private readonly endpoint = 'recipes';
  
  // Active timers
  private activeTimers$ = new BehaviorSubject<ActiveTimer[]>([]);
  private timerSubscriptions = new Map<number, any>();
  
  // Voice recognition
  private recognition: any;
  private isListening$ = new BehaviorSubject<boolean>(false);

  constructor(private api: ApiService) {
    this.initVoiceRecognition();
  }

  /**
   * Get cooking data (parsed steps, timers, temperatures)
   */
  getCookingData(recipeId: string): Observable<CookingData> {
    return this.api.post<CookingData>(`${this.endpoint}/${recipeId}/cooking-data`, {});
  }

  /**
   * Initialize Web Speech API
   */
  private initVoiceRecognition(): void {
    if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
      const SpeechRecognition = (window as any).webkitSpeechRecognition || (window as any).SpeechRecognition;
      this.recognition = new SpeechRecognition();
      this.recognition.continuous = true;
      this.recognition.interimResults = false;
      this.recognition.lang = 'en-US';
    }
  }

  /**
   * Start listening for voice commands
   */
  startListening(commandHandler: (command: string) => void): void {
    if (!this.recognition) {
      console.warn('Speech recognition not supported');
      return;
    }

    this.recognition.onresult = (event: any) => {
      const transcript = event.results[event.results.length - 1][0].transcript.toLowerCase().trim();
      console.log('Voice command:', transcript);
      commandHandler(transcript);
    };

    this.recognition.onerror = (event: any) => {
      console.error('Speech recognition error:', event.error);
      if (event.error === 'not-allowed') {
        this.isListening$.next(false);
      }
    };

    this.recognition.onend = () => {
      // Auto-restart if still supposed to be listening
      if (this.isListening$.value) {
        this.recognition.start();
      }
    };

    try {
      this.recognition.start();
      this.isListening$.next(true);
    } catch (error) {
      console.error('Failed to start recognition:', error);
    }
  }

  /**
   * Stop listening for voice commands
   */
  stopListening(): void {
    if (this.recognition) {
      this.isListening$.next(false);
      this.recognition.stop();
    }
  }

  /**
   * Get listening state
   */
  getIsListening(): Observable<boolean> {
    return this.isListening$.asObservable();
  }

  /**
   * Speak text using Text-to-Speech
   */
  speak(text: string): void {
    if ('speechSynthesis' in window) {
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.rate = 0.9; // Slightly slower for clarity
      utterance.pitch = 1;
      speechSynthesis.speak(utterance);
    }
  }

  /**
   * Stop speaking
   */
  stopSpeaking(): void {
    if ('speechSynthesis' in window) {
      speechSynthesis.cancel();
    }
  }

  /**
   * Start a timer
   */
  startTimer(timer: Timer): void {
    const existing = this.activeTimers$.value.find(t => t.timer.index === timer.index);
    if (existing) {
      if (existing.isPaused) {
        // Resume
        existing.isPaused = false;
        existing.isRunning = true;
        this.startTimerCountdown(timer.index);
      }
      return;
    }

    const activeTimer: ActiveTimer = {
      timer,
      remainingSeconds: timer.seconds,
      isRunning: true,
      isPaused: false
    };

    this.activeTimers$.next([...this.activeTimers$.value, activeTimer]);
    this.startTimerCountdown(timer.index);
  }

  /**
   * Pause a timer
   */
  pauseTimer(index: number): void {
    const timers = this.activeTimers$.value;
    const timer = timers.find(t => t.timer.index === index);
    if (timer) {
      timer.isRunning = false;
      timer.isPaused = true;
      this.stopTimerCountdown(index);
      this.activeTimers$.next([...timers]);
    }
  }

  /**
   * Reset a timer
   */
  resetTimer(index: number): void {
    const timers = this.activeTimers$.value;
    const timer = timers.find(t => t.timer.index === index);
    if (timer) {
      timer.remainingSeconds = timer.timer.seconds;
      timer.isRunning = false;
      timer.isPaused = false;
      this.stopTimerCountdown(index);
      this.activeTimers$.next([...timers]);
    }
  }

  /**
   * Remove a timer
   */
  removeTimer(index: number): void {
    this.stopTimerCountdown(index);
    const timers = this.activeTimers$.value.filter(t => t.timer.index !== index);
    this.activeTimers$.next(timers);
  }

  /**
   * Get active timers
   */
  getActiveTimers(): Observable<ActiveTimer[]> {
    return this.activeTimers$.asObservable();
  }

  /**
   * Clear all timers
   */
  clearAllTimers(): void {
    this.timerSubscriptions.forEach((sub, index) => {
      sub.unsubscribe();
    });
    this.timerSubscriptions.clear();
    this.activeTimers$.next([]);
  }

  /**
   * Internal: Start countdown for a timer
   */
  private startTimerCountdown(index: number): void {
    if (this.timerSubscriptions.has(index)) {
      return; // Already running
    }

    const sub = interval(1000).subscribe(() => {
      const timers = this.activeTimers$.value;
      const timer = timers.find(t => t.timer.index === index);
      
      if (!timer || !timer.isRunning) {
        this.stopTimerCountdown(index);
        return;
      }

      timer.remainingSeconds--;

      if (timer.remainingSeconds <= 0) {
        timer.remainingSeconds = 0;
        timer.isRunning = false;
        this.stopTimerCountdown(index);
        this.onTimerComplete(timer);
      }

      this.activeTimers$.next([...timers]);
    });

    this.timerSubscriptions.set(index, sub);
  }

  /**
   * Internal: Stop countdown for a timer
   */
  private stopTimerCountdown(index: number): void {
    const sub = this.timerSubscriptions.get(index);
    if (sub) {
      sub.unsubscribe();
      this.timerSubscriptions.delete(index);
    }
  }

  /**
   * Internal: Handle timer completion
   */
  private onTimerComplete(timer: ActiveTimer): void {
    // Play sound (browser notification)
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('Timer Complete!', {
        body: timer.timer.label,
        icon: '/favicon.ico'
      });
    }

    // Speak alert
    this.speak(`Timer complete: ${timer.timer.label}`);

    // Remove from active timers after a delay
    setTimeout(() => {
      this.removeTimer(timer.timer.index);
    }, 5000);
  }

  /**
   * Request Wake Lock to keep screen awake
   */
  async requestWakeLock(): Promise<any> {
    if ('wakeLock' in navigator) {
      try {
        return await (navigator as any).wakeLock.request('screen');
      } catch (err) {
        console.error('Wake lock error:', err);
        return null;
      }
    }
    return null;
  }

  /**
   * Release Wake Lock
   */
  releaseWakeLock(wakeLock: any): void {
    if (wakeLock) {
      wakeLock.release();
    }
  }
}
