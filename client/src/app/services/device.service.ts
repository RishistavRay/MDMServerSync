import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DevicePermissions, DeviceRegistration, DeviceUpdate } from '../models/device';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private baseUrl = `${environment.baseUrl}/api/device`;

  constructor(private http: HttpClient) {}

  // Fetch all devices
  getAllDevices(): Observable<DevicePermissions[]> {
    return this.http.get<DevicePermissions[]>(`${this.baseUrl}/all`);
  }

  // Register new device
  registerDevice(payload: DeviceRegistration): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, payload);
  }

  updateDevice(payload: Partial<DeviceUpdate>): Observable<any> {
    return this.http.patch(`${this.baseUrl}/update`, payload);
  }
}
