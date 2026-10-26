export interface Department {
  gId: string;
  departmentName: string;
  description: string | null;
  isActive: boolean;
  created: string;
  lastUpd: string | null;
}

export interface CreateDepartmentRequest {
  departmentName: string;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateDepartmentRequest {
  departmentName: string;
  description?: string | null;
  isActive: boolean;
}

export interface DepartmentListRequest {
  searchTerm?: string | null;
  isActive?: boolean | null;
  pageNumber: number;
  pageSize: number;
}
