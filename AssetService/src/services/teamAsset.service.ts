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

@Injectable()
export class TeamAssetService {
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

  async findAllByTeamId(teamId: string): Promise<TeamAssetDetailDto[]> {
    const teamAssets: TeamAsset[] = await this.teamAssetRepository.find({
      where: { teamId },
    });

    if (teamAssets.length === 0) return [];

    const assetPublicIds: string[] = teamAssets.map((ta) => ta.assetId);
    const assets: Asset[] = await this.assetRepository.find({
      where: { publicId: In(assetPublicIds) },
    });

    const assetMap: Map<string, Asset> = new Map(
      assets.map((a) => [a.publicId, a]),
    );

    // Obtener todos los movimientos de estos assets en este equipo
    const movements = await this.movementRepository.find({
      where: { teamid: teamId, assetid: In(assetPublicIds) },
      select: ['assetid'], // solo necesitamos assetId
    });

    // Crear un set de assetIds que tienen movimientos
    const assetsWithMovements = new Set(movements.map((m) => m.assetid));

    return teamAssets.map(
      (ta): TeamAssetDetailDto & { hasMovements: boolean } => {
        const asset = assetMap.get(ta.assetId);
        if (!asset)
          throw new NotFoundException(
            `Asset con id ${ta.assetId} no encontrado`,
          );
        return {
          ...ta,
          asset,
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
    });
    if (!asset)
      throw new NotFoundException(
        `Asset con id ${teamAsset.assetId} no encontrado`,
      );

    return { ...teamAsset, asset };
  }

  async syncTeamAssets(
    teamId: string,
    selectedAssetIds: string[],
  ): Promise<TeamAssetDetailDto[]> {
    return await this.marketDataSource.transaction(
      async (manager): Promise<TeamAssetDetailDto[]> => {
        // Obtener asociaciones existentes
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

        // Validar assets a agregar
        const validAssets: Asset[] = await this.assetRepository.find({
          where: { publicId: In(assetsToAdd) },
        });

        if (validAssets.length !== assetsToAdd.length) {
          const missingIds: string[] = assetsToAdd.filter(
            (id) => !validAssets.find((a) => a.publicId === id),
          );
          throw new NotFoundException(
            `Assets no encontrados: ${missingIds.join(', ')}`,
          );
        }

        // Crear nuevas asociaciones
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

        // Validar movimientos antes de eliminar asociaciones
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

        // Eliminar asociaciones descartadas
        if (assetsToRemove.length > 0) {
          await manager.delete(TeamAsset, {
            teamId,
            assetId: In(assetsToRemove),
          });
        }

        // Devolver estado actualizado
        const updatedTeamAssets: TeamAsset[] = await manager.find(TeamAsset, {
          where: { teamId },
        });

        const allAssetIds: string[] = updatedTeamAssets.map((ta) => ta.assetId);
        const allAssets: Asset[] = await this.assetRepository.find({
          where: { publicId: In(allAssetIds) },
        });
        const assetMap: Map<string, Asset> = new Map(
          allAssets.map((a) => [a.publicId, a]),
        );

        return updatedTeamAssets.map(
          (ta): TeamAssetDetailDto => ({
            ...ta,
            asset: assetMap.get(ta.assetId)!,
          }),
        );
      },
    );
  }
}
