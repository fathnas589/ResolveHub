export interface SubCategory {
  gId: string;
  subCategoryName: string;
  description: string | null;
  categoryGId: string;
  categoryName: string;
  departmentGId: string;
  departmentName: string;
  companyId: number;
  isActive: boolean;
  created: string;
  lastUpd: string | null;
}

export interface SubCategoryListRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  categoryGId?: string;
  departmentGId?: string;
  isActive?: boolean | null;
}

export interface CreateSubCategoryRequest {
  categoryGId: string;
  subCategoryName: string;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateSubCategoryRequest {
  categoryGId: string;
  subCategoryName: string;
  description?: string | null;
  isActive: boolean;
}
