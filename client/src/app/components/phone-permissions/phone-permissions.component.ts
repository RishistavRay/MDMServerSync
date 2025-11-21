import { Component, ElementRef, inject, Input, SimpleChanges, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DeviceUpdate } from '../../models/device';
import { DeviceService } from '../../services/device.service';
import { RedisHubService } from '../../services/RedisHubService.service';

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

    private _redisHubService = inject(RedisHubService);
    private _latestState: DeviceUpdate = {
      deviceId: '',
      cameraEnabled: false,
      micEnabled: false,
      locationEnabled: false,
      softwareVersion: 0
    }
    message: string = '';
    isModified: boolean = false;

    ngOnChanges() {
      this._latestState = {
        deviceId: this.id,
        cameraEnabled: this.cameraEnabled,
        micEnabled: this.micEnabled,
        locationEnabled: this.locationEnabled,
        softwareVersion: this.softwareVersion
      };
    
    }


    markModified() {
      this.isModified = true;
    }

    sendMessage() {
        const currentState: DeviceUpdate= {
            deviceId: this.id,
            cameraEnabled: this.cameraEnabled,
            micEnabled: this.micEnabled,
            locationEnabled: this.locationEnabled,
            softwareVersion: this.softwareVersion
        };

        const delta: Partial<DeviceUpdate> = {};
        delta['deviceId'] = currentState.deviceId;
        for (const key in currentState) {
            const deltaKey = key as keyof DeviceUpdate;
            if (currentState[deltaKey] !== this._latestState[deltaKey]) {
                delta[deltaKey] = currentState[deltaKey] as any;
            }
        }
        
        this._redisHubService.sendDelta(delta)
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
