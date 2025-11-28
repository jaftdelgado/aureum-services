import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { TeamAsset } from '@entities/teamAsset.entity';
import { Asset } from '@entities/asset.entity';

@Injectable()
export class TeamAssetService {
  constructor(
    @InjectRepository(TeamAsset, 'marketConnection')
    private readonly teamAssetRepository: Repository<TeamAsset>,

    @InjectRepository(Asset, 'assetsConnection')
    private readonly assetRepository: Repository<Asset>,
  ) {}

  async findAllByTeamId(teamId: string): Promise<TeamAsset[]> {
    return this.teamAssetRepository.find({
      where: { teamId },
    });
  }

  async findOneByPublicId(publicId: string): Promise<TeamAsset> {
    const teamAsset = await this.teamAssetRepository.findOne({
      where: { publicId },
    });

    if (!teamAsset) {
      throw new NotFoundException(
        `TeamAsset con publicId ${publicId} no existe`,
      );
    }

    return teamAsset;
  }

  async createAssociation(
    teamId: string,
    assetPublicId: string,
  ): Promise<TeamAsset> {
    const asset = await this.assetRepository.findOne({
      where: { publicId: assetPublicId },
    });

    if (!asset) {
      throw new NotFoundException(
        `El Asset con publicId ${assetPublicId} no existe`,
      );
    }

    const currentPrice = asset.basePrice ?? asset.maxPrice ?? 0;

    const newTeamAsset = this.teamAssetRepository.create({
      teamId,
      assetId: asset.publicId,
      currentPrice,
    });

    return this.teamAssetRepository.save(newTeamAsset);
  }
}
