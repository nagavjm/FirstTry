export interface Institution {
  id: string;
  name: string;
  universalId: string;
  email: string;
  description?: string;
  address?: string;
  phone?: string;
  website?: string;
  isActive: boolean;
  createdAt: Date;
  userCount: number;
  users?: InstitutionUser[];
}

export interface InstitutionUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  authenticationType: number; // 0 = BasicAuth, 1 = SSO
}

export interface CreateInstitutionUser {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface CreateInstitutionRequest {
  name: string;
  universalId: string;
  email: string;
  description?: string;
  address?: string;
  phone?: string;
  website?: string;
  users: CreateInstitutionUser[];
}

export interface InstitutionResponse {
  success: boolean;
  message?: string;
  institution?: Institution;
}

export interface InstitutionListResponse {
  success: boolean;
  message?: string;
  institutions: Institution[];
  totalCount: number;
}

export interface AddUserToInstitutionRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UpdateInstitutionRequest {
  name: string;
  universalId: string;
  email: string;
  description?: string;
  address?: string;
  phone?: string;
  website?: string;
  isActive: boolean;
}

export interface UpdateInstitutionUserRequest {
  firstName: string;
  lastName: string;
  isActive: boolean;
  authenticationType: number;
}

export interface UserResponse {
  success: boolean;
  message?: string;
  user?: InstitutionUser;
}

