import { inject, Injectable, signal } from '@angular/core';
import { User } from '../models/user';
import { AuthService } from './auth.service';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';

import { environment } from '../../environments/environment';
import { DevicePermissions, DeviceUpdate } from '../models/device';
import { DeviceService } from './device.service';

@Injectable({
  providedIn: 'root',
})
export class RedisHubService {
  private authService = inject(AuthService);
  private deviceService = inject(DeviceService);
  private hubUrl = `${environment.baseUrl}/hubs/delta-payload`;

  readonly peerConnection = new RTCPeerConnection({
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }],
  });

  onlineClients = signal<User[]>([]);
  isLoading = signal<boolean>(true);
  devices = signal<DevicePermissions[]>([]);

  private deltaHubConnection?: HubConnection;

  mergeDeviceUpdateIntoPermissions(
  update: Partial<DeviceUpdate>,
  existing: DevicePermissions
): DevicePermissions {
  return {
    ...existing,
    cameraEnabled: update.cameraEnabled ?? existing.cameraEnabled,
    micEnabled: update.micEnabled ?? existing.micEnabled,
    locationEnabled: update.locationEnabled ?? existing.locationEnabled,
    softwareVersion: update.softwareVersion ?? existing.softwareVersion
  };
}
  
  public fetchDevices() {
      this.deviceService.getAllDevices().subscribe({
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
        },
        error: (err) => {
          console.error('Failed to fetch devices', err);
        },
      });
  }


  startConnection(token: string, clientId?: string) {
    if (this.deltaHubConnection?.state === HubConnectionState.Connected) return;
    if (this.deltaHubConnection) {
      this.deltaHubConnection.off('Error');
      this.deltaHubConnection.off('RequestReceivedNotify');
      this.deltaHubConnection.off('ApplyDelta');
    }

    this.deltaHubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}?clientId=${clientId || ''}`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.deltaHubConnection
      .start()
      .then(() => {
        console.log('Connection started');
      })
      .catch((error) => {
        console.log('Connection or login error', error);
      });

    this.deltaHubConnection!.on('Error', (error) => { 
      Notification.requestPermission().then((result) => {
        if (result == 'granted') {
          new Notification('Unable to Connect Client to Local Hub', {
            body: error.message,
          });
        }
      });
    });

      this.deltaHubConnection!.on('ApplyDelta', (payload: Partial<DeviceUpdate>) => {
        console.log("Received Delta Payload:", payload);

        const id = payload.deviceId;
        if (!id) return;

        this.devices.update((current) => {
          const index = current.findIndex(d => d.deviceId === id);

          if (index >= 0) {
            const updated = this.mergeDeviceUpdateIntoPermissions(payload, current[index]);
            const newArr = [...current];
            newArr[index] = updated;
            return newArr;
          }

          return [
            ...current,
            {
              deviceId: payload.deviceId!,
              cameraEnabled: payload.cameraEnabled ?? false,
              micEnabled: payload.micEnabled ?? false,
              locationEnabled: payload.locationEnabled ?? false,
              softwareVersion: payload.softwareVersion ?? 0
            }
          ];
        });

        console.log("Devices signal value:", this.devices());
      });


  }

  sendDelta(delta: Partial<DeviceUpdate>) { 

      this.deltaHubConnection?.invoke(
        'applyAndPublishDelta', this.authService.currentLoggedUser!.id, delta
      )
      .catch((error) => {
        console.log("Error Occurred:", error);
      });
  }

  

  disConnectConnection() {
    if (this.deltaHubConnection?.state === HubConnectionState.Connected) {
      this.deltaHubConnection.stop().catch((error) => console.log(error));
    }
  }
}
