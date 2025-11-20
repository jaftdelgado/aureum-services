import { Controller, Get, Post, Body, Param } from '@nestjs/common';
import { AssetService } from '../service/asset.service';
import { CreateAssetDto } from '../dto/create-asset.dto';
import { Asset } from '../entities/asset.entity';

@Controller('assets')
export class AssetController {
  constructor(private readonly assetService: AssetService) {}

  // GET /assets
  @Get()
  findAll(): Promise<Asset[]> {
    return this.assetService.findAll();
  }

  // GET /assets/:publicId
  @Get(':publicId')
  findOne(@Param('publicId') publicId: string): Promise<Asset> {
    return this.assetService.findOneByPublicId(publicId);
  }

  // POST /asset
  @Post()
  create(@Body() dto: CreateAssetDto): Promise<Asset> {
    return this.assetService.create(dto);
  }
}
