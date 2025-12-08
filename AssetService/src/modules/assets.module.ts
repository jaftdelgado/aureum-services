import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';

import { Asset } from '@entities/asset.entity';
import { AssetCategory } from '@entities/assetCategory.entity';

import { AssetController } from '@controllers/asset.controller';
import { AssetService } from '@services/asset.service';

@Module({
  imports: [
    TypeOrmModule.forFeature([Asset, AssetCategory], 'assetsConnection'),
  ],
  controllers: [AssetController],
  providers: [AssetService],
  exports: [AssetService, TypeOrmModule],
})
export class AssetsModule {}
