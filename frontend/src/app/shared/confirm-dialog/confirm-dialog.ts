import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  imports: [],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  // inputs  customize the dialog per use
  readonly title = input<string>('Are you sure?');
  readonly message = input<string>('This action cannot be undone.');
  readonly confirmText = input<string>('Confirm');
  readonly cancelText = input<string>('Cancel');

  // outputs — tell the parent what the user chose
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}