export interface User {
  gId: string;
  userName: string;
  fullName: string;
  email: string;
  phone: string | null;
  roleCode: 'ADMIN' | 'AGENT' | 'USER';
  departmentGId: string | null;
  departmentName: string | null;
  companyId: number;
  isActive: boolean;
  created: string;
  lastUpd: string | null;
}

export interface UserListRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  roleCode?: string;
  departmentGId?: string;
  isActive?: boolean | null;
}

export interface CreateUserRequest {
  departmentGId?: string | null;
  userName: string;
  fullName: string;
  email: string;
  phone?: string | null;
  password: string;
  roleCode: string;
  isActive: boolean;
}

export interface UpdateUserRequest {
  departmentGId?: string | null;
  userName: string;
  fullName: string;
  email: string;
  phone?: string | null;
  roleCode: string;
  isActive: boolean;
}

export interface ChangePasswordRequest {
  newPassword: string;
}
