import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-deactivate-account-confirmation-dialog',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './deactivate-account-confirmation-dialog.component.html',
  styleUrl: './deactivate-account-confirmation-dialog.component.scss',
})
export class DeactivateAccountConfirmationDialogComponent {
  private readonly dialogRef =
    inject<MatDialogRef<DeactivateAccountConfirmationDialogComponent, boolean>>(
      MatDialogRef,
    );

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  protected confirm(): void {
    this.dialogRef.close(true);
  }
}