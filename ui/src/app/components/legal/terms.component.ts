import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  template: `
    <div class="legal-page">
      <div class="legal-container">
        <h1>Terms of Service</h1>
        <p class="last-updated">Last Updated: March 28, 2026</p>

        <h2>Section 1: Agreement to Terms</h2>
        <p>By accessing or using RecipeVault at myrecipevault.io (the "Service"), you agree to be bound by these Terms of Service ("Terms"). If you do not agree to these Terms, do not use the Service.</p>

        <h2>Section 2: Description of Service</h2>
        <p>RecipeVault is a web-based recipe management platform that allows you to store, organize, import, create, share, and remix recipes. The Service includes features such as AI-powered recipe import, recipe generation, meal planning, nutrition tracking, cooking mode, and social sharing through circles. RecipeVault is currently in beta. Features, availability, and pricing are subject to change.</p>

        <h2>Section 3: Eligibility</h2>
        <p>You must be at least 13 years old to use RecipeVault. By using the Service, you represent that you meet this requirement and have the legal capacity to enter into these Terms.</p>

        <h2>Section 4: Account Registration</h2>
        <ul>
          <li>You must provide a valid email address to create an account.</li>
          <li>You are responsible for maintaining the security of your account credentials.</li>
          <li>You are responsible for all activity that occurs under your account.</li>
          <li>You must notify us immediately if you suspect unauthorized access to your account.</li>
        </ul>

        <h2>Section 5: Your Content</h2>
        <p><strong>Ownership:</strong> You own your content. Recipes, images, notes, meal plans, and any other content you create or upload to RecipeVault remain your intellectual property. We do not claim ownership of your content.</p>
        <p><strong>License to Us:</strong> By uploading content to RecipeVault, you grant us a limited, non-exclusive, worldwide, royalty-free license to: store, display, and deliver your content back to you; process your content through AI features you initiate; display your content to other users when you choose to share it. This license exists solely to operate the Service and ends when you delete your content or account.</p>
        <p><strong>Recipe Forking:</strong> When you make a recipe public or share it to a circle, other users may "fork" (remix) your recipe, creating their own copy. The fork links back to your original. You retain ownership of your original recipe. The forking user owns their modified version.</p>
        <p><strong>Shared Content:</strong> Content shared to social circles is visible to circle members. Content set to "public" is visible to all RecipeVault users. You control the visibility of your content.</p>

        <h2>Section 6: Acceptable Use</h2>
        <p>You agree not to: use the Service for any illegal purpose; upload content that infringes on the intellectual property rights of others; upload harmful, abusive, threatening, or harassing content; attempt to gain unauthorized access to other users' accounts or data; use automated tools to scrape, crawl, or bulk-download content; interfere with or disrupt the Service; reverse engineer or decompile the Service; use the Service to build a competing product.</p>

        <h2>Section 7: AI Features</h2>
        <p>RecipeVault offers AI-powered features including recipe import from photos/URLs, recipe generation, recipe mixing, and smart substitutions. AI-generated content is provided "as is" and may contain errors. You are responsible for reviewing AI-generated recipes before cooking. We are not liable for adverse outcomes from following AI-generated instructions. AI features may use third-party providers. AI-generated recipes you save become your content.</p>

        <h2>Section 8: Beta Service</h2>
        <p>RecipeVault is currently in beta: No uptime guarantees; Features may change; Data loss is possible (export regularly); No warranty. By using the beta, you acknowledge and accept these risks.</p>

        <h2>Section 9: Premium Subscription (Future)</h2>
        <p>Premium features and pricing are not yet available. Waitlist signups do not constitute a purchase or guarantee of pricing. Free tier features will remain available without payment.</p>

        <h2>Section 10: Intellectual Property</h2>
        <p>The RecipeVault name, logo, design, and software are owned by RecipeVault and protected by applicable intellectual property laws.</p>

        <h2>Section 11: Privacy</h2>
        <p>Your use of RecipeVault is also governed by our <a routerLink="/privacy">Privacy Policy</a>.</p>

        <h2>Section 12: Termination</h2>
        <p><strong>By you:</strong> Delete your account at any time from Settings. <strong>By us:</strong> We may suspend or terminate your account for Terms violations. We will make reasonable efforts to notify you.</p>

        <h2>Section 13: Disclaimer of Warranties</h2>
        <p>THE SERVICE IS PROVIDED "AS IS" AND "AS AVAILABLE" WITHOUT WARRANTIES OF ANY KIND, EXPRESS OR IMPLIED.</p>

        <h2>Section 14: Limitation of Liability</h2>
        <p>TO THE MAXIMUM EXTENT PERMITTED BY LAW, RECIPEVAULT SHALL NOT BE LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR PUNITIVE DAMAGES. TOTAL LIABILITY SHALL NOT EXCEED THE AMOUNT PAID IN THE PRECEDING 12 MONTHS, OR $50, WHICHEVER IS GREATER.</p>

        <h2>Section 15: Indemnification</h2>
        <p>You agree to indemnify and hold harmless RecipeVault from claims arising from your use of the Service, your content, or your violation of these Terms.</p>

        <h2>Section 16: Governing Law</h2>
        <p>These Terms are governed by the laws of the State of Colorado, United States.</p>

        <h2>Section 17: Changes to These Terms</h2>
        <p>We may update these Terms. Continued use constitutes acceptance.</p>

        <h2>Section 18: Contact Us</h2>
        <p>Email: <a href="mailto:legal@myrecipevault.io">legal@myrecipevault.io</a></p>

        <p class="disclaimer">Disclaimer: These terms are provided for informational purposes and do not constitute legal advice.</p>

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
export class TermsComponent {}
