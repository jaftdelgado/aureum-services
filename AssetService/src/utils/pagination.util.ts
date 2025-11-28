import { Repository, FindManyOptions, ObjectLiteral } from 'typeorm';

export interface PaginatedResult<T> {
  data: T[];
  meta: {
    totalItems: number;
    itemCount: number;
    itemsPerPage: number;
    totalPages: number;
    currentPage: number;
  };
}

export async function paginate<T extends ObjectLiteral>(
  repository: Repository<T>,
  page: number = 1,
  limit: number = 10,
  options?: FindManyOptions<T>,
): Promise<PaginatedResult<T>> {
  const take = limit;
  const skip = (page - 1) * limit;

  const [data, total] = await repository.findAndCount({
    ...options,
    skip,
    take,
  });

  return {
    data,
    meta: {
      totalItems: total,
      itemCount: data.length,
      itemsPerPage: limit,
      totalPages: Math.ceil(total / limit),
      currentPage: page,
    },
  };
}
