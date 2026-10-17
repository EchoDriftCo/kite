import { ApplicationConfig, ErrorHandler, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import * as Sentry from '@sentry/angular';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { environment } from '../environments/environment';
import { provideServiceWorker } from '@angular/service-worker';
import { guestAccessInterceptor } from './core/guest-access.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes), 
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([guestAccessInterceptor])),
    // Sentry error handling (only if enabled)
    ...(environment.sentry?.enabled ? [
      {
        provide: ErrorHandler,
        useValue: Sentry.createErrorHandler({
          showDialog: false, // We'll trigger feedback manually
        }),
      },
    ] : []),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    }),
  ]
};
