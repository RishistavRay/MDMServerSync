import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhonePermissionsComponent } from '../phone-permissions/phone-permissions.component';

import { DevicePermissions, DeviceRegistration } from '../../models/device';
import { AuthService } from '../../services/auth.service';
import { v4 as uuidv4 } from 'uuid';
import { DeviceService } from '../../services/device.service';

@Component({
  selector: 'app-phone-permissions-grid',
  standalone: true,
  imports: [CommonModule, PhonePermissionsComponent],
  templateUrl: './phone-permissions-grid.component.html',
  styleUrls: ['./phone-permissions-grid.component.css'],
})
export class PhonePermissionsGridComponent implements OnInit {
    private _authService = inject(AuthService);
    devices: DevicePermissions[] = [];
    showConfirm = false;

    constructor(private _deviceService: DeviceService) {}

    ngOnInit() {
        this.fetchDevices();
    }

  trackById(index: number, item: DevicePermissions): string {
    return item.id;
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
        this.fetchDevices(); // refresh the grid
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
          // If API returns res.data
          
          const apiDevices = res.data ?? res; 
          this.devices = apiDevices.map((d: any) => ({
            id: d.id,
            cameraEnabled: d.cameraEnabled,
            micEnabled: d.micEnabled,
            locationEnabled: d.locationEnabled,
            softwareVersion: d.softwareVersion,
          })) as DevicePermissions[];
        },
        error: (err) => {
          console.error('Failed to fetch devices', err);
        },
      });
  }
}