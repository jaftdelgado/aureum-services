import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Asset } from '../entities/asset.entity';
import { CreateAssetDto } from '../dto/create-asset.dto';
import { AssetCategory } from '../entities/assetCategory.entity';

@Injectable()
export class AssetService {
  constructor(
    @InjectRepository(Asset)
    private readonly assetRepository: Repository<Asset>,

    @InjectRepository(AssetCategory)
    private readonly categoryRepository: Repository<AssetCategory>,
  ) {}

  findAll(): Promise<Asset[]> {
    return this.assetRepository.find({
      relations: ['category'],
    });
  }

  async findOne(id: number): Promise<Asset> {
    const asset = await this.assetRepository.findOne({
      where: { assetId: id },
      relations: ['category'],
    });
    if (!asset) {
      throw new NotFoundException(`El activo con ID ${id} no existe`);
    }
    return asset;
  }

  async create(dto: CreateAssetDto): Promise<Asset> {
    const asset = this.assetRepository.create(dto);

    if (dto.categoryId) {
      const category = await this.categoryRepository.findOneBy({
        categoryId: dto.categoryId,
      });
      if (!category) {
        throw new NotFoundException(
          `La categoría con ID ${dto.categoryId} no existe`,
        );
      }
      asset.category = category;
    }

    return this.assetRepository.save(asset);
  }
}
