export interface Category {
  gId: string;
  categoryName: string;
  description: string | null;
  departmentGId: string;
  departmentName: string;
  isActive: boolean;
  created: string;
  lastUpd: string | null;
}

export interface CategoryListRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  departmentGId?: string;
  isActive?: boolean | null;
}

export interface CreateCategoryRequest {
  departmentGId: string;
  categoryName: string;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateCategoryRequest {
  departmentGId: string;
  categoryName: string;
  description?: string | null;
  isActive: boolean;
}
