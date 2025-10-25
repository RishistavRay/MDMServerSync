import { Component, ElementRef, inject, Input, SimpleChanges, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { VideoChatService } from '../../services/video-chat.service';
import { AuthService } from '../../services/auth.service';
import { DevicePermissions, DeviceUpdate } from '../../models/device';
import { DeviceService } from '../../services/device.service';

@Component({
  selector: 'app-phone-permissions',
  templateUrl: 'phone-permissions.component.html',
  styleUrls: ['phone-permissions.component.css'],
  standalone: true,
  imports: [FormsModule],
})
export class PhonePermissionsComponent {
    @ViewChild('chatBox') chatContainer?: ElementRef;

    @Input() cameraEnabled!: boolean;
    @Input() micEnabled!: boolean;
    @Input() locationEnabled!: boolean;
    @Input() softwareVersion!: number;
    @Input() id!: string;

    private _chatService = inject(ChatService);
    private _latestState: DeviceUpdate = {
      DeviceId: '',
      CameraEnabled: false,
      MicEnabled: false,
      LocationEnabled: false,
      SoftwareVersion: 0
    }
    message: string = '';
    isModified: boolean = false;

    constructor(private _deviceService: DeviceService) {}

    ngOnChanges() {
      this._latestState = {
        DeviceId: this.id,
        CameraEnabled: this.cameraEnabled,
        MicEnabled: this.micEnabled,
        LocationEnabled: this.locationEnabled,
        SoftwareVersion: this.softwareVersion
      };
    
    }


    markModified() {
      this.isModified = true;
    }

    sendMessage() {
        // Build the payload from the component’s data
        const currentState: DeviceUpdate= {
            DeviceId: this.id,
            CameraEnabled: this.cameraEnabled,
            MicEnabled: this.micEnabled,
            LocationEnabled: this.locationEnabled,
            SoftwareVersion: this.softwareVersion
        };

        const delta: Partial<DeviceUpdate> = {};
        delta['DeviceId'] = currentState.DeviceId;
        for (const key in currentState) {
            const deltaKey = key as keyof DeviceUpdate;
            if (currentState[deltaKey] !== this._latestState[deltaKey]) {
                delta[deltaKey] = currentState[deltaKey] as any;
            }
        }

        // Convert to JSON string before sending
        const jsonMessage = JSON.stringify(delta);

        // Send via your chat service
        this._deviceService.updateDevice(delta).subscribe({
          next: (res) => {
            this._chatService.sendMessage("tried to send " +jsonMessage);
            if (res.success) {
              //why do i never come here maybe because res is not really anything
              this._chatService.sendMessage(jsonMessage);
            }
          },
          error: (err) => {
            this._chatService.sendMessage('Failed to update device permissions: ' + jsonMessage);
            console.error('Failed to update device permissions', err);
          }
      });


        // Optionally reset or scroll
        this.scrollToBottom();
        this.isModified = false;
    }
 
    private scrollToBottom() {
    if (this.chatContainer) {
      this.chatContainer.nativeElement.scrollTop =
        this.chatContainer.nativeElement.scrollHeight;
    }
    }

  togglePermission(type: 'camera' | 'mic' | 'location') {
    if (type === 'camera') this.cameraEnabled = !this.cameraEnabled;
    else if (type === 'mic') this.micEnabled = !this.micEnabled;
    else if (type === 'location') this.locationEnabled = !this.locationEnabled;
  }

  incrementVersion() {
    this.softwareVersion = parseFloat((this.softwareVersion + 0.1).toFixed(1));
    this.markModified();
  }

  decrementVersion() {
    if (this.softwareVersion > 0) {
      this.softwareVersion = parseFloat((this.softwareVersion - 0.1).toFixed(1));
      this.markModified();
    }
  }


}
