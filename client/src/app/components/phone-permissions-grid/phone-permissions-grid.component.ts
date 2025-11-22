import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhonePermissionsComponent } from '../phone-permissions/phone-permissions.component';

import { DevicePermissions, DeviceRegistration } from '../../models/device';
import { AuthService } from '../../services/auth.service';
import { v4 as uuidv4 } from 'uuid';
import { DeviceService } from '../../services/device.service';
import { RedisHubService } from '../../services/RedisHubService.service';

@Component({
  selector: 'app-phone-permissions-grid',
  standalone: true,
  imports: [CommonModule, PhonePermissionsComponent],
  templateUrl: './phone-permissions-grid.component.html',
  styleUrls: ['./phone-permissions-grid.component.css'],
})
export class PhonePermissionsGridComponent {
    private _authService = inject(AuthService);
    private _redisHubService = inject(RedisHubService);
    devices = this._redisHubService.devices;
    showConfirm = false;

    constructor(private _deviceService: DeviceService) {}
    trackById(index: number, item: DevicePermissions): string {
      return item.deviceId;
    }

    openRegisterDialog() {
      this.showConfirm = true;
    }

    confirmRegister() {
      const payload: DeviceRegistration = {
        DeviceId: uuidv4(),
        DeviceName: "Rishi's iPhone",
        UserId: this._authService.currentLoggedUser!.id,
        CameraEnabled: true,
        MicEnabled: true,
        LocationEnabled: true,
        SoftwareVersion: 0.1,
      };

      this._deviceService.registerDevice(payload).subscribe({
        next: (res) => {
          console.log('Device registered:', res);
          this.fetchDevices();
          this.showConfirm = false;
        },
        error: (err) => {
          console.error('Failed to register device', err);
          this.showConfirm = false;
        },
      });
    }

    cancelRegister() {
      this.showConfirm = false;
    }

    public refreshPage() {
      this.fetchDevices();
    }


    public fetchDevices() {
    this._deviceService.getAllDevices().subscribe({
      next: (res: any) => {
        const apiDevices = res.data ?? res; 

        this.devices.set(
          apiDevices.map((d: any) => ({
            deviceId: d.id,
            cameraEnabled: d.cameraEnabled,
            micEnabled: d.micEnabled,
            locationEnabled: d.locationEnabled,
            softwareVersion: d.softwareVersion,
          }))
        );

        console.log("Fetched devices in grid:", this.devices());
      },
      error: (err) => {
        console.error('Failed to fetch devices', err);
      },
    });
  }

}