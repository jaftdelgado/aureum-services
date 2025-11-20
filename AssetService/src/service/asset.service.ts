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

  // GET all
  findAll(): Promise<Asset[]> {
    return this.assetRepository.find({
      relations: ['category'],
    });
  }

  // GET by publicId
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

  // POST create
  async create(dto: CreateAssetDto): Promise<Asset> {
    const asset = this.assetRepository.create(dto);

    // Si viene categoryId, validamos que exista
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
