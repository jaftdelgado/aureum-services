import { Controller, Get, Post, Body, Param, Query } from '@nestjs/common';
import { AssetService } from '@services/asset.service';
import { CreateAssetDto } from '@dtos/create-asset.dto';
import { GetAssetsDto } from '@dtos/get-assets.dto';
import { Asset } from '@entities/asset.entity';
import { PaginatedResult } from '@utils/pagination.util';

@Controller('assets')
export class AssetController {
  constructor(private readonly assetService: AssetService) {}

  // GET /assets
  @Get()
  getAssets(
    @Query() getAssetsDto: GetAssetsDto,
  ): Promise<PaginatedResult<Asset>> {
    return this.assetService.getAssets(getAssetsDto);
  }

  // GET /assets/:publicId
  @Get(':publicId')
  findOne(@Param('publicId') publicId: string): Promise<Asset> {
    return this.assetService.findOneByPublicId(publicId);
  }

  // POST /assets
  @Post()
  create(@Body() dto: CreateAssetDto): Promise<Asset> {
    return this.assetService.create(dto);
  }
}
