import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-documents',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-gray-900">Documents & Forms</h1>
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
        <p class="text-5xl mb-4">📄</p>
        <p class="text-gray-600 text-lg">Document management module</p>
        <p class="text-gray-400 text-sm mt-2">Upload, manage, and track clinical documents, consent forms, and patient records.</p>
      </div>
    </div>
  `
})
export class DocumentsComponent {}
