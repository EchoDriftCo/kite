import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

interface Feature {
  name: string;
  description?: string;
  free: boolean;
  premium: boolean;
  highlight?: boolean;
}

interface PricingTier {
  name: string;
  price: string;
  period: string;
  description: string;
  recommended?: boolean;
  ctaText: string;
}

@Component({
  selector: 'app-features',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="features-page">
      <!-- Hero Section -->
      <section class="hero">
        <h1 class="hero-title">Choose Your Plan</h1>
        <p class="hero-subtitle">RecipeVault is free to use. Upgrade for AI-powered features.</p>
      </section>

      <!-- Remix Callout Card -->
      <section class="remix-callout">
        <div class="remix-card">
          <div class="remix-badge">✨ No other recipe app does this</div>
          <h2 class="remix-title">Remix Any Recipe</h2>
          <p class="remix-description">
            See a recipe you love? One tap to copy it, tweak the ingredients, swap the spices, make it yours. 
            Your version stays linked to the original so you can always go back.
          </p>
          <blockquote class="remix-example">
            "Your friend shares Grandma's Lasagna. You remix it, swap beef for turkey, add spinach. 
            Now it's YOUR lasagna — and your friend's original is still untouched."
          </blockquote>
          
          <div class="remix-visual">
            <div class="remix-box">
              <mat-icon>menu_book</mat-icon>
              <span>Original Recipe</span>
            </div>
            <mat-icon class="remix-arrow">arrow_forward</mat-icon>
            <div class="remix-box remix-yours">
              <mat-icon>edit_note</mat-icon>
              <span>Your Remix</span>
            </div>
            <mat-icon class="remix-arrow">arrow_forward</mat-icon>
            <div class="remix-box">
              <mat-icon>share</mat-icon>
              <span>Share to Circle</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Comparison Table -->
      <section class="comparison">
        <div class="table-container">
          <!-- Desktop Table -->
          <table class="comparison-table desktop-only">
            <thead>
              <tr>
                <th class="feature-col">Feature</th>
                <th class="plan-col">Free</th>
                <th class="plan-col premium-col">Premium</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let feature of features" [class.highlight-row]="feature.highlight">
                <td class="feature-name">
                  {{ feature.name }}
                  <span *ngIf="feature.description" class="feature-desc">{{ feature.description }}</span>
                </td>
                <td class="feature-check">
                  <mat-icon *ngIf="feature.free" class="check-icon">check_circle</mat-icon>
                  <mat-icon *ngIf="!feature.free" class="dash-icon">remove</mat-icon>
                </td>
                <td class="feature-check">
                  <mat-icon *ngIf="feature.premium" class="check-icon">check_circle</mat-icon>
                  <mat-icon *ngIf="!feature.premium" class="dash-icon">remove</mat-icon>
                </td>
              </tr>
            </tbody>
          </table>

          <!-- Mobile Cards -->
          <div class="comparison-cards mobile-only">
            <div *ngFor="let feature of features" class="feature-card" [class.highlight-card]="feature.highlight">
              <div class="feature-card-name">{{ feature.name }}</div>
              <div *ngIf="feature.description" class="feature-card-desc">{{ feature.description }}</div>
              <div class="feature-card-plans">
                <div class="plan-status">
                  <span class="plan-label">Free</span>
                  <mat-icon *ngIf="feature.free" class="check-icon">check_circle</mat-icon>
                  <mat-icon *ngIf="!feature.free" class="dash-icon">remove</mat-icon>
                </div>
                <div class="plan-status">
                  <span class="plan-label">Premium</span>
                  <mat-icon *ngIf="feature.premium" class="check-icon">check_circle</mat-icon>
                  <mat-icon *ngIf="!feature.premium" class="dash-icon">remove</mat-icon>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Pricing Cards -->
      <section class="pricing">
        <div class="pricing-cards">
          <div *ngFor="let tier of pricingTiers" 
               class="pricing-card" 
               [class.recommended]="tier.recommended">
            <div *ngIf="tier.recommended" class="recommended-badge">Best Value</div>
            <h3 class="tier-name">{{ tier.name }}</h3>
            <div class="tier-price">
              {{ tier.price }}
              <span class="tier-period">{{ tier.period }}</span>
            </div>
            <p class="tier-description">{{ tier.description }}</p>
            <button mat-raised-button color="primary" routerLink="/login">
              {{ tier.ctaText }}
            </button>
          </div>
        </div>
      </section>

      <!-- Footer CTA -->
      <section class="footer-cta">
        <p class="cta-text">Start cooking for free. No credit card required.</p>
        <button mat-raised-button color="primary" routerLink="/login">
          Sign Up Free
        </button>
      </section>
    </div>
  `,
  styles: [`
    .features-page {
      min-height: 100vh;
      background-color: var(--color-bg-primary);
      padding: var(--space-lg) var(--space-md);

      @media (min-width: 768px) {
        padding: var(--space-xl) var(--space-lg);
      }

      @media (min-width: 1024px) {
        padding: var(--space-2xl) var(--space-xl);
      }
    }

    // Hero Section
    .hero {
      text-align: center;
      margin-bottom: var(--space-2xl);
      
      @media (min-width: 768px) {
        margin-bottom: 56px;
      }
    }

    .hero-title {
      font: var(--font-h1);
      color: var(--color-text-primary);
      margin-bottom: var(--space-md);

      @media (min-width: 768px) {
        font-size: 40px;
        line-height: 48px;
      }
    }

    .hero-subtitle {
      font: var(--font-body-large);
      color: var(--color-text-secondary);
      margin: 0;
    }

    // Remix Callout
    .remix-callout {
      max-width: 900px;
      margin: 0 auto var(--space-2xl);
    }

    .remix-card {
      background: linear-gradient(135deg, #ff8c42 0%, #7c9885 100%);
      border-radius: var(--radius-xl);
      padding: var(--space-lg);
      box-shadow: var(--shadow-3);
      color: #1a2230;
      position: relative;

      // White overlay — lightens the sage end for better text contrast
      &::before {
        content: '';
        position: absolute;
        inset: 0;
        background: linear-gradient(135deg, 
          rgba(255, 255, 255, 0.05) 0%, 
          rgba(255, 255, 255, 0.2) 100%
        );
        border-radius: var(--radius-xl);
        pointer-events: none;
      }

      // Ensure all content sits above the overlay
      > * {
        position: relative;
        z-index: 1;
      }

      @media (min-width: 768px) {
        padding: var(--space-xl);
      }
    }

    .remix-badge {
      display: inline-block;
      background: rgba(255, 255, 255, 0.95);
      color: #1a2230;
      padding: var(--space-xs) var(--space-md);
      border-radius: var(--radius-full);
      font: var(--font-caption);
      font-weight: 600;
      margin-bottom: var(--space-md);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .remix-title {
      font: var(--font-h2);
      color: #1a2230;
      margin-bottom: var(--space-md);

      @media (min-width: 768px) {
        font-size: 28px;
        line-height: 36px;
      }
    }

    .remix-description {
      font: var(--font-body-large);
      color: rgba(26, 34, 48, 0.9);
      margin-bottom: var(--space-lg);
      line-height: 1.6;
    }

    .remix-example {
      background: rgba(255, 255, 255, 0.2);
      border-left: 4px solid rgba(255, 255, 255, 0.6);
      padding: var(--space-md);
      margin: 0 0 var(--space-lg);
      border-radius: var(--radius-md);
      font: var(--font-body);
      font-style: italic;
      color: rgba(26, 34, 48, 0.85);
    }

    .remix-visual {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-md);
      flex-wrap: wrap;

      @media (min-width: 768px) {
        flex-wrap: nowrap;
        gap: var(--space-lg);
      }
    }

    .remix-box {
      flex: 1;
      min-width: 120px;
      background: rgba(255, 255, 255, 0.95);
      border-radius: var(--radius-md);
      padding: var(--space-md);
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-sm);
      box-shadow: var(--shadow-1);

      mat-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        color: #6b8574;
      }

      span {
        font: var(--font-body-small);
        font-weight: 500;
        color: #1a2230;
        text-align: center;
      }

      &.remix-yours {
        background: rgba(255, 255, 255, 1);
        box-shadow: var(--shadow-2);

        mat-icon {
          color: #ff8c42;
        }
      }
    }

    .remix-arrow {
      color: rgba(255, 255, 255, 0.8);
      font-size: 24px;
      width: 24px;
      height: 24px;
      flex-shrink: 0;

      @media (max-width: 767px) {
        display: none;
      }
    }

    // Comparison Table
    .comparison {
      max-width: 1000px;
      margin: 0 auto var(--space-2xl);
    }

    .table-container {
      background: var(--color-surface-default);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-2);
      overflow: hidden;
    }

    .comparison-table {
      width: 100%;
      border-collapse: collapse;

      thead {
        background: var(--color-bg-secondary);

        th {
          padding: var(--space-lg);
          font: var(--font-h4);
          color: var(--color-text-primary);
          text-align: left;

          &.plan-col {
            text-align: center;
            width: 150px;
          }

          &.premium-col {
            background: rgba(255, 140, 66, 0.08);
          }
        }
      }

      tbody {
        tr {
          border-bottom: 1px solid var(--color-divider);

          &:last-child {
            border-bottom: none;
          }

          &.highlight-row {
            background: rgba(255, 140, 66, 0.03);
            border-left: 4px solid var(--color-primary);

            .feature-name {
              font-weight: 500;
            }
          }

          &:hover {
            background: var(--color-surface-hover);
          }
        }

        td {
          padding: var(--space-md) var(--space-lg);

          &.feature-name {
            font: var(--font-body);
            color: var(--color-text-primary);

            .feature-desc {
              display: block;
              font: var(--font-body-small);
              color: var(--color-text-secondary);
              margin-top: var(--space-xs);
            }
          }

          &.feature-check {
            text-align: center;
          }
        }
      }
    }

    .check-icon {
      color: var(--color-success);
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .dash-icon {
      color: var(--color-text-disabled);
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    // Mobile Cards
    .comparison-cards {
      display: flex;
      flex-direction: column;
      gap: var(--space-md);
      padding: var(--space-md);
    }

    .feature-card {
      background: var(--color-surface-default);
      border-radius: var(--radius-md);
      padding: var(--space-md);
      border: 1px solid var(--color-border-default);

      &.highlight-card {
        background: rgba(255, 140, 66, 0.03);
        border-left: 4px solid var(--color-primary);
      }
    }

    .feature-card-name {
      font: var(--font-body);
      font-weight: 500;
      color: var(--color-text-primary);
      margin-bottom: var(--space-xs);
    }

    .feature-card-desc {
      font: var(--font-body-small);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-sm);
    }

    .feature-card-plans {
      display: flex;
      gap: var(--space-lg);
    }

    .plan-status {
      display: flex;
      align-items: center;
      gap: var(--space-sm);
    }

    .plan-label {
      font: var(--font-caption);
      font-weight: 500;
      color: var(--color-text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    // Responsive visibility
    .desktop-only {
      display: none;

      @media (min-width: 768px) {
        display: table;
      }
    }

    .mobile-only {
      display: flex;

      @media (min-width: 768px) {
        display: none;
      }
    }

    // Pricing Cards
    .pricing {
      max-width: 1000px;
      margin: 0 auto var(--space-2xl);
    }

    .pricing-cards {
      display: grid;
      gap: var(--space-lg);
      
      @media (min-width: 768px) {
        grid-template-columns: repeat(2, 1fr);
      }

      @media (min-width: 1024px) {
        grid-template-columns: repeat(3, 1fr);
      }
    }

    .pricing-card {
      position: relative;
      background: var(--color-surface-default);
      border: 2px solid var(--color-border-default);
      border-radius: var(--radius-xl);
      padding: var(--space-xl);
      box-shadow: var(--shadow-1);
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      transition: all var(--transition-normal);

      &:hover {
        box-shadow: var(--shadow-3);
        transform: translateY(-4px);
      }

      &.recommended {
        border-color: var(--color-primary);
        box-shadow: var(--shadow-2);

        &:hover {
          box-shadow: var(--shadow-4);
        }
      }
    }

    .recommended-badge {
      position: absolute;
      top: -12px;
      left: 50%;
      transform: translateX(-50%);
      background: var(--color-primary);
      color: var(--color-primary-text);
      padding: var(--space-xs) var(--space-md);
      border-radius: var(--radius-full);
      font: var(--font-caption);
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      box-shadow: var(--shadow-2);
    }

    .tier-name {
      font: var(--font-h3);
      color: var(--color-text-primary);
      margin-bottom: var(--space-sm);
      margin-top: var(--space-md);
    }

    .tier-price {
      font-size: 36px;
      font-weight: 600;
      color: var(--color-primary);
      margin-bottom: var(--space-xs);
      line-height: 1.2;
    }

    .tier-period {
      font: var(--font-body);
      color: var(--color-text-secondary);
      font-weight: 400;
    }

    .tier-description {
      font: var(--font-body);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-lg);
      flex-grow: 1;
    }

    .pricing-card button {
      width: 100%;
      min-height: 48px;
    }

    // Footer CTA
    .footer-cta {
      text-align: center;
      max-width: 600px;
      margin: 0 auto;
      padding: var(--space-xl) 0;
    }

    .cta-text {
      font: var(--font-body-large);
      color: var(--color-text-secondary);
      margin-bottom: var(--space-lg);
    }

    .footer-cta button {
      min-width: 200px;
      height: 48px;
    }
  `]
})
export class FeaturesComponent {
  features: Feature[] = [
    { name: 'Unlimited recipe storage', free: true, premium: true },
    { name: 'AI recipe import (photo/URL/web)', free: true, premium: true },
    { name: 'Manual recipe creation & editing', free: true, premium: true },
    { name: 'Recipe collections & tags', free: true, premium: true },
    { 
      name: 'Recipe remixing', 
      description: 'Copy any recipe, make it yours, keep the link to the original',
      free: true, 
      premium: true,
      highlight: true 
    },
    { name: 'Social circles & sharing', free: true, premium: true },
    { name: 'Cooking mode + timers', free: true, premium: true },
    { name: 'Basic nutrition info', free: true, premium: true },
    { name: 'Meal planning + grocery lists', free: true, premium: true },
    { name: 'Dark/light theme toggle', free: true, premium: true },
    { name: 'Export (JSON)', free: true, premium: true },
    { name: 'AI recipe generation (3 creative modes)', free: false, premium: true },
    { name: 'AI recipe mixing & fusion', free: false, premium: true },
    { name: 'Smart substitutions ("What Can I Make?")', free: false, premium: true },
    { name: 'Advanced nutrition (USDA data + dietary profiles)', free: false, premium: true },
    { name: 'Kitchen equipment filtering', free: false, premium: true },
    { name: 'Cooking history & stats heatmap', free: false, premium: true },
    { name: 'Priority support', free: false, premium: true }
  ];

  pricingTiers: PricingTier[] = [
    {
      name: 'Monthly',
      price: '$2.99',
      period: '/month',
      description: 'Most flexible',
      ctaText: 'Get Started'
    },
    {
      name: 'Annual',
      price: '$29.99',
      period: '/year',
      description: 'Save 17%',
      recommended: true,
      ctaText: 'Get Started'
    },
    {
      name: 'Lifetime',
      price: '$79.99',
      period: 'one-time',
      description: 'Pay once, own forever',
      ctaText: 'Get Started'
    }
  ];
}
