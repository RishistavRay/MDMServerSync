export interface DeviceRegistration {
  DeviceId: string;
  DeviceName: string;
  UserId: string;
  CameraEnabled: boolean;
  MicEnabled: boolean;
  LocationEnabled: boolean;
  SoftwareVersion: number;
}

export interface DevicePermissions {
  deviceId: string;
  cameraEnabled: boolean;
  micEnabled: boolean;
  locationEnabled: boolean;
  softwareVersion: number;
}

export interface DeviceUpdate {
  deviceId: string;
  cameraEnabled?: boolean;
  micEnabled?: boolean;
  locationEnabled?: boolean;
  softwareVersion?: number;
}