import Header from './TableHeader.vue'
import Pages from './TablePages.vue'
import Sizes from './TableSizes.vue'

export { Header }
export { Pages }
export { Sizes }

export interface IListResponse<T> {
  items: Array<T>
  size: number
  page: number
  total: number
  ascending: boolean
  sortBy?: string
}

export interface ITableParams {
  size: number
  page: number
  total: number
  ascending: boolean
  sortBy?: string
}

export const initParams = (): ITableParams => {
  return { page: 1, size: 25, total: 0, ascending: false }
}

export const updateParams = (
  params: ITableParams,
  response: {
    size: number
    page: number
    total: number
    ascending: boolean
    sortBy?: string | null
  }
) => {
  if (params.size !== response.size) params.size = response.size
  if (params.page !== response.page) params.page = response.page
  if (params.total !== response.total) params.total = response.total
  if (params.ascending !== response.ascending) params.ascending = response.ascending
  if (params.sortBy !== response.sortBy)
    params.sortBy = response.sortBy === null ? undefined : response.sortBy
}

export const defaultPageSizes = [10, 25, 50, 100]
