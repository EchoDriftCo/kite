import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  template: `
    <div class="legal-page">
      <div class="legal-container">
        <h1>Privacy Policy</h1>
        <p class="last-updated">Last Updated: March 28, 2026</p>

        <h2>Section 1: Introduction</h2>
        <p>RecipeVault is committed to protecting your privacy. This Privacy Policy explains how we collect, use, store, and protect your information.</p>

        <h2>Section 2: Information We Collect</h2>
        <p><strong>Information You Provide:</strong> Account Information (email, password); Recipe Data (titles, ingredients, instructions, images, tags, notes); Dietary Information (profiles, restrictions, preferences); Meal Plans and grocery lists; Waitlist Information (email); Feedback.</p>
        <p><strong>Information Collected Automatically:</strong> Usage Data (pages visited, features used, timestamps); Device Information (browser, OS, screen resolution); Log Data (IP address, access times, referring URLs).</p>
        <p><strong>Information We Do NOT Collect:</strong> Payment or credit card information; Precise geolocation data; Data from contacts, camera, or microphone (unless explicitly using photo import).</p>

        <h2>Section 3: How We Use Your Information</h2>
        <p>We use your information to: Provide the Service; Improve the Service; Communicate with You; Ensure Security.</p>
        <p>We do NOT: Train AI models on your personal recipes without consent; Sell or rent your personal information; Send unsolicited marketing emails.</p>

        <h2>Section 4: How We Store and Protect Your Data</h2>
        <p><strong>Hosting:</strong> Fly.io and Supabase. <strong>Database:</strong> PostgreSQL with row-level security. <strong>Encryption:</strong> In transit (HTTPS/TLS) and at rest. <strong>Authentication:</strong> Passwords hashed with industry-standard algorithms. <strong>Access Control:</strong> Only authorized systems and personnel.</p>

        <h2>Section 5: Data Sharing</h2>
        <p>We do not sell your personal information. We may share with: Service Providers (hosting, email, analytics); AI Processing (recipe text to AI providers, no PII); Legal Requirements (if required by law).</p>

        <h2>Section 6: Your Rights</h2>
        <p>Access, Export (JSON), Correct, Delete, Withdraw Consent.</p>
        <p><strong>GDPR (EU/EEA):</strong> Restriction of processing, Object to processing, Data portability, Lodge complaints.</p>
        <p><strong>CCPA (California):</strong> Know what data is collected, Request deletion, Opt out of sale (we don't sell), Non-discrimination.</p>

        <h2>Section 7: Cookies</h2>
        <p>Authentication cookies, Preference cookies (theme), Session cookies. No third-party advertising cookies or tracking pixels.</p>

        <h2>Section 8: Children's Privacy</h2>
        <p>Not intended for children under 13. We do not knowingly collect from children under 13.</p>

        <h2>Section 9: Data Retention</h2>
        <p><strong>Active accounts:</strong> retained while active. <strong>Deleted accounts:</strong> permanently deleted within 30 days. <strong>Waitlist emails:</strong> retained until unsubscribe or premium launch.</p>

        <h2>Section 10: Third-Party Links</h2>
        <p>Not responsible for external sites' privacy practices.</p>

        <h2>Section 11: Changes to This Policy</h2>
        <p>We may update this policy. Significant changes will be communicated.</p>

        <h2>Section 12: Contact Us</h2>
        <p>Email: <a href="mailto:privacy@myrecipevault.io">privacy@myrecipevault.io</a></p>

        <p class="disclaimer">Disclaimer: This privacy policy is provided for informational purposes and does not constitute legal advice.</p>

        <a routerLink="/features" class="back-link">
          <mat-icon>arrow_back</mat-icon>
          Back to home
        </a>
      </div>
    </div>
  `,
  styles: [`
    .legal-page {
      min-height: 100vh;
      background-color: var(--color-bg-primary);
      padding: var(--space-lg) var(--space-md);
    }
    .legal-container {
      max-width: 800px;
      margin: 0 auto;
    }
    h1 {
      font: var(--font-h1);
      color: var(--color-text-primary);
      margin-bottom: var(--space-xs);
    }
    .last-updated {
      font: var(--font-body);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-xl);
    }
    h2 {
      font: var(--font-h3);
      color: var(--color-text-primary);
      margin-top: var(--space-xl);
      margin-bottom: var(--space-md);
    }
    p {
      font: var(--font-body);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-md);
      line-height: 1.7;
    }
    ul {
      padding-left: var(--space-lg);
      margin-bottom: var(--space-md);
    }
    li {
      font: var(--font-body);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-sm);
      line-height: 1.6;
    }
    strong {
      color: var(--color-text-primary);
    }
    a {
      color: var(--color-primary);
      text-decoration: none;
    }
    a:hover {
      text-decoration: underline;
    }
    .disclaimer {
      font: var(--font-body-small);
      color: var(--color-text-hint);
      font-style: italic;
      margin-top: var(--space-xl);
      padding-top: var(--space-lg);
      border-top: 1px solid var(--color-border-default);
    }
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: var(--space-xs);
      margin-top: var(--space-xl);
      margin-bottom: var(--space-xl);
      font: var(--font-body);
      color: var(--color-primary);
      text-decoration: none;
    }
    .back-link:hover {
      text-decoration: underline;
    }
  `]
})
export class PrivacyComponent {}
