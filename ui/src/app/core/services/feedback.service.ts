import { Injectable } from '@angular/core';
import * as Sentry from '@sentry/angular';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  
  /**
   * Opens Sentry's feedback dialog for users to report bugs
   * @param options Optional configuration for the feedback form
   */
  async openFeedbackDialog(options?: {
    title?: string;
    subtitle?: string;
    name?: string;
    email?: string;
  }): Promise<void> {
    if (!environment.sentry?.enabled || !environment.sentry?.dsn) {
      console.warn('Sentry feedback is not available in this environment');
      return;
    }

    const feedback = Sentry.feedbackIntegration({
      colorScheme: 'system',
      showBranding: false,
      formTitle: options?.title || 'Report a Bug',
      submitButtonLabel: 'Send Report',
      cancelButtonLabel: 'Cancel',
      nameLabel: 'Name',
      namePlaceholder: 'Your name',
      emailLabel: 'Email',
      emailPlaceholder: 'your@email.com',
      messageLabel: 'What happened?',
      messagePlaceholder: 'Describe the issue you encountered...',
      successMessageText: 'Thanks for your feedback! We\'ll look into it.',
    });

    // Create and open the feedback form
    const form = await feedback.createForm();
    form.appendToDom();
    form.open();
  }

  /**
   * Captures a user message/feedback without showing the dialog
   */
  captureFeedback(message: string, context?: Record<string, unknown>): void {
    if (!environment.sentry?.enabled) {
      console.log('Feedback (dev):', message, context);
      return;
    }

    Sentry.captureMessage(message, {
      level: 'info',
      tags: { type: 'user-feedback' },
      extra: context,
    });
  }

  /**
   * Check if feedback feature is enabled
   */
  isEnabled(): boolean {
    return !!(environment.sentry?.enabled && environment.sentry?.dsn);
  }
}
