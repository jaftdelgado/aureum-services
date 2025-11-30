import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository, In } from 'typeorm';
import { TeamAsset } from '@entities/teamAsset.entity';
import { Asset } from '@entities/asset.entity';
import { TeamAssetDetailDto } from '@dtos/team-asset-detail.dto';

@Injectable()
export class TeamAssetService {
  constructor(
    @InjectRepository(TeamAsset, 'marketConnection')
    private readonly teamAssetRepository: Repository<TeamAsset>,

    @InjectRepository(Asset, 'assetsConnection')
    private readonly assetRepository: Repository<Asset>,
  ) {}

  async findAllByTeamId(teamId: string): Promise<TeamAssetDetailDto[]> {
    const teamAssets = await this.teamAssetRepository.find({
      where: { teamId },
    });
    if (teamAssets.length === 0) return [];

    const assetPublicIds = teamAssets.map((ta) => ta.assetId);
    const assets = await this.assetRepository.find({
      where: { publicId: In(assetPublicIds) },
    });

    const assetMap = new Map(assets.map((a) => [a.publicId, a]));

    return teamAssets.map((ta) => {
      const asset = assetMap.get(ta.assetId);
      if (!asset)
        throw new NotFoundException(`Asset con id ${ta.assetId} no encontrado`);
      return { ...ta, asset };
    });
  }

  async findOneByPublicId(publicId: string): Promise<TeamAssetDetailDto> {
    const teamAsset = await this.teamAssetRepository.findOne({
      where: { publicId },
    });
    if (!teamAsset)
      throw new NotFoundException(
        `TeamAsset con publicId ${publicId} no existe`,
      );

    const asset = await this.assetRepository.findOne({
      where: { publicId: teamAsset.assetId },
    });
    if (!asset)
      throw new NotFoundException(
        `Asset con id ${teamAsset.assetId} no encontrado`,
      );

    return { ...teamAsset, asset };
  }

  async createAssociation(
    teamId: string,
    assetPublicId: string,
  ): Promise<TeamAssetDetailDto> {
    const asset = await this.assetRepository.findOne({
      where: { publicId: assetPublicId },
    });
    if (!asset)
      throw new NotFoundException(
        `El Asset con publicId ${assetPublicId} no existe`,
      );

    const currentPrice = asset.basePrice ?? asset.maxPrice ?? 0;

    const newTeamAsset = this.teamAssetRepository.create({
      teamId,
      assetId: asset.publicId,
      currentPrice,
    });

    const saved = await this.teamAssetRepository.save(newTeamAsset);

    return { ...saved, asset };
  }
}
