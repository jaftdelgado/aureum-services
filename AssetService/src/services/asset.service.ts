import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Asset } from '@entities/asset.entity';
import { AssetCategory } from '@entities/assetCategory.entity';
import { GetAssetsDto } from '@dtos/get-assets.dto';
import { PaginatedResult } from '@utils/pagination.util';

@Injectable()
export class AssetService {
  constructor(
    @InjectRepository(Asset, 'assetsConnection')
    private readonly assetRepository: Repository<Asset>,

    @InjectRepository(AssetCategory, 'assetsConnection')
    private readonly categoryRepository: Repository<AssetCategory>,
  ) {}

  async getAssets(
    dto: GetAssetsDto & { selectedAssetIds?: string[] },
  ): Promise<PaginatedResult<Asset>> {
    const {
      page = 1,
      limit = 10,
      search,
      assetType,
      basePrice,
      categoryId,
      orderByBasePrice,
      orderByAssetName,
      selectedAssetIds = [],
    } = dto;

    const query = this.assetRepository
      .createQueryBuilder('asset')
      .leftJoinAndSelect('asset.category', 'category');

    if (assetType)
      query.andWhere('asset.assetType = :assetType', { assetType });
    if (basePrice !== undefined)
      query.andWhere('asset.basePrice = :basePrice', { basePrice });
    if (categoryId !== undefined)
      query.andWhere('asset.category.categoryId = :categoryId', { categoryId });

    if (search) {
      const formattedSearch = search
        .trim()
        .split(/\s+/)
        .map((term) => `${term}:*`)
        .join(' & ');

      query.andWhere(
        `to_tsvector('spanish', unaccent_immutable(coalesce(asset.assetName, '') || ' ' || coalesce(asset.assetSymbol, ''))) @@ to_tsquery('spanish', :search)`,
        { search: formattedSearch },
      );

      query.addSelect(
        `ts_rank_cd(to_tsvector('spanish', unaccent_immutable(coalesce(asset.assetName, '') || ' ' || coalesce(asset.assetSymbol, ''))), to_tsquery('spanish', :search))`,
        'rank',
      );

      query.orderBy('rank', 'DESC');
    }

    if (selectedAssetIds.length) {
      query
        .addSelect(
          `CASE WHEN asset.publicId IN (:...selectedAssetIds) THEN 0 ELSE 1 END`,
          'is_selected',
        )
        .setParameter('selectedAssetIds', selectedAssetIds)
        .addOrderBy('is_selected', 'ASC');
    }

    const order: Record<string, 'ASC' | 'DESC'> = {};
    if (orderByBasePrice) order.basePrice = orderByBasePrice;
    if (orderByAssetName) order.assetName = orderByAssetName;

    Object.entries(order).forEach(([key, value]) => {
      query.addOrderBy(`asset.${key}`, value);
    });

    if (
      !search &&
      Object.keys(order).length === 0 &&
      selectedAssetIds.length === 0
    )
      query.orderBy('asset.assetId', 'DESC');

    const [data, total] = await query
      .skip((page - 1) * limit)
      .take(limit)
      .getManyAndCount();

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

  async findOneByPublicId(publicId: string): Promise<Asset> {
    const asset = await this.assetRepository.findOne({
      where: { publicId },
      relations: ['category'],
    });

    if (!asset) {
      throw new NotFoundException(
        `El activo con publicId ${publicId} no existe`,
      );
    }

    return asset;
  }
}
