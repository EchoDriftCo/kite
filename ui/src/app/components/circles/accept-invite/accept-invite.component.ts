import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CircleService } from '../../../services/circle.service';
import { CircleInvite } from '../../../models/circle.model';

@Component({
  selector: 'app-accept-invite',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './accept-invite.component.html',
  styleUrl: './accept-invite.component.scss'
})
export class AcceptInviteComponent implements OnInit {
  invite: CircleInvite | null = null;
  loading = false;
  accepting = false;
  error = '';
  success = false;
  inviteToken: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private circleService: CircleService
  ) {}

  ngOnInit() {
    this.inviteToken = this.route.snapshot.paramMap.get('token');
    
    if (this.inviteToken) {
      this.loadInviteDetails();
    } else {
      this.error = 'Invalid invite link';
    }
  }

  loadInviteDetails() {
    if (!this.inviteToken) return;

    this.loading = true;
    this.error = '';

    this.circleService.getInviteDetails(this.inviteToken).subscribe({
      next: (invite) => {
        this.invite = invite;
        this.loading = false;

        // Check if already accepted or expired
        if (invite.status !== 'Pending') {
          if (invite.status === 'Accepted') {
            this.error = 'This invitation has already been accepted';
          } else if (invite.status === 'Expired') {
            this.error = 'This invitation has expired';
          } else {
            this.error = 'This invitation is no longer valid';
          }
        }
      },
      error: (err) => {
        this.error = err.message || 'Failed to load invitation details';
        this.loading = false;
        console.error('Error loading invite:', err);
      }
    });
  }

  acceptInvite() {
    if (!this.inviteToken) return;

    this.accepting = true;
    this.error = '';

    this.circleService.acceptInvite({ inviteToken: this.inviteToken }).subscribe({
      next: (circle) => {
        this.accepting = false;
        this.success = true;
        
        // Redirect to circle after brief delay
        setTimeout(() => {
          this.router.navigate(['/circles', circle.circleResourceId]);
        }, 2000);
      },
      error: (err) => {
        this.error = err.message || 'Failed to accept invitation';
        this.accepting = false;
        console.error('Error accepting invite:', err);
      }
    });
  }

  goToCircles() {
    this.router.navigate(['/circles']);
  }
}
