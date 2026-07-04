import { Component, inject } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface DeleteWorkoutDialogData {
  readonly title: string;
}

@Component({
  selector: 'app-delete-workout-confirmation-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
  ],
  templateUrl: './delete-workout-confirmation-dialog.component.html',
  styleUrl: './delete-workout-confirmation-dialog.component.scss',
})
export class DeleteWorkoutConfirmationDialogComponent {
  private readonly dialogRef = inject<
    MatDialogRef<DeleteWorkoutConfirmationDialogComponent, boolean>
  >(MatDialogRef);

  private readonly data = inject<DeleteWorkoutDialogData>(MAT_DIALOG_DATA);

  protected readonly workoutTitle = this.data.title;

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  protected confirm(): void {
    this.dialogRef.close(true);
  }
}