export interface ChildApplication {
  id: string;
  name: string;
  description: string;
  iconUrl: string;
  launchUrl: string;
  displayOrder: number;
  hasAccess: boolean;
}

export interface ChildApplicationListResponse {
  success: boolean;
  message?: string;
  applications: ChildApplication[];
}

