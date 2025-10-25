import { Component, inject } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { TitleCasePipe } from '@angular/common';
import { PhonePermissionsGridComponent } from '../phone-permissions-grid/phone-permissions-grid.component';

@Component({
  selector: 'app-chat-right-sidebar',
  imports: [TitleCasePipe, PhonePermissionsGridComponent],
  templateUrl: './chat-right-sidebar.component.html',
  styles: ``,
})
export class ChatRightSidebarComponent {
  chatService = inject(ChatService);
}
