import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Asset } from '@entities/asset.entity';
import { AssetCategory } from '@entities/assetCategory.entity';
import { GetAssetsDto } from '@dtos/get-assets.dto';
import { AssetResponseDto } from '@dtos/asset-response.dto';
import { PaginatedResult } from '@utils/pagination.util';

@Injectable()
export class AssetService {
  private baseUrl = process.env.ASSET_ICONS_CDN_URL;
  private version = process.env.ASSET_ICONS_VERSION;

  constructor(
    @InjectRepository(Asset, 'assetsConnection')
    private readonly assetRepository: Repository<Asset>,

    @InjectRepository(AssetCategory, 'assetsConnection')
    private readonly categoryRepository: Repository<AssetCategory>,
  ) {}

  private getLogoUrl(domain?: string): string | undefined {
    if (!domain) return undefined;
    if (!this.baseUrl || !this.version) return undefined;

    return `${this.baseUrl}/icons/${domain}.png?v=${this.version}`;
  }

  async getAssets(
    dto: GetAssetsDto & { selectedAssetIds?: string[] },
  ): Promise<PaginatedResult<AssetResponseDto>> {
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

    const assetResponseDto: AssetResponseDto[] = data.map((asset) => ({
      publicId: asset.publicId,
      assetSymbol: asset.assetSymbol,
      assetName: asset.assetName,
      assetType: asset.assetType,
      basePrice: asset.basePrice,
      volatility: asset.volatility,
      drift: asset.drift,
      maxPrice: asset.maxPrice,
      minPrice: asset.minPrice,
      dividendYield: asset.dividendYield,
      liquidity: asset.liquidity,
      logoUrl: this.getLogoUrl(asset.assetPicUrl),
      category: asset.category,
    }));

    return {
      data: assetResponseDto,
      meta: {
        totalItems: total,
        itemCount: assetResponseDto.length,
        itemsPerPage: limit,
        totalPages: Math.ceil(total / limit),
        currentPage: page,
      },
    };
  }

  async findOneByPublicId(publicId: string): Promise<AssetResponseDto> {
    const asset = await this.assetRepository.findOne({
      where: { publicId },
      relations: ['category'],
    });

    if (!asset)
      throw new NotFoundException(
        `El activo con publicId ${publicId} no existe`,
      );

    const assetResponse: AssetResponseDto = {
      publicId: asset.publicId,
      assetSymbol: asset.assetSymbol,
      assetName: asset.assetName,
      assetType: asset.assetType,
      basePrice: asset.basePrice,
      volatility: asset.volatility,
      drift: asset.drift,
      maxPrice: asset.maxPrice,
      minPrice: asset.minPrice,
      dividendYield: asset.dividendYield,
      liquidity: asset.liquidity,
      logoUrl: this.getLogoUrl(asset.assetPicUrl),
      category: asset.category,
    };

    return assetResponse;
  }
}
