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
  id: string;
  cameraEnabled: boolean;
  micEnabled: boolean;
  locationEnabled: boolean;
  softwareVersion: number;
}

export interface DeviceUpdate {
  DeviceId: string;
  CameraEnabled?: boolean;
  MicEnabled?: boolean;
  LocationEnabled?: boolean;
  SoftwareVersion?: number;
}