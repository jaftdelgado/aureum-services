import {
  Controller,
  Get,
  Post,
  Body,
  Param,
  ParseIntPipe,
} from '@nestjs/common';
import { AssetService } from '../service/asset.service';
import { CreateAssetDto } from '../dto/create-asset.dto';
import { Asset } from '../entities/asset.entity';

@Controller('assets')
export class AssetController {
  constructor(private readonly assetService: AssetService) {}

  @Get()
  findAll(): Promise<Asset[]> {
    return this.assetService.findAll();
  }

  @Get(':id')
  findOne(@Param('id', ParseIntPipe) id: number): Promise<Asset> {
    return this.assetService.findOne(id);
  }

  @Post()
  create(@Body() dto: CreateAssetDto): Promise<Asset> {
    return this.assetService.create(dto);
  }
}
