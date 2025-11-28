import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Asset } from '@entities/asset.entity';
import { CustomAsset } from '@entities/customAsset.entity';
import { AssetCategory } from '@entities/assetCategory.entity';
import { TeamAsset } from '@entities/teamAsset.entity';

@Module({
  imports: [
    TypeOrmModule.forRoot({
      name: 'assetsConnection',
      type: 'postgres',
      url: process.env.ASSETS_DB_URL,
      entities: [Asset, CustomAsset, AssetCategory],
      synchronize: false,
      ssl: { rejectUnauthorized: false },
    }),

    TypeOrmModule.forRoot({
      name: 'marketConnection',
      type: 'postgres',
      url: process.env.MARKET_DB_URL,
      entities: [TeamAsset],
      synchronize: false,
      ssl: { rejectUnauthorized: false },
    }),
  ],
})
export class DatabaseModule {}
