// src/services/teamAsset.service.ts
import {
  Injectable,
  NotFoundException,
  BadRequestException,
} from '@nestjs/common';
import { InjectRepository, InjectDataSource } from '@nestjs/typeorm';
import { Repository, In, DataSource } from 'typeorm';
import { TeamAsset } from '@entities/teamAsset.entity';
import { Asset } from '@entities/asset.entity';
import { Movement } from '@entities/movement.entity';
import { TeamAssetDetailDto } from '@dtos/team-asset-detail.dto';
import { AssetResponseDto } from '@dtos/asset-response.dto';

@Injectable()
export class TeamAssetService {
  private baseUrl = process.env.ASSET_ICONS_CDN_URL;
  private version = process.env.ASSET_ICONS_VERSION;

  constructor(
    @InjectRepository(TeamAsset, 'marketConnection')
    private readonly teamAssetRepository: Repository<TeamAsset>,

    @InjectRepository(Asset, 'assetsConnection')
    private readonly assetRepository: Repository<Asset>,

    @InjectRepository(Movement, 'marketConnection')
    private readonly movementRepository: Repository<Movement>,

    @InjectDataSource('marketConnection')
    private readonly marketDataSource: DataSource,
  ) {}

  private getLogoUrl(domain?: string): string | undefined {
    if (!domain) return undefined;
    if (!this.baseUrl || !this.version) return undefined;

    return `${this.baseUrl}/icons/${domain}.png?v=${this.version}`;
  }

  private mapAssetToResponse(asset: Asset): AssetResponseDto {
    return {
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
  }

  async findAllByTeamId(teamId: string): Promise<TeamAssetDetailDto[]> {
    const teamAssets: TeamAsset[] = await this.teamAssetRepository.find({
      where: { teamId },
    });
    if (teamAssets.length === 0) return [];

    const assetPublicIds: string[] = teamAssets.map((ta) => ta.assetId);
    const assets: Asset[] = await this.assetRepository.find({
      where: { publicId: In(assetPublicIds) },
      relations: ['category'],
    });

    const assetMap: Map<string, Asset> = new Map(
      assets.map((a) => [a.publicId, a]),
    );

    const movements = await this.movementRepository.find({
      where: { teamid: teamId, assetid: In(assetPublicIds) },
      select: ['assetid'],
    });

    const assetsWithMovements = new Set(movements.map((m) => m.assetid));

    return teamAssets.map(
      (ta): TeamAssetDetailDto & { hasMovements: boolean } => {
        const assetEntity = assetMap.get(ta.assetId);
        if (!assetEntity)
          throw new NotFoundException(
            `Asset con id ${ta.assetId} no encontrado`,
          );

        return {
          ...ta,
          asset: this.mapAssetToResponse(assetEntity),
          hasMovements: assetsWithMovements.has(ta.assetId),
        };
      },
    );
  }

  async findOneByPublicId(publicId: string): Promise<TeamAssetDetailDto> {
    const teamAsset: TeamAsset | null = await this.teamAssetRepository.findOne({
      where: { publicId },
    });
    if (!teamAsset)
      throw new NotFoundException(
        `TeamAsset con publicId ${publicId} no existe`,
      );

    const asset: Asset | null = await this.assetRepository.findOne({
      where: { publicId: teamAsset.assetId },
      relations: ['category'],
    });
    if (!asset)
      throw new NotFoundException(
        `Asset con id ${teamAsset.assetId} no encontrado`,
      );

    return { ...teamAsset, asset: this.mapAssetToResponse(asset) };
  }

  async syncTeamAssets(
    teamId: string,
    selectedAssetIds: string[],
  ): Promise<TeamAssetDetailDto[]> {
    return await this.marketDataSource.transaction(
      async (manager): Promise<TeamAssetDetailDto[]> => {
        const existingTeamAssets: TeamAsset[] = await manager.find(TeamAsset, {
          where: { teamId },
        });
        const existingAssetIds: string[] = existingTeamAssets.map(
          (ta) => ta.assetId,
        );

        const assetsToAdd: string[] = selectedAssetIds.filter(
          (id) => !existingAssetIds.includes(id),
        );
        const assetsToRemove: string[] = existingAssetIds.filter(
          (id) => !selectedAssetIds.includes(id),
        );

        const validAssets: Asset[] = await this.assetRepository.find({
          where: { publicId: In(assetsToAdd) },
          relations: ['category'],
        });

        if (validAssets.length !== assetsToAdd.length) {
          const missingIds: string[] = assetsToAdd.filter(
            (id) => !validAssets.find((a) => a.publicId === id),
          );
          throw new NotFoundException(
            `Assets no encontrados: ${missingIds.join(', ')}`,
          );
        }

        const newTeamAssets: TeamAsset[] = validAssets.map((asset) => {
          const currentPrice: number = asset.basePrice ?? asset.maxPrice ?? 0;
          return this.teamAssetRepository.create({
            teamId,
            assetId: asset.publicId,
            currentPrice,
          });
        });

        if (newTeamAssets.length > 0)
          await manager.save(TeamAsset, newTeamAssets);

        for (const assetId of assetsToRemove) {
          const movementCount = await manager.count(Movement, {
            where: { teamid: teamId, assetid: assetId },
          });

          if (movementCount > 0) {
            throw new BadRequestException(
              `No se puede eliminar el asset ${assetId} del equipo ${teamId} porque ya tiene movimientos registrados`,
            );
          }
        }

        if (assetsToRemove.length > 0) {
          await manager.delete(TeamAsset, {
            teamId,
            assetId: In(assetsToRemove),
          });
        }

        const updatedTeamAssets: TeamAsset[] = await manager.find(TeamAsset, {
          where: { teamId },
        });

        const allAssetIds: string[] = updatedTeamAssets.map((ta) => ta.assetId);
        const allAssets: Asset[] = await this.assetRepository.find({
          where: { publicId: In(allAssetIds) },
          relations: ['category'],
        });

        const assetMap: Map<string, Asset> = new Map(
          allAssets.map((a) => [a.publicId, a]),
        );

        return updatedTeamAssets.map(
          (ta): TeamAssetDetailDto => ({
            ...ta,
            asset: this.mapAssetToResponse(assetMap.get(ta.assetId)!),
          }),
        );
      },
    );
  }
}
